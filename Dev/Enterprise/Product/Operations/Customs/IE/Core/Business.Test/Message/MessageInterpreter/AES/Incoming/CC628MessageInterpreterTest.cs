using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC628C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC628MessageInterpreter))]
	class CC628MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC628MessageInterpreter, CC628CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE628;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Exit Summary Declaration Acknowledgement message has been received from Customs for Job B00000012 through the IE628 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Released for Export (REL)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Acceptance Date</td><td>09-Jun-22</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc628Text = AESInterchangeProcessorTestHelper.GetStandardCC628CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc628Text, includeResponseWrap: false);

			return message;
		}

		protected override CC628CProvider GetProvider(TextReader reader) => new CC628CProvider(new MailBoxItemProvider<Cc628C>(reader).Message);
	}
}
