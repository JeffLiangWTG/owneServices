using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM426;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM426MessageInterpreter))]
	sealed class IM426MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM426MessageInterpreter, IM426Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM464;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<AISInboundEDIMessage>();
			message.EM_LinkedObject = entryHeader;
			var im415vText = AISInterchangeProcessorTestHelper.GetStandardIM426Text("LRN123456789", "21IEDUB11A782454R2");
			message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText("TransactionID", im415vText, includeResponseWrap: false);

			return message;
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Registration Notification (IM426) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Customs Registration Number</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Registration Date and Time</td><td>17-Aug-23 14:38</td></tr><tr><td>Presentation Notification Due Date</td><td>2023-05-20</td></tr></table>";

		protected override IM426Provider GetProvider(TextReader reader) => new IM426Provider(new MailBoxItemProvider<Im426>(reader).Message);
	}
}
