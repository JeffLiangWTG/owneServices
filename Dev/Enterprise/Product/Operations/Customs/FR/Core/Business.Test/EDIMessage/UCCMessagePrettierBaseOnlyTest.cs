using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class UCCMessagePrettierBaseOnlyTest : TestCaseWithFactory
	{
		public void TestMessageInterpretationWithInvalidMessageText()
		{
			var message = Factory.New<DeltaIEFREDIMessage>();
			message.EM_MessageText = "Invalid Message Text";
			var messageObject = new IE428MessageDataObject(message);
			Assert("Prerequisite: IE428MessagePrettier is derived from UCCMessagePrettier.", messageObject.Prettier is UCCMessagePrettier<CC428BType>);

			var expectedMessageInterpretation = "Message content is badly formatted.: Error parsing Infinity value. Path '', line 1, position 2.<br><br>Invalid Message Text";
			AssertEquals("Error should show in MessageInterpretation when message content is badly formatted.", expectedMessageInterpretation, messageObject.Prettier.GetMessageInterpretation());

			var expectedMessageReported = "Message content is badly formatted.\r\nError parsing Infinity value. Path '', line 1, position 2.\r\nMessage Text:\r\nInvalid Message Text";
			AssertEquals("Error should be reported when message content is badly formatted.", expectedMessageReported, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
