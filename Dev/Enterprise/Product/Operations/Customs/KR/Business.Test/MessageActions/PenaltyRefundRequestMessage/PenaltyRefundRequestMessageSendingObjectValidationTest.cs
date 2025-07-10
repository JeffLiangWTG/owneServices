using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PenaltyRefundRequestMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRefundCause()
		{
			var sendingObject = GetPenaltyRefundRequestMessageSendingObject();

			sendingObject.RefundCause = ZString.Empty;
			sendingObject.Validation.ValidateRefundCause();
			AssertHasMessageErrorContaining(sendingObject.RefundCauseInfo, MandatoryValidation.YouHaveNotEntered);

			sendingObject.RefundCause = "20";
			sendingObject.Validation.ValidateRefundCause();
			AssertHasMessageErrorContaining(sendingObject.RefundCauseInfo, ListValidation.InvalidCodeMessageError);

			sendingObject.RefundCause = "01";
			sendingObject.Validation.ValidateRefundCause();
			AssertNoMessageErrors(sendingObject.RefundCauseInfo);
		}

		public void TestCheckRefundReason()
		{
			var sendingObject = GetPenaltyRefundRequestMessageSendingObject();

			sendingObject.RefundReason = ZString.Empty;
			sendingObject.Validation.ValidateRefundReason();
			AssertNoMessageErrors(sendingObject.RefundReasonInfo);

			sendingObject.RefundReason = "05";
			sendingObject.Validation.ValidateRefundReason();
			AssertHasMessageErrorContaining(sendingObject.RefundReasonInfo, ListValidation.InvalidCodeMessageError);

			sendingObject.RefundReason = "01";
			sendingObject.Validation.ValidateRefundReason();
			AssertNoMessageErrors(sendingObject.RefundReasonInfo);
		}

		public void TestCheckTaxOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.ZZ.NKCodeType.TaxOffice, "Tax Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, "100", "서울청", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var sendingObject = GetPenaltyRefundRequestMessageSendingObject();

			sendingObject.TaxOffice = ZString.Empty;
			AssertNoMessageErrors(sendingObject.TaxOfficeInfo);

			sendingObject.TaxOffice = "100";
			AssertNoMessageErrors(sendingObject.TaxOfficeInfo);

			sendingObject.TaxOffice = "0";
			AssertHasMessageError(sendingObject.TaxOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestObjectsToSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_TaxOffice = "613";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var refundSessionalData = instruction.RefundSessionalDataCollection.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = "2";

			var refundEntryNumber = entry.EntryNumbers.AddNew();
			refundEntryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			refundEntryNumber.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			refundEntryNumber.CE_EntryLineReference = "22222";

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement.B2_StatementNumber = "22222";
			statement.B2_Status = StatementHeaderStatusList.Codes.A;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			statement.B2_GC = declaration.JE_GC;
			statement.B2_ProcessDate = new ZDateTime(2025, 03, 01);

			var statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_EntryNum = entry.EntryNumber;
			statementLine2.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var sendingObject1 = new PenaltyRefundRequestMessageSendingObject(entry, "22222", refundEntryNumber);
			sendingObject1.ShouldSend = true;
			AssertHasError(sendingObject1.ShouldSendInfo, "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.");

			var sendingObject2 = GetPenaltyRefundRequestMessageSendingObject();
			sendingObject2.ShouldSend = true;
			AssertNoError(sendingObject2.ShouldSendInfo, "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.");
		}

		PenaltyRefundRequestMessageSendingObject GetPenaltyRefundRequestMessageSendingObject()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = SharedJobMessageTypeList.Codes.Import;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_RefundType = RefundTypeList.Codes.A;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;

			return new PenaltyRefundRequestMessageSendingObject(entry, "11111", entryNumber);
		}
	}
}
