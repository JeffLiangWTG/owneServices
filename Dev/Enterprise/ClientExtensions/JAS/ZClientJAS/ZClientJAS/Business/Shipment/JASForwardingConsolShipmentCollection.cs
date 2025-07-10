using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS.Business
{
	public class JASForwardingConsolShipmentCollection : ForwardingConsolShipmentCollection
	{
		public JASForwardingConsolShipmentCollection(JASForwardingConsol parent)
			: base(parent)
		{
		}

		public new JASForwardingShipment this[int index]
		{
			get { return (JASForwardingShipment)Elements[index]; }
		}

		public new JASForwardingShipment AddNew()
		{
			return (JASForwardingShipment)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(JASForwardingShipment);
		}
	}
}
