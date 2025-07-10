using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.Business.eNett_Integration.Testing
{
	class StorageFeeInvoicePaymentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApportionmentMethods()
		{
			var lookups = new StorageFeeInvoicePaymentLookups(new StorageFeeInvoicePayment(new Mock<IContainerStorageDataProvider>().Object));
			Assert(!lookups.ApportionmentMethods.ContainsCode(AllocationMethod.FreeSpaceContribution));
			Assert(!lookups.ApportionmentMethods.ContainsCode(AllocationMethod.OuterPackTotal));
		}
	}
}
