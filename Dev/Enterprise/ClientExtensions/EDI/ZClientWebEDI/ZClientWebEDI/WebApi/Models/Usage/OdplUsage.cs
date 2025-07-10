using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class OdplUsage
	{
		public OdplUsage(ZString orgName)
		{
			this.OrgName = orgName;
			this.usageDataCollection = new Collection<UsageData>();
		}

		public ZString OrgName { get; private set; }
		public IEnumerable<UsageData> UsageData
		{
			get { return usageDataCollection.OrderBy(x => x.Description); }
		}

		readonly Collection<UsageData> usageDataCollection;

		public UsageData AddUsageData(ZString systemDescription)
		{
			var usageData = new UsageData(systemDescription);
			usageDataCollection.Add(usageData);
			return usageData;
		}
	}
}