using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public class ForwardingShipmentCustomsStatusProviderCcsuk : EU.Business.ForwardingShipmentCustomsStatusProvider,
																Integration.Customs.GB.CCSUK.ICcsukForwardingShipmentCustomsStatusProvider
	{
		public ForwardingShipmentCustomsStatusProviderCcsuk(ForwardingShipment shipment)
			: base(shipment)
		{ }

		public override ZString CustomsCargoStatus()
		{
			var result = ZString.Empty;
			using (var plugin = new CcsukShipmentMultiHawbPlugin(Shipment))
			{
				if (plugin.IsValidShipmentForCcsuk)  // means we don't even bother to look for HAWBs when this shipment is not relevant, when rego is set to hide, when unlicenced, or when consol is a direct. 
				{
					if (plugin.HawbPluginHelper != null && plugin.HawbPluginHelper.Hawbs != null)
					{
						var hawbs = plugin.HawbPluginHelper.Hawbs;
						if (hawbs.Count == 1)
						{
							var hawb = hawbs[0];
							result = hawb.DisplayTextForCustomsCargoStatusColumn;
						}
						else if (hawbs.Count == 0)
						{
							result = "No HAWB(s) found";
						}
						else if (hawbs.Count > 1)
						{
							var sb = new ZStringBuilder();
							foreach (CusHAWB hawb in hawbs)
							{
								sb.AppendIfNotEmpty(hawb.DisplayTextForCustomsCargoStatusColumn);
							}
							result = sb.ToStringWithDelimiterBetweenAppends("; ");
						}
					}
				}
			}
			return result;
		}
	}
}
