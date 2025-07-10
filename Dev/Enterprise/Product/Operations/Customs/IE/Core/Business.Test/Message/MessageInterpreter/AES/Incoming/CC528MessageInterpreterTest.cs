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
	[TestedType(typeof(CC528MessageInterpreter))]
	class CC528MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC528MessageInterpreter, CC528CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE528;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Export MRN Allocation message has been received from Customs for Job B00000012 through the IE528 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Accepted by Customs (MRN Allocated)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Acceptance Date</td><td>09-Jun-22</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc528Text = AESInterchangeProcessorTestHelper.GetStandardCC528CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc528Text, includeResponseWrap: false);

			return message;
		}

		protected override CC528CProvider GetProvider(TextReader reader) => new CC528CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC528C.Cc528C>(reader).Message);
	}
}
