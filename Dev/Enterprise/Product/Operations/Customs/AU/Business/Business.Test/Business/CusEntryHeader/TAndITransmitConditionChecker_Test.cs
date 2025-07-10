using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using TestDataSetUp = Enterprise.Customs.AU.Declaration.Business.TAndITransmitConditionChecker.TestDataSetUp;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class TAndITransmitConditionChecker_Test : TestCaseWithFactory
	{
		public void TestNegativeCustomsValueShouldTransmitTAndI()
		{
			TestDataSetUp setter = new TestDataSetUp();
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			setter.InvoiceLine.AddInfo.ZA_ADJ = "-15000AUD";

			setter.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition:Negative CustomsValue", "Y", setter.InvoiceLine.CusEntryLine.Header.AddInfo.ZA_NegativeCVAdjusted_Hidden);

			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals(true, testChecker.ShouldTransmitTAndIForLine);
		}

		public void TestManualOverrideOfCalculateShouldTransmitTAndI()
		{
			TestDataSetUp setter = new TestDataSetUp();
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			testChecker.CalculateShouldTransmitTAndI();
			AssertEquals("Pre-condition", false, testChecker.fShouldTransmitTAndIForHeader);
			AssertEquals("Pre-condition", false, testChecker.fShouldTransmitTAndIForLine);
			setter.EntryHeader.Declaration.AddInfo.ZA_IsManualTILV_Hidden = true;
			testChecker.RefreshCalculation();
			testChecker.CalculateShouldTransmitTAndI();
			AssertEquals("Header should be true", true, testChecker.fShouldTransmitTAndIForHeader);
			AssertEquals("Line should be true", true, testChecker.fShouldTransmitTAndIForLine);
		}

		public void TestDoesNonDutiableFIFTExistInGroup()
		{
			TestDataSetUp setter = new TestDataSetUp();
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);

			AssertEquals("fShouldTransmitTAndIForLine", false, testChecker.fShouldTransmitTAndIForHeader);

			testChecker.DoesNonDutiableFIFTOrOTHExistInGroupOrInvoice = true;
			AssertEquals("If Non-dutiable FIFT exists, Header T&I should be tranmistted", true, testChecker.fShouldTransmitTAndIForHeader);
		}

		public void TestLineTAndIAndOtherFlags()
		{
			TestDataSetUp setter = new TestDataSetUp();
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);

			AssertEquals("fShouldTransmitTAndIForLine", false, testChecker.fShouldTransmitTAndIForLine);
			testChecker.DoOFTOrONSOrNonDutiableFIFTOrOTHExistInMultipleGroupInvoices = true;
			AssertEquals("fShouldTransmitTAndIForLine", true, testChecker.fShouldTransmitTAndIForLine);

			testChecker.fShouldTransmitTAndIForLine = false;
			testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH = true;
			AssertEquals("fShouldTransmitTAndIForLine", true, testChecker.fShouldTransmitTAndIForLine);

			testChecker.fShouldTransmitTAndIForLine = false;
			testChecker.DoLinesHaveTILVInAddInfo = true;
			AssertEquals("fShouldTransmitTAndIForLine", true, testChecker.fShouldTransmitTAndIForLine);

			testChecker.fShouldTransmitTAndIForLine = false;
			testChecker.DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH = true;
			AssertEquals("fShouldTransmitTAndIForLine", true, testChecker.fShouldTransmitTAndIForLine);

			testChecker.fShouldTransmitTAndIForLine = false;
			testChecker.DoChargesExistDistributedByOtherThanValue = true;
			AssertEquals("fShouldTransmitTAndIForLine", true, testChecker.fShouldTransmitTAndIForLine);
		}

		public void TestRefreshCalculation()
		{
			TestDataSetUp setter = new TestDataSetUp();
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);

			testChecker.isCalculationUpToDate = true;
			testChecker.RefreshCalculation();
			AssertEquals("ShouldTransmitTAndICalculated", false, testChecker.isCalculationUpToDate);
		}

		public void TestWhenThereAreMultipleInvoicesWithOFTOrONSOrFIFT()
		{
			TestDataSetUp testSetUp = new TestDataSetUp();

			//Freight
			testSetUp.SetUpSingleEntryForTwoInvoices(Factory);
			testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", true, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);

			testSetUp.SetUpTwoEntriesForTwoInvoices(Factory);
			testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", false, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);

			//Insurance
			testSetUp.SetUpSingleEntryForTwoInvoices(Factory);
			testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", true, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);

			testSetUp.SetUpTwoEntriesForTwoInvoices(Factory);
			testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", false, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);

			//Non-dutiable FIFT
			testSetUp.SetUpSingleEntryForTwoInvoices(Factory);
			BaseJobComInvHeaderCharge fIFT = testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", true, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);

			testSetUp.SetUpTwoEntriesForTwoInvoices(Factory);
			fIFT = testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", false, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);

			//Non-dutiable OTH
			testSetUp.SetUpSingleEntryForTwoInvoices(Factory);
			BaseJobComInvHeaderCharge oTH = testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, "AUD");
			oTH.J7_IsDutiable = false;
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", true, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);

			testSetUp.SetUpTwoEntriesForTwoInvoices(Factory);
			oTH = testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			oTH.J7_IsDutiable = false;
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForLine", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoInvoicesHaveItsOwnOFTOrONSOrNonDutiableFIFT", false, testChecker.DoInvoicesHaveTheirOwnOFTOrONSOrNonDutiableFIFTOrOTH);
		}

		public void TestWhenNonDutiableFIFTInSingleInvoiceOrGroupInvoice()
		{
			TestDataSetUp testSetUp = new TestDataSetUp();

			//Single Invoice
			testSetUp.SetUpSingleEntryForSingleInvoice(Factory);
			BaseJobComInvHeaderCharge fIFT = testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForHeader", true, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("No need for Line T&I", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoesNonDutiableFIFTExistInGroupOrInvoice", true, testChecker.DoesNonDutiableFIFTOrOTHExistInGroupOrInvoice);

			//Single Invoice with Group FIFT
			testSetUp.SetUpSingleEntryForSingleInvoice(Factory);
			fIFT = testSetUp.Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;

			testSetUp.Declaration.ResumeApportionment();
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForHeader", true, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("No need for Line T&I", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoesNonDutiableFIFTExistInGroupOrInvoice", true, testChecker.DoesNonDutiableFIFTOrOTHExistInGroupOrInvoice);

			//Two Invoices with Group FIFT
			testSetUp.SetUpSingleEntryForTwoInvoices(Factory);
			fIFT = testSetUp.Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;
			testSetUp.Declaration.ResumeApportionment();
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForHeader", true, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("No need for Line T&I", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("DoesNonDutiableFIFTExistInGroupOrInvoice", true, testChecker.DoesNonDutiableFIFTOrOTHExistInGroupOrInvoice);

			testSetUp.SetUpSingleEntryForTwoInvoices(Factory);
			testSetUp.Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForHeader", false, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("No need for Line T&I", false, testChecker.ShouldTransmitTAndIForLine);
		}

		public void TestChangesDistributedByOtherThanValue()
		{
			TestDataSetUp testSetUp = new TestDataSetUp();
			//Single Invoice
			testSetUp.SetUpSingleEntryForSingleInvoice(Factory);
			BaseJobComInvHeaderCharge oFT = testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForHeader", true, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("Need to send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("Distributed by other than value", true, testChecker.DoChargesExistDistributedByOtherThanValue);

			//Two entries for two Invoices
			testSetUp.SetUpTwoEntriesForTwoInvoices(Factory);
			oFT = testSetUp.Declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForHeader", true, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("Need to send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("Distributed by other than value", true, testChecker.DoChargesExistDistributedByOtherThanValue);
			//second entry
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader2);
			AssertEquals("ShouldSendTAndIForHeader", true, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("Need to send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("Distributed by other than value", true, testChecker.DoChargesExistDistributedByOtherThanValue);

			//Single Invoice
			testSetUp.SetUpSingleEntryForSingleInvoice(Factory);
			oFT = testSetUp.Invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(testSetUp.EntryHeader);
			AssertEquals("ShouldSendTAndIForHeader", false, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("Need to send Line T&I", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("Distributed by other than value", false, testChecker.DoChargesExistDistributedByOtherThanValue);
		}

		public void TestTILVInAddInfo()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryForTwoInvoices(Factory);
			setter.InvoiceLine2.AddInfo.ZA_TILV = "100AUD";
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Should send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("As there is TILV in AddInfo", true, testChecker.DoLinesHaveTILVInAddInfo);

			setter.SetUpSingleEntryForSingleInvoice(Factory);
			setter.InvoiceLine.AddInfo.ZA_TILV = "100AUD";
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Should send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("As there is TILV in AddInfo", true, testChecker.DoLinesHaveTILVInAddInfo);
		}

		public void TestLineOFTOrONSOrFIFT()
		{
			TestDataSetUp setter = new TestDataSetUp();

			//Freight
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			setter.InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Should send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("As there is Line OFT", true, testChecker.DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH);

			//Insurance
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			setter.InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Should send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("As there is Line ONS", true, testChecker.DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH);

			//Dutiable FIFT
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			BaseJobComInvHeaderCharge fIFT = setter.InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Should not send Line T&I", false, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("As there is Line FIFT", false, testChecker.DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH);

			//Non-Dutiable FIFT
			setter.SetUpSingleEntryForSingleInvoice(Factory);
			fIFT = setter.InvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, "AUD");
			fIFT.J7_IsDutiable = false;
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("Should not send Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
			AssertEquals("As there is Line non-dutiable FIFT", true, testChecker.DoLinesHaveOFTOrONSOrNonDutiableFIFTOrOTH);
		}

		public void TestMultipleGroupInvoices()
		{
			TestDataSetUp setter = new TestDataSetUp();

			setter.SetUpSingleEntryWithTwoInvoicesUnderTwoGroupInvoices(Factory);
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("No need for Header T&I", false, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("No need for Line T&I", false, testChecker.ShouldTransmitTAndIForLine);

			setter.TopGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("No need for Header T&I", false, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("No need for Line T&I", false, testChecker.ShouldTransmitTAndIForLine);

			setter.TopGroup.Charges.RemoveAndDeleteAll();
			setter.SubGroup1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, "AUD");
			testChecker = new TAndITransmitConditionChecker(setter.EntryHeader);
			AssertEquals("No need for Header T&I", true, testChecker.ShouldTransmitTAndIForHeader);
			AssertEquals("No need for Line T&I", true, testChecker.ShouldTransmitTAndIForLine);
		}

		TAndITransmitConditionChecker testChecker;
	}
}
