using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ExposedReportAnalyser : ReportAnalyser
	{
		public ExposedReportAnalyser(Report report)
			: base(report)
		{
		}

		public void ExposedAddDataSource(string cellValue)
		{
			AddDataSource(cellValue);
		}

		public void ExposedAddNodeToConstantsIfNotAlreadyAdded(StringTreeNode constantNode)
		{
			base.AddNodeToConstantsIfNotAlreadyAdded(constantNode);
		}
	}
}
