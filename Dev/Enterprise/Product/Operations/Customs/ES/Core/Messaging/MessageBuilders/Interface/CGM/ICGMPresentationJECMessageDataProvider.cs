using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ICGMPresentationJECMessageDataProvider : ICGMCommonDataProvider
{
	ZString PresentationCustomsOffice { get; }
	ZString MRN { get; }
	ICGMPartyProviderWithAddressAndContactPerson RepresentativeAtArrivalForCGM { get; }
	ZBool IsContainerised { get; }
	IReadOnlyCollection<ICGMPresentationTransportEquipment> TransportEquipments { get; }
	IReadOnlyCollection<ICGMPresentationGoodsItem> GoodsItems { get; }
}

public interface ICGMPresentationTransportEquipment
{
	ZString ContainerNumber { get; }
	IReadOnlyCollection<ZInt> GoodsReference { get; }
}

public interface ICGMPresentationGoodsItem
{
	ZInt GoodsItemNumber { get; }
	IReadOnlyCollection<ICGMPresentationPreviousDocument> PreviousDocuments { get; }
	ZInt T2LT2LFgoodsItemNumber { get; }
}

public interface ICGMPresentationPreviousDocument : IDocumentsCommon
{
	ZString TypeOfPackages { get; }
	ZInt NumberOfPackages { get; }
	ZString MeasurementUnitAndQualifier { get; }
	ZDecimal Quantity { get; }
	ZInt GoodsItemId { get; }
}
