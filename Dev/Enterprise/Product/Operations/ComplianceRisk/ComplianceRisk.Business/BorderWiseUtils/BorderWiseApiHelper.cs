using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using WTG.Foundation.Http;

namespace Enterprise.ComplianceRisk.Business
{
	public class BorderWiseApiHelper : IBorderWiseApiHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string ConditionNotApply = "No";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal const string ConditionUncertain = "Uncertain";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal const string ConditionApply = "Yes";
		const string ComplianceCheckEndpoint = "/api/v4/compliance/check";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		const string SupportedCountriesCheckEndpoint = "/api/v3/compliance/supported-countries";

		public async Task<ICollection<ComplianceCheckResponseModel>> ComplianceCheckAsync(IEnumerable<ComplianceCheckRequestModel> complianceCheckRequests, CancellationToken cancellationToken)
		{
#if DEBUG
			if (Globals.IsTest && ReturnDummyResponseForTest)
			{
				return DummyComplianceCheckResponseModelsForTest;
			}
#endif

			ICollection<ComplianceCheckResponseModel> responseModel = null;

			var baseUrl = DataRegistry.Instance.BorderWiseWebAddress;
			var borderWiseUrl = new StringBuilder()
				.Append(baseUrl != null ? baseUrl.TrimEnd('/') : "")
				.Append(ComplianceCheckEndpoint).ToString();

			using var client = GetHttpClient();
			var content = new StringContent(JsonConvert.SerializeObject(complianceCheckRequests));
			content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse((NoResString)"application/json-patch+json");
			var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(DataRegistry.BorderWiseAPIKey));
			var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier(string.Empty);
			content.Headers.Add((NoResString)"Authentication", ComputeSha256Hash(decodedString + licenseCode));
			content.Headers.Add("LicenseCode", licenseCode);

			var response = await client.PostAsync(borderWiseUrl, content, cancellationToken).ConfigureAwait(false);

			if (response.IsSuccessStatusCode)
			{
				//TODO: use the ReadAsStringAsync(cancellationToken) once we finish migration to .net 5+
				var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				responseModel = JsonConvert.DeserializeObject<ICollection<ComplianceCheckResponseModel>>(responseText);
			}
			else
			{
				var errorMessage = await TryGetErrorMessage(response, cancellationToken);

				throw new ComplianceCheckException($"Failed to do compliance check, failed code: {response.StatusCode}, error message: {errorMessage} url: {borderWiseUrl}");
			}

			return responseModel;
		}

		public async Task<SupportedCountriesCheckResponseModel> SupportedCountriesCheckAsync(CancellationToken cancellationToken)
		{
#if DEBUG
			if (Globals.IsTest && ReturnDummyResponseForTest)
			{
				return DummySupportedCountriesCheckResponseModelForTest;
			}
#endif

			SupportedCountriesCheckResponseModel responseModel = null;

			var baseUrl = DataRegistry.Instance.BorderWiseWebAddress;
			var borderWiseUrl = new StringBuilder()
				.Append(baseUrl != null ? baseUrl.TrimEnd('/') : "")
				.Append(SupportedCountriesCheckEndpoint).ToString();

			using var client = GetHttpClient();
			var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(DataRegistry.BorderWiseAPIKey));
			var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier(string.Empty);

			var requestUri = new UriBuilder(borderWiseUrl).ToString();

			var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
			request.Headers.Add((NoResString)"Authentication", ComputeSha256Hash(decodedString + licenseCode));
			request.Headers.Add("LicenseCode", licenseCode);

			var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
			if (response.IsSuccessStatusCode)
			{
				//TODO: use the ReadAsStringAsync(cancellationToken) once we finish migration to .net 5+
				var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				responseModel = JsonConvert.DeserializeObject<SupportedCountriesCheckResponseModel>(responseText);
			}
			else
			{
				var errorMessage = await TryGetErrorMessage(response, cancellationToken);

				throw new ComplianceCheckException($"Failed to do supported countries check, failed code: {response.StatusCode}, error message: {errorMessage} url: {borderWiseUrl}");
			}
			return responseModel;
		}

		static string ComputeSha256Hash(string rawData)
		{
			using var sha256Hash = SHA256.Create();
			return Convert.ToBase64String(sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData)));
		}

		async Task<string> TryGetErrorMessage(HttpResponseMessage response, CancellationToken cancellationToken)
		{
			try
			{
				//TODO: use the ReadAsStringAsync(cancellationToken) once we finish migration to .net 5+
				var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				var errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(responseText);
				return errorMessage?.Message;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// ignore
			}

			return null;
		}

		HttpClient GetHttpClient()
		{
			var httpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
			var httpClient = httpClientFactory
				.CreateNew(
					new HttpClientHandlerWithDiagnostics(new CookieContainer()),
					TimeSpan.FromSeconds(60));

			return httpClient;
		}

#if DEBUG
		static SupportedCountriesCheckResponseModel DummySupportedCountriesCheckResponseModelForTest { get; set; }

		static bool ReturnDummyResponseForTest { get; set; } = true;

		public static IDisposable SetResponse(params ComplianceCheckResponseModel[] responses)
		{
			return new SetResponseModel(responses);
		}

		public static IDisposable SetSupportedCountriesResponse(SupportedCountriesCheckResponseModel response)
		{
			return new SetSupportedCountriesResponseModel(response);
		}

		static ICollection<ComplianceCheckResponseModel> DummyComplianceCheckResponseModelsForTest { get; set; }

		class SetResponseModel : IDisposable
		{
			internal SetResponseModel(ComplianceCheckResponseModel[] responses)
			{
				DummyComplianceCheckResponseModelsForTest = responses;
			}

			public void Dispose()
			{
				DummyComplianceCheckResponseModelsForTest = null;
			}
		}

		class SetSupportedCountriesResponseModel : IDisposable
		{
			public SetSupportedCountriesResponseModel(SupportedCountriesCheckResponseModel response)
			{
				DummySupportedCountriesCheckResponseModelForTest = response;
			}
			public void Dispose()
			{
				DummySupportedCountriesCheckResponseModelForTest = null;
			}
		}

		internal class SetReturnDummyResponseForTestToFalse : IDisposable
		{
			public SetReturnDummyResponseForTestToFalse()
			{
				ReturnDummyResponseForTest = false;
			}

			public void Dispose()
			{
				ReturnDummyResponseForTest = true;
			}
		}
#endif
	}
}
