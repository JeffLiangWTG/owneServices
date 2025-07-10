using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM404Processor))]
	class IM404ProcessorTest : EntryHeaderMessageProcessorTest<IM404Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM404Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM404;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText()
		{
			return Serialize(
				new Im404
				{
					ImportOperation = new MCciOperationType47
					{
						Lrn = "LRN001",
						CustomsRegistrationNumber = "12AB345CDEFGH678R9",
						Mrn = "12MRN345CDEFG678R9",
						AmendmentDateAndTime = new DateTime(2023, 08, 10, 14, 30, 45),
						AmendmentAcceptanceDateAndTime = new DateTime(2023, 08, 11, 14, 30, 45),
						PreferredPaymentMethod = "A",
						Remarks = "Remarks001",
					},
					CustomsOfficeOfPresentation = new MPcoType { ReferenceNumber = "PCO12345" },
					SupervisingCustomsOffice = new MScoType { ReferenceNumber = "SCO12345" },
					CustomsOfficeLodgement = new MScoType { ReferenceNumber = "LCO12345" },
					Declarant = new MDeclarantType { IdentificationNumber = "DECLARANT" },
					Representative = new MRepresentativeType { IdentificationNumber = "REPRESENTATIVE", Status = "0" },
					DeferredPayment = new System.Collections.ObjectModel.Collection<MDeferredPaymentType> { },
					GoodsShipment = new GoodsShipmentIm404Type
					{
						Taxes = new Taxes02Type { },
						GoodsShipmentItem = new System.Collections.ObjectModel.Collection<GoodsShipmentItemIm404Type>
					{
						new GoodsShipmentItemIm404Type
						{
							DeclarationGoodsItemNumber = "1001",
							CalculationOfTaxes = CreateCulationOfTaxes(),
						}
					},
					},
				}
			);
		}

		protected override ZString MessageFriendlyName => "IM404: Amendment Request Registration";

		protected override IM404Processor Processor => new IM404Processor(logger, typeof(Im404));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, @"An Amendment Request Registration (IM404) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Customs Registration Number</td><td>12AB345CDEFGH678R9</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Amendment Date and Time</td><td>10-Aug-23 14:30</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>11-Aug-23 14:30</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Amendment Request Registration (IM404) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });

			MessageProcessorHelperTest.AssertConfirmedDutiesAndTaxesAdded(messageAttachee.MergedLines[0]);
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var entryHeader = result.messageAttachee;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1001;

			return result;
		}

		MCalculationOfTaxesType04 CreateCulationOfTaxes()
		{
			return new MCalculationOfTaxesType04()
			{
				DutiesAndTaxes = MessageProcessorHelperTest.CreateDutiesAndTaxesType03Data()
			};
		}
	}
}
