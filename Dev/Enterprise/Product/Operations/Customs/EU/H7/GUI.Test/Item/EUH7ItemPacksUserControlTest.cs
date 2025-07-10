using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7ItemPacksUserControl))]
	sealed class EUH7ItemPacksUserControlTest : TestCaseWithFactory
	{
		public void TestGridColumnsManifest()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new EUH7ItemPacksUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("CusTempPacksGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid, ["IsLinked", "PackQty", "PackageNumber"]);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, "IsLinked");
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: true, "PackQty");
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: true, "PackageNumber");
				});
			}
		}

		public void TestGridColumnsBill()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var package = bill.Packs.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new EUH7ItemPacksUserControl())
			{
				control.SetDataBinding(bill, string.Empty);
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("CusTempPacksGrid");

				CombineAssertions(() =>
				{
					EUH7GUITestHelper.AssertGridLayout(grid, ["IsLinked", "PackQty", "PackageNumber"]);
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: false, "IsLinked");
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: true, "PackQty");
					EUH7GUITestHelper.AssertReadOnlyGridColumns(grid, shouldBeReadOnly: true, "PackageNumber");
				});
			}
		}
	}
}
