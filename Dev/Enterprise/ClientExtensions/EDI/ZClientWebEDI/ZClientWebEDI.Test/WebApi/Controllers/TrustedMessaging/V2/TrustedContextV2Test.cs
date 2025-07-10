using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class TrustedContextV2Test : TestCaseWithFactory
	{
		public void TestNew()
		{
			var controller = new V2ControllerForTest();
			controller.Request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://www.cw1.com/api/sso/v2/test.auto-login-url"));
			controller.RequestContext = new HttpRequestContext() { ClientCertificate = CertificatesProvider.RemoteCertificate };

			var requestInfo = new ERequestInfo() { Product = "CW1", SystemId = "8000", InfoTimestamp = ZDateTime.UtcNow.ToDateTime() };
			var context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);

			AssertEquals(true, context.Success);
			AssertEquals(null, context.Messages);
			AssertEquals("CW1", context.Product);
			AssertEquals("8000", context.SystemId);
			AssertEquals("8000", context.TrustedSystem.ETS_SystemID);
		}

		public void TestNewWithHttpProxyHeader()
		{
			var controller = new V2ControllerForTest();
			controller.Request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://www.cw1.com/api/sso/v2/test.auto-login-url"));
			controller.RequestContext = new HttpRequestContext() { ClientCertificate = null };
			controller.Request.Headers.Add("X-SSL-Client-SHA1", CertificatesProvider.RemoteCertificate.GetCertHashString());
			controller.Request.Headers.Add("X-SSL-Client-Serial", CertificatesProvider.RemoteCertificate.GetSerialNumberString());

			var requestInfo = new ERequestInfo() { Product = "CW1", SystemId = "8000", InfoTimestamp = ZDateTime.UtcNow.ToDateTime() };
			var context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);

			AssertEquals(true, context.Success);
			AssertEquals(null, context.Messages);
			AssertEquals("CW1", context.Product);
			AssertEquals("8000", context.SystemId);
			AssertEquals("8000", context.TrustedSystem.ETS_SystemID);
		}

		public void TestCreateHttpActionResult()
		{
			var controller = new V2ControllerForTest();
			controller.Request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://www.cw1.com/api/sso/v2/test.auto-login-url"));
			controller.RequestContext = new HttpRequestContext() { ClientCertificate = CertificatesProvider.RemoteCertificate };
			controller.Configuration = new HttpConfiguration();

			var requestInfo = new ERequestInfo() { Product = "CW1", SystemId = "8000", InfoTimestamp = ZDateTime.UtcNow.ToDateTime() };
			var context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);
			context.ResponseInfo = new AutoLoginResponse(new Uri("https://www.cw.com/q?token=123"));

			AssertEquals(true, context.Success);
			AssertEquals(null, context.Messages);
			AssertEquals("CW1", context.Product);
			AssertEquals("8000", context.SystemId);
			AssertEquals("8000", context.TrustedSystem.ETS_SystemID);
			var responseMessage = context.CreateHttpActionResult().ExecuteAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
			AssertEquals("{\"url\":\"https://www.cw.com/q?token=123\"}", responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());

			context.ResponseInfo = null;
			context.Success = false;
			context.Messages = new WTG.TrustedMessaging.Models.ErrorMessages();
			context.Messages.AddMessage("C01", "something wrong!");
			responseMessage = context.CreateHttpActionResult().ExecuteAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
			AssertEquals("{\"errors\":[{\"code\":\"C01\",\"message\":\"something wrong!\"}]}", responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult());
		}

		public void TestErrorMessages()
		{
			var controller = new V2ControllerForTest();
			controller.Request = new HttpRequestMessage(HttpMethod.Post, new Uri("http://www.cw1.com/api/sso/v2/test.auto-login-url"));
			controller.RequestContext = new HttpRequestContext() { ClientCertificate = CertificatesProvider.RemoteCertificate };
			var requestInfo = new ERequestInfo() { Product = "CW1", SystemId = "8000", InfoTimestamp = ZDateTime.UtcNow.ToDateTime() };

			controller.Request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://www.cw1.com/api/sso/v2/test.auto-login-url"));
			controller.RequestContext = new HttpRequestContext() { ClientCertificate = CertificatesProvider.RemoteCertificate };
			requestInfo.Product = "XXX";
			var context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);
			AssertEquals(false, context.Success);
			AssertEquals(ErrorCodes.Codes.Validation_InvalidSystem, context.Messages.Messages.Single().Code);
			AssertEquals(ErrorCodes.Descriptions.Validation_InvalidSystem, context.Messages.Messages.Single().Message);

			requestInfo.Product = "CW1";
			controller.RequestContext = new HttpRequestContext() { ClientCertificate = null };
			context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);
			AssertEquals(false, context.Success);
			AssertEquals(ErrorCodes.Codes.Validation_MissingClientCertificate, context.Messages.Messages.Single().Code);
			AssertEquals(ErrorCodes.Descriptions.Validation_MissingClientCertificate, context.Messages.Messages.Single().Message);

			controller.RequestContext = new HttpRequestContext() { ClientCertificate = CertificatesProvider.LocalCertificate };
			context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);
			AssertEquals(false, context.Success);
			AssertEquals(ErrorCodes.Codes.Critical_CertificateMismatched, context.Messages.Messages.Single().Code);
			AssertEquals(ErrorCodes.Descriptions.Critical_CertificateMismatched, context.Messages.Messages.Single().Message);

			requestInfo.InfoTimestamp = ZDateTime.UtcNow.AddHours(-25).ToDateTime();
			context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);
			AssertEquals(false, context.Success);
			AssertEquals(ErrorCodes.Codes.Critical_InfoExpired, context.Messages.Messages.Single().Code);
			AssertEquals(ErrorCodes.Descriptions.Critical_InfoExpired, context.Messages.Messages.Single().Message);

			requestInfo.InfoTimestamp = ZDateTime.UtcNow.AddHours(23).ToDateTime();
			controller.RequestContext = new HttpRequestContext() { ClientCertificate = null };
			controller.Request.Headers.Add("X-SSL-Client-SHA1", "EA683A20F496D7A6DD4ACB1ABE874B1979957281");
			controller.Request.Headers.Add("X-SSL-Client-Serial", "457731D4A8C0CC894D8C86CE4A204A74");
			context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, controller);
			AssertEquals(false, context.Success);
			AssertEquals(ErrorCodes.Codes.Critical_CertificateMismatched, context.Messages.Messages.Single().Code);
			AssertEquals(ErrorCodes.Descriptions.Critical_CertificateMismatched, context.Messages.Messages.Single().Message);
		}

		CertificatesProviderForTest CertificatesProvider;

		protected override void SetUp()
		{
			base.SetUp();

			CertificatesProvider = new CertificatesProviderForTest();
			var product = "CW1";
			var system = Factory.New<EdiTrustedSystem>();
			system.ETS_Product = product;
			system.ETS_SystemID = "8000";
			system.GetOrCreateCertificateConfig();
			system.CertificateConfig.ETM_CertificateData = CertificatesProvider.RemoteCertificate.Export(X509ContentType.Cert);

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_DatabaseNumber = 8000;
			db.LD_TenantID = "DDDABCSYD";
			db.LD_Product = product;
			db.LD_ETS_TrustedSystem = system.PK;
			db.LicEnterprise.LE_EnterpriseID = "E001001";

			Factory.Save();
		}
	}

	[RoutePrefix("api/sso/v2/test")]
	class V2ControllerForTest : TrustedController
	{
		public V2ControllerForTest() : base()
		{
		}

		public V2ControllerForTest(NLogWrapper logger) : base(logger)
		{
		}

		[Route("auto-login-url")]
		[HttpPost]
		public IHttpActionResult GetAutoLoginUrl([FromBody] ERequestInfo requestInfo)
		{
			var context = TrustedContextV2<ERequestInfo, AutoLoginResponse>.New(requestInfo, this);
			context.ResponseInfo = new AutoLoginResponse(new Uri("https://www.cw.com/q?token=123"));
			return context.CreateHttpActionResult();
		}
	}
}
