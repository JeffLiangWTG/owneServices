using CargoWise.Types;
using Enterprise.Edifact.Manual;
using Enterprise.Messaging.Business;

namespace Enterprise.Messaging.Testing
{
	public class MessageProcessingExceptionTest : NUnit.Framework.TransactionedTestCase
	{
		public void TestContructorParameters()
		{
			var error = InterchangeStringExtensions.ExceptionMessages.CannotDetermineCharacterSet;

			var exception = new MessageProcessingException(error, MessageText, true, false, false);
			AssertEquals("ErrorMessage should crop answer", error + MessageTextCropped, exception.Message);
			AssertEquals("ErrorMessage", MessageText, exception.MessageOrInterchangeText);
			AssertEquals("ShouldSendEmailToUsers", true, exception.ShouldSendEmailToUsers);
			AssertEquals("ShouldSendDeveloperInformation", false, exception.ShouldSendDeveloperInformation);

			exception = new MessageProcessingException(error, MessageText, false, false);
			AssertEquals("ErrorMessage should crop answer", error + MessageTextCropped, exception.Message);
			AssertEquals("ErrorMessage", MessageText, exception.MessageOrInterchangeText);
			AssertEquals("ShouldSendEmailToUsers", false, exception.ShouldSendEmailToUsers);
			AssertEquals("ShouldSendDeveloperInformation", false, exception.ShouldSendDeveloperInformation);

			var exceptionWithAllText = new MessageProcessingException(error, MessageText, true, true, true);
			AssertEquals("ErrorMessage should show all text", error + MessageTextAllFormatted, exceptionWithAllText.Message);
			AssertEquals("ShouldSendEmailToUsers", true, exceptionWithAllText.ShouldSendEmailToUsers);
			AssertEquals("ShouldSendDeveloperInformation", true, exceptionWithAllText.ShouldSendDeveloperInformation);

			exceptionWithAllText = new MessageProcessingException(error, MessageText, false, true);
			AssertEquals("ErrorMessage should now default to cropped answer", error + MessageTextCropped, exceptionWithAllText.Message);
			AssertEquals("MessageOrInterchangeText", MessageText, exception.MessageOrInterchangeText);
			AssertEquals("ShouldSendEmailToUsers", false, exceptionWithAllText.ShouldSendEmailToUsers);
			AssertEquals("ShouldSendDeveloperInformation", true, exceptionWithAllText.ShouldSendDeveloperInformation);
		}

		public void TestContructorWithBlankInterchangeText()
		{
			var error = InterchangeStringExtensions.ExceptionMessages.InterchangeTooShort;
			var exception = new MessageProcessingException(error, "", true, false, true);
			AssertEquals("ErrorMessage should only show the error message", error, exception.Message);
			AssertEquals("MessageOrInterchangeText", "", exception.MessageOrInterchangeText);

			exception = new MessageProcessingException(error, "", true, false);
			AssertEquals("Again ErrorMessage should only show the error message", error, exception.Message);
			AssertEquals("MessageOrInterchangeText", "", exception.MessageOrInterchangeText);
		}

		public void TestContructorWithLongError()
		{
			var error = "This is a really long error message to test the message processing exception does not cut the error message, just the message text.";
			var exceptionCroppedMessage = new MessageProcessingException(error, MessageText, false, true, false);
			AssertEquals("ErrorMessage should only show the error message", error + MessageTextCropped, exceptionCroppedMessage.Message);
			AssertEquals("MessageOrInterchangeText", MessageText, exceptionCroppedMessage.MessageOrInterchangeText);

			var exceptionNoMessage = new MessageProcessingException(error, "", false, true, true);
			AssertEquals("ErrorMessage should only show the error message", error, exceptionNoMessage.Message);
			AssertEquals("MessageOrInterchangeText", "", exceptionNoMessage.MessageOrInterchangeText);

			var exceptionAllMessage = new MessageProcessingException(error, MessageText, false, true, true);
			AssertEquals("ErrorMessage should only show the error message", error + MessageTextAllFormatted, exceptionAllMessage.Message);
			AssertEquals("MessageOrInterchangeText", MessageText, exceptionAllMessage.MessageOrInterchangeText);
		}

		#region Message Strings

		static readonly ZString MessageText = "'UNH+1+APERAK:D:05B:UN:040+STATUS'BGM+963:::ARD+XXXXXXXXE01T        200609139010'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'ERC+CAREJ'FTX+AAO+++PERMIT APPLICATION NOT APPROVED PLS CHECK LICENCE NO LICENCE 1:END OF MESSAGES'UNT+7+1'UNZ+6+5'";
		static readonly ZString MessageTextCropped = "\r\nFirst 100 message or interchange characters: " + MessageText.SubstringSafe(0, 100);
		static readonly ZString MessageTextAllFormatted = "\r\nContents of failed file: \r\n" + MessageText;

		#endregion
	}
}
