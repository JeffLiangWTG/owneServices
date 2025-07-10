using System.Collections;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Edifact.Utilities;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class MessageUtilities
	{
		public static void PopulatePCI(PCISegment pCI, MarkingInstructionsCodedList markingInstructionsCoded, string marksAndNumbers)
		{
			pCI.MarkingInstructionsCoded = markingInstructionsCoded;
			TextSplitter splitter = new TextSplitter(35);
			splitter.Text = marksAndNumbers.Replace("\r\n", " ");
			pCI.MarksLabels.ShippingMarks1 = splitter[0];
			pCI.MarksLabels.ShippingMarks2 = splitter[1];
			pCI.MarksLabels.ShippingMarks3 = splitter[2];
			pCI.MarksLabels.ShippingMarks4 = splitter[3];
			pCI.MarksLabels.ShippingMarks5 = splitter[4];
			pCI.MarksLabels.ShippingMarks6 = splitter[5];
			pCI.MarksLabels.ShippingMarks7 = splitter[6];
			pCI.MarksLabels.ShippingMarks8 = splitter[7];
			pCI.MarksLabels.ShippingMarks9 = splitter[8];
		}

		public static void PopulatePAC(PACSegment pAC, string packageTypeDescriptionCode, CodeListIdentificationCodeList codeListIdentificationCode, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			pAC.PackageType.PackageTypeDescriptionCode = packageTypeDescriptionCode;
			pAC.PackageType.CodeListIdentificationCode = codeListIdentificationCode;
			pAC.PackageType.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulatePAC(PACSegment pAC, int numberOfPackages)
		{
			pAC.NumberOfPackages = numberOfPackages.ToString();
		}

		public static void PopulatePAC(PACSegment pAC, int numberOfPackages, string packageTypeDescriptionCode, CodeListIdentificationCodeList codeListIdentificationCode, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			pAC.NumberOfPackages = numberOfPackages.ToString();
			pAC.PackageType.PackageTypeDescriptionCode = packageTypeDescriptionCode;
			pAC.PackageType.CodeListIdentificationCode = codeListIdentificationCode;
			pAC.PackageType.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulateRFF(RFFSegment rFF, ReferenceFunctionCodeQualifierList referenceFunctionCodeQualifier, string referenceIdentifier, string lineNumber)
		{
			rFF.Reference.ReferenceFunctionCodeQualifier = referenceFunctionCodeQualifier;
			rFF.Reference.ReferenceIdentifier = referenceIdentifier;
			rFF.Reference.LineNumber = lineNumber;
		}

		public static void PopulateUNH(UNHSegment uNH, string messageReferenceNumber, MessageTypeList messageType, MessageVersionNumberList messageVersionNumber, MessageReleaseNumberList messageReleaseNumber, ControllingAgencyList controllingAgency)
		{
			uNH.MessageReferenceNumber = messageReferenceNumber;
			uNH.MessageIdentifier.MessageType = messageType;
			uNH.MessageIdentifier.MessageVersionNumber = messageVersionNumber;
			uNH.MessageIdentifier.MessageReleaseNumber = messageReleaseNumber;
			uNH.MessageIdentifier.ControllingAgency = controllingAgency;
		}

		public static void PopulateBGM(BGMSegment bGM, DocumentNameCodeList documentNameCode, string documentName, string documentMessageNumber, string version, MessageFunctionCodeList messageFunctionCode)
		{
			bGM.DocumentMessageName.DocumentNameCode = documentNameCode;
			bGM.DocumentMessageName.DocumentName = documentName;
			bGM.DocumentMessageIdentification.DocumentMessageNumber = documentMessageNumber;
			bGM.DocumentMessageIdentification.Version = version;
			bGM.MessageFunctionCode = messageFunctionCode;
		}

		public static void PopulateLOC(LOCSegment lOC, LocationFunctionCodeQualifierList locationFunctionCodeQualifier, string locationNameCode, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			lOC.LocationFunctionCodeQualifier = locationFunctionCodeQualifier;
			lOC.LocationIdentification.LocationNameCode = locationNameCode;
			lOC.LocationIdentification.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulateLOC(LOCSegment lOC, LocationFunctionCodeQualifierList locationFunctionCodeQualifier, string locationNameCode, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode, string relatedPlaceLocationOneIdentification, CodeListResponsibleAgencyCodeList relatedPlaceLocationOneCodeListResponsibleAgencyCode)
		{
			lOC.LocationFunctionCodeQualifier = locationFunctionCodeQualifier;
			if (locationNameCode != null && !string.IsNullOrEmpty(locationNameCode))
			{
				lOC.LocationIdentification.LocationNameCode = locationNameCode;
				lOC.LocationIdentification.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
			}
			lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = relatedPlaceLocationOneIdentification;
			lOC.RelatedLocationOneIdentification.CodeListResponsibleAgencyCode = relatedPlaceLocationOneCodeListResponsibleAgencyCode;
		}

		public static void PopulateDTM(DTMSegment dTM, DateTimePeriodFunctionCodeQualifierList dateTimePeriodFunctionCodeQualifier, string dateTimePeriodValue, DateTimePeriodFormatCodeList dateTimePeriodFormatCode)
		{
			dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = dateTimePeriodFunctionCodeQualifier;
			dTM.DateTimePeriod.DateTimePeriodValue = dateTimePeriodValue;
			dTM.DateTimePeriod.DateTimePeriodFormatCode = dateTimePeriodFormatCode;
		}

		public static void PopulateGIS(GISSegment gIS, ProcessingIndicatorDescriptionCodeList processingIndicatorDescriptionCode, CodeListIdentificationCodeList codeListIdentificationCode, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = processingIndicatorDescriptionCode;
			gIS.ProcessingIndicator_X.CodeListIdentificationCode = codeListIdentificationCode;
			gIS.ProcessingIndicator_X.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulateCST(CSTSegment cST, string lineNumber, string lineAction)
		{
			cST.GoodsItemNumber = lineNumber;
			if (lineAction != null && !string.IsNullOrEmpty(lineAction))
			{
				cST.CustomsIdentityCodes1.CustomsCodeIdentification = lineAction;
				cST.CustomsIdentityCodes1.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
			}
		}

		public static void PopulateCST(CSTSegment cST, string customsCodeIdent, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			cST.CustomsIdentityCodes1.CustomsCodeIdentification = customsCodeIdent;
			cST.CustomsIdentityCodes1.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulateFTX(FTXSegment fTX, TextSubjectCodeQualifierList textSubjectCodeQualifier, string textValue)
		{
			fTX.TextSubjectCodeQualifier = textSubjectCodeQualifier;
			if (textSubjectCodeQualifier == TextSubjectCodeQualifierList.GoodsDescription)
			{
				TextSplitter descriptionSplitter = new TextSplitter(512);
				descriptionSplitter.Text = textValue.Replace("\r\n", " ");
				fTX.TextLiteral.FreeTextValue1 = descriptionSplitter[0];
				fTX.TextLiteral.FreeTextValue2 = descriptionSplitter[1];
				fTX.TextLiteral.FreeTextValue3 = descriptionSplitter[2];
				fTX.TextLiteral.FreeTextValue4 = descriptionSplitter[3];
				fTX.TextLiteral.FreeTextValue5 = descriptionSplitter[4];
			}
			else
			{
				fTX.TextLiteral.FreeTextValue1 = textValue;
			}
		}

		public static void PopulateFTX(FTXSegment fTX, TextSubjectCodeQualifierList textSubjectCodeQualifier, TextFunctionCodedList textFunctionCoded)
		{
			fTX.TextSubjectCodeQualifier = textSubjectCodeQualifier;
			fTX.TextFunctionCoded = textFunctionCoded;
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementAttributeCodeList measurementAttributeCode, string measuredAttributeCode, string measurementUnitCode, string measurementValue)
		{
			PopulateMEA(mEA, measurementAttributeCode, MeasuredAttributeCodeList.GetFromString(measuredAttributeCode), measurementUnitCode, measurementValue);
		}

		public static void PopulateMEA(MEASegment mEA, MeasurementAttributeCodeList measurementAttributeCode, MeasuredAttributeCodeList measuredAttributeCode, string measurementUnitCode, string measurementValue)
		{
			mEA.MeasurementAttributeCode = measurementAttributeCode;
			mEA.MeasurementDetails.MeasuredAttributeCode = measuredAttributeCode;
			mEA.ValueRange.MeasurementUnitCode = measurementUnitCode;
			mEA.ValueRange.MeasurementValue = measurementValue;
		}

		public static void PopulateEQD(EQDSegment eQD, EquipmentTypeCodeQualifierList equipmentTypeCodeQualifier, string eQDValue, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode)
		{
			eQD.EquipmentTypeCodeQualifier = equipmentTypeCodeQualifier;
			eQD.EquipmentIdentification.EquipmentIdentificationNumber = eQDValue;
			eQD.EquipmentIdentification.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
		}

		public static void PopulateMOA(MOASegment mOA, MonetaryAmountTypeCodeQualifierList monetaryAmountTypeCodeQualifier, string monetaryAmountValue, string currencyIdentificationCode)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = monetaryAmountTypeCodeQualifier;
			mOA.MonetaryAmount.MonetaryAmountValue = monetaryAmountValue;
			mOA.MonetaryAmount.CurrencyIdentificationCode = currencyIdentificationCode;
		}

		public static void PopulateNAD(NADSegment nAD, PartyFunctionCodeQualifierList partyFunctionCodeQualifier, string partyIdentifier, CodeListResponsibleAgencyCodeList codeListResponsibleAgencyCode, string partyName, string partyCity)
		{
			nAD.PartyFunctionCodeQualifier = partyFunctionCodeQualifier;
			if (partyIdentifier != null)
			{
				partyIdentifier = partyIdentifier.Replace(" ", "").Replace("-", "");
				TrimString(ref partyIdentifier, 35);
				nAD.PartyIdentificationDetails.PartyIdentifier = partyIdentifier;
				nAD.PartyIdentificationDetails.CodeListResponsibleAgencyCode = codeListResponsibleAgencyCode;
			}
			if (partyName != null)
			{
				TrimString(ref partyName, 35);
				nAD.PartyName.PartyName1 = partyName;
			}
			TrimString(ref partyCity, 35);
			nAD.CityName = partyCity;
		}

		public static void PopulateTDT(TDTSegment tDT, TransportStageCodeQualifierList transportStageCodeQualifier, string conveyanceReferenceNumber, TransportMeansDescriptionCodeList transportMeansDescriptionCode, string carrierIdentification, CodeListResponsibleAgencyCodeList carrierResponsibleAgency, string transportMeansIdentificationNameIdentifier)
		{
			tDT.TransportStageCodeQualifier = transportStageCodeQualifier;
			tDT.ConveyanceReferenceNumber = conveyanceReferenceNumber;
			tDT.TransportMeans.TransportMeansDescriptionCode = transportMeansDescriptionCode;
			if (carrierIdentification != null)
			{
				carrierIdentification = carrierIdentification.Replace(" ", "").Replace("-", "");
				tDT.Carrier.CarrierIdentification = carrierIdentification;
				tDT.Carrier.CodeListResponsibleAgencyCode = carrierResponsibleAgency;
			}
			if (transportMeansIdentificationNameIdentifier != null)
			{
				tDT.TransportIdentification.TransportMeansIdentificationNameIdentifier = transportMeansIdentificationNameIdentifier;
				tDT.TransportIdentification.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.LloydsRegisterOfShipping;
			}
		}

		public static void PopulateTDT(TDTSegment tDT, TransportStageCodeQualifierList transportStageCodeQualifier, string conveyanceReferenceNumber, string transportModeNameCode, string carrierIdentification, CodeListResponsibleAgencyCodeList carrierResponsibleAgency, string transportMeansIdentificationNameIdentifier, CodeListResponsibleAgencyCodeList trasnportResponsibleAgency)
		{
			tDT.TransportStageCodeQualifier = transportStageCodeQualifier;
			tDT.ConveyanceReferenceNumber = conveyanceReferenceNumber;
			tDT.ModeOfTransport.TransportModeNameCode = transportModeNameCode;
			tDT.Carrier.CarrierIdentification = carrierIdentification;
			tDT.Carrier.CodeListResponsibleAgencyCode = carrierResponsibleAgency;
			if (transportMeansIdentificationNameIdentifier != null && !string.IsNullOrEmpty(transportMeansIdentificationNameIdentifier))
			{
				tDT.TransportIdentification.TransportMeansIdentificationNameIdentifier = transportMeansIdentificationNameIdentifier;
				tDT.TransportIdentification.CodeListResponsibleAgencyCode = trasnportResponsibleAgency;
			}
		}

		public static void PopulateCNT(CNTSegment cNT, ControlTotalTypeCodeQualifierList controlTotalTypeCodeQualifier, string controlValue)
		{
			cNT.Control.ControlTotalTypeCodeQualifier = controlTotalTypeCodeQualifier;
			cNT.Control.ControlValue = controlValue;
		}

		public static void PopulateCNI(CNISegment cNI, string consolidationItemNumber, string languageNameCode)
		{
			cNI.ConsolidationItemNumber = consolidationItemNumber;
			cNI.DocumentMessageDetails.LanguageNameCode = languageNameCode;
		}

		public static void PopulateGID(GIDSegment gID, string goodsItemNumber)
		{
			gID.GoodsItemNumber = goodsItemNumber;
		}

		public static void PopulateUNT(UNTSegment uNT, string numberOfSegmentsInTheMessage, string messageReferenceNumber)
		{
			uNT.NumberOfSegmentsInTheMessage = numberOfSegmentsInTheMessage;
			uNT.MessageReferenceNumber = messageReferenceNumber;
		}

		public static void PopulateUNS(UNSSegment uNS, SectionIdentificationList sectionIdentification)
		{
			uNS.SectionIdentification = sectionIdentification;
		}

		public static void PopulateDMS(DMSSegment dMS, string documentMessageNumber)
		{
			dMS.DocumentMessageIdentification.DocumentMessageNumber = documentMessageNumber;
		}

		public static void PopulateSTS(STSSegment sTS, StatusDescriptionCodeList statusDescriptionCode)
		{
			sTS.Status.StatusDescriptionCode = statusDescriptionCode;
			sTS.Status.CodeListIdentificationCode = CodeListIdentificationCodeList.HandlingAction;
			sTS.Status.CodeListResponsibleAgencyCode = CodeListResponsibleAgencyCodeList.AuAustralianCustomsService;
		}

		public static void SetAllMaximumsToZero(object message)
		{
			Queue myQueue = new Queue();
			myQueue.Enqueue(message);
			while (myQueue.Count > 0)
			{
				object myObj = myQueue.Dequeue();
				System.Reflection.FieldInfo[] fields = myObj.GetType().GetFields();
				foreach (System.Reflection.FieldInfo field in fields)
				{
					string fieldName = field.Name;
					if (fieldName.Length == 3 || fieldName.StartsWith("Group"))//segment or segment group
					{
						object fieldValue = field.GetValue(myObj);
						if (fieldValue != null)
						{
							System.Reflection.PropertyInfo childrenProperty = fieldValue.GetType().GetProperty("Children");
							if (childrenProperty != null)
							{
								ArrayList children = (ArrayList)childrenProperty.GetValue(fieldValue, null);
								foreach (object child in children)
								{
									myQueue.Enqueue(child);
								}
								System.Reflection.PropertyInfo property = fieldValue.GetType().GetProperty("MaximumRepetitions");
								if (property != null)
								{
									property.SetValue(fieldValue, 0, null);
								}
							}
						}
					}
				}
			}
		}

		static void TrimString(ref string stringToTrim, int maximumCharacters)
		{
			if (stringToTrim != null && stringToTrim.Length > maximumCharacters)
			{
				stringToTrim = stringToTrim.Substring(0, maximumCharacters);
			}
		}
	}
}
