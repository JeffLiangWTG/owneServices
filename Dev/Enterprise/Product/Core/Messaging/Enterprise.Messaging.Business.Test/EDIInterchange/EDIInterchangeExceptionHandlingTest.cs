using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.Manual;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	public class EDIInterchangeExceptionHandlingTest : TestCaseWithFactory
	{
		#region Setting Character Set From Interchange String

		public void TestSetCharacterSetFromInterchangeString()
		{
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.CannotDetermineCharacterSet, "hehe", true, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.InterchangeTooShort, "UNA", true, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.InterchangeTooShort, "UNB", true, false, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.UnknownCharacterSetForUNB + "UNOD", "UNB+UNOD+AAA", true, false, false);

			var longText = "The text sent to the error reporter must be over 100 characters to check that all the characters in this Inttra message will be sent.";
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.CannotDetermineCharacterSet, longText, true, false, true);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.CannotDetermineCharacterSet, longText, true, false, false);
		}

		#endregion

		#region Creating New Interchange From String

		public void TestCreateNewInterchangeFromStringAndExceptionHandling()
		{
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.NoUNBSegment, "UNA:+.? '", true, false, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.NoUNZSegment, "UNA:+.? 'UNB", true, false, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.MultipleUNBSegments, "UNA:+.? 'UNB'UNB'", true, false, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.MultipleInterchangesDetected, "UNA:+.? 'UNB'UNZ'UNA:+.? '", true, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.MultipleUNZSegments, "UNA:+.? 'UNB'UNZ'UNZ'", true, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.MalformedInterchange, "UNA:+.? 'UNB'UNZ'blah", true, false);

			ZString interchangeString = "UNA:+.? 'UNB+UNOA:2+hehehe:ZZZ+hehehe:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407'";
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.MalformedInterchange, interchangeString + "lala", true, false, true);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.MalformedInterchange, interchangeString + "lala", true, false, false);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.CannotDetermineCharacterSet, interchangeString.Substring(50), true, false, true);
			AssertExceptionCreated(InterchangeStringExtensions.ExceptionMessages.CannotDetermineCharacterSet, interchangeString.Substring(50), true, false, false);

			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString);
			AssertEquals("Default application code should be blank", "", interchange.EI_ApplicationCode);
		}

		[ExpectNoExceptions]
		public void TestEmptyCharactersAreIgnoredAtEndOfInterchange()
		{
			var interchangeStringWithEmptyCharactersAtEnd = "UNA:+.? 'UNB+UNOA:2+hehehe:ZZZ+hehehe:ZZZ+030509:1203+407'UNH+1320+CUSRES:D:96B:UN+SSYD00001093'BGM+963+00000000'GIS+841:120:143'ERP+001::530'ERC+148::143'ERP+001::515'ERC+491::143'UNT+8+1320'UNZ+1+407' \r\n";
			EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeStringWithEmptyCharactersAtEnd);
		}

		#endregion

		#region EDI Interchange Exception Assertions

		void AssertExceptionCreated(string exceptionMessage, string interchangeText, bool expectedShouldSendEmailToUsers,
			bool expectedShouldSendDevelopException, bool shouldReportWholeErrorMessage = false)
		{
			MessageProcessingException exception = null;

			try
			{
				EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeText, shouldReportWholeErrorMessage);
			}
			catch (MessageProcessingException e)
			{
				exception = e;
			}
			AssertNotNull("an exception should have thrown", exception);

			ZString interchangeTextToCrop = interchangeText;
			ZString failedMessageText = shouldReportWholeErrorMessage
				? exceptionMessage + "\r\nContents of failed file: \r\n" + interchangeText
				: exceptionMessage + "\r\nFirst 100 message or interchange characters: " + interchangeTextToCrop.SubstringSafe(0, 100);

			AssertEquals("Exception Message: ", failedMessageText, exception.Message);
			AssertEquals("Text", interchangeText, exception.MessageOrInterchangeText);
			AssertEquals("ShouldSendEmailToUsers", expectedShouldSendEmailToUsers, exception.ShouldSendEmailToUsers);
			AssertEquals("ShouldSendDeveloperInformation", expectedShouldSendDevelopException, exception.ShouldSendDeveloperInformation);
		}

		#endregion
	}
}
