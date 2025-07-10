using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.D96A.Elements;
using Enterprise.Edifact.D96A.Messages.CUSRES;
using Enterprise.Edifact.D96A.Segments;
using Enterprise.Edifact.Utilities;
using Enterprise.ZArchitecture.Schema;
using D96ACUSREP = Enterprise.Edifact.D96A.Messages.CUSREP;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	class D96AMessageUtilities
	{
		#region Convert

		public static ZString ConvertPackUnitToACROSSUnit(ZString code)
		{
			var result = code;

			switch (code)
			{
				case Constants.PkgUnit.Bag:
					result = ACROSSPackageTypes.Codes.BAG;
					break;
				case Constants.PkgUnit.BaleCompressed:
					result = ACROSSPackageTypes.Codes.BALEBLE;
					break;
				case Constants.PkgUnit.BaleUncompressed:
					result = ACROSSPackageTypes.Codes.BALEBLE;
					break;
				case Constants.PkgUnit.Basket:
					result = ACROSSPackageTypes.Codes.BASKETORHAMPER;
					break;
				case Constants.PkgUnit.Bottle:
					result = ACROSSPackageTypes.Codes.BOTTLE;
					break;
				case Constants.PkgUnit.Box:
					result = ACROSSPackageTypes.Codes.BOX;
					break;
				case Constants.PkgUnit.BreakBulk:
					result = ACROSSPackageTypes.Codes.BULK;
					break;
				case Constants.PkgUnit.BulkBag:
					result = ACROSSPackageTypes.Codes.BULKBAG;
					break;
				case Constants.PkgUnit.Bundle:
					result = ACROSSPackageTypes.Codes.BUNDLE;
					break;
				case Constants.PkgUnit.Carton:
					result = ACROSSPackageTypes.Codes.CARTON;
					break;
				case Constants.PkgUnit.Case:
					result = ACROSSPackageTypes.Codes.CASE;
					break;
				case Constants.PkgUnit.Coil:
					result = ACROSSPackageTypes.Codes.COIL;
					break;
				case Constants.PkgUnit.Container:
					result = ACROSSPackageTypes.Codes.CONTAINER;
					break;
				case Constants.PkgUnit.Cradle:
					result = ACROSSPackageTypes.Codes.CRADLE;
					break;
				case Constants.PkgUnit.Crate:
					result = ACROSSPackageTypes.Codes.CRATE;
					break;
				case Constants.PkgUnit.Cylinder:
					result = ACROSSPackageTypes.Codes.CYLINDER;
					break;
				case Constants.PkgUnit.Drum:
					result = ACROSSPackageTypes.Codes.DRUM;
					break;
				case Constants.PkgUnit.Envelope:
					result = ACROSSPackageTypes.Codes.ENVELOPE;
					break;
				case Constants.PkgUnit.Keg:
					result = ACROSSPackageTypes.Codes.KEG;
					break;
				case Constants.PkgUnit.Mix:
					result = ACROSSPackageTypes.Codes.MIXEDTYPEPACK;
					break;
				case Constants.PkgUnit.Package:
					result = ACROSSPackageTypes.Codes.PACKAGE;
					break;
				case Constants.PkgUnit.Pail:
					result = ACROSSPackageTypes.Codes.PAIL;
					break;
				case Constants.PkgUnit.Pallet:
					result = ACROSSPackageTypes.Codes.PALLET;
					break;
				case Constants.PkgUnit.Piece:
					result = ACROSSPackageTypes.Codes.PIECE;
					break;
				case Constants.PkgUnit.Reel:
					result = ACROSSPackageTypes.Codes.REEL;
					break;
				case Constants.PkgUnit.Roll:
					result = ACROSSPackageTypes.Codes.ROLL;
					break;
				case Constants.PkgUnit.Sheet:
					result = ACROSSPackageTypes.Codes.SHEET;
					break;
				case Constants.PkgUnit.Skid:
					result = ACROSSPackageTypes.Codes.SKID;
					break;
				case Constants.PkgUnit.Spool:
					result = ACROSSPackageTypes.Codes.SPOOL;
					break;
				case Constants.PkgUnit.Tube:
					result = ACROSSPackageTypes.Codes.TUBE;
					break;
				case Constants.PkgUnit.Unit:
					result = ACROSSPackageTypes.Codes.UNIT;
					break;
			}
			return result;
		}

		internal static ZString ConvertWeightUnitToACROSSUnit(ZString code)
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
				case Constants.Weight.Decitons:
					result = MessageConstants.CBSAWeightUnits.Deciton;
					break;
				case Constants.Weight.Grams:
					result = MessageConstants.CBSAWeightUnits.Gram;
					break;
				case Constants.Weight.Hectograms:
					result = MessageConstants.CBSAWeightUnits.Hectogram;
					break;
				case Constants.Weight.Kilotonnes:
					result = MessageConstants.CBSAWeightUnits.Kiloton;
					break;
				case Constants.Weight.MetricCarat:
					result = MessageConstants.CBSAWeightUnits.MetricCarat;
					break;
				case Constants.Weight.Milligrams:
					result = MessageConstants.CBSAWeightUnits.Milligram;
					break;
			}
			return result;
		}

		#endregion

		#region Populate Details

		public static void PopulateUNH(UNHSegment uNH, string messageReferenceNumber, Enterprise.Edifact.D96A.Elements.MessageTypeList messageType, string messageVersionNumber, string messageReleaseNumber, ControllingAgencyList controllingAgency)
		{
			uNH.MessageReferenceNumber = messageReferenceNumber;
			uNH.MessageIdentifier.MessageType = messageType;
			uNH.MessageIdentifier.MessageVersionNumber = messageVersionNumber;
			uNH.MessageIdentifier.MessageReleaseNumber = messageReleaseNumber;
			uNH.MessageIdentifier.ControllingAgency = controllingAgency;
		}

		public static void PopulateBGM(BGMSegment bGM, string documentName, string documentMessageNumber, MessageFunctionCodedList messageFunctionCode, DocumentMessageNameCodedList documentNameCoded)
		{
			bGM.DocumentMessageName.DocumentMessageName = documentName;
			if (documentNameCoded != null)
			{
				bGM.DocumentMessageName.DocumentMessageNameCoded = documentNameCoded;
			}

			bGM.DocumentMessageNumber = documentMessageNumber;
			if (messageFunctionCode != null)
			{
				bGM.MessageFunctionCoded = messageFunctionCode;
			}
		}

		public static void PopulateCST(CSTSegment cST, string customsIdentityCode1, CodeListQualifierList codeListQualifier1,
			string customsIdentityCode2, CodeListQualifierList codeListQualifier2, string customsIdentityCode3, CodeListQualifierList codeListQualifier3,
			string customsIdentityCode4, CodeListQualifierList codeListQualifier4, string customsIdentityCode5, CodeListQualifierList codeListQualifier5)
		{
			if (!string.IsNullOrEmpty(customsIdentityCode1))
			{
				cST.CustomsIdentityCodes1.CustomsCodeIdentification = customsIdentityCode1;
				cST.CustomsIdentityCodes1.CodeListQualifier = codeListQualifier1;
			}
			if (!string.IsNullOrEmpty(customsIdentityCode2))
			{
				cST.CustomsIdentityCodes2.CustomsCodeIdentification = customsIdentityCode2;
				cST.CustomsIdentityCodes2.CodeListQualifier = codeListQualifier2;
			}
			if (!string.IsNullOrEmpty(customsIdentityCode3))
			{
				cST.CustomsIdentityCodes3.CustomsCodeIdentification = customsIdentityCode3;
				cST.CustomsIdentityCodes3.CodeListQualifier = codeListQualifier3;
			}
			if (!string.IsNullOrEmpty(customsIdentityCode4))
			{
				cST.CustomsIdentityCodes4.CustomsCodeIdentification = customsIdentityCode4;
				cST.CustomsIdentityCodes4.CodeListQualifier = codeListQualifier4;
			}
			if (!string.IsNullOrEmpty(customsIdentityCode5))
			{
				cST.CustomsIdentityCodes5.CustomsCodeIdentification = customsIdentityCode5;
				cST.CustomsIdentityCodes5.CodeListQualifier = codeListQualifier5;
			}
		}

		public static void PopulateLOC(LOCSegment lOC, PlaceLocationQualifierList qualifier, string releaseOfficeNo, CodeListQualifierList codeListQualifier, string placeLocation, string goodsLocationCode, string goodsLocationName)
		{
			lOC.PlaceLocationQualifier = qualifier;
			if (!string.IsNullOrEmpty(releaseOfficeNo))
			{
				lOC.LocationIdentification.PlaceLocationIdentification = releaseOfficeNo.PadLeft(4, '0');
			}

			if (codeListQualifier != null)
			{
				lOC.LocationIdentification.CodeListQualifier = codeListQualifier;
			}

			if (!string.IsNullOrEmpty(placeLocation))
			{
				lOC.LocationIdentification.PlaceLocation = placeLocation;
			}

			if (!string.IsNullOrEmpty(goodsLocationCode))
			{
				lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = goodsLocationCode;
			}

			if (!string.IsNullOrEmpty(goodsLocationName))
			{
				lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOne = goodsLocationName;
			}
		}

		public static void PopulateLOC(LOCSegment lOC, string countryOfOrigin, string countryOfExport, string countryOfTranshipment)
		{
			lOC.PlaceLocationQualifier = PlaceLocationQualifierList.CountryOfOrigin;
			lOC.LocationIdentification.PlaceLocationIdentification = countryOfOrigin;
			if (!string.IsNullOrEmpty(countryOfExport))
			{
				lOC.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = countryOfExport;
			}

			if (!string.IsNullOrEmpty(countryOfTranshipment))
			{
				lOC.RelatedLocationTwoIdentification.RelatedPlaceLocationTwoIdentification = countryOfTranshipment;
			}
		}

		public static void PopulateDTM203(DTMSegment dTM, DateTimePeriodQualifierList dateTimePeriodQualifier, ZDateTime dateTimePeriodValue)
		{
			dTM.DateTimePeriod.DateTimePeriodQualifier = dateTimePeriodQualifier;
			dTM.DateTimePeriod.DateTimePeriod = dateTimePeriodValue.ToString("yyyyMMddHHmm");
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmddhhmm;
		}

		public static void PopulateDTM102(DTMSegment dTM, DateTimePeriodQualifierList dateTimePeriodQualifier, ZDate datePeriodValue)
		{
			dTM.DateTimePeriod.DateTimePeriodQualifier = dateTimePeriodQualifier;
			dTM.DateTimePeriod.DateTimePeriod = datePeriodValue.ToString("yyyyMMdd");
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;
		}

		public static void PopulateMEAWeight(MEASegment mEA, MeasurementDimensionCodedList measurementDimensionCode, ZDecimal value, string units)
		{
			var convertedUnits = ConvertWeightUnitToACROSSUnit(units);
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
			mEA.MeasurementApplicationQualifier = MeasurementApplicationQualifierList.Weights;
			mEA.MeasurementDetails.MeasurementDimensionCoded = measurementDimensionCode;
			mEA.ValueRange.MeasureUnitQualifier = convertedUnits;
			mEA.ValueRange.MeasurementValue = (convertedWeight.IsEmpty ? 0m : (convertedWeight < 1m ? 1m : (decimal)convertedWeight.Round(0))).ToString(CultureInfo.InvariantCulture);
		}

		public static void PopulateEQDContainer(EQDSegment eQD, ZString containerNumber)
		{
			eQD.EquipmentQualifier = EquipmentQualifierList.Container;
			eQD.EquipmentIdentification.EquipmentIdentificationNumber = containerNumber;
		}

		public static void PopulateRFF(RFFSegment rFF, ReferenceQualifierList referenceQualifier, string referenceNumber)
		{
			rFF.Reference.ReferenceQualifier = referenceQualifier;
			rFF.Reference.ReferenceNumber = referenceNumber.Replace(" ", string.Empty);
		}

		public static void PopulatePAC(PACSegment pAC, ZInt packages, string packType)
		{
			pAC.NumberOfPackages = packages.ToString();
			pAC.PackageType.TypeOfPackages = packType;//TODO: convert to standard ACROSS package type
		}

		public static void PopulateTOD(TODSegment tOD, TermsOfDeliveryOrTransportFunctionCodedList todCode, string instructions1, string instructions2)
		{
			if (todCode != null)
			{
				tOD.TermsOfDeliveryOrTransportFunctionCoded = todCode;
			}

			if (!string.IsNullOrEmpty(instructions1))
			{
				tOD.TermsOfDeliveryOrTransport.TermsOfDeliveryOrTransport1 = instructions1;
			}

			if (!string.IsNullOrEmpty(instructions2))
			{
				tOD.TermsOfDeliveryOrTransport.TermsOfDeliveryOrTransport2 = instructions2;
			}
		}

		public static void PopulateUNS(UNSSegment uNS, SectionIdentificationList sectionIdentification)
		{
			uNS.SectionIdentification = sectionIdentification;
		}

		public static void PopulateMOA(MOASegment mOA, MonetaryAmountTypeQualifierList monetaryAmountTypeQualifier, ZDecimal amount, string currencyCode, int decimalsToShow)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = monetaryAmountTypeQualifier;
			mOA.MonetaryAmount.MonetaryAmount = decimalsToShow == 4 ? amount.Round(decimalsToShow).ToString("0.00##") : amount.Round(decimalsToShow).ToString(decimalsToShow);
			if (!string.IsNullOrEmpty(currencyCode))
			{
				mOA.MonetaryAmount.CurrencyCoded = currencyCode;
			}
		}

		public static void PopulateDMS(DMSSegment dMS, string docReference)
		{
			dMS.DocumentMessageNumber = docReference;
		}

		public static void PopulateDOC(DOCSegment dOC, string rulingNumber, string placeOfDirectShipment)
		{
			dOC.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.DeclarationOfOrigin;
			if (!string.IsNullOrEmpty(rulingNumber))
			{
				dOC.DocumentMessageName.DocumentMessageName = rulingNumber;
			}

			if (!string.IsNullOrEmpty(placeOfDirectShipment))
			{
				dOC.DocumentMessageDetails.DocumentMessageSource = placeOfDirectShipment;
			}
		}

		public static void PopulateLIN(LINSegment lIN, ZInt invoicePageNumber, string tariffNumber, ZInt invoiceLineNumber)
		{
			lIN.LineItemNumber = invoicePageNumber.ToString();
			if (!string.IsNullOrEmpty(tariffNumber))
			{
				lIN.ItemNumberIdentification.ItemNumber = tariffNumber;
			}

			if (!invoiceLineNumber.IsEmpty)
			{
				lIN.SubLineInformation.LineItemNumber = invoiceLineNumber.ToString();
			}
		}

		public static void PopulateQTY(QTYSegment qTY, ZDecimal quantity, string quantityUnits)
		{
			qTY.QuantityDetails.Quantity = quantity.ToString("0.####");
			qTY.QuantityDetails.QuantityQualifier = QuantityQualifierList.GetFromString(quantityUnits);//TODO: convert to standard ACROSS package type
		}

		public static void PopulateIMD(IMDSegment iMD, string itemDescID, string itemDescription)
		{
			iMD.ItemDescription.ItemDescriptionIdentification = itemDescID;
			var splitter = new TextSplitter(35) { Text = itemDescription };
			iMD.ItemDescription.ItemDescription1 = splitter[0].Trim();
			iMD.ItemDescription.ItemDescription2 = splitter[1].Trim();
		}

		public static void PopulateFTX(FTXSegment fTX, TextSubjectQualifierList textQualifier, string freeText)
		{
			fTX.TextSubjectQualifier = textQualifier;
			fTX.TextLiteral.FreeText1 = freeText;
		}

		public static void PopulateUNT(UNTSegment uNT, string numberOfSegmentsInTheMessage, string messageReferenceNumber)
		{
			uNT.NumberOfSegmentsInTheMessage = numberOfSegmentsInTheMessage;
			uNT.MessageReferenceNumber = messageReferenceNumber;
		}

		public static void PopulateCOM(COMSegment com, CommunicationChannelQualifierList channelQualifier, ZString number)
		{
			com.CommunicationContact.CommunicationNumber = number.KeepChars("1234567890").Right(10);
			com.CommunicationContact.CommunicationChannelQualifier = channelQualifier;
		}

		#endregion

		#region Get Details

		internal static EDIMessage GetOriginalRNSMessage(BusinessObjectFactory factory, CUSRESMessage message)
		{
			EDIMessage result = null;
			var messageNumber = (from ERPSegment erp in message.Group1[0].ERP select (ZString)erp.ErrorPointDetails.MessageItemNumber).FirstOrDefault();
			if (!messageNumber.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(EDIMessage));
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RNSRequest);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNumber);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + OrderByClause.Descending;
				result = factory.LoadTop1<EDIMessage>(query);
			}
			return result;
		}

		internal static ZString GetServiceOption(BGMSegmentMessageSection bgmSection)
		{
			return bgmSection.Count > 0 ? (ZString)bgmSection[0].DocumentMessageName.DocumentMessageName : ZString.Empty;
		}

		internal static ZString GetProcessingIndicator(GISSegmentMessageSection gisSection)
		{
			return gisSection.Count > 0 ? gisSection[0].ProcessingIndicator.ProcessingIndicatorCoded.ToString() : ProcessingIndicatorCodedList.TransactionUnknown.ToString();
		}

		internal static IEnumerable<ZString> GetContainers(EQDSegmentMessageSection eqdSection)
		{
			return from EQDSegment eqd in eqdSection
				   select (ZString)eqd.EquipmentIdentification.EquipmentIdentificationNumber;
		}

		internal static ZString GetFreeText(FTXSegmentMessageSection ftxSection, TextSubjectQualifierList qualifier)
		{
			return (from FTXSegment ftx in ftxSection
					where ftx.TextSubjectQualifier == qualifier
					let text = ftx.TextLiteral
					let parts = new[] { text.FreeText1, text.FreeText2, text.FreeText3, text.FreeText4, text.FreeText5 }
					select new ZStringBuilder(parts).ToStringWithNewLineBetweenAppends().TrimEnd()).FirstOrDefault();
		}

		internal static ZString GetLocation(LOCSegmentMessageSection locSection, PlaceLocationQualifierList qualifier)
		{
			return (from LOCSegment loc in locSection
					where loc.PlaceLocationQualifier == qualifier
					select loc.LocationIdentification.PlaceLocationIdentification).FirstOrDefault();
		}

		internal static ZString GetRelatedLocation(LOCSegmentMessageSection locSection, PlaceLocationQualifierList qualifier)
		{
			return (from LOCSegment loc in locSection
					where loc.PlaceLocationQualifier == qualifier
					select loc.LocationIdentification.PlaceLocation).FirstOrDefault();
		}

		internal static ZDateTime GetDate(DTMSegmentMessageSection dtmSection, DateTimePeriodQualifierList qualifier)
		{
			return (from DTMSegment dtm in dtmSection
					where dtm.DateTimePeriod.DateTimePeriodQualifier == qualifier
					select ParseDate(dtm.DateTimePeriod.DateTimePeriod)).FirstOrDefault();
		}

		static ZDateTime ParseDate(string dateTime)
		{
			ZDateTime result;
			ZDateTime.TryParseExact(dateTime, out result, "yyyyMMddHHmm");
			return result;
		}

		internal static ZString GetReferenceCode(D96ACUSREP.SegmentGroup1MessageSection group1MessageSection, ReferenceQualifierList referenceQualifier)
		{
			var result = ZString.Empty;
			if (group1MessageSection != null)
			{
				foreach (D96ACUSREP.SegmentGroup1 group1 in group1MessageSection)
				{
					foreach (RFFSegment rFFSegment in group1.RFF)
					{
						if (rFFSegment != null && rFFSegment.Reference.ReferenceQualifier == referenceQualifier)
						{
							result = rFFSegment.Reference.ReferenceNumber.Replace(" ", "");
							break;
						}
					}
					if (!result.IsEmpty)
					{
						break;
					}
				}
			}
			return result;
		}

		#endregion
	}
}
