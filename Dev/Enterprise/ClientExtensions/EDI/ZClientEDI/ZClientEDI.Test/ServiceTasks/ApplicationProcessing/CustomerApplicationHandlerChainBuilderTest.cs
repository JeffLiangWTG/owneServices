using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler;

namespace ZClientEDI.Test.ServiceTasks.ApplicationProcessing.Test
{
	public class CustomerApplicationHandlerChainBuilderTest : TestCaseWithFactory
	{
		public void TestBuildApplicationHandlerChain_CustomerApplication()
		{
			var application = Factory.New<EdiIdentityApplication>();
			Factory.Save();

			var applicationHandlerChainBuilder = new CustomerApplicationHandlerChainBuilder();
			var handler = applicationHandlerChainBuilder.BuildApplicationHandlerChain();

			CombineAssertions(() =>
			{
				AssertNotNull("addCertHandler", handler);
				AssertType<BaseAddCertificateHandler>(handler);

				var addCertHandler = handler as BaseAddCertificateHandler;
				AssertNotNull("removeCertHandler", addCertHandler!.nextHandler);
				AssertType<BaseRemoveCertificateHandler>(addCertHandler.nextHandler);

				var removeCertHandler = addCertHandler.nextHandler as BaseRemoveCertificateHandler;
				AssertNotNull("rollbackAppHandler", removeCertHandler!.nextHandler);
				AssertType<BaseRollbackApplicationHandler>(removeCertHandler.nextHandler);

				var rollbackAppHandler = removeCertHandler.nextHandler as BaseRollbackApplicationHandler;
				AssertNull(rollbackAppHandler!.nextHandler);
			});
		}
	}
}
