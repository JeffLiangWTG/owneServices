using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class ForwardingConsolCustomsStatusProviderCcsuk : Forwarding.Business.ForwardingConsolCustomsStatusProvider,
																Integration.Customs.GB.CCSUK.ICcsukForwardingConsolCustomsStatusProvider
	{
		public ForwardingConsolCustomsStatusProviderCcsuk(ForwardingConsol consol)
			: base(consol)
		{ }

		public override ZString CustomsCargoStatus()
		{
			var result = ZString.Empty;
			if (Consol.IsValidForCcsukForPlugin())
			{
				var pluginHelper = new ConsolToManyMawbsPluginHelper(Consol);
				if (pluginHelper.Mawbs != null)
				{
					var mawbs = pluginHelper.Mawbs;
					if (mawbs.Count == 1)
					{
						var mawb = mawbs[0];
						result = mawb.DisplayTextForCustomsCargoStatusColumn;
					}
					else if (mawbs.Count > 1)
					{
						var sb = new ZStringBuilder();
						foreach (CusMAWB mawb in mawbs)
						{
							sb.AppendIfNotEmpty(mawb.DisplayTextForCustomsCargoStatusColumn);
						}
						result = sb.ToStringWithDelimiterBetweenAppends("; ");
					}
				}
			}
			return result;
		}
	}
}
