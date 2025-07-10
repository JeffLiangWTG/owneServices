using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.Testing
{
	class GridIntegrationTest : TestCaseWithFactory
	{
		public void TestRowPositionIsNotChangedOnSaveWithActiveBizoCollection()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithActiveCollection>();

			var dummy1 = dummy.AddChildDummy();
			dummy1.AddChildDummy();
			dummy1.AddChildDummy();
			dummy1.AddChildDummy();

			var dummy2 = dummy.AddChildDummy();
			dummy2.AddChildDummy();
			dummy2.AddChildDummy();
			dummy2.AddChildDummy();

			Factory.Save();

			using (var form = new TestForm(dummy))
			{
				form.Show();
				Application.DoEvents();

				form.grid1.CurrentCell = new DataGridCell(1, 0);
				form.grid2.CurrentCell = new DataGridCell(2, 0);

				AssertEquals("Required condition", 2, form.grid1.ListManager.Count);
				AssertEquals("Required condition", 3, form.grid2.ListManager.Count);
				AssertEquals("Precondition", 1, form.grid1.CurrentCell.RowNumber);
				AssertEquals("Precondition", 2, form.grid2.CurrentCell.RowNumber);

				Factory.Save();

				AssertEquals("Required condition", 2, form.grid1.ListManager.Count);
				AssertEquals("Required condition", 3, form.grid2.ListManager.Count);
				AssertEquals("Position should not change", 1, form.grid1.CurrentCell.RowNumber);
				AssertEquals("Position should not change", 2, form.grid2.CurrentCell.RowNumber);
			}
		}

		public void TestDuplicateMenuItem_Enable()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithActiveCollection>();
			dummy.AddChildDummy();

			using (var form = new TestForm(dummy))
			{
				form.Show();
				Application.DoEvents();
				var grid = form.grid1;

				AssertEquals(false, grid.ContextMenu.MenuItems.ContainsKey("Duplicate"));
				grid.AddDuplicateSelectedRowsMenuItem();
				AssertEquals(true, grid.ContextMenu.MenuItems.ContainsKey("Duplicate"));
				grid.AddDuplicateSelectedRowsMenuItem();
				AssertEquals(1, grid.ContextMenu.MenuItems.Cast<MenuItem>().Count(item => item.Text == "Du&plicate"));

				var gridList = ((DummyActiveCollection)grid.ListManager.List);
				gridList.SetAllowAddNewTo(true);
				grid.ContextMenu.DoPopup();
				AssertEquals("Precondition, Allow new should be set for list", true, gridList.AllowNew_Expose);
				AssertEquals(true, grid.ContextMenu.MenuItems["Duplicate"].Enabled);

				gridList.SetAllowAddNewTo(false);
				grid.ContextMenu.DoPopup();
				AssertEquals("Precondition, Allow new should be set for list", false, gridList.AllowNew_Expose);
				AssertEquals(false, grid.ContextMenu.MenuItems["Duplicate"].Enabled);
			}
		}

		public void TestEnteringEnterAndLeave()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithActiveCollection>();
			dummy.AddChildDummy().AddChildDummy();

			var grid1EnteringCount = 0;
			var grid1EnterCount = 0;
			var grid1LeaveCount = 0;

			var grid2EnteringCount = 0;
			var grid2EnterCount = 0;
			var grid2LeaveCount = 0;

			using (var form = new TestForm(dummy))
			{
				form.grid1.Entering += (sender, __) =>
				{
					AssertEquals(form.grid1, sender);
					AssertEquals(grid1EnteringCount, grid1EnterCount);
					AssertEquals(grid1EnteringCount, grid1LeaveCount);
					grid1EnteringCount++;
				};

				form.grid1.Enter += (sender, __) =>
				{
					AssertEquals(form.grid1, sender);
					AssertEquals(grid1EnterCount + 1, grid1EnteringCount);
					AssertEquals(grid1EnterCount, grid1LeaveCount);
					grid1EnterCount++;
				};

				form.grid1.Leave += (sender, __) =>
				{
					AssertEquals(form.grid1, sender);
					AssertEquals(grid1LeaveCount + 1, grid1EnteringCount);
					AssertEquals(grid1LeaveCount + 1, grid1EnterCount);
					grid1LeaveCount++;
				};

				form.grid2.Entering += (sender, __) =>
				{
					AssertEquals(form.grid2, sender);
					AssertEquals(grid2EnteringCount, grid2EnterCount);
					AssertEquals(grid2EnteringCount, grid2LeaveCount);
					grid2EnteringCount++;
				};

				form.grid2.Enter += (sender, __) =>
				{
					AssertEquals(form.grid2, sender);
					AssertEquals(grid2EnterCount + 1, grid2EnteringCount);
					AssertEquals(grid2EnterCount, grid2LeaveCount);
					grid2EnterCount++;
				};

				form.grid2.Leave += (sender, __) =>
				{
					AssertEquals(form.grid2, sender);
					AssertEquals(grid2LeaveCount + 1, grid2EnteringCount);
					AssertEquals(grid2LeaveCount + 1, grid2EnterCount);
					grid2LeaveCount++;
				};

				form.Show();
				Application.DoEvents();

				AssertEquals("Should have entered in grid one because is the first control in the form", 1, grid1EnteringCount);
				AssertEquals("Should have entered in grid one because is the first control in the form", 1, grid1EnterCount);
				AssertEquals(0, grid1LeaveCount);
				AssertEquals(0, grid2EnteringCount);
				AssertEquals(0, grid2EnterCount);
				AssertEquals(0, grid2LeaveCount);

				form.grid2.Focus();
				Application.DoEvents();

				AssertEquals(1, grid1EnteringCount);
				AssertEquals(1, grid1EnterCount);
				AssertEquals(1, grid1LeaveCount);
				AssertEquals(1, grid2EnteringCount);
				AssertEquals(1, grid2EnterCount);
				AssertEquals(0, grid2LeaveCount);

				form.grid1.Focus();
				Application.DoEvents();

				AssertEquals(2, grid1EnteringCount);
				AssertEquals(2, grid1EnterCount);
				AssertEquals(1, grid1LeaveCount);
				AssertEquals(1, grid2EnteringCount);
				AssertEquals(1, grid2EnterCount);
				AssertEquals(1, grid2LeaveCount);
			}
		}

		public void TestListManagerChangedEventOnParentGridPositionChanged()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithActiveCollection>();
			dummy.AddChildDummy().AddChildDummy();
			dummy.AddChildDummy().AddChildDummy();

			using (var form = new TestForm(dummy))
			{
				form.Show();
				Application.DoEvents();

				var listManagerListChangedEvents = 0;
				form.grid2.ListManagerListChanged += (s, e) => listManagerListChangedEvents++;
				form.grid1.ListManager.Position = 1;
				AssertEquals(1, listManagerListChangedEvents);
				form.grid1.ListManager.Position = 0;
				AssertEquals(2, listManagerListChangedEvents);
				form.grid1.ListManager.Position = 1;
				AssertEquals(3, listManagerListChangedEvents);
				form.grid1.ListManager.Position = 1;
				form.grid1.ListManager.Position = 1;
				form.grid1.ListManager.Position = 1;
				AssertEquals(3, listManagerListChangedEvents);
			}
		}

		public void TestUntickAll_SomeElementsUnticked()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Add(Factory.NewWithValidTestData<DummyBusinessObject>());
			collection.Add(Factory.NewWithValidTestData<DummyBusinessObject>());

			collection[0].Z0_Bool = true;
			collection[1].Z0_Bool = false;

			var activeOrAllBusinessObjects = new ActiveOrAllBusinessObjectCollection(collection);

			Factory.Save();

			using (var form = new TestFormWithActiveOrAllCollection(activeOrAllBusinessObjects))
			{
				form.Show();
				AssertNoExceptionThrown("Unticking All is throwing exception when BusinessObjectCollectionView relies on ticked value to determine elements to show", () => form.grid1.TickUntickAllForTest(1, false));
			}
		}

		public void TestTickAll_SomeElementsUnticked()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Add(Factory.NewWithValidTestData<DummyBusinessObject>());
			collection.Add(Factory.NewWithValidTestData<DummyBusinessObject>());

			collection[0].Z0_Bool = true;
			collection[1].Z0_Bool = false;

			var activeOrAllBusinessObjects = new ActiveOrAllBusinessObjectCollection(collection);

			Factory.Save();

			using (var form = new TestFormWithActiveOrAllCollection(activeOrAllBusinessObjects))
			{
				form.Show();
				AssertNoExceptionThrown("Ticking All is throwing exception when BusinessObjectCollectionView relies on ticked value to determine elements to show", () => form.grid1.TickUntickAllForTest(1, true));
			}
		}
	}
}
