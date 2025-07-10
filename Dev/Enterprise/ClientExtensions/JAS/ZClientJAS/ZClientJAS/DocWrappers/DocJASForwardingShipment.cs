using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.JAS
{
	public class DocJASForwardingShipment : DocForwardingShipment
	{
		#region Implementation

		protected DocJASForwardingShipment(ForwardingShipment shipment, BusinessObjectFactory factory) : base(shipment, factory) { }

		public new static DocJASForwardingShipment New(ForwardingShipment shipment, BusinessObjectFactory factory)
		{
			return (shipment == null) ? null : new DocJASForwardingShipment(shipment, factory);
		}

		static DocJASForwardingShipment OverriddenNewMethod(ForwardingShipment shipment, BusinessObjectFactory factory)
		{
			return DocJASForwardingShipment.New(shipment, factory);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		#endregion

		public ZString BONDNumber
		{
			get { return JASDataRegistry.Instance.BOLBondNum; }
		}
	}
}

#region Setup
#endregion
