using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM404Processor))]
	class IM404ProcessorTest : AISH7MessageProcessorTest<IM404Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM404Provider>
	{
		protected override IM404Processor Processor => new IM404Processor(logger, typeof(Im404));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM404;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM404;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"An Amendment Request Registration (IM404) message has been received from customs for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Amendment Acceptance Date</td><td>01-Aug-23</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(new Im404
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					AmendmentAcceptanceDate = "20230801",
					CustomsOffices = new CustomsOffices02Type() { CustomsOfficeLodgement = "LCO12345" },
					Parties = new PartiesType()
					{
						Declarant = new DeclarantType()
						{
							DeclarantName = "Tony",
							DeclarantIdentificationNumber = "DC012345",
							DeclarantAddress = new AddressType()
							{
								DeclarantAddressCity = "New York",
								DeclarantAddressCountry = "US",
								DeclarantAddressStreetAndNumber = "No.1 of Wall Street",
								DeclarantAddressPostCode = "100000"
							}
						}
					},
					DeferredPayment = new DeferredPaymentType()
					{
						DeferredPayment = "A"
					},
					PreferredPaymentMethod = "A",
					Remarks = "Remarks001"
				},
				GoodsShipment = new GoodsShipmentType
				{
					GovernmentAgencyGoodsItem = new System.Collections.ObjectModel.Collection<GovernmentAgencyGoodsItem>
						{
							new GovernmentAgencyGoodsItem
							{
								GoodsItemNumber = "1",
							}
						}
				}
			});
		}
	}
}
