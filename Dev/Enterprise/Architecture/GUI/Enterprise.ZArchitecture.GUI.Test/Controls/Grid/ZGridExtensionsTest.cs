using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridExtensionsTest : TestCaseWithFactory
	{
		#region TestGetCurrentPK

		public void TestGetCurrentPK()
		{
			using (var form = new FormForTest(Factory))
			{
				var grid = form.Grid;
				form.Show();

				grid.SelectSingleElement(form.DummyChild1);
				AssertEquals(form.DummyChild1.PK, grid.GetCurrentPK());

				grid.SelectSingleElement(form.DummyChild2);
				AssertEquals(form.DummyChild2.PK, grid.GetCurrentPK());

				form.Dummy.Collection.RemoveAndDeleteAll();
				AssertEquals(ZGuid.Empty, grid.GetCurrentPK());
			}
		}

		#endregion

		#region TestSelectSingleElementByPK

		public void TestSelectSingleElementByPK()
		{
			using (var form = new FormForTest(Factory))
			{
				var grid = form.Grid;
				form.Show();

				grid.SelectSingleElementByPK(form.DummyChild1.PK);
				AssertContainsExactElementsInAnyOrder(new[] { form.DummyChild1 }, grid.SelectedElements);

				grid.SelectSingleElementByPK(form.DummyChild2.PK);
				AssertContainsExactElementsInAnyOrder(new[] { form.DummyChild2 }, grid.SelectedElements);

				grid.SelectSingleElementByPK(ZGuid.NewZGuid());
				AssertContainsExactElementsInAnyOrder("Selection will not change if attempting to select an invalid bizO.", new[] { form.DummyChild2 }, grid.SelectedElements);

				form.Dummy.Collection.RemoveAndDeleteAll();
				grid.SelectSingleElementByPK(ZGuid.Empty);
				AssertEquals(0, grid.SelectedElements.Length);
			}
		}

		#endregion

		#region TestIsDoubleClickOnRow

		public void TestIsDoubleClickOnRow()
		{
			using (var form = new FormForTest(Factory))
			{
				var grid = form.Grid;
				form.Show();

				AssertEquals(true, grid.IsDoubleClickOnRow(new MouseEventArgs(MouseButtons.Left, 2, 50, 30, 0)));
				AssertEquals("Right-Click should not count as a double-click.", false, grid.IsDoubleClickOnRow(new MouseEventArgs(MouseButtons.Right, 2, 50, 30, 0)));
				AssertEquals("Single-Click should not count as a double-click.", false, grid.IsDoubleClickOnRow(new MouseEventArgs(MouseButtons.Left, 1, 50, 30, 0)));
				AssertEquals("Click outside the row should not count as a double-click.", false, grid.IsDoubleClickOnRow(new MouseEventArgs(MouseButtons.Left, 2, 50, 0, 0)));
			}
		}

		#endregion

		#region TestHookDoubleClickToOpenBizO

		public void TestHookDoubleClickToOpenBizO()
		{
			ZGridExtensions.ControllerForTesting = null;

			var bizO = Factory.New<DummyForTest>();

			using (var form = new ZForm(bizO))
			{
				try
				{
					var grid = new ZGrid();
					grid.DataSource = bizO;
					var c = bizO.Collection.AddNew();
					Factory.Save();
					grid.DataMember = "Collection";
					form.Controls.Add(grid);
					ZGridExtensions.HookDoubleClickToOpenJob<DummyBusinessObject>(grid, DummyControllerIDs.Dummy);
					form.Show();
					AssertNull("Precondition", ZGridExtensions.ControllerForTesting);

					grid.PerformMouseDownForTest(0, 2);
					AssertEquals(ODisplayMode.Browse, ZGridExtensions.ControllerForTesting.LastShownForm.DisplayMode);
					AssertEquals(typeof(ZDummyForm), ZGridExtensions.ControllerForTesting.LastShownForm.GetType());
				}
				finally
				{
					ZGridExtensions.ControllerForTesting.LastShownForm.Dispose();
					ZGridExtensions.ControllerForTesting = null;
				}
			}
		}

		#endregion

		class DummyForTest : AutoDummyBizo
		{
			public DummyForTest(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row) { }

			public DummyBusinessObjectCollection Collection
			{
				get { return collection ?? (collection = new DummyBusinessObjectCollection(Factory)); }
			}
			DummyBusinessObjectCollection collection;
		}
	}
}
