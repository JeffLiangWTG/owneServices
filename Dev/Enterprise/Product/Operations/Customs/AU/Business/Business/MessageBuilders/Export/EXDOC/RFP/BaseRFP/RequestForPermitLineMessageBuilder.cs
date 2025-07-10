using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class RequestForPermitLineMessageBuilder
	{
		protected RequestForPermitLineMessageBuilder(SANCRTMessage sANCRT, string messageTypeToSend)
		{
			this.sANCRT = sANCRT;
			this.MessageTypeToSend = messageTypeToSend;
		}

		public void GenerateLine(int lineNumber, JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
			GenerateGroup11AndChildren(lineNumber);
		}

		QuarantineExDocHeader Header
		{
			get { return fHeader ?? (fHeader = invoiceLine.InvoiceHeader.QuarantineExDocHeader); }
		}
		QuarantineExDocHeader fHeader;

		void GenerateGroup11AndChildren(int lineNumber)
		{
			SegmentGroup11 group11 = sANCRT.Group11.InstantiateAChildAndAddItToChildrenCollection();
			GenerateLineIdentifier(group11, lineNumber);
			GenerateLineNetQuantity(group11);
			GenerateLineImperialNetWeight(group11);
			GenerateLineMetricGrossWeight(group11);
			GenerateDrainedWeight(group11);
			GeneratePercentageOfMilkProtein(group11);
			GeneratePercentageOfMilkFat(group11);
			GenerateTotalWeightOfMilkProteinInMixtures(group11);
			GenerateTotalWeightOfMilkFatInMixtures(group11);
			GenerateBeefVealWeight(group11);
			GenerateChemicalLeanPercentage(group11);
			if (Header.IsChangeAllowed(EXDOCDataFields.ProductCode))
			{
				GenerateProductCode(group11);
			}

			if (Header.IsChangeAllowed(EXDOCDataFields.CutCode))
			{
				GenerateCutType(group11);
			}

			GenerateEAN13Number(group11);
			GenerateAHECCCode(group11);

			GenerateDominantAndAdditionalProducts(group11);
			GenerateRFPLineItemDescription(group11);
			GenerateExporterDefinedProductDescription(group11);
			GenerateProdCountryOfOrigin(group11);
			GenerateBatchCode(group11);
			GenerateAdditionalProductDescription(group11);
			GenerateCommercialProductDescription(group11);
			GenerateProductCondition(group11);
			GenerateProductPart(group11);
			GenerateProductQualityQualification(group11);
			GenerateProductLocationQualification(group11);
			GenerateNatureOfCommodity(group11);
			GenerateTreatmentType(group11);
			GenerateLabelApprovalNumber(group11);
			GenerateExportSchemeCode(group11);
			GenerateAMLCQuotaApprovalReference(group11);
			GenerateLabelApprovalIndicator(group11);
			GenerateUngradedProductIndicator(group11);
			GenerateHalalProductIndicator(group11);
			if (UniversalReferenceHelper.Errata51Enabled())
			{
				GenerateFinalConsumer(group11);
			}
			GenerateFishWaterIndicator(group11);

			GenerateDurabilityStartDate(group11);
			GenerateDurabilityEndDate(group11);
			GenerateSaltingDate(group11);
			GenerateCatchDates(group11);
			GenerateProductSourceState(group11);
			GenerateAdditionalDeclarationInformation(group11);
			GenerateCodedStatement(group11);
			GenerateFreeTextStatement(group11);
			if (Header.IsChangeAllowed(EXDOCDataFields.GrowerNumber))
			{
				GenerateGrowerNumber(group11);
			}

			GenerateImportAuthorityCode(group11);
			GenerateClientLineItemID(group11);
			GenerateFOBAmount(group11);
			GenerateGroup12AndChildren(group11);
			GenerateGroup13AndChildren(group11);
			GenerateGroup15AndChildren(group11);
			GenerateGroup16AndChildren(group11);
			GenerateGroup18AndChildren(group11);
			GenerateCustomsWeights(group11);
		}

		protected virtual void GenerateCustomsWeights(SegmentGroup11 group11)
		{
		}

		void GenerateLineIdentifier(SegmentGroup11 group11, int lineNumber)
		{
			EXDOCMessageUtilities.PopulateLIN(group11.LIN.InstantiateAChildAndAddItToChildrenCollection(), lineNumber.ToString(CultureInfo.InvariantCulture));
		}

		void GenerateLineNetQuantity(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_NetQuantity.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
						MeasurementPurposeQualifierList.LineItemMeasurement,
						PropertyMeasuredCodedList.ShippedQuantity,
						invoiceLine.QuarantineExDocLine.QL_NetQuantityUnit,
						invoiceLine.QuarantineExDocLine.QL_NetQuantity.ToString(3));
			}
		}

		protected virtual void GenerateLineImperialNetWeight(SegmentGroup11 group11)
		{
		}

		void GenerateLineMetricGrossWeight(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_GrossMetricWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group11.MEA.InstantiateAChildAndAddItToChildrenCollection(),
						MeasurementPurposeQualifierList.ItemWeight,
						PropertyMeasuredCodedList.ItemGrossWeight,
						invoiceLine.QuarantineExDocLine.QL_GrossMetricWeightUnit,
						invoiceLine.QuarantineExDocLine.QL_GrossMetricWeight.ToStringTrimZeros());
			}
		}

		protected virtual void GenerateDrainedWeight(SegmentGroup11 group11)
		{
		}

		protected virtual void GeneratePercentageOfMilkProtein(SegmentGroup11 group11)
		{
		}

		protected virtual void GeneratePercentageOfMilkFat(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateTotalWeightOfMilkProteinInMixtures(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateTotalWeightOfMilkFatInMixtures(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateBeefVealWeight(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateChemicalLeanPercentage(SegmentGroup11 group11)
		{
		}

		void GenerateProductCode(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.ProductCode.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(),
					ProductIdFunctionQualifierList.ProductIdentification,
					invoiceLine.QuarantineExDocLine.ProductCode,
					ItemNumberTypeCodedList.IndustryCommodityCode);
			}
		}

		protected virtual void GenerateCutType(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_CutCode.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(),
					ProductIdFunctionQualifierList.ProductIdentification,
					invoiceLine.QuarantineExDocLine.QL_CutCode,
					ItemNumberTypeCodedList.BuyersPartNumber);
			}
		}

		protected virtual void GenerateFinalConsumer(SegmentGroup11 group11)
		{
			EXDOCMessageUtilities.PopulateATT(group11.ATT.InstantiateAChildAndAddItToChildrenCollection(), EXDOCMessageUtilities.FinalConsumerIndicator, invoiceLine.QuarantineExDocLine.QL_FinalConsumer);
		}

		protected virtual void GenerateEAN13Number(SegmentGroup11 group11)
		{
		}

		void GenerateAHECCCode(SegmentGroup11 group11)
		{
			if (!invoiceLine.TariffNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePIA(group11.PIA.InstantiateAChildAndAddItToChildrenCollection(),
						ProductIdFunctionQualifierList.ProductIdentification,
						invoiceLine.TariffNumber,
						ItemNumberTypeCodedList.HarmonisedSystem);
			}
		}

		protected virtual void GenerateDominantAndAdditionalProducts(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateRFPLineItemDescription(SegmentGroup11 group11)
		{
		}

		void GenerateExporterDefinedProductDescription(SegmentGroup11 group11)
		{
			if (invoiceLine.QuarantineExDocLine.QL_SendHCDesc && !invoiceLine.JI_Description.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
						UserDefinedCertificate,
	invoiceLine.JI_Description.Trim());
			}
		}

		protected virtual void GenerateFishWaterIndicator(SegmentGroup11 group11) { }

		protected virtual void GenerateProdCountryOfOrigin(SegmentGroup11 group11) { }

		protected virtual void GenerateBatchCode(SegmentGroup11 group11) { }

		void GenerateAdditionalProductDescription(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_AddtionalProductDescription.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
						Additional,
						invoiceLine.QuarantineExDocLine.QL_AddtionalProductDescription);
			}
		}

		protected virtual void GenerateCommercialProductDescription(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateProductPart(SegmentGroup11 group11)
		{
			var productPart = invoiceLine.QuarantineExDocLine.QL_ProductPart;

			if (!productPart.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					ProductPartQualifier,
					productPart);
			}
		}

		protected virtual void GenerateProductCondition(SegmentGroup11 group11)
		{
			if (invoiceLine.QuarantineExDocLine.ProductConditions.Count > 0)
			{
				foreach (var productCondition in invoiceLine.QuarantineExDocLine.ProductConditions)
				{
					EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
						ItemCharacteristicCodedList.Condition,
						ProductConditionQualifier,
						productCondition.CY_Code);
				}
			}
		}

		protected virtual void GenerateProductQualityQualification(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_ProductDescriptionQualityQualifier.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					QualityQualifier,
					invoiceLine.QuarantineExDocLine.QL_ProductDescriptionQualityQualifier);
			}
		}

		protected virtual void GenerateProductLocationQualification(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_ProductDescriptionLocationQualifier.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					LocationQualifier,
					invoiceLine.QuarantineExDocLine.QL_ProductDescriptionLocationQualifier);
			}
		}

		void GenerateNatureOfCommodity(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_NatureOfCommodity.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					NatureOfCommodity,
					invoiceLine.QuarantineExDocLine.QL_NatureOfCommodity);
			}
		}

		void GenerateTreatmentType(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_TreatmentType.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group11.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					TreatmentType,
					invoiceLine.QuarantineExDocLine.QL_TreatmentType);
			}
		}

		void GenerateLabelApprovalNumber(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_LabelApprovalNumber.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(),
					ReferenceQualifierList.MarkingLabelReference,
					invoiceLine.QuarantineExDocLine.QL_LabelApprovalNumber);
			}
		}

		protected virtual void GenerateLabelApprovalIndicator(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateUngradedProductIndicator(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateHalalProductIndicator(SegmentGroup11 group11)
		{
		}

		void GenerateExportSchemeCode(SegmentGroup11 group11)
		{
			if (Header.QH_ObtainExportCustomsPermit)
			{
				if (invoiceLine.JI_Drawback)
				{
					EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						ReferenceQualifierList.SchemePlanNumber,
						"DRB");
				}
				if (invoiceLine.JI_MotorVehiclePlan)
				{
					EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						ReferenceQualifierList.SchemePlanNumber,
						"MVP");
				}
				if (invoiceLine.JI_Texco)
				{
					EXDOCMessageUtilities.PopulateRFF(group11.RFF.InstantiateAChildAndAddItToChildrenCollection(),
						ReferenceQualifierList.SchemePlanNumber,
						"TX");
				}
			}
		}

		protected virtual void GenerateAMLCQuotaApprovalReference(SegmentGroup11 group11)
		{
		}

		void GenerateDurabilityStartDate(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_UseByStart.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(),
						DateTimePeriodQualifierList.StartDateTime,
						invoiceLine.QuarantineExDocLine.QL_UseByStart.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
						DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		void GenerateDurabilityEndDate(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_UseByEnd.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group11.DTM.InstantiateAChildAndAddItToChildrenCollection(),
						DateTimePeriodQualifierList.EndDateTime,
						invoiceLine.QuarantineExDocLine.QL_UseByEnd.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
						DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected virtual void GenerateSaltingDate(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateProductSourceState(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateAdditionalDeclarationInformation(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateCodedStatement(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateFreeTextStatement(SegmentGroup11 group11)
		{
		}

		protected virtual void GenerateGrowerNumber(SegmentGroup11 group11)
		{
		}

		void GenerateImportAuthorityCode(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_ImportAuthorityCode.IsEmpty)
			{
				const int MaxImportAuthorityCodeChars = 350;
				const int MaxImportAuthorityCodeElementLength = 70;
				EXDOCMessageUtilities.PopulateFTX(group11,
					TextSubjectQualifierList.GovernmentInformation,
					MaxImportAuthorityCodeChars,
					MaxImportAuthorityCodeElementLength,
					invoiceLine.QuarantineExDocLine.QL_ImportAuthorityCode);
			}
		}

		void GenerateClientLineItemID(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_ClientLineItemID.IsEmpty)
			{
				const int MaxClientLineItemIDChars = 250;
				const int MaxClientLineItemIDElementLength = 70;
				EXDOCMessageUtilities.PopulateFTX(group11,
					TextSubjectQualifierList.LineItem,
					MaxClientLineItemIDChars,
					MaxClientLineItemIDElementLength,
					invoiceLine.QuarantineExDocLine.QL_ClientLineItemID);
			}
		}

		void GenerateFOBAmount(SegmentGroup11 group11)
		{
			if (!invoiceLine.JI_Calc_FOB.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMOA(group11.MOA.InstantiateAChildAndAddItToChildrenCollection(),
						MonetaryAmountTypeQualifierList.FobValue,
						invoiceLine.JI_Calc_FOB);
			}
		}

		void GenerateGroup12AndChildren(SegmentGroup11 group11)
		{
			SegmentGroup12 group12 = group11.Group12.InstantiateAChildAndAddItToChildrenCollection();
			GenerateRequestedCertificateTemplateEndorsement(group12);
			GenerateExtraCertificate(group12);
			if (Header.QH_ObtainExportCustomsPermit &&
					(!invoiceLine.JI_RelatedExportPermitAuthority.IsEmpty ||
					!invoiceLine.JI_RelatedExportPermitDate.IsEmpty ||
					!invoiceLine.JI_RelatedExportPermitNumber.IsEmpty))
			{
				GenerateRelatedExportPermitNumber(group12);
				GenerateRelatedExportPermitDate(group12);
			}
		}

		void GenerateRequestedCertificateTemplateEndorsement(SegmentGroup12 group12)
		{
			ZString hCformat = invoiceLine.QuarantineExDocLine.QL_HCFormatRequested.IsEmpty ? invoiceLine.QuarantineExDocLine.QL_HCFormatAllocated : invoiceLine.QuarantineExDocLine.QL_HCFormatRequested;
			if (!hCformat.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDOC(group12.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.SanitaryCertificate,
					hCformat,
					ZString.Empty,
					DocumentMessageStatusCodedList.DocumentAppliedFor);
			}
		}

		protected virtual void GenerateExtraCertificate(SegmentGroup12 group12)
		{
			foreach (ZString extraCertificate in invoiceLine.QuarantineExDocLine.QL_ExtraCertificate.Split(','))
			{
				if (!extraCertificate.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateDOC(group12.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.SanitaryCertificate,
					extraCertificate,
					ZString.Empty,
					DocumentMessageStatusCodedList.ToBePrinted);
				}
			}
		}

		void GenerateRelatedExportPermitNumber(SegmentGroup12 group12)
		{
			EXDOCMessageUtilities.PopulateDOC(group12.DOC.InstantiateAChildAndAddItToChildrenCollection(),
					DocumentMessageNameCodedList.GoodsControlCertificate,
					invoiceLine.JI_RelatedExportPermitNumber,
					invoiceLine.JI_RelatedExportPermitAuthority);
		}

		void GenerateRelatedExportPermitDate(SegmentGroup12 group12)
		{
			EXDOCMessageUtilities.PopulateDTM(group12.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.DocumentMessageDateTime,
					invoiceLine.JI_RelatedExportPermitDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
		}

		void GenerateGroup13AndChildren(SegmentGroup11 group11)
		{
			SegmentGroup13 group13 = group11.Group13.InstantiateAChildAndAddItToChildrenCollection();
			GenerateAMLCPerformanceExporterNumber(group13);
		}

		protected virtual void GenerateAMLCPerformanceExporterNumber(SegmentGroup13 group13)
		{
		}

		void GenerateGroup15AndChildren(SegmentGroup11 group11)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_OuterPackCount.IsEmpty ||
				invoiceLine.QuarantineExDocLine.QL_OuterPackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				invoiceLine.QuarantineExDocLine.QL_OuterPackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments)
			{
				SegmentGroup15 outerGroup15 = group11.Group15.InstantiateAChildAndAddItToChildrenCollection();
				GenerateOuterPackingDetails(outerGroup15);
				GenerateShippingMarks(outerGroup15);
				GenerateOuterPackageMeasurement(outerGroup15);
			}

			if (!invoiceLine.QuarantineExDocLine.QL_IntermediatePackCount.IsEmpty ||
				invoiceLine.QuarantineExDocLine.QL_IntermediatePackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				invoiceLine.QuarantineExDocLine.QL_IntermediatePackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments)
			{
				SegmentGroup15 intermediateGroup15 = group11.Group15.InstantiateAChildAndAddItToChildrenCollection();
				GenerateIntermediatePackingDetails(intermediateGroup15);
				GenerateIntermediatePackageMeasurement(intermediateGroup15);
			}

			if (!invoiceLine.QuarantineExDocLine.QL_InnerPackCount.IsEmpty ||
				invoiceLine.QuarantineExDocLine.QL_InnerPackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				invoiceLine.QuarantineExDocLine.QL_InnerPackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments)
			{
				SegmentGroup15 innerGroup15 = group11.Group15.InstantiateAChildAndAddItToChildrenCollection();
				GenerateInnerPackingDetails(innerGroup15);
				GenerateInnerPackageMeasurement(innerGroup15);
			}
		}

		void GenerateOuterPackingDetails(SegmentGroup15 group15)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_OuterPackCount.IsEmpty ||
				invoiceLine.QuarantineExDocLine.QL_OuterPackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				invoiceLine.QuarantineExDocLine.QL_OuterPackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments)
			{
				EXDOCMessageUtilities.PopulatePAC(group15.PAC.InstantiateAChildAndAddItToChildrenCollection(),
					invoiceLine.QuarantineExDocLine.QL_OuterPackCount.ToString(),
					PackagingLevelCodedList.Outer,
					invoiceLine.QuarantineExDocLine.QL_OuterPackType);
			}
		}

		void GenerateOuterPackageMeasurement(SegmentGroup15 group15)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_OuterPackWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group15.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Package,
					PropertyMeasuredCodedList.NetWeight,
					MeasurementSignificanceCodedList.GetFromString(invoiceLine.QuarantineExDocLine.QL_OuterPackAccuracy),
					invoiceLine.QuarantineExDocLine.QL_OuterPackWeightUnit,
					invoiceLine.QuarantineExDocLine.QL_OuterPackWeight.ToString(3));
			}
		}

		void GenerateIntermediatePackingDetails(SegmentGroup15 group15)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_IntermediatePackCount.IsEmpty ||
				invoiceLine.QuarantineExDocLine.QL_IntermediatePackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				invoiceLine.QuarantineExDocLine.QL_IntermediatePackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments)
			{
				EXDOCMessageUtilities.PopulatePAC(group15.PAC.InstantiateAChildAndAddItToChildrenCollection(),
					invoiceLine.QuarantineExDocLine.QL_IntermediatePackCount.ToString(),
					PackagingLevelCodedList.Intermediate,
					invoiceLine.QuarantineExDocLine.QL_IntermediatePackType);
			}
		}

		void GenerateIntermediatePackageMeasurement(SegmentGroup15 group15)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_IntermediatePackWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group15.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Package,
					PropertyMeasuredCodedList.NetWeight,
					MeasurementSignificanceCodedList.GetFromString(invoiceLine.QuarantineExDocLine.QL_IntermediatePackAccuracy),
					invoiceLine.QuarantineExDocLine.QL_IntermediatePackWeightUnit,
					invoiceLine.QuarantineExDocLine.QL_IntermediatePackWeight.ToString(3));
			}
		}

		void GenerateInnerPackingDetails(SegmentGroup15 group15)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_InnerPackCount.IsEmpty ||
				invoiceLine.QuarantineExDocLine.QL_InnerPackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				invoiceLine.QuarantineExDocLine.QL_InnerPackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments)
			{
				EXDOCMessageUtilities.PopulatePAC(group15.PAC.InstantiateAChildAndAddItToChildrenCollection(),
						invoiceLine.QuarantineExDocLine.QL_InnerPackCount.ToString(),
						PackagingLevelCodedList.Inner,
						invoiceLine.QuarantineExDocLine.QL_InnerPackType);
			}
		}

		void GenerateShippingMarks(SegmentGroup15 group15)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_EffectiveShippingMarks.IsEmpty)
			{
				EXDOCMessageUtilities.PopulatePCI(group15.PCI.InstantiateAChildAndAddItToChildrenCollection(),
	invoiceLine.QuarantineExDocLine.QL_EffectiveShippingMarks);
			}
		}

		void GenerateInnerPackageMeasurement(SegmentGroup15 group15)
		{
			if (!invoiceLine.QuarantineExDocLine.QL_InnerPackWeight.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group15.MEA.InstantiateAChildAndAddItToChildrenCollection(),
						MeasurementPurposeQualifierList.Package,
						PropertyMeasuredCodedList.NetWeight,
						MeasurementSignificanceCodedList.GetFromString(invoiceLine.QuarantineExDocLine.QL_InnerPackAccuracy),
						invoiceLine.QuarantineExDocLine.QL_InnerPackWeightUnit,
						invoiceLine.QuarantineExDocLine.QL_InnerPackWeight.ToString(3));
			}
		}

		void GenerateGroup16AndChildren(SegmentGroup11 group11)
		{
			SegmentGroup16 group16;
			foreach (CusContainerInvoiceLinePivot pivot in invoiceLine.ContainersPivot)
			{
				group16 = group11.Group16.InstantiateAChildAndAddItToChildrenCollection();
				GenerateContainer(group16, pivot);
				if (!pivot.C2_GrossWeight.IsEmpty || !pivot.C2_NetWeight.IsEmpty)
				{
					GenerateIMA1NetWeight(group16, pivot);
					GenerateIMA1GrossWeight(group16, pivot);
					GenerateIMA1ProductDescription(group16);
					GenerateIMA1SerialNumber(group16);
					GenerateIMA1InvoiceNumber(group16);
					GenerateIMA1InvoiceDate(group16);
					GenerateIMA1QuotaYear(group16);
				}
				GenerateGroup17(group16, pivot);
			}
		}

		void GenerateContainer(SegmentGroup16 group16, CusContainerInvoiceLinePivot pivot)
		{
			var containerNo = pivot.Container != null ? pivot.Container.CO_ContainerNumber.Trim() : ZString.Empty;
			if (!containerNo.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateEQD(group16.EQD.InstantiateAChildAndAddItToChildrenCollection(),
					EquipmentQualifierList.Container,
					containerNo);
			}
		}

		protected virtual void GenerateIMA1NetWeight(SegmentGroup16 group16, CusContainerInvoiceLinePivot pivot)
		{
		}

		protected virtual void GenerateIMA1GrossWeight(SegmentGroup16 group16, CusContainerInvoiceLinePivot pivot)
		{
		}

		protected virtual void GenerateIMA1ProductDescription(SegmentGroup16 group16)
		{
		}

		protected virtual void GenerateIMA1SerialNumber(SegmentGroup16 group16)
		{
		}

		protected virtual void GenerateIMA1InvoiceNumber(SegmentGroup16 group16)
		{
		}

		protected virtual void GenerateIMA1InvoiceDate(SegmentGroup16 group16)
		{
		}

		protected virtual void GenerateIMA1QuotaYear(SegmentGroup16 group16)
		{
		}

		void GenerateGroup17(SegmentGroup16 group16, CusContainerInvoiceLinePivot pivot)
		{
			SegmentGroup17 group17 = group16.Group17.InstantiateAChildAndAddItToChildrenCollection();
			GenerateContainerSeal(group17, pivot);
		}

		void GenerateContainerSeal(SegmentGroup17 group17, CusContainerInvoiceLinePivot pivot)
		{
			var cusContainer = pivot.Container;
			if (cusContainer != null)
			{
				if (!cusContainer.CO_Seal.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateSEL(group17.SEL.InstantiateAChildAndAddItToChildrenCollection(),
						cusContainer.CO_Seal, "", "");
				}
				else if (!cusContainer.SealStartNumber.IsEmpty || !cusContainer.SealEndNumber.IsEmpty)
				{
					EXDOCMessageUtilities.PopulateSEL(group17.SEL.InstantiateAChildAndAddItToChildrenCollection(), "",
						cusContainer.SealStartNumber, cusContainer.SealEndNumber);
				}
			}
		}

		void GenerateGroup18AndChildren(SegmentGroup11 group11)
		{
			var processes = invoiceLine.QuarantineExDocLine.Processes.Cast<QuarantineExDocEstablishmentAndTime>();

			processes.Where(IsTreatmentProcess).ForEach(GenerateTreatmentGroup(group11));
			if (MessageTypeToSend != EXDOCMessageTypeCodes.Codes.RPL)
			{
				processes.Where(IsHarvestProcess).ForEach(GenerateHarvestGroup(group11));
			}
			if (Header.IsChangeAllowed(EXDOCDataFields.ProcessDetails))
			{
				processes.Where(IsDetailsProcess).ForEach(GenerateProcessGroup(group11));
			}
		}

		bool IsTreatmentProcess(QuarantineExDocEstablishmentAndTime process)
		{
			return process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Treatment;
		}

		bool IsHarvestProcess(QuarantineExDocEstablishmentAndTime process)
		{
			return process.EE_ProcessingType == EXDOCProcessTypeCodes.Codes.Harvest;
		}

		bool IsDetailsProcess(QuarantineExDocEstablishmentAndTime process)
		{
			switch (process.EE_ProcessingType)
			{
				case EXDOCProcessTypeCodes.Codes.AquacultureFarm:
				case EXDOCProcessTypeCodes.Codes.CatcherVessel:
				case EXDOCProcessTypeCodes.Codes.Freezing:
				case EXDOCProcessTypeCodes.Codes.Processing:
				case EXDOCProcessTypeCodes.Codes.Packing:
				case EXDOCProcessTypeCodes.Codes.Slaughter:
				case EXDOCProcessTypeCodes.Codes.Loadout:
				case EXDOCProcessTypeCodes.Codes.Storage:

				case EXDOCProcessTypeCodesFish.Codes.FishingAndFactoryVessel:
				case EXDOCProcessTypeCodesFish.Codes.FishingVessel:
				case EXDOCProcessTypeCodesFish.Codes.IndependentColdStoreRawMaterial:
				case EXDOCProcessTypeCodesFish.Codes.TransportFishingVessel:

					return true;
				default:
					return false;
			}
		}

		Action<QuarantineExDocEstablishmentAndTime> GenerateTreatmentGroup(SegmentGroup11 group11)
		{
			return process =>
			{
				var group18 = group11.Group18.InstantiateAChildAndAddItToChildrenCollection();
				GenerateTreatment(group18, process);
				GenerateTreatmentInformation(group18, process);
				GenerateTreatmentActiveIngredient(group18, process);
				GenerateTreatmentConcentration(group18, process);
				GenerateTreatmentDuration(group18, process);
				GenerateTreatmentTemperature(group18, process);
				GenerateTreatmentStartDate(group18, process);
				GenerateTreatmentEndDate(group18, process);
			};
		}

		Action<QuarantineExDocEstablishmentAndTime> GenerateHarvestGroup(SegmentGroup11 group11)
		{
			return process =>
			{
				var group18 = group11.Group18.InstantiateAChildAndAddItToChildrenCollection();
				GenerateHarvestProcess(group18, process);
				GenerateHarvestStartDate(group18, process);
				GenerateHarvestEndDate(group18, process);
				GenerateDepurationDate(group18, process);
				GenerateHarvestAreaLeaseNumber(group18, process);
				GenerateDepurationPlantNumber(group18, process);
			};
		}

		Action<QuarantineExDocEstablishmentAndTime> GenerateProcessGroup(SegmentGroup11 group11)
		{
			return process =>
			{
				var group18 = group11.Group18.InstantiateAChildAndAddItToChildrenCollection();
				GenerateProductionProcess(group18, process);
				GenerateProcessStartDate(group18, process);
				GenerateProcessEndDate(group18, process);
				GenerateProcessingEstablishment(group18, process);
			};
		}

		protected virtual void GenerateCatchDates(SegmentGroup11 group11)
		{ }

		protected virtual void GenerateTreatment(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			EXDOCMessageUtilities.PopulatePRC(group18.PRC.InstantiateAChildAndAddItToChildrenCollection(),
					ProcessTypeIdentificationList.GetFromString(EXDOCProcessTypeCodes.Codes.Treatment));
		}

		protected virtual void GenerateTreatmentTemperature(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_TreatmentTemperatureUQ.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group18.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Temperature,
					PropertyMeasuredCodedList.Temperature,
					process.EE_TreatmentTemperatureUQ,
					process.EE_TreatmentTemperature.ToString());
			}
		}

		protected virtual void GenerateTreatmentDuration(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_TreatmentDurationUQ.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group18.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.UnitOfTime,
					PropertyMeasuredCodedList.TimePeriod,
					process.EE_TreatmentDurationUQ,
					process.EE_TreatmentDuration.ToString());
			}
		}

		protected virtual void GenerateTreatmentConcentration(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_TreatmentConcentrationUQ.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateMEA(group18.MEA.InstantiateAChildAndAddItToChildrenCollection(),
					MeasurementPurposeQualifierList.Chemistry,
					null,
					process.EE_TreatmentConcentrationUQ,
					process.EE_TreatmentConcentration.ToString());
			}
		}

		protected virtual void GenerateTreatmentInformation(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_TreatmentCode.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateIMD(group18.IMD.InstantiateAChildAndAddItToChildrenCollection(),
					ItemCharacteristicCodedList.EndTreatment,
					process.EE_TreatmentCode,
					process.EE_TreatmentInfo);
			}
		}

		protected virtual void GenerateTreatmentActiveIngredient(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			foreach (var ingredient in process.Ingredients.Cast<TreatmentActiveIngredient>().OrderBy(x => x.CY_Order))
			{
				EXDOCMessageUtilities.PopulateIMD(group18.IMD.InstantiateAChildAndAddItToChildrenCollection(),
						EXDOCMessageUtilities.TreatmentActiveIngredient,
						ingredient.CY_Code);
			}
		}

		protected virtual void GenerateTreatmentStartDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_StartDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group18.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.StartDateTime,
					process.EE_StartDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected virtual void GenerateTreatmentEndDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_EndDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group18.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.EndDateTime,
					process.EE_EndDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected virtual void GenerateHarvestProcess(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected virtual void GenerateHarvestStartDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected virtual void GenerateHarvestEndDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected virtual void GenerateDepurationDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected virtual void GenerateHarvestAreaLeaseNumber(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected virtual void GenerateDepurationPlantNumber(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
		}

		protected void GenerateProductionProcess(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			EXDOCMessageUtilities.PopulatePRC(group18.PRC.InstantiateAChildAndAddItToChildrenCollection(),
								ProcessTypeIdentificationList.GetFromString(process.EE_ProcessingType));
		}

		protected void GenerateProcessStartDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_StartDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group18.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.StartDateTime,
					process.EE_StartDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected void GenerateProcessEndDate(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_EndDate.IsEmpty)
			{
				EXDOCMessageUtilities.PopulateDTM(group18.DTM.InstantiateAChildAndAddItToChildrenCollection(),
					DateTimePeriodQualifierList.EndDateTime,
					process.EE_EndDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
					DateTimePeriodFormatQualifierList.Ccyymmdd);
			}
		}

		protected void GenerateProcessingEstablishment(SegmentGroup18 group18, QuarantineExDocEstablishmentAndTime process)
		{
			if (!process.EE_AuthorisationEstablishmentID.IsEmpty)
			{
				var group19 = group18.Group19.InstantiateAChildAndAddItToChildrenCollection();
				EXDOCMessageUtilities.PopulatePNA(
					group19.PNA.InstantiateAChildAndAddItToChildrenCollection(),
					PartyQualifierList.ManufacturingPlant,
					process.EE_AuthorisationEstablishmentID);
			}
			else
			{
				var address = process.Address;
				if (address != null && !address.E2_CompanyName.IsEmpty)
				{
					var group19 = group18.Group19.InstantiateAChildAndAddItToChildrenCollection();
					EXDOCMessageUtilities.PopulatePNA(
						group19.PNA.InstantiateAChildAndAddItToChildrenCollection(),
						PartyQualifierList.ManufacturingPlant, "",
						NameComponentQualifierList.WholeName, address.E2_CompanyNameTruncated);

					if (process.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.CatcherVessel &&
						process.EE_ProcessingType != EXDOCProcessTypeCodes.Codes.AquacultureFarm)
					{
						GenerateAddress(group19, address);
					}
				}
			}
		}

		void GenerateAddress(SegmentGroup19 group19, JobDocAddress address)
		{
			ZString combinedAddress = (address.E2_Address1 + " " + address.E2_Address2).Trim();
			EXDOCMessageUtilities.PopulateADR(group19.ADR.InstantiateAChildAndAddItToChildrenCollection(),
				AddressFormatCodedList.UnstructuredAddress,
				combinedAddress.SubstringSafe(0, 35),
				combinedAddress.SubstringSafe(35, 35),
				address.E2_City,
				address.E2_Postcode,
				address.E2_RN_NKCountryCode,
				address.E2_State);
			if (!address.E2_Phone.IsEmpty)
			{
				var group20 = group19.Group20.InstantiateAChildAndAddItToChildrenCollection();
				EXDOCMessageUtilities.PopulateCTA(
					group20.CTA.InstantiateAChildAndAddItToChildrenCollection(),
					ContactFunctionCodedList.MutuallyDefined, ZString.Empty);
				EXDOCMessageUtilities.PopulateCOM(
					group20.COM.InstantiateAChildAndAddItToChildrenCollection(),
					CommunicationChannelQualifierList.Telephone,
					address.E2_Phone);
			}
		}

		#region Implementation
		protected SANCRTMessage sANCRT;
		protected JobComInvoiceLine invoiceLine;
		protected readonly string MessageTypeToSend;
		public const string Percentage = "P1";
		public const string UserDefinedCertificate = "UHC";
		public const string Additional = "AD";
		public const string ProductPartQualifier = "PVP";
		public const string ProductConditionQualifier = "CND";
		public const string QualityQualifier = "QQ";
		public const string LocationQualifier = "LQ";
		public const string NatureOfCommodity = "NC";
		public const string TreatmentType = "TT";
		#endregion

	}
}
