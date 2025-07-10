using System.Collections.ObjectModel;
using CargoWise.Customs.KR.MessageDefinitions.DS;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5TM;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging
{
	[CodeAlive("Soon to be used")]
	[MessageType(ElectronicDocumentTypeList.Codes._5TM)]
	public class GOVCBR5TMMessageBuilder : MessageBuilder<Declaration>
	{
		readonly IImport5TMHeader dataProvider;
		public GOVCBR5TMMessageBuilder(IImport5TMHeader dataProvider)
		{
			this.dataProvider = dataProvider;
		}

		public override Declaration GenerateMessage()
		{
			return new Declaration
			{
				Id = PopulateID(),
				IssueDateTime = PopulateIssueDateTime(),
				Payer = PopulatePayer(),
				GoodsShipment = PopulateGoodsShipment(),
				TypeCode = PopulateTypeCode()
			};
		}

		DeclarationIdentificationIdType PopulateID()
		{
			return new DeclarationIdentificationIdType
			{
				Value = dataProvider.ImportDeclarationNumber
			};
		}

		ZString PopulateIssueDateTime()
		{
			return ZDate.Today.ToString(DateFormatType.Date);
		}

		DeclarationTypeCodeType PopulateTypeCode()
		{
			return new DeclarationTypeCodeType
			{
				Value = GOVCBR + ElectronicDocumentTypeList.Codes._5TM
			};
		}

		Collection<DeclarationGoodsShipment> PopulateGoodsShipment()
		{
			var entryLines = new Collection<DeclarationGoodsShipment>();
			foreach (var entryLine in dataProvider.EntryLines)
			{
				var item = new DeclarationGoodsShipment
				{
					SequenceNumeric = entryLine.EntryLineNo,

					GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem
					{
						Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity
						{
							CargoDescription = new CommodityCargoDescriptionTextType
							{
								Value = entryLine.HSDescription
							},
							Classification = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityClassification
							{
								Id = new ClassificationIdentificationIdType
								{
									Value = entryLine.HSCode
								}
							},
							DutyTaxFee = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee
							{
								AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType
								{
									Value = entryLine.ValueForVAT
								},
								Payment = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFeePayment
								{
									TaxAssessedAmount = new PaymentTaxAssessedAmountType
									{
										Value = entryLine.VAT
									}
								}
							},
							GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure
							{
								NetNetWeightMeasure = new GoodsMeasureNetNetWeightMeasureType
								{
									Value = entryLine.NetWeightKG,
									KcsUnitCode = DefaultWeightUnit
								}
							}
						}
					}
				};
				entryLines.Add(item);
			}
			return entryLines;
		}

		DeclarationPayer PopulatePayer()
		{
			if (dataProvider.Payer != null)
			{
				var payerBusinessNumberType = dataProvider.Payer.IsIndividual ? IdentificationType.KoreanRegNoForResident : IdentificationType.CorporationCode;
				var exporterIdentificationID = dataProvider.Payer.GetRegistrationTypeAndNumber(payerBusinessNumberType)?.Number ?? ZString.Empty;
				if (!exporterIdentificationID.IsEmpty)
				{
					return new DeclarationPayer
					{
						Id = new PayerIdentificationIdType
						{
							Value = exporterIdentificationID
						}
					};
				}
			}
			return null;
		}
	}
}
