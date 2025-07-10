using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public static class ISupportQueueingForComplianceReportsExtensions
	{
		public static void QueueForComplianceReports(this ISupportQueueingForComplianceReports bizo, BusinessObjectFactory factory = null)
		{
			if (bizo.CheckIsValidForQueueing(factory ?? bizo.Factory))
			{
				bizo.Queuer.TryToQueueForComplianceReports();
			}
		}
	}
}
