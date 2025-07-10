namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusEntryHeaderAddInfoValidationTest : AUAddInfoValidationTest
	{
		public void TestValidateWarehouseNumberOfPacksForNature10()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertNoMessageErrors("No errors", entryHeader.WarehouseNumberOfPacksInfo);
		}

		public void TestValidateWarehouseNumberOfPacksForNature20()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_IsPackToBondForLine = true;
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertNoMessageErrors("No errors", entryHeader.WarehouseNumberOfPacksInfo);
		}

		public void TestValidateWarehouseNumberOfPacksForNature30()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "EXW";
			CusEntryHeader entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertHasMessageErrors("Error", entryHeader1.WarehouseNumberOfPacksInfo);
			declaration.JE_TotalNoOfPacks = 150;
			entryHeader1.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertNoMessageErrors("No errors", entryHeader1.WarehouseNumberOfPacksInfo);
			declaration.JE_TotalNoOfPacks = 0;
			entryHeader1.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertHasMessageErrors("No errors", entryHeader1.WarehouseNumberOfPacksInfo);
			declaration.JE_TotalNoOfPacks = 150;

			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertHasMessageErrors("Error", entryHeader2.WarehouseNumberOfPacksInfo);
			entryHeader1.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertHasMessageErrors("Error", entryHeader1.WarehouseNumberOfPacksInfo);

			declaration.CustomsEntryHeaders.RemoveAndDelete(entryHeader2);
			entryHeader1.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertNoMessageErrors("No errors", entryHeader1.WarehouseNumberOfPacksInfo);

			entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertHasMessageErrors("Error", entryHeader2.WarehouseNumberOfPacksInfo);
			entryHeader2.WarehouseNumberOfPacks = 150;
			AssertNoMessageErrors("No errors", entryHeader2.WarehouseNumberOfPacksInfo);

			entryHeader1.AddInfo.Validation.ValidateZA_WarehouseNumberOfPacks_Hidden();
			AssertHasMessageErrors("Error", entryHeader1.WarehouseNumberOfPacksInfo);
			entryHeader1.WarehouseNumberOfPacks = 150;
			AssertNoMessageErrors("No errors", entryHeader1.WarehouseNumberOfPacksInfo);
		}

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
			entryHeader.RefundReasonCode = "E";
			AssertNoMessageError(entryHeader.RefundReasonCodeInfo, TestString);

			goodsDeliveredQ.ON_AnswerCode = CMRCusEntryCPDec.Answers.NO;
			entryHeader.RefundReasonCode = "E";
			AssertHasMessageError(entryHeader.RefundReasonCodeInfo, TestString);

			entryHeader.RefundReasonCode = "126A";
			AssertNoMessageError(entryHeader.RefundReasonCodeInfo, TestString);
		}

		public void TestCheckRefundReasonCodeWhenNotEntered()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			AssertEquals("IsCustomsChargePaid", false, entryHeader.IsCustomsChargePaid);

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryHeader.RefundReasonCode = "FA";
			AssertHasMessageError(entryHeader.RefundReasonCodeInfo, "This entry is not paid yet and so a refund cannot occur.");

			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			entryHeader.AddInfo.Validation.ValidateZA_RRC_Hidden();
			AssertNoMessageError(entryHeader.RefundReasonCodeInfo, "This entry is not paid yet and so a refund cannot occur.");
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = false;
			entryHeader.Charges.AddNew(CusEntryChargeTypeList.Codes.DutyDeferredAmount, 100m);
			entryHeader.AddInfo.Validation.ValidateZA_RRC_Hidden();
			AssertNoMessageError("Duty Deferred", entryHeader.RefundReasonCodeInfo, "This entry is not paid yet and so a refund cannot occur.");

			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 9m);
			entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 10m);
			AssertEquals("IsARefundDue", false, entryHeader.IsARefundDue);
			entryHeader.RefundReasonCode = "FA";
			AssertNoWarning(entryHeader.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
			entryHeader.AddInfo.ZA_IsPAYRECAck_Hidden = true;
			entryHeader.Charges[CusEntryChargeTypeList.Codes.DutyDeferredAmount].C1_ChargeAmount = 0m;
			entryHeader.AddInfo.Validation.ValidateZA_RRC_Hidden();
			AssertHasWarning(entryHeader.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");

			entryHeader.RefundReasonCode = "";
			Factory.SuspendValidation();
			entryHeader.RefundReasonCode = "FA";
			Factory.ResumeValidation();
			AssertNoWarning(entryHeader.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
			entryHeader.RunPreSaveValidation();
			AssertHasWarning(entryHeader.RefundReasonCodeInfo, "The current total duty and tax amount calculated is not less than what you paid last time and so a refund reason is not required.");
		}
	}
}
