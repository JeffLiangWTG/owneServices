using System.IO;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC574MessageInterpreter))]
	class CC574MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC574MessageInterpreter, CC574CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE574;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Amendment Acceptance message has been received from Customs for Job B00000509 through the IE574 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REL- Released for Export</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Submission Date and Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>18-Sep-71 00:00</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000509";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc574Text = AESInterchangeProcessorTestHelper.GetStandardCC574CText("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc574Text, includeResponseWrap: false);

			return message;
		}

		protected override CC574CProvider GetProvider(TextReader reader) => new CC574CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC574C.Cc574C>(reader).Message);
	}
}
