using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccComplianceReportProcessTaskCollection : ProcessTaskCollection
	{
		public AccComplianceReportProcessTaskCollection(AccComplianceReport report)
			: base(report)
		{
		}

		public new AccComplianceReportProcessTask this[int index]
		{
			get { return (AccComplianceReportProcessTask)Elements[index]; }
		}

		public new AccComplianceReportProcessTask AddNew()
		{
			return (AccComplianceReportProcessTask)base.AddNew();
		}
	}
}
