using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class InvoiceNormalisationTest : TestCaseWithFactory
	{
		public void TestInvoiceTotalReturnLocalCurrencyIfInvoicesHaveMoreThanOneCurrency()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 15);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.75m, helper.USDCurrency);
			helper.SetExchangeRate(testDec.JE_ExportDate, testDec.JE_ExportDate.AddDays(1), 0.45m, helper.EURCurrency);

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = helper.EURCurrency.RX_Code;//22222.22 AUD
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_InvoiceAmount = 20000m;//26666.67 AUD : FOB:26520 AUD
			invoice2.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;
			BaseJobComInvHeaderCharge oFT = invoice2.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, helper.USDCurrency.RX_Code);
			oFT.J7_IsIncludedInITOT = true;
			BaseJobComInvHeaderCharge oNS = invoice2.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 10m, helper.USDCurrency.RX_Code);
			oNS.J7_IsIncludedInITOT = true;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20000m;

			AssertEquals("Line Price for line1", 10000m, line1.JI_LinePrice);
			AssertEquals("Line Price for line1", helper.EURCurrency.RX_Code, line1.JI_LinePriceMoney.Currency.Code);
			AssertEquals("Line Price for line2", 20000m, line2.JI_LinePrice);
			AssertEquals("Line Price for line2", helper.USDCurrency.RX_Code, line2.JI_LinePriceMoney.Currency.Code);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line2.JI_CL = entryLine.PK;
			testDec.ResumeApportionment();
			AssertEquals("Invoice total Currency: Normalised invoice total should be expressed in AUD.", helper.AUDCurrency.RX_Code, entryHeader.InvoiceTotal.Currency.Code);
			AssertEquals("Invoice total normalised", 48742.22m, entryHeader.InvoiceTotal.Amount);
			AssertEquals("Invoice total Currency: Normalised invoice total should be expressed in AUD.", helper.AUDCurrency.RX_Code, entryLine.Price.Currency.Code);
			AssertEquals("Invoice total normalised", 48742.22m, entryLine.Price.Amount);
		}

		ZTestHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new ZTestHelper(Factory);
		}
	}
}
