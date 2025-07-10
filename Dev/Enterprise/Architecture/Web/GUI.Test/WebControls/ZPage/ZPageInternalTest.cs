using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZPageInternalTest : ZPageTestCase
	{
		class GridItemDummyBusinessObject : DummyBusinessObject
		{
			public GridItemDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public bool ClearAllNotificationsHasBeenCalled { get; set; }

			protected override void ClearAllNotificationsCore()
			{
				ClearAllNotificationsHasBeenCalled = true;
			}

			public bool IsInDatabaseForTesting { get; set; }

			public override bool IsInDatabase => IsInDatabaseForTesting;
		}

		public void TestClearAllNotificationsForNewEmptyGridRows()
		{
			var grid = new ZDataGrid();
			var item = Factory.New<GridItemDummyBusinessObject>();
			grid.DataSource = new DummyBusinessObjectCollection(Factory);
			grid.Collection.Add(item);

			grid.ReadOnly = true;
			grid.AllowAdd = true;
			grid.AllowEdit = true;
			item.ClearAllNotificationsHasBeenCalled = false;
			Page.ClearAllNotificationsForNewEmptyGridRows(grid);
			AssertEquals(false, item.ClearAllNotificationsHasBeenCalled);

			grid.ReadOnly = false;
			Page.ClearAllNotificationsForNewEmptyGridRows(grid);
			AssertEquals(true, item.ClearAllNotificationsHasBeenCalled);

			grid.AllowEdit = false;
			item.ClearAllNotificationsHasBeenCalled = false;
			Page.ClearAllNotificationsForNewEmptyGridRows(grid);
			AssertEquals(true, item.ClearAllNotificationsHasBeenCalled);

			grid.AllowAdd = false;
			item.ClearAllNotificationsHasBeenCalled = false;
			Page.ClearAllNotificationsForNewEmptyGridRows(grid);
			AssertEquals(false, item.ClearAllNotificationsHasBeenCalled);
		}

		public void TestRemoveNewEmptyGridRows()
		{
			var grid = new ZDataGrid();
			var item = Factory.New<GridItemDummyBusinessObject>();
			grid.DataSource = new DummyBusinessObjectCollection(Factory);
			grid.Collection.Add(item);

			grid.ReadOnly = true;
			grid.AllowAdd = true;
			grid.AllowEdit = true;
			Page.RemoveNewEmptyGridRows(grid);
			AssertEquals(false, item.IsDeleted);
			AssertCollectionContains(item, grid.Collection);

			grid.ReadOnly = false;
			Page.RemoveNewEmptyGridRows(grid);
			AssertEquals(true, item.IsDeleted);
			AssertCollectionNotContains(item, grid.Collection);

			grid.AllowEdit = false;
			item = Factory.New<GridItemDummyBusinessObject>();
			grid.Collection.Add(item);
			Page.RemoveNewEmptyGridRows(grid);
			AssertEquals(true, item.IsDeleted);
			AssertCollectionNotContains(item, grid.Collection);

			grid.AllowAdd = false;
			item = Factory.New<GridItemDummyBusinessObject>();
			grid.Collection.Add(item);
			Page.RemoveNewEmptyGridRows(grid);
			AssertEquals(false, item.IsDeleted);
			AssertCollectionContains(item, grid.Collection);
		}

		protected override ZPage GetNewZPage()
		{
			return new ZTestPage();
		}
	}
}
