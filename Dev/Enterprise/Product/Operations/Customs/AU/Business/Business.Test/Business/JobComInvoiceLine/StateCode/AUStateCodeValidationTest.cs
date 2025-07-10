using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AUStateCodeValidationTest : TestCaseWithFactory
	{
		public void TestCodeAgainstLookupList()
		{
			AUStateCode aUStateCode = new AUStateCode(invoiceLine, Factory);
			AssertNoError("AUState Code has no error", aUStateCode.CodeInfo, "Enter a valid selection.");

			aUStateCode.Code = "AAA";
			AssertHasError("AUState Code has error", aUStateCode.CodeInfo, "Enter a valid selection.");
		}

		public void TestDuplicatedCodes()
		{
			AUStateCode aUStateCode = invoiceLine.AUStateCodeCollection.AddNew();
			aUStateCode.Code = "NSW";
			AssertNoError(aUStateCode.CodeInfo, "Duplicated AU State Codes on this invoice line, (AU State Code : NSW)");

			AUStateCode aUStateCode2 = invoiceLine.AUStateCodeCollection.AddNew();
			aUStateCode2.Code = "NSW";
			AssertHasError(aUStateCode2.CodeInfo, "Duplicated AU State Codes on this invoice line, (AU State Code : NSW)");
		}

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_AUState_Hidden = "ACT,FO";
		}
		JobComInvoiceLine invoiceLine;
	}
}
