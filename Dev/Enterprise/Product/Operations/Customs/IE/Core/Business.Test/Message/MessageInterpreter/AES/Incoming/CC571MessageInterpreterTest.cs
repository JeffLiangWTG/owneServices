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
	[TestedType(typeof(CC571MessageInterpreter))]
	class CC571MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC571MessageInterpreter, CC571CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE571;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Re-Export Notification Registration message has been received from Customs for Job B00000509 through the IE571 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REL- Released for Export</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Re-Export Notification Registration Date</td><td>18-Sep-71</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000509";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc571Text = AESInterchangeProcessorTestHelper.GetStandardCC571CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc571Text, includeResponseWrap: false);

			return message;
		}

		protected override CC571CProvider GetProvider(TextReader reader) => new CC571CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC571C.Cc571C>(reader).Message);
	}
}
