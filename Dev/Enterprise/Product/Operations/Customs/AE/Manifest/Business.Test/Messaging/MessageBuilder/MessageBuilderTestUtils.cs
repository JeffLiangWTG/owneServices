using System.Collections.Generic;
using Enterprise.Customs.AE.Business;
using Moq;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

static class MessageBuilderTestUtils
{
	public static IMessageDetailsProvider GetMessageDetails()
	{
		var mockMessageDetails = new Mock<IMessageDetailsProvider>();
		mockMessageDetails.Setup(x => x.DocumentCode).Returns("714");
		mockMessageDetails.Setup(x => x.DocumentIdentifier).Returns("0001");
		mockMessageDetails.Setup(x => x.Version).Returns("2");

		return mockMessageDetails.Object;
	}

	public static IDateTimePeriodProvider GetDateTimePeriod()
	{
		var mockDateTimePeriod = new Mock<IDateTimePeriodProvider>();
		mockDateTimePeriod.Setup(x => x.DateTimePeriodFunctionCode).Returns("123");
		mockDateTimePeriod.Setup(x => x.DateTimePeriodText).Returns("20240123");
		mockDateTimePeriod.Setup(x => x.DateTimePeriodFormat).Returns("2");

		return mockDateTimePeriod.Object;
	}

	public static ILocationProvider GetLocation(string locationIdentifier, string functionCode = "123")
	{
		var mockLocation = new Mock<ILocationProvider>();
		mockLocation.Setup(x => x.LocationFunctionCode).Returns(functionCode);
		mockLocation.Setup(x => x.LocationIdentifier).Returns(locationIdentifier);

		return mockLocation.Object;
	}

	public static IReferenceProvider GetReference(string code, string identifier)
	{
		var mockReference = new Mock<IReferenceProvider>();
		mockReference.Setup(x => x.ReferenceCode).Returns(code);
		mockReference.Setup(x => x.ReferenceIdentifier).Returns(identifier);

		return mockReference.Object;
	}

	public static IPartyProvider GetParty(string code, string identifier)
	{
		var mockParty = new Mock<IPartyProvider>();
		mockParty.Setup(x => x.PartyFunctionCode).Returns(code);
		mockParty.Setup(x => x.PartyIdentifier).Returns(identifier);

		return mockParty.Object;
	}

	public static IFreeTextProvider GetFreeText()
	{
		var mockFreeText = new Mock<IFreeTextProvider>();
		mockFreeText.Setup(x => x.SubjectCode).Returns("ABC");
		mockFreeText.Setup(x => x.Text).Returns("XYZ 123");

		return mockFreeText.Object;
	}

	public static ITransportEquipmentInfoProvider GetTransportEquipmentInfo()
	{
		var mockTransportEquipmentInfo = new Mock<ITransportEquipmentInfoProvider>();
		mockTransportEquipmentInfo.Setup(x => x.ContainerDetails).Returns(GetTransportEquipmentDetails());
		mockTransportEquipmentInfo.Setup(x => x.ServiceRequirements).Returns(GetTransportServiceRequirements());
		mockTransportEquipmentInfo.Setup(x => x.GoodsWeightInKgs).Returns(123.45m);
		mockTransportEquipmentInfo.Setup(x => x.SealNumber).Returns("SEAL123");
		mockTransportEquipmentInfo.Setup(x => x.Temperature).Returns(() => null);

		return mockTransportEquipmentInfo.Object;
	}

	static ITransportEquipmentDetailsProvider GetTransportEquipmentDetails()
	{
		var mockTransportEquipmentDetails = new Mock<ITransportEquipmentDetailsProvider>();
		mockTransportEquipmentDetails.Setup(x => x.EquipmentType).Returns("FCL");
		mockTransportEquipmentDetails.Setup(x => x.EquipmentIdentifier).Returns("MKB0123");
		mockTransportEquipmentDetails.Setup(x => x.EquipmentIndicator).Returns("5");
		return mockTransportEquipmentDetails.Object;
	}

	static ITransportServiceRequirementsProvider GetTransportServiceRequirements()
	{
		var mockTransportServiceRequirements = new Mock<ITransportServiceRequirementsProvider>();
		mockTransportServiceRequirements.Setup(x => x.ServiceRequirementCode).Returns("SVR");
		mockTransportServiceRequirements.Setup(x => x.CargoType).Returns("12");

		return mockTransportServiceRequirements.Object;
	}

	static IMeasurementProvider GetMeasurement(string purpose)
	{
		var mockMeasurement = new Mock<IMeasurementProvider>();
		mockMeasurement.Setup(x => x.MeasurementPurpose).Returns(purpose);
		mockMeasurement.Setup(x => x.MeasurementValue).Returns(12.34m);
		mockMeasurement.Setup(x => x.MeasurementUnit).Returns("KGM");

		return mockMeasurement.Object;
	}

	public static IConsignmentInfoProvider GetConsignmentInfo()
	{
		var mockConsignmentInfo = new Mock<IConsignmentInfoProvider>();
		mockConsignmentInfo.Setup(x => x.TotalHouseBills).Returns(1);
		mockConsignmentInfo.Setup(x => x.BillDetails).Returns(GetConsignmentDetails());

		return mockConsignmentInfo.Object;
	}

	static IConsignmentDetailsProvider GetConsignmentDetails()
	{
		var mockConsignmentDetails = new Mock<IConsignmentDetailsProvider>();
		mockConsignmentDetails.Setup(x => x.MonetaryAmounts).Returns(GetMonetaryAmounts());
		mockConsignmentDetails.Setup(x => x.Locations).Returns(GetLocations());
		mockConsignmentDetails.Setup(x => x.Parties).Returns(GetPartiesFromOrgAddress());
		mockConsignmentDetails.Setup(x => x.ManifestNature).Returns("28");
		mockConsignmentDetails.Setup(x => x.Packs).Returns(GetGoodsInfos());

		return mockConsignmentDetails.Object;
	}

	static List<IMonetaryAmountProvider> GetMonetaryAmounts() => new () { GetMonetaryAmount("ABC"), GetMonetaryAmount("XYZ") };

	static IMonetaryAmountProvider GetMonetaryAmount(string currency)
	{
		var mockMonetaryAmount = new Mock<IMonetaryAmountProvider>();
		mockMonetaryAmount.Setup(x => x.AmountType).Returns("123");
		mockMonetaryAmount.Setup(x => x.Amount).Returns(12.34m);
		mockMonetaryAmount.Setup(x => x.Currency).Returns(currency);

		return mockMonetaryAmount.Object;
	}

	static List<ILocationProvider> GetLocations() => new () { GetLocation("AEABC"), GetLocation("AEXYZ") };

	static List<IPartyFromOrgAddressProvider> GetPartiesFromOrgAddress() => new () { GetPartyFromOrgAddress("PTY1"), GetPartyFromOrgAddress("PTY2"), GetPartyFromOrgAddress("PTY3", "CN") };

	static IPartyFromOrgAddressProvider GetPartyFromOrgAddress(string partyName, string partyFunctionCode = "123")
	{
		var mockParty = new Mock<IPartyFromOrgAddressProvider>();
		mockParty.Setup(x => x.PartyFunctionCode).Returns(partyFunctionCode);
		mockParty.Setup(x => x.PartyIdentifier).Returns("ABC");
		mockParty.Setup(x => x.CodeListIdentificationCode).Returns("2");
		mockParty.Setup(x => x.PartyName).Returns(partyName);
		mockParty.Setup(x => x.StreetAddress).Returns("XYZ 123");
		mockParty.Setup(x => x.City).Returns("City");
		mockParty.Setup(x => x.Country).Returns("AE");
		mockParty.Setup(x => x.ContactCommunication).Returns(GetContactCommunication());

		return mockParty.Object;
	}

	static IPartyContactCommunicationProvider GetContactCommunication()
	{
		var mockContactCommunication = new Mock<IPartyContactCommunicationProvider>();
		mockContactCommunication.Setup(x => x.CommunicationCode).Returns("EM");
		mockContactCommunication.Setup(x => x.CommunicationIdentifier).Returns("+abc@xyz.com");

		return mockContactCommunication.Object;
	}

	static List<IGoodsInfoProvider> GetGoodsInfos() => new () { GetGoodsInfo("Good1"), GetGoodsInfo("Goods2") };

	static IGoodsInfoProvider GetGoodsInfo(string description)
	{
		var mockGoodsInfo = new Mock<IGoodsInfoProvider>();
		mockGoodsInfo.Setup(x => x.Goods).Returns(GetGoodsDetails());
		mockGoodsInfo.Setup(x => x.GoodsDescription).Returns(GetFreeText());
		mockGoodsInfo.Setup(x => x.Measurements).Returns(GetMeasurements());
		mockGoodsInfo.Setup(x => x.GoodsContainer).Returns(GetGoodsContainerDetails());
		mockGoodsInfo.Setup(x => x.GoodsMarksDescription).Returns(description);
		mockGoodsInfo.Setup(x => x.CustomsGoodsIdentifier).Returns("ID123");
		mockGoodsInfo.Setup(x => x.OriginCountry).Returns(GetLocation("AE", "27"));

		return mockGoodsInfo.Object;
	}

	static IGoodsDetailsProvider GetGoodsDetails()
	{
		var mockGoodsDetails = new Mock<IGoodsDetailsProvider>();
		mockGoodsDetails.Setup(x => x.PackageLineNo).Returns(1);
		mockGoodsDetails.Setup(x => x.PackageQuantity).Returns(123);
		mockGoodsDetails.Setup(x => x.PackageType).Returns("ABC Desc");
		mockGoodsDetails.Setup(x => x.PackageTypeCode).Returns("ABC");

		return mockGoodsDetails.Object;
	}

	static List<IMeasurementProvider> GetMeasurements() => new () { GetMeasurement("WGT"), GetMeasurement("VOL") };

	static IGoodsContainerDetailsProvider GetGoodsContainerDetails()
	{
		var mockGoodsContainerDetails = new Mock<IGoodsContainerDetailsProvider>();
		mockGoodsContainerDetails.Setup(x => x.ContainerIdentifier).Returns("CONT");
		mockGoodsContainerDetails.Setup(x => x.PackageQuantity).Returns(123);

		return mockGoodsContainerDetails.Object;
	}
}
