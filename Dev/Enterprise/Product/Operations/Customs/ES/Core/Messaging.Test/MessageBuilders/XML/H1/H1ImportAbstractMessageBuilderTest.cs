using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public abstract class H1ImportAbstractMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
	where TProvider : class, IH1ImportCommonDataProvider
	where TMessageBuilder : H1ImportCommonMessageBuilder<TProvider, T>
{
	public override sealed void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When provider is null", () => CreateMessageBuilderWithNullProvider());
	}

	[TestDate(2020, 1, 9, 15, 13, 23, 456)]
	public override sealed void TestCreateEDIMessage()
	{
		var messageBuilder = CreateMessageBuilder();

		CombineAssertions(() =>
		{
			AssertEquals("messageBuilder.MessageType", ExpectedMessageType, messageBuilder.MessageType);
			AssertEquals("messageBuilder.MessageSubType", ExpectedMessageSubType, messageBuilder.MessageSubType);
			AssertEquals("messageBuilder.Provider", mockProvider.Object, messageBuilder.Provider);
			AssertUnsignedMessageText(messageBuilder.UnsignedMessageText);
			AssertSignedMessageText(messageBuilder.GetSignedMessageText());
		});
	}

	protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
	protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
	protected abstract ZString GetTestFile();

	#region Structures Common SetUp

	protected Mock<ImportOperation> SetUpCommonImportOperation<ImportOperation>()
		where ImportOperation : class, IH1CommonImportOperation
	{
		var mockExportOperation = new Mock<ImportOperation>();
		mockExportOperation.Setup(m => m.LRN).Returns("PRLSVNE000020");
		mockExportOperation.Setup(m => m.DeclarationType).Returns("IM");

		return mockExportOperation;
	}

	protected Mock<IH1ImportOperation> SetUpImportOperation()
	{
		var mockExportOperation = SetUpCommonImportOperation<IH1ImportOperation>();
		mockExportOperation.Setup(m => m.AdditionalDeclarationType).Returns("B");
		mockExportOperation.Setup(m => m.LanguageCode).Returns("EN");

		return mockExportOperation;
	}

	protected Mock<IH1PartyProviderWithAddress> SetUpPartyProviderWithAddress(ZString id, ZString name, ZString address, ZString city, ZString postCode, ZString country)
	{
		var partyProvider = new Mock<IH1PartyProviderWithAddress>();
		partyProvider.Setup(m => m.Id).Returns(id);
		partyProvider.Setup(m => m.Name).Returns(name);
		partyProvider.Setup(m => m.Address.Address).Returns(address);
		partyProvider.Setup(m => m.Address.City).Returns(city);
		partyProvider.Setup(m => m.Address.PostCode).Returns(postCode);
		partyProvider.Setup(m => m.Address.Country).Returns(country);
		return partyProvider;
	}

	protected Mock<IH1DeferredPayment> SetUpDeferredPayment(ZString sequenceNumber, ZString payment, ZString ccQualifier)
	{
		var mockDeferredPayment = new Mock<IH1DeferredPayment>();
		mockDeferredPayment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDeferredPayment.Setup(m => m.Payment).Returns(payment);
		mockDeferredPayment.Setup(m => m.CcQualifier).Returns(ccQualifier);

		return mockDeferredPayment;
	}

	protected Mock<IH1Destination> SetUpDestination()
	{
		var mockDestination = new Mock<IH1Destination>();
		mockDestination.Setup(m => m.Country).Returns("FR");
		mockDestination.Setup(m => m.Region).Returns("region");
		mockDestination.Setup(m => m.CcQualifier).Returns("ZZ");

		return mockDestination;
	}

	protected Mock<IH1CommonDocument> SetUpCommonDocument(ZString sequenceNumber, ZString name, ZString number, ZString ccQualifier)
	{
		var mockDocument = new Mock<IH1CommonDocument>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.CcQualifier).Returns(ccQualifier);
		return mockDocument;
	}

	protected Mock<IH1CommonSupportingDocument> SetUpCommonSupportingDocument(ZString sequenceNumber, ZString name, ZString number, ZString ccQualifier, ZString issuingAuthorityName, ZString lineNumber, ZDateTime documentDate)
	{
		var mockDocument = new Mock<IH1CommonSupportingDocument>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.CcQualifier).Returns(ccQualifier);
		mockDocument.Setup(m => m.IssuingAuthorityName).Returns(issuingAuthorityName);
		mockDocument.Setup(m => m.LineNumber).Returns(lineNumber);
		mockDocument.Setup(m => m.DocumentDate).Returns(documentDate);
		return mockDocument;
	}

	protected Mock<IH1CommonPreviousDocument> SetUpCommonPreviousDocument(ZString sequenceNumber, ZString name, ZString number, ZString ccQualifier, ZString typeOfPackages, ZString numberOfPackages,
		ZString measurementUnitAndQualifier, ZString ccQualifierForMeasurementUnitAndQualifier, ZDecimal quantity, ZString goodsItemId)
	{
		var mockDocument = new Mock<IH1CommonPreviousDocument>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.CcQualifier).Returns(ccQualifier);
		mockDocument.Setup(m => m.TypeOfPackages).Returns(typeOfPackages);
		mockDocument.Setup(m => m.NumberOfPackages).Returns(numberOfPackages);
		mockDocument.Setup(m => m.MeasurementUnitAndQualifier).Returns(measurementUnitAndQualifier);
		mockDocument.Setup(m => m.CcQualifierForMeasurementUnitAndQualifier).Returns(ccQualifierForMeasurementUnitAndQualifier);
		mockDocument.Setup(m => m.Quantity).Returns(quantity);
		mockDocument.Setup(m => m.GoodsItemId).Returns(goodsItemId);
		return mockDocument;
	}

	protected Mock<IH1CommonLineSupportingDocument> SetUpCommonLineSupportingDocument(ZString sequenceNumber, ZString name, ZString number, ZString ccQualifier, ZString issuingAuthorityName, ZString lineNumber,
		ZDateTime documentDate, ZString measurementUnitAndQualifier, ZString ccQualifierForMeasurementUnitAndQualifier, ZDecimal quantity, ZString currency, ZDecimal amount)
	{
		var mockDocument = new Mock<IH1CommonLineSupportingDocument>();
		mockDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDocument.Setup(m => m.Name).Returns(name);
		mockDocument.Setup(m => m.Number).Returns(number);
		mockDocument.Setup(m => m.CcQualifier).Returns(ccQualifier);
		mockDocument.Setup(m => m.IssuingAuthorityName).Returns(issuingAuthorityName);
		mockDocument.Setup(m => m.LineNumber).Returns(lineNumber);
		mockDocument.Setup(m => m.DocumentDate).Returns(documentDate);
		mockDocument.Setup(m => m.MeasurementUnitAndQualifier).Returns(measurementUnitAndQualifier);
		mockDocument.Setup(m => m.CcQualifierForMeasurementUnitAndQualifier).Returns(ccQualifierForMeasurementUnitAndQualifier);
		mockDocument.Setup(m => m.Quantity).Returns(quantity);
		mockDocument.Setup(m => m.Currency).Returns(currency);
		mockDocument.Setup(m => m.Amount).Returns(amount);
		return mockDocument;
	}

	protected Mock<IH1AdditionalFiscalReference> SetUpAdditionalFiscalReference(ZString sequenceNumber, ZString role, ZString vat)
	{
		var mockAdditionalFiscalReference = new Mock<IH1AdditionalFiscalReference>();
		mockAdditionalFiscalReference.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockAdditionalFiscalReference.Setup(m => m.Role).Returns(role);
		mockAdditionalFiscalReference.Setup(m => m.VAT).Returns(vat);
		return mockAdditionalFiscalReference;
	}

	protected Mock<Procedure> SetCommonUpProcedure<Procedure>()
		where Procedure : class, ICommonH1Procedure
	{
		var mockProcedure = new Mock<Procedure>();
		mockProcedure.Setup(m => m.RequestedCPC).Returns("40");
		mockProcedure.Setup(m => m.PreviousCPC).Returns("00");

		return mockProcedure;
	}

	protected Mock<IH1Procedure> SetUpProcedure()
	{
		var mockProcedure = SetCommonUpProcedure<IH1Procedure>();

		var mockAddProcedure1 = SetUpH1AdditionalCode("1", "9632", "TT");
		var mockAddProcedure2 = SetUpH1AdditionalCode("2", "7412", "UU");
		var additionalProcedures = new IH1AdditionalCode[] { mockAddProcedure1.Object, mockAddProcedure2.Object };
		mockProcedure.Setup(m => m.AdditionalProcedures).Returns(additionalProcedures);

		return mockProcedure;
	}

	protected Mock<IH1Origin> SetUpOrigin()
	{
		var mockOrigin = new Mock<IH1Origin>();
		mockOrigin.Setup(m => m.Country).Returns("FR");
		mockOrigin.Setup(m => m.PreferentialCountry).Returns("MO");

		return mockOrigin;
	}

	protected Mock<Commodity> SetUpCommonCommodity<Commodity>()
		where Commodity : class, ICommonH1Commodity
	{
		var mockCommodity = new Mock<Commodity>();
		mockCommodity.Setup(m => m.GoodsDescription).Returns("Waterproof footwear DOS");

		return mockCommodity;
	}

	protected Mock<Commodity> SetUpCompleteAndSimplifiedCommonCommodity<Commodity>()
		where Commodity : class, IH1CompleteAndSimplifiedCommonImportCommodity
	{
		var mockCommodity = SetUpCommonCommodity<Commodity>();
		mockCommodity.Setup(m => m.CusCode).Returns("0018896-5");
		mockCommodity.Setup(m => m.QuotaOrderNumber).Returns("123");
		mockCommodity.Setup(m => m.CommodityCode).Returns(SetUpCommodityCode().Object);
		mockCommodity.Setup(m => m.GoodsMeasure).Returns(BuilderHelperTest.SetUpCommonGoodsMeasureWithSupUnitsAndSpecified());
		mockCommodity.Setup(m => m.InvoiceLineAmountInvoiced).Returns(200.111m);

		return mockCommodity;
	}

	protected Mock<IH1AdditionalCode> SetUpH1AdditionalCode(ZString sequenceNumber, ZString code, ZString ccQualifier)
	{
		var mockAdditionalCode = new Mock<IH1AdditionalCode>();
		mockAdditionalCode.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockAdditionalCode.Setup(m => m.Code).Returns(code);
		mockAdditionalCode.Setup(m => m.CcQualifier).Returns(ccQualifier);

		return mockAdditionalCode;
	}

	protected Mock<CommodityCode> SetUpCommonCommodityCode<CommodityCode>()
		where CommodityCode : class, ICommonH1CommodityCode
	{
		var mockCommodityCode = new Mock<CommodityCode>();
		mockCommodityCode.Setup(m => m.TariffCode).Returns("640192");
		mockCommodityCode.Setup(m => m.TariffCodeCombined).Returns("10");
		mockCommodityCode.Setup(m => m.TaricCode).Returns("111");

		return mockCommodityCode;
	}

	protected Mock<IH1CommodityCode> SetUpCommodityCode()
	{
		var mockCommodityCode = SetUpCommonCommodityCode<IH1CommodityCode>();

		var mockTariffAdditionalCode1 = BuilderHelperTest.SetUpAdditionalCode("1", "4321");
		var mockTariffAdditionalCode2 = BuilderHelperTest.SetUpAdditionalCode("2", "5678");
		var mockTariffAdditionalCodes = new ICommonAdditionalCode[] { mockTariffAdditionalCode1, mockTariffAdditionalCode2 };
		mockCommodityCode.Setup(m => m.TariffAdditionalCodes).Returns(mockTariffAdditionalCodes);

		var mockTariffNationalAdditionalCode1 = SetUpH1AdditionalCode("1", "1234", "AA");
		var mockTariffNationalAdditionalCode2 = SetUpH1AdditionalCode("2", "8523", "BB");
		var mockTariffNationalAdditionalCodes = new IH1AdditionalCode[] { mockTariffNationalAdditionalCode1.Object, mockTariffNationalAdditionalCode2.Object };
		mockCommodityCode.Setup(m => m.NationalAdditionalCodes).Returns(mockTariffNationalAdditionalCodes);

		return mockCommodityCode;
	}

	protected Mock<IH1CalculationOfTaxes> SetUpCalculationOfTaxes()
	{
		var mockCalculationOfTaxes = new Mock<IH1CalculationOfTaxes>();
		mockCalculationOfTaxes.Setup(m => m.Preference).Returns("111");
		var mockDutiesAndTaxes1 = SetUpDutiesAndTaxes("1", "A00", "AA", "A");
		var mockDutiesAndTaxes2 = SetUpDutiesAndTaxes("2", "B00", "BB", "J");
		var mockDutiesAndTaxes = new IH1DutiesAndTaxes[] { mockDutiesAndTaxes1.Object, mockDutiesAndTaxes2.Object };
		mockCalculationOfTaxes.Setup(m => m.DutiesAndTaxes).Returns(mockDutiesAndTaxes);

		return mockCalculationOfTaxes;
	}

	protected Mock<IH1DutiesAndTaxes> SetUpDutiesAndTaxes(ZString sequenceNumber, ZString type, ZString ccQualifier, ZString methodOfPayment)
	{
		var mockCalculationOfTaxes = new Mock<IH1DutiesAndTaxes>();
		mockCalculationOfTaxes.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockCalculationOfTaxes.Setup(m => m.Type).Returns(type);
		mockCalculationOfTaxes.Setup(m => m.CcQualifier).Returns(ccQualifier);
		mockCalculationOfTaxes.Setup(m => m.MethodOfPayment).Returns(methodOfPayment);

		var mockTaxBase1 = SetUpTaxBase("1", 0.25m, "KGM", "JJ", 50.5m, 6.025m, 0.123m);
		var mockTaxBase2 = SetUpTaxBase("2", 102m, "HL", "HH", 7.147m, 41.012m, 0.789m);
		var mockTaxBases = new IH1TaxBase[] { mockTaxBase1.Object, mockTaxBase2.Object };
		mockCalculationOfTaxes.Setup(m => m.TaxBases).Returns(mockTaxBases);

		return mockCalculationOfTaxes;
	}

	protected Mock<IH1TaxBase> SetUpTaxBase(ZString sequenceNumber, ZDecimal rate, ZString measurementUnitAndQualifier, ZString ccQualifierForMeasurementUnitAndQualifier, ZDecimal quantity, ZDecimal amount, ZDecimal taxAmount)
	{
		var mockCalculationOfTaxes = new Mock<IH1TaxBase>();
		mockCalculationOfTaxes.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockCalculationOfTaxes.Setup(m => m.Rate).Returns(rate);
		mockCalculationOfTaxes.Setup(m => m.MeasurementUnitAndQualifier).Returns(measurementUnitAndQualifier);
		mockCalculationOfTaxes.Setup(m => m.CcQualifierForMeasurementUnitAndQualifier).Returns(ccQualifierForMeasurementUnitAndQualifier);
		mockCalculationOfTaxes.Setup(m => m.Quantity).Returns(quantity);
		mockCalculationOfTaxes.Setup(m => m.Amount).Returns(amount);
		mockCalculationOfTaxes.Setup(m => m.TaxAmount).Returns(taxAmount);

		return mockCalculationOfTaxes;
	}

	protected Mock<IH1CustomsValuation> SetUpCustomsValuation()
	{
		var mockCustomsValuation = new Mock<IH1CustomsValuation>();
		mockCustomsValuation.Setup(m => m.ValuationMethod).Returns("5");

		var mockAdditionsAndDeductions1 = SetUpAdditionsAndDeductions("1", "AA", -6.025m);
		var mockAdditionsAndDeductions2 = SetUpAdditionsAndDeductions("2", "BB", 41.012m);
		var mockAdditionsAndDeductions = new IH1AdditionsAndDeductions[] { mockAdditionsAndDeductions1.Object, mockAdditionsAndDeductions2.Object };
		mockCustomsValuation.Setup(m => m.AdditionsAndDeductions).Returns(mockAdditionsAndDeductions);

		return mockCustomsValuation;
	}

	protected Mock<IH1AdditionsAndDeductions> SetUpAdditionsAndDeductions(ZString sequenceNumber, ZString code, ZDecimal amount)
	{
		var mockAdditionsAndDeductions = new Mock<IH1AdditionsAndDeductions>();
		mockAdditionsAndDeductions.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockAdditionsAndDeductions.Setup(m => m.Code).Returns(code);
		mockAdditionsAndDeductions.Setup(m => m.Amount).Returns(amount);

		return mockAdditionsAndDeductions;
	}

	protected Mock<ICommonGoodsReference> SetUpGoodReference(ZString sequenceNuber, ZString itemNumber)
	{
		var mockGoodReference = new Mock<ICommonGoodsReference>();
		mockGoodReference.Setup(m => m.SequenceNumber).Returns(sequenceNuber);
		mockGoodReference.Setup(m => m.GoodsItemNumber).Returns(itemNumber);

		return mockGoodReference;
	}

	protected Mock<ICommonTransportEquipment> SetUpTransportEquipment(ZString sequenceNumber, ZString containerNumber, IEnumerable<ICommonGoodsReference> goodsReferences)
	{
		var mockTransportEquipment = new Mock<ICommonTransportEquipment>();
		mockTransportEquipment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockTransportEquipment.Setup(m => m.ContainerNumber).Returns(containerNumber);
		mockTransportEquipment.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<ICommonGoodsReference>)goodsReferences);

		return mockTransportEquipment;
	}

	protected Mock<ICommonLocationOfGoods> SetUpLocationOfGoods()
	{
		var mockLocation = new Mock<ICommonLocationOfGoods>();
		mockLocation.Setup(m => m.LocationType).Returns("B");
		mockLocation.Setup(m => m.LocationQualifier).Returns("Y");
		mockLocation.Setup(m => m.LocationId).Returns("010101GENE");
		mockLocation.Setup(m => m.LocationAdditionalId).Returns("1234");
		mockLocation.Setup(m => m.LocationUNloCode).Returns("Code");
		mockLocation.Setup(m => m.LocationCustomOffice).Returns("ES009999");

		var mockLocationGNSS = new Mock<ICommonGNSS>();
		mockLocationGNSS.Setup(m => m.Latitude).Returns("40°30′N");
		mockLocationGNSS.Setup(m => m.Longitude).Returns("3°40′O");
		mockLocation.Setup(m => m.LocationGNSS).Returns(mockLocationGNSS.Object);

		mockLocation.Setup(m => m.LocationEconomicOperatorId).Returns("12345678A");

		mockLocation.Setup(m => m.LocationAddress).Returns(BuilderHelperTest.SetUpAddress("Street", "Madrid", "28003", "ES"));

		var mockLocationPostCodeAddress = new Mock<ICommonPostcodeAddress>();
		mockLocationPostCodeAddress.Setup(m => m.HouseNumber).Returns("HouseNumber");
		mockLocationPostCodeAddress.Setup(m => m.PostCode).Returns("28003");
		mockLocationPostCodeAddress.Setup(m => m.Country).Returns("ES");
		mockLocation.Setup(m => m.LocationPostcodeAddress).Returns(mockLocationPostCodeAddress.Object);

		return mockLocation;
	}

	protected Mock<Consigment> SetUpCommonImportConsignment<Consigment>(ICommonLocationOfGoods mockLocationOfGoods)
		where Consigment : class, ICommonImportH1Consigment
	{
		var mockConsignment = new Mock<Consigment>();
		mockConsignment.Setup(m => m.IsContainerised).Returns(true);
		mockConsignment.Setup(m => m.GrossMass).Returns(300.1234m);
		mockConsignment.Setup(m => m.ReferenceNumberUCR).Returns("0E15968DFC98");

		var mockGoodReference1 = SetUpGoodReference("1", "1");
		var mockGoodReference2 = SetUpGoodReference("2", "2");
		var mockGoodReferences = new ICommonGoodsReference[] { mockGoodReference1.Object, mockGoodReference2.Object };

		var mockTransportEquipment1 = SetUpTransportEquipment("1", "HXDU1234567", mockGoodReferences);
		var mockTransportEquipment2 = SetUpTransportEquipment("2", "HXDU1234589", Enumerable.Empty<ICommonGoodsReference>());
		var mockTransportEquipments = new ICommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };

		mockConsignment.Setup(m => m.TransportEquipments).Returns(mockTransportEquipments);
		mockConsignment.Setup(m => m.LocationOfGoods).Returns(mockLocationOfGoods);

		var mockTransportDocument1 = BuilderHelperTest.SetUpDocumentSequenceNumber("1", "N714", "SO600080");
		var mockTransportDocument2 = BuilderHelperTest.SetUpDocumentSequenceNumber("2", "N713", "Moto");
		var mockTransportDocuments = new ICommonDocumentSequenceNumber[] { mockTransportDocument1, mockTransportDocument2 };

		mockConsignment.Setup(m => m.TransportDocuments).Returns(mockTransportDocuments);
		return mockConsignment;
	}

	protected Mock<GoodsShipmentItem> SetUpCommonGoodsShipmentItem<GoodsShipmentItem>(ZString declarationGoodsItemNumber)
		where GoodsShipmentItem : class, ICommonH1GoodsShipmentItem
	{
		var mockGoodsShipmentItem = new Mock<GoodsShipmentItem>();
		mockGoodsShipmentItem.Setup(m => m.DeclarationGoodsItemNumber).Returns(declarationGoodsItemNumber);
		return mockGoodsShipmentItem;
	}

	protected Mock<GoodsShipmentItem> SetUpCompleteAndSimplifiedCommonGoodsShipmentItem<GoodsShipmentItem>(ZString declarationGoodsItemNumber, ZString referenceNumberUCR, IEnumerable<ICommonAuthorisation> authorisations, IH1Procedure procedure,
		IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> additionalSupplyChainActors, IH1PartyProviderWithAddress exporter, IH1Origin origin, ZString countryOfDispatch, IEnumerable<ICommonPackageWithSequenceAndPackNum> packages,
		IEnumerable<IH1CommonPreviousDocument> previousDocuments, IEnumerable<IH1CommonLineSupportingDocument> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments, IEnumerable<IH1CommonDocument> additionalReferences,
		IEnumerable<IH1CommonDocument> additionalInformations, IEnumerable<IH1AdditionalFiscalReference> additionalFiscalReferences)
		where GoodsShipmentItem : class, ICompleteAndSimplifiedCommonImportH1GoodsShipmentItem
	{
		var mockGoodsShipmentItem = SetUpCommonGoodsShipmentItem<GoodsShipmentItem>(declarationGoodsItemNumber);
		mockGoodsShipmentItem.Setup(m => m.ReferenceNumberUCR).Returns(referenceNumberUCR);
		mockGoodsShipmentItem.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<ICommonAuthorisation>)authorisations);
		mockGoodsShipmentItem.Setup(m => m.Procedure).Returns(procedure);
		mockGoodsShipmentItem.Setup(m => m.AdditionalSupplyChainActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)additionalSupplyChainActors);
		mockGoodsShipmentItem.Setup(m => m.Exporter).Returns(exporter);
		mockGoodsShipmentItem.Setup(m => m.Origin).Returns(origin);
		mockGoodsShipmentItem.Setup(m => m.CountryOfDispatch).Returns(countryOfDispatch);
		mockGoodsShipmentItem.Setup(m => m.Packages).Returns((IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum>)packages);
		mockGoodsShipmentItem.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IH1CommonPreviousDocument>)previousDocuments);
		mockGoodsShipmentItem.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IH1CommonLineSupportingDocument>)supportingDocuments);
		mockGoodsShipmentItem.Setup(m => m.TransportDocuments).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
		mockGoodsShipmentItem.Setup(m => m.AdditionalReferences).Returns((IReadOnlyCollection<IH1CommonDocument>)additionalReferences);
		mockGoodsShipmentItem.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<IH1CommonDocument>)additionalInformations);
		mockGoodsShipmentItem.Setup(m => m.AdditionalFiscalReferences).Returns((IReadOnlyCollection<IH1AdditionalFiscalReference>)additionalFiscalReferences);
		return mockGoodsShipmentItem;
	}

	protected Mock<GoodsShipment> SetUpCommonGoodsShipment<GoodsShipment>(ZString invoiceCurrency, ZDecimal exchangeRate,
		IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> additionalSupplyChainActors, IH1PartyProviderWithAddress exporter, ZString countryOfDispatch,
		IEnumerable<IH1CommonDocument> previousDocuments, IEnumerable<IH1CommonSupportingDocument> supportingDocuments, IEnumerable<IH1CommonDocument> additionalReferences,
		IEnumerable<IH1CommonDocument> additionalInformations, IEnumerable<IH1AdditionalFiscalReference> additionalFiscalReferences)
		where GoodsShipment : class, ICommonImportH1GoodsShipment
	{
		var mockGoodsShipment = new Mock<GoodsShipment>();
		mockGoodsShipment.Setup(m => m.InvoiceCurrency).Returns(invoiceCurrency);
		mockGoodsShipment.Setup(m => m.ExchangeRate).Returns(exchangeRate);
		mockGoodsShipment.Setup(m => m.AdditionalSupplyChainActors).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)additionalSupplyChainActors);
		mockGoodsShipment.Setup(m => m.Exporter).Returns(exporter);
		mockGoodsShipment.Setup(m => m.CountryOfDispatch).Returns(countryOfDispatch);
		mockGoodsShipment.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IH1CommonDocument>)previousDocuments);
		mockGoodsShipment.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IH1CommonSupportingDocument>)supportingDocuments);
		mockGoodsShipment.Setup(m => m.AdditionalReferences).Returns((IReadOnlyCollection<IH1CommonDocument>)additionalReferences);
		mockGoodsShipment.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<IH1CommonDocument>)additionalInformations);
		mockGoodsShipment.Setup(m => m.AdditionalFiscalReferences).Returns((IReadOnlyCollection<IH1AdditionalFiscalReference>)additionalFiscalReferences);

		return mockGoodsShipment;
	}

	#endregion
}
