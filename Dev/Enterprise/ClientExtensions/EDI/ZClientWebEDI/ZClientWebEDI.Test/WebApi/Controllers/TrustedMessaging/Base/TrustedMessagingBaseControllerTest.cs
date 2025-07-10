using System;
using System.Net.Http;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class TrustedMessagingBaseControllerTest : TestCaseWithFactory
	{
		public void TestCertificates_Database()
		{
			var logger = new NLogWrapperForTest(GetType());

			var certDb = SetupDbWithCertificate();

			var licence = BillingTestHelper.CreateLicence(Factory, "EEE", "DEF", "MEL");
			var requestDb = licence.Database;
			requestDb.LD_TenantID = "CSP573";
			requestDb.LD_DatabaseNumber = 4821;
			requestDb.LD_Product = "CSP";
			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateCertificateContext(controller, requestDb.TrustedSystem, requestDb.LD_Product, requestDb.LD_TenantID, certDb.LD_Product, certDb.LD_TenantID);
			controller.CertificateCore_Exposed(context);
			AssertEquals(certDb.TrustedSystem.CertificateConfig.ETM_CertificateData, context.ResponseInfo.CertificateData);
		}

		public void TestCertificates_TrustedService()
		{
			var logger = new NLogWrapperForTest(GetType());

			var certDb = SetupDbWithCertificate();

			var serviceCode = "DDD";
			var trustedServices = new CodeDescriptionBoolCollection
			{
				{ serviceCode, (NoResString)"Demo Service", true }
			};
			EDIDataRegistry.Instance.MyAccountTrustedServices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, trustedServices);

			var trustedService = Factory.New<EdiTrustedSystem>();
			trustedService.ETS_Product = serviceCode;
			Factory.Save();

			var controller = CreateController(logger);
			var context = CreateCertificateContext(controller, trustedService, serviceCode, string.Empty, certDb.LD_Product, certDb.LD_TenantID);
			controller.CertificateCore_Exposed(context);
			AssertEquals(certDb.TrustedSystem.CertificateConfig.ETM_CertificateData, context.ResponseInfo.CertificateData);
		}

		static TrustedContext<CertificateInfo, CertificateResponse> CreateCertificateContext(TrustedController controller,
			EdiTrustedSystem trustedSystem, string product, string systemId, string certRequestProduct, string certRequestSystemId)
		{
			var infoExpires = ZDateTime.UtcNow.AddMinutes(5).ToDateTime();
			var certInfo = new CertificateInfo()
			{
				Product = product,
				SystemId = systemId,
				CertificateOwnerProduct = certRequestProduct,
				CertificateOwnerSystemId = certRequestSystemId,
				InfoExpires = infoExpires
			};
			return new TrustedContextForTest<CertificateInfo, CertificateResponse>(product, systemId, certInfo, trustedSystem, controller) { Success = true };
		}

		LicenceDatabase SetupDbWithCertificate()
		{
			var product = "SMF";
			var licence = BillingTestHelper.CreateLicence(Factory, "AAA", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 23400;
			db.LD_Product = product;
			var remoteCert = db.GetOrCreateTrustedSystem().GetOrCreateCertificateConfig();
			remoteCert.ETM_CertificateData = CertificatesProviderTest.LoadLocalCertAsBytes("Client.cer");
			Factory.Save();

			return db;
		}

		TrustedMessagingBaseControllerForTest CreateController(NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/TrustedMessaging/Certificate");
			requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");
			var controller = new TrustedMessagingBaseControllerForTest(logger);
			controller.Request = requestMessage;
			return controller;
		}

		public class TrustedMessagingBaseControllerForTest : TrustedMessagingBaseController
		{
			public TrustedMessagingBaseControllerForTest() : base()
			{
			}

			public TrustedMessagingBaseControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			public void CertificateCore_Exposed(TrustedContext<CertificateInfo, CertificateResponse> context) => base.CertificateCore(context);
		}
	}
}
