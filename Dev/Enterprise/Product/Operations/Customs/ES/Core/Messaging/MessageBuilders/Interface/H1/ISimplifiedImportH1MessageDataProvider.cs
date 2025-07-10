using System.Collections.Generic;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface ISimplifiedImportH1MessageDataProvider : IH1ImportDataProvider
{
	ISimplifiedImportH1GoodsShipment GoodsShipment { get; }
}

public interface ISimplifiedImportH1GoodsShipment : ICommonImportH1GoodsShipment
{
	ICommonImportH1Consigment Consignment { get; }
	IReadOnlyCollection<ISimplifiedImportH1GoodsShipmentItem> GoodsShipmentItems { get; }
}

public interface ISimplifiedImportH1GoodsShipmentItem : ICompleteAndSimplifiedCommonImportH1GoodsShipmentItem
{
	ISimplifiedH1Commodity Commodity { get; }
}

public interface ISimplifiedH1Commodity : IH1CompleteAndSimplifiedCommonImportCommodity
{
	string Preference { get; }
}
