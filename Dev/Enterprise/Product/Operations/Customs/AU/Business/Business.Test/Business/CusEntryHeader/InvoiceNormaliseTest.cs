using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class InvoiceNormaliseTest : TestCaseWithFactory
	{
		public void TestShouldNotAccessJZ_IncoTermOnADeletedInvoiceHeader()
		{
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();

			var invoice1 = testDec.Invoices.AddNew();
			var invoice2 = testDec.Invoices.AddNew();
			var invoiceLine11 = invoice1.InvoiceLines.AddNew();
			var invoiceLine21 = invoice2.InvoiceLines.AddNew();

			invoiceLine11.JI_CL = entryLine1.PK;
			invoiceLine21.JI_CL = entryLine1.PK;

			var group1 = testDec.JobComInvoiceGroupHeaders[0];
			var invoice1Group1 = group1.JobComInvoiceHeaders.AddNew();
			var line1 = invoice1Group1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			var checker = new NormalisationConditionChecker(entry1);

			AssertEquals("pre-condition, cache invoiceheaders of entry", 3, entry1.InvoiceHeaders.Length);

			invoice1.Delete();

			AssertNoExceptionThrown(() =>
			{
				AssertEquals(false, checker.HaveDifferentIncoTermOrZeroAmountMandatoryCharges);
			});
		}

		public void TestShouldNotAccessJZ_JZ_GroupInvoiceFKOnADeletedInvoiceHeader()
		{
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry1.MergedLines.AddNew();

			var invoice1 = testDec.Invoices.AddNew();
			var invoice2 = testDec.Invoices.AddNew();
			var invoiceLine11 = invoice1.InvoiceLines.AddNew();
			var invoiceLine21 = invoice2.InvoiceLines.AddNew();

			invoiceLine11.JI_CL = entryLine1.PK;
			invoiceLine21.JI_CL = entryLine1.PK;

			var group1 = testDec.JobComInvoiceGroupHeaders[0];
			var invoice1Group1 = group1.JobComInvoiceHeaders.AddNew();
			var line1 = invoice1Group1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			var checker = new NormalisationConditionChecker(entry1);

			AssertEquals("pre-condition, cache invoiceheaders of entry", 3, entry1.InvoiceHeaders.Length);

			invoice1.Delete();

			AssertNoExceptionThrown(() =>
			{
				AssertEquals(false, checker.HaveInvoiceLevelChargeOrSubGroupCharges);
				AssertEquals(false, checker.HaveChargesDistributedByOtherThanValue);
			});
		}

		public void TestHaveChargesDistributedByOtherThanValue()
		{
			CusEntryHeader entry1 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			JobComInvoiceGroupHeader group1 = testDec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceGroupHeader group2 = group1.JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceGroupHeader group3 = group2.JobComInvoiceGroupHeaders.AddNew();

			JobComInvoiceHeader invoice1_Group1 = group1.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = invoice1_Group1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			JobComInvoiceHeader invoice2_Group2 = group2.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line2 = invoice2_Group2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;

			JobComInvoiceHeader invoice3_Group3 = group3.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line3 = invoice3_Group3.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine2.PK;

			JobComInvoiceHeader invoice4_Group3 = group3.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line4 = invoice4_Group3.JobComInvoiceLines.AddNew();
			line4.JI_CL = entryLine2.PK;

			BaseJobComInvHeaderCharge group2Charge = group2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			group2Charge.J7_DistributeBy = Enterprise.Customs.Common.ChargeDistributeByList.Codes.Volume;

			NormalisationConditionChecker checker1 = new NormalisationConditionChecker(entry1);
			NormalisationConditionChecker checker2 = new NormalisationConditionChecker(entry2);

			AssertEquals("HaveChargesDistributedByOtherThanValue for the first Entry", false, checker1.HaveChargesDistributedByOtherThanValue);
			AssertEquals("Should Normalise invoices for the 1st entry", false, checker1.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);

			AssertEquals("HaveChargesDistributedByOtherThanValue for the second Entry", true, checker2.HaveChargesDistributedByOtherThanValue);
			AssertEquals("Should Normalise invoices for the 2nd entry", true, checker2.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
		}

		public void TestHaveInvoiceLevelChargeOrSubGroupChargesWithSubGroupInvoiceRecursively()
		{
			CusEntryHeader entry1 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();

			CusEntryHeader entry2 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			JobComInvoiceGroupHeader group1 = testDec.JobComInvoiceGroupHeaders[0];

			JobComInvoiceGroupHeader group2 = group1.JobComInvoiceGroupHeaders.AddNew();
			JobComInvoiceGroupHeader group3 = group1.JobComInvoiceGroupHeaders.AddNew();
			group3.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);

			JobComInvoiceHeader invoice1_Group1 = group1.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line1 = invoice1_Group1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			JobComInvoiceHeader invoice2_Group2 = group2.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line2 = invoice2_Group2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;

			JobComInvoiceHeader invoice3_Group3 = group3.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line3 = invoice3_Group3.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine2.PK;

			JobComInvoiceHeader invoice4_Group3 = group3.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine line4 = invoice4_Group3.JobComInvoiceLines.AddNew();
			line4.JI_CL = entryLine2.PK;

			NormalisationConditionChecker checker1 = new NormalisationConditionChecker(entry1);
			AssertEquals("HaveInvoiceLevelChargeOrSubGroupCharges for entry 1", false, checker1.HaveInvoiceLevelChargeOrSubGroupCharges);
			AssertEquals("ShouldNormaliseInvoices", false, checker1.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);

			NormalisationConditionChecker checker2 = new NormalisationConditionChecker(entry2);
			AssertEquals("HaveInvoiceLevelChargeOrSubGroupCharges for entry 2", true, checker2.HaveInvoiceLevelChargeOrSubGroupCharges);
			AssertEquals("ShouldNormaliseInvoices", true, checker2.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
		}

		public void TestJobWithTopGroupHeaderOnlyDoesNotGetNormalised()
		{
			JobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");

			JobComInvoiceHeader invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			NormalisationConditionChecker checker = new NormalisationConditionChecker(entryHeader);
			AssertEquals("ShouldNormaliseInvoices as there is only one group header", false, checker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
		}

		public void TestWithTwoInvoicesWithTheFirstInvoiceHavingMissingMandatoryCharges()
		{
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 15);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100m, "AUD");
			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 20000m;

			AssertEquals("PreCondition:There is no OFT", 0, invoice.GroupCharges.GetCharge("OFT").Length);

			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			line1.JI_CL = entryLine.PK;

			NormalisationConditionChecker checker = new NormalisationConditionChecker(entryHeader);
			AssertEquals("Has missing mandatory charges", true, checker.HaveDifferentIncoTermOrZeroAmountMandatoryCharges);
			AssertEquals("Should be normalised", true, checker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
		}

		public void TestWithTwoInvoicesWithTheFirstInvoiceHavingInvoiceLevelCharge()
		{
			testDec.JE_ExportDate = new ZDateTime(2005, 3, 15);

			JobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100m, "AUD");

			JobComInvoiceHeader invoice = subGroup.JobComInvoiceHeaders.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 20000m;

			JobComInvoiceHeader invoice1 = topGroup.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_JZ_GroupInvoiceFK = topGroup.PK;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			line.JI_CL = entryLine.PK;
			line1.JI_CL = entryLine.PK;

			NormalisationConditionChecker checker = new NormalisationConditionChecker(entryHeader);
			AssertEquals("Has missing mandatory charges", true, checker.HaveInvoiceLevelChargeOrSubGroupCharges);
			AssertEquals("Should be normalised", true, checker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
		}

		public void TestNotifyNormalisationConditionIsDirty()
		{
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			NormalisationConditionChecker checker = new NormalisationConditionChecker(entryHeader);
			checker.needToRecalculate_DifferentCurrencies = false;
			checker.needToRecalculate_DifferentIncoTermOrZeroAmountMandatoryCharges = false;
			checker.needToRecalculate_InvoiceLevelChargeOrSubGroupCharges = false;

			AssertEquals("Precondition Different currency", false, checker.needToRecalculate_DifferentCurrencies);
			AssertEquals("Precondition Different IncoTermOrZeroAmountMandatoryCharges", false, checker.needToRecalculate_DifferentIncoTermOrZeroAmountMandatoryCharges);
			AssertEquals("Precondition Different Charges", false, checker.needToRecalculate_InvoiceLevelChargeOrSubGroupCharges);

			checker.NotifyNormalisationConditionIsDirty();
			AssertEquals("Different currency", true, checker.needToRecalculate_DifferentCurrencies);
			AssertEquals("Different IncoTermOrZeroAmountMandatoryCharges", true, checker.needToRecalculate_DifferentIncoTermOrZeroAmountMandatoryCharges);
			AssertEquals("Different Charges", true, checker.needToRecalculate_InvoiceLevelChargeOrSubGroupCharges);
			AssertEquals("Line Level Charges", true, checker.needToRecalculate_HaveInvoiceLineCharges);
		}

		public void TestWithOneInvoiceHavingCharges()
		{
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 850m;

			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 40m, "AUD");

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("No need to normalise as it is one invoice", false, entryHeader.NormalisationConditionChecker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
			AssertEquals("No need to normalise", false, entryHeader.NormalisationConditionChecker.ShouldEntryBeNormalised);
		}

		public void TestWithOneInvoiceWithLineLevelCharges()
		{
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 850m;
			line1.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 150m, "AUD");

			CusEntryHeader entry1 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entry1.MergedLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			AssertEquals("There are line level charges", true, entry1.NormalisationConditionChecker.ShouldInvoiceLinesBeNormalised);
			AssertEquals("need to normalise", true, entry1.NormalisationConditionChecker.ShouldEntryBeNormalised);

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 850m;

			CusEntryHeader entry2 = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();
			line2.JI_CL = entryLine2.PK;

			AssertEquals("There are no line level charges", false, entry2.NormalisationConditionChecker.ShouldInvoiceLinesBeNormalised);
			AssertEquals("no need to normalise", false, entry2.NormalisationConditionChecker.ShouldEntryBeNormalised);
		}

		public void TestInvoicesWithInvoiceCharges()
		{
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100m, "AUD");

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Should be normalsed", true, entryHeader.NormalisationConditionChecker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
			AssertEquals("Invoice charges", true, entryHeader.NormalisationConditionChecker.HaveInvoiceLevelChargeOrSubGroupCharges);
		}

		public void TestInvoicesWithoutInvoiceCharges()
		{
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Invoice charges", false, entryHeader.NormalisationConditionChecker.HaveInvoiceLevelChargeOrSubGroupCharges);
		}

		public void TestInvoicesWithDifferentInvoiceCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = helper.AUDCurrency.RX_Code;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Should be normalsed", true, entryHeader.NormalisationConditionChecker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
			AssertEquals("Currency", true, entryHeader.NormalisationConditionChecker.HaveDifferentCurrency);
		}

		public void TestHaveDifferntCurrency()
		{
			ZTestHelper helper = new ZTestHelper(Factory);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			testDec.Invoices[0].Delete();
			AssertNoExceptionThrown("Should not be exception", () => forceToCallHaveDifferentCurrency(entryHeader));
		}

		void forceToCallHaveDifferentCurrency(CusEntryHeader entryHeader)
		{
			AssertEquals("Currency", false, entryHeader.NormalisationConditionChecker.HaveDifferentCurrency);
		}

		public void TestInvoicesWithSameCurrencies()
		{
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Currency", false, entryHeader.NormalisationConditionChecker.HaveDifferentCurrency);
		}

		public void TestInvoicesWithDifferentIncoTermGetNormalised()
		{
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = helper.USDCurrency.RX_Code;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;//ITOT is CIF
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = helper.AUDCurrency.RX_Code;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Have different incoterm", true, entryHeader.NormalisationConditionChecker.HaveDifferentIncoTermOrZeroAmountMandatoryCharges);
			AssertEquals("Should be normalsed", true, entryHeader.NormalisationConditionChecker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
		}

		public void TestInvoicesWithSameITOTIncoTerm()
		{
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;//ITOT is FOB
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, "AUD");

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Have same ITOT incoterm", false, entryHeader.NormalisationConditionChecker.HaveDifferentIncoTermOrZeroAmountMandatoryCharges);
		}

		public void TestInvoicesWithMissingMandatoryChargesGetNormalised()
		{
			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;//OFT is mandatory
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("Missing mandatory charges", true, entryHeader.NormalisationConditionChecker.HaveDifferentIncoTermOrZeroAmountMandatoryCharges);
			AssertEquals("Should be normalsed", true, entryHeader.NormalisationConditionChecker.ShouldInvoicesBeNormalisedIfThereAreMoreThanOneInvoice);
		}

		public void TestInvoicesWithNoMissingMandatoryCharges()
		{
			testDec.AutoCreateChargesBasedOnIncoTerm = false;
			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 1000m, JobDeclaration.LocalCurrencyConstantCode);

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			testDec.ResumeApportionment();
			invoice2.GroupCharges[0].J7_IsIncludedInITOT = false;//ITOT incoterm becomes FOB

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("No Missing mandatory charges", false, entryHeader.NormalisationConditionChecker.HaveDifferentIncoTermOrZeroAmountMandatoryCharges);
		}

		public void TestInvoicesWithSubGroupCharges()
		{
			JobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.Charges.AddNew("ADD", 100m, "AUD");

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_JZ_GroupInvoiceFK = subGroup.PK;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("HaveSubGroupCharges", true, entryHeader.NormalisationConditionChecker.HaveInvoiceLevelChargeOrSubGroupCharges);
		}

		public void TestInvoicesWithoutSubGroupCharges()
		{
			JobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_JZ_GroupInvoiceFK = subGroup.PK;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("HaveSubGroupCharges", false, entryHeader.NormalisationConditionChecker.HaveInvoiceLevelChargeOrSubGroupCharges);
		}

		public void TestInvoicesWithSubGroupChargesButWithoutAnyInvoices()
		{
			JobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			JobComInvoiceGroupHeader subGroup = topGroup.JobComInvoiceGroupHeaders.AddNew();
			subGroup.Charges.AddNew("ADD", 100m, "AUD");

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 10000m;

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];

			AssertEquals("Precondition met", true, entryHeader.NormalisationConditionChecker.IsPreConditionInvoicesNormalisationMet);
			AssertEquals("HaveSubGroupCharges", false, entryHeader.NormalisationConditionChecker.HaveInvoiceLevelChargeOrSubGroupCharges);
		}

		public void TestNormalisedCharges()
		{
			JobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 850m;

			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 40m, "AUD");
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 20m, "AUD");
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 40m, "AUD");
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 50m, "AUD");

			JobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, "AUD");
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 200m, "AUD");

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredAtPlace;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 19400m;

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("One entry header", 1, testDec.CustomsEntryHeaders.Count);

			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders[0];
			AssertEquals("The invoices should have been normalised", true, entryHeader.ShouldEntryBeNormalised);
			AssertEquals("Normalised OTH", true, entryHeader.OtherCharges1.IsEmpty);
			AssertEquals("Normalised COM", true, entryHeader.Commission.IsEmpty);
			AssertEquals("Normalised FIFT", true, entryHeader.ForeignInlandFreight.IsEmpty);
			AssertEquals("Normalised PAC", true, entryHeader.PackingCosts.IsEmpty);
			AssertEquals("Normalised LandingCharges", true, entryHeader.LandingCharges.IsEmpty);
			AssertEquals("Normalised OtherCharge2", true, entryHeader.OtherCharges2.IsEmpty);
		}

		#region Implemetation

		ZTestHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new ZTestHelper(Factory);
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}
		JobDeclaration testDec;

		#endregion
	}
}
