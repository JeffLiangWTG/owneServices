using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.DocumentWrappers.Freight.NZ
{
	public class DocShipment : DocBaseWrapper
	{
		protected DocShipment(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
			this.Shipment = shipment;
		}

		public static DocShipment New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			DocShipment result = null;

			if (shipment != null)
			{
				result = new DocShipment(shipment, factoryToWrap);
			}

			return result;
		}

		public ZString ShipmentNumber
		{
			get { return Shipment.JS_UniqueConsignRef; }
		}

		public ZString HouseBill
		{
			get { return Shipment.JS_HouseBill; }
		}

		public ZInt SequenceNumber
		{
			get { return fSequenceNumber; }
			set { fSequenceNumber = value; }
		}

		public ZString ClearanceNo
		{
			get { return Shipment.CustomsEntryNumber; }
		}

		#region Implementation

		protected readonly ForwardingShipment Shipment;
		protected ZInt fSequenceNumber;

		#endregion
	}
}
