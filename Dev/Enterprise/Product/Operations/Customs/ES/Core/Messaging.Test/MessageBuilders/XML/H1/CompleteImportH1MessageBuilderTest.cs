using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.CC415AV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(CompleteImportH1MessageBuilder))]
sealed class CompleteImportH1MessageBuilderTest : H1ImportAbstractMessageBuilderTest<CompleteImportH1MessageBuilder, ICompleteImportH1MessageDataProvider, Cc415Av1Ent>
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

	public void TestPopulatePersonProvidingAGuarantee()
	{
		mockProvider.Setup(m => m.PersonProvidingAGuarantee).Returns((IPartyIdProvider)null);
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

	public void TestPopulateGuarantees()
	{
		mockProvider.Setup(m => m.Guarantees).Returns((IReadOnlyCollection<ICompleteImportH1Guarantee>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</Representative>
    <CurrencyExchange>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateCurrency()
	{
		mockProvider.Setup(m => m.Currency).Returns(ZString.Empty);

		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</Guarantee>
    <DeferredPayment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateDeferredPayments()
	{
		mockProvider.Setup(m => m.DeferredPayments).Returns((IReadOnlyCollection<IH1DeferredPayment>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</CurrencyExchange>
    <GoodsShipment>";

		CombineAssertions(() =>
		{
			AssertContains(expectedResult, messageText);
		});
	}

	public void TestPopulateGoodsShipments()
	{
		mockProvider.Setup(m => m.GoodsShipments).Returns((IReadOnlyCollection<ICompleteImportH1GoodsShipment>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</DeferredPayment>
  </CC415A>";

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

	public void TestPopulateConsigmentArrivalTransportMeans()
	{
		mockConsignment.Setup(m => m.ArrivalTransportMeans).Returns((ICommonArrivalTransportMeans)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

		var messageText = CreateMessageBuilder().GetSignedMessageText();
		var expectedResult = @"</LocationOfGoods>
        <ActiveBorderTransportMeans>";

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
		var expectedResult = @"</ActiveBorderTransportMeans>
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

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportCompleteActiveH1;

	protected override CompleteImportH1MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CompleteImportH1MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override CompleteImportH1MessageBuilder CreateMessageBuilderWithNullProvider() => new CompleteImportH1MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.H1TestFilePath, "TestCompleteImportH1Message.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.DataProviderMRN.MRN).Returns("PRLSVNE000006");
		mockProvider.Setup(m => m.Operation).Returns("A");
		mockProvider.Setup(m => m.CustomsOfficeOfImport).Returns("ES000101");
		mockProvider.Setup(m => m.ActivationMode).Returns("Aduanas");
		mockProvider.Setup(m => m.RecapitulationPeriod).Returns(new ZDate(2021, 11, 03));

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

		var mockPersonProvidingAGuarantee = BuilderHelperTest.SetUpPartyId("21345678A");
		mockProvider.Setup(m => m.PersonProvidingAGuarantee).Returns(mockPersonProvidingAGuarantee);

		var mockPersonPayingCustomsDuty = BuilderHelperTest.SetUpPartyId("12345687A");
		mockProvider.Setup(m => m.PersonPayingCustomsDuty).Returns(mockPersonPayingCustomsDuty);

		var mockRepresentative = BuilderHelperTest.SetUpCommonRepresentativeWithContactPerson("ESA78587268", "2", mockContactPerson);
		mockProvider.Setup(m => m.Representative).Returns(mockRepresentative);

		var mockGuaranteeReference1 = SetUpGuaranteeReference("1", "GRN1234567890", "AA", "0123", "EUR", 55.251m, "AAAA", "ES009999");
		var mockGuaranteeReference2 = SetUpGuaranteeReference("2", "GRN1234567891", "BB", "4567", "DOL", 105.951m, "BBBB", "ES009998");
		var mockGuaranteeReferences = new ICompleteImportH1GuaranteeReference[] { mockGuaranteeReference1.Object, mockGuaranteeReference2.Object };

		var mockGuarantee1 = SetUpGuarantee("1", "6", mockGuaranteeReferences);
		var mockGuarantee2 = SetUpGuarantee("2", "5", Enumerable.Empty<ICompleteImportH1GuaranteeReference>());
		var mockGuarantees = new ICompleteImportH1Guarantee[] { mockGuarantee1.Object, mockGuarantee2.Object };
		mockProvider.Setup(m => m.Guarantees).Returns(mockGuarantees);

		mockProvider.Setup(m => m.Currency).Returns("EUR");

		var mockDeferredPayment1 = SetUpDeferredPayment("1", "Payment", "CC");
		var mockDeferredPayment2 = SetUpDeferredPayment("2", "Payment2", "DD");
		var mockDeferredPayments = new IH1DeferredPayment[] { mockDeferredPayment1.Object, mockDeferredPayment2.Object };
		mockProvider.Setup(m => m.DeferredPayments).Returns(mockDeferredPayments);

		var mockAdditionalSupplyActor1 = BuilderHelperTest.SetUpSupplyActor("1", "CS", "12345678A");
		var mockAdditionalSupplyActor2 = BuilderHelperTest.SetUpSupplyActor("2", "AS", "12345698A");
		var mockAdditionalSupplyActors = new ICommonAdditionalSupplyChainActorSeqNum[] { mockAdditionalSupplyActor1, mockAdditionalSupplyActor2 };

		var mockSeller = SetUpPartyProviderWithAddress("89890001K", "Lobo Lobate", "Lobera de Valdelacierva", "Por poco en Vinuelas", "19069", "ES");
		var mockBuyer = SetUpPartyProviderWithAddress("89890001K", "Ave", "Acostadero eras de abajo", "Fuentelahigera de Albatages", "19024", "ES");
		var mockExporter = SetUpPartyProviderWithAddress("12345678A", "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS");

		var mockDeliveryTerms = BuilderHelperTest.SetUpCommonDeliveryTerms();
		var mockDestination = SetUpDestination();
		var mockWarehouse = BuilderHelperTest.SetUpWarehouseCommon();

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
		mockConsignment = SetUpConsignment();

		var mockLineAuthorisation1 = BuilderHelperTest.SetUpCommonAuthorisation("1", "BAA", "Reference3", "A12345678");
		var mockLineAuthorisation2 = BuilderHelperTest.SetUpCommonAuthorisation("2", "BAA", "Reference4", "A98765431");
		var mockLineAuthorisations = new ICommonAuthorisation[] { mockLineAuthorisation1, mockLineAuthorisation2 };

		var mockProcedure = SetUpProcedure();
		var mockOrigin = SetUpOrigin();
		var mockCommodity = SetUpCommodity();

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

		var mockCustomsValuation = SetUpCustomsValuation();

		var mockGoodsShipmentItem1 = SetUpGoodsShipmentItem("1", "2", 20m, "10", "FGHF125356486", mockLineAuthorisations, null, mockAdditionalSupplyActors, null, mockSeller.Object, null, mockOrigin.Object, "GB", null,
			mockCommodity.Object, Enumerable.Empty<ICommonPackageWithSequenceAndPackNum>(), mockPreviousDocumentsForLine, Enumerable.Empty<IH1CommonLineSupportingDocument>(), mockTransportDocuments, Enumerable.Empty<IH1CommonDocument>(), mockAdditionalInfos, null, "0011", mockAdditionalFiscalReferences);
		var mockGoodsShipmentItem2 = SetUpGoodsShipmentItem("2", "3", 0.55m, "20", "OGHVIP88645", Enumerable.Empty<ICommonAuthorisation>(), mockProcedure.Object, Enumerable.Empty<ICommonAdditionalSupplyChainActorSeqNum>(), mockBuyer.Object, null, mockExporter.Object, null, "DE", mockDestination.Object,
			null, mockPackages, Enumerable.Empty<IH1CommonPreviousDocument>(), mockSupportingDocumentsForLine, Enumerable.Empty<ICommonDocumentSequenceNumber>(), mockAdditionalReferences, Enumerable.Empty<IH1CommonDocument>(), mockCustomsValuation.Object, "0101", Enumerable.Empty<IH1AdditionalFiscalReference>());
		var mockGoodsShipmentItems = new ICompleteImportH1GoodsShipmentItem[] { mockGoodsShipmentItem1.Object, mockGoodsShipmentItem2.Object };

		var mockGoodsShipment1 = SetUpGoodsShipment("1", "91", 20m, "EUR", new ZDateTime(2021, 03, 12), 0.2m, mockAdditionalSupplyActors, null, mockSeller.Object, null, mockDeliveryTerms, "ES", null,
			mockWarehouse, Enumerable.Empty<IH1CommonDocument>(), mockSupportingDocuments, Enumerable.Empty<IH1CommonDocument>(), mockAdditionalInfos, Enumerable.Empty<IH1AdditionalFiscalReference>(), mockConsignment.Object, Enumerable.Empty<ICompleteImportH1GoodsShipmentItem>());
		var mockGoodsShipment2 = SetUpGoodsShipment("2", "92", 30m, "DOL", new ZDateTime(2022, 03, 12), 0.1m, Enumerable.Empty<ICommonAdditionalSupplyChainActorSeqNum>(), mockBuyer.Object, null, mockExporter.Object, null, "FR", mockDestination.Object,
			null, mockPreviousDocuments, Enumerable.Empty<IH1CommonSupportingDocument>(), mockAdditionalReferences, Enumerable.Empty<IH1CommonDocument>(), mockAdditionalFiscalReferences, null, mockGoodsShipmentItems);
		var mockGoodsShipments = new ICompleteImportH1GoodsShipment[] { mockGoodsShipment1.Object, mockGoodsShipment2.Object };
		mockProvider.Setup(m => m.GoodsShipments).Returns(mockGoodsShipments);
	}

	Mock<ICommonLocationOfGoods> mockLocationOfGoods;
	Mock<ICompleteImportH1Consigment> mockConsignment;

	Mock<ICompleteImportH1Guarantee> SetUpGuarantee(ZString sequenceNumber, ZString type, IEnumerable<ICompleteImportH1GuaranteeReference> references)
	{
		var mockGuarantee = new Mock<ICompleteImportH1Guarantee>();
		mockGuarantee.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockGuarantee.Setup(m => m.GuaranteeType).Returns(type);
		mockGuarantee.Setup(m => m.GuaranteeReferences).Returns((IReadOnlyCollection<ICompleteImportH1GuaranteeReference>)references);

		return mockGuarantee;
	}

	Mock<ICompleteImportH1GuaranteeReference> SetUpGuaranteeReference(ZString sequenceNumber, ZString grn, ZString ccQualifier, ZString code, ZString currencyCode, ZDecimal amount, ZString otherGuaranteeReference, ZString customsOfficeOfGuarantee)
	{
		var mockGuaranteeReference = new Mock<ICompleteImportH1GuaranteeReference>();
		mockGuaranteeReference.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockGuaranteeReference.Setup(m => m.GRN).Returns(grn);
		mockGuaranteeReference.Setup(m => m.CcQualifier).Returns(ccQualifier);
		mockGuaranteeReference.Setup(m => m.AccessCode).Returns(code);
		mockGuaranteeReference.Setup(m => m.CurrencyCode).Returns(currencyCode);
		mockGuaranteeReference.Setup(m => m.AmountToBeCovered).Returns(amount);
		mockGuaranteeReference.Setup(m => m.OtherGuaranteeReference).Returns(otherGuaranteeReference);
		mockGuaranteeReference.Setup(m => m.CustomsOfficeOfGuarantee).Returns(customsOfficeOfGuarantee);
		return mockGuaranteeReference;
	}

	Mock<ICompleteImportH1Consigment> SetUpConsignment()
	{
		var mockConsignment = SetUpCommonImportConsignment<ICompleteImportH1Consigment>(mockLocationOfGoods.Object);
		mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("AIR");
		mockConsignment.Setup(m => m.ModeOfTransportAtBorder).Returns("SEA");
		mockConsignment.Setup(m => m.ArrivalTransportMeans).Returns(BuilderHelperTest.SetUpCommonArrivalTransportMeans("20191201ACM40302", "40"));
		mockConsignment.Setup(m => m.ActiveBorderTransportMeansNationality).Returns("ES");

		return mockConsignment;
	}

	Mock<ICompleteImportH1GoodsShipmentItem> SetUpGoodsShipmentItem(ZString sequenceNumber, ZString declarationGoodsItemNumber, ZDecimal statisticalValue, ZString natureOfTransaction, ZString referenceNumberUCR,
		IEnumerable<ICommonAuthorisation> authorisations, IH1Procedure procedure, IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> additionalSupplyChainActors, IH1PartyProviderWithAddress buyer, IH1PartyProviderWithAddress seller,
		IH1PartyProviderWithAddress exporter, IH1Origin origin, ZString countryOfDispatch, IH1Destination destination, ICompleteImportH1Commodity commodity, IEnumerable<ICommonPackageWithSequenceAndPackNum> packages, IEnumerable<IH1CommonPreviousDocument> previousDocuments,
		IEnumerable<IH1CommonLineSupportingDocument> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments, IEnumerable<IH1CommonDocument> additionalReferences, IEnumerable<IH1CommonDocument> additionalInformations,
		IH1CustomsValuation customsValuation, ZString valuationAdjustmentIndicator, IEnumerable<IH1AdditionalFiscalReference> additionalFiscalReferences)
	{
		var mockGoodsShipmentItem = SetUpCompleteAndSimplifiedCommonGoodsShipmentItem<ICompleteImportH1GoodsShipmentItem>(declarationGoodsItemNumber, referenceNumberUCR, authorisations, procedure, additionalSupplyChainActors, exporter, origin, countryOfDispatch,
			packages, previousDocuments, supportingDocuments, transportDocuments, additionalReferences, additionalInformations, additionalFiscalReferences);

		mockGoodsShipmentItem.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockGoodsShipmentItem.Setup(m => m.StatisticalValue).Returns(statisticalValue);
		mockGoodsShipmentItem.Setup(m => m.NatureOfTransaction).Returns(natureOfTransaction);
		mockGoodsShipmentItem.Setup(m => m.Buyer).Returns(buyer);
		mockGoodsShipmentItem.Setup(m => m.Seller).Returns(seller);
		mockGoodsShipmentItem.Setup(m => m.Destination).Returns(destination);
		mockGoodsShipmentItem.Setup(m => m.Commodity).Returns(commodity);
		mockGoodsShipmentItem.Setup(m => m.CustomsValuation).Returns(customsValuation);
		mockGoodsShipmentItem.Setup(m => m.ValuationAdjustmentIndicator).Returns(valuationAdjustmentIndicator);
		return mockGoodsShipmentItem;
	}

	Mock<ICompleteImportH1Commodity> SetUpCommodity()
	{
		var mockCommodity = SetUpCompleteAndSimplifiedCommonCommodity<ICompleteImportH1Commodity>();
		mockCommodity.Setup(m => m.CalculationOfTaxes).Returns(SetUpCalculationOfTaxes().Object);

		return mockCommodity;
	}

	Mock<ICompleteImportH1GoodsShipment> SetUpGoodsShipment(ZString sequenceNumber, ZString natureOfTransaction, ZDecimal totalAmountInvoiced, ZString invoiceCurrency, ZDateTime dateOfAcceptance, ZDecimal exchangeRate,
		IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> additionalSupplyChainActors, IH1PartyProviderWithAddress buyer, IH1PartyProviderWithAddress seller, IH1PartyProviderWithAddress exporter, ICommonDeliveryTerms deliveryTerms, ZString countryOfDispatch,
		IH1Destination destination, IWarehouseCommon warehouse, IEnumerable<IH1CommonDocument> previousDocuments, IEnumerable<IH1CommonSupportingDocument> supportingDocuments, IEnumerable<IH1CommonDocument> additionalReferences,
		IEnumerable<IH1CommonDocument> additionalInformations, IEnumerable<IH1AdditionalFiscalReference> additionalFiscalReferences, ICompleteImportH1Consigment consignment, IEnumerable<ICompleteImportH1GoodsShipmentItem> goodsShipmentItems)
	{
		var mockGoodsShipment = SetUpCommonGoodsShipment<ICompleteImportH1GoodsShipment>(invoiceCurrency, exchangeRate, additionalSupplyChainActors, exporter, countryOfDispatch, previousDocuments, supportingDocuments, additionalReferences, additionalInformations, additionalFiscalReferences);
		mockGoodsShipment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockGoodsShipment.Setup(m => m.NatureOfTransaction).Returns(natureOfTransaction);
		mockGoodsShipment.Setup(m => m.TotalAmountInvoiced).Returns(totalAmountInvoiced);
		mockGoodsShipment.Setup(m => m.DateOfAcceptance).Returns(dateOfAcceptance);
		mockGoodsShipment.Setup(m => m.Buyer).Returns(buyer);
		mockGoodsShipment.Setup(m => m.Seller).Returns(seller);
		mockGoodsShipment.Setup(m => m.DeliveryTerms).Returns(deliveryTerms);
		mockGoodsShipment.Setup(m => m.Destination).Returns(destination);
		mockGoodsShipment.Setup(m => m.Warehouse).Returns(warehouse);
		mockGoodsShipment.Setup(m => m.Consignment).Returns(consignment);
		mockGoodsShipment.Setup(m => m.GoodsShipmentItems).Returns((IReadOnlyCollection<ICompleteImportH1GoodsShipmentItem>)goodsShipmentItems);

		return mockGoodsShipment;
	}

	#endregion
}
