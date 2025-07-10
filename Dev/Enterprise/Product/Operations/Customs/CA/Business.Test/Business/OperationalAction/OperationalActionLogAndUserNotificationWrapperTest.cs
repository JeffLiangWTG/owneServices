using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.MessageManagers.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.CA.Business.OperationalAction.Testing
{
	sealed class OperationalActionLogAndUserNotificationWrapperTest : TestCaseWithFactory
	{
		[NUnit.Framework.TestDate(2015, 05, 01)]
		public void TestOperationalActionLogAndUserNotificationWrapper()
		{
			var testUserNotification = new TestUserNotification();
			var testOperationalLog = new DummyOperationalActionSectionLog();

			AssertEquals(0, testOperationalLog.messages.Count);
			var testTarget = new OperationalActionLogAndUserNotificationWrapper(testUserNotification, testOperationalLog, false, false, false);

			CombineAssertions("TestShowWarning", () =>
			{
				testTarget.ShowWarning("TestWarningMessage", "TestWarningCaption");
				AssertEquals("TestWarningMessage", testUserNotification.LastMessage);
				AssertEquals(1, testOperationalLog.messages.Count);
				AssertEquals("WARNING: TestWarningMessage", testOperationalLog.messages.Last());
			});

			CombineAssertions("TestShowInformation", () =>
			{
				testTarget.ShowInformation("TestInformationMessage", "TestInformationCaption");
				AssertEquals("TestInformationMessage", testUserNotification.LastMessage);
				AssertEquals(2, testOperationalLog.messages.Count);
				AssertEquals("INFO: TestInformationMessage", testOperationalLog.messages.Last());
			});

			CombineAssertions("TestShowError", () =>
			{
				testTarget.ShowError("TestErrorMessage", "TestErrorCaption");
				AssertEquals("TestErrorMessage", testUserNotification.LastMessage);
				AssertEquals(4, testOperationalLog.messages.Count);
				AssertEquals("ERROR: [Encounter Error when processing]", testOperationalLog.messages[2]);
				AssertEquals("ERROR: TestErrorMessage", testOperationalLog.messages[3]);
			});

			CombineAssertions("TestShowQuestion", () =>
			{
				testUserNotification.NextTextAnswer = "TestAnswer";
				testTarget.ShowQuestion("TestQuestionMessage", "TestQuestionCaption", 1, null);
				AssertEquals("TestQuestionMessage", testUserNotification.LastMessage);
				AssertEquals(6, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Answer]:TestQuestionMessage", testOperationalLog.messages[4]);
				AssertEquals("INFO: [User's Answer]:TestAnswer", testOperationalLog.messages[5]);
			});

			CombineAssertions("TestShowConfirmation", () =>
			{
				testUserNotification.NextAnswer = false;
				testTarget.ShowConfirmation("TestConfirmationMessage_1", "TestConfirmationCaption_1");
				AssertEquals("TestConfirmationMessage_1", testUserNotification.LastMessage);
				AssertEquals(8, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:TestConfirmationMessage_1", testOperationalLog.messages[6]);
				AssertEquals("INFO: [User's Answer]:No", testOperationalLog.messages[7]);
			});

			CombineAssertions("TestShowConfirmation", () =>
			{
				testUserNotification.NextAnswer = true;
				testTarget.ShowConfirmation("TestConfirmationMessage_2", "TestConfirmationCaption_2");
				AssertEquals("TestConfirmationMessage_2", testUserNotification.LastMessage);
				AssertEquals(10, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:TestConfirmationMessage_2", testOperationalLog.messages[8]);
				AssertEquals("INFO: [User's Answer]:Yes", testOperationalLog.messages[9]);
			});

			CombineAssertions("TestShowMessageInstructionForm", () =>
			{
				var testMessageInstructionUserNotification = new TestMessageInstructionUserNotification();
				testMessageInstructionUserNotification.NextAnswer = false;
				testTarget = new OperationalActionLogAndUserNotificationWrapper(testMessageInstructionUserNotification, testOperationalLog, false, false, false);
				var testInstruction = new MessageInstruction(Factory, false, "ValiMessage", "NotificationMessage");
				Assert(!testMessageInstructionUserNotification.HasShowMessageInstructionFormBeenCalled);
				testTarget.ShowMessageInstructionForm(testInstruction);
				Assert(testMessageInstructionUserNotification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("ValiMessage", testMessageInstructionUserNotification.ValidationErrorsMessage);
				AssertEquals("NotificationMessage", testMessageInstructionUserNotification.AdditionalWarningsMessage);
				AssertEquals(13, testOperationalLog.messages.Count);

				testMessageInstructionUserNotification.Reset();
				testMessageInstructionUserNotification.NextAnswer = true;
				testTarget = new OperationalActionLogAndUserNotificationWrapper(testMessageInstructionUserNotification, testOperationalLog, false, false, false);
				Assert(!testMessageInstructionUserNotification.HasShowMessageInstructionFormBeenCalled);
				testTarget.ShowMessageInstructionForm(testInstruction);
				Assert(testMessageInstructionUserNotification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("ValiMessage", testMessageInstructionUserNotification.ValidationErrorsMessage);
				AssertEquals("NotificationMessage", testMessageInstructionUserNotification.AdditionalWarningsMessage);
				AssertEquals(16, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:ValiMessage", testOperationalLog.messages[13]);
				AssertEquals("INFO: [Awaiting User's Confirmation]:NotificationMessage", testOperationalLog.messages[14]);
				AssertEquals("INFO: [User's Answer]:Continue sending despite of rationality warnings.", testOperationalLog.messages[15]);

				testMessageInstructionUserNotification.Reset();
				testTarget = new OperationalActionLogAndUserNotificationWrapper(testMessageInstructionUserNotification, testOperationalLog, true, false, false);
				Assert(!testMessageInstructionUserNotification.HasShowMessageInstructionFormBeenCalled);
				testTarget.ShowMessageInstructionForm(testInstruction);
				Assert(!testMessageInstructionUserNotification.HasShowMessageInstructionFormBeenCalled);
				AssertEquals("", testMessageInstructionUserNotification.ValidationErrorsMessage);
				AssertEquals("", testMessageInstructionUserNotification.AdditionalWarningsMessage);
				AssertEquals(19, testOperationalLog.messages.Count);
				AssertEquals("INFO: [Awaiting User's Confirmation]:ValiMessage", testOperationalLog.messages[16]);
				AssertEquals("INFO: [Awaiting User's Confirmation]:NotificationMessage", testOperationalLog.messages[17]);
				AssertEquals("INFO: [Automatic Answer]:Continue sending despite of rationality warnings.", testOperationalLog.messages[18]);
			});
		}
	}
}
