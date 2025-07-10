using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IIncompleteImportH1MessageDataProvider : IH1CommonImportDataProvider
{
	IIncompleteImportH1ImportOperation ImportOperation { get; }
	ZString CountryOfDispatch { get; }
	IReadOnlyCollection<ICommonTransportEquipment> TransportEquipments { get; }
	IReadOnlyCollection<IIncompleteImportH1GoodsShipmentItem> GoodsShipmentItems { get; }
}

public interface IIncompleteImportH1ImportOperation : IH1CommonImportOperation
{
	ZString CustomsRegistrationNumber { get; }
}

public interface IIncompleteImportH1GoodsShipmentItem : ICommonH1GoodsShipmentItem
{
	ICommonH1Procedure Procedure { get; }
	ZString CountryOfOrigin { get; }
	IIncompleteImportH1Commodity Commodity { get; }
}

public interface IIncompleteImportH1Commodity : ICommonH1Commodity
{
	ICommonH1CommodityCode CommodityCode { get; }
}
