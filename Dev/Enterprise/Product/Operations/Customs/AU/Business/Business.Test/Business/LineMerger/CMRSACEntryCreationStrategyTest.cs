using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRSACEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestGetKeyForHeader()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = "NZ";
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			LineMerger merger = new LineMerger(testDec);
			CMRSACEntryCreationStrategy strategy = new CMRSACEntryCreationStrategy(merger);

			Customs.Business.MergeKey keyForHeader = strategy.GetKeyForHeader(invoiceLine);
			AssertEquals("KeyForHeader contains Orgin in AddInfo", true, keyForHeader.Contains(invoice.AddInfo.ZA_ORG));
		}
	}
}
