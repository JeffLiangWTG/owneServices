using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IGoodsNotificationAESMessageDataProvider : IAESCommonDataProvider
{
	IGoodsNotificationAESExportOperationLRN ExportOperation { get; }
	ZString CustomOfficeOfPresentation { get; }
	ZString CustomOfficeOfExport { get; }
	IPartyIdProviderWithContactPerson Declarant { get; }
	ICommonRepresentativeWithContactPerson Representative { get; }
	IGoodsNotificationAESGoodsShipment GoodsShipment { get; }
}

public interface IGoodsNotificationAESExportOperationLRN
{
	ZString LRN { get; }
}

public interface IGoodsNotificationAESGoodsShipment
{
	IGoodsNotificationAESConsignment Consignment { get; }
}

public interface IGoodsNotificationAESConsignment : IAESCommonConsignment
{
	IReadOnlyCollection<IAESCommonTransportEquipment> TransportEquipment { get; }
	IAESCommonLocationOfGoods LocationOfGoods { get; }
	IReadOnlyCollection<ICommonDepartureTransportMeans> DepartureTransportMeans { get; }
}
