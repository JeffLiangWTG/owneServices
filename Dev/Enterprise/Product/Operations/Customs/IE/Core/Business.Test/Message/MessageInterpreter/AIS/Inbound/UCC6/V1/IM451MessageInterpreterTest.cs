using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM451;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM451Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM451Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM451MessageInterpreter))]
	class IM451MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM451MessageInterpreter, IIM451Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM451;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(new Im451
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345ABCDE678R9",
					Lrn = "LRN001",
					AdditionalDeclarationType = "A",
					RejectionReason = "Invalid data",
					PreferredPaymentMethod = "J",
					Remarks = "Remarks001",
					ControlResult = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX.ControlsType
					{
						ControlResultCode = "CI001",
						ControlDate = "20230101",
						Remarks = "rr001"
					}
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Release Rejection (IM451) message has been received for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Decision Reason</td><td>Invalid data</td></tr><tr><td>Preferred Payment Method</td><td>J</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Result Code</td><td>CI001</td></tr><tr><td>Control Result Date</td><td>01-Jan-23</td></tr><tr><td>Remarks</td><td>rr001</td></tr></table>";

		protected override IIM451Provider GetProvider(TextReader reader) => new IM451Provider(new MailBoxItemProvider<Im451>(reader).Message);
	}
}
