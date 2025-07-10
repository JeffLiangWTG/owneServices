using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class EntryCreationStrategyBaseTest : TestCaseWithFactory
	{
		public void TestGetNonAmendableLineDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();

			DummyEntryCreationStrategy strategy = new DummyEntryCreationStrategy(new LineMerger(declaration));
			AssertEquals(CusEntryHeader.NatureTypesForImportCMR.Nature10, strategy.GetNonAmendableLineDetailsExposed(line));

			line.JI_IsPackToBondForLine = true;
			AssertEquals(CusEntryHeader.NatureTypesForImportCMR.Nature20, strategy.GetNonAmendableLineDetailsExposed(line));

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(CusEntryHeader.NatureTypesForImportCMR.Nature30, strategy.GetNonAmendableLineDetailsExposed(line));
		}

		public void TestDiffererntInstrumentTypeCode()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;
			line1.AddInfo.ZA_InstrumentCode_Hidden = "981";
			line1.AddInfo.ZA_InstrumentType_Hidden = "TL1";

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			DummyEntryCreationStrategy strategy = new DummyEntryCreationStrategy(merger);

			Customs.Business.MergeKey keyForLine1 = strategy.GetKeyForLine(line1);
			Customs.Business.MergeKey keyForLine2 = strategy.GetKeyForLine(line2);

			Assert("Line 1 and line2 cannot be merged into one", keyForLine1 != keyForLine2);
			Assert("Key1", keyForLine1.IndexOf(line1.JI_ConcessionOrder) >= 0);
		}

		public void TestDifferentValuationBasis()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;
			line1.AddInfo.ZA_ValuationBasis_Hidden = "UT";

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;
			line2.AddInfo.ZA_ValuationBasis_Hidden = "RT";

			LineMerger merger = new LineMerger(testDec);
			DummyEntryCreationStrategy strategy = new DummyEntryCreationStrategy(merger);

			Customs.Business.MergeKey keyForLine1 = strategy.GetKeyForLine(line1);
			Customs.Business.MergeKey keyForLine2 = strategy.GetKeyForLine(line2);

			Assert("Line 1 and line2 cannot be merged into one", keyForLine1 != keyForLine2);
			Assert("Key1", keyForLine1.IndexOf(line1.AddInfo.ZA_ValuationBasis_Hidden) >= 0);
			Assert("Key2", keyForLine2.IndexOf(line2.AddInfo.ZA_ValuationBasis_Hidden) >= 0);
		}

		public void TestDifferentTreatmentCode()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;
			line1.AddInfo.ZA_TreatmentCode_Hidden = "111";

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			DummyEntryCreationStrategy strategy = new DummyEntryCreationStrategy(merger);

			Customs.Business.MergeKey keyForLine1 = strategy.GetKeyForLine(line1);
			Customs.Business.MergeKey keyForLine2 = strategy.GetKeyForLine(line2);

			Assert("Line 1 and line2 cannot be merged into one", keyForLine1 != keyForLine2);
			Assert("Key1", keyForLine1.IndexOf(line1.AddInfo.ZA_TreatmentCode_Hidden) >= 0);
		}

		public void TestDifferentAddInfoString()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;
			line1.AddInfo.ZA_PRF = "Z";

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;
			line2.AddInfo.ZA_PRF = "S";

			LineMerger merger = new LineMerger(testDec);
			DummyEntryCreationStrategy strategy = new DummyEntryCreationStrategy(merger);

			Customs.Business.MergeKey keyForLine1 = strategy.GetKeyForLine(line1);
			Customs.Business.MergeKey keyForLine2 = strategy.GetKeyForLine(line2);

			Assert("Line 1 and line2 cannot be merged into one", keyForLine1 != keyForLine2);
		}
	}
}
