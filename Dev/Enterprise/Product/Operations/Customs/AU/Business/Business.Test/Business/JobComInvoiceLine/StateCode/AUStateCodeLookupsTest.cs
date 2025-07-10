using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AUStateCodeLookupsTest : TestCaseWithFactory
	{
		public void TestAUStateCodeList()
		{
			AUStateCodeLookups aUStateCodeLookups = new AUStateCodeLookups(aUStateCode);
			AssertNotNull("Commodity Code List", aUStateCodeLookups.AUStateCodeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AUState_Hidden = "ACT,FO";
			aUStateCode = new AUStateCode(invoiceLine, Factory);
		}
		AUStateCode aUStateCode;
	}
}
