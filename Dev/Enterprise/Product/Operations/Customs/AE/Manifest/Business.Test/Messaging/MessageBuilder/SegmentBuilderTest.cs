using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.AE.Business.Testing;
using Enterprise.Edifact.D23A.Segments;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class SegmentBuilderTest : TestCaseWithFactory
{
	[ExpectNoExceptions]
	public void TestPopulateBGMSegment()
	{
		var mockMessageDetails = TestHelperUtils.CreateMockInstance<IMessageDetailsProvider>();
		var bGM = new BGMSegment();
		SegmentBuilder.PopulateBGMSegment(() => bGM, mockMessageDetails.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(bGM.DocumentMessageName.DocumentNameCode.ToString(), Is.EqualTo("DocumentCode_X"), "DocumentCode");
			NUnit.Framework.Assert.That(bGM.DocumentMessageIdentification.DocumentIdentifier, Is.EqualTo("DocumentIdentifier_X"), "DocumentIdentifier");
			NUnit.Framework.Assert.That(bGM.DocumentMessageIdentification.VersionIdentifier, Is.EqualTo("Version_X"), "Version");
			NUnit.Framework.Assert.That(bGM.MessageFunctionCode.ToString(), Is.EqualTo("MessageFunction_X"), "MessageFunction");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateLOCSegment()
	{
		var mockLocation = TestHelperUtils.CreateMockInstance<ILocationProvider>();
		var lOC = new LOCSegment();
		SegmentBuilder.PopulateLOCSegment(() => lOC, mockLocation.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(lOC.LocationFunctionCodeQualifier.ToString(), Is.EqualTo("LocationFunctionCode_X"), "LocationFunctionCode");
			NUnit.Framework.Assert.That(lOC.LocationIdentification.LocationIdentifier, Is.EqualTo("LocationIdentifier_X"), "LocationIdentifier");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateRFFSegment()
	{
		var mockReference = TestHelperUtils.CreateMockInstance<IReferenceProvider>();
		var rFF = new RFFSegment();
		SegmentBuilder.PopulateRFFSegment(() => rFF, mockReference.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(rFF.Reference.ReferenceCodeQualifier.ToString(), Is.EqualTo("ReferenceCode_X"), "ReferenceCode");
			NUnit.Framework.Assert.That(rFF.Reference.ReferenceIdentifier, Is.EqualTo("ReferenceIdentifier_X"), "ReferenceIdentifier");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateNADSegment_MapParty()
	{
		var mockParty = TestHelperUtils.CreateMockInstance<IPartyProvider>();
		var nAD = new NADSegment();
		SegmentBuilder.PopulateNADSegment(() => nAD, mockParty.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(nAD.PartyFunctionCodeQualifier.ToString(), Is.EqualTo("PartyFunctionCode_X"), "PartyFunctionCode");
			NUnit.Framework.Assert.That(nAD.PartyIdentificationDetails.PartyIdentifier, Is.EqualTo("PartyIdentifier_X"), "PartyIdentifier");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateNADSegment_MapPartyFromOrgAddress()
	{
		var mockPartyFromOrgAddress = TestHelperUtils.CreateMockInstance<IPartyFromOrgAddressProvider>();
		var nAD = new NADSegment();
		SegmentBuilder.PopulateNADSegment(() => nAD, mockPartyFromOrgAddress.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(nAD.PartyFunctionCodeQualifier.ToString(), Is.EqualTo("PartyFunctionCode_X"), "PartyFunctionCode");
			NUnit.Framework.Assert.That(nAD.PartyIdentificationDetails.PartyIdentifier, Is.EqualTo("PartyIdentifier_X"), "PartyIdentifier");
			NUnit.Framework.Assert.That(nAD.PartyIdentificationDetails.CodeListIdentificationCode, Is.EqualTo("CodeListIdentificationCode_X"), "CodeListIdentificationCode");
			NUnit.Framework.Assert.That(nAD.PartyName.PartyName1, Is.EqualTo("PartyName_X"), "PartyName");
			NUnit.Framework.Assert.That(nAD.Street.StreetAndNumberOrPostOfficeBoxIdentifier1, Is.EqualTo("StreetAddress_X"), "StreetAddress");
			NUnit.Framework.Assert.That(nAD.CityName, Is.EqualTo("City_X"), "City");
			NUnit.Framework.Assert.That(nAD.CountryIdentifier, Is.EqualTo("Country_X"), "Country");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateFTXSegment()
	{
		var mockFreeText = TestHelperUtils.CreateMockInstance<IFreeTextProvider>();
		var fTX = new FTXSegment();
		SegmentBuilder.PopulateFTXSegment(() => fTX, mockFreeText.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(fTX.TextSubjectCodeQualifier.ToString(), Is.EqualTo("SubjectCode_X"), "SubjectCode");
			NUnit.Framework.Assert.That(fTX.TextLiteral.FreeText1, Is.EqualTo("Text_X"), "Text");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateEQDSegment()
	{
		var mockTransportEquipmentDetails = TestHelperUtils.CreateMockInstance<ITransportEquipmentDetailsProvider>();
		var eQD = new EQDSegment();
		SegmentBuilder.PopulateEQDSegment(() => eQD, mockTransportEquipmentDetails.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(eQD.EquipmentIdentification.EquipmentIdentifier, Is.EqualTo("EquipmentIdentifier_X"), "EquipmentIdentifier");
			NUnit.Framework.Assert.That(eQD.EquipmentSizeAndType.EquipmentSizeAndTypeDescriptionCode.ToString(), Is.EqualTo("EquipmentType_X"), "EquipmentType");
			NUnit.Framework.Assert.That(eQD.FullOrEmptyIndicatorCode.ToString(), Is.EqualTo("EquipmentIndicator_X"), "EquipmentIndicator");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateMEASegment()
	{
		var mockMeasurement = TestHelperUtils.CreateMockInstance<IMeasurementProvider>();
		var mEA = new MEASegment();
		SegmentBuilder.PopulateMEASegment(() => mEA, mockMeasurement.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(mEA.MeasurementDetails.MeasuredAttributeCode.ToString(), Is.EqualTo("MeasurementPurpose_X"), "MeasurementPurpose");
			NUnit.Framework.Assert.That(mEA.ValueRange.MeasurementUnitCode.ToString(), Is.EqualTo("MeasurementUnit_X"), "MeasurementUnit");
			NUnit.Framework.Assert.That(mEA.ValueRange.Measure, Is.EqualTo("123.45"), "MeasurementValue");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateTMPSegment()
	{
		var mockTemperatureDetails = TestHelperUtils.CreateMockInstance<ITemperatureDetailsProvider>();
		var tMP = new TMPSegment();
		SegmentBuilder.PopulateTMPSegment(() => tMP, mockTemperatureDetails.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(tMP.TemperatureTypeCodeQualifier.ToString(), Is.EqualTo("TemperatureTypeCode_X"), "TemperatureTypeCode");
			NUnit.Framework.Assert.That(tMP.TemperatureSetting.MeasurementUnitCode.ToString(), Is.EqualTo("TemperatureUnit_X"), "TemperatureUnit");
			NUnit.Framework.Assert.That(tMP.TemperatureSetting.TemperatureDegree, Is.EqualTo("123.45"), "TemperatureDegree");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateMOASegment()
	{
		var mockMonetaryAmount = TestHelperUtils.CreateMockInstance<IMonetaryAmountProvider>();
		var mOA = new MOASegment();
		SegmentBuilder.PopulateMOASegment(() => mOA, mockMonetaryAmount.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(mOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier.ToString(), Is.EqualTo("AmountType_X"), "AmountType");
			NUnit.Framework.Assert.That(mOA.MonetaryAmount.MonetaryAmount, Is.EqualTo("123.45"), "Amount");
			NUnit.Framework.Assert.That(mOA.MonetaryAmount.CurrencyIdentificationCode, Is.EqualTo("Currency_X"), "Currency");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateGIDSegment()
	{
		var mockGoodsDetailsProvider = TestHelperUtils.CreateMockInstance<IGoodsDetailsProvider>();
		var gID = new GIDSegment();
		SegmentBuilder.PopulateGIDSegment(() => gID, mockGoodsDetailsProvider.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(gID.GoodsItemNumber, Is.EqualTo("123"), "PackageLineNo");
			NUnit.Framework.Assert.That(gID.NumberAndTypeOfPackages1.PackageQuantity, Is.EqualTo("123"), "PackageQuantity");
			NUnit.Framework.Assert.That(gID.NumberAndTypeOfPackages1.TypeOfPackages, Is.EqualTo("PackageType_X"), "PackageType");
			NUnit.Framework.Assert.That(gID.NumberAndTypeOfPackages1.PackageTypeDescriptionCode, Is.EqualTo("PackageTypeCode_X"), "PackageTypeCode");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateSGPSegment()
	{
		var mockGoodsContainerDetailsProvider = TestHelperUtils.CreateMockInstance<IGoodsContainerDetailsProvider>();
		var sGP = new SGPSegment();
		SegmentBuilder.PopulateSGPSegment(() => sGP, mockGoodsContainerDetailsProvider.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(sGP.EquipmentIdentification.EquipmentIdentifier, Is.EqualTo("ContainerIdentifier_X"), "ContainerIdentifier");
			NUnit.Framework.Assert.That(sGP.PackageQuantity, Is.EqualTo("123"), "PackageQuantity");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulatePCISegment()
	{
		var description = new string('A', 316);
		var pCI = new PCISegment();
		SegmentBuilder.PopulatePCISegment(() => pCI, description);
		var expectedElement = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription1, Is.EqualTo(expectedElement), "Element #1");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription2, Is.EqualTo(expectedElement), "Element #2");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription3, Is.EqualTo(expectedElement), "Element #3");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription4, Is.EqualTo(expectedElement), "Element #4");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription5, Is.EqualTo(expectedElement), "Element #5");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription6, Is.EqualTo(expectedElement), "Element #6");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription7, Is.EqualTo(expectedElement), "Element #7");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription8, Is.EqualTo(expectedElement), "Element #8");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription9, Is.EqualTo(expectedElement), "Element #9");
			NUnit.Framework.Assert.That(pCI.MarksLabels.ShippingMarksDescription10, Is.EqualTo("A"), "Element #10");
		});
	}

	[ExpectNoExceptions]
	public void TestPopulateTSRSegment()
	{
		var mockTransportServiceRequirements = TestHelperUtils.CreateMockInstance<ITransportServiceRequirementsProvider>();
		var tSR = new TSRSegment();
		SegmentBuilder.PopulateTSRSegment(() => tSR, mockTransportServiceRequirements.Object);
		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(tSR.Service.ServiceRequirementCode1.ToString(), Is.EqualTo("ServiceRequirementCode_X"), "ServiceRequirementCode");
			NUnit.Framework.Assert.That(tSR.NatureOfCargo.CargoTypeClassificationCode.ToString(), Is.EqualTo("CargoType_X"), "CargoType");
		});
	}
}
