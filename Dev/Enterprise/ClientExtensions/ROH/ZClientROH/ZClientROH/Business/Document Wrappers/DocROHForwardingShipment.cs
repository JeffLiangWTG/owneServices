using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.Rohlig.DocWrappers
{
	public class DocROHForwardingShipment : DocForwardingShipment
	{
		#region Contructor Overrides
		protected DocROHForwardingShipment(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public static new DocROHForwardingShipment New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return (shipment == null) ? null : new DocROHForwardingShipment(shipment, factoryToWrap);
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public ZString HouseBillSignedBy
		{
			get
			{
				return RohDataRegistry.Instance.HouseBillSignedBy.ToUpper();
			}
		}
	}
}

#region Implementation
#endregion
