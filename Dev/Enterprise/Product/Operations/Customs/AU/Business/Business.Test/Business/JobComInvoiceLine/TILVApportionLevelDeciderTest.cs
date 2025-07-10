using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class TILVApportionLevelDeciderTest : TestCaseWithFactory
	{
		public void TestWhenInvoiceHasItsOwnNonDutiableGSTApplicableCharges()
		{
			AssertEquals(false, TILVApportionLevelDecider.IsManuallyAdjustedInvoice(declaration, invoice));

			invoice.Charges.AddNew("OFT", 50m, "AUD");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals(true, TILVApportionLevelDecider.IsManuallyAdjustedInvoice(declaration, invoice));
		}

		public void TestWhenInvoiceHasTILV()
		{
			AssertEquals(false, TILVApportionLevelDecider.IsManuallyAdjustedInvoice(declaration, invoice));

			invoice.AddInfo.ZA_TILV = "0AUD";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, TILVApportionLevelDecider.IsManuallyAdjustedInvoice(declaration, invoice));
		}

		public void TestSubGroupHasItsOwnNonDutiableGSTApplicableCharges()
		{
			AssertEquals(false, TILVApportionLevelDecider.IsManuallyAdjustedInvoice(declaration, invoice));

			declaration.TopGroupInvoice.Charges.AddNew("OFT", 50m, "AUD");
			JobComInvoiceGroupHeader subGroup = (JobComInvoiceGroupHeader)declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
			subGroup.Charges.AddNew("OFT", 40m, "AUD");
			invoice.JZ_JZ_GroupInvoiceFK = subGroup.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(true, TILVApportionLevelDecider.IsManuallyAdjustedInvoice(declaration, invoice));
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			declaration.InvoiceLines.AddNew();
		}
	}
}
