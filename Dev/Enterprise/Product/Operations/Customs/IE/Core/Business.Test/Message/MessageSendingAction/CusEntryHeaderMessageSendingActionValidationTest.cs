using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class CusEntryHeaderMessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckSendingActionType()
		{
			var cusEntryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var sendingAction = new CusEntryHeaderMessageSendingActionForTesting(cusEntryHeader);
			sendingAction.ShouldSend = false;
			sendingAction.MessageType = "XXX";
			AssertNoErrors("No check against MessageType when not selected to send.", sendingAction.MessageTypeInfo);
			sendingAction.MessageType = ZString.Empty;
			AssertNoErrors("No check against MessageType when not selected to send.", sendingAction.MessageTypeInfo);

			sendingAction.ShouldSend = true;
			sendingAction.Validation.ValidateAll();
			AssertHasErrorContaining("Should have an error when empty on MessageType.", sendingAction.MessageTypeInfo, MandatoryValidation.MustBeEntered);

			sendingAction.MessageType = "XXX";
			AssertHasErrorContaining("Should have an error when invalid value on MessageType.", sendingAction.MessageTypeInfo, ListValidation.InvalidCodeError);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportOriginal;
			AssertNoErrors("Should be clear when valid value on MessageType.", sendingAction.MessageTypeInfo);

			sendingAction.MessageType = AESOutgoingMessageTypeList.Codes.ExportAmendment;
			AssertNoErrors("Should be clear when valid value on MessageType.", sendingAction.MessageTypeInfo);
		}
	}
}
