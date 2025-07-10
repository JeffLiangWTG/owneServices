using System;
using System.Net.Http;
using System.Net.Http.Headers;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	public abstract class MTDRequestBase
	{
		protected MTDRequestBase(MTDClient client)
		{
			Vrn = client.VATRegistrationNumber;
			DateFrom = client.ComplianceReport.ACR_DateFrom;
			DateTo = client.ComplianceReport.ACR_DateTo;
			NeedAccessTokenToExecuteTheRequest = true;
			IsTestEnvironment = MTDClient.IsTestEnvironment;
			CompanyPK = client.ComplianceReport.ACR_GC_Company;
			FraudPreventionData = client.FraudPreventionData;
		}

		#region Functions

		public HttpRequestMessage GetAsHttpRequest(HttpClient client)
		{
			var httpRequest = new HttpRequestMessage(Method, new Uri(FormattableString.Invariant($"{client?.BaseAddress.ToString() ?? string.Empty}{Path}")));
			BuildRequestHeader(httpRequest.Headers);
			httpRequest.Content = GetRequestBody();
			return httpRequest;
		}

		protected virtual string HandleErrors(MTDErrorInfo errorInfo) => errorInfo?.ToString() ?? string.Empty;

		protected virtual void BuildRequestHeader(HttpRequestHeaders requestHeaders)
		{
			AddAcceptAndAuthorizationHeaders(requestHeaders);
			AddFraudPreventionHeaders(requestHeaders);

			ConfigureHeaderForScenarioSimulation(requestHeaders);
		}

		protected virtual ByteArrayContent GetRequestBody() => null;

		protected void ConfigureHeaderForScenarioSimulation(HttpRequestHeaders requestHeaders)
		{
			if (IsTestEnvironment && !string.IsNullOrEmpty(ScenarioToSimulate))
			{
				AddValueToHeaders(requestHeaders, "Gov-Test-Scenario", ScenarioToSimulate);
			}
		}

		protected void AddAcceptAndAuthorizationHeaders(HttpRequestHeaders requestHeaders)
		{
			requestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.hmrc.1.0+json"));
			requestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
		}

		protected void AddFraudPreventionHeaders(HttpRequestHeaders requestHeaders)
		{
			foreach (var data in FraudPreventionData.Get())
			{
				AddValueToHeaders(requestHeaders, data.Key, data.Value);
			}
		}

		protected void AddValueToHeaders(HttpRequestHeaders requestHeaders, string key, params string[] values)
		{
			if (values != null)
			{
				if (requestHeaders.Contains(key))
				{
					requestHeaders.Remove(key);
				}

				if (values.Length == 1)
				{
					requestHeaders.Add(key, values[0]);
				}
				else
				{
					requestHeaders.Add(key, values);
				}
			}
		}

		#endregion

		#region Properties

		public abstract HttpMethod Method { get; }

		public virtual string Path => FormattableString.Invariant($"organisations/vat/{Vrn}");

		protected virtual string QueryString => string.Empty;

		protected string DateRangeQueryString => FormattableString.Invariant($"from={DateFrom.ToMTDCompliantFormat()}&to={DateTo.ToMTDCompliantFormat()}");

		protected string AccessToken => MTDClient.GetAccessToken(CompanyPK);

		protected virtual string ScenarioToSimulate => string.Empty;

		public bool NeedAccessTokenToExecuteTheRequest { get; protected set; }

		protected bool IsTestEnvironment { get; }

		protected ZDate DateFrom { get; }

		protected ZDate DateTo { get; }

		protected string Vrn { get; }

		protected ZGuid CompanyPK { get; }

		protected FraudPreventionDataProvider FraudPreventionData { get; }

		#endregion
	}

	#endregion
}
