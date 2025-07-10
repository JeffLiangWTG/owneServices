using System;
using System.Linq;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Segments;

namespace Enterprise.Customs.AE.Manifest.Business;

static class SegmentBuilder
{
	public static void PopulateBGMSegment(Func<BGMSegment> bGMProvider, IMessageDetailsProvider messageDetailsProvider)
	{
		if (messageDetailsProvider == null)
		{
			return;
		}
		var bGM = bGMProvider();
		bGM.DocumentMessageName.DocumentNameCode = DocumentNameCodeList.GetFromString(messageDetailsProvider.DocumentCode);
		bGM.DocumentMessageIdentification.DocumentIdentifier = messageDetailsProvider.DocumentIdentifier;
		bGM.DocumentMessageIdentification.VersionIdentifier = messageDetailsProvider.Version;
		bGM.MessageFunctionCode = MessageFunctionCodeList.GetFromString(messageDetailsProvider.MessageFunction);
	}

	public static void PopulateLOCSegment(Func<LOCSegment> lOCProvider, ILocationProvider locationProvider)
	{
		if (locationProvider == null)
		{
			return;
		}
		var lOC = lOCProvider();
		lOC.LocationFunctionCodeQualifier = LocationFunctionCodeQualifierList.GetFromString(locationProvider.LocationFunctionCode);
		lOC.LocationIdentification.LocationIdentifier = locationProvider.LocationIdentifier;
	}

	public static void PopulateRFFSegment(Func<RFFSegment> rFFProvider, IReferenceProvider referenceProvider)
	{
		if (referenceProvider == null)
		{
			return;
		}
		var rFF = rFFProvider();
		rFF.Reference.ReferenceCodeQualifier = ReferenceCodeQualifierList.GetFromString(referenceProvider.ReferenceCode);
		rFF.Reference.ReferenceIdentifier = referenceProvider.ReferenceIdentifier;
	}

	public static void PopulateNADSegment(Func<NADSegment> nADProvider, IPartyProvider partyProvider)
	{
		if (partyProvider == null)
		{
			return;
		}
		var nAD = nADProvider();
		nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(partyProvider.PartyFunctionCode);
		nAD.PartyIdentificationDetails.PartyIdentifier = partyProvider.PartyIdentifier;
	}

	public static void PopulateNADSegment(Func<NADSegment> nADProvider, IPartyFromOrgAddressProvider partyProvider)
	{
		if (partyProvider == null)
		{
			return;
		}
		var nAD = nADProvider();
		nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.GetFromString(partyProvider.PartyFunctionCode);
		nAD.PartyIdentificationDetails.PartyIdentifier = partyProvider.PartyIdentifier;
		nAD.PartyIdentificationDetails.CodeListIdentificationCode = partyProvider.CodeListIdentificationCode;
		nAD.PartyName.PartyName1 = partyProvider.PartyName;
		nAD.Street.StreetAndNumberOrPostOfficeBoxIdentifier1 = partyProvider.StreetAddress;
		nAD.CityName = partyProvider.City;
		nAD.CountryIdentifier = partyProvider.Country;
	}

	public static void PopulateNADSegment(Func<NADSegment> nADProvider, string partyName)
	{
		var nAD = nADProvider();
		nAD.PartyFunctionCodeQualifier = PartyFunctionCodeQualifierList.FreightPayerOnBehalfOfTheConsignee;
		nAD.PartyName.PartyName1 = partyName;
	}

	public static void PopulateFTXSegment(Func<FTXSegment> fTXProvider, IFreeTextProvider freeTextProvider)
	{
		if (freeTextProvider == null)
		{
			return;
		}
		var fTX = fTXProvider();
		fTX.TextSubjectCodeQualifier = TextSubjectCodeQualifierList.GetFromString(freeTextProvider.SubjectCode);
		fTX.TextLiteral.FreeText1 = freeTextProvider.Text;
	}

	public static void PopulateEQDSegment(Func<EQDSegment> eQDProvider, ITransportEquipmentDetailsProvider transportEquipmentDetailsProvider)
	{
		if (transportEquipmentDetailsProvider == null)
		{
			return;
		}
		var eQD = eQDProvider();
		eQD.EquipmentIdentification.EquipmentIdentifier = transportEquipmentDetailsProvider.EquipmentIdentifier;
		eQD.EquipmentSizeAndType.EquipmentSizeAndTypeDescriptionCode = EquipmentSizeAndTypeDescriptionCodeList.GetFromString(transportEquipmentDetailsProvider.EquipmentType);
		eQD.FullOrEmptyIndicatorCode = FullOrEmptyIndicatorCodeList.GetFromString(transportEquipmentDetailsProvider.EquipmentIndicator);
	}

	public static void PopulateMEASegment(Func<MEASegment> mEAProvider, IMeasurementProvider measurementProvider)
	{
		if (measurementProvider == null)
		{
			return;
		}
		var mEA = mEAProvider();
		mEA.MeasurementDetails.MeasuredAttributeCode = MeasuredAttributeCodeList.GetFromString(measurementProvider.MeasurementPurpose);
		mEA.ValueRange.MeasurementUnitCode = MeasurementUnitCodeList.GetFromString(measurementProvider.MeasurementUnit);
		mEA.ValueRange.Measure = measurementProvider.MeasurementValue.ToString();
	}

	public static void PopulateTMPSegment(Func<TMPSegment> tMPProvider, ITemperatureDetailsProvider temperatureDetailsProvider)
	{
		if (temperatureDetailsProvider == null)
		{
			return;
		}
		var tMP = tMPProvider();
		tMP.TemperatureTypeCodeQualifier = TemperatureTypeCodeQualifierList.GetFromString(temperatureDetailsProvider.TemperatureTypeCode);
		tMP.TemperatureSetting.MeasurementUnitCode = MeasurementUnitCodeList.GetFromString(temperatureDetailsProvider.TemperatureUnit);
		tMP.TemperatureSetting.TemperatureDegree = temperatureDetailsProvider.TemperatureDegree.ToString();
	}

	public static void PopulateMOASegment(Func<MOASegment> mOAProvider, IMonetaryAmountProvider monetaryAmountProvider)
	{
		if (monetaryAmountProvider == null)
		{
			return;
		}
		var mOA = mOAProvider();
		mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier = MonetaryAmountTypeCodeQualifierList.GetFromString(monetaryAmountProvider.AmountType);
		mOA.MonetaryAmount.MonetaryAmount = monetaryAmountProvider.Amount.ToString();
		mOA.MonetaryAmount.CurrencyIdentificationCode = monetaryAmountProvider.Currency;
	}

	public static void PopulateGIDSegment(Func<GIDSegment> gIDProvider, IGoodsDetailsProvider goodsDetailsProvider)
	{
		if (goodsDetailsProvider == null)
		{
			return;
		}
		var gID = gIDProvider();
		gID.GoodsItemNumber = goodsDetailsProvider.PackageLineNo.ToString();
		gID.NumberAndTypeOfPackages1.PackageQuantity = goodsDetailsProvider.PackageQuantity.ToString();
		gID.NumberAndTypeOfPackages1.PackageTypeDescriptionCode = goodsDetailsProvider.PackageTypeCode;
		gID.NumberAndTypeOfPackages1.TypeOfPackages = goodsDetailsProvider.PackageType;
	}

	public static void PopulateSGPSegment(Func<SGPSegment> sGPProvider, IGoodsContainerDetailsProvider goodsContainerDetailsProvider)
	{
		if (goodsContainerDetailsProvider == null)
		{
			return;
		}
		var sGP = sGPProvider();
		sGP.EquipmentIdentification.EquipmentIdentifier = goodsContainerDetailsProvider.ContainerIdentifier;
		sGP.PackageQuantity = goodsContainerDetailsProvider.PackageQuantity.ToString();
	}

	public static void PopulatePCISegment(Func<PCISegment> pCIProvider, string description)
	{
		if (string.IsNullOrEmpty(description))
		{
			return;
		}
		var pCI = pCIProvider();
		const int ElementSize = 35;

		var marksLabelsElement = pCI.MarksLabels;
		var descriptions = description.SplitString(ElementSize).ToList();
		var descriptionsCount = descriptions.Count;

		marksLabelsElement.ShippingMarksDescription1 = descriptions[0];
		if (descriptionsCount > 1)
		{
			marksLabelsElement.ShippingMarksDescription2 = descriptions[1];
		}
		if (descriptionsCount > 2)
		{
			marksLabelsElement.ShippingMarksDescription3 = descriptions[2];
		}
		if (descriptionsCount > 3)
		{
			marksLabelsElement.ShippingMarksDescription4 = descriptions[3];
		}
		if (descriptionsCount > 4)
		{
			marksLabelsElement.ShippingMarksDescription5 = descriptions[4];
		}
		if (descriptionsCount > 5)
		{
			marksLabelsElement.ShippingMarksDescription6 = descriptions[5];
		}
		if (descriptionsCount > 6)
		{
			marksLabelsElement.ShippingMarksDescription7 = descriptions[6];
		}
		if (descriptionsCount > 7)
		{
			marksLabelsElement.ShippingMarksDescription8 = descriptions[7];
		}
		if (descriptionsCount > 8)
		{
			marksLabelsElement.ShippingMarksDescription9 = descriptions[8];
		}
		if (descriptionsCount > 9)
		{
			marksLabelsElement.ShippingMarksDescription10 = descriptions[9];
		}
	}

	public static void PopulateTSRSegment(Func<TSRSegment> tSRProvider, ITransportServiceRequirementsProvider transportServiceRequirementsProvider)
	{
		if (transportServiceRequirementsProvider == null)
		{
			return;
		}
		var tSRSegment = tSRProvider();
		tSRSegment.Service.ServiceRequirementCode1 = ServiceRequirementCodeList.GetFromString(transportServiceRequirementsProvider.ServiceRequirementCode);
		tSRSegment.NatureOfCargo.CargoTypeClassificationCode = CargoTypeClassificationCodeList.GetFromString(transportServiceRequirementsProvider.CargoType);
	}

	public static void PopulateCOMSegment(Func<COMSegment> cOMProvider, IPartyContactCommunicationProvider partyContactCommunicationProvider)
	{
		if (partyContactCommunicationProvider == null)
		{
			return;
		}
		var cOMSegment = cOMProvider();
		cOMSegment.CommunicationContact.CommunicationMeansTypeCode = CommunicationMeansTypeCodeList.GetFromString(partyContactCommunicationProvider.CommunicationCode);

		var identifierFormatted = AECharacterSet.New().FormatElement(partyContactCommunicationProvider.CommunicationIdentifier).Trim();
		cOMSegment.CommunicationContact.CommunicationAddressIdentifier = identifierFormatted;
	}
}
