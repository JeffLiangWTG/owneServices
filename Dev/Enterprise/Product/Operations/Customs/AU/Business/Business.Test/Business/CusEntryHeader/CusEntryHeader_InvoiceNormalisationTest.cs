using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusEntryHeader_InvoiceNormalisationTest : TestCaseWithFactory
	{
		public void TestFlag_CanInvoicesBeNormalised()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("PreCondition of normalisation met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("PreCondition of normalisation not met as it is legacy", false, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
		}

		public void TestFlag_ShouldInvoicesBeNormalisedForDifferentIncoterms()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;

			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			BaseJobComInvHeaderCharge oFT = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			oFT.J7_IsIncludedInITOT = true;
			BaseJobComInvHeaderCharge oNS = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m);
			oNS.J7_IsIncludedInITOT = true;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;

			AssertEquals("PreCondition: Can be normalised", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Invoices should be normalised", true, entryHeader.ShouldEntryBeNormalised);
		}

		public void TestFlag_ShouldInvoicesBeNormalisedForDifferentIncotermsButSameITOTIncoterm()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;

			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m);
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;//FOB, but with invoice charges

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;

			AssertEquals("PreCondition: Can be normalised", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Invoices should be normalised as one invoice has its own charge", true, entryHeader.ShouldEntryBeNormalised);
		}

		public void TestFlag_ShouldInvoicesBeNormalisedForDifferentCurrency()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_RX_NKInvoice_Currency = "AUD";

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_RX_NKInvoice_Currency = "NZD";

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;

			AssertEquals("PreCondition: Can be normalised", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Invoices should be normalised", true, entryHeader.ShouldEntryBeNormalised);
		}

		public void TestFlag_ShouldInvoicesBeNormalisedForSameIncotermAndCurrency()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;

			AssertEquals("PreCondition: Can be normalised", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Invoices should be normalised", false, entryHeader.ShouldEntryBeNormalised);
		}

		public void TestOverseasFreightAndOverseasInsuranceForNormalisedInvoices()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 1500m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 150m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			line2.JI_LinePrice = 18350m;
			testDec.ResumeApportionment();
			AssertEquals("Invoice is normalised", true, entryHeader.ShouldEntryBeNormalised);
			AssertEquals("OFT is reported as this amount does not affect normalised invoices which reported as FOB invoice", JobDeclaration.LocalCurrencyConstantCode, entryHeader.OverseasFreight.Currency.Code);
			AssertEquals("OFT for normalised entry", 1500m, entryHeader.OverseasFreight.Amount);
			AssertEquals("ONS is reported as this amount does not affect normalised invoices which reported as FOB invoice", JobDeclaration.LocalCurrencyConstantCode, entryHeader.OverseasFreight.Currency.Code);
			AssertEquals("ONS for normalised entry", 150m, entryHeader.OverseasInsurance.Amount);
		}

		JobDeclaration testDec;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			TaxOrFeeTestHelper.SetUp();
		}
	}
}
