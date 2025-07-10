using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.Common;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Dat.Implementation
{
	[Immutable]
	public interface IProductKeyService
	{
		string GetInternalTestKey(string enterpriseCode, string serverName, string databaseName);
	}

	[Immutable]
	class ProductKeyService : IProductKeyService
	{
		public static readonly Overridable<IProductKeyService> Instance = new Overridable<IProductKeyService>(new ProductKeyService());

		public string GetInternalTestKey(string enterpriseCode, string serverName, string databaseName)
		{
			using (var client = new HttpClient())
			{
				var json = JsonConvert.SerializeObject(new { EnterpriseCode = enterpriseCode, ServerName = serverName, DatabaseName = databaseName });
				using (var requestContent = new StringContent(json, Encoding.UTF8, "application/json"))
				{
					const int TimeoutMinutesAllowingForApplicationPoolRestart = 5;
					client.Timeout = TimeSpan.FromMinutes(TimeoutMinutesAllowingForApplicationPoolRestart);
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					using (var response = client.PostAsync(ServiceUrl + "InternalTestKey", requestContent).Result)
					{
						var responseContentTask = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
						responseContentTask.Wait();
						return responseContentTask.Result.Trim('\"');
					}
				}
			}
		}

		const string ServiceUrl = "https://myaccount-portal.cargowise.com/myaccount/api/ProductKey/";
	}
}
