using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Edifact.Utilities;
using Enterprise.Edifact.V921ES.Elements;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Edifact.V921ES.Segments;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public static class CommonMessageTextBuilder
	{
		public const int MarksLabelsShippingMarksLength = 35;
		public const int NameAndAddressLineLength = 35;

		public static void PopulateUNHSegment(UNHSegment unhSegment, MessageReleaseNumberList releaseNumber, ZString associationassigned)
		{
			unhSegment.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
			unhSegment.MessageIdentifier.MessageType = MessageTypeList.CustomsDeclarationMessage;
			unhSegment.MessageIdentifier.MessageVersionNumber = MessageVersionNumberList.Status1Version;
			unhSegment.MessageIdentifier.MessageReleaseNumber = releaseNumber;
			unhSegment.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnCefact;
			unhSegment.MessageIdentifier.AssociationAssignedCode = associationassigned;
		}

		public static void PopulateBGMSegment(BGMSegment bgmSegment, DocumentMessageNameCodedList docMessageNumber, string localReferenceNumber, MessageFunctionCodedList messageFunction)
		{
			bgmSegment.DocumentMessageName.DocumentMessageNameCoded = docMessageNumber;
			bgmSegment.DocumentMessageNumber = localReferenceNumber;
			bgmSegment.MessageFunctionCoded = messageFunction;
		}

		#region LOC Segment

		public static LOCSegment AddNewLOCSegment(LOCSegmentMessageSection locSection, ZString placeIdentification, PlaceLocationQualifierList locType, CodeListResponsibleAgencyCodedList responsibleAgency = null)
		{
			if (!placeIdentification.IsEmpty)
			{
				var locSegment = locSection.InstantiateAChildAndAddItToChildrenCollection();
				locSegment.PlaceLocationQualifier = locType;
				locSegment.LocationIdentification.PlaceLocationIdentification = placeIdentification;
				if (responsibleAgency != null)
				{
					locSegment.LocationIdentification.CodeListResponsibleAgencyCoded = responsibleAgency;
				}
				return locSegment;
			}
			return null;
		}

		public static LOCSegment AddNewLOCSegment(LOCSegmentMessageSection locSection, ZString placeIdentification, PlaceLocationQualifierList locType, ZString placeLoc, CodeListResponsibleAgencyCodedList responsibleAgency = null)
		{
			var locSegment = AddNewLOCSegment(locSection, placeIdentification, locType, responsibleAgency);
			if (locSegment != null)
			{
				locSegment.LocationIdentification.PlaceLocation = placeLoc;
			}
			return locSegment;
		}

		public static LOCSegment AddNewLOCSegment(LOCSegmentMessageSection locSection, ZString placeIdentification, PlaceLocationQualifierList locType, ZString relatedPlaceOneIdentification, CodeListResponsibleAgencyCodedList responsibleAgency = null, CodeListResponsibleAgencyCodedList relatedResponsibleAgencyOne = null)
		{
			var locSegment = AddNewLOCSegment(locSection, placeIdentification, locType, responsibleAgency);
			if (locSegment != null)
			{
				if (!relatedPlaceOneIdentification.IsEmpty)
				{
					locSegment.RelatedLocationOneIdentification.RelatedPlaceLocationOneIdentification = relatedPlaceOneIdentification;
					if (relatedResponsibleAgencyOne != null)
					{
						locSegment.RelatedLocationOneIdentification.CodeListResponsibleAgencyCoded = relatedResponsibleAgencyOne;
					}
				}
			}
			return locSegment;
		}

		public static void AddNewLOCSegment(LOCSegmentMessageSection locSection, PlaceLocationQualifierList locType, ZString relatedPlaceUnloading)
		{
			if (!relatedPlaceUnloading.IsEmpty)
			{
				var locSegment = locSection.InstantiateAChildAndAddItToChildrenCollection();
				locSegment.PlaceLocationQualifier = locType;
				locSegment.RelatedLocationOneIdentification.RelatedPlaceLocationOne = relatedPlaceUnloading;
			}
		}

		#endregion

		public static void AddNewDTMSegment(DTMSegmentMessageSection dtmSection, ZString dateTime, DateTimePeriodQualifierList dateTimeType, DateTimePeriodFormatQualifierList dateTimeFormat)
		{
			if (!dateTime.IsEmpty)
			{
				var dtmSegment = dtmSection.InstantiateAChildAndAddItToChildrenCollection();
				dtmSegment.DateTimePeriod.DateTimePeriodQualifier = dateTimeType;
				dtmSegment.DateTimePeriod.DateTimePeriod = dateTime;
				dtmSegment.DateTimePeriod.DateTimePeriodFormatQualifier = dateTimeFormat;
			}
		}

		public static void AddNewGISSegment(GISSegmentMessageSection gisSection, ZBool indicatorCode, CodeListQualifierList indicatorQualifier, CodeListResponsibleAgencyCodedList responsibleAgency, ProcessTypeIdentificationList processTypeId = null)
		{
			if (indicatorQualifier == CodeListQualifierList.CustomsIndicator)
			{
				var gisSegment = gisSection.InstantiateAChildAndAddItToChildrenCollection();
				gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = indicatorCode ? ProcessingIndicatorCodedList.MessageContentAccepted : ProcessingIndicatorCodedList.GetFromString("0");
				gisSegment.ProcessingIndicator.CodeListQualifier = indicatorQualifier;
				gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = responsibleAgency;
			}
			else
			{
				if (indicatorCode)
				{
					var gisSegment = gisSection.InstantiateAChildAndAddItToChildrenCollection();
					gisSegment.ProcessingIndicator.ProcessingIndicatorCoded = ProcessingIndicatorCodedList.MessageContentAccepted;
					gisSegment.ProcessingIndicator.CodeListQualifier = indicatorQualifier;
					gisSegment.ProcessingIndicator.CodeListResponsibleAgencyCoded = responsibleAgency;
					if (processTypeId != null)
					{
						gisSegment.ProcessingIndicator.ProcessTypeIdentification = processTypeId;
					}
				}
			}
		}

		public static void AddNewEQDSegment(EQDSegmentMessageSection eqdSection, EquipmentQualifierList equipmentType, ZString countryCode)
		{
			if (!countryCode.IsEmpty)
			{
				var eqdSegment = eqdSection.InstantiateAChildAndAddItToChildrenCollection();
				eqdSegment.EquipmentQualifier = equipmentType;
				eqdSegment.EquipmentIdentification.CountryCoded = countryCode;
			}
		}

		public static void AddNewSELSegment(SELSegmentMessageSection selSection, int sealsNumber, SealingPartyCodedList sealingPartyCoded, ZString sealCode)
		{
			if (!sealCode.IsEmpty)
			{
				var selSegment = selSection.InstantiateAChildAndAddItToChildrenCollection();
				selSegment.SealNumber = sealsNumber.ToString(CultureInfo.CurrentCulture);
				selSegment.SealIssuer.SealingPartyCoded = sealingPartyCoded;
				selSegment.SealIssuer.SealingParty = sealCode;
			}
		}

		public static void AddNewFTXSegment(FTXSegmentMessageSection ftxSection, ZString function)
		{
			if (!function.IsEmpty)
			{
				var ftxSegment = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
				ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.PaymentInstructionsInformation;
				ftxSegment.TextFunctionCoded = TextFunctionCodedList.GetFromString(function);
			}
		}

		public static void AddNewFTXSegmentTextLiteral(FTXSegmentMessageSection ftxSection, ZString freeText)
		{
			if (!freeText.IsEmpty)
			{
				var ftxSegment = ftxSection.InstantiateAChildAndAddItToChildrenCollection();
				ftxSegment.TextSubjectQualifier = TextSubjectQualifierList.GetFromString("ACR");
				ftxSegment.TextLiteral.FreeText1 = freeText;
			}
		}

		#region RFF Segment

		public static RFFSegment AddNewRFFSegment(RFFSegmentMessageSection rffSection, ReferenceQualifierList referenceType, ZString referenceNumber)
		{
			RFFSegment rffSegment = null;
			if (rffSection != null && referenceType != null && !referenceNumber.IsEmpty)
			{
				rffSegment = rffSection.InstantiateAChildAndAddItToChildrenCollection();
				rffSegment.Reference.ReferenceQualifier = referenceType;
				rffSegment.Reference.ReferenceNumber = referenceNumber;
			}
			return rffSegment;
		}

		public static void AddNewRFFSegment(RFFSegmentMessageSection rffSection, ReferenceQualifierList referenceType, ZString referenceNumber, ZString lineNumber, ZString versionNumber)
		{
			var rffSegment = AddNewRFFSegment(rffSection, referenceType, referenceNumber);
			if (rffSegment != null)
			{
				rffSegment.Reference.LineNumber = lineNumber;
				if (!versionNumber.IsEmpty)
				{
					rffSegment.Reference.ReferenceVersionNumber = versionNumber;
				}
			}
		}

		#endregion

		#region SG4 Population

		public static void AddNewSG4Group(SegmentGroup4MessageSection group4, TransportStageQualifierList qualifier, ZString transportMode, ZString transportMedium, ZString transportNationality, ZString transportId)
		{
			var sg4 = group4.InstantiateAChildAndAddItToChildrenCollection();
			var tdtSegment = sg4.TDT.InstantiateAChildAndAddItToChildrenCollection();
			tdtSegment.TransportStageQualifier = qualifier;
			if (!transportMedium.IsEmpty)
			{
				tdtSegment.ModeOfTransport.ModeOfTransport = transportMedium;
			}
			if (!transportMode.IsEmpty)
			{
				tdtSegment.ModeOfTransport.ModeOfTransportCoded = transportMode;
			}
			if (!transportId.IsEmpty)
			{
				AddNewTPLSegment(sg4.TPL, transportId, transportNationality);
			}
		}

		public static void AddNewTPLSegment(TPLSegmentMessageSection tplSegSection, ZString transportId, ZString transportNationality)
		{
			var tplSegment = tplSegSection.InstantiateAChildAndAddItToChildrenCollection();
			tplSegment.TransportIdentification.IdOfTheMeansOfTransport = transportId;
			tplSegment.TransportIdentification.NationalityOfMeansOfTransportCoded = transportNationality;
		}
		#endregion

		#region NAD Segment

		public static void AddNewNADWithAddress(NADSegmentMessageSection nadSegSection, PartyQualifierList addressType, CodeListResponsibleAgencyCodedList agencyCodedList, IPartyProvider addressDetails)
		{
			if (addressType != null && addressDetails != null)
			{
				var nadSegment = nadSegSection.InstantiateAChildAndAddItToChildrenCollection();
				nadSegment.PartyQualifier = addressType;
				if (!addressDetails.Id.IsEmpty)
				{
					nadSegment.PartyIdentificationDetails.PartyIdIdentification = addressDetails.Id;
					if (agencyCodedList != null)
					{
						nadSegment.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = agencyCodedList;
					}
				}
				SetNADAddressDetails(nadSegment, addressDetails);
			}
		}

		public static void SetNADAddressDetails(NADSegment nadSegment, IPartyProvider addressDetails)
		{
			nadSegment.PartyName.PartyName1 = addressDetails.Name.Left(35);
			nadSegment.Street.StreetAndNumberPOBox1 = addressDetails.Address.Left(35);
			nadSegment.CityName = addressDetails.City.Left(35);
			nadSegment.PostcodeIdentification = addressDetails.PostCode.Left(9);
			nadSegment.CountryCoded = addressDetails.Country;
		}

		public static void AddNewNADWithEmail(NADSegmentMessageSection nadSegSection, CodeListResponsibleAgencyCodedList agencyCodedList, IPartyEmailProvider addressDetails)
		{
			if (addressDetails != null)
			{
				var nadSegment = nadSegSection.InstantiateAChildAndAddItToChildrenCollection();
				nadSegment.PartyQualifier = PartyQualifierList.Declarant;
				nadSegment.PartyIdentificationDetails.PartyIdIdentification = addressDetails.Id;
				if (agencyCodedList != null)
				{
					nadSegment.PartyIdentificationDetails.CodeListResponsibleAgencyCoded = agencyCodedList;
				}
				var textSplitter = new TextSplitter(NameAndAddressLineLength);
				textSplitter.Text = addressDetails.EmailAddress;
				nadSegment.NameAndAddress.NameAndAddressLine1 = textSplitter[0];
				nadSegment.NameAndAddress.NameAndAddressLine2 = textSplitter[1];
				nadSegment.PartyName.PartyName1 = addressDetails.Name.Left(35);
			}
		}

		#endregion

		public static void AddNewMOAInSG8Group(SegmentGroup8MessageSection group8, MonetaryAmountTypeQualifierList qualifier, ZDecimal totalAmount, ZString currencyCode, int valueDecimalPlace = 2)
		{
			if (qualifier != null && !currencyCode.IsEmpty)
			{
				var sg8 = group8.InstantiateAChildAndAddItToChildrenCollection();
				var moaSegment = sg8.MOA.InstantiateAChildAndAddItToChildrenCollection();
				moaSegment.MonetaryAmount.MonetaryAmountTypeQualifier = qualifier;
				if (!totalAmount.IsEmpty)
				{
					moaSegment.MonetaryAmount.MonetaryAmount = totalAmount.IsInteger ? Utilities.FormatNumberNational(totalAmount, 0) : Utilities.FormatNumberNational(totalAmount, valueDecimalPlace);
				}
				moaSegment.MonetaryAmount.CurrencyCoded = currencyCode;
			}
		}

		public static void PopulateUNS1Segment(UNSSegment unsSegment)
		{
			unsSegment.SectionIdentification = SectionIdentificationList.HeaderDetailSectionSeparation;
		}

		public static void AddFTXSegmentForGoodsDescription(SegmentGroup30 sg30, TextSubjectQualifierList textType, ZString textValue, int fieldLimit)
		{
			if (textType != null && !textValue.IsEmpty)
			{
				var ftxSegment = sg30.FTX.InstantiateAChildAndAddItToChildrenCollection();
				var splitter = new TextSplitter(fieldLimit);
				splitter.Text = textValue;
				ftxSegment.TextSubjectQualifier = textType;
				ftxSegment.TextLiteral.FreeText1 = splitter[0];
				ftxSegment.TextLiteral.FreeText2 = splitter[1];
				ftxSegment.TextLiteral.FreeText3 = splitter[2];
				ftxSegment.TextLiteral.FreeText4 = splitter[3];
				ftxSegment.TextLiteral.FreeText5 = splitter[4];
			}
		}

		public static void AddNewMEASegment(SegmentGroup30 sg30, ZDecimal value, ZString unit, MeasurementApplicationQualifierList qualifier, int valueDecimalPlace = 3, MeasurementDimensionCodedList dimension = null)
		{
			if (!unit.IsEmpty && !value.IsEmpty)
			{
				var meaSegment = sg30.MEA.InstantiateAChildAndAddItToChildrenCollection();
				meaSegment.MeasurementApplicationQualifier = qualifier;
				if (dimension != null)
				{
					meaSegment.MeasurementDetails.MeasurementDimensionCoded = dimension;
				}
				meaSegment.ValueRange.MeasureUnitQualifier = unit;
				meaSegment.ValueRange.MeasurementValue = value.IsInteger ? Utilities.FormatNumberNational(value, 0) : Utilities.FormatNumberNational(value, valueDecimalPlace);
			}
		}

		public static void AddNewTDTSegmentOnSG30(SegmentGroup30 sg30, ZString code, TransportStageQualifierList qualifier)
		{
			if (!code.IsEmpty)
			{
				var tdtSegment = sg30.TDT.InstantiateAChildAndAddItToChildrenCollection();
				tdtSegment.TransportStageQualifier = qualifier;
				tdtSegment.TransportIdentification.IdOfMeansOfTransportIdentification = code;
			}
		}

		#region SG31 Population

		public static void AddNewSG31Group(SegmentGroup31MessageSection sg31Section, IExternalPackagesInfoCommon externalPackages, PackagingLevelCodedList packagingLevel)
		{
			if (externalPackages != null && !externalPackages.NumberOfPackages.IsEmpty)
			{
				var sg31 = SetPACSegmentForExternal(sg31Section, externalPackages, packagingLevel);
				var tags = externalPackages.Tags.ToArray();
				var numberOfTags = tags.Length;
				if (numberOfTags > 0)
				{
					for (int i = 0; i < numberOfTags; i += 10)
					{
						var sg32 = sg31.Group32.InstantiateAChildAndAddItToChildrenCollection();
						var pciSegment = sg32.PCI.InstantiateAChildAndAddItToChildrenCollection();
						pciSegment.MarksLabels.ShippingMarks1 = tags[i];
						if (numberOfTags > (i + 1))
						{
							pciSegment.MarksLabels.ShippingMarks2 = tags[i + 1];
						}
						if (numberOfTags > (i + 2))
						{
							pciSegment.MarksLabels.ShippingMarks3 = tags[i + 2];
						}
						if (numberOfTags > (i + 3))
						{
							pciSegment.MarksLabels.ShippingMarks4 = tags[i + 3];
						}
						if (numberOfTags > (i + 4))
						{
							pciSegment.MarksLabels.ShippingMarks5 = tags[i + 4];
						}
						if (numberOfTags > (i + 5))
						{
							pciSegment.MarksLabels.ShippingMarks6 = tags[i + 5];
						}
						if (numberOfTags > (i + 6))
						{
							pciSegment.MarksLabels.ShippingMarks7 = tags[i + 6];
						}
						if (numberOfTags > (i + 7))
						{
							pciSegment.MarksLabels.ShippingMarks8 = tags[i + 7];
						}
						if (numberOfTags > (i + 8))
						{
							pciSegment.MarksLabels.ShippingMarks9 = tags[i + 8];
						}
						if (numberOfTags > (i + 9))
						{
							pciSegment.MarksLabels.ShippingMarks10 = tags[i + 9];
						}
					}
				}
			}
		}

		public static SegmentGroup31 SetPACSegmentForExternal(SegmentGroup31MessageSection sg31Section, IExternalPackagesInfoCommon externalPackages, PackagingLevelCodedList packagingLevel)
		{
			var sg31 = sg31Section.InstantiateAChildAndAddItToChildrenCollection();
			var pacSegment = sg31.PAC.InstantiateAChildAndAddItToChildrenCollection();
			pacSegment.NumberOfPackages = Utilities.FormatNumberFromZLongNational(externalPackages.NumberOfPackages, 0);
			pacSegment.PackagingDetails.PackagingLevelCoded = packagingLevel;
			pacSegment.PackageType.TypeOfPackagesIdentification = externalPackages.PackageType;
			return sg31;
		}

		public static void AddNewSG31Group(SegmentGroup31MessageSection sg31Section, IInternalPackagesInfoCommon internalPackages, PackagingLevelCodedList packagingLevel)
		{
			if (internalPackages != null && internalPackages.Packages.Any())
			{
				var (pacSegment, sg32) = SetPACSegmentAndAddNewSG32GroupForInternal(sg31Section, packagingLevel);
				if (internalPackages.Packages.Skip(1).Any())
				{
					SetMultipleInternalPackages(sg32, internalPackages);
				}
				else
				{
					SetSingleInternalPackage(sg32, pacSegment, internalPackages);
				}
			}
		}

		public static (PACSegment pacSegment, SegmentGroup32 sg32) SetPACSegmentAndAddNewSG32GroupForInternal(SegmentGroup31MessageSection sg31Section, PackagingLevelCodedList packagingLevel)
		{
			var sg31 = sg31Section.InstantiateAChildAndAddItToChildrenCollection();
			var pacSegment = sg31.PAC.InstantiateAChildAndAddItToChildrenCollection();
			pacSegment.PackagingDetails.PackagingLevelCoded = packagingLevel;
			return (pacSegment, sg31.Group32.InstantiateAChildAndAddItToChildrenCollection());
		}

		public static void SetMultipleInternalPackages(SegmentGroup32 sg32, IInternalPackagesInfoCommon internalPackages)
		{
			foreach (var package in internalPackages.Packages)
			{
				var pciSegment = sg32.PCI.InstantiateAChildAndAddItToChildrenCollection();
				var splitter = new TextSplitter(MarksLabelsShippingMarksLength)
				{
					Text = package.Tag
				};
				pciSegment.MarksLabels.ShippingMarks1 = splitter[0];
				pciSegment.MarksLabels.ShippingMarks2 = splitter[1];
				pciSegment.MarksLabels.ShippingMarks3 = Utilities.FormatNumberFromZLongNational(package.NumberOfElements, 0);
				pciSegment.ContainerPackageStatusCoded = ContainerPackageStatusCodedList.GetFromString(package.ElementsType);
			}
		}

		public static void SetSingleInternalPackage(SegmentGroup32 sg32, PACSegment pacSegment, IInternalPackagesInfoCommon internalPackages)
		{
			var package = internalPackages.Packages.First();
			pacSegment.NumberOfPackages = Utilities.FormatNumberFromZLongNational(package.NumberOfElements, 0);
			pacSegment.PackageType.TypeOfPackagesIdentification = ContainerPackageStatusCodedList.GetFromString(package.ElementsType);
			var pciSegment = sg32.PCI.InstantiateAChildAndAddItToChildrenCollection();
			var splitter = new TextSplitter(MarksLabelsShippingMarksLength)
			{
				Text = package.Tag
			};
			pciSegment.MarksLabels.ShippingMarks1 = splitter[0];
			pciSegment.MarksLabels.ShippingMarks2 = splitter[1];
		}

		public static void AddNewSG31Group(SegmentGroup31MessageSection sg31Section, IVehiclePackagesInfoCommon vehiclePackages, PackagingLevelCodedList packagingLevel)
		{
			if (vehiclePackages != null && vehiclePackages.Packages.Any())
			{
				var sg31 = sg31Section.InstantiateAChildAndAddItToChildrenCollection();
				var pacSegment = sg31.PAC.InstantiateAChildAndAddItToChildrenCollection();
				pacSegment.PackagingDetails.PackagingLevelCoded = packagingLevel;

				var sg32 = sg31.Group32.InstantiateAChildAndAddItToChildrenCollection();
				foreach (var package in vehiclePackages.Packages)
				{
					var pciSegment = sg32.PCI.InstantiateAChildAndAddItToChildrenCollection();
					pciSegment.MarksLabels.ShippingMarks1 = package.Chassis;
					pciSegment.MarksLabels.ShippingMarks2 = package.Brand;
					pciSegment.MarksLabels.ShippingMarks3 = package.Model;
				}
			}
		}

		#endregion

		public static void AddNewMOASegmentSG33Group(SegmentGroup33MessageSection sg33Section, MonetaryAmountTypeQualifierList qualifier, ZDecimal totalAmount, bool needsDecimals = false, int valueDecimalPlace = 2)
		{
			if (qualifier != null && !totalAmount.IsEmpty)
			{
				var sg33 = sg33Section.InstantiateAChildAndAddItToChildrenCollection();
				var moaSegment = sg33.MOA.InstantiateAChildAndAddItToChildrenCollection();
				moaSegment.MonetaryAmount.MonetaryAmountTypeQualifier = qualifier;
				moaSegment.MonetaryAmount.MonetaryAmount = totalAmount.IsInteger && !needsDecimals ? Utilities.FormatNumberNational(totalAmount, 0) : Utilities.FormatNumberNational(totalAmount, valueDecimalPlace);
			}
		}

		public static void AddNewDOCSegment(DOCSegmentMessageSection docSection, ZString docName, ZString docNumber, ZString docSource, ZString docNameCoded)
		{
			var docSegment = docSection.InstantiateAChildAndAddItToChildrenCollection();
			if (!docName.IsEmpty)
			{
				docSegment.DocumentMessageName.DocumentMessageName = docName;
			}
			docSegment.DocumentMessageDetails.DocumentMessageNumber = docNumber;
			if (!docSource.IsEmpty)
			{
				docSegment.DocumentMessageDetails.DocumentMessageSource = docSource;
			}
			if (!docNameCoded.IsEmpty)
			{
				docSegment.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.GetFromString(docNameCoded);
			}
		}

		public static void PopulateUNS2Segment(UNSSegment unsSegment)
		{
			unsSegment.SectionIdentification = SectionIdentificationList.DetailSummarySectionSeparation;
		}

		public static void AddNewCNTSegment(CNTSegmentMessageSection cntSection, ZString controlValue, ControlQualifierList qualifier)
		{
			if (!controlValue.IsEmpty)
			{
				var cntSegment = cntSection.InstantiateAChildAndAddItToChildrenCollection();
				cntSegment.Control.ControlValue = controlValue;
				cntSegment.Control.ControlQualifier = qualifier;
			}
		}

		public static void PopulateUNTSegment(UNTSegment untSegment, int segmentCount)
		{
			untSegment.NumberOfSegmentsInTheMessage = segmentCount.ToString(CultureInfo.CurrentCulture);
			untSegment.MessageReferenceNumber = EDIMessage.MessageNumberPlaceHolder;
		}
	}
}
