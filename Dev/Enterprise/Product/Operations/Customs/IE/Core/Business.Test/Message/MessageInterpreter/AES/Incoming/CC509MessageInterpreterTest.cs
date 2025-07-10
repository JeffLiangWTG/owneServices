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
	[TestedType(typeof(CC509MessageInterpreter))]
	class CC509MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, CC509MessageInterpreter, CC509CProvider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE509;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"An Invalidation Decision message has been received from Customs for Job B00000509 through the IE509 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Canceled (CAN)</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Invalidation Decision Date and Time</td><td>&lt;Invalid&gt;</td></tr><tr><td>Invalidation Request Date and Time</td><td>&nbsp;</td></tr><tr><td>Initiated by Customs</td><td>Y</td></tr><tr><td>Invalidation Justification</td><td>&nbsp;</td></tr></table>";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000509";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC509CMailboxItemText("TransactionID", "1", includeResponseWrap: false);

			return message;
		}

		protected override CC509CProvider GetProvider(TextReader reader) => new CC509CProvider(new MailBoxItemProvider<CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC509C.Cc509C>(reader).Message);
	}
}
