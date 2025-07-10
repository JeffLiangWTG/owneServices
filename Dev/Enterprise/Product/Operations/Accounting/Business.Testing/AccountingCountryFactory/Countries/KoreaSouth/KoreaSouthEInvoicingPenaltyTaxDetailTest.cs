using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingPenaltyTaxDetail))]
	public class KoreaSouthEInvoicingPenaltyTaxDetailTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var detail = new KoreaSouthEInvoicingPenaltyTaxDetail("type", "explanation", "supplier", "receiver");
			AssertEquals("type", detail.Type);
			AssertEquals("explanation", detail.Explanation);
			AssertEquals("supplier", detail.Supplier);
			AssertEquals("receiver", detail.Receiver);
		}
	}
}
