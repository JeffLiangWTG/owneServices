using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public interface IShipment
	{
		ZString ShipmentID { get; set; }
		ZString HouseBillNumber { get; set; }
		ZString ITNNumber { get; set; }
		ZString ExportStatement { get; set; }
		ZString ExportStatementCode { get; set; }
		ZString ExportStatementField1Type { get; set; }
		ZString ExportStatementField1Code { get; set; }
		ZString ExportStatementField2Type { get; set; }
		ZString ExportStatementField2Code { get; set; }
		ZString DUENumber { get; set; }
		ZString UCRNumber { get; set; }
		ZString CTKNumber { get; set; }
		ZString CTNNumber { get; set; }
		ICodeDescription ContainerPackingMode { get; set; }
		ICodeDescription ShipmentType { get; set; }
		ZString ShipperReference { get; set; }
		ZDateTime PickRequestedByDate { get; set; }
		ZDateTime DeliveryRequiredByDate { get; set; }
		IAddress Buyer { get; }
		IAddress Consignee { get; }
		IAddress Consignor { get; }
		IAddress PickupFrom { get; }
		IAddress PickupCFS { get; }
		IAddress DeliveryTo { get; }
		IAddress DeliveryCFS { get; }
		IAddress NotifyParty { get; }
		IAddress NotifyParty2 { get; }
		IAddress NotifyParty3 { get; }
		IAddress Supplier { get; }
		IUnloco Origin { get; }
		IUnloco Destination { get; }
		IMoney GoodsValue { get; }

		IReadOnlyCollection<IPackingLine> PackingLines { get; }
		IReadOnlyCollection<IShipment> Shipments { get; }
		ITransports Transports { get; }
	}
}
