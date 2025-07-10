using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class SharedImportJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestCustomsUQList()
		{
			JobComInvoiceLine invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals(typeof(AUCustomsImportUQList), new CMRImportJobComInvoiceLineValidationForTest(invoiceLine).CustomsUQList.GetType());
		}

		//		public void TestTariffDescription()
		//		{
		//			AUCClass DummyImportTariff = Factory.New<AUCClass>();
		//
		//			DummyImportTariff.UJ_Code = "0000.00.00 00";
		//			DummyImportTariff.UJ_Txt = ZString.Empty;
		//			
		//			InvoiceLine.JI_Tariff = "00000000 00";
		//			AssertEquals(true, new CMRImportJobComInvoiceLineValidationForTest(InvoiceLine).TariffDescription.IsEmpty);
		//
		//			DummyImportTariff.UJ_Txt = "TXT";
		//			AssertEquals("TXT", new CMRImportJobComInvoiceLineValidationForTest(InvoiceLine).TariffDescription);
		//		}

		#region Implementation

		protected JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
		}

		public class CMRImportJobComInvoiceLineValidationForTest : CMRImportJobComInvoiceLineValidation
		{
			public CMRImportJobComInvoiceLineValidationForTest(JobComInvoiceLine line)
				: base(line)
			{
			}
		}

		#endregion

	}
}
