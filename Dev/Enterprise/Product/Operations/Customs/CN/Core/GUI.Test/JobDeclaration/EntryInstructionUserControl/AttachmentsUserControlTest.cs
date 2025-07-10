using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(AttachmentsUserControl))]
	class AttachmentsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using var control = new AttachmentsUserControl();
			var grid = TestUtility.AssertControlExistance<ZGrid>(control, "AttachmentsGrid", "Attachments");
			AssertEquals("4 columns", 4, grid.ColumnStyles.Count);
		}
	}
}
