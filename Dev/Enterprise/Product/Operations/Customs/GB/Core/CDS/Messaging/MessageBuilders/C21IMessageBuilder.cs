using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class C21IMessageBuilder : H5MessageBuilder
	{
		public C21IMessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateTotalPackageQuantity()
		{
			if (TotalPackageQuantity > 0 && IsMovementThroughAnInventoryLinkingLocation())
			{
				decMessage.TotalPackageQuantity = new DeclarationTotalPackageQuantityType
				{
					Value = TotalPackageQuantity
				};
			}
		}

		protected override void PopulateExporter()
		{
			if (ExporterNameAndAddress != null)
			{
				var exporter = new DeclarationExporter
				{
					Name = new ExporterNameTextType { Value = ExporterNameAndAddress.Name },
					Address = GetIOrgAddress<DeclarationExporterAddress>(ExporterNameAndAddress)
				};
				decMessage.Exporter = exporter;
			}
		}

		protected override void PopulateAgent()
		{
			// Note: below is for the implementation of note 12L on element 3/20 and 3/21
			var procedureCodes = GoodsItems.SelectMany(item => item.GovernmentProcedures.Select(p => GetProcedureCode(p))).Distinct().ToArray();
			if (!procedureCodes.Contains("0009") && !procedureCodes.Contains("0019"))
			{
				base.PopulateAgent();
			}
		}

		protected override void PopulateDomesticDutyTaxParties()
		{
			PopulateDomesticDutyTaxPartiesBase();
		}

		protected override void PopulateDomesticDutyTaxParties(IGovernmentAgencyGoodsItem goodsItem)
		{
			PopulateDomesticDutyTaxPartiesBase(goodsItem);
		}

		protected override void PopulateInvoiceAmount()
		{
		}

		protected override void PopulateAEOMutualRecognitionParties()
		{
		}

		protected override void PopulateAEOMutualRecognitionParties(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateTradeTerms()
		{
		}

		protected override void PopulateDutyRegimeCode(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityDutyTaxFee result, IDutyTaxFee dutyTaxFee)
		{
		}

		protected override void PopulateCurrencyExchanges()
		{
		}

		protected override void PopulateCustomsValuations()
		{
		}

		protected override void PopulateCustomsValuations(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateValuationAdjustmentAdditionCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateDestination()
		{
		}

		protected override void PopulateDestinationCountryCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateInvoiceLineItemChargeAmount(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
			if (!giCommodity.InvoiceLineItemCharge.Currency.IsEmpty)
			{
				base.PopulateInvoiceLineItemChargeAmount(commodity, giCommodity);
			}
		}

		protected override void PopulateExportCountryID()
		{
		}

		protected override void PopulateExportCountryID(IGovernmentAgencyGoodsItem goodsItem)
		{
			var procedure = goodsItem.GovernmentProcedures.FirstOrDefault();
			var procedureCode = GetProcedureCode(procedure);
			var mustHaveExportCountryID = procedureCode != "0008" && procedureCode != "0009";
			if (mustHaveExportCountryID)
			{
				base.PopulateExportCountryID(goodsItem);
			}
		}

		string GetProcedureCode(IGovernmentProcedure p) =>
			!string.IsNullOrEmpty(p?.CurrentCode) && !string.IsNullOrEmpty(p?.PreviousCode) ? $"{p.CurrentCode}{p.PreviousCode}" : string.Empty;

		protected override void PopulateOrigins(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateGoodsMeasure(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected override void PopulateMarksNumbersID(DeclarationGoodsShipmentGovernmentAgencyGoodsItemPackaging goodsItemPackaging, IPackaging packaging)
		{
			goodsItemPackaging.MarksNumbersID = new PackagingMarksNumbersIDType { Value = packaging.MarksNumbersID.StripNewlineCharacters(512) };
		}

		protected override void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected override void PopulateArrivalTransportMeans(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			PopulateArrivalTransportMeansBase(consignment, consignmentData);
		}

		protected override void PopulateBorderTransportMeans()
		{
			if (BorderTransportMeans != null)
			{
				var borderTransportMeans = BorderTransportMeans == null ? new DeclarationBorderTransportMeans() : new DeclarationBorderTransportMeans
				{
					IdentificationTypeCode = new BorderTransportMeansIdentificationTypeCodeType { Value = BorderTransportMeans.IdentificationTypeCode },
					ModeCode = new BorderTransportMeansModeCodeType { Value = BorderTransportMeans.ModeCode },
				};
				decMessage.BorderTransportMeans = borderTransportMeans;
			}
		}

		protected override void PopulateObligationGuarantees()
		{
			PopulateObligationGuaranteesBase();
		}

		protected override void PopulateTransactionNatureCode()
		{
		}

		protected override void PopulateTransactionNatureCode(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateStatisticalValue(IGovernmentAgencyGoodsItem goodsItem)
		{
		}

		protected override void PopulateAcceptanceDateTime()
		{
		}

		protected override void PopulateSpecificCircumstancesCodeCode()
		{
		}

		protected override void PopulateWriteOff(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
		}
	}
}
