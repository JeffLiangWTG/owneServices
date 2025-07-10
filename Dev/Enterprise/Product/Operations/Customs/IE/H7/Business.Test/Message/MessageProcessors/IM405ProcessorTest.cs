using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM405Processor))]
	class IM405ProcessorTest : AISH7MessageProcessorTest<IM405Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM405Provider>
	{
		protected override IM405Processor Processor => new IM405Processor(logger, typeof(Im405));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM405;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM405;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", LogicalStatusList.Codes.Invalid, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"An Amendment Request Rejection (IM405) message has been received from customs for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Amendment Rejection Date</td><td>01-Aug-23</td></tr><tr><td>Amendment Rejection Motivation Text</td><td>RejectionMotivationText</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(new Im405
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					AmendmentRejectionDate = "20230801",
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
					AmendmentRejectionMotivationText = "RejectionMotivationText",
					Remarks = "Remarks001"
				}
			});
		}
	}
}
