using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM099;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM099 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM099.Im099;
using IM099Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM099Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM099MessageInterpreter))]
	class IM099MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM099MessageInterpreter, IIM099Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM099;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(new IM099
			{
				Declaration = new DeclarationType
				{
					Lrn = "1234567890123456789012",
					DateLimitOfResponse = "20231231",
					CustomsOffices = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX.CustomsOfficeLodgementType
					{
						CustomsOfficeLodgement = "AB123456"
					},
					Remarks = "Sample remarks"
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A General Notification and Request Information (IM099) message has been received for Job B00000012.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>1234567890123456789012</td></tr><tr><td>Date Limit of Response</td><td>31-Dec-23</td></tr><tr><td>Remarks</td><td>Sample remarks</td></tr><tr><td>Customs Office Lodgement</td><td>AB123456</td></tr></table>";

		protected override IIM099Provider GetProvider(TextReader reader) => new IM099Provider(new MailBoxItemProvider<IM099>(reader).Message);
	}
}
