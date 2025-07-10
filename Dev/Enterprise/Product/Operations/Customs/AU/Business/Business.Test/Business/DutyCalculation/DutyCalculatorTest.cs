using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DutyCalculatorTest : TestCaseWithFactory
	{
		public void TestUseCustomsValueToCalculateDuty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MergeBy = "NON";
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 26);

			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.7721m, helper.USDCurrency);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 141.44m, "USD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 262.15m, "USD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 3.13m, "USD");

			invoice.JZ_InvoiceAmount = 847.84m;
			invoice.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 847.84m;
			line1.JI_CountryOfOrigin = "CN";
			line1.JI_Tariff = "9403.20.00 19";
			line1.JI_AddInfo = "PRF=T*DRC=13*ORG=CN";

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 0m;
			line2.JI_CountryOfOrigin = "US";
			line2.JI_Tariff = "8524.32.00 02";
			line2.JI_AddInfo = "PRF=X*ORG=US*ADJ=115.00USD";

			JobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 0m;
			line3.JI_CustomsQuantity = 132;
			line3.JI_CountryOfOrigin = "US";
			line3.JI_Tariff = "9999.31.18 24";
			line3.JI_AddInfo = "PRF=X*ORG=US*ADJ=130.00USD";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Three entry line", 3, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			var entryLine1 = Factory.Load<CusEntryLine>(line1.JI_CL);
			var entryLine2 = Factory.Load<CusEntryLine>(line2.JI_CL);
			var entryLine3 = Factory.Load<CusEntryLine>(line3.JI_CL);

			AssertEquals("Customs value for line1", 1281.27m, entryLine1.CL_CustomsValue, 0.01m);
			AssertEquals("Customs value for line2", 148.94m, entryLine2.CL_CustomsValue, 0.01m);
			AssertEquals("Customs value for line3", 168.37m, entryLine3.CL_CustomsValue, 0.01m);
			AssertEquals("Total customs value", 1598.58m, entryHeader.CustomsValueInAUD.Amount, 0.01m);
		}

		public void TestUseInvoiceORG_PRFIfLineOnesEmpty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MergeBy = "NON";
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 26);

			ZTestHelper helper = new ZTestHelper(Factory);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.7721m, helper.USDCurrency);
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 141.44m, "USD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 262.15m, "USD");
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 3.13m, "USD");

			invoice.JZ_InvoiceAmount = 847.84m;
			invoice.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.PackedAtFactory;
			invoice.AddInfo.ZA_ORG = "US";
			invoice.AddInfo.ZA_PRF = "X";

			JobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 847.84m;
			line1.JI_CountryOfOrigin = "CN";
			line1.JI_Tariff = "9403.20.00 19";
			line1.JI_AddInfo = "PRF=T*DRC=13*ORG=CN";

			JobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 0m;
			line2.JI_Tariff = "8524.32.00 02";
			line2.JI_AddInfo = "ADJ=115.00USD";

			JobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 0m;
			line3.JI_CustomsQuantity = 132;
			line3.JI_Tariff = "9999.31.18 24";
			line3.JI_AddInfo = "ADJ=130.00USD";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("Three entry line", 3, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			var entryLine1 = Factory.Load<CusEntryLine>(line1.JI_CL);
			var entryLine2 = Factory.Load<CusEntryLine>(line2.JI_CL);
			var entryLine3 = Factory.Load<CusEntryLine>(line3.JI_CL);

			AssertEquals("Customs value for line1", 1281.27m, entryLine1.CL_CustomsValue, 0.01m);
			AssertEquals("Customs value for line2", 148.94m, entryLine2.CL_CustomsValue, 0.01m);
			AssertEquals("Customs value for line3", 168.37m, entryLine3.CL_CustomsValue, 0.01m);
			AssertEquals("Total customs value", 1598.58m, entryHeader.CustomsValueInAUD.Amount, 0.01m);
		}

		public void TestWineEqualisationTaxOnNonWineTariff()
		{
			DutyData dutyData = new DutyData();
			dutyData.fTariffNumber = "19042120";
			dutyData.fStatCode = "74";
			dutyData.fQuantity = 1;
			dutyData.fUnitOfQuantity = "L";
			dutyData.fPrice = new Money(1000, JobDeclaration.GetLocalCurrency());
			dutyData.fDateOfValuation = new ZDateTime(2003, 11, 14);
			dutyData.fEffectiveDutyDate = dutyData.DateOfValuation;
			DutyCalculator dutyCalculator = new DutyCalculator(dutyData);
			AssertEquals(0m, dutyCalculator.WineEqualisationTax.Amount);
		}

		public void TestGSTAmount()
		{
			const decimal ThePrice = 1000;
			DutyData dutyData = new DutyData();
			dutyData.fTariffNumber = "12345678";
			dutyData.fStatCode = "9012";
			dutyData.fQuantity = 1;
			dutyData.fUnitOfQuantity = "KG";
			dutyData.fPrice = new Money(ThePrice, JobDeclaration.GetLocalCurrency());
			dutyData.fCustomsValue = dutyData.fPrice;
			dutyData.fDateOfValuation = new ZDateTime(2003, 11, 14);
			dutyData.fEffectiveDutyDate = dutyData.DateOfValuation;
			DutyCalculator dutyCalculator = new DutyCalculator(dutyData);
			AssertEquals(ThePrice * DutyCalculator.GSTRate, dutyCalculator.GSTAmount.Amount);
		}

		public void TestCustomsValueWithoutUplift()
		{
			DutyData dutyData = new DutyData();
			dutyData.fPrice = new Money(100, JobDeclaration.GetLocalCurrency());
			dutyData.fCustomsValue = dutyData.fPrice;
			DutyCalculator dC = new DutyCalculator(dutyData);
			AssertEquals(100M, dC.CustomsValue.Amount);
		}

		public void TestCustomsValueWithDollarUplift()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MergeBy = "NON";
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 12);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100m;
			line.JI_CountryOfOrigin = "NZ";
			line.JI_Tariff = "4814.90.00 36";
			line.JI_CustomsQuantity = 223;
			line.AddInfo.AdjustmentAmount_Hidden = 15;
			line.AddInfo.AdjustmentCurrency_Hidden = "AUD";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			var entryLine = Factory.Load<CusEntryLine>(line.JI_CL);
			AssertEquals("Customs value for line1", 115m, entryLine.CL_CustomsValue);
		}

		public void TestCustomsValueWithPercentageUplift()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MergeBy = "NON";
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 12);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 100m;
			line.JI_CountryOfOrigin = "NZ";
			line.JI_Tariff = "4814.90.00 36";
			line.JI_CustomsQuantity = 223;
			line.AddInfo.AdjustmentAmount_Hidden = 7.5m;
			line.AddInfo.AdjustmentDollarPercentage_Hidden = "%";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			var entryLine = Factory.Load<CusEntryLine>(line.JI_CL);
			AssertEquals("Customs value for line1", 107.50m, entryLine.CL_CustomsValue);
		}
	}
}
