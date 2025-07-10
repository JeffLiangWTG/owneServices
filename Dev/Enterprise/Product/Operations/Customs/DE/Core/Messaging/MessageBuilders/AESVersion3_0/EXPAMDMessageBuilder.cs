using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;
using CargoWise.Customs.Shared.MessageContracts;
using static CargoWise.Customs.DE.MessageContracts.MessageSchema.AESMessageSchema;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public sealed class EXPAMDMessageBuilder : MessageBuilder<DEXPAE>
	{
		public EXPAMDMessageBuilder(IAESMessageHeader dataProvider)
		{
			this.dataProvider = Argument.NotNull(dataProvider, nameof(dataProvider));
			headerProvider = (IEXPAMDHeader)dataProvider.AESHeader;
		}
		readonly IAESMessageHeader dataProvider;
		readonly IEXPAMDHeader headerProvider;

		protected override DEXPAE GetMessageCore() => AESMessageBuilderHelper.CreateMessage<DEXPAE, DEXPAEMessageSender, DEXPAEMessageRecipient>(dataProvider, "E.1.7", message =>
		{
			bool anyCommoditySpecified = headerProvider.Lines.Any(l => l.CommoditySpecified);

			message.ExportOperation = PopulateExportOperation();
			message.CustomsOfficeOfExport = PopulateCustomsOfficeOfExport();
			message.Declarant = AESMessageBuilderHelper.CreateCommonEoriIdentification<DEXPAEDeclarant>(headerProvider.Declarant);
			message.Representative = AESMessageBuilderHelper.CreateCommonEoriIdentification<DEXPAERepresentative>(headerProvider.Representative);
			message.GoodsShipment = PopulateGoodsShipment();

			DEXPAEExportOperation PopulateExportOperation()
			{
				var invoiceAmountAndCurrencySpecified = headerProvider.InvoiceAmountAndCurrencySpecified && anyCommoditySpecified;
				return new DEXPAEExportOperation()
				{
					LRN = headerProvider.LocalReferenceNumber.LeftOrNull(LocalReferenceNumberMaxLength),
					MRN = headerProvider.MRN.LeftOrNull(MRNMaxLength),
					amendmentSubmissionDateAndTime = headerProvider.SubmissionDateAndTimeUtc.DateAndTime.ToUnspecified(),
					totalAmountInvoiced = headerProvider.InvoiceAmount,
					totalAmountInvoicedSpecified = invoiceAmountAndCurrencySpecified,
					invoiceCurrency = invoiceAmountAndCurrencySpecified ? headerProvider.Currency.LeftOrNull(CurrencyMaxLength) : null
				};
			}

			DEXPAECustomsOfficeOfExport PopulateCustomsOfficeOfExport()
			{
				return new DEXPAECustomsOfficeOfExport()
				{
					referenceNumber = headerProvider.ExportCustomsOffice.LeftOrNull(ExportCustomsOfficeMaxLength),
				};
			}

			DEXPAEGoodsShipment PopulateGoodsShipment()
			{
				return new DEXPAEGoodsShipment()
				{
					Consignment = PopulateConsignment(),
					GoodsItem = headerProvider.Lines.Select(PopulateGoodsItem).ToArray(),
				};

				DEXPAEGoodsShipmentConsignment PopulateConsignment()
				{
					return new DEXPAEGoodsShipmentConsignment
					{
						containerIndicator = headerProvider.IsContainerized ? DEXPAEGoodsShipmentConsignmentContainerIndicator.Item1 : DEXPAEGoodsShipmentConsignmentContainerIndicator.Item0,
						containerIndicatorSpecified = headerProvider.ContainerIndicatorSpecified,
						inlandModeOfTransport = headerProvider.InlandTransportMeansMode.LeftOrNull(InlandTransportMeanModeMaxLength1),
						modeOfTransportAtTheBorder = headerProvider.BorderTransportMeansMode.LeftOrNull(BorderTransportMeansModeMaxLength1),
						grossMass = headerProvider.TotalGrossMass,
						grossMassSpecified = headerProvider.TotalGrossMass > 0 && anyCommoditySpecified,
						referenceNumberUCR = headerProvider.CommercialReferenceNumber.LeftOrNull(ReferenceNumberMaxLength),
						TransportEquipment = headerProvider.TransportEquipments.Select(PopulateTransportEquipment).ToArray(),
						DepartureTransportMeans = headerProvider.DepartureTransportMeans.Select(PopulateDepartureTransportMeans).ToArray(),
						ActiveBorderTransportMeans = headerProvider.ActiveBorderTransportMeansSpecified ? PopulateActiveBorderTransportMeans() : null,
					};

					DEXPAEGoodsShipmentConsignmentTransportEquipment PopulateTransportEquipment(ITransportEquipment transportEquipment, int index)
					{
						return new DEXPAEGoodsShipmentConsignmentTransportEquipment
						{
							sequenceNumber = (index + 1).ToString(),
							containerIdentificationNumber = transportEquipment.ContainerIdentificationNumber.LeftOrNull(ContainerIdentificationNumberMaxLength),
							numberOfSeals = "0",
							GoodsReference = transportEquipment.DeclarationGoodsItemNumbers.Select(PopulateGoodsReference).ToArray(),
						};

						DEXPAEGoodsShipmentConsignmentTransportEquipmentGoodsReference PopulateGoodsReference(int goodsItemNumber, int goodsReferenceIndex)
						{
							return new DEXPAEGoodsShipmentConsignmentTransportEquipmentGoodsReference
							{
								sequenceNumber = (goodsReferenceIndex + 1).ToString(),
								declarationGoodsItemNumber = goodsItemNumber.ToString()
							};
						}
					}

					DEXPAEGoodsShipmentConsignmentDepartureTransportMeans PopulateDepartureTransportMeans(IDepartureTransportMeans departureTransportMeans, int index)
					{
						return new DEXPAEGoodsShipmentConsignmentDepartureTransportMeans
						{
							sequenceNumber = (index + 1).ToString(),
							typeOfIdentification = departureTransportMeans.TypeOfIdentification.LeftOrNull(TransportMeansTypeMaxLength),
							identificationNumber = departureTransportMeans.IdentificationNumber.LeftOrNull(TransportMeansIdentityMaxLength),
							nationality = departureTransportMeans.Nationality.LeftOrNull(TransportMeansNationalityMaxLength),
						};
					}

					DEXPAEGoodsShipmentConsignmentActiveBorderTransportMeans PopulateActiveBorderTransportMeans()
					{
						return new DEXPAEGoodsShipmentConsignmentActiveBorderTransportMeans
						{
							typeOfIdentification = headerProvider.BorderTransportMeansType.LeftOrNull(TransportMeansTypeMaxLength),
							identificationNumber = headerProvider.BorderTransportMeansIdentity.LeftOrNull(TransportMeansIdentityMaxLength),
							nationality = headerProvider.BorderTransportMeansNationality.LeftOrNull(TransportMeansNationalityMaxLength),
						};
					}
				}

				DEXPAEGoodsShipmentGoodsItem PopulateGoodsItem(IEXPAMDLine line, int index)
				{
					return new DEXPAEGoodsShipmentGoodsItem
					{
						sequenceNumber = line.LineNumber.ToString(),
						statisticalValue = line.StatisticalValue,
						statisticalValueSpecified = line.CommoditySpecified && line.StatisticalValueSpecified,
						referenceNumberUCR = line.CommercialReferenceNumber.LeftOrNull(ReferenceNumberMaxLength),
						Commodity = line.CommoditySpecified ? PopulateItemCommodity() : null,
						Packaging = line.CommoditySpecified ? line.Packages.Select(PopulateItemPackaging).ToArray() : Array.Empty<DEXPAEGoodsShipmentGoodsItemPackaging>(),
					};

					DEXPAEGoodsShipmentGoodsItemCommodity PopulateItemCommodity()
					{
						return new DEXPAEGoodsShipmentGoodsItemCommodity
						{
							GoodsMeasure = new DEXPAEGoodsShipmentGoodsItemCommodityGoodsMeasure
							{
								grossMass = line.GrossMass,
								netMass = line.NetMass,
								supplementaryUnits = line.SupplementaryQuantity,
								supplementaryUnitsSpecified = line.SupplementaryQuantity > 0
							}
						};
					}

					DEXPAEGoodsShipmentGoodsItemPackaging PopulateItemPackaging(IPackage package, int itemPackagingIndex)
					{
						return new DEXPAEGoodsShipmentGoodsItemPackaging
						{
							sequenceNumber = (itemPackagingIndex + 1).ToString(),
							typeOfPackages = package.Kind.LeftOrNull(PackageKindMaxLength2),
							numberOfPackages = package.IsSupportEmptyPackType ? package.Quantity.GetValueOrDefault().ToString() : null,
							shippingMarks = package.MarksNumbers.LeftOrNull(PackageMarksNumbersMaxLength),
							PackageReference = package.Quantity.GetValueOrDefault() == 0 ? new DEXPAEGoodsShipmentGoodsItemPackagingPackageReference() { declarationGoodsItemNumber = package.PositionNumber.ToString() } : null
						};
					}
				}
			}
		});
	}
}
