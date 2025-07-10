using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectValidation))]
	sealed class MessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckAction()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			messageSendingObject.Action = "444";
			AssertHasError("Should show error when invalid Message Type was entered.", messageSendingObject.ActionInfo, "Enter a valid Message Type.");

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			AssertNoErrors("No error for a valid input", messageSendingObject.ActionInfo);
		}

		public void TestCheckSubStyle()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			messageSendingObject.Action = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			messageSendingObject.SubStyle = "S";
			AssertHasError("Should show error when invalid Sub Style was entered.", messageSendingObject.SubStyleInfo, "Enter a valid Additional Declaration Type.");

			messageSendingObject.SubStyle = SubStyleCodeList.Codes.NormalDeclaration;
			AssertNoErrors("No error for a valid input", messageSendingObject.SubStyleInfo);
		}

		public void TestCheckAmendmentInvalidationReason()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new MessageSendingObject(bill);

			var actions = new[]
			{
				AISOutgoingMessageTypeList.Codes.AmendmentRequest,
				AISOutgoingMessageTypeList.Codes.InvalidationRequest
			};

			foreach (var action in actions)
			{
				messageSendingObject.Action = action;
				messageSendingObject.AmendmentInvalidationReason = ZString.Empty;
				AssertHasMessageError($"Should show error for action {action} when Amendment / Invalidation Reason is empty.", messageSendingObject.AmendmentInvalidationReasonInfo, "You have not entered an Amendment / Invalidation Reason.");

				messageSendingObject.AmendmentInvalidationReason = "Test";
				AssertNoMessageErrors(messageSendingObject.AmendmentInvalidationReasonInfo);
			}
		}
	}
}
