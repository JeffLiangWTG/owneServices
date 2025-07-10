using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RequestForPermitMeatLineMessageBuilder : RequestForPermitLineMessageBuilder
	{
		public RequestForPermitMeatLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
			: base(sANCRT, messageTypeToSend)
		{
		}

		protected override void GenerateBatchCode(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_BatchCode.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateGIN(group11.GIN.InstantiateAChildAndAddItToChildrenCollection(),
					IdentityNumberQualifierList.BatchNumber,
					invoiceLine.QuarantineExDocLine.QL_BatchCode);
			}
		}

		protected override void GenerateLabelApprovalIndicator(SegmentGroup11 group11)
		{
			if (invoiceLine.QuarantineExDocLine.QL_LabelApprovalIndicator)
			{
				EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.LabelApprovalIndicatorQualifier);
			}
		}

		protected override void GenerateUngradedProductIndicator(SegmentGroup11 group11)
		{
			if (invoiceLine.QuarantineExDocLine.QL_UngradedProductIndicator)
			{
				EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.UngradedProductIndicatorQualifier);
			}
		}

		protected override void GenerateHalalProductIndicator(SegmentGroup11 group11)
		{
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.HalalProductIndicatorQualifier, invoiceLine.QuarantineExDocLine.QL_HalalProductIndicator);
		}

		protected override void GenerateLineImperialNetWeight(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_ImperialNetWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
						MeasurementPurposeQualifierList.ItemWeight,
						PropertyMeasuredCodedList.NetWeight,
						invoiceLine.QuarantineExDocLine.QL_ImperialNetWeightUnit,
						invoiceLine.QuarantineExDocLine.QL_ImperialNetWeight.ToStringTrimZeros());
			}
		}

		protected override void GenerateBeefVealWeight(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_BeefVealWeightAmount.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
						MeasurementPurposeQualifierList.ItemWeight,
						PropertyMeasuredCodedList.ChargeableWeight,
						EXDOCMetricWeightUnitCodes.Codes.Kilogram,
						invoiceLine.QuarantineExDocLine.QL_BeefVealWeightAmount.ToStringTrimZeros());
			}
		}

		protected override void GenerateChemicalLeanPercentage(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_ChemicalLeanPercentage.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
						MeasurementPurposeQualifierList.Chemistry,
						Percentage,
						invoiceLine.QuarantineExDocLine.QL_ChemicalLeanPercentage.ToString());
			}
		}

		protected override void GenerateDominantAndAdditionalProducts(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_DominantProduct.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(),
						ProductIdFunctionQualifierList.ProductIdentification,
						invoiceLine.QuarantineExDocLine.QL_DominantProduct,
						ItemNumberTypeCodedList.CommodityGrouping);
				if (!invoiceLine.QuarantineExDocLine.QL_AdditionalProducts.IsEmpty)
				{
					EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(),
							ProductIdFunctionQualifierList.ProductIdentification,
							invoiceLine.QuarantineExDocLine.QL_AdditionalProducts,
							ItemNumberTypeCodedList.StandardGroupOfProductsMixedAssortment);
				}
			}
		}

		protected override void GenerateRFPLineItemDescription(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_MeatInspectionDescription.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
						Inspection,
						invoiceLine.QuarantineExDocLine.QL_MeatInspectionDescription.Trim());
			}
		}

		protected override void GenerateProductQualityQualification(SegmentGroup11 group11)
		{
		}

		protected override void GenerateProductLocationQualification(SegmentGroup11 group11)
		{
		}

		protected override void GenerateAMLCQuotaApprovalReference(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_QuotaApprovalRef.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						ReferenceQualifierList.ReleaseNumber,
						invoiceLine.QuarantineExDocLine.QL_QuotaApprovalRef);
			}
		}

		protected override void GenerateAMLCPerformanceExporterNumber(SegmentGroup13 group13)
		{
			if (!invoiceLine.EXDOCAMLCPerformanceNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePNA(group13.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.Exporter,
					invoiceLine.EXDOCAMLCPerformanceNumber);
			}
		}

		protected override void GenerateTreatment(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected override void GenerateTreatmentInformation(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected override void GenerateTreatmentStartDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected override void GenerateTreatmentEndDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected override void GenerateProductSourceState(SegmentGroup11 group11)
		{
			if (!invoiceLine.JI_AUState.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateLOC(group11.LOC.InstantiateAChildAndAddItToChildrenCollection(),
					PlaceLocationQualifierList.MutuallyDefined,
					invoiceLine.JI_AUState);
			}
		}

		public const string Inspection = "IN";
	}
}
