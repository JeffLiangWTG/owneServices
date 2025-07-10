using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class EdificeEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestDifferentRelatedLinePK()
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

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 0m;
			line2.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Parent;

			JobComInvoiceLine line3 = invoice2.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "0206.29.00 26";
			line3.JI_LinePrice = 10000m;
			line3.JI_LinePrefix = JobComInvoiceLine.LinePrefixString.Trailer;

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);

			Customs.Business.MergeKey keyForLine1 = strategy.GetKeyForLine(line1);
			Customs.Business.MergeKey keyForLine2 = strategy.GetKeyForLine(line2);
			Customs.Business.MergeKey keyForLine3 = strategy.GetKeyForLine(line3);

			Assert("Line 1 and line2 cannot be merged into one", keyForLine1 != keyForLine2);
			Assert("Line 3 and line2 cannot be merged into one", keyForLine2 != keyForLine3);
			Assert("Line 1 and line3 cannot be merged into one", keyForLine1 != keyForLine3);

			Assert("Key2", keyForLine2.IndexOf(new ZString(line3.PK.ToString())) >= 0);
			Assert("Key3", keyForLine3.IndexOf(new ZString(line2.PK.ToString())) >= 0);
		}

		public void TestInvoiceAndLinesFromDiffererntInvoicesWithItsOwnChargesNotMerged()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice1.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 150m, "AUD");

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice2.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 150m, "AUD");

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);
			Customs.Business.MergeKey keyForHeader1 = strategy.GetKeyForHeader(line1);
			Customs.Business.MergeKey keyForHeader2 = strategy.GetKeyForHeader(line2);

			Customs.Business.MergeKey keyForLine1 = strategy.GetKeyForLine(line1);
			Customs.Business.MergeKey keyForLine2 = strategy.GetKeyForLine(line2);

			Assert("Line 1 and line2 cannot be merged into one", keyForLine1 != keyForLine2);
			Assert("Invoice 1 and Invoice 2 cannot be merged into one", keyForHeader1 != keyForHeader2);
		}

		public void TestDifferentSupplier()
		{
			var supplier1 = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var supplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, supplier1.PK));

			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_OH_Supplier = supplier1.PK;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_OH_Supplier = supplier2.PK;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);
			Customs.Business.MergeKey keyForHeader1 = strategy.GetKeyForHeader(line1);
			Customs.Business.MergeKey keyForHeader2 = strategy.GetKeyForHeader(line2);

			AssertEquals("Differernt keys", false, keyForHeader1 == keyForHeader2);
			AssertEquals("Key1", true, keyForHeader1.IndexOf(supplier1.OH_Code) >= 0);
			AssertEquals("Key2", true, keyForHeader2.IndexOf(supplier2.OH_Code) >= 0);
		}

		public void TestDifferentCurrencyCode()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);
			Customs.Business.MergeKey keyForHeader1 = strategy.GetKeyForHeader(line1);
			Customs.Business.MergeKey keyForHeader2 = strategy.GetKeyForHeader(line2);

			AssertEquals("Differernt keys", false, keyForHeader1 == keyForHeader2);
			AssertEquals("Key1", true, keyForHeader1.IndexOf(invoice1.Invoice_Currency.RX_Code) >= 0);
			AssertEquals("Key2", true, keyForHeader2.IndexOf(invoice2.Invoice_Currency.RX_Code) >= 0);
		}

		public void TestDifferentValuationBasis()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_ValuationBasis = "RT";

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_ValuationBasis = "UT";

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);
			Customs.Business.MergeKey keyForHeader1 = strategy.GetKeyForHeader(line1);
			Customs.Business.MergeKey keyForHeader2 = strategy.GetKeyForHeader(line2);

			AssertEquals("Differernt keys", false, keyForHeader1 == keyForHeader2);
			AssertEquals("Key1 ", true, keyForHeader1.IndexOf(invoice1.JZ_ValuationBasis) >= 0);
			AssertEquals("Key2 ", true, keyForHeader2.IndexOf(invoice2.JZ_ValuationBasis) >= 0);
		}

		public void TestDifferentLineNature()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;
			line2.JI_IsPackToBondForLine = true;

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);
			Customs.Business.MergeKey keyForHeader1 = strategy.GetKeyForHeader(line1);
			Customs.Business.MergeKey keyForHeader2 = strategy.GetKeyForHeader(line2);

			AssertEquals("Differernt keys", false, keyForHeader1 == keyForHeader2);
			AssertEquals("Key1", true, keyForHeader1.IndexOf(line1.Nature) >= 0);
			AssertEquals("Key2", true, keyForHeader2.IndexOf(line2.Nature) >= 0);
		}

		public void TestDifferentEffectiveDutyDate()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "0206.29.00 26";
			line1.JI_LinePrice = 10000m;
			invoice1.AddInfo.ZA_EFD = "050210";

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0206.29.00 26";
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);
			Customs.Business.MergeKey keyForHeader1 = strategy.GetKeyForHeader(line1);
			Customs.Business.MergeKey keyForHeader2 = strategy.GetKeyForHeader(line2);

			AssertEquals("Differernt keys", false, keyForHeader1 == keyForHeader2);
			AssertEquals("Key1", true, keyForHeader1.IndexOf(invoice1.AddInfo.ZA_EFD) >= 0);
		}

		public void TestDifferentValuationDatesSplit()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_ValuationDateOverride = new ZDateTime(2005, 7, 21);

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_ValuationDateOverride = new ZDateTime(2005, 7, 22);

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();

			LineMerger merger = new LineMerger(testDec);
			EdificeEntryCreationStrategy strategy = new EdificeEntryCreationStrategy(merger);
			Customs.Business.MergeKey keyForHeader1 = strategy.GetKeyForHeader(line1);
			Customs.Business.MergeKey keyForHeader2 = strategy.GetKeyForHeader(line2);

			AssertEquals("Differernt keys", false, keyForHeader1 == keyForHeader2);
			//				AssertEquals("Key1", true, KeyForHeader1.IndexOf(Invoice1.JZ_ValuationDateOverride.ToString()) >=0);
		}
	}
}
