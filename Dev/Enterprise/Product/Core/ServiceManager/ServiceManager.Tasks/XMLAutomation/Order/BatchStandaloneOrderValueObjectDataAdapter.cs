using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BatchStandaloneOrderValueObjectDataAdapter : OrderValueObjectDataAdapter
	{
		public BatchStandaloneOrderValueObjectDataAdapter()
		{
		}

		public BatchStandaloneOrderValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
			: base(triggeredByEvents)
		{
		}

		protected override bool ShouldUpdateExistingObject(Order orderBizObj, INotifications notifications)
		{
			bool result = ShouldUpdateShipmentOrdersDuringAutomaticImport(orderBizObj, notifications);

			if (SystemDataRegistry.Instance.CompleteOrderLineUpdate.Value && result)
			{
				result = !orderBizObj.OrderLines.Any((orderLine) => orderBizObj.Factory.LoadTop1<BaseJobComInvoiceLine>(new ZQuery(JobComInvoiceLineSchema.JI_JO, orderLine.PK)) != null);

				if (!result)
				{
					notifications.Notify(new WarningNotification(Res.GetString("d698abe5-2760-4378-b8c8-15228f1bbed0", "Cannot update {0} as it has order lines that are already linked to commercial invoice lines.", orderBizObj.HumanReadableName)));
				}
			}

			return result;
		}

		bool ShouldUpdateShipmentOrdersDuringAutomaticImport(Order orderBizObj, INotifications notifications)
		{
			bool result = false;
			if (!SystemDataRegistry.Instance.UpdateShipmentOrdersDuringAutomaticImport.Value)
			{
				result = base.ShouldUpdateExistingObject(orderBizObj, notifications);
			}
			else
			{
				result = !orderBizObj.IsDeclarationAttached && (!orderBizObj.IsShipmentAttached || orderBizObj.Shipment.Declarations.Length == 0);

				if (!result)
				{
					var warning = Res.GetString("92509B0E-02A0-4634-B868-4BAFA2085E25", "{0} is linked to a declaration and cannot be updated.", orderBizObj.HumanReadableName);
					NotifyWarningIfRequired(warning, notifications);
				}
			}

			return result;
		}

		protected override void RemoveMissingOrderLines(OrderLineCollection existingLines, Xsd.OrderOrderLineCollection xsdOrderLines, INotifications notify)
		{
			if (SystemDataRegistry.Instance.CompleteOrderLineUpdate.Value)
			{
				for (int index = existingLines.Count; index > 0; index--)
				{
					OrderLine orderLine = existingLines[index - 1];

					bool found = xsdOrderLines.Cast<Xsd.OrderOrderLine>().Any(l => l.OrderLineNo == orderLine.JO_LineNo &&
											(orderLine.JO_SubLineNo == l.OrderSubLineNo || !l.OrderSubLineNoSpecified));

					if (!found)
					{
						notify.Notify(new InfoNotification(Res.GetString("d788abe5-2760-4378-b8c8-15228f1bbed0", "Order line with line no {0} will be removed as it doesn't exist in the XML.", orderLine.JO_LineNoAndSplitAndSubLine)));
						orderLine.Delete();
					}
				}
			}
		}
	}
}
