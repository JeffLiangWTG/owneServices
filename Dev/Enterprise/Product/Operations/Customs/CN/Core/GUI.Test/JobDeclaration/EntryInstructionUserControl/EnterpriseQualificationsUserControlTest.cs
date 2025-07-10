using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(EnterpriseQualificationsUserControl))]
	class EnterpriseQualificationsUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using var control = new EnterpriseQualificationsUserControl();
			TestUtility.AssertControlExistance(control, "XC_EnterprisePromisedCheckBox", "CEI_EnterprisePromised");
			var grid = TestUtility.AssertControlExistance<ZGrid>(control, "EnterpriseQualificationsGrid", "EnterpriseQualifications");
			AssertEquals("3 columns", 3, grid.ColumnStyles.Count);
		}
	}
}
