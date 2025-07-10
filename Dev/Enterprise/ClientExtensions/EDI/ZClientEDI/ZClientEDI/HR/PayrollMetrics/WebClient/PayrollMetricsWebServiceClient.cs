using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Client.EDI.HR.PayrollMetrics
{
	interface IPayrollMetricsWebServiceClient : IDisposable
	{
		LeaveModifiedResponse GetEssLeaveRequestStatusModified(EssLeaveRequestStatusModifiedApiParam param);
	}

	public class PayrollMetricsWebServiceClient : IPayrollMetricsWebServiceClient
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public PayrollMetricsWebServiceClient(int timeoutMs = 60000)
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

			baseUri = new Uri(EDIDataRegistry.Instance.PayrollMetricsServiceUri.Value);

			string accessToken = GetAccessToken();
			client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
		}

		HttpClient client;
		readonly Uri baseUri;

		protected virtual string GetAccessToken()
		{
			var uri = new Uri(baseUri, new Uri("Api/Token", UriKind.Relative));

			var keyValues = new Dictionary<string, string>();
			keyValues["username"] = EDIDataRegistry.Instance.PayrollMetricsServiceUser.Value;
			keyValues["password"] = EDIDataRegistry.Instance.PayrollMetricsServicePassword.Value;
			keyValues["customercode"] = EDIDataRegistry.Instance.PayrollMetricsCustomerCode.Value;
			keyValues["grant_type"] = "password";

			string responseText;

			using (var content = new FormUrlEncodedContent(keyValues))
			{
				responseText = client.PostAsync(uri, content)
					.Result
					.EnsureSuccessStatusCode()
					.Content
					.ReadAsStringAsync()
					.Result;
			}

			var jsonObject = JsonConvert.DeserializeObject<JObject>(responseText);
			return (string)jsonObject.GetValue("access_token", StringComparison.Ordinal);
		}

		public LeaveModifiedResponse GetEssLeaveRequestStatusModified(EssLeaveRequestStatusModifiedApiParam param)
		{
			var uri = new Uri(baseUri, new Uri("Api/EssLeaveRequestStatusModified", UriKind.Relative));
			var json = JsonConvert.SerializeObject(param);
			var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

			LeaveModifiedResponse result = GetResponse(uri, httpContent);
			result.RequestJson = json;

			return result;
		}

		protected virtual LeaveModifiedResponse GetResponse(Uri uri, StringContent httpContent)
		{
			var response = client.PostAsync(uri, httpContent).Result;

			return response
				.EnsureSuccessStatusCode()
				.Content
				.ReadAsAsync<LeaveModifiedResponse>()
				.Result;
		}

		#region IDisposable Support

		bool disposedValue; // To detect redundant calls

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					client.Dispose();
					client = null;
				}

				disposedValue = true;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "The only derived classes are in tests")]
		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
