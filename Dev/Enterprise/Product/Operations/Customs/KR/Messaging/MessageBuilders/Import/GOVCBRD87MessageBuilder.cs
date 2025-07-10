using System;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRD87;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._D87)]
	public class GOVCBRD87MessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImportD87Header dataProvider;
		public GOVCBRD87MessageBuilder(IImportD87Header dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				ExpirationDateTime = PopulateExpirationDateTime(),
				Id = PopulateID(),
				InvoiceAmount = PopulateInvoiceAmount(),
				IssueDateTime = PopulateIssueDateTime(),
				TotalGrossMassMeasure = PopulateTotalGrossMassMeasure(),
				TotalPackageQuantity = PopulateTotalPackageQuantity(),
				TypeCode = PopulateTypeCode(),
				Agent = PopulateAgent(),
				GoodsShipment = PopulateGoodsShipment(),
				Importer = PopulateImporter(),
				TransportContractDocument = PopulateTransportContractDocument(),
				Ucr = PopulateUCR()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		ZString PopulateExpirationDateTime()
		{
			return dataProvider.EffectiveToDate.IsValid ? dataProvider.EffectiveToDate.ToString(DateFormatType.Date) : string.Empty;
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.CarnetCertificateNumber };
		}

		DeclarationInvoiceAmountType PopulateInvoiceAmount()
		{
			return new DeclarationInvoiceAmountType
			{
				Value = dataProvider.TotalInvoiceAmount,
				CurrencyId = Enum.TryParse(dataProvider.InvoiceCurrency, true, out Iso3AlphaCurrencyCodeContentType result) ? result : new Iso3AlphaCurrencyCodeContentType?(),
			};
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTotalGrossMassMeasureType PopulateTotalGrossMassMeasure()
		{
			return dataProvider.TotalGrossWeight.IsEmpty && dataProvider.TotalGrossWeighUnit.IsEmpty ? null : new DeclarationTotalGrossMassMeasureType
			{
				Value = dataProvider.TotalGrossWeight,
				KcsUnitCode = dataProvider.TotalGrossWeighUnit
			};
		}

		DeclarationTotalPackageQuantityType PopulateTotalPackageQuantity()
		{
			return new DeclarationTotalPackageQuantityType { Value = dataProvider.TotalQty };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._D87 };
		}

		DeclarationAgent PopulateAgent()
		{
			return new DeclarationAgent
			{
				Id = new AgentIdentificationIdType { Value = dataProvider.UnipassDeclarantID },
				Name = new AgentNameTextType { Value = dataProvider.Supplier?.CompanyName ?? ZString.Empty }
			};
		}

		DeclarationGoodsShipment PopulateGoodsShipment() => new DeclarationGoodsShipment
		{
			GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
			{
				Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
				{
					CargoDescription = new CommodityCargoDescriptionTextType { Value = dataProvider.RepresentativeProductName },
					IntendedUseCode = new CommodityIntendedUseCodeType { Value = dataProvider.CarnetUseCode }
				},
				Packaging = dataProvider.TotalPackQty.IsEmpty && dataProvider.PackType.IsEmpty ? null : new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging
				{
					QuantityQuantity = dataProvider.TotalPackQty.IsEmpty ? null : new PackagingQuantityQuantityType { Value = dataProvider.TotalPackQty },
					TypeCode = dataProvider.PackType.IsEmpty ? null : new PackagingTypeCodeType { Value = dataProvider.PackType }
				}
			},
			Warehouse = dataProvider.BondedAreaCode.IsEmpty ? null : new DeclarationGoodsShipmentWarehouse
			{
				Id = new WarehouseIdentificationIdType { Value = dataProvider.BondedAreaCode }
			}
		};

		DeclarationImporter PopulateImporter()
		{
			return dataProvider.Importer == null ? null : new DeclarationImporter
			{
				Name = dataProvider.Importer.CompanyName.IsEmpty ? null : new ImporterNameTextType { Value = dataProvider.Importer.CompanyName },
				Address = dataProvider.Importer.AddressLine1.IsEmpty && dataProvider.Importer.AddressLine2.IsEmpty ? null : new DeclarationImporterAddress
				{
					Line = new AddressLineTextType { Value = (dataProvider.Importer.AddressLine1 + " " + dataProvider.Importer.AddressLine2).Trim() }
				},
				Communication = dataProvider.Importer.PhoneNumber.IsEmpty ? null : new DeclarationImporterCommunication
				{
					Id = new CommunicationIdentificationIdType { Value = dataProvider.Importer.PhoneNumber }
				}
			};
		}

		DeclarationTransportContractDocument PopulateTransportContractDocument()
		{
			return new DeclarationTransportContractDocument
			{
				SplitDeclarationIndicator = dataProvider.HouseBillSplitDeclarationIndicator
			};
		}

		DeclarationUcr PopulateUCR()
		{
			return new DeclarationUcr
			{
				CustomsAssignedReferenceId = new UcrCustomsAssignedReferenceIdType { Value = dataProvider.CargoManagementNo }
			};
		}
	}
}
