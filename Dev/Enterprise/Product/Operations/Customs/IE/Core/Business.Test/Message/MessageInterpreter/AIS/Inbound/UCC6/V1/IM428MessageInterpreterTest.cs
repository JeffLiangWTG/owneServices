using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM428;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM428Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM428Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM428MessageInterpreter))]
	class IM428MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM428MessageInterpreter, IIM428Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM428;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(new Im428
			{
				Declaration = new DeclarationType
				{
					Lrn = "LRN001",
					Mrn = "12MRN345CDEFG678R9",
					AcceptanceDate = "20230810",
					AdditionalDeclarationType = "IM",
					PreferredPaymentMethod = "A",
					Remarks = "Remarks001",
				},
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Customs Declaration Acceptance or Goods Deemed to be Placed under Customs Warehousing Procedure (IM428) message has been received from customs for Job B00001000.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Additional Declaration Type</td><td>IM</td></tr><tr><td>Declaration Acceptance Date</td><td>10-Aug-23</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>";

		protected override IIM428Provider GetProvider(TextReader reader) => new IM428Provider(new MailBoxItemProvider<Im428>(reader).Message);
	}
}
