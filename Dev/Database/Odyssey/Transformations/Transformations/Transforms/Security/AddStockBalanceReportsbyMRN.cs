namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class AddStockBalanceReportsbyMRN : AddReportsToNeoGroup
	{
		protected override string ReportType => "RepWhsReport";

		protected override string ReportName => "Stock Balance Reports by MRN";

		protected override string GroupCode => "RepWhsRepo00062";
	}
}
