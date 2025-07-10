using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IAESCommonDataProvider : IESEDIMessageCollectionProvider
{
	IAESCommonMessage Message { get; }
	ZBool IsFinalPeriod { get; }
	ZBool PhaseIDSpecified { get; }
}

public interface IAESCommonMessage
{
	ZString Sender { get; }
	ZString MessageIdentification { get; }
}

public interface IAESCommonExportOperationMRN
{
	ZString MRN { get; }
}

public interface IAESCommonGoodsShipment
{
	IWarehouseCommon Warehouse { get; }
}

public interface IAESCommonConsignment
{
	ZBool IsContainerised { get; }
	ZString InlandModeOfTransport { get; }
}

public interface IAESCommonTransportEquipment : ICommonTransportEquipment
{
	ZString NumberOfSeals { get; }
	IReadOnlyCollection<ISealCommon> Seals { get; }
}

public interface IAESCommonLocationOfGoods : ICommonLocationOfGoods
{
	IPartyContactProvider LocationContactPerson { get; }
}

public interface IAESCommonLine
{
	ZString SequenceNumber { get; }
	ZDecimal StatisticalValue { get; }
}

public interface IAESCommonOrigin
{
	ZString CountryOfOrigin { get; }
	ZString StateOfOrigin { get; }
}

public interface IAESCommonLineNumberDocument : ICommonDocumentSequenceNumber
{
	ZString LineNumber { get; }
}

public interface IAESCommonDocument : IAESCommonLineNumberDocument
{
	ZString Measurement { get; }
	ZDecimal Quantity { get; }
	ZBool QuantitySpecified { get; }
}

public interface IAESCommonSupportingDocumentExtraFields
{
	ZString IssuingAuthorityName { get; }
	DateTime DocumentDate { get; }
	ZBool DocumentDateSpecified { get; }
}
