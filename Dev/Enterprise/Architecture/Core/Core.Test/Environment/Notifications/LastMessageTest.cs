using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class LastMessageTest : TestCase
	{
		public void TestThrowOnWarningAndError()
		{
			AssertThrowOnWarningAndError("ShowWarning - ", UnitTestUserNotification.Instance.ShowWarning);
			AssertThrowOnWarningAndError("ShowError - ", UnitTestUserNotification.Instance.ShowError);
		}

		void AssertThrowOnWarningAndError(string prefix, Action<string> method)
		{
			UnitTestUserNotification.Instance.ThrowOnWarningAndError = false;
			AssertNoExceptionThrown(prefix + "Should default to false", () => method("Hey there"));

			UnitTestUserNotification.Instance.ThrowOnWarningAndError = true;
			try
			{
				method("Oh no! Impending Doom!");
				Fail(prefix + "Should have thrown an exception");
			}
			catch (InvalidOperationException ex)
			{
				Assert(prefix + "Should contain the message --> " + ex.Message, ex.Message.Contains("Oh no! Impending Doom!"));
			}
		}

		public void TestIsInteractive()
		{
			AssertEquals("Is Interactive", true, Notification.IsInteractive);
		}

		public void TestFromINotification()
		{
			var notification = new Notification(NotificationType.Information, "Information");
			Notification.Show(notification);
			Assert(Notification.LastMessage.WasInformation);

			notification = new Notification(NotificationType.Warning, "Warning");
			Notification.Show(notification);
			Assert(Notification.LastMessage.WasWarning);

			notification = new Notification(NotificationType.Error, "Error");
			Notification.Show(notification);
			Assert(Notification.LastMessage.WasError);
		}

		public void TestClear()
		{
			AssertEquals(null, Notification.LastMessage.Text);
			AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);
			Assert(Notification.LastMessage.WasNone);
			Assert(!Notification.LastMessage.WasQuestion);
			Assert(!Notification.LastMessage.WasInformation);
			Assert(!Notification.LastMessage.WasWarning);
			Assert(!Notification.LastMessage.WasError);
		}

		public void TestClearMessagesAndAnswers()
		{
			AssertEquals("Initial Length (Notification.None)", 1, Notification.PreviousMessages.Length);
			Notification.ShowError("Test Message");
			AssertEquals("Length after ShowError", 2, Notification.PreviousMessages.Length);

			AssertEquals("Initial Answers Count", 0, Notification.NextAnswers.Count);
			Notification.AddAnswer(ZDialogResult.OK);
			AssertEquals("Answers Count", 1, Notification.NextAnswers.Count);

			Notification.ClearMessagesAndAnswers();
			AssertEquals("Length (Notification.None) after Clear", 1, Notification.PreviousMessages.Length);
			AssertEquals("Answers Count after Clear", 0, Notification.NextAnswers.Count);
		}

		public void TestClearMessages()
		{
			AssertEquals("Initial Length (Notification.None)", 1, Notification.PreviousMessages.Length);
			Notification.ShowError("Test Message");
			AssertEquals("Length after ShowError", 2, Notification.PreviousMessages.Length);

			Notification.ClearMessages();
			AssertEquals("Length (Notification.None) after Clear", 1, Notification.PreviousMessages.Length);
		}

		public void TestClearUserResponses()
		{
			AssertEquals("Initial Count", 0, Notification.NextUserResponses.Count);
			Notification.AddUserResponse("My response");
			AssertEquals("Count after Add User Response", 1, Notification.NextUserResponses.Count);

			Notification.ClearUserResponses();
			AssertEquals("Count after Clear", 0, Notification.NextUserResponses.Count);
		}

		public void TestMessageShowQuestion()
		{
			const string Expected = "Some question?";
			Notification.Show(Expected, "Some Caption", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question, ZDialogResult.Yes);
			AssertEquals(Expected, Notification.LastMessage.Text);
			AssertEquals(ZDialogResult.Yes, Notification.LastMessage.Answer);
			Assert(!Notification.LastMessage.WasNone);
			Assert(Notification.LastMessage.WasQuestion);
			Assert(!Notification.LastMessage.WasInformation);
			Assert(!Notification.LastMessage.WasWarning);
			Assert(!Notification.LastMessage.WasError);
		}

		public void TestMessageShowInformation()
		{
			const string Expected = "Some information message";
			Notification.ShowInformation(Expected);
			AssertEquals(Expected, Notification.LastMessage.Text);
			AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);
			Assert(!Notification.LastMessage.WasNone);
			Assert(!Notification.LastMessage.WasQuestion);
			Assert(Notification.LastMessage.WasInformation);
			Assert(!Notification.LastMessage.WasWarning);
			Assert(!Notification.LastMessage.WasError);
		}

		public void TestMessageShowWarning()
		{
			const string Expected = "Some warning message";
			Notification.ShowWarning(Expected);
			AssertEquals(Expected, Notification.LastMessage.Text);
			AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);
			Assert(!Notification.LastMessage.WasNone);
			Assert(!Notification.LastMessage.WasQuestion);
			Assert(!Notification.LastMessage.WasInformation);
			Assert(Notification.LastMessage.WasWarning);
			Assert(!Notification.LastMessage.WasError);
		}

		public void TestMessageShowError()
		{
			const string Expected = "Some error message";
			Notification.ShowError(Expected);
			AssertEquals(Expected, Notification.LastMessage.Text);
			AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);
			Assert(!Notification.LastMessage.WasNone);
			Assert(!Notification.LastMessage.WasQuestion);
			Assert(!Notification.LastMessage.WasInformation);
			Assert(!Notification.LastMessage.WasWarning);
			Assert(Notification.LastMessage.WasError);
		}

		public void TestMessageShowDeveloperErrorAlways()
		{
			try
			{
				const string Expected = "Some developer error message";

				AssertEquals("Should not be any exceptions", 0, ExceptionReporterTestListener.Instance.Count);
				Notification.ShowDeveloperErrorAlways(Expected, "Some Caption");
				AssertEquals("Should be an exception", 1, ExceptionReporterTestListener.Instance.Count);

				AssertEquals(null, Notification.LastMessage.Text);
				AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);
				Assert(Notification.LastMessage.WasNone);
				Assert(!Notification.LastMessage.WasQuestion);
				Assert(!Notification.LastMessage.WasInformation);
				Assert(!Notification.LastMessage.WasWarning);
				Assert(!Notification.LastMessage.WasError);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestMessageShowDeveloperException()
		{
			try
			{
				const string Expected = "Some developer error message";
				Exception innerException = new ArgumentException("The developer exception's inner exception");

				AssertEquals("Should not be any exceptions", 0, ExceptionReporterTestListener.Instance.Count);
				Notification.ShowDeveloperException(Expected, innerException);
				AssertEquals("Should be an exception", 1, ExceptionReporterTestListener.Instance.Count);
				Exception ex = ExceptionReporterTestListener.Instance[0];
				AssertEquals("Message of exception thrown", Expected, ex.Message);
				AssertEquals("Inner exception", innerException, ex.InnerException);

				AssertEquals(null, Notification.LastMessage.Text);
				AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);
				Assert(Notification.LastMessage.WasNone);
				Assert(!Notification.LastMessage.WasQuestion);
				Assert(!Notification.LastMessage.WasInformation);
				Assert(!Notification.LastMessage.WasWarning);
				Assert(!Notification.LastMessage.WasError);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestPreviousMessages()
		{
			AssertEquals(1, Notification.PreviousMessages.Length);
			AssertEquals(Notification.LastMessage, Notification.PreviousMessages[0]);
			AssertEquals(null, Notification.PreviousMessages[0].Text);

			Notification.Show("1");
			AssertEquals(2, Notification.PreviousMessages.Length);
			AssertEquals(Notification.LastMessage, Notification.PreviousMessages[0]);
			AssertEquals("1", Notification.PreviousMessages[0].Text);
			AssertEquals(null, Notification.PreviousMessages[1].Text);

			Notification.Show("2");
			AssertEquals(3, Notification.PreviousMessages.Length);
			AssertEquals(Notification.LastMessage, Notification.PreviousMessages[0]);
			AssertEquals("2", Notification.PreviousMessages[0].Text);
			AssertEquals("1", Notification.PreviousMessages[1].Text);
			AssertEquals(null, Notification.PreviousMessages[2].Text);
		}

		public void TestAddAnswer()
		{
			Notification.AddAnswer(ZDialogResult.Yes);
			Notification.AddAnswer(ZDialogResult.No);
			AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);

			Notification.Show("A message that has no question, so it doesn't use the answer");
			AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);

			Notification.Show("This is a question, so it does use the answer!", "Some Caption", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question);
			AssertEquals(ZDialogResult.Yes, Notification.LastMessage.Answer);

			Notification.Show("This is another question, which will use the 2nd answer", "Some Caption", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question);
			AssertEquals(ZDialogResult.No, Notification.LastMessage.Answer);

			Notification.Show("No answer was supplied for this question", "Some Caption", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Question);
			AssertEquals(ZDialogResult.None, Notification.LastMessage.Answer);
		}

		public void TestShowConfirmationWithNotifications_WhenOKQueuedButErrorsExist_ShouldReportError()
		{
			var descriptor = new ConfirmationDialogDescriptor("", "", new ConfirmationNotification(NotificationTypes.Error, "Ponies 4 lyf"));

			Notification.AddAnswer(ZDialogResult.OK);
			Notification.ShowConfirmationWithNotifications(descriptor);

			AssertEquals(descriptor, Notification.PreviousMessages[0].ConfirmationDialogDescriptor);
			AssertMultilineASCIIEquals("", @"Success answer is not possible because there are notification errors.
Ponies 4 lyf", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestShowConfirmationWithNotifications_WhenOKQueuedAndWarningsExist()
		{
			Notification.AddAnswer(ZDialogResult.OK);

			var descriptor = new ConfirmationDialogDescriptor("", "", new ConfirmationNotification(NotificationTypes.Warning, "Ponies 4 lyf"));
			var result = Notification.ShowConfirmationWithNotifications(descriptor);

			AssertEquals(ZDialogResult.OK, result);
			AssertEquals(descriptor, Notification.PreviousMessages[0].ConfirmationDialogDescriptor);
		}

		public void TestShowConfirmationWithNotifications_WhenNoAnswerQueuedAndWarningsExist()
		{
			var descriptor = new ConfirmationDialogDescriptor("", "", new ConfirmationNotification(NotificationTypes.Warning, "Ponies 4 lyf"));
			var result = Notification.ShowConfirmationWithNotifications(descriptor);

			AssertEquals(ZDialogResult.None, result);
			AssertEquals(descriptor, Notification.PreviousMessages[0].ConfirmationDialogDescriptor);
		}

		protected override void SetUp()
		{
			Notification = new UnitTestUserNotification();
		}

		UnitTestUserNotification Notification;

		public void TestFindingOfAPreviousMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals(
							"Looking for the [I like lager] popup should be unsuccessful as we have not popped anything yet",
							Notification.PreviousMessages.ContainsMessageWithThisText("I like lager"),
							false
						);

			Notification.Show("I like lager");
			Notification.Show("I like ale");

			AssertEquals(
							"Looking for the [I like ale] popup should be successful",
							Notification.PreviousMessages.ContainsMessageWithThisText("I like ale"),
							true
						);

			AssertEquals(
							"Looking for the [I like lager] popup should be successful",
							Notification.PreviousMessages.ContainsMessageWithThisText("I like lager"),
							true
						);

			AssertEquals(
							"Looking for the [I like white white spritzers] popup should be unsuccessful",
							Notification.PreviousMessages.ContainsMessageWithThisText("I like white white spritzers"),
							false
						);
		}

		public void TestCaptionIsExposed()
		{
			Globals.Message.Show("I have a dream....", "Martin Luther King", ZMessageBoxButtons.OK, ZMessageBoxIcon.Hand);
			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("I have a dream....", lastMessage.Text);
			AssertEquals("Martin Luther King", lastMessage.Caption);
		}
	}
}
