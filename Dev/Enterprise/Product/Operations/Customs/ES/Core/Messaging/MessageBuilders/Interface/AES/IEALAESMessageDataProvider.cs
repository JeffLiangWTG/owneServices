using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IEALAESMessageDataProvider : IAESCommonDataProvider
{
	IEALAESExportOperation ExportOperation { get; }
	ZString CustomsOfficeOfExitActual { get; }
	IEALAESGoodsShipment GoodsShipment { get; }
}

public interface IEALAESExportOperation : IAESCommonExportOperationMRN
{
	ZBool StoringFlag { get; }
	ZBool DiscrepanciesExist { get; }
}

public interface IEALAESGoodsShipment
{
	IEALAESConsignment Consignment { get; }
	IReadOnlyCollection<IEALAESGoodsItem> GoodsItem { get; }
}

public interface IEALAESConsignment
{
	ZString ModeOfTransportAtTheBorder { get; }
	ZString ReferenceNumberUCR { get; }
	IPartyIdProviderWithContactPerson ExitCarrier { get; }
	IReadOnlyCollection<IAESCommonTransportEquipment> TransportEquipment { get; }
	IEALAESLocationOfGoods LocationOfGoods { get; }
	ITransportMediumInfoCommon ActiveBorderTransportMeans { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocument { get; }
}

public interface IEALAESLocationOfGoods
{
	ZString SequenceNumber { get; }
	ZString LocationType { get; }
	ZString LocationQualifier { get; }
	ZString LocationId { get; }
}

public interface IEALAESGoodsItem
{
	ZString SequenceNumber { get; }
	ZString ReferenceNumberUCR { get; }
	IEALAESCommodity Commodity { get; }
	IReadOnlyCollection<ICommonPackageWithSequenceAndPackNum> Packaging { get; }
	IReadOnlyCollection<ICommonDocumentSequenceNumber> TransportDocuments { get; }
}

public interface IEALAESCommodity
{
	ICommonGoodsMeasureWithSpecified GoodsMeasure { get; }
}
