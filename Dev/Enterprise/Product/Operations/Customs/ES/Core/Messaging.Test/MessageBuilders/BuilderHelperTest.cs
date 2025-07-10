using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public static class BuilderHelperTest
{
	#region Common
	public static IPartyProvider SetUpParty(ZString id, ZString name, ZString address, ZString city, ZString postCode, ZString country)
	{
		var addressInformation = new Mock<IPartyProvider>();
		addressInformation.Setup(m => m.Id).Returns(id);
		addressInformation.Setup(m => m.Name).Returns(name);
		addressInformation.Setup(m => m.Address).Returns(address);
		addressInformation.Setup(m => m.City).Returns(city);
		addressInformation.Setup(m => m.PostCode).Returns(postCode);
		addressInformation.Setup(m => m.Country).Returns(country);
		return addressInformation.Object;
	}

	public static IPartyNameProvider SetUpPartyName(ZString id, ZString name)
	{
		var mockDeclarant = new Mock<IPartyNameProvider>();
		mockDeclarant.Setup(m => m.Id).Returns(id);
		mockDeclarant.Setup(m => m.Name).Returns(name);
		return mockDeclarant.Object;
	}

	public static IPartyIdProvider SetUpPartyId(ZString id)
	{
		var mockDeclarant = new Mock<IPartyIdProvider>();
		mockDeclarant.Setup(m => m.Id).Returns(id);
		return mockDeclarant.Object;
	}

	public static IPartyContactProvider SetUpContactInformation(ZString name, ZString email, ZString phoneNumber)
	{
		var mockContactInformation = new Mock<IPartyContactProvider>();
		mockContactInformation.Setup(m => m.Name).Returns(name);
		mockContactInformation.Setup(m => m.Email).Returns(email);
		mockContactInformation.Setup(m => m.PhoneNumber).Returns(phoneNumber);
		return mockContactInformation.Object;
	}

	public static IPartyAddressProvider SetUpAddress(ZString address, ZString city, ZString postCode, ZString country)
	{
		var mockAddress = new Mock<IPartyAddressProvider>();
		mockAddress.Setup(m => m.Address).Returns(address);
		mockAddress.Setup(m => m.City).Returns(city);
		mockAddress.Setup(m => m.PostCode).Returns(postCode);
		mockAddress.Setup(m => m.Country).Returns(country);
		return mockAddress.Object;
	}

	public static IPartyIdProviderWithContactPerson SetUpPartyIdProviderWithContactPerson(ZString id, IPartyContactProvider contactPerson)
	{
		var mockDeclarant = new Mock<IPartyIdProviderWithContactPerson>();
		mockDeclarant.Setup(m => m.Id).Returns(id);
		mockDeclarant.Setup(m => m.ContactPerson).Returns(contactPerson);
		return mockDeclarant.Object;
	}

	public static ICommonRepresentativeWithContactPerson SetUpCommonRepresentativeWithContactPerson(ZString id, ZString status, IPartyContactProvider contactPerson)
	{
		var mockRepresentative = new Mock<ICommonRepresentativeWithContactPerson>();
		mockRepresentative.Setup(m => m.Id).Returns(id);
		mockRepresentative.Setup(m => m.Status).Returns(status);
		mockRepresentative.Setup(m => m.ContactPerson).Returns(contactPerson);
		return mockRepresentative.Object;
	}

	public static IInternalPackageIdentificationCommon SetUpInternalPackages()
	{
		var mockPackages = new Mock<IInternalPackageIdentificationCommon>();
		mockPackages.Setup(m => m.Tag).Returns("MARCABULTIMARCABULTIMARCABULTIAASDNT");
		mockPackages.Setup(m => m.ElementsType).Returns("BX");
		mockPackages.Setup(m => m.NumberOfElements).Returns(50L);
		return mockPackages.Object;
	}

	public static IPackageCommonNumbers SetUpPackageCommon(ZInt packagesQty, ZInt piecesQty)
	{
		var mockPackage = new Mock<IPackageCommonNumbers>();
		mockPackage.Setup(m => m.PackageType).Returns("BX");
		mockPackage.Setup(m => m.Marks).Returns("Cubo entero 001");
		mockPackage.Setup(m => m.PackagesQty).Returns(packagesQty);
		mockPackage.Setup(m => m.PiecesQty).Returns(piecesQty);
		return mockPackage.Object;
	}

	public static IExternalPackagesInfoCommon SetUpExternalPackages(ZLong numberofPackages, IEnumerable<ZString> tags)
	{
		var mockExternalPackages = new Mock<IExternalPackagesInfoCommon>();
		mockExternalPackages.Setup(m => m.NumberOfPackages).Returns(numberofPackages);
		mockExternalPackages.Setup(m => m.PackageType).Returns("CT");
		mockExternalPackages.Setup(m => m.Tags).Returns((IReadOnlyCollection<ZString>)tags);
		return mockExternalPackages.Object;
	}

	public static IVehicleCommon SetUpVehicle(ZString chassis, ZString brand, ZString model)
	{
		var mockVehicle = new Mock<IVehicleCommon>();
		mockVehicle.Setup(m => m.Chassis).Returns(chassis);
		mockVehicle.Setup(m => m.Brand).Returns(brand);
		mockVehicle.Setup(m => m.Model).Returns(model);
		return mockVehicle.Object;
	}

	public static IDocumentsCommon SetUpDocument(ZString name, ZString number)
	{
		var mockDocument = new Mock<IDocumentsCommon>();
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		return mockDocument.Object;
	}

	public static ICommonDocumentSequenceNumber SetUpDocumentSequenceNumber(ZString sequenceNumber, ZString name, ZString number)
	{
		var mockTransportDocument = new Mock<ICommonDocumentSequenceNumber>();
		mockTransportDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockTransportDocument.Setup(m => m.Name).Returns(name);
		mockTransportDocument.Setup(m => m.Number).Returns(number);
		return mockTransportDocument.Object;
	}

	public static ICommonDocumentGoodsItemId SetupDocumentWithGoodItemId(ZString name, ZString number, ZString goodItemId)
	{
		var mockDocument = new Mock<ICommonDocumentGoodsItemId>();
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.GoodsItemId).Returns(goodItemId);
		return mockDocument.Object;
	}

	public static Mock<ITransportMediumInfoCommon> SetUpTransportMediumInfo(ZString mode, ZString id, ZString nationality)
	{
		var mockTransportMediumInfo = new Mock<ITransportMediumInfoCommon>();
		mockTransportMediumInfo.Setup(m => m.TransportMode).Returns(mode);
		mockTransportMediumInfo.Setup(m => m.TransportId).Returns(id);
		mockTransportMediumInfo.Setup(m => m.TransportNationality).Returns(nationality);
		return mockTransportMediumInfo;
	}

	public static Mock<ISealCommon> SetUpSeal(ZString sequenceNuber, ZString seal)
	{
		var mockSeal = new Mock<ISealCommon>();
		mockSeal.Setup(m => m.SequenceNumber).Returns(sequenceNuber);
		mockSeal.Setup(m => m.SealNumber).Returns(seal);

		return mockSeal;
	}

	public static ICommodityCodeCommon SetUpCommodityCode(ZString harmonizedCode, ZString combineCode)
	{
		var mockCommodityCode = new Mock<ICommodityCodeCommon>();
		mockCommodityCode.Setup(m => m.TariffCode).Returns(harmonizedCode);
		mockCommodityCode.Setup(m => m.TariffCodeCombined).Returns(combineCode);
		return mockCommodityCode.Object;
	}

	public static IGoodsMeasureCommon SetUpGoodsMeasure(ZDecimal grossWeight, ZDecimal netWeight)
	{
		var mockGoodsMeasure = new Mock<IGoodsMeasureCommon>();
		mockGoodsMeasure.Setup(m => m.GrossWeight).Returns(grossWeight);
		mockGoodsMeasure.Setup(m => m.NetWeight).Returns(netWeight);
		return mockGoodsMeasure.Object;
	}

	public static ZBlob GetCertificateBytes()
	{
		using (var certificateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.ES.Messaging.Test.MessageBuilders.TestFiles.ESCertificate_password.pfx"))
		using (var memoryStream = new MemoryStream())
		{
			certificateStream.CopyTo(memoryStream);
			return memoryStream.ToArray();
		}
	}

	public static ZString CertificatePassword => "password";

	public static IPartyEmailProvider SetUpPartyEmail(ZString id, ZString name, ZString email)
	{
		var mockDeclarant = new Mock<IPartyEmailProvider>();
		mockDeclarant.Setup(m => m.Id).Returns(id);
		mockDeclarant.Setup(m => m.Name).Returns(name);
		mockDeclarant.Setup(m => m.EmailAddress).Returns(email);
		return mockDeclarant.Object;
	}

	public static IAnnexDocCommon SetUpAnnexDocCommon(ZString description, ZString referenceNumber, ZBlob image, ZString extension)
	{
		var mockReceptionDocumento = new Mock<IAnnexDocCommon>();
		mockReceptionDocumento.Setup(m => m.Description).Returns(description);
		mockReceptionDocumento.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
		mockReceptionDocumento.Setup(m => m.Image).Returns(image);
		mockReceptionDocumento.Setup(m => m.Extension).Returns(extension);
		return mockReceptionDocumento.Object;
	}

	public static IGenericLocation SetUpGenericLocation(ZString type, ZString qualifier)
	{
		var mockGenericLocation = new Mock<IGenericLocation>();
		mockGenericLocation.Setup(m => m.Type).Returns(type);
		mockGenericLocation.Setup(m => m.Qualifier).Returns(qualifier);
		mockGenericLocation.Setup(m => m.Coded).Returns(SetUpCodedGenericLocation());
		mockGenericLocation.Setup(m => m.Address).Returns(SetUpAddressGenericLocation());
		return mockGenericLocation.Object;
	}

	public static ICodedGenericLocation SetUpCodedGenericLocation()
	{
		var mockLocation = new Mock<ICodedGenericLocation>();
		mockLocation.Setup(m => m.UNLOCOCode).Returns("PTLIS");
		mockLocation.Setup(m => m.CustomsOffice).Returns("IT307100");
		mockLocation.Setup(m => m.GPS).Returns(SetUpGPS());
		mockLocation.Setup(m => m.EconomicOperator).Returns("159487263");
		mockLocation.Setup(m => m.AuthorisationNumber).Returns("ITTST307100XXXXX01");
		mockLocation.Setup(m => m.AdditionalId).Returns("AB03");
		return mockLocation.Object;
	}

	public static ICommonGNSS SetUpGPS()
	{
		var mockGPS = new Mock<ICommonGNSS>();
		mockGPS.Setup(m => m.Longitude).Returns("-3.669839");
		mockGPS.Setup(m => m.Latitude).Returns("40.462974");
		return mockGPS.Object;
	}

	public static IPartyAddressProvider SetUpAddressGenericLocation()
	{
		var mockAddress = new Mock<IPartyAddressProvider>();
		mockAddress.Setup(m => m.Address).Returns("street and number aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbb");
		mockAddress.Setup(m => m.City).Returns("city");
		mockAddress.Setup(m => m.PostCode).Returns("12354");
		mockAddress.Setup(m => m.Country).Returns("IT");
		return mockAddress.Object;
	}

	public static ICommonAuthorisation SetUpCommonAuthorisation(ZString sequenceNumber, ZString type, ZString reference, ZString holder)
	{
		var mockAuthorisation = new Mock<ICommonAuthorisation>();
		mockAuthorisation.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockAuthorisation.Setup(m => m.Type).Returns(type);
		mockAuthorisation.Setup(m => m.ReferenceNumber).Returns(reference);
		mockAuthorisation.Setup(m => m.Holder).Returns(holder);

		return mockAuthorisation.Object;
	}

	public static ICommonAdditionalSupplyChainActorSeqNum SetUpSupplyActor(ZString sequenceNumber, ZString role, ZString id)
	{
		var mockSupplyActor = new Mock<ICommonAdditionalSupplyChainActorSeqNum>();
		mockSupplyActor.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockSupplyActor.Setup(m => m.Role).Returns(role);
		mockSupplyActor.Setup(m => m.Id).Returns(id);

		return mockSupplyActor.Object;
	}

	public static ICommonDeliveryTerms SetUpCommonDeliveryTerms()
	{
		var mockDeliveryTerms = new Mock<ICommonDeliveryTerms>();
		mockDeliveryTerms.Setup(m => m.Incoterm).Returns("XXX");
		mockDeliveryTerms.Setup(m => m.UNLCode).Returns("UNLCode");
		mockDeliveryTerms.Setup(m => m.IncotermLocation).Returns("ES009999000002");
		mockDeliveryTerms.Setup(m => m.DeliveryCountry).Returns("ES");
		mockDeliveryTerms.Setup(m => m.DeliveryText).Returns("hh");

		return mockDeliveryTerms.Object;
	}

	public static IWarehouseCommon SetUpWarehouseCommon()
	{
		var mockWarehouse = new Mock<IWarehouseCommon>();
		mockWarehouse.Setup(m => m.Type).Returns("R");
		mockWarehouse.Setup(m => m.Identifier).Returns("Warehouse");

		return mockWarehouse.Object;
	}

	public static ICommonArrivalTransportMeans SetUpCommonArrivalTransportMeans(ZString id, ZString type)
	{
		var mockTransportMeans = new Mock<ICommonArrivalTransportMeans>();
		mockTransportMeans.Setup(m => m.Id).Returns(id);
		mockTransportMeans.Setup(m => m.Type).Returns(type);
		return mockTransportMeans.Object;
	}

	public static ICommonAdditionalCode SetUpAdditionalCode(ZString sequenceNumber, ZString code)
	{
		var mockAdditionalCode = new Mock<ICommonAdditionalCode>();
		mockAdditionalCode.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockAdditionalCode.Setup(m => m.Code).Returns(code);

		return mockAdditionalCode.Object;
	}

	public static ICommonGoodsMeasureWithSupUnitsAndSpecified SetUpCommonGoodsMeasureWithSupUnitsAndSpecified()
	{
		var mockGoodsMeasure = new Mock<ICommonGoodsMeasureWithSupUnitsAndSpecified>();
		mockGoodsMeasure.Setup(m => m.GrossWeight).Returns(310);
		mockGoodsMeasure.Setup(m => m.GrossWeightSpecified).Returns(true);
		mockGoodsMeasure.Setup(m => m.NetWeight).Returns(300);
		mockGoodsMeasure.Setup(m => m.NetWeightSpecified).Returns(true);
		mockGoodsMeasure.Setup(m => m.SupplementaryUnits).Returns(1);
		mockGoodsMeasure.Setup(m => m.SupplementaryUnitsSpecified).Returns(true);

		return mockGoodsMeasure.Object;
	}

	public static ICommonPackageWithSequenceAndPackNum SetUpCommonPackages(ZString sequenceNumber, ZString type, ZString marks, ZString quantity)
	{
		var mockPackages = new Mock<ICommonPackageWithSequenceAndPackNum>();
		mockPackages.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockPackages.Setup(m => m.PackageType).Returns(type);
		mockPackages.Setup(m => m.Marks).Returns(marks);
		mockPackages.Setup(m => m.NumberOfPackages).Returns(quantity);

		return mockPackages.Object;
	}

	#endregion

	#region Export

	public static IExportDeclarantPartyIdProvider SetUpExportDeclarantPartyId(ZString nameCode, ZString emailAddress)
	{
		var mockDeclarant = new Mock<IExportDeclarantPartyIdProvider>();
		mockDeclarant.Setup(m => m.PartyQualifier).Returns("2");
		mockDeclarant.Setup(m => m.Id).Returns("1210244B");
		mockDeclarant.Setup(m => m.Name).Returns("GUTIERREZ S.A.");
		mockDeclarant.Setup(m => m.NameCode).Returns(nameCode);
		mockDeclarant.Setup(m => m.EmailAddress).Returns(emailAddress);
		return mockDeclarant.Object;
	}
	#endregion

	public static string AsString(this Stream stream)
	{
		if (stream == null)
		{
			return null;
		}

		using (StreamReader streamReader = new StreamReader(stream))
		{
			return streamReader.ReadToEnd();
		}
	}
}
