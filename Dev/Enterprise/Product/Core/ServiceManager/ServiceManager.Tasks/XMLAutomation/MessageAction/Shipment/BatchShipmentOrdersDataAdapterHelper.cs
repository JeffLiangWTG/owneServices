using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchShipmentOrdersDataAdapterHelper : ShipmentOrdersDataAdapterHelper
	{
		readonly bool shouldUpdateShipmentOrders;

		public BatchShipmentOrdersDataAdapterHelper(EventsWithSourceType triggeredByEvents, IValueObjectDataAdapter orderValueObjectDataAdapter, bool shouldUpdateShipmentOrders)
			: base(triggeredByEvents, orderValueObjectDataAdapter)
		{
			this.shouldUpdateShipmentOrders = shouldUpdateShipmentOrders;
		}

		protected override bool CanAttachToOrder(ImportArgs importArgs, Xsd.Order orderValue, out Order order)
		{
			var canAttachOrder = false;

			if (!shouldUpdateShipmentOrders)
			{
				canAttachOrder = base.CanAttachToOrder(importArgs, orderValue, out order);
			}
			else
			{
				order = importArgs.Context.Factory.LoadTop1<Order>(GetOrderFilter(orderValue, importArgs.Context));

				canAttachOrder = (order == null ||
						(order != null && !order.IsDeclarationAttached &&
						 (!order.IsShipmentAttached ||
						 (order.IsShipmentAttached && order.Shipment.PK == importArgs.Shipment.PK && order.Shipment.Declarations.Length == 0))
						));

				if (canAttachOrder)
				{
					order = (Order)OrderValueObjectDataAdapter.CreateOrUpdateFromValueObject(orderValue, importArgs.Context);
					canAttachOrder = order != null;
				}
			}
			return canAttachOrder;
		}
	}
}
