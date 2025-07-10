using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM415V;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM415VProcessor))]
	class IM415VProcessorTest : AISH7MessageProcessorTest<IM415VProcessor, AISInboundEDIMessage, AISOutboundEDIMessage, IM415VProvider>
	{
		protected override IM415VProcessor Processor => new IM415VProcessor(logger, typeof(Im415V));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM415V;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM415V;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Prelodged, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", LogicalStatusList.Codes.Acknowledged, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration Acknowledgment (IM415V) message has been received for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>D</td></tr><tr><td>LRN</td><td>ACPTESTIM0990446123456</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Declaration Acknowledgement Date</td><td>2021-02-15</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im415V()
				{
					Declaration = new DeclarationType()
					{
						AdditionalDeclarationType = "D",
						Lrn = "ACPTESTIM0990446123456",
						Mrn = "21IEDUB11A782454R2",
						DeclarationAcknowledgementDate = "20210215",
						CustomsOffices = new CustomsOfficesType
						{
							CustomsOfficeLodgement = "IEDUB100",
						},
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
						}
					}
				});
		}
	}
}
