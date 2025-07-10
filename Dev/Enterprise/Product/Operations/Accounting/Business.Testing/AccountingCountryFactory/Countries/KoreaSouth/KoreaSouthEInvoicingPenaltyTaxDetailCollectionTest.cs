using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingPenaltyTaxDetailCollection))]
	public class KoreaSouthEInvoicingPenaltyTaxDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<KoreaSouthEInvoicingPenaltyTaxDetailCollection>
	{
		public void TestReadOnly()
		{
			var collection = GetCollectionToTest();
			Assert(collection.ReadOnly);
		}

		protected override KoreaSouthEInvoicingPenaltyTaxDetailCollection GetCollectionToTest()
		{
			return new KoreaSouthEInvoicingPenaltyTaxDetailCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new KoreaSouthEInvoicingPenaltyTaxDetail("type", "explanation", "supplier", "receiver");
		}
	}
}
