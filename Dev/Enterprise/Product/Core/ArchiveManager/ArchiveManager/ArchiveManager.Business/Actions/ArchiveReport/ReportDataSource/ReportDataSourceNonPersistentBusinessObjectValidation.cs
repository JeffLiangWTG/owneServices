namespace Enterprise.ArchiveManager.Business.Actions.ArchiveReport
{
	public class ReportDataSourceNonPersistentBusinessObjectValidation : AutoReportDataSourceNonPersistentBusinessObjectValidation
	{
		public ReportDataSourceNonPersistentBusinessObjectValidation(AutoReportDataSourceNonPersistentBusinessObject parent)
			: base(parent)
		{ }

		public new ReportDataSourceNonPersistentBusinessObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ReportDataSourceNonPersistentBusinessObject)base.Parent; }
		}
	}
}
