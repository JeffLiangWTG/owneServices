using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.TSF.DocWrappers
{
	public class DocTSFForwardingShipment : DocForwardingShipment
	{
		#region Constructors and Type Overriding

		public DocTSFForwardingShipment(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public new static DocTSFForwardingShipment New(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return (shipment != null) ? new DocTSFForwardingShipment(shipment, factoryToWrap) : null;
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(OverriddenNewMethod);
		}

		static DocTSFForwardingShipment OverriddenNewMethod(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			return DocTSFForwardingShipment.New(shipment, factoryToWrap);
		}

		#endregion

		#region Wrapper Fields

		public DocContainer FirstContainer
		{
			get
			{
				DocContainer result = null;

				if (Containers.Count > 0)
				{
					result = (DocContainer)Containers[0];
				}

				return result;
			}
		}

		#endregion

	}
}
