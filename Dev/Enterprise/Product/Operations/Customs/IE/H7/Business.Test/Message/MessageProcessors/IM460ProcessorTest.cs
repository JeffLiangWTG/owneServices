using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM460;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM460Processor))]
	class IM460ProcessorTest : AISH7MessageProcessorTest<IM460Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM460Provider>
	{
		protected override IM460Processor Processor => new IM460Processor(logger, typeof(Im460));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM460;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => "IM460 – Control Notice";

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Control, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"A Control Notice (IM460) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Notification Date</td><td>20-Feb-24</td></tr><tr><td>Time Limit For Control</td><td>07-Mar-24 14:37</td></tr><tr><td>Overall Control Type Code</td><td>Orange</td></tr><tr><td>Overall Control Type Description</td><td>Documentary Control</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im460
				{
					Declaration = new DeclarationType
					{
						Mrn = "12MRN345CDEFG678R9",
						ControlNotificationDate = "202402202359GMT",
						TimeLimitForControl = "202403071437GMT",
						CustomsOffices = new CustomsOfficesLodgmentType() { CustomsOfficeLodgement = "LCO12345" },
					},
					OverallControlType = new OverAllControlsType
					{
						ControlTypeCoded = "Orange",
					},
					GoodsShipment = new System.Collections.ObjectModel.Collection<GoodsShipmentItemType>
					{
						new GoodsShipmentItemType
						{
							GoodsItemNumber = "1",
							ControlType = new System.Collections.ObjectModel.Collection<ControlsType>
							{
								new ControlsType
								{
									ControlTypeCoded = "Orange"
								}
							}
						}
					}
				}
			);
		}
	}
}
