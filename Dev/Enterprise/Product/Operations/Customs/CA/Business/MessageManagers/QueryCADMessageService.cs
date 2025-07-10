using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.Common;
using Enterprise.Customs.CA.Registry;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	[Immutable]
	public interface IQueryCADMessageService
	{
		(string QueryContent, string ResponseContent) QueryCADMessage(string transactionNumber, HttpClient httpClient = null);
	}

	[Immutable]
	class QueryCADMessageService : IQueryCADMessageService
	{
		public static readonly Overridable<IQueryCADMessageService> Instance = new Overridable<IQueryCADMessageService>(new QueryCADMessageService());

		public (string QueryContent, string ResponseContent) QueryCADMessage(string transactionNumber, HttpClient httpClient = null)
		{
			using (var client = httpClient ?? new HttpClient())
			{
				var carmAPIKey = CACustomsDataRegistry.Instance.CARMAPIKey.Value;
				var queryUrl = $"{CACustomsDataRegistry.Instance.CARMEndPoint.Value}?TransactionNumber={transactionNumber}";
				const int TimeoutMinutesAllowingForApplicationPoolRestart = 5;
				client.Timeout = TimeSpan.FromMinutes(TimeoutMinutesAllowingForApplicationPoolRestart);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Add("Language", "EN");
				client.DefaultRequestHeaders.Add("x-api-key", carmAPIKey);

				ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;

				using (var response = client.GetAsync(queryUrl).Result)
				{
					var queryContent = new StringBuilder();
					queryContent.Append($"QueryUrl: {queryUrl}\n");
					queryContent.Append($"x-api-key: {carmAPIKey}");
					var responseContentTask = response.Content.ReadAsStringAsync();
					responseContentTask.Wait();
					return (queryContent.ToString(), responseContentTask.Result.Trim('\"'));
				}
			}
		}
	}
}
