using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

[TestedType(typeof(SWControlsUserControl))]
sealed class SWControlsUserControlTest : TestCaseWithFactory
{
	public void TestColumnsInfos()
	{
		using var userControl = new SWControlsUserControl();

		var controlGrid = userControl.SWControlsGrid;

		CombineAssertions(() =>
		{
			AssertColumnStyleAvailable("CSI_LineNo");
			AssertColumnStyleAvailable("ControlResultDescription");
			AssertColumnStyleAvailable("CSI_Description");
			AssertColumnStyleAvailable("CSI_ControlLocation");
			AssertColumnStyleAvailable("CSI_ReferenceNumber2");
			AssertColumnStyleAvailable("CSI_DateOfIssue");
			AssertColumnStyleAvailable("CSI_DateOfExpiry");
		});

		void AssertColumnStyleAvailable(string columnName)
		{
			var columnStyle = controlGrid.GetColumnStyle(columnName);
			Assert(!columnStyle.IsUnavailable);
		}
	}
}
