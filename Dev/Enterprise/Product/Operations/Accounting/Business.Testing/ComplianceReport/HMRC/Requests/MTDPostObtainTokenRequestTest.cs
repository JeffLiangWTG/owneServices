using System;
using System.Linq;
using System.Net;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDPostObtainTokenRequestTest : MTDRequestBaseTest
	{
		public override void TestSuccessfulResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/oauth/token");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));

				var client = new MTDClient(report);
				var token = new MTDTokensData()
				{
					access_token = "access token",
					expires_in = 1440,
					refresh_token = "refresh token",
					scope = "both",
					token_type = "bearer"
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, System.Net.HttpStatusCode.OK
					, token.ToJSON());

				var request = new MTDPostObtainTokenRequest(client, MTDTestHelper.MockAuthorizationCode);

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDTokensData>(request);

				AssertNotNull("Content", response);
				AssertNull("Error", client.LastErrorInfo);
				AssertEquals(HttpStatusCode.OK, client.LastHttpStatusCode);
				AssertEquals("access_token", "access token", response.access_token);
				AssertEquals("expires_in", 1440, response.expires_in);
				AssertEquals("refresh_token", "refresh token", response.refresh_token);
				AssertEquals("scope", "both", response.scope);
				AssertEquals("token_type", "bearer", response.token_type);
			}
		}

		public override void TestFailedResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/oauth/token");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));

				var client = new MTDClient(report);

				var responseErrorInfo = new MTDErrorInfo()
				{
					code = "invalid_client",
					message = "Client ID is invalid",
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, System.Net.HttpStatusCode.Unauthorized
					, responseErrorInfo.ToJSON());

				var request = new MTDPostObtainTokenRequest(client, MTDTestHelper.MockAuthorizationCode);

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDTokensData>(request);

				AssertNull("Content", response);
				AssertNotNull("Error", client.LastErrorInfo);
				AssertEquals(HttpStatusCode.Unauthorized, client.LastHttpStatusCode);
				AssertErrorInfo(client.LastErrorInfo
					, "invalid_client"
					, "Client ID is invalid");
			}
		}

		public override void TestRequestHeaderContent()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/oauth/token");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = new MTDClient(report);
				var request = new MTDPostObtainTokenRequest(client, MTDClient.AuthorisationPart);
				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestHeaders = httpRequest.Headers;
					AssertEquals("Empty request header", false, requestHeaders.Any());
				}
			}
		}

		public override void TestRequestBodyContent()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/oauth/token");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = new MTDClient(report);
				var request = new MTDPostObtainTokenRequest(client, string.Empty);
				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestBody = httpRequest.Content;

					//Assert Content Header
					if (requestBody.Headers.TryGetValues("Content-Type", out var contentTypes))
					{
						AssertEquals(1, contentTypes.Count());
						AssertEquals("application/x-www-form-urlencoded", contentTypes.First());
					}
					else
					{
						Assert("Missing Content-type", false);
					}

					//Assert Content Body
					var expectedRequestBodyContent = FormattableString.Invariant($"{MTDClient.AuthorisationPart}&grant_type=authorization_code&redirect_uri=edient%3ACommand%3DAcc.WebCallback%3FHMRC&code=");
					var actualRequestBodyContent = requestBody.ReadAsStringAsync().Result;
					AssertEquals("Content body", expectedRequestBodyContent, actualRequestBodyContent);
				}
			}
		}

		protected override MTDRequestBase GetRequest(MTDClient client) => new MTDPostObtainTokenRequest(client, string.Empty);

		public override void TestGetAsString()
		{
			var company = CreateUKCompany();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = new MTDClient(report);
				var request = new MTDPostObtainTokenRequest(client, MTDTestHelper.MockAuthorizationCode);
				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestDetails = httpRequest.GetAsString();
					AssertEquals("Obtain Token request details", @"Request Details:
Method: POST
Uri: https://test-api.service.hmrc.gov.uk/oauth/token
Header: 
Body: client_secret=b5d69a47-ce66-45cf-b410-97a84248ebdd&client_id=dnTztKp2F1nNvEhHuR8mxyiWOYMa&grant_type=authorization_code&redirect_uri=edient%3ACommand%3DAcc.WebCallback%3FHMRC&code=bcb8cae0a1c046f38ecb863820d441ef", requestDetails);
				}
			}
		}
	}
}
