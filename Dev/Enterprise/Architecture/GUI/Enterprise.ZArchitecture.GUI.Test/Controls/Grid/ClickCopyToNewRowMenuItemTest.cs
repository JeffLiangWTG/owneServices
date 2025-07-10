using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.Testing
{
	class TestClickCopyToNewRowMenuItem : TestCaseWithFactory
	{
		public void TestRightClickDataImportAction()
		{
			var parentBO = Factory.New<DummyBOWithITemplateCopyableChildren>();
			parentBO.Collection.AddNew();
			using (var form = new ZTestForm(parentBO))
			{
				form.Show();
				var grid = form.Grid;
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.importDataMenuItem.Visible", true, grid.importDataMenuItem.Visible);
				AssertEquals("grid.importDataMenuItem.Enabled", true, grid.importDataMenuItem.Enabled);

				grid.DisableImportDataMenuItem = true;
				form.Grid.SetCurrentHitTestForTest(0, 0);
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.importDataMenuItem.Visible", false, grid.importDataMenuItem.Visible);
				AssertEquals("grid.importDataMenuItem.Enabled", false, grid.importDataMenuItem.Enabled);
			}
		}

		public void TestRightClickCopyToNewRowAction()
		{
			var parentBO = Factory.New<DummyBOWithITemplateCopyableChildren>();
			parentBO.Collection.AddNew();
			using (var form = new ZTestForm(parentBO))
			{
				form.Show();
				var grid = form.Grid;
				grid.AllowCopyToNewRowMenuItem = false;
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.CopyMenuItem.Visible", false, grid.CopyToNewRowMenuItem.Visible);
				AssertEquals("grid.CopyMenuItem.Enabled", false, grid.CopyToNewRowMenuItem.Enabled);

				grid.AllowCopyToNewRowMenuItem = true;

				form.Grid.SetCurrentHitTestForTest(0, 0);
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.CopyMenuItem.Visible", true, grid.CopyToNewRowMenuItem.Visible);
				AssertEquals("grid.CopyMenuItem.Enabled", true, grid.CopyToNewRowMenuItem.Enabled);

				form.Grid.Focus();
				form.Grid.PerformMouseDownForTest(0, 1, -1);
				form.Grid.PerformMouseUpForTest(0, -1);
				Application.DoEvents();
				AssertEquals("Precondition: form.ActiveControl is ZGrid", true, form.ActiveControl is ZGrid);

				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.CopyMenuItem.Visible", true, grid.CopyToNewRowMenuItem.Visible);
				AssertEquals("grid.CopyMenuItem.Enabled", true, grid.CopyToNewRowMenuItem.Enabled);

				grid.CopyMenuItemClick(grid, new EventArgs());
				AssertEquals("parentBO.Collection.Count", 2, parentBO.Collection.Count);
				AssertEquals("parentBO.Collection.HasChanges", true, parentBO.Collection.HasChanges);
				AssertEquals("grid.ListManager.Position (0 indexed)", 1, grid.ListManager.Position);

				grid.ReadOnly = true;
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.CopyMenuItem.Visible", true, grid.CopyToNewRowMenuItem.Visible);
				AssertEquals("grid.CopyMenuItem.Enabled", false, grid.CopyToNewRowMenuItem.Enabled);
			}
		}

		public void TestCopyToNewRowDoesntShowIfBODoesntImplementITemplateCopyable()
		{
			var nonCopyableChildCollectionBO = Factory.New<DummyBusinessObject>();
			nonCopyableChildCollectionBO.Collection.AddNew();
			using (var form = new ZTestForm(nonCopyableChildCollectionBO))
			{
				form.Show();
				var grid = form.Grid;
				grid.AllowCopyToNewRowMenuItem = true;
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.CopyMenuItem.Visible", false, grid.CopyToNewRowMenuItem.Visible);
				AssertEquals("grid.CopyMenuItem.Enabled", false, grid.CopyToNewRowMenuItem.Enabled);
			}
		}

		public void TestCopyToNewRowDoesntShowIfAllowAddNewTurnedOffOnTheCollection()
		{
			var parentBO = Factory.New<DummyBOWithITemplateCopyableChildren>();
			parentBO.Collection.AddNew();
			parentBO.Collection.SetAllowNew(false);
			using (var form = new ZTestForm(parentBO))
			{
				form.Show();
				var grid = form.Grid;
				grid.AllowCopyToNewRowMenuItem = true;

				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("grid.CopyMenuItem.Visible", false, grid.CopyToNewRowMenuItem.Visible);
				AssertEquals("grid.CopyMenuItem.Enabled", false, grid.CopyToNewRowMenuItem.Enabled);
			}
		}

		public void TestCopyToNewRowFromUncommittedWithDependentCollection()
		{
			var parentBizo = Factory.New<DummyBOWithDependentITemplateCopyableChildren>();
			using (var form = new ZTestForm(parentBizo))
			{
				AssertEquals("Precondition", 0, parentBizo.Collection.Count);

				form.Show();
				Application.DoEvents();

				var grid = form.Grid;
				grid.Focus();
				grid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				AssertEquals("New element should be created in collection", 1, parentBizo.Collection.Count);

				var originalBizo = parentBizo.Collection[0];
				originalBizo.Z0_Guid = parentBizo.PK;
				originalBizo.Z0_Number = 5; // Set some field after Z0_Guid (ordered alphabetically)

				Assert("New element should be uncommitted", ((IBusinessObjectInternals)originalBizo).IsUnCommittedRow);

				grid.SetCurrentHitTestForTest(0, 0);
				grid.ContextMenu_Popup(grid, new EventArgs());

				AssertEquals("grid.CopyMenuItem.Visible", true, grid.CopyToNewRowMenuItem.Visible);
				AssertEquals("grid.CopyMenuItem.Enabled", true, grid.CopyToNewRowMenuItem.Enabled);

				Assert("The act of showing the menu should commit the bizo", !((IBusinessObjectInternals)originalBizo).IsUnCommittedRow);

				grid.CopyMenuItemClick(grid, new EventArgs());

				AssertEquals("New element should be added", 2, parentBizo.Collection.Count);
				Assert("Original element should not be deleted", !originalBizo.IsDeleted);
				Assert("Original element should remain in collection", parentBizo.Collection.Contains(originalBizo));
				Assert("Original element should become committed", !((IBusinessObjectInternals)originalBizo).IsUnCommittedRow);
			}
		}

		public void TestDeleteMenu()
		{
			var parentBO = Factory.New<DummyBOWithITemplateCopyableChildren>();
			var childBO1 = parentBO.Collection.AddNew();

			using (var form = new ZTestForm(parentBO))
			{
				form.Show();
				var grid = form.Grid;
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("Precondition: Delete menu item should be visible", true, grid.DeleteMenuItem.Visible);

				form.Grid.SetCurrentHitTestForTest(0, 0);
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("Delete menu item should be enabled", true, grid.DeleteMenuItem.Enabled);

				form.Grid.SetCurrentHitTestForTest(1, 0);
				grid.ContextMenu_Popup(grid, new EventArgs());
				AssertEquals("Delete menu item should be disabled", false, grid.DeleteMenuItem.Enabled);
			}
		}

		#region Implementation

		class DummyBOWithITemplateCopyableChildren : DummyBusinessObject
		{
			public DummyBOWithITemplateCopyableChildren(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			ChildCollection fCollection;
			public new ChildCollection Collection
			{
				get { return fCollection ?? (fCollection = new ChildCollection(this)); }
			}

			public class ChildCollection : BusinessObjectCollection<DummyChildBOWithITemplateCopyable>
			{
				public ChildCollection(DummyBOWithITemplateCopyableChildren parent) : base(parent.Factory) { }

				internal void SetAllowNew(bool allowNew)
				{
					allowNewOverride = allowNew;
				}

				bool? allowNewOverride;
				protected override bool AllowNewCore
				{
					get { return allowNewOverride ?? base.AllowNewCore; }
				}
			}
		}

		class DummyChildBOWithITemplateCopyable : DummyChildBusinessObject, ITemplateCopyable
		{
			public DummyChildBOWithITemplateCopyable(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			IBusiness ITemplateCopyable.TemplateCopy()
			{
				return Clone();
			}
		}

		class DummyBOWithDependentITemplateCopyableChildren : DummyBusinessObject
		{
			public DummyBOWithDependentITemplateCopyableChildren(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			DependentChildCollection fCollection;
			public new DependentChildCollection Collection
			{
				get { return fCollection ?? (fCollection = new DependentChildCollection(this)); }
			}

			public class DependentChildCollection : BusinessObjectCollection<DummyDependentChildBOWithITemplateCopyable>
			{
				public DependentChildCollection(DummyBOWithDependentITemplateCopyableChildren parent) : base(parent.Factory) { }
			}
		}

		class DummyDependentChildBOWithITemplateCopyable : DummyChildBOWithITemplateCopyable
		{
			public DummyDependentChildBOWithITemplateCopyable(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			DummyBOWithDependentITemplateCopyableChildren Parent
			{
				get { return Factory.Load<DummyBOWithDependentITemplateCopyableChildren>(Z0_Guid); }
			}

			public override ZGuid Z0_Guid
			{
				get { return base.Z0_Guid; }
				set
				{
					base.Z0_Guid = value;

					var parent = Parent;
					if (parent != null)
					{
						parent.Collection.Add(this);
					}
				}
			}
		}

		#endregion
	}
}
