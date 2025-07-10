using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IPresentationT2LMessageDataProvider : IT2LPOUSCommonDataProvider
	{
		ZString PreviousMRN { get; }
		ZString LocationOfGoods { get; }
		IT2LPOUSCommonContainerIndicator ContainerIndication { get; }
		IReadOnlyCollection<IT2LPOUSTransportEquipment> TransportEquipment { get; }
		IReadOnlyCollection<IT2LPOUSPresentationGoodItem> GoodItems { get; }
	}

	public interface IT2LPOUSPresentationGoodItem : IT2LPOUSCommonGoodsItem
	{
		IReadOnlyCollection<IT2LPOUSPresentationPreviousDocument> PreviousDocuments { get; }
		ZInt T2LT2LFgoodsItemNumber { get; }
	}

	public interface IT2LPOUSPresentationPreviousDocument : IDocumentsCommon
	{
		ZString MeasurementUnitAndQualifier { get; }
		ZDecimal Quantity { get; }
		ZBool QuantityValueSpecified { get; }
		ZInt GoodsItemIdentifier { get; }
		IT2LPOUSCommonPackaging Packaging { get; }
	}
}
