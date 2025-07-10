using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC529C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC529MessageInterpreter))]
	class CC529MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC529MessageInterpreter, CC529CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE529;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Release Response message has been received from Customs for Job B00000012 through the IE529 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Goods Released for Export</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Release Date</td><td>19-Sep-71</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc529Text = AESInterchangeProcessorTestHelper.GetStandardCC529CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc529Text, includeResponseWrap: false);

			return message;
		}

		protected override CC529CProvider GetProvider(TextReader reader) => new CC529CProvider(new MailBoxItemProvider<Cc529C>(reader).Message);
	}
}
