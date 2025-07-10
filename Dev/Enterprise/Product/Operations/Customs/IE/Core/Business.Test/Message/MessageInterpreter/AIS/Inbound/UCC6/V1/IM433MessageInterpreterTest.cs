using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM433;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM433Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM433Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM433MessageInterpreter))]
	class IM433MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM433MessageInterpreter, IIM933Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM433;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(new Im433
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345ABCDE678R9",
					RejectionDate = "20230101",
					RejectionReason = "Invalid data"
				},
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Presentation Notification Rejection (IM433) message has been received for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Rejection Date</td><td>01-Jan-23</td></tr><tr><td>Rejection Reason</td><td>Invalid data</td></tr></table>";

		protected override IIM933Provider GetProvider(TextReader reader) => new IM433Provider(new MailBoxItemProvider<Im433>(reader).Message);
	}
}
