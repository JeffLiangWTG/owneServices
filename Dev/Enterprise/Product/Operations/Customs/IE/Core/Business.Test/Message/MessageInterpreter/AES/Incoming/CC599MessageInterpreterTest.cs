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
	[TestedType(typeof(CC599MessageInterpreter))]
	class CC599MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC599MessageInterpreter, CC599CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE599;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Export Notification message has been received from Customs for Job B00000012 through the IE599 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>EXP</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Exit Control Code</td><td>A1</td></tr><tr><td>Exit Date</td><td>22-Feb-22</td></tr><tr><td>Exit Stopped Date</td><td>01-Jul-22</td></tr><tr><td>State of Seals</td><td>0</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc556Text = AESInterchangeProcessorTestHelper.GetStandardCC599CText("A1", "LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc556Text, includeResponseWrap: false);

			return message;
		}

		protected override CC599CProvider GetProvider(TextReader reader) => new CC599CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC599C.Cc599C>(reader).Message);
	}
}
