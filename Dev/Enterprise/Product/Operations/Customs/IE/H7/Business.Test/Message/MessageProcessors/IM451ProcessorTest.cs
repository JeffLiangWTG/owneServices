using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM451;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM451Processor))]
	class IM451ProcessorTest : AISH7MessageProcessorTest<IM451Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM451Provider>
	{
		protected override IM451Processor Processor => new IM451Processor(logger, typeof(Im451));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM451;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM451;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", AISEntryStatusList.Codes.NotReleased, messageAttachee.ABL_BillStatus);
			AssertMessageInterpretation(incomingMessage, @"A Release Rejection (IM451) message has been received for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Decision Reason</td><td>Invalid data</td></tr><tr><td>Preferred Payment Method</td><td>J</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Result Code</td><td>TT</td></tr><tr><td>Control Result Date</td><td>01-Jan-23</td></tr><tr><td>Remarks</td><td>Remarks002</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im451
				{
					Declaration = new DeclarationType
					{
						Mrn = "12MRN345ABCDE678R9",
						Lrn = "LRN001",
						AdditionalDeclarationType = "A",
						RejectionReason = "Invalid data",
						PreferredPaymentMethod = "J",
						Remarks = "Remarks001",
						ControlResult = new ControlsType
						{
							ControlResultCode = "TT",
							ControlDate = "20230101",
							Remarks = "Remarks002"
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
						CustomsOffices = new CustomsOffices02Type
						{
							CustomsOfficeLodgement = "AB123456"
						},
					},
					GoodsShipment = new GoodsShipmentType
					{
						ValuationInformation = new ValuationInformationType
						{
						},
						GovernmentAgencyGoodsItem = new Collection<GoodsShipmentItemType>
						{
							new GoodsShipmentItemType
							{
								GoodsItemNumber = "1",
								Procedure = new Collection<AdditionalProcedureType>
								{
									new AdditionalProcedureType
									{
										AdditionalProcedure = new ProcedureCodeType
										{
											AdditionalProcedure = "C08"
										}
									}
								},
								ItemAmountInvoicedIntrinsicValue = new ValuationInformation02Type
								{
									Value = new ItemAmountType
									{
									}
								},
								GoodsInformation = new GoodsInformationType
								{
									DescriptionOfGoods = new DescriptionOfGoodsType
									{
										DescriptionOfGoods = "Sample description"
									},
									CommodityCode = new CommodityCodeType
									{
										CommodityCodeHarmonizedSystemSubHeadingCode = "183955"
									},
									Packaging = new PackagesMandatoryType
									{
										PackagingNumberOfPackages = "3"
									}
								}
							}
						}
					}
				});
		}
	}
}
