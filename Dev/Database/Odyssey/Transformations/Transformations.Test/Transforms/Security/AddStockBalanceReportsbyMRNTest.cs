using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddStockBalanceReportsbyMRN))]
	public class AddStockBalanceReportsbyMRNTest : AddReportsToNeoGroupTest
	{
		protected override string ReportType => "RepWhsReport";

		protected override string ReportName => "Stock Balance Reports by MRN";

		protected override string GroupCode => "RepWhsRepo00062";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddStockBalanceReportsbyMRN();
	}
}
