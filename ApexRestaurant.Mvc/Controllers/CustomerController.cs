using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ApexRestaurant.Mvc.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Text;

public class CustomerController : Controller
{
    private string baseUri;

    public CustomerController(IConfiguration iConfig)
    {
        // Get baseUri of Web API from appsettings.json > ApiBaseUrl
        baseUri = iConfig.GetValue<string>("ApiBaseUrl");
    }
    
    // GET: Customer
    public ActionResult Index()
    {
        IEnumerable<CustomerViewModel> customers = null;

        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Add("accept", "application/json");

            var responseTask = client.GetAsync("api/customer");
            responseTask.Wait();
            
            var result = responseTask.Result;

            if (result.IsSuccessStatusCode)
            {   
                var apiResponse = result.Content.ReadAsStringAsync();
                apiResponse.Wait();
                //json to object
                customers = JsonConvert.DeserializeObject<IList<CustomerViewModel>>(apiResponse.Result);
            }
            else //web api sent error response 
            {
                customers = Enumerable.Empty<CustomerViewModel>();
                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            }
        }
        return View(customers);
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Create(CustomerViewModel customer)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            //HTTP POST
            customer.EnrollDate = DateTime.Now;
            customer.CreatedBy = "admin";
            customer.CreatedOn = DateTime.Now;
            customer.UpdatedBy = "admin";
            customer.UpdatedOn = DateTime.Now;

            var customerJson = JsonConvert.SerializeObject(customer);
            var postTask = client.PostAsync("api/customer", new StringContent(customerJson, Encoding.UTF8, "application/json"));
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
        }

        ModelState.AddModelError(string.Empty, "Server Error. Please contact administrator.");
        return View(customer);
    }

}
