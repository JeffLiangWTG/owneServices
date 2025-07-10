using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.MasterFiles.Integration;
using D99BCUSRES = Enterprise.Edifact.D99B.Messages.CUSRES;
using D99BMessageTypeList = Enterprise.Edifact.D99B.Elements.MessageTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Messaging
{
	public static class D99BMessageUtilities
	{
		#region Populate Details

		#region PopulateUNH

		public static void PopulateUNH(UNHSegment uNH, string messageReferenceNumber, D99BMessageTypeList messageType,
										string messageVersionNumber, string messageReleaseNumber, ControllingAgencyList controllingAgency)
		{
			uNH.MessageReferenceNumber = messageReferenceNumber;
			uNH.MessageIdentifier.MessageType = messageType;
			uNH.MessageIdentifier.MessageVersionNumber = messageVersionNumber;
			uNH.MessageIdentifier.MessageReleaseNumber = messageReleaseNumber;
			uNH.MessageIdentifier.ControllingAgency = controllingAgency;
		}

		#endregion

		#region PopulateBGM

		public static void PopulateBGM(BGMSegment bGM, string documentName, ZString documentMessageNumber, MessageFunctionCodeList messageFunctionCode)
		{
			bGM.DocumentMessageName.DocumentName = documentName;
			if (!documentMessageNumber.IsEmpty)
			{
				bGM.DocumentMessageIdentification.DocumentMessageNumber = documentMessageNumber;
			}
			bGM.MessageFunctionCode = messageFunctionCode;
		}

		public static void PopulateBGM(BGMSegment bGM, string documentName, string documentMessageNumber, string documentMessageVersion, MessageFunctionCodeList messageFunctionCode)
		{
			PopulateBGM(bGM, documentName, documentMessageNumber, messageFunctionCode);
			bGM.DocumentMessageIdentification.Version = documentMessageVersion;
		}

		public static void PopoulateBGMWithDocumentNameCode(BGMSegment bGM, DocumentNameCodeList documentNameCode, string documentMessageNumber, MessageFunctionCodeList messageFunctionCode)
		{
			bGM.DocumentMessageName.DocumentNameCode = documentNameCode;
			bGM.DocumentMessageIdentification.DocumentMessageNumber = documentMessageNumber;
			bGM.MessageFunctionCode = messageFunctionCode;
		}

		#endregion

		#region PopulateCST

		public static void PopulateCST(CSTSegment cST, string customsIdentityCode)
		{
			cST.CustomsIdentityCodes1.CustomsCodeIdentification = customsIdentityCode;
		}

		public static void PopulateCST(CSTSegment cST, string goodsItemNumber, string customsIdentityCode1,
										string customsIdentityCode2, string customsIdentityCode3, string customsIdentityCode4, string customsIdentityCode5)
		{
			PouplateCST(cST, goodsItemNumber, customsIdentityCode1, null, customsIdentityCode2, null, customsIdentityCode3, null, customsIdentityCode4, null, customsIdentityCode5, null);
		}

		public static void PouplateCST(CSTSegment cST, string goodsItemNumber, string customsIdentityCode1, CodeListIdentificationCodeList identificationCode1
			, string customsIdentityCode2 = "", CodeListIdentificationCodeList identificationCode2 = null, string customsIdentityCode3 = "", CodeListIdentificationCodeList identificationCode3 = null
			, string customsIdentityCode4 = "", CodeListIdentificationCodeList identificationCode4 = null, string customsIdentityCode5 = "", CodeListIdentificationCodeList identificationCode5 = null)
		{
			if (!string.IsNullOrEmpty(goodsItemNumber))
			{
				cST.GoodsItemNumber = goodsItemNumber;
			}
			SetIdentityCodes(cST.CustomsIdentityCodes1, customsIdentityCode1, identificationCode1);
			SetIdentityCodes(cST.CustomsIdentityCodes2, customsIdentityCode2, identificationCode2);
			SetIdentityCodes(cST.CustomsIdentityCodes3, customsIdentityCode3, identificationCode3);
			SetIdentityCodes(cST.CustomsIdentityCodes4, customsIdentityCode4, identificationCode4);
			SetIdentityCodes(cST.CustomsIdentityCodes5, customsIdentityCode5, identificationCode5);
		}

		static void SetIdentityCodes(CustomsIdentityCodesElements elements, string code, CodeListIdentificationCodeList identificationCode)
		{
			if (!string.IsNullOrEmpty(code))
			{
				elements.CustomsCodeIdentification = code;
			}
			if (identificationCode != null)
			{
				elements.CodeListIdentificationCode = identificationCode;
			}
		}

		#endregion

		#region PopulateLOC

		public static void PopulateLOC(LOCSegment lOC, LocationFunctionCodeQualifierList qualifier, string locationNameCode)
		{
			lOC.LocationFunctionCodeQualifier = qualifier;
			lOC.LocationIdentification.LocationNameCode = locationNameCode;
		}

		public static void PopulateLOC(LOCSegment lOC, string countryOfOrigin, string placeOfExport, string usPortOfExitCode)
		{
			PopulateLOC(lOC, LocationFunctionCodeQualifierList.CountryOfOrigin, countryOfOrigin);
			lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = placeOfExport;
			if (!string.IsNullOrEmpty(usPortOfExitCode))
			{
				lOC.RelatedLocationTwoIdentification.RelatedPlaceLocationTwoIdentification = usPortOfExitCode;
			}
		}

		#endregion

		#region PopulateRFF

		public static void PopulateRFF(RFFSegment rFF, ReferenceFunctionCodeQualifierList referenceQualifier, string referenceIdentifier)
		{
			PopulateRFF(rFF, referenceQualifier, referenceIdentifier, string.Empty);
		}

		public static void PopulateRFF(RFFSegment rFF, ReferenceFunctionCodeQualifierList referenceQualifier, string referenceIdentifier, string lineNumber)
		{
			rFF.Reference.ReferenceFunctionCodeQualifier = referenceQualifier;
			if (!string.IsNullOrEmpty(referenceIdentifier))
			{
				rFF.Reference.ReferenceIdentifier = referenceIdentifier;
			}

			if (!string.IsNullOrEmpty(lineNumber))
			{
				rFF.Reference.LineNumber = lineNumber;
			}
		}

		#endregion

		#region PopulateERC

		public static void PopulateERC(ERCSegment erc, string errorIdentification)
		{
			erc.ApplicationErrorDetail.ApplicationErrorIdentification = errorIdentification;
		}

		#endregion

		#region PopulateTDT

		public static void PopulateTDT(TDTSegment tDT, TransportStageCodeQualifierList transportStageQualifier, string transportModeNameCode, string carrierIdentifier = "")
		{
			tDT.TransportStageCodeQualifier = transportStageQualifier;
			tDT.ModeOfTransport.TransportModeNameCode = transportModeNameCode;
			tDT.Carrier.CarrierIdentification = carrierIdentifier;
		}

		#endregion

		#region PopulateDOC

		public static void PopulateDOC(DOCSegment dOC, DocumentNameCodeList documentNameCode, string documentMessageNumber)
		{
			dOC.DocumentMessageName.DocumentNameCode = documentNameCode;
			if (!string.IsNullOrEmpty(documentMessageNumber))
			{
				dOC.DocumentMessageDetails.DocumentMessageNumber = documentMessageNumber;
			}
		}

		public static void PopulateDOC(DOCSegment dOC, DocumentNameCodeList documentNameCode, string documentMessageNumber, string documentMessageSource)
		{
			dOC.DocumentMessageName.DocumentNameCode = documentNameCode;
			dOC.DocumentMessageDetails.DocumentMessageNumber = documentMessageNumber;
			dOC.DocumentMessageDetails.DocumentMessageSource = documentMessageSource;
		}

		#endregion

		#region PopulateDTM

		public static void PopulateDTM(DTMSegment dTM, DateTimePeriodFunctionCodeQualifierList dateTimePeriodQualifier, ZDateTime dateTimePeriodValue)
		{
			dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = dateTimePeriodQualifier;
			dTM.DateTimePeriod.DateTimePeriodValue = dateTimePeriodValue.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			dTM.DateTimePeriod.DateTimePeriodFormatCode = DateTimePeriodFormatCodeList.Ccyymmdd;
		}

		public static void PopulateDTM(DTMSegment dTM, DateTimePeriodFunctionCodeQualifierList dateTimePeriodQualifier, ZDateTime dateTimePeriodValue, string dateTimePeriodValueFormat, DateTimePeriodFormatCodeList formatCode)
		{
			dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = dateTimePeriodQualifier;
			dTM.DateTimePeriod.DateTimePeriodValue = dateTimePeriodValue.ToString(dateTimePeriodValueFormat, CultureInfo.InvariantCulture);
			dTM.DateTimePeriod.DateTimePeriodFormatCode = formatCode;
		}

		#endregion

		#region PopulateGIS

		public static void PopulateGIS(GISSegment gis, ProcessingIndicatorDescriptionCodeList processingIndicatorDescriptionCode)
		{
			gis.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode = processingIndicatorDescriptionCode;
		}

		#endregion

		#region PopulateERP

		public static void PopulateERP(ERPSegment erp, MessageSectionCodedList messageSectionCoded, ZString messageItemNumber, ZString subItemNumber)
		{
			if (messageSectionCoded != null)
			{
				erp.ErrorPointDetails.MessageSectionCoded = messageSectionCoded;
			}
			erp.ErrorPointDetails.MessageItemNumber = messageItemNumber;
			if (!subItemNumber.IsEmpty)
			{
				erp.ErrorPointDetails.MessageSubItemNumber = subItemNumber;
			}
		}

		#endregion

		#region PopulateFTX

		public static void PopulateFTX(FTXSegment ftx, TextSubjectCodeQualifierList textSubjectCodeQualifier, ZString errorText1, ZString errorText2)
		{
			ftx.TextSubjectCodeQualifier = textSubjectCodeQualifier;
			ftx.TextLiteral.FreeTextValue1 = errorText1;
			ftx.TextLiteral.FreeTextValue2 = errorText2;
		}

		#endregion

		#region PopulateMOA

		public static void PopulateMOA(MOASegment mOA, MonetaryAmountTypeCodeQualifierList monetaryAmountTypeQualifier, ZDecimal amount)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = monetaryAmountTypeQualifier;
			mOA.MonetaryAmount.MonetaryAmountValue = (amount * 100).ToString("000", CultureInfo.InvariantCulture); //0.00
		}

		public static void PopulateMOA(MOASegment mOA, MonetaryAmountTypeCodeQualifierList monetaryAmountTypeQualifier, string currencyCode)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = monetaryAmountTypeQualifier;
			mOA.MonetaryAmount.CurrencyIdentificationCode = currencyCode;
		}

		public static void PopulateMOARounded(MOASegment mOA, MonetaryAmountTypeCodeQualifierList monetaryAmountTypeQualifier, ZDecimal amount)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = monetaryAmountTypeQualifier;
			mOA.MonetaryAmount.MonetaryAmountValue = amount.Round(0).ToString("0", CultureInfo.InvariantCulture);
		}

		public static void PopulateMOA(MOASegment mOA, MonetaryAmountTypeCodeQualifierList monetaryAmountTypeQualifier)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = monetaryAmountTypeQualifier;
		}

		#endregion

		#region PopulateUNS

		public static void PopulateUNS(UNSSegment uNS, SectionIdentificationList sectionIdentification)
		{
			uNS.SectionIdentification = sectionIdentification;
		}

		#endregion

		#region PopulateDMS

		public static void PopulateDMS(DMSSegment dMS, string docReference)
		{
			dMS.DocumentMessageIdentification.DocumentMessageNumber = docReference;
		}

		public static void PopulateDMSWithNameCode(DMSSegment dMS, DocumentNameCodeList nameCode)
		{
			dMS.DocumentMessageName.DocumentNameCode = nameCode;
		}

		#endregion

		#region PopulateNAD

		public static void PopulateNAD(NADSegment nAD, PartyFunctionCodeQualifierList qualifier, IDocAddress docAddress, ZString overrideStateCode, ZString overrideZipCode)
		{
			if (docAddress != null)
			{
				nAD.PartyFunctionCodeQualifier = qualifier;
				nAD.NameAndAddress.NameAndAddressLine1 = docAddress.E2_CompanyName.Left(30);
				if (!overrideStateCode.IsEmpty)
				{
					nAD.CountrySubEntityDetails.CountrySubEntityNameCode = overrideStateCode.TrimStart().Left(3).PadRight(3);
				}

				if (!overrideZipCode.IsEmpty)
				{
					nAD.PostalIdentificationCode = overrideZipCode.TrimStart().Left(5);
				}
			}
		}

		#endregion

		#region PopulatePAT

		public static void PopulatePAT(PATSegment pAT, PaymentTermsTypeCodeQualifierList qualifier, string descriptionIdentifier, string description,
										TimeReferenceCodeList timeReferenceCode, string periodTypeCode, string periodCountQuantity)
		{
			pAT.PaymentTermsTypeCodeQualifier = qualifier;
			pAT.PaymentTerms_X.PaymentTermsDescriptionIdentifier = PaymentTermsDescriptionIdentifierList.GetFromString(descriptionIdentifier);
			pAT.PaymentTerms_X.PaymentTermsDescription1 = description;

			if (!(string.IsNullOrEmpty(periodTypeCode) || string.IsNullOrEmpty(periodCountQuantity)))
			{
				pAT.TermsTimeInformation_X.TimeReferenceCode = timeReferenceCode;
				pAT.TermsTimeInformation_X.PeriodTypeCode = PeriodTypeCodeList.GetFromString(periodTypeCode);
				pAT.TermsTimeInformation_X.PeriodCountQuantity = periodCountQuantity;
			}
		}

		#endregion

		#region PopulateGIN

		public static void PopulateGIN(SegmentGroup35 group35, ObjectIdentificationCodeQualifierList qualifier, ZString[] descriptions, MessageInterpretation interpretation)
		{
			foreach (var description in descriptions)
			{
				GINSegment gin = null;
				ISegmentInterpretation ginInterpretation = null;
				var first = true;
				var friendlyName = Res.GetString("9b2e867c-0a57-48d5-918a-949eabb4f22d", "Part Number Description");

				var splitter = new TextSplitter(39) { Text = description };
				for (var i = 0; i < splitter.Count; i++)
				{
					ZString partOfDescription = splitter[i];
					if (!string.IsNullOrEmpty(partOfDescription))
					{
						if (i % 5 == 0)
						{
							gin = group35.GIN.InstantiateAChildAndAddItToChildrenCollection();
							gin.ObjectIdentificationCodeQualifier = qualifier;
							ginInterpretation = interpretation.AddNewSegmentInterpretation(gin);
						}

						var range = GetIdentityNumberRange(gin, i);
						range.ObjectIdentifier1 = partOfDescription.Left(35);
						range.ObjectIdentifier2 = partOfDescription.SubstringSafe(35, 4);

						ginInterpretation.AddElementInterpretationIfNotEmpty(friendlyName, partOfDescription);
						if (first)
						{
							friendlyName += " " + Res.GetString("bd7ce243-e0e6-4c6d-b57d-b816e3a868be", "(Continued)");
						}

						first = false;
					}
				}
			}
		}

		static IdentityNumberRangeElements GetIdentityNumberRange(GINSegment gin, int index)
		{
			switch (index % 5)
			{
				case 1:
					return gin.IdentityNumberRange2;
				case 2:
					return gin.IdentityNumberRange3;
				case 3:
					return gin.IdentityNumberRange4;
				case 4:
					return gin.IdentityNumberRange5;
				default:
					return gin.IdentityNumberRange1;
			}
		}

		#endregion

		#region PopulateTAX

		public static void PopulateTAXTypeName(TAXSegment tAX, DutyTaxFeeFunctionQualifierList qualifier, string dutyTaxFeeTypeName)
		{
			tAX.DutyTaxFeeFunctionQualifier = qualifier;
			tAX.DutyTaxFeeType.DutyTaxFeeTypeName = dutyTaxFeeTypeName;
		}

		public static void PopulateTAX(TAXSegment tAX, DutyTaxFeeFunctionQualifierList qualifier, DutyTaxFeeTypeNameCodeList dutyTaxFeeNameCode, string dutyTaxFeeAssessmentBasis)
		{
			PopulateTAX(tAX, qualifier, dutyTaxFeeAssessmentBasis);
			tAX.DutyTaxFeeType.DutyTaxFeeTypeNameCode = dutyTaxFeeNameCode;
		}

		public static void PopulateTAX(TAXSegment tAX, DutyTaxFeeFunctionQualifierList qualifier, string dutyTaxFeeAssessmentBasis)
		{
			tAX.DutyTaxFeeFunctionQualifier = qualifier;
			tAX.DutyTaxFeeAssessmentBasis = dutyTaxFeeAssessmentBasis;
		}

		public static void PopulateTAX(TAXSegment tAX, DutyTaxFeeFunctionQualifierList qualifier)
		{
			tAX.DutyTaxFeeFunctionQualifier = qualifier;
		}

		#endregion

		#region PopulateGIR

		public static void PopulateGIR(GIRSegment gIR, SetIdentificationQualifierList qualifier, string objectIdentifier)
		{
			gIR.SetIdentificationQualifier = qualifier;
			gIR.IdentificationNumber1.ObjectIdentifier = objectIdentifier;
		}

		#endregion

		#region PopulateMEA

		public static void PopulateMEA(MEASegment mEA, MeasurementAttributeCodeList measurementAttributeCode, string measurementUnitCode, ZDecimal measurementValue)
		{
			mEA.MeasurementAttributeCode = measurementAttributeCode;
			if (!string.IsNullOrEmpty(measurementUnitCode))
			{
				mEA.ValueRange.MeasurementUnitCode = measurementUnitCode;
			}

			mEA.ValueRange.MeasurementValue = (measurementValue * 1000).ToString("0000", CultureInfo.InvariantCulture); //0.000
		}

		public static void PopulateMEARounded(MEASegment mEA, MeasurementAttributeCodeList measurementAttributeCode, string measurementUnitCode, ZDecimal measurementValue)
		{
			mEA.MeasurementAttributeCode = measurementAttributeCode;
			if (!string.IsNullOrEmpty(measurementUnitCode))
			{
				mEA.ValueRange.MeasurementUnitCode = measurementUnitCode;
			}

			mEA.ValueRange.MeasurementValue = measurementValue.Round(0).ToString("0", CultureInfo.InvariantCulture);
		}

		#endregion

		#region PopulateUNT

		public static void PopulateUNT(UNTSegment uNT, string numberOfSegmentsInTheMessage, string messageReferenceNumber)
		{
			uNT.NumberOfSegmentsInTheMessage = numberOfSegmentsInTheMessage;
			uNT.MessageReferenceNumber = messageReferenceNumber;
		}

		#endregion

		#region PopulateLIN

		public static void PopulateLIN(LINSegment lIN, ZString lineItemNumber, ActionRequestNotificationDescriptionCodeList actionRequestNotificationDescriptionCode = null)
		{
			lIN.LineItemNumber = lineItemNumber;
			if (actionRequestNotificationDescriptionCode != null)
			{
				lIN.ActionRequestNotificationDescriptionCode = actionRequestNotificationDescriptionCode;
			}
		}

		#endregion

		#endregion

		#region Get Details

		#region Address

		public static IDocAddress GetAddress(NADSegmentMessageSection nadSection, PartyFunctionCodeQualifierList qualifier)
		{
			return (from NADSegment nad in nadSection
					where nad.PartyFunctionCodeQualifier == qualifier
					select GetAddress(nad)).FirstOrDefault();
		}

		static IDocAddress GetAddress(NADSegment nad)
		{
			var result = new DocAddressWrapper(nad.NameAndAddress.NameAndAddressLine1);
			result.CountryCode = nad.CountrySubEntityDetails.CountrySubEntityNameCode;
			if (result.CountryCode.Length == 3)
			{
				result.E2_State = result.CountryCode.Right(2);
				result.CountryCode = Constants.CountryCodes.UnitedStates;
				result.E2_Postcode = nad.PostalIdentificationCode;
			}
			return result;
		}

		#endregion

		#region Location

		public static ZString GetLocation(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList qualifier)
		{
			return (from LOCSegment loc in locSection
					where loc.LocationFunctionCodeQualifier == qualifier
					select loc.LocationIdentification.LocationNameCode).FirstOrDefault();
		}

		public static ZString GetRelatedLocationOne(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList qualifier)
		{
			return (from LOCSegment loc in locSection
					where loc.LocationFunctionCodeQualifier == qualifier
					select loc.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification).FirstOrDefault();
		}

		public static ZString GetRelatedLocationTwo(LOCSegmentMessageSection locSection, LocationFunctionCodeQualifierList qualifier)
		{
			return (from LOCSegment loc in locSection
					where loc.LocationFunctionCodeQualifier == qualifier
					select loc.RelatedLocationTwoIdentification.RelatedPlaceLocationTwoIdentification).FirstOrDefault();
		}

		#endregion

		#region Transport

		public static ZString GetTransportMode(SegmentGroup4MessageSection group4Section, TransportStageCodeQualifierList qualifier)
		{
			return (from SegmentGroup4 group4 in group4Section
					from TDTSegment tdt in group4.TDT
					where tdt.TransportStageCodeQualifier == qualifier
					select tdt.ModeOfTransport.TransportModeNameCode).FirstOrDefault();
		}

		public static ZString GetCarrierCode(SegmentGroup4MessageSection group4Section, TransportStageCodeQualifierList qualifier)
		{
			return (from SegmentGroup4 group4 in group4Section
					from TDTSegment tdt in group4.TDT
					where tdt.TransportStageCodeQualifier == qualifier
					select tdt.Carrier.CarrierIdentification).FirstOrDefault();
		}

		#endregion

		#region Reference

		public static ZString GetReference(SegmentGroup1MessageSection group1Section, ReferenceFunctionCodeQualifierList qualifier)
		{
			return (from SegmentGroup1 group1 in group1Section
					from RFFSegment rff in group1.RFF
					where rff.Reference.ReferenceFunctionCodeQualifier == qualifier
					select rff.Reference.ReferenceIdentifier).FirstOrDefault();
		}

		public static ZString GetReference(SegmentGroup35MessageSection group35Section, ReferenceFunctionCodeQualifierList qualifier)
		{
			return (from SegmentGroup35 group35 in group35Section
					from RFFSegment rff in group35.RFF
					where rff.Reference.ReferenceFunctionCodeQualifier == qualifier
					select rff.Reference.ReferenceIdentifier).FirstOrDefault();
		}

		public static ZString GetReference(RFFSegmentMessageSection rffSection, ReferenceFunctionCodeQualifierList qualifier)
		{
			return (from RFFSegment rff in rffSection
					where rff.Reference.ReferenceFunctionCodeQualifier == qualifier
					select rff.Reference.ReferenceIdentifier).FirstOrDefault();
		}

		public static ZBool GetReferenceIndicator(RFFSegmentMessageSection rffSection, ReferenceFunctionCodeQualifierList qualifier)
		{
			return (from RFFSegment rff in rffSection
					where rff.Reference.ReferenceFunctionCodeQualifier == qualifier
					select rff.Reference.LineNumber.Equals("Y", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
		}

		public static string GetGINAsString(SegmentGroup35MessageSection group35Section, ObjectIdentificationCodeQualifierList qualifier)
		{
			var result = from SegmentGroup35 group35 in group35Section
						 from GINSegment gin in group35.GIN
						 where gin.ObjectIdentificationCodeQualifier == qualifier
						 select GetGINAsString(gin);
			return new ZStringBuilder(result).ToString();
		}

		static ZString GetGINAsString(GINSegment gin)
		{
			var builder = new StringBuilder();
			builder.Append(gin.IdentityNumberRange1.ObjectIdentifier1);
			builder.Append(gin.IdentityNumberRange1.ObjectIdentifier2);
			builder.Append(gin.IdentityNumberRange2.ObjectIdentifier1);
			builder.Append(gin.IdentityNumberRange2.ObjectIdentifier2);
			builder.Append(gin.IdentityNumberRange3.ObjectIdentifier1);
			builder.Append(gin.IdentityNumberRange3.ObjectIdentifier2);
			builder.Append(gin.IdentityNumberRange4.ObjectIdentifier1);
			builder.Append(gin.IdentityNumberRange4.ObjectIdentifier2);
			builder.Append(gin.IdentityNumberRange5.ObjectIdentifier1);
			builder.Append(gin.IdentityNumberRange5.ObjectIdentifier2);
			return builder.ToString();
		}

		#endregion

		#region Quantity

		public static ZDecimal GetQuantity(MEASegmentMessageSection meaSection, MeasurementAttributeCodeList qualifier, int decimals = 3)
		{
			return (from MEASegment mea in meaSection
					where mea.MeasurementAttributeCode == qualifier
					select ParseDecimal(mea.ValueRange.MeasurementValue, decimals)).FirstOrDefault();
		}

		public static ZString GetUnitOfMeasure(MEASegmentMessageSection meaSection, MeasurementAttributeCodeList qualifier)
		{
			return (from MEASegment mea in meaSection
					where mea.MeasurementAttributeCode == qualifier
					select mea.ValueRange.MeasurementUnitCode).FirstOrDefault();
		}

		#endregion

		#region Amount

		public static ZDecimal GetAmount(SegmentGroup33MessageSection group33Section, MonetaryAmountTypeCodeQualifierList qualifier)
		{
			return (from SegmentGroup33 group33 in group33Section
					from MOASegment moa in group33.MOA
					where moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == qualifier
					select ParseDecimal(moa.MonetaryAmount.MonetaryAmountValue)).FirstOrDefault();
		}

		public static ZDecimal GetAmount(SegmentGroup41MessageSection group41Section, MonetaryAmountTypeCodeQualifierList qualifier)
		{
			return (from SegmentGroup41 group41 in group41Section
					from MOASegment moa in group41.MOA
					where moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == qualifier
					select ParseDecimal(moa.MonetaryAmount.MonetaryAmountValue)).FirstOrDefault();
		}

		public static ZDecimal GetAmount(IEnumerable<SegmentGroup49> group49Section, MonetaryAmountTypeCodeQualifierList qualifier)
		{
			return (from SegmentGroup49 group49 in group49Section
					from MOASegment moa in group49.MOA
					where moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == qualifier
					select ParseDecimal(moa.MonetaryAmount.MonetaryAmountValue)).FirstOrDefault();
		}

		public static ZDecimal GetAmount(MOASegmentMessageSection moaSection, MonetaryAmountTypeCodeQualifierList qualifier, int decimals = 2)
		{
			return (from MOASegment moa in moaSection
					where moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == qualifier
					select ParseDecimal(moa.MonetaryAmount.MonetaryAmountValue, decimals)).FirstOrDefault();
		}

		public static ZString GetCurrency(MOASegmentMessageSection moaSection, MonetaryAmountTypeCodeQualifierList qualifier)
		{
			return (from MOASegment moa in moaSection
					where moa.MonetaryAmount.MonetaryAmountTypeCodeQualifier == qualifier
					select moa.MonetaryAmount.CurrencyIdentificationCode).FirstOrDefault();
		}

		public static ZDecimal ParseDecimal(string value, int decimals = 2)
		{
			string tempValue = value.Length < decimals ? value.PadLeft(decimals, '0') : value;
			return value == "0" ? ZDecimal.Zero : ZDecimal.ParseSafe(tempValue.Insert(tempValue.Length - decimals, "."), 0m);
		}

		#endregion

		#region Duty Or Tax Rate Or ExemptCode

		public static ZString GetDutyOrTaxRateOrExemptCode(SegmentGroup41MessageSection group41Section, DutyTaxFeeFunctionQualifierList qualifier, DutyTaxFeeTypeNameCodeList code)
		{
			return (from SegmentGroup41 group41 in group41Section
					from TAXSegment tax in group41.TAX
					where tax.DutyTaxFeeFunctionQualifier == qualifier
						  && tax.DutyTaxFeeType.DutyTaxFeeTypeNameCode == code
					select tax.DutyTaxFeeAssessmentBasis).FirstOrDefault();
		}

		#endregion

		#region Date

		public static ZDate GetDate(DTMSegmentMessageSection dtmSection, DateTimePeriodFunctionCodeQualifierList qualifier)
		{
			return (from DTMSegment dtm in dtmSection
					where dtm.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == qualifier
					select ParseDate(dtm.DateTimePeriod.DateTimePeriodValue)).FirstOrDefault();
		}

		public static ZDate ParseDate(string yyyyMMdd)
		{
			ZDateTime result;
			if (string.IsNullOrEmpty(yyyyMMdd.Trim()))
			{
				result = ZDateTime.Empty;
			}
			else if (yyyyMMdd == "99999999")
			{
				result = new ZDateTime(2079, 6, 5);
			}
			else
			{
				ZDateTime.TryParseExact(yyyyMMdd, out result, "yyyyMMdd");
			}
			return result.Date;
		}

		#endregion

		public static string GetDocumentName(D99BCUSRES.CUSRESMessage resMessage)
		{
			var result = ZString.Empty;
			if (resMessage != null)
			{
				result = resMessage.BGM[0].DocumentMessageName.DocumentName;
			}
			return result.IsEmpty ? result : result.PadLeft(9, '0');
		}

		public static string GetAccountSecurityNumber(D99BCUSRES.CUSRESMessage resMessage)
		{
			var result = ZString.Empty;
			if (resMessage != null)
			{
				result = resMessage.UNH[0].CommonAccessReference;
			}
			return result.IsEmpty ? result : result.PadLeft(5, '0');
		}

		#endregion
	}
}
