
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.Freight.NZ
{
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

		public void Contruct(ConsolShipmentCollection shipments)
		{
			foreach (ForwardingShipment shipment in shipments)
			{
				DocShipment docShipment = DocShipment.New(shipment, Factory);
				Add(docShipment);
			}
			Sort("ShipmentNumber", System.ComponentModel.ListSortDirection.Ascending);
			Resequence();
		}

		protected void Resequence()
		{
			int index = 1;
			foreach (DocShipment docShipment in this)
			{
				docShipment.SequenceNumber = index++;
			}
		}
	}
}
