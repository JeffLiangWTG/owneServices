using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS328;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS328MessageInterpreter))]
	sealed class TS328MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS328MessageInterpreter, TS328Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS328;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var messageText = IEXmlObjectSerializer.Serialize(new Ts328()
			{
				Declaration = new DeclarationType13
				{
					Lrn = "LRN123456789",
					Mrn = "21IEDUB11A782454R2",
					AcceptanceDate = new DateOfAcceptanceType
					{
						DateOfAcceptance = new System.DateTime(2023, 09, 05),
					},
					ResponseDateLimit = new System.DateTime(2023, 10, 05),
					PreviousDocument = new System.Collections.ObjectModel.Collection<PreviousdocumentType08>
					{
						new PreviousdocumentType08
						{
							DateAndTimeOfPresentationOfTheGoods = new System.DateTime(2023, 09, 01, 0, 0, 0),
						}
					},
					Remarks = "Sample text 123",
				}
			});
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "MAN0001000", messageText);
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A [G4 | G4+G3 | Manifest] Declaration Acceptance (TS328) message has been received for Job MAN0001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Date of Acceptance</td><td>05-Sep-23</td></tr><tr><td>Response Date Limit</td><td>05-Oct-23</td></tr><tr><td>Date and Time of Presentation of the Goods</td><td>01-Sep-23 00:00</td></tr><tr><td>Remarks</td><td>Sample text 123</td></tr></table>";

		protected override TS328Provider GetProvider(TextReader reader) => new TS328Provider(new MailBoxItemProvider<Ts328>(reader).Message);
	}
}
