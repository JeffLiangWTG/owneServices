using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IComplXAESMessageDataProvider : IAESCommonDataProvider
{
	IComplXAESExportOperation ExportOperation { get; }
	IPartyIdProvider Declarant { get; }
	IPartyIdProvider Representative { get; }
	IComplXAESGoodsShipment GoodsShipment { get; }
}

public interface IComplXAESExportOperation : IAESCommonExportOperationMRN
{
	ZDecimal TotalAmount { get; }
	ZString Currency { get; }
}

public interface IComplXAESGoodsShipment : IAESCommonGoodsShipment
{
	ZString NatureOfTransaction { get; }
	ICommonDeliveryTerms DeliveryTerms { get; }
	IComplXAESConsignment Consignment { get; }
	IReadOnlyCollection<IComplXAESLine> Lines { get; }
}

public interface IComplXAESConsignment
{
	ZString InlandModeOfTransport { get; }
	ZString ModeOfTransportAtBorder { get; }
	ITransportMediumInfoCommon ActiveBorderTransportMeans { get; }
	ZString TransportChargesMoP { get; }
}

public interface IComplXAESLine : IAESCommonLine
{
	IAESCommonOrigin Origin { get; }
	IComplXAESCommodity Commodity { get; }
	IReadOnlyCollection<IAESCommonDocument> PreviousDocuments { get; }
	IReadOnlyCollection<IComplXAESSupportingDocument> SupportingDocuments { get; }
}

public interface IComplXAESCommodity
{
	ICommonGoodsMeasureWithSpecified GoodsMeasure { get; }
}

public interface IComplXAESSupportingDocument : IAESCommonLineNumberDocument
{
	IAESCommonSupportingDocumentExtraFields CommonSupportingDocumentExtraFields { get; }
}
