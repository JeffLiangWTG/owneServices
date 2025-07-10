using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM862;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM862MessageInterpreter))]
	class IM862MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM862MessageInterpreter, IM862Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM862;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AISInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var im862Text = AISInterchangeProcessorTestHelper.GetStandardIM862Text("21IEDUB11A782454R2", "Reason", "123456789123456789123456789456123456");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", im862Text, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Declaration Amendment Request Cancellation (IM862) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>123456789123456789123456789456123456</td></tr><tr><td>Amendment Request Cancellation Reason</td><td>Reason</td></tr></table>";

		protected override IM862Provider GetProvider(TextReader reader) => new IM862Provider(new MailBoxItemProvider<Im862>(reader).Message);
	}
}
