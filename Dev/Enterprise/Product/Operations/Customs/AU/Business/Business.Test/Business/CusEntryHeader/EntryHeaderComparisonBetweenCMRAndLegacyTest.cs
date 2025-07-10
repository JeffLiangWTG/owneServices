using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EntryHeaderComparisonBetweenCMRAndLegacyTest : TestCaseWithFactory
	{
		public void TestCharges()
		{
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.Discount, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100m, JobDeclaration.LocalCurrencyConstantCode);

			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, JobDeclaration.LocalCurrencyConstantCode);
			invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100m, JobDeclaration.LocalCurrencyConstantCode);
			BaseJobComInvHeaderCharge lCH = invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
			lCH.J7_IsIncludedInITOT = true;

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.ResumeApportionment();
			AssertEquals("Discount", 100m, entryHeader.Discount.Amount);
			AssertEquals("ForeignInlandFreight", 100m, entryHeader.ForeignInlandFreight.Amount);
			AssertEquals("Commission", 100m, entryHeader.Commission.Amount);
			AssertEquals("OTH + ADD", 200m, entryHeader.OtherCharges1.Amount);
			AssertEquals("OTher charges2 : DED", -100m, entryHeader.OtherCharges2.Amount);
			AssertEquals("PackingCost", 100m, entryHeader.PackingCosts.Amount);
			AssertEquals("OverseasFreight", 100m, entryHeader.OverseasFreight.Amount);
			AssertEquals("OverseasInsurance", 100m, entryHeader.OverseasInsurance.Amount);
			AssertEquals("LandingCharges", 100m, entryHeader.LandingCharges.Amount);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.ResumeApportionment();
			AssertEquals("Discount", 0m, entryHeader.Discount.Amount);
			AssertEquals("ForeignInlandFreight", 0m, entryHeader.ForeignInlandFreight.Amount);
			AssertEquals("Commission", 0m, entryHeader.Commission.Amount);
			AssertEquals("OtherCharges1", 0m, entryHeader.OtherCharges1.Amount);
			AssertEquals("OTher charges2", 0m, entryHeader.OtherCharges2.Amount);
			AssertEquals("PackingCost", 0m, entryHeader.PackingCosts.Amount);
			AssertEquals("OverseasFreight", 100m, entryHeader.OverseasFreight.Amount);
			AssertEquals("OverseasInsurance", 100m, entryHeader.OverseasInsurance.Amount);
			AssertEquals("LandingCharges", 0m, entryHeader.LandingCharges.Amount);
		}

		public void TestOtherCharges2ForCMR()
		{
			CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Enterprise.Customs.Common.ChargeDistributeByList.Codes.Value);
			{
				BaseJobDeclaration testDec = Factory.New<BaseJobDeclaration>();
				testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				BaseJobComInvHeaderCharge nonDutyOth = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, JobDeclaration.LocalCurrencyConstantCode);
				nonDutyOth.J7_IsDutiable = false;
				nonDutyOth.J7_IsGSTApplicable = false;
				nonDutyOth.J7_IsIncludedInITOT = true;

				BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
				BaseJobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
				line.JI_LinePrice = 1000m;

				CusEntryHeader entryHeader = (CusEntryHeader)testDec.CustomsEntryHeaders.AddNew();
				CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
				line.JI_CL = entryLine.PK;
				testDec.ResumeApportionment();
				AssertEquals("OTher charges2 : DED", 100m, entryHeader.OtherCharges2.Amount);
			}
		}

		public void TestITOTIncoterm()
		{
			entryHeader.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			AssertEquals("ITOT incoterm", entryHeader.RandomHeader.ITOTIncoTerm, entryHeader.ITOTIncoTerm);

			entryHeader.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader.NormalisationConditionChecker.NotifyNormalisationConditionIsDirty();
			AssertEquals("ITOT incoterm", Core.Constants.IncoTerms.FreeOnBoard, entryHeader.ITOTIncoTerm);
		}

		public void TestCustomsFactorForLegacy()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;

			InvoiceCharge lCH = invoice2.Charges.AddNew();
			lCH.J7_ChargeType = AUChargeCodeList.Codes.LandingCharges;
			lCH.J7_Amount = 200m;
			lCH.J7_RX_NKCurrency = "AUD";
			lCH.J7_IsIncludedInITOT = true;
			testDec.ResumeApportionment();
			AssertEquals("FOB Amount", 9800m, invoice2.JZ_Calc_FOBAmount);
			AssertEquals("Customs factor", 0.99m, entryHeader.CustomsFactor);
		}

		public void TestCustomsFactorForCMR()
		{
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			InvoiceCharge lCH = invoice2.Charges.AddNew();
			lCH.J7_ChargeType = AUChargeCodeList.Codes.LandingCharges;
			lCH.J7_Amount = 200m;
			lCH.J7_RX_NKCurrency = "AUD";
			lCH.J7_IsIncludedInITOT = true;

			entryHeader.NormalisationConditionChecker.NotifyNormalisationConditionIsDirty();
			AssertEquals("Normalised entry header should have 1 as Customs factor", 1m, entryHeader.CustomsFactor);
		}

		public void TestInvoiceTotal()
		{
			InvoiceCharge lCH = invoice2.Charges.AddNew();
			lCH.J7_ChargeType = AUChargeCodeList.Codes.LandingCharges;
			lCH.J7_Amount = 200m;
			lCH.J7_RX_NKCurrency = "AUD";
			lCH.J7_IsIncludedInITOT = true;

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.ResumeApportionment();
			AssertEquals("Invoice total", 20000m, entryHeader.InvoiceTotal.Amount);

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			entryHeader.NormalisationConditionChecker.NotifyNormalisationConditionIsDirty();
			testDec.ResumeApportionment();
			AssertEquals("Invoice Total", 19800m, entryHeader.InvoiceTotal.Amount);
		}

		public void TestCustomsFactorForNormalisedEntries()
		{
			AssertEquals("PreCondition:Should be normalised as there are two invoices with different incoterms", true, entryHeader.ShouldEntryBeNormalised);
			AssertEquals("Customs Factor for this case should be 1", 1m, entryHeader.CustomsFactor);

			entryHeader.ResetTotalsAndCachedValues();

			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;

			testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 500m, "AUD");
			testDec.ResumeApportionment();
			ZDecimal expectedCustomsFactor = (20000 - 500m) / 20000m;
			AssertEquals("Should be normalised", false, entryHeader.ShouldEntryBeNormalised);
			AssertEquals("Customs Factor for this case", expectedCustomsFactor, entryHeader.CustomsFactor);
		}

		public void TestEntryHeaderStatusDescription()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();

			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("Should be clear", CMRImportEntryAdvice.Clear.Description, entryHeader.EntryHeaderStatusDescription);
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("Should be not sent", "Not Sent", entryHeader.EntryHeaderStatusDescription);
			entryHeader.CH_EntryStatus = "!@#";
			AssertEquals("Should be unknown", "Unknown", entryHeader.EntryHeaderStatusDescription);
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			StmALog addLogForWorkComplete = testDec.Logs.AddNew(Events.DeclarationWorkComplete, "work complete");
			StmALog addLogForImpedement = entryHeader.Logs.AddNew(Events.CustomsImpedimentReceived, "quarantine impediment");
			AssertEquals("Should be dec work complete and impedement", "Finalized - Paid:work complete Impediment(s):quarantine impediment", entryHeader.EntryHeaderStatusDescription);
		}

		public void TestCMREntryMayHaveChangedPostLodge()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			Assert("Entry should be clean", !entryHeader.CMREntryMayHaveChangedPostLodge);
			entryHeader.CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			Assert("Entry should be dirty", entryHeader.CMREntryMayHaveChangedPostLodge);
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			Assert("Entry should be clean", !entryHeader.CMREntryMayHaveChangedPostLodge);
			StmALog addLogForAmendmentQueued = testDec.Logs.AddNew(Events.DeclarationAmendmentQueued, "xxx");
			Assert("Entry should be dirty", entryHeader.CMREntryMayHaveChangedPostLodge);
			addLogForAmendmentQueued.Delete();
			Assert("Entry should be clean", !entryHeader.CMREntryMayHaveChangedPostLodge);
			StmALog addLogForAmendmentNotQueued = testDec.Logs.AddNew(Events.DeclarationAmendedPermitApproved, "xxx");
			Assert("Entry should be dirty", entryHeader.CMREntryMayHaveChangedPostLodge);
		}

		JobDeclaration testDec;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine1;
		CusEntryLine entryLine2;

		JobComInvoiceHeader invoice1;
		JobComInvoiceLine line1;
		JobComInvoiceHeader invoice2;
		JobComInvoiceLine line2;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine2 = entryHeader.MergedLines.AddNew();

			invoice1 = testDec.Invoices.AddNew();
			invoice1.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice1.JZ_InvoiceAmount = 10000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10000m;
			line1.JI_CL = entryLine1.PK;

			invoice2 = testDec.Invoices.AddNew();
			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoice2.JZ_InvoiceAmount = 10000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 10000m;
			line2.JI_CL = entryLine2.PK;
			TaxOrFeeTestHelper.SetUp();
		}
	}
}
