using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AC;
using CargoWise.Customs.KR.MessageDefinitions.KCSDS;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5AC)]
	public class GOVCBR5ACMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IExport5ACHeader dataProvider;
		public GOVCBR5ACMessageBuilder(IExport5ACHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				DeclarationOfficeId = PopulateDeclarationOfficeID(),
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				LoadingListQuantity = PopulateLoadingListQuantity(),
				TypeCode = PopulateTypeCode(),
				Reason = PopulateReason(),
				AdditionalInformation = PopulateAdditionalInformation(),
				GoodsShipment = PopulateGoodsShipment(),
				Submitter = PopulateSubmitter()
			};
		}

		DeclarationDeclarationOfficeIdType PopulateDeclarationOfficeID()
		{
			return new DeclarationDeclarationOfficeIdType { Value = dataProvider.DeclarationCustomsOffice + dataProvider.DeclarationCustomsDivision };
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType { Value = dataProvider.ApplicationNumber };
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationLoadingListQuantityType PopulateLoadingListQuantity()
		{
			return new DeclarationLoadingListQuantityType { Value = dataProvider.Entries?.Count() ?? ZDecimal.Zero };
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType { Value = GOVCBR + ElectronicDocumentTypeList.Codes._5AC };
		}

		DeclarationReasonTextType PopulateReason()
		{
			return new DeclarationReasonTextType { Value = dataProvider.ApplicationReason };
		}

		DeclarationAdditionalInformation PopulateAdditionalInformation()
		{
			return new DeclarationAdditionalInformation
			{
				BeginningDateTime = dataProvider.StartDateTime.IsValid ? dataProvider.StartDateTime.ToString(DateFormatType.DateTimeNoSecond) : string.Empty,
				EndingDateTime = dataProvider.EndDateTime.IsValid ? dataProvider.EndDateTime.ToString(DateFormatType.DateTimeNoSecond) : null
			};
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			if (dataProvider.Entries == null)
			{
				return null;
			}

			var goodShipmentLines = new Collection<DeclarationGoodsShipment>();
			foreach (var goodShipmentLine in dataProvider.Entries)
			{
				var item = new DeclarationGoodsShipment
				{
					AdditionalDocument = new DeclarationGoodsShipmentAdditionalDocument
					{
						Id = new AdditionalDocumentIdentificationIdType { Value = goodShipmentLine.ReferenceNumber }
					},
					Exporter = new DeclarationGoodsShipmentExporter
					{
						Name = new ExporterNameTextType { Value = goodShipmentLine.SupplierName }
					},
					GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
					{
						Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
						{
							CountQuantity = new CommodityCountQuantityType { Value = goodShipmentLine.TotalPackQty },
							SizeMeasure = new CommoditySizeMeasureType { Value = goodShipmentLine.TotalGrossWeightInKG },
							DutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
							{
								AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType { Value = goodShipmentLine.TotalCustomsValueInUSD }
							}
						}
					}
				};
				goodShipmentLines.Add(item);
			}
			return goodShipmentLines;
		}

		DeclarationSubmitter PopulateSubmitter()
		{
			return new DeclarationSubmitter
			{
				Name = new SubmitterNameTextType { Value = dataProvider.Declarant.CompanyName },
				Contact = new DeclarationSubmitterContact
				{
					Name = new ContactNameTextType { Value = dataProvider.Declarant.RepresentativeName }
				},
				Communication = new DeclarationSubmitterCommunication
				{
					Id = new CommunicationIdentificationIdType { Value = dataProvider.Declarant.PhoneNumber }
				}
			};
		}
	}
}





