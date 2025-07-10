using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging.Billing
{
	public class StmUsageData : AutoStmUsageData
	{
		public StmUsageData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SUD_PostedTimeUtc = ZDateTime.UtcNow;
		}
	}
}
