using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GW;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5GW)]
	public class GOVCBR5GWMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5GWHeader dataProvider;
		public GOVCBR5GWMessageBuilder(IImport5GWHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				TypeCode = PopulateTypeCode(),
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Reason = PopulateReason(),
				AdditionalInformation = PopulateAdditionalInformation(),
				GoodsShipment = PopulateGoodsShipment(),
				Submitter = PopulateSubmitter()
			};
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType
			{
				Value = dataProvider.ApplicationNumber
			};
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5GW };
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationReasonTextType PopulateReason()
		{
			return dataProvider.ApplicationReason.IsEmpty ? null : new DeclarationReasonTextType { Value = dataProvider.ApplicationReason };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				BeginningDateTime = dataProvider.StartDateTime.IsValid ? dataProvider.StartDateTime.ToString(DateFormatType.DateTime) : string.Empty,
				EndingDateTime = dataProvider.EndDateTime.IsValid ? dataProvider.EndDateTime.ToString(DateFormatType.DateTime) : null
			};
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			var entryLines = new Collection<DeclarationGoodsShipment>();
			foreach (var entry in dataProvider.Entries)
			{
				var item = new DeclarationGoodsShipment
				{
					AdditionalDocument = new DeclarationGoodsShipmentAdditionalDocument
					{
						Id = new AdditionalDocumentIdentificationIdType
						{
							Value = entry.ReferenceNumber
						},
						TypeCode = new AdditionalDocumentTypeCodeType
						{
							Value = ReferenceNumberTypeList.GetMappedCustomsCode(entry.ReferenceNumberType)
						}
					},
					GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
					{
						Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
						{
							CargoDescription = new CommodityCargoDescriptionTextType { Value = entry.HSDescription },
							CountQuantity = entry.TotalPackQty == 0 ? null : new CommodityCountQuantityType { Value = entry.TotalPackQty },
							SizeMeasure = entry.TotalGrossWeightInKG == 0 ? null : new CommoditySizeMeasureType { Value = entry.TotalGrossWeightInKG },
							DutyTaxFee = entry.TotalCustomsValueInUSD == 0 ? null :
							new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
							{
								AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType
								{
									Value = entry.TotalCustomsValueInUSD
								}
							}
						},
						Payer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemPayer
						{
							Name = new PayerNameTextType
							{
								Value = entry.PayerCompanyName
							}
						},
					},
					Warehouse = new DeclarationGoodsShipmentWarehouse
					{
						Id = new WarehouseIdentificationIdType
						{
							Value = entry.BondedAreaCode
						}
					}
				};
				entryLines.Add(item);
			}
			return entryLines;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType
				{
					Value = dataProvider.Declarant?.CompanyName ?? ZString.Empty
				},
				Contact = new DeclarationSubmitterContact
				{
					Name = new ContactNameTextType
					{
						Value = dataProvider.Declarant?.RepresentativeName ?? ZString.Empty
					}
				},
				Communication = new DeclarationSubmitterCommunication
				{
					Id = new CommunicationIdentificationIdType
					{
						Value = dataProvider.Declarant?.PhoneNumber ?? ZString.Empty
					}
				}
			};
		}
	}
}
