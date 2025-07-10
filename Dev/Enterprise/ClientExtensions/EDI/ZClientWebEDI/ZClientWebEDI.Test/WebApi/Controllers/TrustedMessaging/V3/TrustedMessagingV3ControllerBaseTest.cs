using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public abstract class TrustedMessagingV3ControllerBaseTest : TestCaseWithFactory
	{
		protected (HttpStatusCode statusCode, string content) CallWebApi(Uri requestUri, object requestObj, SystemToSystemTrustHelper trustHelper, string accessToken)
		{
			return CallWebApi(requestUri, HttpMethod.Post, requestObj, trustHelper, accessToken);
		}

		protected (HttpStatusCode statusCode, string content) CallWebApi(Uri requestUri, HttpMethod method, object requestObj, SystemToSystemTrustHelper trustHelper, string accessToken)
		{
			var requestMessage = new HttpRequestMessage(method, requestUri);
			requestMessage.Headers.Authorization = new AuthenticationHeaderValue(SystemToSystemTrustHelper.BearerKey, accessToken);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");

			using (var controller = GetTrustedController(Logger, trustHelper))
			{
				var responseMessage = ControllerTestHelper.Execute(controller, requestMessage);
				return (responseMessage.StatusCode, responseMessage.Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult() ?? "");
			}
		}

		protected void AssertWebApi(Uri requestUri, object requestObj, SystemToSystemTrustHelper trustHelper, string accessToken, HttpStatusCode expectedStatusCode, string expectedResponseContent)
		{
			AssertWebApi(requestUri, HttpMethod.Post, requestObj, trustHelper, accessToken, expectedStatusCode, expectedResponseContent);
		}

		protected void AssertWebApi(Uri requestUri, HttpMethod method, object requestObj, SystemToSystemTrustHelper trustHelper, string accessToken, HttpStatusCode expectedStatusCode, string expectedResponseContent)
		{
			var (statusCode, content) = CallWebApi(requestUri, method, requestObj, trustHelper, accessToken);
			CombineAssertions(() =>
			{
				AssertEquals(expectedStatusCode, statusCode);
				AssertEquals(expectedResponseContent, content);
			});
		}

		protected abstract TrustedController GetTrustedController(NLogWrapper logger, SystemToSystemTrustHelper trustHelper);

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new NLogWrapperForTest(GetType());

			var product = "CW1";

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_TenantID = string.Empty;
			db.LD_Product = product;
			db.LicEnterprise.LE_EnterpriseID = "E001001";
			Database = db;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();
		}

		protected LicenceDatabase Database;
		protected NLogWrapperForTest Logger;
	}
}
