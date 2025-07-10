using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler;

namespace ZClientEDI.Test.ServiceTasks.ApplicationProcessing.Test
{
	public class AzureApplicationHandlerChainBuilderTest : TestCaseWithFactory
	{
		public void TestBuildApplicationHandlerChain_AzureApplication()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();

			var applicationHandlerChainBuilder = new AzureApplicationHandlerChainBuilder(null, null);
			var handler = applicationHandlerChainBuilder.BuildApplicationHandlerChain(application);

			CombineAssertions(() =>
			{
				AssertNotNull("createAppHandler", handler);
				AssertType<CreateApplicationHandler>(handler);

				var createAppHandler = handler as CreateApplicationHandler;
				AssertNotNull("addCertificateHandler", createAppHandler!.nextHandler);
				AssertType<AddCertificateHandler>(createAppHandler.nextHandler);

				var addCertHandler = createAppHandler.nextHandler as AddCertificateHandler;

				AssertNotNull("removeCertHandler", addCertHandler!.nextHandler);
				AssertType<RemoveCertificateHandler>(addCertHandler.nextHandler);

				var removeCertHandler = addCertHandler.nextHandler as RemoveCertificateHandler;
				AssertNotNull("redirectUrlHandler", removeCertHandler!.nextHandler);
				AssertType<RedirectUrlHandler>(removeCertHandler.nextHandler);

				var redirectUrlHandler = removeCertHandler.nextHandler as RedirectUrlHandler;
				AssertNotNull("rollbackAppHandler", redirectUrlHandler!.nextHandler);
				AssertType<RollbackApplicationHandler>(redirectUrlHandler.nextHandler);

				var rollbackAppHandler = redirectUrlHandler.nextHandler as RollbackApplicationHandler;
				AssertNotNull("rollbackAppHandler", rollbackAppHandler);
				AssertNull(rollbackAppHandler!.nextHandler);
			});
		}

		public void TestReactivateAzureApplication()
		{
			var licenseEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var license = Factory.NewWithValidTestData<LicenceDatabase>();
			license.LD_LE = licenseEnterprise.PK;

			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ClientID = Guid.NewGuid().ToString();
			application.IDA_LD = license.PK;
			application.IDA_ApplicationName = "TestApp";
			application.IDA_IsRollback = true;
			application.IDA_IsActive = false;

			var applicationHandlerChainBuilder = new AzureApplicationHandlerChainBuilder(null, null);
			var handler = applicationHandlerChainBuilder.BuildApplicationHandlerChain(application);

			CombineAssertions(() =>
			{
				AssertNotNull("reactivateAppHandler", handler);
				AssertType<ReactivateApplicationHandler>(handler);
			});
		}
	}
}

