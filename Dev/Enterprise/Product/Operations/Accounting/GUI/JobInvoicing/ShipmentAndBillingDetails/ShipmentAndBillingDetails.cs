using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	internal class ShipmentAndBillingDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ShipmentAndBillingDetails(ForwardingConsol consol) : base(consol.Factory)
		{
			Consol = consol;
			Consol.Shipments.CountChanged += ConsolShipments_CountChanged;
		}

		readonly ForwardingConsol Consol;

		public ShipmentAndBillingDetailsCollection DetailsRows
		{
			get
			{
				return detailsRows ?? (detailsRows = new ShipmentAndBillingDetailsCollection(Consol));
			}
		}
		ShipmentAndBillingDetailsCollection detailsRows;

		void ConsolShipments_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var shipment = e.BizObject as ForwardingShipment;
			if (!e.BizObject.IsDeleted)
			{
				if (e.ItemAdded)
				{
					DetailsRows.AddRowForShipment(shipment);
				}
				else if (e.ItemRemoved)
				{
					DetailsRows.RemoveRowForShipment(shipment);
				}
			}
		}

		public ZGuid GetRowPKByRelatedJob(ZString jobNum)
		{
			return DetailsRows.Cast<ShipmentAndBillingDetailsRow>().FirstOrDefault(x => x.RelatedJobNum == jobNum)?.PK ?? ZGuid.Empty;
		}
	}
}
