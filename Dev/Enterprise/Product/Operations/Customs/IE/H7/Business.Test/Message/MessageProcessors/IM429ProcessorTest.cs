using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM429;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM429Processor))]
	class IM429ProcessorTest : AISH7MessageProcessorTest<IM429Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM429Provider>
	{
		protected override IM429Processor Processor => new IM429Processor(logger, typeof(Im429));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM429;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => "IM429: Release for Import";

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Released, messageAttachee.ABL_BillStatus);
			Assert("Requested document status", messageAttachee.RequestedDocuments.Cast<EU.Business.RequestedDocument>().All(x => x.CSI_Status == RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived));
			AssertMessageInterpretation(incomingMessage, @"A Customs Declaration (IM429) message has been received for Job H7D00000001.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Sample remarks</td></tr></table>");
		}

		ZString GetMessageText()
		{
			return Serialize(
				new Im429
				{
					Declaration = new DeclarationType
					{
						Lrn = "LRN001",
						AdditionalDeclarationType = "A",
						CustomsOffices = new CustomsOffices02Type
						{
							CustomsOfficeLodgement = "AB123456"
						},
						DeferredPayment = new DeferredPaymentType
						{
							DeferredPayment = "A"
						},
						Mrn = "12MRN345ABCDE678R9",
						Parties = new PartiesType
						{
							Declarant = new DeclarantType
							{
								DeclarantName = "John Doe",
								DeclarantIdentificationNumber = "ID123456",
								DeclarantAddress = new AddressType
								{
									DeclarantAddressCity = "Dublin",
									DeclarantAddressCountry = "IE",
									DeclarantAddressStreetAndNumber = "Main Street 123",
									DeclarantAddressPostCode = "D01"
								}
							}
						},
						PreferredPaymentMethod = "A",
						Remarks = "Sample remarks"
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
