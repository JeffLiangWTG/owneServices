using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	internal class ShipmentAndBillingDetailsCollection : NonPersistentBusinessObjectCollection<ShipmentAndBillingDetailsRow>
	{
		public ShipmentAndBillingDetailsCollection(ForwardingConsol consol) : base(consol.Factory)
		{
			Consol = consol;
			ShipmentsPendingAdd = new List<ForwardingShipment>();
			Initialize();
		}

		protected override bool AllowNewCore => false;

		public void AddRowForShipment(ForwardingShipment shipment)
		{
			if (shipment.IsInDatabase)
			{
				AddRowForShipmentCore(shipment);
			}
			else
			{
				ShipmentsPendingAdd.Add(shipment);
				shipment.OnShipmentSaved += AddToGrid_OnShipmentSaved;
			}
		}

		public void AddRowForShipmentCore(ForwardingShipment shipment)
		{
			if (shipment != null)
			{
				Add(new ShipmentAndBillingDetailsRow(shipment, Consol));
			}
		}

		void AddToGrid_OnShipmentSaved(object sender, EventArgs e)
		{
			if (sender is ForwardingShipment shipment)
			{
				if (!shipment.IsDeleted)
				{
					AddRowForShipmentCore(shipment);
				}
				ShipmentsPendingAdd.Remove(shipment);
				shipment.OnShipmentSaved -= AddToGrid_OnShipmentSaved;
			}
		}

		public void RemoveRowForShipment(ForwardingShipment shipment)
		{
			if (ShipmentsPendingAdd.Contains(shipment))
			{
				ShipmentsPendingAdd.Remove(shipment);
				shipment.OnShipmentSaved -= AddToGrid_OnShipmentSaved;
			}
			else if (this.Cast<ShipmentAndBillingDetailsRow>().FirstOrDefault(x => x.RelatedJobNum == shipment.JobNumber) is ShipmentAndBillingDetailsRow row)
			{
				Remove(row);
			}
		}

		public void Initialize()
		{
			Consol.Shipments.ForEach(x => AddRowForShipment((ForwardingShipment)x));
		}

		readonly ForwardingConsol Consol;
		readonly List<ForwardingShipment> ShipmentsPendingAdd;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShipmentAndBillingDetailsRow();
		}

		#endregion
	}
}
