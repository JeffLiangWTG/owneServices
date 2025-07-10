namespace Enterprise.DocumentEngine
{
	public class ReportPrintSet : PrintTask
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ReportPrintSet(ReportCommand reportCommand)
			: base(reportCommand)
		{
			IsReportPrintSet = true;
			DocumentPack documentPack = new DocumentPack(reportCommand);
			Add(documentPack);
		}
	}
}
