using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ContractCollection))]
	sealed class ContractCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			return new ContractCollection(invoiceHeader);
		}
	}
}
