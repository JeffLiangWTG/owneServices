using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Elements;
using Enterprise.Edifact.D00A.Messages.CUSRES;
using Enterprise.Edifact.D00A.Segments;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public static class D00AMessageUtilities
	{
		#region Convert Values

		internal static ZString ConvertWeightUnitToACIUnit(ZString code)
		{
			var result = ZString.Empty;

			switch (code)
			{
				case Constants.Weight.Kilograms:
					result = MessageConstants.CBSAWeightUnits.Kilogram;
					break;
				case Constants.Weight.Tonnes:
					result = MessageConstants.CBSAWeightUnits.MetricTon;
					break;
				case Constants.Weight.Pounds:
					result = MessageConstants.CBSAWeightUnits.Pound;
					break;
			}
			return result;
		}

		internal static ZString ConvertVolumeUnitToACIUnit(ZString code)
		{
			var result = ZString.Empty;

			switch (code)
			{
				case MessageConstants.ACIVolumeUnits.CubicCentimetre:
				case MessageConstants.ACIVolumeUnits.Cord:
				case MessageConstants.ACIVolumeUnits.BoardFoot100:
				case MessageConstants.ACIVolumeUnits.GallonsUK:
				case MessageConstants.ACIVolumeUnits.HundredsTTTons:
				case MessageConstants.ACIVolumeUnits.GallonsUSDry:
				case MessageConstants.ACIVolumeUnits.GallonsUSLiquid:
				case MessageConstants.ACIVolumeUnits.HundredsTTTonsShort:
				case MessageConstants.ACIVolumeUnits.TonShort:
				case MessageConstants.ACIVolumeUnits.TonMetric:
				case MessageConstants.ACIVolumeUnits.Car:
				case MessageConstants.ACIVolumeUnits.TonLong:
				case MessageConstants.ACIVolumeUnits.VolumetricUnit:
				case MessageConstants.ACIVolumeUnits.Barge:
				case MessageConstants.ACIVolumeUnits.Container:
					result = code;
					break;
				case Constants.Volume.CubicDecimetres:
					result = MessageConstants.ACIVolumeUnits.CubicDecimetre;
					break;
				case Constants.Volume.CubicFeet:
					result = MessageConstants.ACIVolumeUnits.CubicFeet;
					break;
				case Constants.Volume.CubicInches:
					result = MessageConstants.ACIVolumeUnits.CubicInches;
					break;
				case Constants.Volume.CubicMetres:
					result = MessageConstants.ACIVolumeUnits.CubicMeters;
					break;
				case Constants.Volume.Litre:
					result = MessageConstants.ACIVolumeUnits.Litre;
					break;
				case MessageConstants.ACIVolumeUnits.LoadForEnterprise:
					result = MessageConstants.ACIVolumeUnits.Load;
					break;
			}
			return result;
		}

		#endregion

		#region Populate Details

		public static void PopulateUNH(UNHSegment unh, string messageReferenceNumber, string messageType, string messageVersionNumber, string messageReleaseNumber, string controllingAgency, string associationAssignedCode)
		{
			unh.MessageReferenceNumber = messageReferenceNumber;
			unh.MessageIdentifier.MessageType = messageType;
			unh.MessageIdentifier.MessageVersionNumber = messageVersionNumber;
			unh.MessageIdentifier.MessageReleaseNumber = messageReleaseNumber;
			unh.MessageIdentifier.ControllingAgency = controllingAgency;
			unh.MessageIdentifier.AssociationAssignedCode = associationAssignedCode;
		}

		public static void PopulateBGM(BGMSegment bgm, DocumentNameCodeList documentNameCode, string documentName, string documentMessageNumber, MessageFunctionCodeList messageFunctionCode)
		{
			bgm.DocumentMessageName.DocumentNameCode = documentNameCode;
			bgm.DocumentMessageName.DocumentName = documentName;
			bgm.DocumentMessageIdentification.DocumentIdentifier = documentMessageNumber;
			bgm.MessageFunctionCode = messageFunctionCode;
		}

		public static void PopulateUNT(UNTSegment unt, string numberOfSegmentsInTheMessage, string messageReferenceNumber)
		{
			unt.NumberOfSegmentsInTheMessage = numberOfSegmentsInTheMessage;
			unt.MessageReferenceNumber = messageReferenceNumber;
		}

		public static void PopulateUNS(UNSSegment uns, string sectionIdentification)
		{
			uns.SectionIdentification = sectionIdentification;
		}

		public static void PopulateCST(CSTSegment cst, string customsCodeIdent, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = customsCodeIdent;
			cst.CustomsIdentityCodes1.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulateTDT(TDTSegment tdt, TransportStageCodeQualifierList transportStageQualifier, string modeOfTransportCoded, string carrierIdentification)
		{
			tdt.TransportStageCodeQualifier = transportStageQualifier;
			tdt.ModeOfTransport.TransportModeNameCode = modeOfTransportCoded;
			tdt.Carrier.CarrierIdentifier = carrierIdentification;
		}

		public static void PopulateTDT(TDTSegment tdt, TransportStageCodeQualifierList transportStageQualifier, string modeOfTransportCoded, string carrierIdentification, string carrierName, string vesselName)
		{
			tdt.TransportStageCodeQualifier = transportStageQualifier;
			tdt.ModeOfTransport.TransportModeNameCode = modeOfTransportCoded;
			if (!string.IsNullOrEmpty(carrierIdentification))
			{
				tdt.Carrier.CarrierIdentifier = carrierIdentification;
				tdt.Carrier.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.CaRevenueCanadaCustomsAndExcise;
			}
			else
			{
				tdt.Carrier.CarrierName = carrierName;
			}
			if (!string.IsNullOrEmpty(vesselName))
			{
				tdt.TransportIdentification.TransportMeansIdentificationName = vesselName;
			}
		}

		public static void PopulateDOC(DOCSegment doc, string modeOfTransport, string documentMessageNumber)
		{
			var documentMessageNameCoded = DocumentNameCodeList.MasterBillOfLading;
			switch (modeOfTransport)
			{
				case MessageConstants.ModeOfTransportMessageCodes.Rail:
					documentMessageNameCoded = DocumentNameCodeList.RailConsignmentNoteGenericTerm;
					break;
				case MessageConstants.ModeOfTransportMessageCodes.Highway:
					documentMessageNameCoded = DocumentNameCodeList.RoadConsignmentNote;
					break;
				case MessageConstants.ModeOfTransportMessageCodes.Air:
					documentMessageNameCoded = DocumentNameCodeList.MasterAirWaybill;
					break;
			}
			PopulateDOC(doc, documentMessageNameCoded, documentMessageNumber);
		}

		public static void PopulateDOC(DOCSegment doc, DocumentNameCodeList documentMessageNameCoded, string documentMessageNumber)
		{
			doc.DocumentMessageName.DocumentNameCode = documentMessageNameCoded;
			doc.DocumentMessageDetails.DocumentIdentifier = documentMessageNumber;
		}

		public static void PopulateRFF(RFFSegment rff, ReferenceFunctionCodeQualifierList referenceQualifier, string referenceNumber)
		{
			rff.Reference.ReferenceFunctionCodeQualifier = referenceQualifier;
			rff.Reference.ReferenceIdentifier = referenceNumber;
		}

		public static void PopulateLOC(LOCSegment loc, LocationFunctionCodeQualifierList placeLocationQualifier, string destinationCountryCode, string destinationCityName, string destinationPortName)
		{
			PopulateLOC(loc, placeLocationQualifier, destinationCountryCode, destinationCityName, destinationPortName, null);
		}

		public static void PopulateLOC(LOCSegment loc, LocationFunctionCodeQualifierList placeLocationQualifier, string locationCode, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			PopulateLOC(loc, placeLocationQualifier, locationCode, "", "", codeListResponsibleAgencyCode);
		}

		public static void PopulateLOC(LOCSegment loc, LocationFunctionCodeQualifierList placeLocationQualifier, string locationCode, string destinationCityName, string destinationPortName, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			loc.LocationFunctionCodeQualifier = placeLocationQualifier;
			loc.LocationIdentification.LocationNameCode = locationCode;
			if (!string.IsNullOrEmpty(destinationCityName))
			{
				loc.LocationIdentification.LocationName = destinationCityName;
			}

			if (codeListResponsibleAgencyCode != null)
			{
				loc.LocationIdentification.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
			}

			if (!string.IsNullOrEmpty(destinationPortName))
			{
				loc.RelatedLocationOneIdentification.FirstRelatedLocationNameCode = destinationPortName;
			}
		}

		public static void PopulateLOC(LOCSegment loc, LocationFunctionCodeQualifierList qualifier, string locationCode, CodeListResponsibleAgencyCodeList responsibleAgencyCode,
			string releatedLocation, CodeListResponsibleAgencyCodeList releatedLocationResponsibleAgencyCode)
		{
			loc.LocationFunctionCodeQualifier = qualifier;
			loc.LocationIdentification.LocationNameCode = locationCode;
			loc.LocationIdentification.CodeListResponsibleAgencyCode = responsibleAgencyCode;
			if (!string.IsNullOrEmpty(releatedLocation))
			{
				loc.RelatedLocationOneIdentification.FirstRelatedLocationNameCode = releatedLocation;
				loc.RelatedLocationOneIdentification.CodeListResponsibleAgencyCode = releatedLocationResponsibleAgencyCode;
			}
		}

		public static void PopulateGEI(GEISegment gei, string customsProcedureCode)
		{
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString("6");
			gei.ProcessingIndicator.ProcessingIndicatorDescription = customsProcedureCode;
		}

		public static void PopulateGEI(GEISegment gei, string natureOfTransaction, string processIndicator)
		{
			gei.ProcessingInformationCodeQualifier = ProcessingInformationCodeQualifierList.GetFromString(natureOfTransaction);
			gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode = ProcessingIndicatorDescriptionCodeList.GetFromString(processIndicator);
		}

		public static void PopulateFTX(FTXSegment ftx, TextSubjectCodeQualifierList textSubjectCode, string freeText)
		{
			ftx.TextSubjectCodeQualifier = textSubjectCode;
			ftx.TextLiteral.FreeTextValue1 = freeText;
		}

		public static void PopulateEQDContainer(EQDSegment eqd, ZString containerNumber, ZString countryOfRegistration, ZString sizeCode, bool? isEmpty = null)
		{
			eqd.EquipmentTypeCodeQualifier = EquipmentTypeCodeQualifierList.Container;
			var regSizeNum = countryOfRegistration + sizeCode;
			if (regSizeNum.Length != 6)
			{
				regSizeNum = string.Empty;
			}

			eqd.EquipmentIdentification.EquipmentIdentifier = containerNumber.SubstringSafe(0, 11).PadRight(11) + regSizeNum;
			if (eqd.EquipmentIdentification.EquipmentIdentifier.Length > 13)
			{
				eqd.EquipmentIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.IsoInternationalOrganizationForStandardization;
			}
			if (isEmpty.HasValue)
			{
				eqd.FullOrEmptyIndicatorCode = (isEmpty.Value ? FullOrEmptyIndicatorCodeList.Empty : FullOrEmptyIndicatorCodeList.Full);
			}
		}

		public static void PopulatePAC(PACSegment pac, ZInt packages, string packType)
		{
			pac.PackageQuantity = packages.ToString();
			pac.PackageType.PackageTypeDescriptionCode = packType;//TODO: syncronise must convert std cargowise pack type to across pack type
		}

		public static void PopulateMEAWeight(MEASegment mEA, ZDecimal value, string units)
		{
			var convertedUnits = ConvertWeightUnitToACIUnit(units);
			var convertedWeight = value;
			if (convertedUnits.IsEmpty)
			{
				convertedUnits = MessageConstants.CBSAWeightUnits.Kilogram;
				try
				{
					convertedWeight = Constants.Weight.Convert(value, units, Constants.Weight.Kilograms);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					convertedUnits = units;
					convertedWeight = value;
				}
			}
			mEA.MeasurementAttributeCode = MeasurementAttributeCodeList.Weights;
			mEA.MeasurementDetails.MeasuredAttributeCode = MeasuredAttributeCodeList.ItemGrossWeight;
			mEA.ValueRange.MeasurementUnitCode = convertedUnits;
			//ZDecimal weight = convertedWeight.Round(4);
			mEA.ValueRange.MeasurementValue = convertedWeight.Round(4).ToStringTrimZeros();
		}

		public static void PopulateMEAWeightInKG(MEASegment mea, ZDecimal value, string units)
		{
			ZString convertedUnits = MessageConstants.CBSAWeightUnits.Kilogram;
			ZDecimal convertedWeight;
			try
			{
				convertedWeight = Constants.Weight.Convert(value, units, Constants.Weight.Kilograms);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				convertedUnits = units;
				convertedWeight = value;
			}
			mea.MeasurementAttributeCode = MeasurementAttributeCodeList.Weights;
			mea.MeasurementDetails.MeasuredAttributeCode = MeasuredAttributeCodeList.TotalGrossWeight;
			mea.ValueRange.MeasurementUnitCode = convertedUnits;
			mea.ValueRange.MeasurementValue = convertedWeight.Round(0).ToString();
		}

		public static void PopulateMEAVolume(MEASegment mea, ZDecimal value, string units)
		{
			var convertedUnits = ConvertVolumeUnitToACIUnit(units);
			var convertedVolume = value;
			if (convertedUnits.IsEmpty)
			{
				convertedUnits = MessageConstants.ACIVolumeUnits.CubicMeters;
				try
				{
					convertedVolume = Constants.Volume.Convert(value, units, Constants.Volume.CubicMetres);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					convertedUnits = units;
					convertedVolume = value;
				}
			}
			mea.MeasurementAttributeCode = MeasurementAttributeCodeList.Volume;
			mea.ValueRange.MeasurementUnitCode = "WSD";
			mea.MeasurementDetails.NonDiscreteMeasurementName = convertedUnits;
			mea.ValueRange.MeasurementValue = convertedVolume.Round(4).ToString();
		}

		public static void PopulateMEA(MEASegment mea, MeasurementAttributeCodeList measurementAttributeCode, ZDecimal value, string units)
		{
			mea.MeasurementAttributeCode = measurementAttributeCode;
			mea.ValueRange.MeasurementUnitCode = units;
			mea.ValueRange.MeasurementValue = value.Round(4).ToString("#.####");
		}

		public static void PopulateSGP(SGPSegment sgp, string containerNumber)
		{
			sgp.EquipmentIdentification.EquipmentIdentifier = containerNumber;
		}

		public static void PopulateDGS(DGSSegment dgs, string dgCode)
		{
			dgs.UndgInformation.UnitedNationsDangerousGoodsIdentificationCode = dgCode;
		}

		public static void PopulatePCI(PCISegment pci, List<ZString> marksList)
		{
			pci.MarksLabels.ShippingMarksDescription1 = marksList[0];
			if (marksList.Count > 1)
			{
				pci.MarksLabels.ShippingMarksDescription2 = marksList[1];
			}

			if (marksList.Count > 2)
			{
				pci.MarksLabels.ShippingMarksDescription3 = marksList[2];
			}

			if (marksList.Count > 3)
			{
				pci.MarksLabels.ShippingMarksDescription4 = marksList[3];
			}

			if (marksList.Count > 4)
			{
				pci.MarksLabels.ShippingMarksDescription5 = marksList[4];
			}

			if (marksList.Count > 5)
			{
				pci.MarksLabels.ShippingMarksDescription6 = marksList[5];
			}

			if (marksList.Count > 6)
			{
				pci.MarksLabels.ShippingMarksDescription7 = marksList[6];
			}

			if (marksList.Count > 7)
			{
				pci.MarksLabels.ShippingMarksDescription8 = marksList[7];
			}

			if (marksList.Count > 8)
			{
				pci.MarksLabels.ShippingMarksDescription9 = marksList[8];
			}
		}

		public static void PopulateCSTList(CSTSegment cst, List<ZString> tariffList)
		{
			cst.CustomsIdentityCodes1.CustomsGoodsIdentifier = tariffList[0];
			if (tariffList.Count > 1)
			{
				cst.CustomsIdentityCodes2.CustomsGoodsIdentifier = tariffList[1];
			}

			if (tariffList.Count > 2)
			{
				cst.CustomsIdentityCodes3.CustomsGoodsIdentifier = tariffList[2];
			}

			if (tariffList.Count > 3)
			{
				cst.CustomsIdentityCodes4.CustomsGoodsIdentifier = tariffList[3];
			}

			if (tariffList.Count > 4)
			{
				cst.CustomsIdentityCodes5.CustomsGoodsIdentifier = tariffList[4];
			}
		}

		public static void PopulateAUT(AUTSegment aut, string authorityCode)
		{
			aut.ValidationResultValue = authorityCode;
		}

		public static void PopulateDTM203(DTMSegment dtm, DateOrTimeOrPeriodFunctionCodeQualifierList dateTimePeriodQualifier, ZDateTime dateTimePeriodValue)
		{
			dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier = dateTimePeriodQualifier;
			dtm.DateTimePeriod.DateOrTimeOrPeriodValue = dateTimePeriodValue.ToString("yyyyMMddHHmm");
			dtm.DateTimePeriod.DateOrTimeOrPeriodFormatCode = DateOrTimeOrPeriodFormatCodeList.Ccyymmddhhmm;
		}

		public static void PopulateNAD(NADSegment nad, PartyFunctionCodeQualifierList addressType, string codeID, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			nad.PartyFunctionCodeQualifier = addressType;
			nad.PartyIdentificationDetails.PartyIdentifier = codeID;
			nad.PartyIdentificationDetails.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulateMOA(MOASegment moa, MonetaryAmountTypeCodeQualifierList monetaryAmountTypeCodeQualifier, ZDecimal amount, string currencyCode, int decimalsToShow)
		{
			moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier = monetaryAmountTypeCodeQualifier;
			moa.MonetaryAmount.MonetaryAmount = amount.Round(decimalsToShow).ToString(decimalsToShow);
			if (!string.IsNullOrEmpty(currencyCode))
			{
				moa.MonetaryAmount.CurrencyIdentificationCode = currencyCode;
			}
		}

		internal static void PopulateCTA(CTASegment cta, ContactFunctionCodeList contactType, ZString contactName)
		{
			cta.ContactFunctionCode = contactType;
			if (!contactName.IsEmpty)
			{
				cta.DepartmentOrEmployeeDetails.DepartmentOrEmployeeName = contactName;
			}
		}

		internal static void PopulateCOM(COMSegment com, CommunicationNumberCodeQualifierList qualifier, ZString number)
		{
			com.CommunicationContact.CommunicationNumber = number;
			com.CommunicationContact.CommunicationNumberCodeQualifier = qualifier;
		}

		internal static void TrimString(ref string stringToTrim, int maximumCharacters)
		{
			if (stringToTrim != null && stringToTrim.Length > maximumCharacters)
			{
				stringToTrim = stringToTrim.Substring(0, maximumCharacters);
			}
		}

		#endregion

		#region Get Details

		internal static ZString GetDocumentReference(SegmentMessageSection<BGMSegment> bgmSection)
		{
			return bgmSection.Count > 0 ? bgmSection[0].DocumentMessageIdentification.DocumentIdentifier : string.Empty;
		}

		internal static ZString GetDocumentName(SegmentMessageSection<BGMSegment> bgmSection)
		{
			return bgmSection.Count > 0 ? bgmSection[0].DocumentMessageName.DocumentName : string.Empty;
		}

		internal static ZString GetProcessingIndicator(SegmentMessageSection<GISSegment> gisSection)
		{
			return gisSection.Count > 0 ? gisSection[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode : string.Empty;
		}

		internal static ZString GetReference(SegmentGroupMessageSection<SegmentGroup3> group3Section, ReferenceFunctionCodeQualifierList qualifier)
		{
			return (from SegmentGroup3 group3 in group3Section
					from RFFSegment rff in group3.RFF
					where rff.Reference.ReferenceFunctionCodeQualifier == qualifier
					select rff.Reference.ReferenceIdentifier).FirstOrDefault();
		}

		internal static IEnumerable<ZString> GetEquipment(SegmentGroupMessageSection<SegmentGroup6> group6Section, EquipmentTypeCodeQualifierList qualifier)
		{
			return from SegmentGroup6 group6 in group6Section
				   from EQDSegment eqd in group6.EQD
				   where eqd.EquipmentTypeCodeQualifier == qualifier
				   select new ZString(eqd.EquipmentIdentification.EquipmentIdentifier);
		}

		internal static string GetErrrorIdentifier(SegmentGroupMessageSection<SegmentGroup4> group4Section)
		{
			return (from SegmentGroup4 group4 in group4Section
					from ERPSegment erp in group4.ERP
					select erp.ErrorPointDetails.MessageSubItemIdentifier).FirstOrDefault();
		}

		internal static IEnumerable<string[]> GetErrorCodesAndRejectComments(SegmentGroupMessageSection<SegmentGroup4> group4Section)
		{
			foreach (SegmentGroup4 group4 in group4Section)
			{
				for (var i = 0; i < Math.Max(group4.ERC.Count, group4.FTX.Count); i++)
				{
					var code = i < group4.ERC.Count ? group4.ERC[i].ApplicationErrorDetail.ApplicationErrorCode : string.Empty;
					var comments = i < group4.FTX.Count ? group4.FTX[i].TextLiteral.FreeTextValue1 : string.Empty;
					yield return new[] { code, comments };
				}
			}
		}

		internal static bool CheckIsSyntaxError(SegmentGroupMessageSection<SegmentGroup4> group4Section)
		{
			return (from SegmentGroup4 group4 in group4Section
					from ERCSegment erc in group4.ERC
					where erc.ApplicationErrorDetail.ApplicationErrorCode == "ZZZ"
					select erc).Any();
		}

		internal static ZDateTime GetDate(SegmentMessageSection<DTMSegment> dtmSection, DateOrTimeOrPeriodFunctionCodeQualifierList qualifier)
		{
			return (from DTMSegment dtm in dtmSection
					where dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == qualifier
					select ParseDate(dtm.DateTimePeriod.DateOrTimeOrPeriodValue)).FirstOrDefault();
		}

		internal static ZDateTime ParseDate(string dateTime)
		{
			ZDateTime result;
			ZDateTime.TryParseExact(dateTime, out result, "yyyyMMddHHmm");
			return result;
		}
		#endregion
	}
}
