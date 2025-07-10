using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCMessageDecoderLine
	{
		public EXDOCMessageDecoderLine(SegmentGroup11 group11)
		{
			this.group11 = group11;
		}

		public void Process()
		{
			ProcessLIN();
			ProcessMEA();
			ProcessPIA();
			ProcessIMD();
			ProcessRFF();
			ProcessATT();
			ProcessGIN();
			ProcessDTM();
			ProcessLOC();
			ProcessFTX();
			ProcessMOA();
			ProcessGroup12();
			ProcessGroup13();
			ProcessGroup15();
			ProcessContainers();
		}

		void ProcessLIN()
		{
			foreach (LINSegment lIN in group11.LIN)
			{
				LineNumber = ZShort.Parse(lIN.LineItemNumber);
			}
		}

		void ProcessMEA()
		{
			foreach (MEASegment mEA in group11.MEA)
			{
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.LineItemMeasurement)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out NetQuantity);
					NetQuantityUnit = mEA.ValueRange.MeasureUnitQualifier;
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.ItemWeight
						&& mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.NetWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out ImperialNetWeight);
					ImperialNetWeightUnit = mEA.ValueRange.MeasureUnitQualifier;
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.ItemWeight
						&& mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.ItemGrossWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out MetricGrossWeight);
					MetricGrossWeightUnit = mEA.ValueRange.MeasureUnitQualifier;
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.ItemWeight
						&& mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.NetNetWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out DrainedWeight);
					DrainedWeightUnit = mEA.ValueRange.MeasureUnitQualifier;
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkProtein)
						&& mEA.ValueRange.MeasureUnitQualifier == RequestForPermitLineMessageBuilder.Percentage)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out PercentageOfMilkProtein);
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkFat)
						&& mEA.ValueRange.MeasureUnitQualifier == RequestForPermitLineMessageBuilder.Percentage)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out PercentageOfMilkFat);
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkProtein)
						&& mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.NetWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out TotalWeightOfMilkProteinInMixtures);
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.GetFromString(RequestForPermitDairyLineMessageBuilder.MilkFat)
						&& mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.NetWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out TotalWeightOfMilkFatInMixtures);
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.ItemWeight
						&& mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.ChargeableWeight)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out BeefVealWeight);
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.Chemistry
						&& mEA.ValueRange.MeasureUnitQualifier == RequestForPermitLineMessageBuilder.Percentage)
				{
					ZInt.TryParse(mEA.ValueRange.MeasurementValue, out ChecimalLeanPercentage);
				}
				if (mEA.MeasurementPurposeQualifier == MeasurementPurposeQualifierList.CustomsLineItemMeasurement &&
							mEA.MeasurementDetails.PropertyMeasuredCoded == PropertyMeasuredCodedList.ShippedQuantity)
				{
					ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out CustomsWeight);
					CustomsWeightUQ = mEA.ValueRange.MeasureUnitQualifier;
				}
			}
		}

		void ProcessPIA()
		{
			foreach (PIASegment pIA in group11.PIA)
			{
				if (pIA.ProductIdFunctionQualifier == ProductIdFunctionQualifierList.ProductIdentification
						&& pIA.ItemNumberIdentification1.ItemNumberTypeCoded == ItemNumberTypeCodedList.IndustryCommodityCode)
				{
					productCode = pIA.ItemNumberIdentification1.ItemNumber;
					PreservationType = productCode.SubstringSafe(0, 1) == "X" ? ZString.Empty : productCode.SubstringSafe(0, 1);
					ProductType = productCode.SubstringSafe(1, 3);
					PackType = productCode.SubstringSafe(4, 2);
					SupplimentaryCode = productCode.SubstringSafe(6, 2);
				}
				else if (pIA.ProductIdFunctionQualifier == ProductIdFunctionQualifierList.ProductIdentification
						&& pIA.ItemNumberIdentification1.ItemNumberTypeCoded == ItemNumberTypeCodedList.BuyersPartNumber)
				{
					CutCode = pIA.ItemNumberIdentification1.ItemNumber;
				}
				else if (pIA.ProductIdFunctionQualifier == ProductIdFunctionQualifierList.AdditionalIdentification
						&& pIA.ItemNumberIdentification1.ItemNumberTypeCoded == ItemNumberTypeCodedList.IndustryCommodityCode)
				{
					EAN13Number = pIA.ItemNumberIdentification1.ItemNumber;
				}
				else if (pIA.ProductIdFunctionQualifier == ProductIdFunctionQualifierList.ProductIdentification
						&& pIA.ItemNumberIdentification1.ItemNumberTypeCoded == ItemNumberTypeCodedList.HarmonisedSystem)
				{
					AheccCode = pIA.ItemNumberIdentification1.ItemNumber;
				}
				else if (pIA.ProductIdFunctionQualifier == ProductIdFunctionQualifierList.ProductIdentification
						&& pIA.ItemNumberIdentification1.ItemNumberTypeCoded == ItemNumberTypeCodedList.CommodityGrouping)
				{
					DominantProduct = pIA.ItemNumberIdentification1.ItemNumber;
				}
				else if (pIA.ProductIdFunctionQualifier == ProductIdFunctionQualifierList.ProductIdentification
						&& pIA.ItemNumberIdentification1.ItemNumberTypeCoded == ItemNumberTypeCodedList.StandardGroupOfProductsMixedAssortment)
				{
					AdditionalProducts = pIA.ItemNumberIdentification1.ItemNumber;
				}
			}
		}

		void ProcessIMD()
		{
			foreach (IMDSegment iMD in group11.IMD)
			{
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitMeatLineMessageBuilder.Inspection)
				{
					LineItemDescription += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitLineMessageBuilder.UserDefinedCertificate)
				{
					ExporterDefinedProductDescription += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification.StartsWith("GHC"))
				{
					GeneratedProductDescription += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitLineMessageBuilder.Additional)
				{
					AdditionalProductDescription += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == "ED")
				{
					EANDescription += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitGrainsAndPlantsLineMessageBuilder.Commercial)
				{
					CommercialProductDescription += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitLineMessageBuilder.QualityQualifier)
				{
					ProductQualityQualification += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitLineMessageBuilder.LocationQualifier)
				{
					ProductLocationQualification += iMD.ItemDescription.ItemDescription1 + iMD.ItemDescription.ItemDescription2 + iMD.ItemDescription.ItemDescription3 + iMD.ItemDescription.ItemDescription4 +
						iMD.ItemDescription.ItemDescription5 + iMD.ItemDescription.ItemDescription6 + iMD.ItemDescription.ItemDescription7 + iMD.ItemDescription.ItemDescription8;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitLineMessageBuilder.NatureOfCommodity)
				{
					NatureOfCommodity = iMD.ItemDescription.ItemDescription1;
				}
				if (iMD.ItemDescription.ItemDescriptionIdentification == RequestForPermitLineMessageBuilder.TreatmentType)
				{
					TreatmentType = iMD.ItemDescription.ItemDescription1;
				}
			}
		}

		void ProcessRFF()
		{
			foreach (RFFSegment rFF in group11.RFF)
			{
				if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.MarkingLabelReference)
				{
					LabelApprovalNumber = rFF.Reference.ReferenceNumber;
				}
				if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.SchemePlanNumber)
				{
					exportSchemeCode = rFF.Reference.ReferenceNumber;
					switch (exportSchemeCode)
					{
						case "DRB":
							DutyDrawback = true;
							break;
						case "MVP":
							MotorVehiclePlan = true;
							break;
						case "TX":
							Texaco = true;
							break;
					}
				}
				if (rFF.Reference.ReferenceQualifier == ReferenceQualifierList.ReleaseNumber)
				{
					AMLCQuotaApproval = rFF.Reference.ReferenceNumber;
				}
			}
		}

		void ProcessATT()
		{
			foreach (ATTSegment aTT in group11.ATT)
			{
				if (aTT.AttributeDetails.CodeListQualifier == CodeListQualifierList.GetFromString(EXDOCMessageUtilities.LabelApprovalIndicatorQualifier) && aTT.AttributeDetails.AttributeCoded == "Y")
				{
					LabelApprovalIndicator = true;
				}

				if (aTT.AttributeDetails.CodeListQualifier == CodeListQualifierList.GetFromString(EXDOCMessageUtilities.UngradedProductIndicatorQualifier) && aTT.AttributeDetails.AttributeCoded == "Y")
				{
					UngradedProductIndicator = true;
				}

				if (aTT.AttributeDetails.CodeListQualifier == CodeListQualifierList.GetFromString(EXDOCMessageUtilities.HalalProductIndicatorQualifier) && aTT.AttributeDetails.AttributeCoded == "Y")
				{
					HalalProductIndicator = true;
				}

				if (aTT.AttributeDetails.CodeListQualifier == CodeListQualifierList.GetFromString(EXDOCMessageUtilities.FinalConsumerIndicator) && aTT.AttributeDetails.AttributeCoded == "Y")
				{
					FinalConsumerIndicator = true;
				}
			}
		}

		void ProcessGIN()
		{
			foreach (GINSegment gIN in group11.GIN)
			{
				if (gIN.IdentityNumberQualifier == IdentityNumberQualifierList.BatchNumber)
				{
					BatchNumber = gIN.IdentityNumberRange1.IdentityNumber1;
				}
			}
		}

		void ProcessDTM()
		{
			foreach (DTMSegment dTM in group11.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.StartDateTime)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out DurabilityStartDate, "yyyyMMdd");
				}
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.EndDateTime)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out DurabilityEndDate, "yyyyMMdd");
				}
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.ContainerSafetyConventionCscInspectionDate)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out EmptyContainerInspectionDate, "yyyyMMdd");
				}
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.ProcessingDateTime)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out SaltingDate, "yyyyMMdd");
				}
			}
		}

		void ProcessLOC()
		{
			foreach (LOCSegment lOC in group11.LOC)
			{
				if (lOC.PlaceLocationQualifier == PlaceLocationQualifierList.MutuallyDefined)
				{
					ProductSourceState = lOC.LocationIdentification.PlaceLocationIdentification;
				}
			}
		}

		void ProcessFTX()
		{
			foreach (FTXSegment fTX in group11.FTX)
			{
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.AdditionalExportInformation)
				{
					AdditionalDeclarationText += fTX.TextLiteral.FreeText1 + fTX.TextLiteral.FreeText2 + fTX.TextLiteral.FreeText3 + fTX.TextLiteral.FreeText4 + fTX.TextLiteral.FreeText5;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.MutuallyDefined)
				{
					CodedStatement1 = new ZShort(fTX.TextLiteral.FreeText1);
					CodedStatement2 = new ZShort(fTX.TextLiteral.FreeText2);
					CodedStatement3 = new ZShort(fTX.TextLiteral.FreeText3);
					CodedStatement4 = new ZShort(fTX.TextLiteral.FreeText4);
					CodedStatement5 = new ZShort(fTX.TextLiteral.FreeText5);
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.CertificationStatements)
				{
					FreeTextStatement += fTX.TextLiteral.FreeText1 + fTX.TextLiteral.FreeText2 + fTX.TextLiteral.FreeText3 + fTX.TextLiteral.FreeText4 + fTX.TextLiteral.FreeText5;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.AdditionalMarksNumbersInformation)
				{
					GrowerNumber += fTX.TextLiteral.FreeText1 + fTX.TextLiteral.FreeText2 + fTX.TextLiteral.FreeText3 + fTX.TextLiteral.FreeText4 + fTX.TextLiteral.FreeText5;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.GovernmentInformation)
				{
					ImportAuthorityCode += fTX.TextLiteral.FreeText1 + fTX.TextLiteral.FreeText2 + fTX.TextLiteral.FreeText3 + fTX.TextLiteral.FreeText4 + fTX.TextLiteral.FreeText5;
				}
				if (fTX.TextSubjectQualifier == TextSubjectQualifierList.LineItem)
				{
					ClientLineItemID += fTX.TextLiteral.FreeText1 + fTX.TextLiteral.FreeText2 + fTX.TextLiteral.FreeText3 + fTX.TextLiteral.FreeText4;
				}
			}
		}

		void ProcessMOA()
		{
			foreach (MOASegment mOA in group11.MOA)
			{
				if (mOA.MonetaryAmount.MonetaryAmountTypeQualifier == MonetaryAmountTypeQualifierList.FobValue)
				{
					ZDecimal.TryParse(mOA.MonetaryAmount.MonetaryAmount, out FOBAmount);
				}
			}
		}

		void ProcessGroup12()
		{
			foreach (SegmentGroup12 group12 in group11.Group12)
			{
				ProcessDOC(group12);
				ProcessDTM(group12);
			}
		}

		void ProcessDOC(SegmentGroup12 group12)
		{
			foreach (DOCSegment dOC in group12.DOC)
			{
				if (dOC.DocumentMessageName.DocumentMessageNameCoded == DocumentMessageNameCodedList.SanitaryCertificate
						&& dOC.DocumentMessageDetails.DocumentMessageStatusCoded == DocumentMessageStatusCodedList.ToBePrinted)
				{
					ExtraCertificates.Append(dOC.DocumentMessageName.DocumentMessageName);
				}
				if (dOC.DocumentMessageName.DocumentMessageNameCoded == DocumentMessageNameCodedList.SanitaryCertificate
						&& dOC.DocumentMessageDetails.DocumentMessageStatusCoded == DocumentMessageStatusCodedList.Accepted)
				{
					AssignedCertificateTemplates.Append(dOC.DocumentMessageName.DocumentMessageName);
					AssignedCertificateNumbers.Append(dOC.DocumentMessageDetails.DocumentMessageNumber);
				}
				if (dOC.DocumentMessageName.DocumentMessageNameCoded == DocumentMessageNameCodedList.CertificateOfQuality
						&& dOC.DocumentMessageDetails.DocumentMessageStatusCoded == DocumentMessageStatusCodedList.Accepted)
				{
					AssignedCertificateTemplates.Append(dOC.DocumentMessageName.DocumentMessageName);
					AssignedCertificateNumbers.Append(dOC.DocumentMessageDetails.DocumentMessageNumber);
				}
				if (dOC.DocumentMessageName.DocumentMessageNameCoded == DocumentMessageNameCodedList.CertificateOfQuality
					&& dOC.DocumentMessageDetails.DocumentMessageStatusCoded == DocumentMessageStatusCodedList.DocumentAppliedFor)
				{
					RequestedPrimaryCertificateTemplate = dOC.DocumentMessageName.DocumentMessageName;
					RequestedPrimaryCertificateNumber = dOC.DocumentMessageDetails.DocumentMessageNumber;
				}
				if (dOC.DocumentMessageName.DocumentMessageNameCoded == DocumentMessageNameCodedList.GoodsControlCertificate)
				{
					ExportPermitNumber = dOC.DocumentMessageDetails.DocumentMessageNumber;
					ExportPermitAuthority = dOC.DocumentMessageDetails.DocumentMessageSource;
				}
			}
		}

		void ProcessDTM(SegmentGroup12 group12)
		{
			foreach (DTMSegment dTM in group12.DTM)
			{
				if (dTM.DateTimePeriod.DateTimePeriodQualifier == DateTimePeriodQualifierList.DocumentMessageDateTime)
				{
					ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriod, out ExportPermitDate, "yyyyMMdd");
				}
			}
		}

		void ProcessGroup13()
		{
			foreach (SegmentGroup13 group13 in group11.Group13)
			{
				ProcessPNA(group13);
			}
		}

		void ProcessPNA(SegmentGroup13 group13)
		{
			foreach (PNASegment pNA in group13.PNA)
			{
				if (pNA.PartyQualifier == PartyQualifierList.Exporter)
				{
					AMLCPerformanceExporterNumber = pNA.IdentificationNumber.IdentityNumber;
				}
			}
		}

		void ProcessGroup15()
		{
			foreach (SegmentGroup15 group15 in group11.Group15)
			{
				ProcessPAC(group15);
				ProcessPCI(group15);
				ProcessMEA(group15);
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "ZInt parse returns default of zero if parse fails")]
		void ProcessPAC(SegmentGroup15 group15)
		{
			foreach (PACSegment pAC in group15.PAC)
			{
				if (pAC.PackagingDetails.PackagingLevelCoded == PackagingLevelCodedList.Outer)
				{
					ZInt.TryParse(pAC.NumberOfPackages, out OuterPackCount);
					OuterPackType = pAC.PackageType.TypeOfPackagesIdentification;
				}
				if (pAC.PackagingDetails.PackagingLevelCoded == PackagingLevelCodedList.Intermediate)
				{
					ZInt.TryParse(pAC.NumberOfPackages, out IntermediatePackCount);
					IntermediatePackType = pAC.PackageType.TypeOfPackagesIdentification;
				}
				if (pAC.PackagingDetails.PackagingLevelCoded == PackagingLevelCodedList.Inner)
				{
					ZInt.TryParse(pAC.NumberOfPackages, out InnerPackCount);
					InnerPackType = pAC.PackageType.TypeOfPackagesIdentification;
				}
			}
		}

		void ProcessPCI(SegmentGroup15 group15)
		{
			foreach (PCISegment pCI in group15.PCI)
			{
				ShippingMarks = pCI.MarksLabels.ShippingMarks1;
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "ZDecimal parse returns default of zero if parse fails")]
		void ProcessMEA(SegmentGroup15 group15)
		{
			ZInt mEACount = 1;
			foreach (MEASegment mEA in group15.MEA)
			{
				switch (mEACount)
				{
					case 1:
						OuterPackAccuracy = mEA.MeasurementDetails.MeasurementSignificanceCoded.ToString();
						ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out OuterPackWeight);
						OuterPackWeightUnit = mEA.ValueRange.MeasureUnitQualifier;
						break;
					case 2:
						IntermediatePackAccuracy = mEA.MeasurementDetails.MeasurementSignificanceCoded.ToString();
						ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out IntermediatePackWeight);
						IntermediatePackWeightUnit = mEA.ValueRange.MeasureUnitQualifier;
						break;
					case 3:
						InnerPackAccuracy = mEA.MeasurementDetails.MeasurementSignificanceCoded.ToString();
						ZDecimal.TryParse(mEA.ValueRange.MeasurementValue, out InnerPackWeight);
						InnerPackWeightUnit = mEA.ValueRange.MeasureUnitQualifier;
						break;
				}
				++mEACount;
			}
		}

		void ProcessContainers()
		{
			Containers = new List<EXDOCMessageDecoderContainer>();
			EXDOCMessageDecoderContainer container;
			foreach (SegmentGroup16 group16 in group11.Group16)
			{
				container = new EXDOCMessageDecoderContainer(group16);
				container.Process();
				Containers.Add(container);
			}
		}

		public ZShort LineNumber;
		public ZString InnerPackAccuracy;
		public ZDecimal InnerPackWeight;
		public ZString InnerPackWeightUnit;
		public ZString IntermediatePackAccuracy;
		public ZDecimal IntermediatePackWeight;
		public ZString IntermediatePackWeightUnit;
		public ZString OuterPackAccuracy;
		public ZDecimal OuterPackWeight;
		public ZString OuterPackWeightUnit;
		public ZString ShippingMarks;
		public ZInt InnerPackCount;
		public ZString InnerPackType;
		public ZInt IntermediatePackCount;
		public ZString IntermediatePackType;
		public ZInt OuterPackCount;
		public ZString OuterPackType;
		public ZString AMLCPerformanceExporterNumber;
		public ZDateTime ExportPermitDate;
		public ZString ExportPermitAuthority;
		public ZString ExportPermitNumber;
		public ZStringBuilder AssignedCertificateTemplates = new ZStringBuilder();
		public ZStringBuilder AssignedCertificateNumbers = new ZStringBuilder();
		public ZString RequestedPrimaryCertificateTemplate;
		public ZString RequestedPrimaryCertificateNumber;
		public ZStringBuilder ExtraCertificates = new ZStringBuilder();
		public ZDecimal FOBAmount;
		public ZString ImportAuthorityCode;
		public ZString ClientLineItemID;
		public ZString GrowerNumber;
		public ZString FreeTextStatement;
		public ZShort CodedStatement1;
		public ZShort CodedStatement2;
		public ZShort CodedStatement3;
		public ZShort CodedStatement4;
		public ZShort CodedStatement5;
		public ZString AdditionalDeclarationText;
		public ZString ProductSourceState;
		public ZDateTime SaltingDate;
		public ZDateTime EmptyContainerInspectionDate;
		public ZDateTime DurabilityEndDate;
		public ZDateTime DurabilityStartDate;
		public ZString AMLCQuotaApproval;
		ZString exportSchemeCode;
		public ZBool DutyDrawback;
		public ZBool MotorVehiclePlan;
		public ZBool Texaco;
		public ZString BatchNumber;
		public ZString LabelApprovalNumber;
		public ZBool LabelApprovalIndicator;
		public ZBool UngradedProductIndicator;
		public ZBool HalalProductIndicator;
		public ZBool FinalConsumerIndicator;
		public ZString ProductLocationQualification;
		public ZString ProductQualityQualification;
		public ZString NatureOfCommodity;
		public ZString TreatmentType;
		public ZString CommercialProductDescription;
		public ZString EANDescription;
		public ZString AdditionalProductDescription;
		public ZString GeneratedProductDescription;
		public ZString ExporterDefinedProductDescription;
		public ZString LineItemDescription;
		public ZString AheccCode;
		public ZString EAN13Number;
		public ZString CutCode;
		public ZInt ChecimalLeanPercentage;
		public ZDecimal BeefVealWeight;
		public ZDecimal TotalWeightOfMilkFatInMixtures;
		public ZDecimal TotalWeightOfMilkProteinInMixtures;
		public ZDecimal PercentageOfMilkFat;
		public ZDecimal PercentageOfMilkProtein;
		public ZString DrainedWeightUnit;
		public ZDecimal DrainedWeight;
		public ZString MetricGrossWeightUnit;
		public ZDecimal MetricGrossWeight;
		public ZString ImperialNetWeightUnit;
		public ZDecimal ImperialNetWeight;
		public ZString NetQuantityUnit;
		public ZDecimal NetQuantity;
		public ZString PreservationType;
		public ZString ProductType;
		public ZString PackType;
		public ZString SupplimentaryCode;
		ZString productCode;
		readonly SegmentGroup11 group11;
		public List<EXDOCMessageDecoderContainer> Containers;
		public ZDecimal CustomsWeight;
		public ZString CustomsWeightUQ;
		public ZString DominantProduct;
		public ZString AdditionalProducts;
	}
}
