using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using CargoWise.Common.JSON.Extensions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDClientTest : TestCaseWithFactory
	{
		public void TestEnquireAccessAndRefreshTokens()
		{
			ComplianceReport.OAuthClientAuthorisation += ComplianceReport_OAuthClientAuthorisation;
			AssertEnquireAccessAndRefreshTokens();
			ComplianceReport.OAuthClientAuthorisation -= ComplianceReport_OAuthClientAuthorisation;
		}

		public void TestEnquireAccessAndRefreshTokens_Concurrent()
		{
			ComplianceReport.OAuthClientAuthorisation += ComplianceReport_OAuthClientAuthorisation_Concurrent;
			AssertEnquireAccessAndRefreshTokens();
			ComplianceReport.OAuthClientAuthorisation -= ComplianceReport_OAuthClientAuthorisation_Concurrent;
		}

		void AssertEnquireAccessAndRefreshTokens()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Creator.CreateConfigurationForComplianceReport(ComplianceReport, "AL", "TXR", "", "VAT");

				var tokens = new MTDTokensData() { token_type = "bearer", access_token = "access-token", refresh_token = "refresh-token", expires_in = 1440, scope = "read:vat+write:vat" };
				ClientHandlerMock.AddJsonResponse("https://test-api.service.hmrc.gov.uk/oauth/token", HttpStatusCode.OK, tokens.ToJSON());

				AssertEquals("Access token", string.Empty, MTDClient.GetAccessToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("Refresh token", string.Empty, MTDClient.GetRefreshToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("OAuthHandlerCallCount", 0, OAuthHandlerCallCount);

				var result = Client.EnquireAccessAndRefreshTokens();
				Assert("IsSuccessful", result.IsSuccessful);
				AssertEquals("ErrorMessage", string.Empty, result.ErrorMessage);

				AssertEquals("Access token", "access-token", MTDClient.GetAccessToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("Refresh token", "refresh-token", MTDClient.GetRefreshToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("OAuthHandlerCallCount", 1, OAuthHandlerCallCount);

				OAuthHandlerCallCount = 0;
				ClientHandlerMock.ClearAllResponses();
				tokens = new MTDTokensData() { token_type = "bearer", access_token = "new-access-token", refresh_token = "new-refresh-token", expires_in = 1440, scope = "read:vat+write:vat" };
				ClientHandlerMock.AddJsonResponse("https://test-api.service.hmrc.gov.uk/oauth/token", HttpStatusCode.OK, tokens.ToJSON());

				result = Client.EnquireAccessAndRefreshTokens();
				Assert("IsSuccessful", result.IsSuccessful);
				AssertEquals("ErrorMessage", string.Empty, result.ErrorMessage);

				AssertEquals("Access token", "new-access-token", MTDClient.GetAccessToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("Refresh token", "new-refresh-token", MTDClient.GetRefreshToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("OAuthHandlerCallCount", 0, OAuthHandlerCallCount);
			}
		}

		public void TestSendRequestCallsEnquireAccessAndRefreshTokens()
		{
			ComplianceReport.OAuthClientAuthorisation += ComplianceReport_OAuthClientAuthorisation;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				Creator.CreateConfigurationForComplianceReport(ComplianceReport, "AL", "TXR", "", "VAT");

				var tokens = new MTDTokensData() { token_type = "bearer", access_token = "access-token", refresh_token = "refresh-token", expires_in = 1440, scope = "read:vat+write:vat" };
				ClientHandlerMock.AddJsonResponse("https://test-api.service.hmrc.gov.uk/oauth/token", HttpStatusCode.OK, tokens.ToJSON());

				var dateFrom = ComplianceReport.ACR_DateFrom.ToMTDCompliantFormat();
				var dateTo = ComplianceReport.ACR_DateTo.ToMTDCompliantFormat();
				var dueDate = ComplianceReport.ACR_DateTo.AddDays(10).ToMTDCompliantFormat();

				var expectedEndpoint = FormattableString.Invariant($"https://test-api.service.hmrc.gov.uk/organisations/vat/{Client.VATRegistrationNumber}/liabilities?from={dateFrom}&to={dateTo}");
				var responseData = new MTDLiabilities()
				{
					liabilities = new MTDLiability[]
					{
						new MTDLiability()
						{
							taxPeriod = new MTDTaxPeriod()
							{
								from = dateFrom,
								to = dateTo
							},
							type = "VAT GB",
							originalAmount = 250M,
							outstandingAmount = 100M,
							due = dueDate
						}
					}
				};
				ClientHandlerMock.AddJsonResponse(expectedEndpoint, System.Net.HttpStatusCode.OK, responseData.ToJSON());

				AssertEquals("Access token", string.Empty, MTDClient.GetAccessToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("Refresh token", string.Empty, MTDClient.GetRefreshToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("OAuthHandlerCallCount", 0, OAuthHandlerCallCount);

				var request = new MTDGetLiabilitiesRequest(Client);

				var response = Client.SendRequest<MTDLiabilities>(request);
				AssertNotNull(response);

				AssertEquals("Access token", "access-token", MTDClient.GetAccessToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("Refresh token", "refresh-token", MTDClient.GetRefreshToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("OAuthHandlerCallCount", 1, OAuthHandlerCallCount);

				AssertEquals("Count", 1, response.liabilities.Count());
				var liability = response.liabilities.ToArray()[0];
				AssertEquals("tax period from", dateFrom, liability.taxPeriod.from);
				AssertEquals("tax period to", dateTo, liability.taxPeriod.to);
				AssertEquals("type", "VAT GB", liability.type);
				AssertEquals("originalAmount", 250M, liability.originalAmount);
				AssertEquals("outstandingAmount", 100M, liability.outstandingAmount);
				AssertEquals("due", dueDate, liability.due);

				OAuthHandlerCallCount = 0;
				ClientHandlerMock.ClearAllResponses();
				var responseErrorInfo = new MTDErrorInfo()
				{
					code = "NOT_FOUND",
					message = "The remote endpoint has indicated that no associated data is found"
				};
				ClientHandlerMock.AddJsonResponse(expectedEndpoint, System.Net.HttpStatusCode.BadRequest, responseErrorInfo.ToJSON());

				request = new MTDGetLiabilitiesRequest(Client);

				var actualEndpoint = FormattableString.Invariant($"https://test-api.service.hmrc.gov.uk/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				response = Client.SendRequest<MTDLiabilities>(request);

				AssertNull("No Content", response);
				AssertNotNull("Error", Client.LastErrorInfo);
				AssertEquals("code", "NOT_FOUND", Client.LastErrorInfo.code);
				AssertEquals("message", "The remote endpoint has indicated that no associated data is found", Client.LastErrorInfo.message);

				AssertEquals("Access token", "access-token", MTDClient.GetAccessToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("Refresh token", "refresh-token", MTDClient.GetRefreshToken(ComplianceReport.ACR_GC_Company));
				AssertEquals("OAuthHandlerCallCount", 0, OAuthHandlerCallCount);
			}

			ComplianceReport.OAuthClientAuthorisation -= ComplianceReport_OAuthClientAuthorisation;
		}

		public void TestVGetVATRegistrationNumberIsCleaningSpaces()
		{
			AssertNotNull(ComplianceReport?.Company?.OrgProxy);
			ComplianceReport.Company.OrgProxy.CustomsCodes.AddNew("VAT", "1234567890");
			var vrnRegistration = ComplianceReport.Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry("VAT", ComplianceReport.Company.AccountingCountry);
			AssertNotNull(vrnRegistration);

			AssertEquals("1234567890", Client.VATRegistrationNumber);

			vrnRegistration.OK_CustomsRegNo = " 123  45678 ";
			AssertEquals("Spaces should be cleaned", "12345678", Client.VATRegistrationNumber);
		}

		public void TestMTDClientOAuthUri()
		{
			AssertEquals("edient:Command=Acc.WebCallback?HMRC", MTDClient.RedirectUrl);
			AssertEquals("https://test-api.service.hmrc.gov.uk/oauth/authorize?response_type=code&client_id=dnTztKp2F1nNvEhHuR8mxyiWOYMa&scope=read:vat+write:vat&state=null&redirect_uri=edient:Command=Acc.WebCallback?HMRC", MTDClient.OAuthUri.AbsoluteUri);

			AccountingConfigurationRegistry.Instance.IsMTDProductionMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("https://api.service.hmrc.gov.uk/oauth/authorize?response_type=code&client_id=dnTztKp2F1nNvEhHuR8mxyiWOYMa&scope=read:vat+write:vat&state=null&redirect_uri=edient:Command=Acc.WebCallback?HMRC", MTDClient.OAuthUri.AbsoluteUri);
		}

		void ComplianceReport_OAuthClientAuthorisation(object sender, AccComplianceReport.OAuthClientAuthorisationEventArgs e)
		{
			e.AuthorisationCode = "authorisation_code";
			OAuthHandlerCallCount++;
		}

		void ComplianceReport_OAuthClientAuthorisation_Concurrent(object sender, AccComplianceReport.OAuthClientAuthorisationEventArgs e)
		{
			var otherClient = new MTDClient(ComplianceReport);
			var result = otherClient.EnquireAccessAndRefreshTokens();
			Assert("IsSuccessful", !result.IsSuccessful);
			AssertEquals("ErrorMessage", "Another user is already completing the authentication with HMRC. Please try again later.", result.ErrorMessage);

			e.AuthorisationCode = "authorisation_code";
			OAuthHandlerCallCount++;
		}

		int OAuthHandlerCallCount;

		protected override void SetUp()
		{
			base.SetUp();

			ComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			ComplianceReport.ACR_ReportType = "MTD";

			ClientHandlerMock = MTDHttpClientHandlerMock.New();
			var httpClient = new HttpClient(ClientHandlerMock);

			httpClient.BaseAddress = new Uri(AccountingConfigurationRegistry.Instance.MTDTestWebServiceUrl.Value);
			MTDClient.HttpClientProvider.TestInstance.SetClient_ForTestOnly(httpClient);
			AccountingConfigurationRegistry.Instance.IsMTDProductionMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Creator = new TestObjectCreator(Factory);
			Client = new MTDClient(ComplianceReport);
		}

		AccComplianceReport ComplianceReport;
		MTDClient Client;
		MTDHttpClientHandlerMock ClientHandlerMock;
		TestObjectCreator Creator;
	}
}
