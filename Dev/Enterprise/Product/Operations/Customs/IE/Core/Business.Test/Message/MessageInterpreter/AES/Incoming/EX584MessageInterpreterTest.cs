using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX584;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX584MessageInterpreter))]
	class EX584MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, EX584MessageInterpreter, EX584Provider>
	{
		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX584;

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00000012";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var ex584Text = AESInterchangeProcessorTestHelper.GetStandardEX584Text("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", ex584Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => @"A Document Request Presentation message has been received from Customs for Job B00000012 through the EX584 message stating that a list of following documents will need to be presented physically. <br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Request Date</td><td>03-Aug-22 00:00</td></tr><tr><td>Date Limit</td><td>10-Aug-22 00:00</td></tr><tr><td>Document Type</td><td>D01</td></tr><tr><td>Document Complementary Information</td><td>Document of type 01</td></tr><tr><td>Document Type</td><td>D02</td></tr><tr><td>Document Complementary Information</td><td>Document of type 02</td></tr></table>
";

		protected override EX584Provider GetProvider(TextReader reader) => new EX584Provider(new MailBoxItemProvider<Ex584>(reader).Message);
	}
}
