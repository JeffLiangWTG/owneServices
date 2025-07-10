using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public abstract class TrustedMessagingV2ControllerBaseTest : TestCaseWithFactory
	{
		protected (HttpStatusCode statusCode, string content) CallWebApi(Uri requestUri, object requestObj, HttpMethod method = null)
		{
			var requestMessage = new HttpRequestMessage(method ?? HttpMethod.Post, requestUri);
			requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestObj), Encoding.UTF8, "application/json");

			using (var controller = GetTrustedController(Logger))
			{
				controller.RequestContext = new HttpRequestContext() { ClientCertificate = ClientCert };
				var responseMessage = Execute(controller, requestMessage);
				return (responseMessage.StatusCode, responseMessage.Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult() ?? "");
			}
		}

		protected void AssertWebApi(Uri requestUri, object requestObj, HttpStatusCode expectedStatusCode, string expectedResponseContent)
		{
			AssertWebApi(requestUri, null, requestObj, expectedStatusCode, expectedResponseContent);
		}

		protected void AssertWebApi(Uri requestUri, HttpMethod method, object requestObj, HttpStatusCode expectedStatusCode, string expectedResponseContent)
		{
			var (statusCode, content) = CallWebApi(requestUri, requestObj, method);
			CombineAssertions(() =>
			{
				AssertEquals(expectedStatusCode, statusCode);
				AssertEquals(expectedResponseContent, content);
			});
		}

		static HttpResponseMessage Execute(TrustedController controller, HttpRequestMessage request, Type controllerTargetType = null)
		{
			using (var configuration = new HttpConfiguration())
			{
				configuration.MapHttpAttributeRoutes();
				configuration.EnsureInitialized();

				var requestContext = new HttpRequestContext
				{
					Configuration = configuration,
					Url = new UrlHelper(request)
				};

				requestContext.ClientCertificate = controller.RequestContext.ClientCertificate;
				request.Properties[HttpPropertyKeys.RequestContextKey] = requestContext;

				var controllerType = controllerTargetType ?? controller.GetType();
				var controllerDescriptor = new HttpControllerDescriptor(configuration, controllerType.Name, controllerType);

				var context = new HttpControllerContext(requestContext, request, controllerDescriptor, controller)
				{
					RouteData = configuration.Routes.GetRouteData(request)
				};

				var task = controller.ExecuteAsync(context, CancellationToken.None);
				var result = task.Result;
				return result;
			}
		}

		protected abstract TrustedController GetTrustedController(NLogWrapper logger);

		protected override void SetUp()
		{
			base.SetUp();
			Logger = new NLogWrapperForTest(GetType());

			ClientCert = new CertificatesProviderForTest().RemoteCertificate;
			var product = "CW1";
			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "8000";
			system.GetOrCreateCertificateConfig();
			system.CertificateConfig.ETM_CertificateData = ClientCert.Export(X509ContentType.Cert);
			TrustedSystem = system;

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LicEnterprise.LE_EnterpriseID = "E001001";
			Database = db;

			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "U048173";
			user.EUA_FullName = "User One";
			user.EUA_Email = "user.one@test.com";

			Factory.Save();
		}

		protected EdiTrustedSystem TrustedSystem;
		protected LicenceDatabase Database;
		X509Certificate2 ClientCert;
		protected NLogWrapperForTest Logger;
	}
}
