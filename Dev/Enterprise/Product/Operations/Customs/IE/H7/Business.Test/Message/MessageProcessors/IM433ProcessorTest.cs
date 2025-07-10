using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM433;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM933Processor))]
	class IM433ProcessorTest : AISH7MessageProcessorTest<IM933Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM933Provider>
	{
		protected override IM933Processor Processor => new IM933Processor(logger, typeof(Im433));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM433;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM933;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Prelodged, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", LogicalStatusList.Codes.Invalid, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"A Presentation Notification Rejection (IM433) message has been received for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Rejection Date</td><td>10-Aug-23</td></tr><tr><td>Rejection Reason</td><td>Invalid data</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im433
				{
					Declaration = new DeclarationType
					{
						Mrn = "12MRN345ABCDE678R9",
						RejectionDate = "202308101230ABC",
						RejectionReason = "Invalid data",
						CustomsOffices = new CustomsOfficeLodgementType()
						{
							CustomsOfficeLodgement = "IEDUB400"
						},
						Parties = new PartiesDeclarantType()
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
					},
				});
		}
	}
}
