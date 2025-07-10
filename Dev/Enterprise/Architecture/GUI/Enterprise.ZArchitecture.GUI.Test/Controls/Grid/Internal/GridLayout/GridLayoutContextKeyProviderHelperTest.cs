using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridLayoutContextKeyProviderHelperTest : TestCaseWithDummy
	{
		public void TestGetAllGridIDsForStmModuleFilter_GridIsSpecified_ReturnIds()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var helper = new GridLayoutContextKeyProviderHelper();

				var ids = helper.GetAllGridIDsForStmModuleFilter(form.TabGrid);

				var expectedIDs = new[] { "DataGridLayout|08923409054io45097", "GridLayoutofAakGMRBPGJTfi46LIQCQ==", "GridLayout91zGlAs9vRdkLkL7TtTk0A==" };
				AssertContainsExactElementsInAnyOrder(expectedIDs, ids);
			}
		}

		public void TestGetAllGridIDsForStmModuleFilterHandlesDuplicate()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var helper = new GridLayoutContextKeyProviderHelperDuplicateKey();
				var ids = helper.GetAllGridIDsForStmModuleFilter(form.TabGrid);

				var expectedIDs = new[] { "DataGridLayout|08923409054io45097" };
				AssertContainsExactElementsInAnyOrder(expectedIDs, ids);
			}
		}
	}
}
