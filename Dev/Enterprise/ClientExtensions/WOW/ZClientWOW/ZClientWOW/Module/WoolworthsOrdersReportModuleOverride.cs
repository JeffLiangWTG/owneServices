using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Freight.Forwarding.Orders.Module;

namespace Enterprise.Client.Wow
{
	public class WoolworthsOrdersReportModuleOverride : OrdersReportModule
	{
		public WoolworthsOrdersReportModuleOverride()
		{
		}

		protected override ReportUserControl NewReportUserControl(ReportCommandCollection collection)
		{
			return new WoolworthsReportUserControl(ID, collection, this.SecurityCheckpoint);
		}
	}
}
