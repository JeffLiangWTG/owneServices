using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(RequiredDocumentsUserControl))]
	class RequiredDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using var control = new RequiredDocumentsUserControl();
			var grid = TestUtility.AssertControlExistance<ZGrid>(control, "CIQRequiredDocumentsGrid", "CIQRequiredDocuments");
			AssertEquals("4 columns", 4, grid.ColumnStyles.Count);
		}
	}
}
