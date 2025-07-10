using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM404 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM404;
using IM404Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM404Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM404MessageInterpreter))]
	class IM404MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM404MessageInterpreter, IIM404Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM404;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(new IM404.Im404
			{
				Declaration = new IM404.DeclarationType
				{
					Mrn = "123456789012345678",
					AmendmentAcceptanceDate = "20231231",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX.CustomsOffices02Type
					{
						CustomsOfficeLodgement = "AB123456"
					},
					Remarks = "Sample remarks"
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Amendment Request Registration (IM404) message has been received from customs for Job B00000012.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>123456789012345678</td></tr><tr><td>Amendment Acceptance Date</td><td>31-Dec-23</td></tr><tr><td>Preferred Payment Method</td><td>&nbsp;</td></tr><tr><td>Remarks</td><td>Sample remarks</td></tr></table>";

		protected override IIM404Provider GetProvider(TextReader reader) => new IM404Provider(new MailBoxItemProvider<IM404.Im404>(reader).Message);
	}
}
