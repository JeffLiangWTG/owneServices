using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IG5GenericMessageDataProvider : IESEDIMessageCollectionProvider
{
	ZString SenderId { get; }
}

public interface IG5CommonMessageDataProvider : IG5GenericMessageDataProvider
{
	IG5CommonHeader Header { get; }
	IReadOnlyCollection<IG5CommonLine> Lines { get; }
}

public interface IG5SimplifiedHeader
{
	ZString LRN { get; }
	IG5PartyInfo Declarant { get; }
	IG5RepresentativeInfo Representative { get; }
	IReadOnlyCollection<IDocumentsCommon> AdditionalInfos { get; }
}

public interface IG5CommonHeader : IG5SimplifiedHeader
{
	ZString OriginCustomsOffice { get; }
	IG5LocationGoods GoodsLocationOrigin { get; }
	ZString DestinationCustomsOffice { get; }
	IG5LocationGoods GoodsLocationDestination { get; }
	ZString TSWarehouse { get; }
	ICommonArrivalTransportMeans ArrivalTransportMeans { get; }
	IDocumentsCommon TransportDocument { get; }
	IG5PartyInfo Consignor { get; }
	IG5PartyInfo Consignee { get; }
	IReadOnlyCollection<IDocumentsCommon> SupportingDocuments { get; }
	ZString TotalLinesNum { get; }
	ZInt TotalPackagesNum { get; }
	ZDecimal TotalGrossWeightInKG { get; }
}

public interface IG5LocationGoods
{
	ZString NationalLocation { get; }
	IGenericLocation GenericLocation { get; }
}

public interface IG5PartyInfo : IPartyNameProvider
{
	ZString Type { get; }
	ZString Street { get; }
	ZString StreetAddLine { get; }
	ZString Number { get; }
	ZString POBox { get; }
	ZString State { get; }
	ZString Country { get; }
	ZString PostCode { get; }
	ZString City { get; }
	ZString CommunicationType { get; }
	ZString CommunicationId { get; }
}

public interface IG5RepresentativeInfo : IG5PartyInfo
{
	ZString Status { get; }
}

public interface IG5CommonLine
{
	ZString LineNumber { get; }
	IG5PreviousDocument PreviousDocument { get; }
	ZInt PackagesNum { get; }
	IReadOnlyCollection<IInternalPackageIdentificationCommon> Packages { get; }
	ZDecimal GrossWeightInKG { get; }
	IDocumentsCommon TransportDocument { get; }
	ZString UCRCode { get; }
	ZString CommodityCode { get; }
	ZString GoodsDescription { get; }
	ZString CusCode { get; }
	IReadOnlyCollection<IG5TransportEquipment> TransportEquipments { get; }
	ZDateTime PresentationDateAtOrigin { get; }
	IReadOnlyCollection<IDocumentsCommon> SupportingDocuments { get; }
	IReadOnlyCollection<IDocumentsCommon> AdditionalInfo { get; }
}

public interface IG5PreviousDocument
{
	IG5PreviousTSD PreviousTSD { get; }
	ICommonDocumentGoodsItemId PreviousGeneric { get; }
}

public interface IG5PreviousTSD
{
	ZString MRN { get; }
	ICommonArrivalTransportMeans TransportMeans { get; }
	IDocumentsCommon TransportDocument { get; }
	ZString GoodsItemId { get; }
}

public interface IG5TransportEquipment
{
	ZString Id { get; }
	ZString PackedStatus { get; }
	IReadOnlyCollection<ZString> SealIds { get; }
}
