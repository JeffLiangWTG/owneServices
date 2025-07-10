using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Moq;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing;

public abstract class NCTSCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
	where TProvider : class, INCTSCommonDataProvider
	where TMessageBuilder : NCTSCommonMessageBuilder<TProvider, T>
{
	protected sealed override ZString GetSignedMessageTestFileContent() => GetTestFile();
	protected sealed override ZString GetUnsignedMessageTestFileContent() => GetTestFile();
	protected abstract ZString GetTestFile();
	protected sealed override TestFileReader TestFileReader => testFileReader ?? (testFileReader = new TestFileReader(typeof(NCTSCommonMessageBuilderTest<,,>)));
	TestFileReader testFileReader;

	#region Structures SetUp
	protected Mock<INCTSCommonTransitOperationMRN> SetUpTransitOperationMRN()
	{
		var mockTransitOperation = new Mock<INCTSCommonTransitOperationMRN>();
		mockTransitOperation.Setup(m => m.MRN).Returns("22ES000101100909B7");
		return mockTransitOperation;
	}

	protected Mock<INCTSCommonTransitOperationLRN> SetUpTransitOperationLRN()
	{
		var mockTransitOperation = new Mock<INCTSCommonTransitOperationLRN>();
		mockTransitOperation.Setup(m => m.LRN).Returns("20220804120632");
		return mockTransitOperation;
	}

	protected Mock<INCTSCommonTransitOperation> SetUpTransitOperationCommon()
	{
		var mockTransitOperation = new Mock<INCTSCommonTransitOperation>();
		mockTransitOperation.Setup(m => m.DeclarationType).Returns("T1");
		mockTransitOperation.Setup(m => m.TIRCarnetNumber).Returns("12345678");
		mockTransitOperation.Setup(m => m.Security).Returns("0");
		return mockTransitOperation;
	}

	protected Mock<INCTSCommonCompleteTransitOperation> SetUpTransitOperationCommonComplete()
	{
		var mockTransitOperation = new Mock<INCTSCommonCompleteTransitOperation>();
		mockTransitOperation.Setup(m => m.DeclarationType).Returns("T1");
		mockTransitOperation.Setup(m => m.AdditionalDeclarationType).Returns("A");
		mockTransitOperation.Setup(m => m.TIRCarnetNumber).Returns("12345678");
		mockTransitOperation.Setup(m => m.Security).Returns("0");
		mockTransitOperation.Setup(m => m.ReducedDatasetIndicator).Returns(ZBool.False);
		mockTransitOperation.Setup(m => m.SpecificCircumstanceIndicator).Returns("A20");
		return mockTransitOperation;
	}

	protected Mock<INCTSCommonAuthorisation> SetUpCommonAuthorisation(ZString sequenceNumber, ZString type, ZString reference)
	{
		var mockAuthorisation = new Mock<INCTSCommonAuthorisation>();
		mockAuthorisation.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockAuthorisation.Setup(m => m.Type).Returns(type);
		mockAuthorisation.Setup(m => m.ReferenceNumber).Returns(reference);
		return mockAuthorisation;
	}

	protected Mock<INCTSCommonCustomsOffice> SetUpCommonCustomsOffice(ZString sequenceNumber, ZString reference)
	{
		var mockCustomsOffice = new Mock<INCTSCommonCustomsOffice>();
		mockCustomsOffice.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockCustomsOffice.Setup(m => m.ReferenceNumber).Returns(reference);
		return mockCustomsOffice;
	}

	protected Mock<INCTSCommonHolderOfTheTransitProcedure> SetUpCommonHolderOfTheTransitProcedure()
	{
		var mockHolderOfTheTransitProcedure = new Mock<INCTSCommonHolderOfTheTransitProcedure>();
		mockHolderOfTheTransitProcedure.Setup(m => m.Id).Returns("ES89890001K");
		mockHolderOfTheTransitProcedure.Setup(m => m.TIRHolderIdentificationNumber).Returns("A12345678");
		return mockHolderOfTheTransitProcedure;
	}

	protected Mock<INCTSCommonHolderOfTheTransitProcedureWithAddress> SetUpHolderOfTheTransitProcedureWithAddress()
	{
		var mockHolderOfTheTransitProcedure = new Mock<INCTSCommonHolderOfTheTransitProcedureWithAddress>();
		mockHolderOfTheTransitProcedure.Setup(m => m.Id).Returns("ES89890001K");
		mockHolderOfTheTransitProcedure.Setup(m => m.TIRHolderIdentificationNumber).Returns("A12345678");
		mockHolderOfTheTransitProcedure.Setup(m => m.Address).Returns(SetUpAddressInformation().Object);
		return mockHolderOfTheTransitProcedure;
	}

	protected Mock<INCTSCompleteHolderOfTheTransitProcedure> SetUpHolderOfTheTransitProcedure()
	{
		var mockHolderOfTheTransitProcedure = new Mock<INCTSCompleteHolderOfTheTransitProcedure>();
		mockHolderOfTheTransitProcedure.Setup(m => m.Id).Returns("ES89890001K");
		mockHolderOfTheTransitProcedure.Setup(m => m.TIRHolderIdentificationNumber).Returns("A12345678");
		mockHolderOfTheTransitProcedure.Setup(m => m.Address).Returns(SetUpAddressInformation().Object);
		mockHolderOfTheTransitProcedure.Setup(m => m.ContactPerson).Returns(SetUpContactInformation().Object);
		return mockHolderOfTheTransitProcedure;
	}

	protected Mock<ICommonRepresentativeWithContactPerson> SetUpRepresentative()
	{
		var mockRepresentative = new Mock<ICommonRepresentativeWithContactPerson>();
		mockRepresentative.Setup(m => m.Id).Returns("ESA78587268");
		mockRepresentative.Setup(m => m.Status).Returns("2");
		mockRepresentative.Setup(m => m.ContactPerson).Returns(SetUpContactInformation().Object);
		return mockRepresentative;
	}

	protected Mock<INCTSCommonAddress> SetUpAddress()
	{
		var mockContactInformation = new Mock<INCTSCommonAddress>();
		mockContactInformation.Setup(m => m.StreetAndNumber).Returns("Boix y Morer");
		mockContactInformation.Setup(m => m.City).Returns("Madrid");
		mockContactInformation.Setup(m => m.PostCode).Returns("28003");
		return mockContactInformation;
	}

	protected Mock<INCTSCommonAddressInfo> SetUpAddressInformation()
	{
		var mockContactInformation = new Mock<INCTSCommonAddressInfo>();
		mockContactInformation.Setup(m => m.StreetAndNumber).Returns("Boix y Morer");
		mockContactInformation.Setup(m => m.City).Returns("Madrid");
		mockContactInformation.Setup(m => m.PostCode).Returns("28003");
		mockContactInformation.Setup(m => m.Country).Returns("ES");
		return mockContactInformation;
	}

	protected Mock<IPartyContactProvider> SetUpContactInformation()
	{
		var mockContactInformation = new Mock<IPartyContactProvider>();
		mockContactInformation.Setup(m => m.Name).Returns("Prometeo");
		mockContactInformation.Setup(m => m.Email).Returns("test_email@taric.es");
		mockContactInformation.Setup(m => m.PhoneNumber).Returns("616123443");
		return mockContactInformation;
	}

	protected Mock<INCTSCommonTransportEquipment> SetUpTransportEquipment(ZString sequenceNumber, ZString container, ZString totalOfSeals, IEnumerable<ISealCommon> seals, IEnumerable<INCTSCommonGoodsReference> goodsReferences)
	{
		var mockTransportEquipment = new Mock<INCTSCommonTransportEquipment>();
		mockTransportEquipment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockTransportEquipment.Setup(m => m.ContainerIdentificationNumber).Returns(container);
		mockTransportEquipment.Setup(m => m.NumberOfSeals).Returns(totalOfSeals);
		mockTransportEquipment.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)seals);
		mockTransportEquipment.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<INCTSCommonGoodsReference>)goodsReferences);

		return mockTransportEquipment;
	}

	protected ISealCommon[] SetUpSeals()
	{
		var mockSeal1 = BuilderHelperTest.SetUpSeal("1", "F742");
		var mockSeal2 = BuilderHelperTest.SetUpSeal("2", "F743");
		return new ISealCommon[] { mockSeal1.Object, mockSeal2.Object };
	}

	protected Mock<INCTSCommonGoodsReference> SetUpGoodReference(ZString sequenceNumber, ZString itemNumber)
	{
		var mockGoodReference = new Mock<INCTSCommonGoodsReference>();
		mockGoodReference.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockGoodReference.Setup(m => m.DeclarationGoodsItemNumber).Returns(itemNumber);

		return mockGoodReference;
	}

	protected INCTSCommonGoodsReference[] SetUpGoodsReference()
	{
		var mockGoodReference1 = SetUpGoodReference("1", "2");
		var mockGoodReference2 = SetUpGoodReference("2", "6");
		return new INCTSCommonGoodsReference[] { mockGoodReference1.Object, mockGoodReference2.Object };
	}

	protected Mock<INCTSCommonLocationOfGoods> SetUpLocationOfGoods()
	{
		var mockLocationOfGoods = new Mock<INCTSCommonLocationOfGoods>();
		mockLocationOfGoods.Setup(m => m.TypeOfLocation).Returns("B");
		mockLocationOfGoods.Setup(m => m.QualifierOfIdentification).Returns("Y");
		mockLocationOfGoods.Setup(m => m.AuthorisationNumber).Returns("010101GENE");

		return mockLocationOfGoods;
	}

	protected Mock<ICommonDepartureTransportMeans> SetUpDepartureTransportMeans(ZString sequenceNumber, ZString type, ZString identification, ZString nationality)
	{
		var mockDepartureTransportMeans = new Mock<ICommonDepartureTransportMeans>();
		mockDepartureTransportMeans.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDepartureTransportMeans.Setup(m => m.TransportMode).Returns(type);
		mockDepartureTransportMeans.Setup(m => m.TransportId).Returns(identification);
		mockDepartureTransportMeans.Setup(m => m.TransportNationality).Returns(nationality);

		return mockDepartureTransportMeans;
	}

	protected Mock<INCTSCommonActiveBorderTransportMeans> SetUpActiveBorderTransportMeans(ZString sequenceNumber, ZString type, ZString identification, ZString nationality, ZString conveyance)
	{
		var mockActiveBorderTransportMeans = new Mock<INCTSCommonActiveBorderTransportMeans>();
		mockActiveBorderTransportMeans.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockActiveBorderTransportMeans.Setup(m => m.TransportMode).Returns(type);
		mockActiveBorderTransportMeans.Setup(m => m.TransportId).Returns(identification);
		mockActiveBorderTransportMeans.Setup(m => m.TransportNationality).Returns(nationality);
		mockActiveBorderTransportMeans.Setup(m => m.ConveyanceReferenceNumber).Returns(conveyance);

		return mockActiveBorderTransportMeans;
	}

	protected Mock<INCTSCommonActiveBorderTransportMeansWithOffice> SetUpActiveBorderTransportMeansWithOffice(ZString sequenceNumber, ZString customsOfficeAtBorder, ZString type, ZString identification, ZString nationality, ZString conveyance)
	{
		var mockActiveBorderTransportMeans = new Mock<INCTSCommonActiveBorderTransportMeansWithOffice>();
		mockActiveBorderTransportMeans.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockActiveBorderTransportMeans.Setup(m => m.CustomsOfficeAtBorderReferenceNumber).Returns(customsOfficeAtBorder);
		mockActiveBorderTransportMeans.Setup(m => m.TransportMode).Returns(type);
		mockActiveBorderTransportMeans.Setup(m => m.TransportId).Returns(identification);
		mockActiveBorderTransportMeans.Setup(m => m.TransportNationality).Returns(nationality);
		mockActiveBorderTransportMeans.Setup(m => m.ConveyanceReferenceNumber).Returns(conveyance);

		return mockActiveBorderTransportMeans;
	}

	protected Mock<INCTSCommonPlace> SetUpCommonPlace(ZString code, ZString country, ZString location)
	{
		var mockPlace = new Mock<INCTSCommonPlace>();
		mockPlace.Setup(m => m.UNLocode).Returns(code);
		mockPlace.Setup(m => m.Country).Returns(country);
		mockPlace.Setup(m => m.Location).Returns(location);

		return mockPlace;
	}

	protected Mock<INCTSCommonHouseConsignmentSeqNum> SetUpCommonHouseConsignment(ZString sequenceNumber)
	{
		var mockHouseConsignment = new Mock<INCTSCommonHouseConsignmentSeqNum>();
		mockHouseConsignment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);

		return mockHouseConsignment;
	}

	protected Mock<INCTSCommonHouseConsignmentDepartureAndAmendment> SetUpCommonHouseConsignmentDepartureAndAmendment(ZString sequenceNumber, ZDecimal grossMass, ZString countryOfDispatch,
		ZString countryOfDestination, ZString reference, IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> actors, IEnumerable<INCTSConsignmentItemDepartureAndAmendment> lines,
		IEnumerable<ICommonDepartureTransportMeans> transportMeans, IEnumerable<INCTSCommonDocumentWithInfo> previousDocuments, IEnumerable<INCTSCommonDocumentWithItem> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments,
		IEnumerable<ICommonDocumentSequenceNumber> additionalRef, IEnumerable<ICommonDocumentSequenceNumber> additionalInfo,
		INCTSCommonConsignor consignor = null, INCTSPartyNameProviderWithAddress consignee = null)
	{
		var mockHouseConsignment = new Mock<INCTSCommonHouseConsignmentDepartureAndAmendment>();
		mockHouseConsignment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockHouseConsignment.Setup(m => m.GrossMass).Returns(grossMass);
		mockHouseConsignment.Setup(m => m.CountryOfDispatch).Returns(countryOfDispatch);
		mockHouseConsignment.Setup(m => m.CountryOfDestination).Returns(countryOfDestination);
		mockHouseConsignment.Setup(m => m.ReferenceNumberUCR).Returns(reference);
		mockHouseConsignment.Setup(m => m.Consignor).Returns(consignor);
		mockHouseConsignment.Setup(m => m.Consignee).Returns(consignee);
		mockHouseConsignment.Setup(m => m.AdditionalSupplyChainActor).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)actors);
		mockHouseConsignment.Setup(m => m.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)transportMeans);
		mockHouseConsignment.Setup(m => m.PreviousDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)previousDocuments);
		mockHouseConsignment.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithItem>)supportingDocuments);
		mockHouseConsignment.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
		mockHouseConsignment.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalRef);
		mockHouseConsignment.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalInfo);
		mockHouseConsignment.Setup(m => m.ConsignmentItem).Returns((IReadOnlyCollection<INCTSConsignmentItemDepartureAndAmendment>)lines);
		return mockHouseConsignment;
	}

	protected Mock<INCTSCommonGuarantee> SetUpGuarantee(ZString sequenceNumber, ZString type, IEnumerable<INCTSCommonGuaranteeReference> references)
	{
		var mockGuarantee = new Mock<INCTSCommonGuarantee>();
		mockGuarantee.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockGuarantee.Setup(m => m.GuaranteeType).Returns(type);
		mockGuarantee.Setup(m => m.GuaranteeReference).Returns((IReadOnlyCollection<INCTSCommonGuaranteeReference>)references);
		return mockGuarantee;
	}

	protected Mock<INCTSCommonGuaranteeReference> SetUpGuaranteeReference(ZString sequenceNumber, ZString grn, ZString code, ZDecimal amount)
	{
		var mockGuaranteeReference = new Mock<INCTSCommonGuaranteeReference>();
		mockGuaranteeReference.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockGuaranteeReference.Setup(m => m.GRN).Returns(grn);
		mockGuaranteeReference.Setup(m => m.AccessCode).Returns(code);
		mockGuaranteeReference.Setup(m => m.AmountToBeCovered).Returns(amount);
		return mockGuaranteeReference;
	}

	protected Mock<INCTSCommonCarrier> SetUpCommonCarrier()
	{
		var mockCarrier = new Mock<INCTSCommonCarrier>();
		mockCarrier.Setup(m => m.Id).Returns("ES89890001K");
		mockCarrier.Setup(m => m.ContactPerson).Returns(SetUpContactInformation().Object);
		return mockCarrier;
	}

	protected Mock<INCTSCommonConsignor> SetUpCommonConsignor()
	{
		var mockConsignor = new Mock<INCTSCommonConsignor>();
		mockConsignor.Setup(m => m.Id).Returns("ES89890001K");
		mockConsignor.Setup(m => m.Name).Returns("Consignor");
		mockConsignor.Setup(m => m.Address).Returns(SetUpAddressInformation().Object);
		mockConsignor.Setup(m => m.ContactPerson).Returns(SetUpContactInformation().Object);
		return mockConsignor;
	}

	protected Mock<INCTSPartyNameProviderWithAddress> SetUpNCTSPartyNameProviderWithAddress(string id = "ES89890001K", string name = "Consignee")
	{
		var mockConsignee = new Mock<INCTSPartyNameProviderWithAddress>();
		mockConsignee.Setup(m => m.Id).Returns(id);
		mockConsignee.Setup(m => m.Name).Returns(name);
		mockConsignee.Setup(m => m.Address).Returns(SetUpAddressInformation().Object);
		return mockConsignee;
	}

	protected Mock<ICommonAdditionalSupplyChainActorSeqNum> SetUpCommonAdditionalSupplyChainActor(ZString sequenceNumber, ZString role, ZString id)
	{
		var mockActor = new Mock<ICommonAdditionalSupplyChainActorSeqNum>();
		mockActor.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockActor.Setup(m => m.Role).Returns(role);
		mockActor.Setup(m => m.Id).Returns(id);
		return mockActor;
	}

	protected Mock<ICommonCountryOfRoutingOfConsignment> SetUpCommonCountryOfRouting(ZString sequenceNumber, ZString country)
	{
		var mockCountryOfRouting = new Mock<ICommonCountryOfRoutingOfConsignment>();
		mockCountryOfRouting.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockCountryOfRouting.Setup(m => m.CountryOfRouting).Returns(country);
		return mockCountryOfRouting;
	}

	protected Mock<INCTSConsignmentItemDepartureAndAmendment> SetUpCommonLine(ZString lineNumber, ZString goodsItemNumber, ZString declarationType, ZString countryOfDispatch, ZString countryOfDestination, ZString reference,
		IEnumerable<ICommonAdditionalSupplyChainActorSeqNum> actors, IEnumerable<INCTSCommonPackaging> packages, IEnumerable<INCTSCommonPreviousDocument> previousDocuments,
		IEnumerable<ICommonDocumentSequenceNumber> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments, IEnumerable<ICommonDocumentSequenceNumber> additionalRef,
		IEnumerable<ICommonDocumentSequenceNumber> additionalInfo, INCTSPartyNameProviderWithAddress consignee = null, INCTSCommodityDepartureAndAmendment commodity = null)
	{
		var mockLine = new Mock<INCTSConsignmentItemDepartureAndAmendment>();
		mockLine.Setup(m => m.GoodsItemNumber).Returns(lineNumber);
		mockLine.Setup(m => m.DeclarationGoodsItemNumber).Returns(goodsItemNumber);
		mockLine.Setup(m => m.DeclarationType).Returns(declarationType);
		mockLine.Setup(m => m.CountryOfDispatch).Returns(countryOfDispatch);
		mockLine.Setup(m => m.CountryOfDestination).Returns(countryOfDestination);
		mockLine.Setup(m => m.ReferenceNumberUCR).Returns(reference);
		mockLine.Setup(m => m.Consignee).Returns(consignee);
		mockLine.Setup(m => m.AdditionalSupplyChainActor).Returns((IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum>)actors);
		mockLine.Setup(m => m.Commodity).Returns(commodity);
		mockLine.Setup(m => m.Packaging).Returns((IReadOnlyCollection<INCTSCommonPackaging>)packages);
		mockLine.Setup(m => m.PreviousDocument).Returns((IReadOnlyCollection<INCTSCommonPreviousDocument>)previousDocuments);
		mockLine.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)supportingDocuments);
		mockLine.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
		mockLine.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalRef);
		mockLine.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalInfo);
		return mockLine;
	}

	protected Mock<INCTSCommodityDepartureAndAmendment> SetUpCommodity()
	{
		var mockCommodity = new Mock<INCTSCommodityDepartureAndAmendment>();
		mockCommodity.Setup(m => m.DescriptionOfGoods).Returns("Description1");
		mockCommodity.Setup(m => m.CusCode).Returns("CusCode");
		mockCommodity.Setup(m => m.CommodityCode).Returns(SetUpCommonCommodityCode().Object);

		var mockDangerousGoods1 = SetUpCommonDangerousGoods("1", "0004");
		var mockDangerousGoods2 = SetUpCommonDangerousGoods("2", "0002");
		var mockDangerousGoods = new ICommonDangerousGoods[] { mockDangerousGoods1.Object, mockDangerousGoods2.Object };
		mockCommodity.Setup(m => m.DangerousGoods).Returns(mockDangerousGoods);

		var mockGoodsMeasure = new Mock<INCTSGoodsMeasureDepartureAndAmendment>();
		mockGoodsMeasure.Setup(m => m.GrossMass).Returns(30.102m);
		mockGoodsMeasure.Setup(m => m.GrossMassSpecified).Returns(true);
		mockGoodsMeasure.Setup(m => m.NetMass).Returns(19.000m);
		mockGoodsMeasure.Setup(m => m.NetMassSpecified).Returns(true);
		mockGoodsMeasure.Setup(m => m.SupplementaryUnits).Returns(1.250m);
		mockGoodsMeasure.Setup(m => m.SupplementaryUnitsSpecified).Returns(true);
		mockCommodity.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure.Object);

		return mockCommodity;
	}

	protected Mock<INCTSCommonCommodityCode> SetUpCommonCommodityCode()
	{
		var mockCommodityCode = new Mock<INCTSCommonCommodityCode>();
		mockCommodityCode.Setup(m => m.HarmonizedSystemSubHeadingCode).Returns("440712");
		mockCommodityCode.Setup(m => m.CombinedNomenclatureCode).Returns("20");
		return mockCommodityCode;
	}

	protected Mock<ICommonDangerousGoods> SetUpCommonDangerousGoods(ZString sequenceNumber, ZString code)
	{
		var mockDangerousGoods = new Mock<ICommonDangerousGoods>();
		mockDangerousGoods.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDangerousGoods.Setup(m => m.UNDangerousCode).Returns(code);
		return mockDangerousGoods;
	}

	protected Mock<INCTSCommonPackaging> SetUpCommonPackages(ZString sequenceNumber, ZString type, ZString number, ZString mark)
	{
		var mockPackage = new Mock<INCTSCommonPackaging>();
		mockPackage.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockPackage.Setup(m => m.PackageType).Returns(type);
		mockPackage.Setup(m => m.NumberOfPackages).Returns(number);
		mockPackage.Setup(m => m.Marks).Returns(mark);
		return mockPackage;
	}

	protected Mock<INCTSCommonPreviousDocument> SetUpCommonPreviousDocument(ZString sequenceNumber, ZString type, ZString number, ZString goodItemNumber, ZString unit, ZDecimal quantity, ZBool quantitySpecified, ZString info)
	{
		var mockPreviousDoc = new Mock<INCTSCommonPreviousDocument>();
		mockPreviousDoc.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockPreviousDoc.Setup(m => m.Name).Returns(type);
		mockPreviousDoc.Setup(m => m.Number).Returns(number);
		mockPreviousDoc.Setup(m => m.GoodsItemNumber).Returns(goodItemNumber);
		mockPreviousDoc.Setup(m => m.MeasurementUnitAndQualifier).Returns(unit);
		mockPreviousDoc.Setup(m => m.Quantity).Returns(quantity);
		mockPreviousDoc.Setup(m => m.QuantitySpecified).Returns(quantitySpecified);
		mockPreviousDoc.Setup(m => m.ComplementaryInformation).Returns(info);
		return mockPreviousDoc;
	}

	protected Mock<ICommonDocumentSequenceNumber> SetUpCommonDocument(ZString sequenceNumber, ZString type, ZString number)
	{
		var mockDoc = new Mock<ICommonDocumentSequenceNumber>();
		mockDoc.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDoc.Setup(m => m.Name).Returns(type);
		mockDoc.Setup(m => m.Number).Returns(number);
		return mockDoc;
	}

	protected Mock<INCTSCommonDocumentWithInfo> SetUpCommonDocumentWithInfo(ZString sequenceNumber, ZString type, ZString number, ZString info)
	{
		var mockDoc = new Mock<INCTSCommonDocumentWithInfo>();
		mockDoc.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDoc.Setup(m => m.Name).Returns(type);
		mockDoc.Setup(m => m.Number).Returns(number);
		mockDoc.Setup(m => m.ComplementaryInformation).Returns(info);
		return mockDoc;
	}

	protected Mock<INCTSCommonDocumentWithItem> SetUpCommonDocumentWithItem(ZString sequenceNumber, ZString type, ZString number, ZString info, ZString item)
	{
		var mockDoc = new Mock<INCTSCommonDocumentWithItem>();
		mockDoc.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDoc.Setup(m => m.Name).Returns(type);
		mockDoc.Setup(m => m.Number).Returns(number);
		mockDoc.Setup(m => m.ComplementaryInformation).Returns(info);
		mockDoc.Setup(m => m.GoodsItemNumber).Returns(item);
		return mockDoc;
	}

	#endregion
}
