using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class ForwardingShipmentValueObjectDataAdapterForBatchConsol : ForwardingShipmentValueObjectDataAdapter
	{
		public ForwardingShipmentValueObjectDataAdapterForBatchConsol(ForwardingConsol existingConsol, EventsWithSourceType triggeredByEvents)
			: base(existingConsol, triggeredByEvents)
		{
			ShouldPopulateAttachedToOtherConsolNotification = true;
		}

		protected override ForwardingShipment FindBusinessObject(Xsd.Shipment shipmentValue, IValueObjectImportContext context)
		{
			var result = base.FindBusinessObject(shipmentValue, context);

			if (SystemDataRegistry.Instance.AllowLinkingStandaloneShipmentToConsol.Value && result == null)
			{
				var shipmentLocator = new ShipmentLocator<ForwardingShipment>(context.Factory, GetFindQuery(), null, IncludeInactiveForMatchingOnAgentReference);
				var matchedShipment = shipmentLocator.Find(shipmentValue);

				if (matchedShipment != null && ExistingConsol != null)
				{
					if (matchedShipment.Consols.Count == 0)
					{
						ShouldAttachShipmentToConsol = true;
						result = matchedShipment;
					}
					else if (ShouldPopulateAttachedToOtherConsolNotification)
					{
						context.Notify(new InfoNotification(Res.GetString("a210b17a-d2cb-49d7-8729-dabf7c6fe635", "Cannot attach shipment '{0}' to consol '{1}' as it is already attached to other consol. A new shipment will be created instead.", matchedShipment.JobNumber, ExistingConsol.JK_UniqueConsignRef)));
						ShouldPopulateAttachedToOtherConsolNotification = false;
					}
				}
			}
			return result;
		}

		protected override bool ShouldUpdateExistingObject(ForwardingShipment existingShipment, INotifications notifications)
		{
			if (SystemDataRegistry.Instance.AllowLinkingStandaloneShipmentToConsol.Value && ShouldAttachShipmentToConsol)
			{
				ExistingConsol.Shipments.Add(existingShipment);
				notifications.Notify(new InfoNotification(Res.GetString("dc80bfeb-bb2b-4994-956b-8602e7248f4b", "Attaching shipment '{0}' to consol '{1}' ......", existingShipment.JobNumber, ExistingConsol.JK_UniqueConsignRef)));
			}

			return base.ShouldUpdateExistingObject(existingShipment, notifications);
		}

		protected override ShipmentOrdersDataAdapterHelper GetShipmentOrdersDataAdapterHelper(IValueObjectDataAdapter orderValueObjectDataAdapter = null)
		{
			return new BatchShipmentOrdersDataAdapterHelper(TriggeredByEvents, GetOrderValueObjectDataAdapter(TriggeredByEvents), SystemDataRegistry.Instance.UpdateShipmentOrdersDuringConsolAutomaticImport.Value);
		}

		protected override IValueObjectDataAdapter GetOrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			return new OrderValueObjectDataAdapterForBatchShipment(triggeredByEvents, SystemDataRegistry.Instance.UpdateShipmentOrdersDuringConsolAutomaticImport.Value);
		}

		bool ShouldAttachShipmentToConsol;
		bool ShouldPopulateAttachedToOtherConsolNotification;
	}
}
