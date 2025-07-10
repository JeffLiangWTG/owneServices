using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.DE.Messaging.MessageSchema;
using static Enterprise.Customs.DE.Messaging.MessageSchema.ATLASMessageSchema;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public sealed class CUSCONMessageBuilder : MessageBuilder<GCCONJ>
	{
		public CUSCONMessageBuilder(IImportMessageHeader provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
			header = (ICUSCONHeader)provider.Header;
		}
		readonly IImportMessageHeader provider;
		readonly ICUSCONHeader header;

		ISummaryDeclaration summaryDeclaration => header.SummaryDeclaration;

		ICustomsWarehouse customsWarehouse => header.CustomsWarehouse;

		IInwardProcessing inwardProcessing => header.InwardProcessing;

		protected override GCCONJ GetMessageCore() => new GCCONJ
		{
			MetaData = PopulateMetaData(),
			Header = PopulateHeader(),
			PresentationConfirmer = header.PresentationConfirmer != null ? PopulatePresentationConfirmer() : null,
			ContactPerson = PopulateContactPerson(),
			ArrivalTransportMeans = PopulateArrivalTransportMeans(),
			PreviousAdministrativeReferences = !PreviousAdministrativeReferenceType.IsEmpty ? PopulatePreviousAdministrativeReferences() : null,
			SummaryDeclaration = summaryDeclaration != null ? PopulateSummaryDeclaration() : null,
			CustomsWarehouse = customsWarehouse != null ? PopulateCustomsWarehouse() : null,
			InwardProcessing = inwardProcessing != null ? PopulateInwardProcessing() : null
		};

		GCCONJMetaData PopulateMetaData() => new GCCONJMetaData
		{
			Preparation = new GCCONJMetaDataPreparation
			{
				Date = provider.PreparationDateAndTimeCET.Date,
				Time = provider.PreparationDateAndTimeCET.Time
			},
			InterchangeControlReference = EDIInterchange.InterchangeNumberPlaceHolder,
			MessageReferenceNumber = "1",
			MessageIdentifier = EDIMessage.SendersReferencePlaceHolder,
			MessageGroup = provider.MessageGroup.MapCodeToEnumWithDefault<GCCONJMetaDataMessageGroup>(),
			MessageType = GCCONJMetaDataMessageType.GCCONJ,
			InterchangeSender = new GCCONJMetaDataInterchangeSender
			{
				Identification = new GCCONJMetaDataInterchangeSenderIdentification
				{
					ReferenceNumber = provider.InterchangeSender.EoriNumber.LeftOrNull(EoriCodeMaxLength),
					SubsidiaryNumber = provider.InterchangeSender.EoriBranchSuffix.LeftOrNull(EoriBranchCodeMaxLength)
				}
			},
			InterchangeRecipient = new GCCONJMetaDataInterchangeRecipient
			{
				Identification = new GCCONJMetaDataInterchangeRecipientIdentification
				{
					ReferenceNumber = provider.InterchangeRecipientID.LeftOrNull(InterchangeRecipientReferenceNumberMaxLength)
				}
			}
		};

		GCCONJHeader PopulateHeader() => new GCCONJHeader
		{
			MessageVersion = "J.1.0",
			TemporaryReferenceNumber = header.TemporaryReferenceNumber.ValueOrNullIfEmpty(),
			LRN = header.LocalReferenceNumber.LeftOrNull(LRNMaxLength),
			GoodsLocation = header.GoodsLocation.ValueOrNullIfEmpty(),
			AuthorisationNumber = provider.AuthorisationNumber
		};

		GCCONJPresentationConfirmer PopulatePresentationConfirmer() => new GCCONJPresentationConfirmer
		{
			Identification = new GCCONJPresentationConfirmerIdentification
			{
				ReferenceNumber = header.PresentationConfirmer.EoriNumber,
				SubsidiaryNumber = header.PresentationConfirmer.EoriBranchSuffix.ValueOrNullIfEmpty()
			}
		};

		GCCONJContactPerson PopulateContactPerson() => new GCCONJContactPerson
		{
			Name = header.ContactPerson.PersonName.LeftOrNull(ATLASMessageSchema.ContactNameMaxLength),
			Position = header.ContactPerson.Position.LeftOrNull(ATLASMessageSchema.ContactPositionMaxLength),
			PhoneNumber = header.ContactPerson.PhoneNumber.LeftOrNull(ATLASMessageSchema.ContactPhoneNumberMaxLength),
			MailAddress = header.ContactPerson.MailAddress.LeftOrNull(ATLASMessageSchema.ContactEmailAdressMaxLength)
		};

		GCCONJArrivalTransportMeans PopulateArrivalTransportMeans()
		{
			var identity = header.ArrivalTransportMeansIdentity;
			return identity.IsEmpty() ? null : new GCCONJArrivalTransportMeans { Identity = identity };
		}

		GCCONJPreviousAdministrativeReferences PopulatePreviousAdministrativeReferences()
		{
			var previousReferenceNumber = header.PreviousAdministrativeReferenceNumber;

			var result = new GCCONJPreviousAdministrativeReferences
			{
				Type = PreviousAdministrativeReferenceType.ToString().MapCodeToEnumWithDefaultAndOptionalItemPrefix<GCCONJPreviousAdministrativeReferencesType>()
			};
			if (!PreviousAdministrativeReferenceType.In(new PreviousProcedureTypeList().GetAllCodesZString()) && !previousReferenceNumber.IsEmpty())
			{
				result.PreviousAdministrativeReference = new GCCONJPreviousAdministrativeReferencesPreviousAdministrativeReference { ReferenceNumber = previousReferenceNumber };
			}
			return result;
		}

		GCCONJSummaryDeclaration PopulateSummaryDeclaration() => new GCCONJSummaryDeclaration
		{
			IdentificationIndicator = SummaryDeclarationIdentificationIndicatorIsREG ? GCCONJSummaryDeclarationIdentificationIndicator.REG : GCCONJSummaryDeclarationIdentificationIndicator.AWB,
			GoodsItem = summaryDeclaration.GoodsItems.Select(x => PopulateSummaryDeclarationGoodsitem(x)).ToArray()
		};

		GCCONJSummaryDeclarationGoodsItem PopulateSummaryDeclarationGoodsitem(ISummaryDeclarationGoodsItem goodsItem) => new GCCONJSummaryDeclarationGoodsItem
		{
			Quantity = goodsItem.Quantity.ToString(),
			IdentificationByKey = !SummaryDeclarationIdentificationIndicatorIsREG ? PopulateIdentificationByKey(goodsItem) : null,
			IdentificationByRegistration = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<GCCONJSummaryDeclarationGoodsItemIdentificationByRegistration>(
				summaryDeclaration.IdentificationIndicator,
				goodsItem.IdentificationByRegistrationReferencedRegistrationNumber,
				(x) => x.ReferencedSequenceNumber = goodsItem.IdentificationByRegistrationReferencedSequenceNumber.ToString())
		};

		GCCONJSummaryDeclarationGoodsItemIdentificationByKey PopulateIdentificationByKey(ISummaryDeclarationGoodsItem goodsItem) => new GCCONJSummaryDeclarationGoodsItemIdentificationByKey
		{
			Kind = SummaryDeclarationIdentificationIndicatorIsAWB ? GCCONJSummaryDeclarationGoodsItemIdentificationByKeyKind.AWB : GCCONJSummaryDeclarationGoodsItemIdentificationByKeyKind.ULD,
			Number = goodsItem.IdentificationByKeyNumber,
			Custodian = new GCCONJSummaryDeclarationGoodsItemIdentificationByKeyCustodian
			{
				Identification = new GCCONJSummaryDeclarationGoodsItemIdentificationByKeyCustodianIdentification { ReferenceNumber = goodsItem.IdentificationByKeyCustodianIdentifier.LeftOrNull(ATLASMessageSchema.EoriCodeMaxLength) }
			},
		};

		GCCONJCustomsWarehouse PopulateCustomsWarehouse() => new GCCONJCustomsWarehouse
		{
			SequenceNumber = "1",
			GoodsItemQuantity = customsWarehouse.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = new GCCONJCustomsWarehouseCustomsAuthorisation
			{
				WarehouseOwner = customsWarehouse.WarehouseOwnerIdentifier.LeftOrNull(ATLASMessageSchema.WarehouseOwnerMaxLength),
			},
			LRN = customsWarehouse.LocalReferenceNumber.ValueOrNullIfEmpty(),
			GoodsItem = customsWarehouse.GoodsItems.Select((x, i) => PopulateCustomsWarehouseGoodsItem(x, i + 1)).ToArray(),
		};

		GCCONJCustomsWarehouseGoodsItem PopulateCustomsWarehouseGoodsItem(ICustomsWarehouseGoodsItem goodsItem, int counter)
		{
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<GCCONJCustomsWarehouseGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
				{
					x.SequenceNumber = counter.ToString();
					x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
					x.AccessViaAtlasFlag = goodsItem.AccessViaATLASFlag.MapBoolToJN();
					x.CommodityCode = goodsItem.CommodityCode;
					x.UsualProcessingFlag = goodsItem.UsualProcessingFlag.MapBoolToJN();
					x.Complement = goodsItem.Complement.ValueOrNullIfEmpty();
					x.CommercialAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<GCCONJCustomsWarehouseGoodsItemCommercialAmount>(goodsItem.CommercialAmount);
					x.DebitAmount = CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonAmount<GCCONJCustomsWarehouseGoodsItemDebitAmount>(goodsItem.DebitAmount);
				});
		}

		GCCONJInwardProcessing PopulateInwardProcessing() => new GCCONJInwardProcessing
		{
			SequenceNumber = "1",
			GoodsItemQuantity = inwardProcessing.GoodsItemQuantity.ToString(),
			CustomsAuthorisation = !inwardProcessing.ProcessingOwnerIdentifier.IsEmpty() ? PopulateCustomsAuthorisation() : null,
			SimplifiedGrantAuthorisationFlag = inwardProcessing.SimplifiedGrantAuthorisationFlag.MapBoolToJN(),
			MonitoringCustomsOffice = inwardProcessing.SimplifiedGrantAuthorisationFlag ? PopulateMonitoringCustomsOffice() : null,
			GoodsItem = inwardProcessing.GoodsItems.Select((x, i) => PopulateInwardProcessingGoodsItem(x, i + 1)).ToArray(),
		};

		GCCONJInwardProcessingCustomsAuthorisation PopulateCustomsAuthorisation() => new GCCONJInwardProcessingCustomsAuthorisation { ProcessingOwner = inwardProcessing.ProcessingOwnerIdentifier.LeftOrNull(ATLASMessageSchema.ProcessingOwnerMaxLength) };

		GCCONJInwardProcessingMonitoringCustomsOffice PopulateMonitoringCustomsOffice() => new GCCONJInwardProcessingMonitoringCustomsOffice
		{
			Identification = new GCCONJInwardProcessingMonitoringCustomsOfficeIdentification
			{
				ReferenceNumber = inwardProcessing.MonitoringCustomsOfficeReferenceNumber
			},
		};

		GCCONJInwardProcessingGoodsItem PopulateInwardProcessingGoodsItem(IInwardProcessingGoodsItem goodsItem, int counter)
		{
			return CargoWise.Customs.DE.MessageContracts.MessageBuilderExtensions.CreateCommonIdentificationByRegistration<GCCONJInwardProcessingGoodsItem>(goodsItem.ReferencedRegistrationNumber, x =>
			{
				x.SequenceNumber = counter.ToString();
				x.ReferencedSequenceNumber = goodsItem.ReferencedSequenceNumber.ToString();
				x.AccessViaAtlasFlag = goodsItem.AccessViaAtlasFlag.MapBoolToJN();
				x.GoodsRelatedInformation = goodsItem.GoodsRelatedInformation;
			});
		}

		ZString PreviousAdministrativeReferenceType => header.PreviousAdministrativeReferenceType;

		ZBool SummaryDeclarationIdentificationIndicatorIsREG => summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG;

		ZBool SummaryDeclarationIdentificationIndicatorIsAWB => summaryDeclaration.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.AWB;
	}
}
