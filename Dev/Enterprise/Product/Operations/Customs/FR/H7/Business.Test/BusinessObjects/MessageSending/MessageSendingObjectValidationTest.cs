using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	class MessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestValidateMotivation()
		{
			var header = Factory.New<H7ManifestHeader>();
			var bill = header.Bills.AddNew();
			var messageSendingObject = new MessageSendingObject(bill);

			messageSendingObject.Validation.ValidateAll();
			AssertNoMessageErrors("No message error should when action is not H7M not H7C", messageSendingObject.MotivationInfo);

			messageSendingObject.Action = H7MessageTypeList.Codes.H7Amendment;
			messageSendingObject.Validation.ValidateAll();
			AssertHasMessageError("Should show message error when motivation is empty for H7M message", messageSendingObject.MotivationInfo, "You have not entered a Motivation.");

			messageSendingObject.Motivation = "REC01";
			messageSendingObject.Validation.ValidateAll();
			AssertNoMessageErrors("No message error for valid input", messageSendingObject.MotivationInfo);

			messageSendingObject.Motivation = "";
			messageSendingObject.Validation.ValidateAll();
			AssertHasMessageErrors("Pre-condition: message error for the next test case", messageSendingObject.MotivationInfo);

			messageSendingObject.Action = H7MessageTypeList.Codes.H7Declaration;
			messageSendingObject.Validation.ValidateAll();
			AssertNoMessageErrors("Message error should be removed when the Motivation is set to readonly", messageSendingObject.MotivationInfo);
		}
	}
}
