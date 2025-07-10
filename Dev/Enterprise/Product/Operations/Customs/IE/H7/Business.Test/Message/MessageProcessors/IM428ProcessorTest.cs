using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM428;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM428Processor))]
	class IM428ProcessorTest : AISH7MessageProcessorTest<IM428Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM428Provider>
	{
		protected override IM428Processor Processor => new IM428Processor(logger, typeof(Im428));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM428;

		protected override ZString MessageText => GetMessageText(needCreateTaxes: true);

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM428;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Accepted, messageAttachee.ABL_BillStatus);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration Acceptance or Goods Deemed to be Placed under Customs Warehousing Procedure (IM428) message has been received from customs for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Additional Declaration Type</td><td>IM</td></tr><tr><td>Declaration Acceptance Date</td><td>10-Aug-23</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table>");
		}

		public void TestMRNWithSpecificAdditionalDeclarationType()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();
			messageAttachee.MovementReferenceNumberSetter("00MRN345CDEFG678R9");

			var messageText = GetMessageText(needCreateTaxes: false, isSpecificAdditionalDeclarationType: true);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions(() =>
				{
					AssertEquals("SimplifiedDeclarationMRN ", string.Empty, GetCusEntryNumber(messageAttachee, CusEntryNumberTypes.EU.SimplifiedDeclarationMRN));
					AssertEquals("MovementReferenceNumber ", "12MRN345CDEFG678R9", GetCusEntryNumber(messageAttachee, CusEntryNumberTypes.Standard.MovementReferenceNumber));
					AssertEquals("MovementReferenceNumber ", "12MRN345CDEFG678R9", messageAttachee.MovementReferenceNumber);
				});
			}
		}

		protected ZString GetCusEntryNumber(AsycudaBill messageAttachee, string entryType)
		{
			var simEntryNumber = CusEntryNumber.Load(messageAttachee, entryType, messageAttachee.CountryCode);
			return simEntryNumber?.CE_EntryNum ?? ZString.Empty;
		}

		ZString GetMessageText(bool needCreateTaxes = false, bool isSpecificAdditionalDeclarationType = false)
		{
			return Serialize(
				new Im428
				{
					Declaration = new DeclarationType
					{
						Lrn = "LRN001",
						Mrn = "12MRN345CDEFG678R9",
						AcceptanceDate = "20230810",
						AdditionalDeclarationType = "IM",
						PreferredPaymentMethod = "A",
						Remarks = "Remarks001",
						CustomsOffices = new CustomsOfficesType()
						{
							CustomsOfficeLodgement = "IEDUB400"
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
						},
						DeferredPayment = new DeferredPaymentType()
						{
							DeferredPayment = "A"
						}
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
