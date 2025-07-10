namespace Enterprise.Accounting.Business
{
	public interface IComplianceReportQueuer
	{
		void TryToQueueForComplianceReports();
	}
}
