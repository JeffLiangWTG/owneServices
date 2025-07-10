using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	class ZGridIModifyModuleAndGridLayoutImpTest : TestCaseWithDummy
	{
		public void TestGetReasonLayoutNameNotAllowed()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = Guid.NewGuid();

				form.TabGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible = true;

				form.TabGrid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate { return true; });

				var layout1 = Factory.New<StmModuleFilter>();
				layout1.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				layout1.S9_FilterName = "1";
				layout1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layout1.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;

				var layout2 = Factory.New<StmModuleFilter>();
				layout2.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter;
				layout2.S9_FilterName = "2";
				layout2.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layout2.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;

				var layoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				AssertEquals("Contains custom columns, therefore should save with a one with LayoutContextKey", new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter, layoutManageable.LayoutSetIdentifierToSaveANewLayoutWith);

				var reason = layoutManageable.GetReasonLayoutNameNotAllowed("1", false);
				AssertEquals("Cannot name a new layout to '1'", ZGridLayoutModification.OverridingExistingLayoutNotAcceptableDueToCustomColumns, reason);

				reason = layoutManageable.GetReasonLayoutNameNotAllowed("2", false);
				AssertEquals("However '2' is allowed because it is saved with the same layout key", "", reason);

				form.TabGrid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate { return false; });

				AssertEquals("Contains no custom columns, therefore should save with a one without LayoutContextKey", new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter, layoutManageable.LayoutSetIdentifierToSaveANewLayoutWith);
				reason = layoutManageable.GetReasonLayoutNameNotAllowed("1", false);
				AssertEquals("1 is allowed as it will be saved with a key same as layout1", "", reason);
			}
		}

		public void TestSaveLayoutModifyLayoutIDIfRequired()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = Guid.NewGuid();

				form.TabGrid.Columns[DummyBizoSchema.Z0_Description.Name].IsVisible = true;

				var generalLayout = Factory.New<StmModuleFilter>();
				generalLayout.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				generalLayout.S9_FilterName = "1";
				generalLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				generalLayout.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;

				var specificLayout = Factory.New<StmModuleFilter>();
				specificLayout.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter;
				specificLayout.S9_FilterName = "2";
				specificLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				specificLayout.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				Factory.Save();

				var layoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				//It does not contain any organisation-specific columns
				form.TabGrid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate { return false; });

				//It should not update specificLayout.S9_ModuleID so that it is saved as a general layout
				new DataGridLayoutManager().SavePreconfiguredLayout(layoutManageable, "2", false, false, SaveColumnLayout.Yes);

				var factory2 = new BusinessObjectFactory();
				var specificLayoutLoaded = factory2.Load<StmModuleFilter>(specificLayout.PK);
				AssertEquals("S9_ModuleID should have been changed to a general one", new DataGridLayoutContextKeyProvider(form.TabGrid, false).ContextKeyForStmModuleFilter, specificLayoutLoaded.S9_ModuleID);
			}
		}

		public void TestLayoutSetIdentifiers()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = Guid.NewGuid();

				var columns = new ZGridColumns(form.TabGrid);
				var layoutManageable = new ZGridLayoutModification(form.TabGrid, columns, Factory, null);
				AssertEquals(4, layoutManageable.LayoutSetIdentifiers.Length);

				form.TabGrid.LayoutCategoryPK = Guid.Empty;
				AssertEquals(3, layoutManageable.LayoutSetIdentifiers.Length);
			}
		}

		public void TestUseDifferentFactoryToGridDataSource()
		{
			AssertEquals("PreCondition:Dummy is not in database", false, Dummy.IsInDatabase);

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var grid = form.TabGrid;

				var gridID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID }, new string[] { gridID }, null, ZGuid.Empty);

				AssertNotEquals("Different factory", Dummy.Factory, customiseBizO.Factory);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					var gridLayoutManageable = customiseForm.GetGridLayoutManageableExposed();
					AssertNotEquals("Different factory", Dummy.Factory, gridLayoutManageable.Factory);

					new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "HUFDYUI", true, false, SaveColumnLayout.Yes);
					AssertEquals("Dummy is not in database as SaveLayout is done in a different factory", false, Dummy.IsInDatabase);
				}
			}
		}

		public void TestGetDefaultGridLayout()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				form.TabGrid.Columns.HasLayoutChanged = true;//default is saved
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				var defaultStorage = gridLayoutManageable.GetDefaultGridLayout();
				AssertNotNull(defaultStorage);
				AssertEquals(StmDataGridLayoutStorage.DefaultLayoutName, defaultStorage.ColumnLayoutName);
			}
		}

		public void TestSerialiseLayoutAndWriteTo()
		{
			StmModuleFilter descriptionVisible;
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				//should have serialised correctly
				descriptionVisible = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "DescriptionVisible", false, false, SaveColumnLayout.Ignore);
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				form.TabGrid.CurrentColumnLayout = descriptionVisible;

				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);

				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}
	}
}
