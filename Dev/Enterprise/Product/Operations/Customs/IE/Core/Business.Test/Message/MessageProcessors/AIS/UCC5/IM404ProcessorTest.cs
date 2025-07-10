using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM404;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;
using IM404Provider = Enterprise.Customs.IE.Messaging.UCC5.IM404Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM404Processor))]
	sealed class IM404ProcessorTest : EntryHeaderMessageProcessorTest<IM404Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM404Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM404;

		protected override ZString MessageText => Serialize(GetMessageObject());

		protected override ZString MessageFriendlyName => "IM404: Amendment Acceptance";

		protected override IM404Processor Processor => new IM404Processor(logger, typeof(Im404));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.AmendmentRequestRegistration, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, IM404MessageInterpreterTest.DefaultInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Amendment Request Registration (IM404) message has been received from customs for Job B00001000." },
				new string[] { "staff1@where.com" });

			AssertEquals("ConfirmedFees Count", 1, messageAttachee.MergedLines[0].ConfirmedFees.Count);
			var confirmedFee = messageAttachee.MergedLines[0].ConfirmedFees[0];
			AssertEquals("ConfirmedFee amount", 1.7m, confirmedFee.CF_ChargeAmount);
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var lines = result.messageAttachee.MergedLines;
			(lines.FirstOrDefault() ?? lines.AddNew()).CL_LineNumber = 1;
			return result;
		}

			static internal Im404 GetMessageObject()
		{
			return new Im404
			{
				Declaration = new DeclarationType
				{
					AmendmentAcceptanceDate = "20240301",
					Mrn = "12MRN345CDEFG678R9",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IEDUB100"
					},
					PreferredPaymentMethod48 = "A",
					Remarks = "Remarks001"
				},
				GoodsShipment = new GoodsShipmentType
				{
					GoodsShipmentItem = new System.Collections.ObjectModel.Collection<GoodsShipmentTypeItem>
					{
						new GoodsShipmentTypeItem
						{
							GoodsItemNumber16 = "1",
							Taxes = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxesType
							{
								TaxBox43Bis = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType>
								{
									new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.TaxBoxType
									{
										BoxTaxType = "A00",
										BoxAmount = 100m,
										BoxTaxRate = 1.7m,
										BoxTaxPayableAmount = 1.7m,
										BoxTaxPaymentMethod = "A"
									}
								}
							}
						}
					},
					Taxes = new CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex.Taxes02Type
					{
					}
				}
			};
		}
	}
}

