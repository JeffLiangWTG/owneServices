using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

public interface INCTSCommonDataProvider : IESEDIMessageCollectionProvider
{
	ZString MessageSender { get; }
	ZString MessageIdentification { get; }
	ZBool IsFinalPeriod { get; }
	ZBool PhaseIDSpecified { get; }
}

public interface IDepartureAndNotificationNCTSCommonMessageDataProvider : INCTSCommonDataProvider
{
	ZString CustomsOfficeOfDeparture { get; }
	ICommonRepresentativeWithContactPerson Representative { get; }
}

public interface INCTSCommonTransitOperationMRN
{
	ZString MRN { get; }
}

public interface INCTSCommonTransitOperationLRN
{
	ZString LRN { get; }
}

public interface INCTSCommonTransitOperation
{
	ZString DeclarationType { get; }
	ZString TIRCarnetNumber { get; }
	ZString Security { get; }
}

public interface INCTSCommonCompleteTransitOperation : INCTSCommonTransitOperation
{
	ZString AdditionalDeclarationType { get; }
	ZBool ReducedDatasetIndicator { get; }
	ZString SpecificCircumstanceIndicator { get; }
}

public interface INCTSCommonAuthorisation
{
	ZString SequenceNumber { get; }
	ZString Type { get; }
	ZString ReferenceNumber { get; }
}

public interface INCTSCommonCustomsOffice
{
	ZString SequenceNumber { get; }
	ZString ReferenceNumber { get; }
}

public interface INCTSCommonHolderOfTheTransitProcedure : IPartyIdProvider
{
	ZString TIRHolderIdentificationNumber { get; }
}

public interface INCTSCommonHolderOfTheTransitProcedureWithAddress : INCTSCommonHolderOfTheTransitProcedure
{
	INCTSCommonAddressInfo Address { get; }
}

public interface INCTSCompleteHolderOfTheTransitProcedure : INCTSCommonHolderOfTheTransitProcedureWithAddress
{
	IPartyContactProvider ContactPerson { get; }
}

public interface INCTSCommonAddress
{
	ZString StreetAndNumber { get; }
	ZString City { get; }
	ZString PostCode { get; }
}

public interface INCTSCommonAddressInfo : INCTSCommonAddress
{
	ZString Country { get; }
}

public interface INCTSCommonConsignment
{
	IReadOnlyCollection<INCTSCommonTransportEquipment> TransportEquipment { get; }
	IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans { get; }
}

public interface INCTSCommonDepartureConsignment : INCTSCommonConsignment
{
	ZBool ContainerIndicator { get; }
	ZString InlandModeOfTransport { get; }
	ZString ModeOfTransportAtTheBorder { get; }
}

public interface INCTSCommonDepartureAndNotifConsignment : INCTSCommonDepartureConsignment
{
	INCTSCommonLocationOfGoods LocationOfGoods { get; }
	INCTSCommonPlace PlaceOfLoading { get; }
	IReadOnlyCollection<INCTSCommonActiveBorderTransportMeansWithOffice> ActiveBorderTransportMeans { get; }
}

public interface INCTSCommonConsignmentDepartureAndAmendmentAndTNN
{
	ZString CountryOfDispatch { get; }
	ZString CountryOfDestination { get; }
	ZString ReferenceNumberUCR { get; }
	INCTSPartyNameProviderWithAddress Consignee { get; }
	IReadOnlyCollection<ICommonCountryOfRoutingOfConsignment> CountryOfRoutingOfConsignment { get; }
	ZString MethodOfPayment { get; }
	ZDecimal GrossMass { get; }
	IReadOnlyCollection<INCTSCommonDocumentWithItem> SupportingDocument { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReference { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInformation { get; }
}

public interface INCTSCommonConsignmentDepartureAndAmendment : INCTSCommonDepartureAndNotifConsignment
{
	INCTSCommonConsignmentDepartureAndAmendmentAndTNN CommonConsignmentData { get; }
	IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActor { get; }
	INCTSCommonCarrier Carrier { get; }
	INCTSCommonConsignor Consignor { get; }
	INCTSCommonPlace PlaceOfUnloading { get; }
	IReadOnlyCollection<INCTSCommonHouseConsignmentDepartureAndAmendment> HouseConsignment { get; }
}

public interface INCTSCommonTransportEquipment
{
	ZString SequenceNumber { get; }
	ZString ContainerIdentificationNumber { get; }
	ZString NumberOfSeals { get; }
	IReadOnlyCollection<ISealCommon> Seals { get; }
	IReadOnlyCollection<INCTSCommonGoodsReference> GoodsReference { get; }
}

public interface INCTSCommonGoodsReference
{
	ZString SequenceNumber { get; }
	ZString DeclarationGoodsItemNumber { get; }
}

public interface INCTSCommonLocationOfGoods
{
	ZString TypeOfLocation { get; }
	ZString QualifierOfIdentification { get; }
	ZString AuthorisationNumber { get; }
}

public interface INCTSCommonActiveBorderTransportMeansWithOffice : INCTSCommonActiveBorderTransportMeans
{
	ZString CustomsOfficeAtBorderReferenceNumber { get; }
}

public interface INCTSCommonActiveBorderTransportMeans : ICommonDepartureTransportMeans
{
	ZString ConveyanceReferenceNumber { get; }
}

public interface INCTSCommonPlace
{
	ZString UNLocode { get; }
	ZString Country { get; }
	ZString Location { get; }
}

public interface INCTSCommonHouseConsignmentSeqNum
{
	ZString SequenceNumber { get; }
}

public interface INCTSCommonHouseConsignment : INCTSCommonHouseConsignmentSeqNum
{
	ZDecimal GrossMass { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReference { get; }
}

public interface INCTSCommonHouseConsignmentDepartureAndAmendmentAndTNN : INCTSCommonHouseConsignment
{
	ZString ReferenceNumberUCR { get; }
	IReadOnlyCollection<INCTSCommonDocumentWithItem> SupportingDocument { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInformation { get; }
}

public interface INCTSCommonHouseConsignmentDepartureAndAmendment : INCTSCommonHouseConsignmentDepartureAndAmendmentAndTNN
{
	IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActor { get; }
	IReadOnlyCollection<INCTSConsignmentItemDepartureAndAmendment> ConsignmentItem { get; }
	ZString CountryOfDispatch { get; }
	ZString CountryOfDestination { get; }
	INCTSCommonConsignor Consignor { get; }
	INCTSPartyNameProviderWithAddress Consignee { get; }
	IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans { get; }
	IReadOnlyCollection<INCTSCommonDocumentWithInfo> PreviousDocument { get; }
}

public interface INCTSCommonGuarantee
{
	ZString SequenceNumber { get; }
	ZString GuaranteeType { get; }
	IReadOnlyCollection<INCTSCommonGuaranteeReference> GuaranteeReference { get; }
}

public interface INCTSCommonGuaranteeReference
{
	ZString SequenceNumber { get; }
	ZString GRN { get; }
	ZString AccessCode { get; }
	ZDecimal AmountToBeCovered { get; }
}

public interface INCTSCommonCarrier : IPartyIdProvider
{
	IPartyContactProvider ContactPerson { get; }
}

public interface INCTSCommonConsignor : IPartyNameProvider
{
	INCTSCommonAddressInfo Address { get; }
	IPartyContactProvider ContactPerson { get; }
}

public interface INCTSPartyNameProviderWithAddress : IPartyNameProvider
{
	INCTSCommonAddressInfo Address { get; }
}

public interface INCTSCommonConsignmentItem
{
	ZString GoodsItemNumber { get; }
	ZString DeclarationGoodsItemNumber { get; }
	IReadOnlyCollection<INCTSCommonPackaging> Packaging { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReference { get; }
}

public interface INCTSCommonConsignmentItemDepartureAndAmendmentAndTNN : INCTSCommonConsignmentItem
{
	ZString DeclarationType { get; }
	ZString CountryOfDestination { get; }
	ZString ReferenceNumberUCR { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInformation { get; }
}

public interface INCTSConsignmentItemDepartureAndAmendment : INCTSCommonConsignmentItemDepartureAndAmendmentAndTNN
{
	ZString CountryOfDispatch { get; }
	INCTSPartyNameProviderWithAddress Consignee { get; }
	IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyChainActor { get; }
	INCTSCommodityDepartureAndAmendment Commodity { get; }
	IReadOnlyCollection<INCTSCommonPreviousDocument> PreviousDocument { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> SupportingDocument { get; }
}

public interface INCTSCommonCommodity
{
	ZString DescriptionOfGoods { get; }
	INCTSCommonCommodityCode CommodityCode { get; }
}

public interface INCTSCommonCommodityWithCusCode : INCTSCommonCommodity
{
	ZString CusCode { get; }
}

public interface INCTSCommodityDepartureAndAmendment : INCTSCommonCommodityWithCusCode
{
	IReadOnlyCollection<ICommonDangerousGoods> DangerousGoods { get; }
	INCTSGoodsMeasureDepartureAndAmendment GoodsMeasure { get; }
}

public interface INCTSCommonCommodityCode
{
	ZString HarmonizedSystemSubHeadingCode { get; }
	ZString CombinedNomenclatureCode { get; }
}

public interface INCTSCommonGoodsMeasure
{
	ZDecimal GrossMass { get; }
	ZBool GrossMassSpecified { get; }
	ZDecimal NetMass { get; }
	ZBool NetMassSpecified { get; }
}

public interface INCTSGoodsMeasureDepartureAndAmendment : INCTSCommonGoodsMeasure
{
	ZDecimal SupplementaryUnits { get; }
	ZBool SupplementaryUnitsSpecified { get; }
}

public interface INCTSCommonPackaging : IPackageCommon
{
	ZString SequenceNumber { get; }
	ZString NumberOfPackages { get; }
}

public interface INCTSCommonPreviousDocument : INCTSCommonDocumentWithItem
{
	ZString MeasurementUnitAndQualifier { get; }
	ZDecimal Quantity { get; }
	ZBool QuantitySpecified { get; }
}

public interface INCTSCommonDocumentWithInfo : ICommonDocumentSequenceNumber
{
	ZString ComplementaryInformation { get; }
}

public interface INCTSCommonDocumentWithItem : INCTSCommonDocumentWithInfo
{
	ZString GoodsItemNumber { get; }
}
