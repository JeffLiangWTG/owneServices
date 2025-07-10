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
	[TestedType(typeof(CC582MessageInterpreter))]
	sealed class CC582MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC582MessageInterpreter, CC582CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE582;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Request on Non-exited Export message has been received from Customs for Job B00000509 through the IE582 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Limit for Response Date</td><td>18-Sep-71</td></tr><tr><td>Request on Non-Exited Export Date</td><td>18-Sep-71</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000509";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc582Text = AESInterchangeProcessorTestHelper.GetStandardAESCC582CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc582Text, includeResponseWrap: false);

			return message;
		}

		protected override CC582CProvider GetProvider(TextReader reader) => new CC582CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC582C.Cc582C>(reader).Message);
	}
}
