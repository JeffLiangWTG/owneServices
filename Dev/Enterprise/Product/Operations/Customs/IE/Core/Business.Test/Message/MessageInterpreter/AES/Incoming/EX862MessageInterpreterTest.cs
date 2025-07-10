using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX862;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(EX862MessageInterpreter))]
	class EX862MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX862MessageInterpreter, EX862Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX862;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Declaration Amendment Request Cancellation message has been received from Customs for Job B00000012 through the EX862 message stating that declaration amendment request that was initiated through message EX562 has now been canceled. <br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>CASEID</td></tr><tr><td>Amendment Request Cancellation Reason</td><td>REASON</td></tr></table>
";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex862Text = AESInterchangeProcessorTestHelper.GetStandardEX862Text("21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex862Text, includeResponseWrap: false);

			return message;
		}

		protected override EX862Provider GetProvider(TextReader reader) => new EX862Provider(new MailBoxItemProvider<Ex862>(reader).Message);
	}
}
