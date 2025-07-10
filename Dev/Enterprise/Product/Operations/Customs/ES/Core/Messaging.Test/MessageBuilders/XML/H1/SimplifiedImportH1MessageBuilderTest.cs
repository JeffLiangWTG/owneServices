using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.CCSimplificadaV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(SimplifiedImportH1MessageBuilder))]
sealed class SimplifiedImportH1MessageBuilderTest : H1ImportAbstractMessageBuilderTest<SimplifiedImportH1MessageBuilder, ISimplifiedImportH1MessageDataProvider, CcSimplificadaV1Ent>
{
	#region Tests

	public void TestPopulateImportOperation()
	{
		mockProvider.Setup(m => m.ImportOperation).Returns((IH1ImportOperation)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateAuthorisations()
	{
		mockProvider.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<ICommonAuthorisation>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</ImportOperation>
    <CustomsOfficeOfPresentation>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateCustomsOffices()
	{
		mockProvider.Setup(m => m.CustomsOfficeOfImport).Returns(ZString.Empty);
		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns(ZString.Empty);
		mockProvider.Setup(m => m.SupervisingCustomOffice).Returns(ZString.Empty);

		mockLocationOfGoods.Setup(m => m.LocationCustomOffice).Returns(ZString.Empty);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods.Object);

		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateImporter()
	{
		mockProvider.Setup(m => m.Importer).Returns((IH1PartyProviderWithAddress)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateDeclarant()
	{
		mockProvider.Setup(m => m.Declarant).Returns((IPartyIdProviderWithContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulatePersonPayingCustomsDuty()
	{
		mockProvider.Setup(m => m.PersonPayingCustomsDuty).Returns((IPartyIdProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateRepresentative()
	{
		mockProvider.Setup(m => m.Representative).Returns((ICommonRepresentativeWithContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateCurrency()
	{
		mockProvider.Setup(m => m.Currency).Returns(ZString.Empty);

		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</Representative>
    <GoodsShipment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateGoodsShipment()
	{
		mockProvider.Setup(m => m.GoodsShipment).Returns((ISimplifiedImportH1GoodsShipment)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</CurrencyExchange>
  </CCSimplificada>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateConsigmentTransportEquipment()
	{
		mockConsignment.Setup(m => m.TransportEquipments).Returns((IReadOnlyCollection<ICommonTransportEquipment>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</referenceNumberUCR>
        <LocationOfGoods>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateConsigmentTransportDocuments()
	{
		mockConsignment.Setup(m => m.TransportDocuments).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</LocationOfGoods>
      </Consignment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateContactPerson()
	{
		mockProvider.Setup(m => m.Declarant.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Representative.ContactPerson).Returns((IPartyContactProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationOfGoods()
	{
		mockConsignment.Setup(m => m.LocationOfGoods).Returns((IAESCommonLocationOfGoods)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationGNSS()
	{
		mockConsignment.Setup(m => m.LocationOfGoods.LocationGNSS).Returns((ICommonGNSS)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationEconomicOperator()
	{
		mockConsignment.Setup(m => m.LocationOfGoods.LocationEconomicOperatorId).Returns(ZString.Empty);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationLocationAddress()
	{
		mockConsignment.Setup(m => m.LocationOfGoods.LocationAddress).Returns((IPartyAddressProvider)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLocationLocationPostcodeAddress()
	{
		mockConsignment.Setup(m => m.LocationOfGoods.LocationPostcodeAddress).Returns((ICommonPostcodeAddress)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportSimplifiedActiveH1;

	protected override SimplifiedImportH1MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new SimplifiedImportH1MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override SimplifiedImportH1MessageBuilder CreateMessageBuilderWithNullProvider() => new SimplifiedImportH1MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H1TestFilePath, "TestSimplifiedImportH1Message.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.DataProviderMRN.MRN).Returns("PRLSVNE000006");
		mockProvider.Setup(m => m.Operation).Returns("A");
		mockProvider.Setup(m => m.CustomsOfficeOfImport).Returns("ES000101");
		mockProvider.Setup(m => m.ActivationMode).Returns("Aduanas");

		var mockImportOperation = SetUpImportOperation();
		mockProvider.Setup(m => m.ImportOperation).Returns(mockImportOperation.Object);

		var mockAuthorisation1 = BuilderHelperTest.SetUpCommonAuthorisation("1", "BTI", "Reference1", "A12741852");
		var mockAuthorisation2 = BuilderHelperTest.SetUpCommonAuthorisation("2", "BTI", "Reference2", "A98532678");
		var mockAuthorisations = new ICommonAuthorisation[] { mockAuthorisation1, mockAuthorisation2 };
		mockProvider.Setup(m => m.Authorisations).Returns(mockAuthorisations);

		mockProvider.Setup(m => m.CustomOfficeOfPresentation).Returns("ES009999");
		mockProvider.Setup(m => m.SupervisingCustomOffice).Returns("ES000102");

		var mockImporter = SetUpPartyProviderWithAddress("12345678A", ZString.Empty, "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");
		mockProvider.Setup(m => m.Importer).Returns(mockImporter.Object);

		var mockContactPerson = BuilderHelperTest.SetUpContactInformation("Prometeo", "test_email@taric.es", "616123443");
		var mockDeclarant = BuilderHelperTest.SetUpPartyIdProviderWithContactPerson("ESA78587268", mockContactPerson);
		mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

		var mockPersonPayingCustomsDuty = BuilderHelperTest.SetUpPartyId("12345687A");
		mockProvider.Setup(m => m.PersonPayingCustomsDuty).Returns(mockPersonPayingCustomsDuty);

		var mockRepresentative = BuilderHelperTest.SetUpCommonRepresentativeWithContactPerson("ESA78587268", "2", mockContactPerson);
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative);

		mockProvider.Setup(m => m.Currency).Returns("EUR");

		var mockAdditionalSupplyActor1 = BuilderHelperTest.SetUpSupplyActor("1", "CS", "12345678A");
		var mockAdditionalSupplyActor2 = BuilderHelperTest.SetUpSupplyActor("2", "AS", "12345698A");
		var mockAdditionalSupplyActors = new ICommonAdditionalSupplyChainActorSeqNum[] { mockAdditionalSupplyActor1, mockAdditionalSupplyActor2 };

		var mockExporter = SetUpPartyProviderWithAddress("12345678A", "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");

		var mockPreviousDocument1 = SetUpCommonDocument("1", "N765", "SO700050", "AA");
		var mockPreviousDocument2 = SetUpCommonDocument("2", "N706", "Camion", "BB");
		var mockPreviousDocuments = new IH1CommonDocument[] { mockPreviousDocument1.Object, mockPreviousDocument2.Object };

		var mockSupportingDocument1 = SetUpCommonSupportingDocument("1", "N760", "SO700200", "CC", "Name1", "5", new ZDateTime(2020, 03, 12));
		var mockSupportingDocument2 = SetUpCommonSupportingDocument("2", "N700", "Avion", "DD", "Name2", "9", new ZDateTime(2023, 03, 12));
		var mockSupportingDocuments = new IH1CommonSupportingDocument[] { mockSupportingDocument1.Object, mockSupportingDocument2.Object };

		var mockAdditionalReference1 = SetUpCommonDocument("1", "N761", "SO800050", "EE");
		var mockAdditionalReference2 = SetUpCommonDocument("2", "N701", "Coche", "FF");
		var mockAdditionalReferences = new IH1CommonDocument[] { mockAdditionalReference1.Object, mockAdditionalReference2.Object };

		var mockAdditionalInfo1 = SetUpCommonDocument("1", "N764", "SO700080", "GG");
		var mockAdditionalInfo2 = SetUpCommonDocument("2", "N703", "Barco", "HH");
		var mockAdditionalInfos = new IH1CommonDocument[] { mockAdditionalInfo1.Object, mockAdditionalInfo2.Object };

		var mockAdditionalFiscalReference1 = SetUpAdditionalFiscalReference("1", "AAA", "123456789");
		var mockAdditionalFiscalReference2 = SetUpAdditionalFiscalReference("2", "BBB", "987654321");
		var mockAdditionalFiscalReferences = new IH1AdditionalFiscalReference[] { mockAdditionalFiscalReference1.Object, mockAdditionalFiscalReference2.Object };

		mockLocationOfGoods = SetUpLocationOfGoods();
		mockConsignment = SetUpCommonImportConsignment<ICommonImportH1Consigment>(mockLocationOfGoods.Object);

		var mockLineAuthorisation1 = BuilderHelperTest.SetUpCommonAuthorisation("1", "BAA", "Reference3", "A12345678");
		var mockLineAuthorisation2 = BuilderHelperTest.SetUpCommonAuthorisation("2", "BAA", "Reference4", "A98765431");
		var mockLineAuthorisations = new ICommonAuthorisation[] { mockLineAuthorisation1, mockLineAuthorisation2 };

		var mockProcedure = SetUpProcedure();
		var mockOrigin = SetUpOrigin();
		var mockCommodity = SetUpSimplifiedCommodity();

		var mockPackage1 = BuilderHelperTest.SetUpCommonPackages("1", "VO", "MARCA1", "15");
		var mockPackage2 = BuilderHelperTest.SetUpCommonPackages("2", "CR", "MARCA2", "12");
		var mockPackages = new ICommonPackageWithSequenceAndPackNum[] { mockPackage1, mockPackage2 };

		var mockPreviousDocumentForLine1 = SetUpCommonPreviousDocument("1", "N760", "SO700200", "CC", "BX", "5", "KGM", "WW", 20.10m, "3");
		var mockPreviousDocumentForLine2 = SetUpCommonPreviousDocument("2", "N700", "Avion", "DD", "FR", "9", "HG", "QQ", 45.44m, "5");
		var mockPreviousDocumentsForLine = new IH1CommonPreviousDocument[] { mockPreviousDocumentForLine1.Object, mockPreviousDocumentForLine2.Object };

		var mockSupportingDocumentForLine1 = SetUpCommonLineSupportingDocument("1", "N762", "SO700202", "CC", "Name1", "5", new ZDateTime(2020, 03, 12), "KGM", "OO", 20.10m, "DOL", 77.36m);
		var mockSupportingDocumentForLine2 = SetUpCommonLineSupportingDocument("2", "N702", "Avion2", "DD", "Name2", "9", new ZDateTime(2023, 03, 12), "LT", "LL", 85.25m, "EUR", 745.36m);
		var mockSupportingDocumentsForLine = new IH1CommonLineSupportingDocument[] { mockSupportingDocumentForLine1.Object, mockSupportingDocumentForLine2.Object };

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "N760", "SO700039");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "N705", "Barco");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };

		var mockGoodsShipmentItem1 = SetUpSimplifiedGoodsShipmentItem("1", "FGHF125356486", mockLineAuthorisations, mockProcedure.Object, mockAdditionalSupplyActors, mockExporter.Object, mockOrigin.Object, "GB", mockCommodity.Object,
			mockPackages, mockPreviousDocumentsForLine, mockSupportingDocumentsForLine, mockTransportDocuments, mockAdditionalReferences, mockAdditionalInfos, mockAdditionalFiscalReferences);
		var mockGoodsShipmentItem2 = SetUpSimplifiedGoodsShipmentItem("2", ZString.Empty, Enumerable.Empty<ICommonAuthorisation>(), null, Enumerable.Empty<ICommonAdditionalSupplyChainActorSeqNum>(), null, null, ZString.Empty, null,
			Enumerable.Empty<ICommonPackageWithSequenceAndPackNum>(), Enumerable.Empty<IH1CommonPreviousDocument>(), Enumerable.Empty<IH1CommonLineSupportingDocument>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<IH1CommonDocument>(), Enumerable.Empty<IH1CommonDocument>(), Enumerable.Empty<IH1AdditionalFiscalReference>());
		var mockGoodsShipmentItems = new ISimplifiedImportH1GoodsShipmentItem[] { mockGoodsShipmentItem1.Object, mockGoodsShipmentItem2.Object };

		mockProvider.Setup(m => m.GoodsShipment).Returns(SetUpSimplifiedGoodsShipment("EUR", 0.2m, mockAdditionalSupplyActors, mockExporter.Object, "ES", mockPreviousDocuments, mockSupportingDocuments,
			mockAdditionalReferences, mockAdditionalInfos, mockAdditionalFiscalReferences, mockConsignment.Object, mockGoodsShipmentItems).Object);
	}

	Mock<ICommonLocationOfGoods> mockLocationOfGoods;
	Mock<ICommonImportH1Consigment> mockConsignment;

	Mock<ISimplifiedH1Commodity> SetUpSimplifiedCommodity()
	{
		var mockCommodity = SetUpCompleteAndSimplifiedCommonCommodity<ISimplifiedH1Commodity>();
		mockCommodity.Setup(m => m.Preference).Returns("111");

		return mockCommodity;
	}

	Mock<ISimplifiedImportH1GoodsShipmentItem> SetUpSimplifiedGoodsShipmentItem(ZString declarationGoodsItemNumber, ZString referenceNumberUCR, IEnumerable<ICommonAuthorisation> authorisations, IH1Procedure procedure, IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> additionalSupplyChainActors,
		IH1PartyProviderWithAddress exporter, IH1Origin origin, ZString countryOfDispatch, ISimplifiedH1Commodity commodity, IEnumerable<ICommonPackageWithSequenceAndPackNum> packages, IEnumerable<IH1CommonPreviousDocument> previousDocuments,
		IEnumerable<IH1CommonLineSupportingDocument> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments, IEnumerable<IH1CommonDocument> additionalReferences, IEnumerable<IH1CommonDocument> additionalInformations,
		IEnumerable<IH1AdditionalFiscalReference> additionalFiscalReferences)
	{
		var mockGoodsShipmentItem = SetUpCompleteAndSimplifiedCommonGoodsShipmentItem<ISimplifiedImportH1GoodsShipmentItem>(declarationGoodsItemNumber, referenceNumberUCR, authorisations, procedure, additionalSupplyChainActors, exporter, origin, countryOfDispatch,
			packages, previousDocuments, supportingDocuments, transportDocuments, additionalReferences, additionalInformations, additionalFiscalReferences);

		mockGoodsShipmentItem.Setup(m => m.Commodity).Returns(commodity);
		return mockGoodsShipmentItem;
	}

	Mock<ISimplifiedImportH1GoodsShipment> SetUpSimplifiedGoodsShipment(ZString invoiceCurrency, ZDecimal exchangeRate, IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> additionalSupplyChainActors, IH1PartyProviderWithAddress exporter, ZString countryOfDispatch,
		IEnumerable<IH1CommonDocument> previousDocuments, IEnumerable<IH1CommonSupportingDocument> supportingDocuments, IEnumerable<IH1CommonDocument> additionalReferences,
		IEnumerable<IH1CommonDocument> additionalInformations, IEnumerable<IH1AdditionalFiscalReference> additionalFiscalReferences, ICommonImportH1Consigment consignment, IEnumerable<ISimplifiedImportH1GoodsShipmentItem> goodsShipmentItems)
	{
		var mockGoodsShipment = SetUpCommonGoodsShipment<ISimplifiedImportH1GoodsShipment>(invoiceCurrency, exchangeRate, additionalSupplyChainActors, exporter, countryOfDispatch, previousDocuments, supportingDocuments, additionalReferences, additionalInformations, additionalFiscalReferences);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(consignment);
		mockGoodsShipment.Setup(m => m.GoodsShipmentItems).Returns((IReadOnlyCollection<ISimplifiedImportH1GoodsShipmentItem>)goodsShipmentItems);

		return mockGoodsShipment;
	}

	#endregion
}
