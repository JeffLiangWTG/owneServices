using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM484;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM484Processor))]
	class IM484ProcessorTest : AISH7MessageProcessorTest<IM484Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM484Provider>
	{
		protected override IM484Processor Processor => new IM484Processor(logger, typeof(Im484));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM484;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM484;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);
			AssertMessageInterpretation(incomingMessage, @"A Document Presentation Request (IM484) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>LRN</td><td>LRN123</td></tr><tr><td>Request Date</td><td>14-Mar-24</td></tr><tr><td>Date Limit</td><td>&nbsp;</td></tr></table><br />
<br />Shipment Additional Information 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>Y023</td></tr><tr><td>Document Complementary Information</td><td>comp info1</td></tr></table><br />
<br />Shipment Additional Information 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Document Type</td><td>U713</td></tr><tr><td>Document Complementary Information</td><td>comp info 2</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im484
				{
					Declaration = new DeclarationType()
					{
						Mrn = "12MRN345CDEFG678R9",
						Lrn = "LRN123",
						RequestDate = "20240314",
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
				}
			);
		}
	}
}
