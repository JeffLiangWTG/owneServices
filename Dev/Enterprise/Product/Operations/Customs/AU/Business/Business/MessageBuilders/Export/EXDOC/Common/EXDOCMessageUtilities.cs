using System;
using CargoWise.Types;
using Enterprise.Edifact.D97BAU.Elements;
using Enterprise.Edifact.D97BAU.Messages.SANCRT;
using Enterprise.Edifact.D97BAU.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class EXDOCMessageUtilities
	{
		public static void PopulateUNH(UNHSegment uNH, MessageTypeList messageType, MessageVersionNumberList messageVersionNumber, MessageReleaseNumberList messageReleaseNumber, ControllingAgencyList controllingAgency, string associationAssignedCode)
		{
			uNH.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			uNH.MessageIdentifier.MessageType = messageType;
			uNH.MessageIdentifier.MessageVersionNumber = messageVersionNumber;
			uNH.MessageIdentifier.MessageReleaseNumber = messageReleaseNumber;
			uNH.MessageIdentifier.ControllingAgency = controllingAgency;
			uNH.MessageIdentifier.AssociationAssignedCode = associationAssignedCode;
		}

		public static void PopulateBGM(BGMSegment bGM, DocumentMessageNameCodedList documentMessageNameCode, CodeListResponsibleAgencyCodedList codeListResponsibleAgencyCode, string documentMessageName, string documentMessageNumber, MessageFunctionCodedList messageFunctionCode, ResponseTypeCodedList responseTypeCoded)
		{
			bGM.DocumentMessageName.DocumentMessageNameCoded = documentMessageNameCode;
			bGM.DocumentMessageName.CodeListResponsibleAgencyCoded = codeListResponsibleAgencyCode;
			bGM.DocumentMessageName.DocumentMessageName = documentMessageName;
			bGM.DocumentMessageIdentification.DocumentMessageNumber = documentMessageNumber;
			bGM.MessageFunctionCoded = messageFunctionCode;
			bGM.ResponseTypeCoded = responseTypeCoded;
		}

		public static void PopulateDTM(DTMSegment dTM, DateTimePeriodQualifierList dateTimePeriodQualifier, string dateTimePeriodValue, DateTimePeriodFormatQualifierList dateTimePeriodFormatQualifier)
		{
			dTM.DateTimePeriod.DateTimePeriodQualifier = dateTimePeriodQualifier;
			dTM.DateTimePeriod.DateTimePeriod = dateTimePeriodValue;
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = dateTimePeriodFormatQualifier;
		}

		public static void PopulateLOC(LOCSegment lOC, PlaceLocationQualifierList placeLocationQualifier, string locationIdentification)
		{
			PopulateLOC(lOC, placeLocationQualifier, locationIdentification, ZString.Empty);
		}

		public static void PopulateLOC(LOCSegment lOC, PlaceLocationQualifierList placeLocationQualifier, string locationIdentification, string relatedPlaceLocationOneIdentification)
		{
			lOC.PlaceLocationQualifier = placeLocationQualifier;
			lOC.LocationIdentification.PlaceLocationIdentification = locationIdentification;
			lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = relatedPlaceLocationOneIdentification;
		}

		public static void PopulateRFF(RFFSegment rFF, ReferenceQualifierList referenceQualifier, string referenceNumber)
		{
			rFF.Reference.ReferenceQualifier = referenceQualifier;
			rFF.Reference.ReferenceNumber = referenceNumber;
		}

		public static void PopulateATT(ATTSegment aTT, string codeListQualifier)
		{
			PopulateATT(aTT, codeListQualifier, "Y");
		}

		public static void PopulateATT(ATTSegment aTT, string codeListQualifier, bool codeValue)
		{
			PopulateATT(aTT, codeListQualifier, codeValue ? "Y" : "N");
		}

		public static void PopulateATT(ATTSegment aTT, string codeListQualifier, string codeValue)
		{
			aTT.AttributeFunctionQualifier = AttributeFunctionQualifierList.GetFromString("10");
			aTT.AttributeDetails.AttributeCoded = codeValue;
			aTT.AttributeDetails.CodeListQualifier = CodeListQualifierList.GetFromString(codeListQualifier);
			aTT.AttributeDetails.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.GetFromString("AQ");
		}

		public static void PopulateFTX(SANCRTMessage sANCRT, TextSubjectQualifierList subjectQualifier, int maxChars, int maxElementLength, ZString freeText)
		{
			TextSplitter splitter = new TextSplitter(maxElementLength);
			splitter.Text = freeText.Substring(0, maxChars);
			int elementCounter = 0;
			FTXSegment fTX;
			do
			{
				fTX = sANCRT.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = subjectQualifier;
				fTX.TextLiteral.FreeText1 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText2 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText3 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText4 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText5 = splitter[elementCounter++];
			}
			while (splitter.Count <= Math.Ceiling(maxChars / (double)maxElementLength) && splitter[elementCounter] != ZString.Empty);
		}

		public static void Populate8LineFTX(SANCRTMessage sANCRT, TextSubjectQualifierList subjectQualifier, int maxChars, int maxElementLength, ZString freeText)
		{
			TextSplitter splitter = new TextSplitter(maxElementLength);
			splitter.Text = freeText.Substring(0, maxChars);
			int elementCounter = 0;
			FTXSegment fTX;
			do
			{
				fTX = sANCRT.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = subjectQualifier;
				fTX.TextLiteral.FreeText1 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText2 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText3 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText4 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText5 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText6 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText7 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText8 = splitter[elementCounter++];
			}
			while (splitter.Count <= Math.Ceiling(maxChars / (double)maxElementLength) && splitter[elementCounter] != ZString.Empty);
		}

		public static void PopulateFTX(SegmentGroup11 group11, TextSubjectQualifierList subjectQualifier, ZString freeText1, ZString freeText2, ZString freeText3, ZString freeText4, ZString freeText5)
		{
			FTXSegment fTX = group11.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectQualifier = subjectQualifier;
			fTX.TextLiteral.FreeText1 = freeText1;
			fTX.TextLiteral.FreeText2 = freeText2;
			fTX.TextLiteral.FreeText3 = freeText3;
			fTX.TextLiteral.FreeText4 = freeText4;
			fTX.TextLiteral.FreeText5 = freeText5;
		}

		public static void PopulateFTX(SegmentGroup11 group11, TextSubjectQualifierList subjectQualifier, int maxChars, int maxElementLength, ZString freeText)
		{
			TextSplitter splitter = new TextSplitter(maxElementLength);
			splitter.Text = freeText.Substring(0, maxChars);
			int elementCounter = 0;
			FTXSegment fTX;
			do
			{
				fTX = group11.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = subjectQualifier;
				fTX.TextLiteral.FreeText1 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText2 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText3 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText4 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText5 = splitter[elementCounter++];
			}
			while (splitter.Count <= Math.Ceiling(maxChars / (double)maxElementLength) && splitter[elementCounter] != ZString.Empty);
		}

		public static void PopulateFTX(SegmentGroup16 group16, TextSubjectQualifierList subjectQualifier, int maxChars, int maxElementLength, ZString freeText)
		{
			TextSplitter splitter = new TextSplitter(maxElementLength);
			splitter.Text = freeText.Substring(0, maxChars);
			int elementCounter = 0;
			FTXSegment fTX;
			do
			{
				fTX = group16.FTX.InstantiateAChildAndAddItToChildrenCollection();
				fTX.TextSubjectQualifier = subjectQualifier;
				fTX.TextLiteral.FreeText1 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText2 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText3 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText4 = splitter[elementCounter++];
				fTX.TextLiteral.FreeText5 = splitter[elementCounter++];
			}
			while (splitter.Count <= Math.Ceiling(maxChars / (double)maxElementLength) && splitter[elementCounter] != ZString.Empty);
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementPurposeQualifierList measurementPurposeQualifier, PropertyMeasuredCodedList propertyMeasuredCode, string measureUnitQualifier, string measurementValue, string measurementMinValue, string measurementMaxValue)
		{
			PopulateMEA(mEA, measurementPurposeQualifier, propertyMeasuredCode, MeasurementSignificanceCodedList.GetFromString(""), measureUnitQualifier, measurementValue, measurementMinValue, measurementMaxValue);
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementPurposeQualifierList measurementPurposeQualifier, string measureUnitQualifier, string measurementValue)
		{
			PopulateMEA(mEA, measurementPurposeQualifier, PropertyMeasuredCodedList.GetFromString(ZString.Empty), MeasurementSignificanceCodedList.GetFromString(""), measureUnitQualifier, measurementValue, ZString.Empty, ZString.Empty);
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementPurposeQualifierList measurementPurposeQualifier, PropertyMeasuredCodedList propertyMeasuredCode, MeasurementSignificanceCodedList measurementSignificanceCode, string measureUnitQualifier, string measurementValue)
		{
			PopulateMEA(mEA, measurementPurposeQualifier, propertyMeasuredCode, measurementSignificanceCode, measureUnitQualifier, measurementValue, ZString.Empty, ZString.Empty);
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementPurposeQualifierList measurementPurposeQualifier, PropertyMeasuredCodedList propertyMeasuredCode, MeasurementSignificanceCodedList measurementSignificanceCode, string measureUnitQualifier, string measurementValue, string measurementMinValue, string measurementMaxValue)
		{
			mEA.MeasurementPurposeQualifier = measurementPurposeQualifier;
			mEA.MeasurementDetails.PropertyMeasuredCoded = propertyMeasuredCode;
			mEA.MeasurementDetails.MeasurementSignificanceCoded = measurementSignificanceCode;
			mEA.ValueRange.MeasureUnitQualifier = measureUnitQualifier;
			mEA.ValueRange.MeasurementValue = measurementValue;
			mEA.ValueRange.RangeMinimum = measurementMinValue;
			mEA.ValueRange.RangeMaximum = measurementMaxValue;
		}

		public static void PopulateMOA(MOASegment mOA, MonetaryAmountTypeQualifierList monetaryAmountTypeQualifier, string currencyCode)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = monetaryAmountTypeQualifier;
			mOA.MonetaryAmount.CurrencyCoded = ExDocCurrencyCodeConverter.ConvertToExDoc(currencyCode);
		}

		public static void PopulateMOA(MOASegment mOA, MonetaryAmountTypeQualifierList monetaryAmountTypeQualifier, ZDecimal monetaryAmount)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = monetaryAmountTypeQualifier;
			mOA.MonetaryAmount.MonetaryAmount = monetaryAmount.ToString(2);
		}

		public static void PopulateGIS(SANCRTMessage sANCRT, string processingIndicator, string processType)
		{
			const string CodeListResponsibleAgency = "AQ";
			GISSegment gIS = sANCRT.GIS.InstantiateAChildAndAddItToChildrenCollection();
			gIS.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.GetFromString(processingIndicator);
			gIS.ProcessingIndicator.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.GetFromString(CodeListResponsibleAgency);
			gIS.ProcessingIndicator.ProcessTypeIdentification = ProcessTypeIdentificationList.GetFromString(processType);
		}

		public static void PopulateDOC(DOCSegment dOC, DocumentMessageNameCodedList documentMessageNameCode, string documentMessageNumber)
		{
			PopulateDOC(dOC, documentMessageNameCode, ZString.Empty, documentMessageNumber, DocumentMessageStatusCodedList.GetFromString(ZString.Empty));
		}

		public static void PopulateDOC(DOCSegment dOC, DocumentMessageNameCodedList documentMessageNameCode, string documentMessageName, string documentMessageNumber, DocumentMessageStatusCodedList documentMessageStatusCode)
		{
			PopulateDOC(dOC, documentMessageNameCode, documentMessageName, documentMessageNumber, documentMessageStatusCode, ZString.Empty);
		}

		public static void PopulateDOC(DOCSegment dOC, DocumentMessageNameCodedList documentMessageNameCode, string documentMessageNumber, string documentMessageSource)
		{
			PopulateDOC(dOC, documentMessageNameCode, ZString.Empty, documentMessageNumber, DocumentMessageStatusCodedList.GetFromString(ZString.Empty), documentMessageSource);
		}

		public static void PopulateDOC(DOCSegment dOC, DocumentMessageNameCodedList documentMessageNameCode, string documentMessageName, string documentMessageNumber, DocumentMessageStatusCodedList documentMessageStatusCode, string documentMessageSource)
		{
			dOC.DocumentMessageName.DocumentMessageNameCoded = documentMessageNameCode;
			dOC.DocumentMessageName.DocumentMessageName = documentMessageName;
			dOC.DocumentMessageDetails.DocumentMessageNumber = documentMessageNumber;
			dOC.DocumentMessageDetails.DocumentMessageStatusCoded = documentMessageStatusCode;
			dOC.DocumentMessageDetails.DocumentMessageSource = documentMessageSource;
		}

		public static void PopulateGroup2FTX(FTXSegment fTX, TextSubjectQualifierList subjectQualifier, ZString tracesApprovalID)
		{
			fTX.TextSubjectQualifier = subjectQualifier;
			fTX.TextFunctionCoded = TextFunctionCodedList.NoActionRequired;
			fTX.TextReference.CodeListQualifier = CodeListQualifierList.PartyIdentification;
			fTX.TextReference.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.GetFromString("AQ");
			fTX.TextReference.FreeTextIdentification = "TRACESCONSIGNEEID";
			fTX.TextLiteral.FreeText1 = tracesApprovalID;
		}

		public static void PopulatePNA(PNASegment pNA, PartyQualifierList partyQualifier, string identityNumber)
		{
			PopulatePNA(pNA, partyQualifier, identityNumber, NameComponentQualifierList.GetFromString(""), "");
		}

		public static void PopulatePNA(PNASegment pNA, PartyQualifierList partyQualifier, string identityNumber, NameComponentQualifierList nameComponentQualifier, string nameComponent)
		{
			pNA.PartyQualifier = partyQualifier;
			pNA.IdentificationNumber.IdentityNumber = identityNumber;
			pNA.NameComponentDetails1.NameComponentQualifier = nameComponentQualifier;
			pNA.NameComponentDetails1.NameComponent = nameComponent;
		}

		public static void PopulateCTA(CTASegment cTA, ContactFunctionCodedList contactFunctionCode, string departmentOrEmployeeIdentification)
		{
			cTA.ContactFunctionCoded = contactFunctionCode;
			cTA.DepartmentOrEmployeeDetails.DepartmentOrEmployeeIdentification = departmentOrEmployeeIdentification;
		}

		public static void PopulateCTARepName(CTASegment cta, ContactFunctionCodedList contactFunctionCode, string representativeName)
		{
			cta.ContactFunctionCoded = contactFunctionCode;
			cta.DepartmentOrEmployeeDetails.DepartmentOrEmployee = representativeName;
		}

		public static void PopulateCOM(COMSegment cOM, CommunicationChannelQualifierList communicationChannelQualifier, ZString communicationNumber)
		{
			cOM.CommunicationContact.CommunicationNumber = communicationNumber.KeepChars("1234567890");
			cOM.CommunicationContact.CommunicationChannelQualifier = communicationChannelQualifier;
		}

		public static void PopulateADR(ADRSegment aDR, AddressFormatCodedList addressFormatCode, string address1, string address2, string cityName, string postcode, string countryCode, string state)
		{
			aDR.AddressDetails.AddressFormatCoded = addressFormatCode;
			aDR.AddressDetails.AddressComponent1 = address1;
			aDR.AddressDetails.AddressComponent2 = address2;
			aDR.CityName = cityName;
			aDR.PostcodeIdentification = postcode;
			aDR.CountryCoded = countryCode;
			aDR.CountrySubEntityDetails.CountrySubEntity = state;
		}

		public static void PopulateTDT(TDTSegment tDT, TransportStageQualifierList transportStageQualifier, string voyageFlightNumber, string modeOfTransportCode, string carrierName, string vesselName)
		{
			tDT.TransportStageQualifier = transportStageQualifier;
			tDT.ConveyanceReferenceNumber = voyageFlightNumber;
			tDT.ModeOfTransport.ModeOfTransportCoded = modeOfTransportCode;
			tDT.Carrier.CarrierName = carrierName;
			tDT.TransportIdentification.IdOfTheMeansOfTransport = vesselName;
		}

		public static void PopulateEQD(EQDSegment eQD)
		{
			const string VesselHold = "VH";
			PopulateEQD(eQD, EquipmentQualifierList.GetFromString(VesselHold), ZString.Empty);
		}

		public static void PopulateEQD(EQDSegment eQD, EquipmentQualifierList equipmentQualifier, string equipmentIdentificationNumber)
		{
			eQD.EquipmentQualifier = equipmentQualifier;
			eQD.EquipmentIdentification.EquipmentIdentificationNumber = equipmentIdentificationNumber;
		}

		public static void PopulateSEL(SELSegment sEL, string startSealNumber, string endSealNumber)
		{
			sEL.SealNumber = startSealNumber;
			sEL.SealIssuer.SealingPartyCoded = SealingPartyCodedList.GetFromString(endSealNumber);
		}

		public static void PopulateSEL(SELSegment sEL, string sealNumber, string startSeal, string endSeal)
		{
			if (!string.IsNullOrEmpty(sealNumber))
			{
				sEL.SealNumber = sealNumber;
			}

			if (!string.IsNullOrEmpty(startSeal))
			{
				sEL.SealIssuer.SealingPartyCoded = SealingPartyCodedList.GetFromString(startSeal);
			}

			if (!string.IsNullOrEmpty(endSeal))
			{
				sEL.SealConditionCoded = SealConditionCodedList.GetFromString(endSeal);
			}
		}

		public static void PopulatePRC(PRCSegment pRC, ProcessTypeIdentificationList processTypeIdentification)
		{
			pRC.ProcessTypeAndDescription.ProcessTypeIdentification = processTypeIdentification;
			pRC.ProcessTypeAndDescription.CodeListQualifier = CodeListQualifierList.GetFromString(ProductProcess);
			pRC.ProcessTypeAndDescription.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.GetFromString("AQ");
		}

		//public static void PopulateIMD(SegmentGroup11 Group11, int MaxChars, int MaxElementLength, string ItemDescriptionIdentification, ZString ItemDescription)
		//{
		//    TextSplitter Splitter = new TextSplitter(MaxElementLength);
		//    Splitter.Text = ItemDescription.Substring(0, MaxChars);
		//    int ElementCounter = 0;
		//    IMDSegment IMD;
		//    do
		//    {
		//        IMD = Group11.IMD.InstantiateAChildAndAddItToChildrenCollection();
		//        IMD.ItemDescription.ItemDescriptionIdentification = ItemDescriptionIdentification;
		//        IMD.ItemDescription.ItemDescription1 = Splitter[ElementCounter++];
		//        IMD.ItemDescription.ItemDescription2 = Splitter[ElementCounter++];
		//    }
		//    while (Splitter.Count <= Math.Ceiling(MaxChars / (double)MaxElementLength) && Splitter[ElementCounter] != ZString.Empty);
		//}

		public static void PopulateIMD(IMDSegment iMD, string itemDescriptionIdentification, string itemDescription1)
		{
			PopulateIMD(iMD, ItemCharacteristicCodedList.GetFromString(ZString.Empty), itemDescriptionIdentification, itemDescription1);
		}

		public static void PopulateIMD(IMDSegment iMD, ItemCharacteristicCodedList itemCharacteristicCode, string itemDescriptionIdentification, ZString itemDescription)
		{
			iMD.ItemCharacteristicCoded = itemCharacteristicCode;
			iMD.ItemDescription.ItemDescriptionIdentification = itemDescriptionIdentification;
			iMD.ItemDescription.ItemDescription1 = itemDescription.SubstringSafe(0, 35);
			iMD.ItemDescription.ItemDescription2 = itemDescription.SubstringSafe(35, 35);
			iMD.ItemDescription.ItemDescription3 = itemDescription.SubstringSafe(70, 35);
			iMD.ItemDescription.ItemDescription4 = itemDescription.SubstringSafe(105, 35);
			iMD.ItemDescription.ItemDescription5 = itemDescription.SubstringSafe(140, 35);
			iMD.ItemDescription.ItemDescription6 = itemDescription.SubstringSafe(175, 35);
			iMD.ItemDescription.ItemDescription7 = itemDescription.SubstringSafe(210, 35);
			iMD.ItemDescription.ItemDescription8 = itemDescription.SubstringSafe(245, 35);
		}

		public static void PopulateLIN(LINSegment lIN, string lineNumber)
		{
			lIN.LineItemNumber = lineNumber;
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementPurposeQualifierList measurementPurposeQualifier, PropertyMeasuredCodedList propertyMeasuredCoded, string measureUnitQualifier, string measurementValue)
		{
			PopulateMEA(mEA, measurementPurposeQualifier, propertyMeasuredCoded, measureUnitQualifier, measurementValue, ZString.Empty, ZString.Empty);
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementPurposeQualifierList measurementPurposeQualifier, PropertyMeasuredCodedList propertyMeasuredCoded, string measurementValue)
		{
			PopulateMEA(mEA, measurementPurposeQualifier, propertyMeasuredCoded, ZString.Empty, measurementValue, ZString.Empty, ZString.Empty);
		}

		public static void PopulatePIA(PIASegment pIA, ProductIdFunctionQualifierList productIdFunctionQualifier, string productCode, ItemNumberTypeCodedList itemNumberTypeCode)
		{
			pIA.ProductIdFunctionQualifier = productIdFunctionQualifier;
			pIA.ItemNumberIdentification1.ItemNumber = productCode;
			pIA.ItemNumberIdentification1.ItemNumberTypeCoded = itemNumberTypeCode;
		}

		public static void PopulatePAC(PACSegment pAC, string numberOfPackages, PackagingLevelCodedList packagingLevelCode, string typeOfPackagesIdentification)
		{
			pAC.NumberOfPackages = numberOfPackages;
			pAC.PackagingDetails.PackagingLevelCoded = packagingLevelCode;
			pAC.PackageType.TypeOfPackagesIdentification = typeOfPackagesIdentification;
			pAC.PackageType.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.GetFromString("AQ");
		}

		public static void PopulatePCI(PCISegment pCI, string shippingMarks)
		{
			pCI.MarksLabels.ShippingMarks1 = shippingMarks;
		}

		public static void PopulateGIN(GINSegment gIN, IdentityNumberQualifierList identityNumberQualifier, string identityNumber1)
		{
			gIN.IdentityNumberQualifier = identityNumberQualifier;
			gIN.IdentityNumberRange1.IdentityNumber1 = identityNumber1;
		}

		public static void PopulateUNT(UNTSegment uNT, string numberOfSegmentsInTheMessage)
		{
			uNT.NumberOfSegmentsInTheMessage = numberOfSegmentsInTheMessage;
			uNT.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}

		public const string ProductProcess = "PP";
		public const string LabelApprovalIndicatorQualifier = "LAI";
		public const string UngradedProductIndicatorQualifier = "UPI";
		public const string HalalProductIndicatorQualifier = "HPI";
		public const string FishWaterIndicatorQualifier = "FWI";
		public const string FinalConsumerIndicator = "FCI";
		public const string TreatmentActiveIngredient = "TAI";
	}
}
