using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public abstract class DeclarationAESCommonMessageBuilderTest<TMessageBuilder, TProvider, T, TExportOperationProvider> : AESCommonMessageBuilderTest<TMessageBuilder, TProvider, T>
	where TProvider : class, IDeclarationAESCommonMessageDataProvider
	where TMessageBuilder : AESCommonMessageBuilder<TProvider, T>
	where TExportOperationProvider : IDeclarationAESExportOperation
{
	#region Tests

	public abstract void TestPopulateExportOperation();
	public abstract void TestPopulateAuthorisation();
	public abstract void TestPopulateCustomsOffices();
	public abstract void TestPopulateExporter();
	public abstract void TestPopulateDeclarant();
	public abstract void TestPopulateRepresentative();
	public abstract void TestPopulateGoodsShipment();
	public abstract void TestPopulateAdditionalSupplyChainActor();
	public abstract void TestPopulateDeliveryTerms();
	public abstract void TestPopulateWarehouse();
	public abstract void TestPopulateConsignment();
	public abstract void TestPopulateCarrier();
	public abstract void TestPopulateConsignor();
	public abstract void TestPopulateConsignee();
	public abstract void TestPopulateTransportEquipment();
	public abstract void TestPopulateSeal();
	public abstract void TestPopulateGoodsReference();
	public abstract void TestPopulateLocationOfGoods();
	public abstract void TestPopulateLocationGNSS();
	public abstract void TestPopulateLocationEconomicOperator();
	public abstract void TestPopulateLocationLocationAddress();
	public abstract void TestPopulateLocationLocationPostcodeAddress();
	public abstract void TestPopulateContactPerson();
	public abstract void TestPopulateDepartureTransportMeans();
	public abstract void TestPopulateCountryOfRoutingOfConsignment();
	public abstract void TestPopulateActiveBorderTransportMeans();
	public abstract void TestPopulateTransportCharges();
	public abstract void TestPopulateLine();
	public abstract void TestPopulateProcedure();
	public abstract void TestPopulateAdditionalProcedure();
	public abstract void TestPopulateOrigin();
	public abstract void TestPopulateCommodity();
	public abstract void TestPopulateCommodityCode();
	public abstract void TestPopulateTARICAdditionalCodes();
	public abstract void TestPopulateNationalAdditionalCodes();
	public abstract void TestPopulateDangerousGood();
	public abstract void TestPopulateGoodsMeasure();
	public abstract void TestPopulatePackage();
	public abstract void TestPopulatePreviousDocument();
	public abstract void TestPopulateSupportingDocument();
	public abstract void TestPopulateTransportDocument();
	public abstract void TestPopulateAdditionalReference();
	public abstract void TestPopulateAdditionalInfo();

	#endregion

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
		mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
		mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

		var mockAuthorisation1 = BuilderHelperTest.SetUpCommonAuthorisation("1", "BTI", "Reference", "A12345678");
		var mockAuthorisation2 = BuilderHelperTest.SetUpCommonAuthorisation("2", "BTI", "Reference", "A12345678");
		var mockAuthorisations = new ICommonAuthorisation[] { mockAuthorisation1, mockAuthorisation2 };
		mockProvider.Setup(m => m.Authorisations).Returns(mockAuthorisations);

		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns("ES009999");
		mockProvider.Setup(m => m.CustomOfficeOfExport).Returns("ES000101");
		mockProvider.Setup(m => m.CustomOfficeOfExit).Returns("ES000102");

		var mockExporter = SetUpExporter("ESA78587268", "Street", "Madrid", "28003", "ES");
		mockProvider.Setup(m => m.Exporter).Returns(mockExporter.Object);

		var mockContactPerson = BuilderHelperTest.SetUpContactInformation("Prometeo", "test_email@taric.es", "616123443");
		var mockDeclarant = BuilderHelperTest.SetUpPartyIdProviderWithContactPerson("ESA78587268", mockContactPerson);
		mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

		var mockRepresentative = BuilderHelperTest.SetUpCommonRepresentativeWithContactPerson("ESA78587268", "2", mockContactPerson);
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative);

		mockGoodsShipment = SetUpHeaderGoodsShipment(mockAuthorisations, mockContactPerson);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
	}

	Mock<IDeclarationAESGoodsShipment> mockGoodsShipment;

	Mock<IDeclarationAESExporter> SetUpExporter(ZString id, ZString address, ZString city, ZString postCode, ZString country)
	{
		var mockExporter = new Mock<IDeclarationAESExporter>();
		mockExporter.Setup(m => m.Id).Returns(id);

		mockExporter.Setup(m => m.Address).Returns(BuilderHelperTest.SetUpAddress(address, city, postCode, country));

		return mockExporter;
	}

	Mock<IDeclarationAESGoodsShipment> SetUpHeaderGoodsShipment(IEnumerable<ICommonAuthorisation> authorisations, IPartyContactProvider contactPerson)
	{
		var mockHeader = new Mock<IDeclarationAESGoodsShipment>();
		mockHeader.Setup(m => m.NatureOfTransaction).Returns("91");
		mockHeader.Setup(m => m.CountryOfExport).Returns("ES");
		mockHeader.Setup(m => m.CountryOfDestination).Returns("AE");

		var mockAdditionalSupplyActor1 = SetUpSupplyActor("1", "CS", "12345678A");
		var mockAdditionalSupplyActor2 = SetUpSupplyActor("2", "CS", "12345678A");
		var mockAdditionalSupplyActors = new ICommonAdditionalSupplyChainActorSeqNum[] { mockAdditionalSupplyActor1.Object, mockAdditionalSupplyActor2.Object };
		mockHeader.Setup(m => m.AdditionalSupplyActors).Returns(mockAdditionalSupplyActors);

		var mockDeliveryTerms = new Mock<ICommonDeliveryTerms>();
		mockDeliveryTerms.Setup(m => m.Incoterm).Returns("XXX");
		mockDeliveryTerms.Setup(m => m.UNLCode).Returns("UNLCode");
		mockDeliveryTerms.Setup(m => m.IncotermLocation).Returns("ES009999000002");
		mockDeliveryTerms.Setup(m => m.DeliveryCountry).Returns("ES");
		mockDeliveryTerms.Setup(m => m.DeliveryText).Returns("hh");
		mockHeader.Setup(m => m.DeliveryTerms).Returns(mockDeliveryTerms.Object);

		var mockWarehouse = BuilderHelperTest.SetUpWarehouseCommon();
		mockHeader.Setup(m => m.Warehouse).Returns(mockWarehouse);

		var mockSupportingDocumentHeader1 = SetUpSupportingDocumentHeader("1", "N381", "Factura2", "Custom2", new DateTime(2022, 7, 21), "2");
		var mockSupportingDocumentHeader2 = SetUpSupportingDocumentHeader("2", "N326", "Proforma2", "Custom2", new DateTime(2022, 7, 21), "2");
		var mockSupportingDocumentsHeader = new IDeclarationAESSupportingDocumentHeader[] { mockSupportingDocumentHeader1.Object, mockSupportingDocumentHeader2.Object };
		mockHeader.Setup(m => m.SupportingDocuments).Returns(mockSupportingDocumentsHeader);

		var mockAdditionalReferenceHeader1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y922", "ES89564");
		var mockAdditionalReferenceHeader2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y923", "Other2");
		var mockAdditionalReferencesHeader = new ICommonDocumentSequenceNumber[] { mockAdditionalReferenceHeader1, mockAdditionalReferenceHeader2 };
		mockHeader.Setup(m => m.AdditionalReferences).Returns(mockAdditionalReferencesHeader);

		var mockAdditionalInfoHeader1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y910", "text10");
		var mockAdditionalInfoHeader2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y911", "text20");
		var mockAdditionalInfosHeader = new ICommonDocumentSequenceNumber[] { mockAdditionalInfoHeader1, mockAdditionalInfoHeader2 };
		mockHeader.Setup(m => m.AdditionalInfos).Returns(mockAdditionalInfosHeader);

		var mockConsignor = new Mock<IPartyIdProvider>();
		mockConsignor.Setup(m => m.Id).Returns("A12345678");

		var mockConsignee = SetUpConsignee("A12345678", "Consignee", "Street", "Madrid", "28003", "ES");

		mockConsignment = SetUpConsignment(mockConsignor.Object, mockConsignee.Object, contactPerson);
		mockHeader.Setup(m => m.Consignment).Returns(mockConsignment.Object);

		var mockTariffAdditionalCode1 = BuilderHelperTest.SetUpAdditionalCode("1", "4321");
		var mockTariffAdditionalCode2 = BuilderHelperTest.SetUpAdditionalCode("2", "5678");
		var mockTariffAdditionalCodes = new ICommonAdditionalCode[] { mockTariffAdditionalCode1, mockTariffAdditionalCode2 };

		var mockTariffNationalAdditionalCode1 = BuilderHelperTest.SetUpAdditionalCode("1", "1234");
		var mockTariffNationalAdditionalCode2 = BuilderHelperTest.SetUpAdditionalCode("2", "1234");
		var mockTariffNationalAdditionalCodes = new ICommonAdditionalCode[] { mockTariffNationalAdditionalCode1, mockTariffNationalAdditionalCode2 };

		mockCommodityCode = SetUpCommodityCode(mockTariffAdditionalCodes, mockTariffNationalAdditionalCodes);
		var mockCommodityCode2 = SetUpCommodityCode(Enumerable.Empty<ICommonAdditionalCode>(), Enumerable.Empty<ICommonAdditionalCode>());

		var mockDangerousGood1 = SetUpDangerousGoods("1", "A10");
		var mockDangerousGood2 = SetUpDangerousGoods("2", "A20");
		var mockDangerousGoods = new ICommonDangerousGoods[] { mockDangerousGood1.Object, mockDangerousGood2.Object };

		var mockPackage1 = BuilderHelperTest.SetUpCommonPackages("1", "VO", "MARCA1", "15");
		var mockPackage2 = BuilderHelperTest.SetUpCommonPackages("2", "CR", "MARCA2", "15");
		var mockPackages = new ICommonPackageWithSequenceAndPackNum[] { mockPackage1, mockPackage2 };

		var mockPreviousDocument1 = SetUpAESCommonDocument("1", "NCLE", "12012021", "1", "NAR", 2.456, true);
		var mockPreviousDocument2 = SetUpAESCommonDocument("2", "NCLE", "12012021", "1", "NAR", 2, false);
		var mockPreviousDocuments = new IAESCommonDocument[] { mockPreviousDocument1.Object, mockPreviousDocument2.Object };

		var mockSupportingDocumentLine1 = SetUpSupportingDocumentLine("1", "N380", "Factura", "Custom", new DateTime(2022, 7, 21), "1", "HL", 50.126m, true);
		var mockSupportingDocumentLine2 = SetUpSupportingDocumentLine("2", "N325", "Proforma", "Custom", new DateTime(2022, 7, 21), "1", "HL", 50, false);
		var mockSupportingDocumentsLine = new IDeclarationAESSupportingDocumentLine[] { mockSupportingDocumentLine1.Object, mockSupportingDocumentLine2.Object };

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "N760", "SO700039");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "N705", "Barco");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };

		var mockAdditionalReferenceLine1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y924", "DE89564");
		var mockAdditionalReferenceLine2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y925", "Other");
		var mockAdditionalReferencesLine = new ICommonDocumentSequenceNumber[] { mockAdditionalReferenceLine1, mockAdditionalReferenceLine2 };

		var mockAdditionalInfoLine1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "Y900", "text1");
		var mockAdditionalInfoLine2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "Y901", "text2");
		var mockAdditionalInfosLine = new ICommonDocumentSequenceNumber[] { mockAdditionalInfoLine1, mockAdditionalInfoLine2 };

		mockLine1 = SetUpLine("1", authorisations, mockConsignor.Object, mockConsignee.Object, mockAdditionalSupplyActors, mockCommodityCode.Object,
			mockDangerousGoods, mockPackages, mockPreviousDocuments, mockSupportingDocumentsLine, mockTransportDocuments, mockAdditionalReferencesLine, mockAdditionalInfosLine);

		var mockLine2 = SetUpLine("2", Enumerable.Empty<ICommonAuthorisation>(), mockConsignor.Object, mockConsignee.Object, Enumerable.Empty<ICommonAdditionalSupplyChainActorSeqNum>(),
			mockCommodityCode2.Object, Enumerable.Empty<ICommonDangerousGoods>(), Enumerable.Empty<ICommonPackageWithSequenceAndPackNum>(),
			Enumerable.Empty<IAESCommonDocument>(), Enumerable.Empty<IDeclarationAESSupportingDocumentLine>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(),
			Enumerable.Empty<ICommonDocumentSequenceNumber>());

		var mockLines = new IDeclarationAESLine[] { mockLine1.Object, mockLine2.Object };

		mockHeader.Setup(m => m.Lines).Returns(mockLines);

		return mockHeader;
	}

	Mock<IDeclarationAESConsignment> mockConsignment;

	Mock<IDeclarationAESConsignee> SetUpConsignee(ZString id, ZString name, ZString address, ZString city, ZString postCode, ZString country)
	{
		var mockConsignee = new Mock<IDeclarationAESConsignee>();
		mockConsignee.Setup(m => m.Id).Returns(id);
		mockConsignee.Setup(m => m.Name).Returns(name);

		mockConsignee.Setup(m => m.Address).Returns(BuilderHelperTest.SetUpAddress(address, city, postCode, country));

		return mockConsignee;
	}

	Mock<IDeclarationAESLine> mockLine1;

	Mock<IDeclarationAESConsignment> SetUpConsignment(IPartyIdProvider consignor, IDeclarationAESConsignee consignee, IPartyContactProvider contactPerson)
	{
		var mockConsignment = new Mock<IDeclarationAESConsignment>();
		mockConsignment.Setup(m => m.IsContainerised).Returns(true);
		mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("3");
		mockConsignment.Setup(m => m.ModeOfTransportAtBorder).Returns("3");
		mockConsignment.Setup(m => m.GrossMass).Returns(420.5);
		mockConsignment.Setup(m => m.ReferenceNumberUCR).Returns("AB45612312345C");

		mockConsignment.Setup(m => m.Carrier).Returns(BuilderHelperTest.SetUpPartyId("DE1185233"));

		mockConsignment.Setup(m => m.Consignor).Returns(consignor);
		mockConsignment.Setup(m => m.Consignee).Returns(consignee);

		var mockSeal1 = BuilderHelperTest.SetUpSeal("1", "F742");
		var mockSeal2 = BuilderHelperTest.SetUpSeal("2", "F742");
		var mockSeals = new ISealCommon[] { mockSeal1.Object, mockSeal2.Object };

		var mockGoodReference1 = SetUpGoodReference("1", "1");
		var mockGoodReference2 = SetUpGoodReference("2", "2");
		var mockGoodReferences = new ICommonGoodsReference[] { mockGoodReference1.Object, mockGoodReference2.Object };

		mockTransportEquipment1 = SetUpTransportEquipment("1", "2", mockSeals, mockGoodReferences);
		var mockTransportEquipment2 = SetUpTransportEquipment("2", "0", Enumerable.Empty<ISealCommon>(), Enumerable.Empty<ICommonGoodsReference>());
		var mockTransportEquipments = new IAESCommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
		mockConsignment.Setup(m => m.TransportEquipment).Returns(mockTransportEquipments);

		mockLocationOfGoods = SetUpLocationOfGoods(contactPerson);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);

		var mockDepartureTransportMean1 = SetUpDepartureTransportMeans("1");
		var mockDepartureTransportMean2 = SetUpDepartureTransportMeans("2");
		var mockDepartureTransportMeans = new ICommonDepartureTransportMeans[] { mockDepartureTransportMean1.Object, mockDepartureTransportMean2.Object };
		mockConsignment.Setup(m => m.DepartureTransportMeans).Returns(mockDepartureTransportMeans);

		var mockCountryOfRouting1 = SetUpCountryOfRouting("1", "CZ");
		var mockCountryOfRouting2 = SetUpCountryOfRouting("2", "DE");
		var mockCountryOfRoutings = new ICommonCountryOfRoutingOfConsignment[] { mockCountryOfRouting1.Object, mockCountryOfRouting2.Object };
		mockConsignment.Setup(m => m.CountryOfRoutingOfConsignments).Returns(mockCountryOfRoutings);

		var mockActiveBorderTransport = BuilderHelperTest.SetUpTransportMediumInfo("30", "DE", "CZ");
		mockConsignment.Setup(m => m.ActiveBorderTransportMeans).Returns(mockActiveBorderTransport.Object);

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "N765", "SO700050");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "N706", "Camion");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };
		mockConsignment.Setup(m => m.TransportDocuments).Returns(mockTransportDocuments);

		mockConsignment.Setup(m => m.TransportChargesMoP).Returns("H");

		return mockConsignment;
	}

	Mock<IAESCommonTransportEquipment> mockTransportEquipment1;
	Mock<IAESCommonLocationOfGoods> mockLocationOfGoods;

	Mock<ICommonCountryOfRoutingOfConsignment> SetUpCountryOfRouting(ZString sequenceNumber, ZString country)
	{
		var mockCountryOfRouting = new Mock<ICommonCountryOfRoutingOfConsignment>();
		mockCountryOfRouting.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockCountryOfRouting.Setup(m => m.CountryOfRouting).Returns(country);

		return mockCountryOfRouting;
	}

	Mock<IDeclarationAESLine> SetUpLine(ZString sequenceNumber, IEnumerable<ICommonAuthorisation> authorisations, IPartyIdProvider consignor, IDeclarationAESConsignee consignee,
		IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> supplyActors, IDeclarationAESCommodityCode commodityCode, IEnumerable<ICommonDangerousGoods> dangerousGoods, IEnumerable<ICommonPackageWithSequenceAndPackNum> packages,
		IEnumerable<IAESCommonDocument> previousDocuments, IEnumerable<IDeclarationAESSupportingDocumentLine> documents, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments,
		IEnumerable<ICommonDocumentSequenceNumber> additionalReferences, IEnumerable<ICommonDocumentSequenceNumber> additionalInfos)
	{
		var mockLine = new Mock<IDeclarationAESLine>();
		mockLine.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockLine.Setup(m => m.StatisticalValue).Returns(1234.5623m);
		mockLine.Setup(m => m.UCRReferenceNumber).Returns("UCR");
		mockLine.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<ICommonAuthorisation>)authorisations);

		mockProcedure = new Mock<IDeclarationAESProcedure>();
		mockProcedure.Setup(m => m.RequestedCPC).Returns("10");
		mockProcedure.Setup(m => m.PreviousCPC).Returns("00");

		mockProcedure.Setup(m => m.AdditionalProcedures).Returns(new ICommonAdditionalCode[] { BuilderHelperTest.SetUpAdditionalCode("1", "124"), BuilderHelperTest.SetUpAdditionalCode("2", "125") });
		mockLine.Setup(m => m.Procedure).Returns(mockProcedure.Object);

		mockLine.Setup(m => m.Consignor).Returns(consignor);
		mockLine.Setup(m => m.Consignee).Returns(consignee);
		mockLine.Setup(m => m.AdditionalSupplyActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)supplyActors);

		var mockOrigin = new Mock<IAESCommonOrigin>();
		mockOrigin.Setup(m => m.CountryOfOrigin).Returns("ES");
		mockOrigin.Setup(m => m.StateOfOrigin).Returns("28");
		mockLine.Setup(m => m.Origin).Returns(mockOrigin.Object);

		mockCommodity = new Mock<IDeclarationAESCommodity>();
		mockCommodity.Setup(m => m.GoodsDescription).Returns("Waterproof footwear DOS");
		mockCommodity.Setup(m => m.CusCode).Returns("0018896-5");

		mockCommodity.Setup(m => m.CommodityCode).Returns(commodityCode);

		mockCommodity.Setup(m => m.DangerousGoods).Returns((IReadOnlyCollection<ICommonDangerousGoods>)dangerousGoods);

		var mockGoodsMeasure = BuilderHelperTest.SetUpCommonGoodsMeasureWithSupUnitsAndSpecified();
		mockCommodity.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure);

		mockLine.Setup(m => m.Commodity).Returns(mockCommodity.Object);

		mockLine.Setup(m => m.InternalPackages).Returns((IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum>)packages);
		mockLine.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IAESCommonDocument>)previousDocuments);
		mockLine.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDeclarationAESSupportingDocumentLine>)documents);
		mockLine.Setup(m => m.TransportDocuments).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
		mockLine.Setup(m => m.AdditionalReferences).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalReferences);
		mockLine.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalInfos);

		return mockLine;
	}

	Mock<IDeclarationAESProcedure> mockProcedure;
	Mock<IDeclarationAESCommodity> mockCommodity;
	Mock<IDeclarationAESCommodityCode> mockCommodityCode;

	Mock<ICommonAdditionalSupplyChainActorSeqNum> SetUpSupplyActor(ZString sequenceNumber, ZString role, ZString id)
	{
		var mockSupplyActor = new Mock<ICommonAdditionalSupplyChainActorSeqNum>();
		mockSupplyActor.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockSupplyActor.Setup(m => m.Role).Returns(role);
		mockSupplyActor.Setup(m => m.Id).Returns(id);

		return mockSupplyActor;
	}

	Mock<IDeclarationAESCommodityCode> SetUpCommodityCode(IEnumerable<ICommonAdditionalCode> tariffAdditionalCodes, IEnumerable<ICommonAdditionalCode> tariffNationalAdditionalCodes)
	{
		var mockCommodityCode = new Mock<IDeclarationAESCommodityCode>();
		mockCommodityCode.Setup(m => m.TariffCode).Returns("640192");
		mockCommodityCode.Setup(m => m.TariffCodeCombined).Returns("10");
		mockCommodityCode.Setup(m => m.TariffAdditionalCodes).Returns((IReadOnlyCollection<ICommonAdditionalCode>)tariffAdditionalCodes);
		mockCommodityCode.Setup(m => m.NationalAdditionalCodes).Returns((IReadOnlyCollection<ICommonAdditionalCode>)tariffNationalAdditionalCodes);

		return mockCommodityCode;
	}

	Mock<ICommonDangerousGoods> SetUpDangerousGoods(ZString sequenceNumber, ZString code)
	{
		var mockDangerousGoods = new Mock<ICommonDangerousGoods>();
		mockDangerousGoods.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDangerousGoods.Setup(m => m.UNDangerousCode).Returns(code);

		return mockDangerousGoods;
	}

	Mock<IDeclarationAESSupportingDocumentLine> SetUpSupportingDocumentLine(ZString sequenceNumber, ZString name, ZString number, ZString issueAuthorityName, DateTime documentDate, ZString line, ZString measurement, ZDecimal quantity, ZBool quantitySpecified)
	{
		var mockDocument = new Mock<IDeclarationAESSupportingDocumentLine>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		var mockCommonDocument = SetUpCommonSupportingDocumentExtraFields(issueAuthorityName, documentDate);
		mockDocument.Setup(m => m.CommonSupportingDocumentExtraFields).Returns(mockCommonDocument.Object);
		mockDocument.Setup(m => m.LineNumber).Returns(line);
		mockDocument.Setup(m => m.Measurement).Returns(measurement);
		mockDocument.Setup(m => m.Quantity).Returns(quantity);
		mockDocument.Setup(m => m.QuantitySpecified).Returns(quantitySpecified);

		return mockDocument;
	}

	Mock<IDeclarationAESSupportingDocumentHeader> SetUpSupportingDocumentHeader(ZString sequenceNumber, ZString name, ZString number, ZString issueAuthorityName, DateTime documentDate, ZString line)
	{
		var mockDocument = new Mock<IDeclarationAESSupportingDocumentHeader>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		var mockCommonDocument = SetUpCommonSupportingDocumentExtraFields(issueAuthorityName, documentDate);
		mockDocument.Setup(m => m.CommonSupportingDocumentExtraFields).Returns(mockCommonDocument.Object);
		mockDocument.Setup(m => m.LineNumber).Returns(line);

		return mockDocument;
	}

	#endregion
}
