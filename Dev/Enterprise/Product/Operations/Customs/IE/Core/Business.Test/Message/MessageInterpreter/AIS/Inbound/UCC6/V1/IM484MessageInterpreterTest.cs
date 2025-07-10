using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM484;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM484Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM484Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM484MessageInterpreter))]
	class IM484MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM484MessageInterpreter, IIM484Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM484;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(new Im484
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					Lrn = "LRN123",
					RequestDate = "20240314",
					DateLimit = "20240413",
				},
				GoodsShipment = new Collection<DocumentAdditionalInformationType>()
				{
					new DocumentAdditionalInformationType()
					{
						DocumentComplementaryInformation = "comp info1",
						DocumentType = "Y023",
					},
					new DocumentAdditionalInformationType()
					{
						DocumentComplementaryInformation = "comp info 2",
						DocumentType = "U713",
					}
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Document Presentation Request (IM484) message has been received for Job B00000012.<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>LRN</td><td>LRN123</td></tr><tr><td>Request Date</td><td>14-Mar-24</td></tr><tr><td>Date Limit</td><td>13-Apr-24</td></tr></table><br/>
<br/>Shipment Additional Information 1<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>Y023</td></tr><tr><td>Document Complementary Information</td><td>comp info1</td></tr></table><br/>
<br/>Shipment Additional Information 2<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>U713</td></tr><tr><td>Document Complementary Information</td><td>comp info 2</td></tr></table>";

		protected override IIM484Provider GetProvider(TextReader reader) => new IM484Provider(new MailBoxItemProvider<Im484>(reader).Message);
	}
}
