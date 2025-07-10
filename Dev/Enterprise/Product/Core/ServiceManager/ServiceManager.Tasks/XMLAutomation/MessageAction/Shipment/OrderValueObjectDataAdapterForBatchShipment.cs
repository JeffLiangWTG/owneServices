using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class OrderValueObjectDataAdapterForBatchShipment : OrderValueObjectDataAdapter
	{
		readonly bool shouldUpdateShipmentOrders;

		public OrderValueObjectDataAdapterForBatchShipment(EventsWithSourceType triggeredByEvents, bool shouldUpdateShipmentOrders)
			: base(triggeredByEvents)
		{
			this.shouldUpdateShipmentOrders = shouldUpdateShipmentOrders;
		}

		protected override bool ShouldUpdateExistingObject(Order orderBizObj, INotifications notifications)
		{
			bool result = false;

			if (!shouldUpdateShipmentOrders)
			{
				result = base.ShouldUpdateExistingObject(orderBizObj, notifications);
			}
			else
			{
				result = !orderBizObj.IsDeclarationAttached && (!orderBizObj.IsShipmentAttached || orderBizObj.IsShipmentAttached && orderBizObj.Shipment.Declarations.Length == 0);

				if (!result)
				{
					var warning = Res.GetString("1429898d-e29c-4d49-ac54-be38b54e9bb5", "{0} is linked to a declaration and cannot be updated.", orderBizObj.HumanReadableName);
					NotifyWarningIfRequired(warning, notifications);
				}
			}

			return result;
		}
	}
}
