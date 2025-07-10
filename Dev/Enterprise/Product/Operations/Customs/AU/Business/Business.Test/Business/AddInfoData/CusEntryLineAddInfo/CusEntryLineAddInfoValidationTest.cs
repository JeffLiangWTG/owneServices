namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusEntryLineAddInfoValidationTest : AUAddInfoValidationTest
	{
		public void TestRefundReasonForDeletedLinesPriorToDelivery()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			CMRCusEntryCPDec goodsDeliveredQ = entryHeader.Questions.AddNew();
			goodsDeliveredQ.ON_CPDecNum = 14;
			goodsDeliveredQ.ON_AnswerCode = CMRCusEntryCPDec.Answers.YES;

			const string TestString = "According to declaration questions, goods are not delivered and the refund reason code should be '126A' prior to delivery.";
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.RefundReasonCode = "E";
			AssertNoMessageError(entryLine.RefundReasonCodeInfo, TestString);

			goodsDeliveredQ.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			entryLine.RefundReasonCode = "E";
			AssertHasMessageError(entryLine.RefundReasonCodeInfo, TestString);

			entryLine.RefundReasonCode = "126A";
			AssertNoMessageError(entryLine.RefundReasonCodeInfo, TestString);
		}

		public void TestCheckRefundReasonCodeWhenEntered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			AssertEquals("IsCustomsChargePaid", true, entryHeader.IsCustomsChargePaid);

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 100m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 10m);
			AssertEquals("IsLessDutyAndTax", true, entryLine.IsLessDutyAndTax);
			AssertEquals("IsActive", true, entryLine.IsActive);

			entryLine.RefundReasonCode = "";
			AssertHasWarning(entryLine.RefundReasonCodeInfo, "The entry is paid and the current total duty and tax amount calculated is less than total duty and tax advised in the last lodgement message for this line. Therefore you might need a Refund reason Code for this amendment.");

			entryLine.RefundReasonCode = "126DAA";
			AssertNoWarnings(entryLine.RefundReasonCodeInfo);

			entryHeader.RefundReasonCode = "FA";
			entryLine.RefundReasonCode = "";
			AssertNoWarning(entryLine.RefundReasonCodeInfo, "The entry is paid and the current total duty and tax amount calculated is less than total duty and tax advised in the last lodgement message for this line. Therefore you might need a Refund reason Code for this amendment.");
		}

		public void TestCheckRefundReasonCodeWhenNotEntered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("IsCustomsChargePaid", false, entryHeader.IsCustomsChargePaid);

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.RefundReasonCode = "FA";
			AssertHasMessageError(entryLine.RefundReasonCodeInfo, "This entry is not paid yet and so a refund cannot occur.");

			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 9m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 10m);
			AssertEquals("IsLessDutyAndTax", false, entryLine.IsLessDutyAndTax);
			entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Deleted;
			entryLine.RefundReasonCode = "FA";
			AssertNoWarning(entryLine.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");

			entryLine.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			entryLine.RefundReasonCode = "FA";
			AssertHasWarning(entryLine.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			entryLine.EntryLineAddInfo.Validation.ValidateZA_RRC_Hidden();
			AssertNoWarning(entryLine.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");

			entryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 0m;
			entryLine.RefundReasonCode = "";
			Factory.SuspendValidation();
			entryLine.RefundReasonCode = "FA";
			Factory.ResumeValidation();
			AssertNoWarning(entryLine.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
			entryLine.RunPreSaveValidation();
			AssertHasWarning(entryLine.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
		}
	}
}
