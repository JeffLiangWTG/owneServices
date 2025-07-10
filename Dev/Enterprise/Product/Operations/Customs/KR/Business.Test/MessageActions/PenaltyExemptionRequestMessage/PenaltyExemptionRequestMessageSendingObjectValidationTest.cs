using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class PenaltyExemptionRequestMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestShouldSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			entry.CH_CEI_Instruction = instruction.PK;

			var testDataSetupHelper = new TestDataSetupHelper(Factory);

			var sessionalData = testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1", "1", DutyTaxCorrectionCodeList.Codes.A, "GOVCBR5FK_PenaltyAmountIs0.xml");
			sessionalData.CSI_Status = CustomsEntryStatusTypeList.Codes.DMS;

			var sendingObject = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData, 1);
			var validation = (PenaltyExemptionRequestMessageSendingObjectValidation)sendingObject.Validation;
			sendingObject.ShouldSend = true;
			sendingObject.Validation.ValidateShouldSend();
			AssertHasErrorContaining(sendingObject.ShouldSendInfo, validation.ErrorMessage_5FENotAccepted);
			AssertNoErrorContaining(sendingObject.ShouldSendInfo, validation.ErrorMessage_TotalPenaltyMustBeGreaterThanZero);

			sessionalData = testDataSetupHelper.SetUpAmendmentSessionalData(entry, "2", "2", DutyTaxCorrectionCodeList.Codes.A, "GOVCBR5FK_PenaltyAmountIs0.xml");
			sessionalData.CSI_Status = CustomsEntryStatusTypeList.Codes.ANT;
			AssertEquals(0m, sessionalData.PenaltyAmountFrom5FK);

			sendingObject = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData, 1);
			sendingObject.ShouldSend = true;
			sendingObject.Validation.ValidateShouldSend();
			AssertNoErrorContaining(sendingObject.ShouldSendInfo, validation.ErrorMessage_5FENotAccepted);
			AssertHasErrorContaining(sendingObject.ShouldSendInfo, validation.ErrorMessage_TotalPenaltyMustBeGreaterThanZero);

			sessionalData = testDataSetupHelper.SetUpAmendmentSessionalData(entry, "3", "3", DutyTaxCorrectionCodeList.Codes.A);
			sessionalData.CSI_Status = CustomsEntryStatusTypeList.Codes.ANT;
			AssertEquals(999m, sessionalData.PenaltyAmountFrom5FK);

			sendingObject = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData, 1);
			sendingObject.ShouldSend = true;
			sendingObject.Validation.ValidateShouldSend();
			AssertNoErrors(sendingObject.ShouldSendInfo);
		}
	}
}
