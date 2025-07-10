using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	class ElectronicInvoicingObjectFactoryTest : TestCaseWithFactory
	{
		public void TestGetElectronicInvoicingUpdateActionPermissions_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingUpdateActionPermissions());
		}

		public void TestGetEInvoicingConfigurationChecks_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetEInvoicingConfigurationChecks());
		}

		public void TestGetElectronicInvoicingRequeueStrategy_ReturnsNotNull()
		{
			AssertNotNull(ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetElectronicInvoicingRequeueStrategy());
		}
	}
}
