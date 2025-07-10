using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IDeclarationAESCommonMessageDataProvider : IAESCommonDataProvider
{
	IReadOnlyCollection<ICommonAuthorisation> Authorisations { get; }
	ZString CustomOfficeOfPresentation { get; }
	ZString CustomOfficeOfExport { get; }
	ZString CustomOfficeOfExit { get; }
	IDeclarationAESExporter Exporter { get; }
	IPartyIdProviderWithContactPerson Declarant { get; }
	ICommonRepresentativeWithContactPerson Representative { get; }
	IDeclarationAESGoodsShipment GoodsShipment { get; }
}

public interface IDeclarationAESMessageDataProvider : IDeclarationAESCommonMessageDataProvider
{
	IDeclarationAESExportOperation ExportOperation { get; }
}

public interface IDeclarationAESExportOperation
{
	ZString LRN { get; }
	ZString DeclarationType { get; }
	ZString DeclarationSubType { get; }
	ZDateTime RecapitulationDate { get; }
	ZBool RecapitulationDateSpecified { get; }
	ZString SecurityFlag { get; }
	ZString SpecificCircumstance { get; }
	ZDecimal TotalAmount { get; }
	ZString Currency { get; }
}

public interface IDeclarationAESExporter : IPartyIdProvider
{
	IPartyAddressProvider Address { get; }
}

public interface IDeclarationAESGoodsShipment : IAESCommonGoodsShipment
{
	ZString NatureOfTransaction { get; }
	ZString CountryOfExport { get; }
	ZString CountryOfDestination { get; }
	IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyActors { get; }
	ICommonDeliveryTerms DeliveryTerms { get; }
	IReadOnlyCollection<IDeclarationAESSupportingDocumentHeader> SupportingDocuments { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReferences { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInfos { get; }
	IDeclarationAESConsignment Consignment { get; }
	IReadOnlyCollection<IDeclarationAESLine> Lines { get; }
}

public interface IDeclarationAESConsignment : IAESCommonConsignment
{
	ZString ModeOfTransportAtBorder { get; }
	ZDecimal GrossMass { get; }
	ZString ReferenceNumberUCR { get; }
	IPartyIdProvider Carrier { get; }
	IPartyIdProvider Consignor { get; }
	IDeclarationAESConsignee Consignee { get; }
	IReadOnlyCollection<IAESCommonTransportEquipment> TransportEquipment { get; }
	IAESCommonLocationOfGoods LocationOfGoods { get; }
	IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans { get; }
	IReadOnlyCollection<ICommonCountryOfRoutingOfConsignment> CountryOfRoutingOfConsignments { get; }
	ITransportMediumInfoCommon ActiveBorderTransportMeans { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments { get; }
	ZString TransportChargesMoP { get; }
}

public interface IDeclarationAESConsignee : IPartyNameProvider
{
	IPartyAddressProvider Address { get; }
}

public interface IDeclarationAESLine : IAESCommonLine
{
	ZString UCRReferenceNumber { get; }
	IReadOnlyCollection<ICommonAuthorisation> Authorisations { get; }
	IDeclarationAESProcedure Procedure { get; }
	IPartyIdProvider Consignor { get; }
	IDeclarationAESConsignee Consignee { get; }
	IReadOnlyCollection<ICommonAdditionalSupplyChainActorSeqNum> AdditionalSupplyActors { get; }
	IAESCommonOrigin Origin { get; }
	IDeclarationAESCommodity Commodity { get; }
	IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum> InternalPackages { get; }
	IReadOnlyCollection<IAESCommonDocument> PreviousDocuments { get; }
	IReadOnlyCollection<IDeclarationAESSupportingDocumentLine> SupportingDocuments { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalReferences { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> AdditionalInfos { get; }
}

public interface IDeclarationAESProcedure
{
	ZString RequestedCPC { get; }
	ZString PreviousCPC { get; }
	IReadOnlyCollection<ICommonAdditionalCode> AdditionalProcedures { get; }
}

public interface IDeclarationAESCommodity
{
	ZString GoodsDescription { get; }
	ZString CusCode { get; }
	IDeclarationAESCommodityCode CommodityCode { get; }
	IReadOnlyCollection<ICommonDangerousGoods> DangerousGoods { get; }
	ICommonGoodsMeasureWithSupUnitsAndSpecified GoodsMeasure { get; }
}

public interface IDeclarationAESCommodityCode : ICommodityCodeCommon
{
	IReadOnlyCollection<ICommonAdditionalCode> TariffAdditionalCodes { get; }
	IReadOnlyCollection<ICommonAdditionalCode> NationalAdditionalCodes { get; }
}

public interface IDeclarationAESSupportingDocumentHeader : IAESCommonLineNumberDocument
{
	IAESCommonSupportingDocumentExtraFields CommonSupportingDocumentExtraFields { get; }
}

public interface IDeclarationAESSupportingDocumentLine : IAESCommonDocument
{
	IAESCommonSupportingDocumentExtraFields CommonSupportingDocumentExtraFields { get; }
}
