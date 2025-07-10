using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.TNT.DocWrappers
{
	public class TNTDocForwardingShipment : DocForwardingShipment
	{
		#region Constructors and Type override

		protected TNTDocForwardingShipment(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public new static TNTDocForwardingShipment New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return (shipment != null) ? new TNTDocForwardingShipment(shipment, factoryToWrap) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		public override ZDecimal GoodsValue
		{
			get { return base.GoodsValue.Round(2); }
		}
	}
}
