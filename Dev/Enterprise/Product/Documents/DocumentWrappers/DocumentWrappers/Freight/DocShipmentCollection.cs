using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	#region DocForwardingShipmentCollection

	public class DocForwardingShipmentCollection : DocShipmentCollection
	{
		public DocForwardingShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocForwardingShipment this[int index]
		{
			get { return (DocForwardingShipment)Elements[index]; }
		}

		public override void Add(DocShipment shipment)
		{
			if (!typeof(DocForwardingShipment).IsAssignableFrom(shipment.GetType()))
			{
				throw new ArgumentException(string.Format(
					"Cannot add a Wrapper of type '{0}' to DocForwardingShipmentCollection (must be a DocForwardingShipment).",
					shipment.GetType().Name));
			}

			base.Add(shipment);
		}
	}

	#endregion

	#region DocShipmentCollection

	public class DocShipmentCollection : DocumentWrapperCollection
	{
		public DocShipmentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocShipment this[int index]
		{
			get { return (DocShipment)base[index]; }
		}

		public virtual void Add(DocShipment shipment)
		{
			if (shipment != null)
			{
				shipment.SequenceNumber = this.Count + 1;
			}
			base.Add(shipment);
		}

		public void SortOnInterimReceipt()
		{
			this.Sort(new InterimReceiptComparer());
			ReSequence();
		}

		public void SortOnHBL()
		{
			this.Sort(new ShipmentHBLSorter());
			ReSequence();
		}

		public void SortOnShipmentNumber()
		{
			this.Sort("ShipmentNumber", System.ComponentModel.ListSortDirection.Ascending);
			ReSequence();
		}

		protected void ReSequence()
		{
			ZInt sequence = 1;
			foreach (DocShipment shipment in this)
			{
				shipment.SequenceNumber = sequence++;
			}
		}
	}

	#endregion
}
