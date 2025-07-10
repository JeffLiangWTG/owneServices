using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC609C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC609MessageInterpreter))]
	sealed class CC609MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC609MessageInterpreter, CC609CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE609;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var cc609Text = AESInterchangeProcessorTestHelper.GetStandardAESCC609CText("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", cc609Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An EXS/REN Invalidation Decision message has been received from Customs for Job B00000012 through the IE609 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Invalidation Decision Date and Time</td><td>19-Sep-71 00:00</td></tr><tr><td>Invalidation Request Date and Time</td><td>18-Sep-71 00:00</td></tr></table>";

		protected override CC609CProvider GetProvider(TextReader reader) => new CC609CProvider(new MailBoxItemProvider<Cc609C>(reader).Message);
	}
}
