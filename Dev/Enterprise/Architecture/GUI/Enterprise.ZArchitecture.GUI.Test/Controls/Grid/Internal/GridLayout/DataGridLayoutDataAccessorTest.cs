using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DataGridLayoutDataAccessorTest : TestCaseWithDummy
	{
		class DummyFilterStripLayoutsHelper : FilterStripLayoutsHelper
		{
			protected override ZGuid GetCurrentOrganisationPk()
			{
				return CurrentOrganisationPkExposed;
			}

			public ZGuid CurrentOrganisationPkExposed { get; set; }
		}

		public void TestSaveLayoutSetsPKOnColourStrip()
		{
			var bo = new GridColourSchemeTest.FilterStripBusinessObjectForTest();

			var strip = new GridColourStripBusinessObject(bo, null, null);
			((IFilterStripBusinessObjectInternals)strip).LayoutContext = "elvis" + GridColourFactory.ColorStripCode;
			strip.RuleName = "rule1";

			var filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "rule1";
			filter.S9_ModuleID = "elvis" + GridColourFactory.ColorStripCode;
			filter.S9_RelatedEntityID = ZGuid.Empty;

			Factory.Save();

			new DataGridLayoutDataAccessor().SavePreconfiguredLayout(strip, "rule1", false, false, SaveColumnLayout.No, SaveGridColourLayout.No);
			AssertEquals(filter.PK, strip.StmModuleFilter.PK);
			var data = new BusinessObjectFactory().LoadTop1<StmModuleFilterUserData>(new ZQuery(StmModuleFilterUserDataSchema.S0_S9, filter.PK));
			AssertNotNull("StmModuleFilterUserData should have saved", data);
		}

		public void TestSavePreconfiguredLayoutForWeb()
		{
			Globals.IsWeb = true;
			var strip = new Modules.Testing.DummyFilterStripBusinessObject();
			var helper = new DummyFilterStripLayoutsHelper();
			helper.CurrentOrganisationPkExposed = Guid.NewGuid();
			strip.LayoutsHelper = helper;

			StmModuleFilter filter;

			//private
			filter = new DataGridLayoutDataAccessor().SavePreconfiguredLayoutForWeb(strip, "rule1", false, false, SaveColumnLayout.No, SaveGridColourLayout.No);
			AssertEquals(filter.S9_FilterName, "rule1");
			AssertEquals(false, filter.S9_IsPublished);
			AssertEquals(false, filter.S9_IsSystem);
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, filter.S9_RelatedEntityID);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, filter.S9_GC);

			//publish for organisation
			filter = new DataGridLayoutDataAccessor().SavePreconfiguredLayoutForWeb(strip, "rule1", true, false, SaveColumnLayout.No, SaveGridColourLayout.No);
			AssertEquals(filter.S9_FilterName, "rule1");
			AssertEquals(true, filter.S9_IsPublished);
			AssertEquals(false, filter.S9_IsSystem);
			AssertEquals(strip.LayoutsHelper.CurrentOrganisationPk, filter.S9_RelatedEntityID);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, filter.S9_GC);

			//publish company
			filter = new DataGridLayoutDataAccessor().SavePreconfiguredLayoutForWeb(strip, "rule1", true, true, SaveColumnLayout.No, SaveGridColourLayout.No);
			AssertEquals(filter.S9_FilterName, "rule1");
			AssertEquals(true, filter.S9_IsPublished);
			AssertEquals(false, filter.S9_IsSystem);
			AssertEquals(Guid.Empty, filter.S9_RelatedEntityID);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, filter.S9_GC);
		}

		public void TestPublishGlobal()
		{
			FilterStripBusinessObject strip = new GridColourSchemeTest.FilterStripBusinessObjectForTest();
			((IFilterStripBusinessObjectInternals)strip).LayoutContext = "elvis" + GridColourFactory.ColorStripCode;

			var filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "rule1";
			filter.S9_ModuleID = "elvis" + GridColourFactory.ColorStripCode;

			Factory.Save();

			filter = new DataGridLayoutDataAccessor().SavePreconfiguredLayout(strip, "rule1", false, false, SaveColumnLayout.No, SaveGridColourLayout.No);
			AssertEquals(filter.S9_FilterName, "rule1");
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, filter.S9_GC);

			filter = new DataGridLayoutDataAccessor().SavePreconfiguredLayout(strip, "rule1", true, false, SaveColumnLayout.No, SaveGridColourLayout.No);
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, filter.S9_GC);

			filter = new DataGridLayoutDataAccessor().SavePreconfiguredLayout(strip, "[rule1]", true, true, SaveColumnLayout.No, SaveGridColourLayout.No);
			AssertEquals("Empty S9_GC for globally published layout", ZGuid.Empty, filter.S9_GC);
		}

		public void TestConvertFromLegacyKeyToNewContextKey()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var serialiser = new DataGridLayoutDataSetSerialiser();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var layout = Factory.New<StmModuleFilter>();
				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					layout.S9_ColumnLayoutData = layoutStream.ToArray();
					layout.S9_FilterName = "DescriptionVisible";

					//saved with a legacy
					layout.S9_ModuleID = new LegacyDataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					layout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
					layout.S9_SaveColumnLayout = true;
					layout.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
					Factory.Save();
				}

				form.TabGrid.CurrentColumnLayout = layout;
				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("CurrentColumnLayoutName should have been calculated correctly", "DescriptionVisible", form.TabGrid.CurrentColumnLayout.ColumnLayoutName);

				AssertEquals("Number column should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Desc column should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				var legacyKey = new LegacyDataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				AssertNull("Layout saved with a legacy key is converted", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(legacyKey, "DescriptionVisible", false));

				var newContextKey = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				AssertNotNull("Layout saved with a legacy key is converted", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(newContextKey, "DescriptionVisible", false));
			}
		}

		public void TestTwoLayoutsWithSameNameForDifferentUsers()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var serialiser = new DataGridLayoutDataSetSerialiser();

				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					var layoutForOtherUser = Factory.New<StmModuleFilter>();

					layoutForOtherUser.S9_ColumnLayoutData = layoutStream.ToArray();
					layoutForOtherUser.S9_FilterName = "TEST";
					layoutForOtherUser.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					layoutForOtherUser.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
					layoutForOtherUser.S9_SaveColumnLayout = true;
					layoutForOtherUser.S9_RelatedEntityID = ZGuid.NewZGuid();
				}

				var layoutForCurrentUser = Factory.New<StmModuleFilter>();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					layoutForCurrentUser.S9_ColumnLayoutData = layoutStream.ToArray();
					layoutForCurrentUser.S9_FilterName = "TEST";
					layoutForCurrentUser.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					layoutForCurrentUser.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
					layoutForCurrentUser.S9_SaveColumnLayout = true;
					layoutForCurrentUser.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				}

				var currentlySelectedData = Factory.New<StmData>();
				currentlySelectedData.SD_Name = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				currentlySelectedData.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedData.SD_GuidValue = layoutForCurrentUser.PK;
				form.TabGrid.Columns.HasLayoutChanged = false;

				Factory.Save();
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestGetUserLayoutSetting()
		{
#pragma warning disable CW1108 // Do Not Use DataSet
			DataSet lastSavedColumnSetting = null;
#pragma warning restore CW1108 // Do Not Use DataSet

			var newWidth = 0;
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("PreCondition:Z0_Number is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("PreCondition:Z0_Description is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width += 10;
				newWidth = form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width;

				new DataGridLayoutManager().SaveDefaultLayout(form.TabGrid);

				lastSavedColumnSetting = new DataGridLayoutDataAccessor().GetUserLayoutSetting(form.TabGrid);

				form.TabGrid.Columns.HasLayoutChanged = false;
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				using (var layoutStream = new DataGridLayoutDataSetSerialiser().GetLayoutStreamFromDataSet(lastSavedColumnSetting))
				{
					new TestDataGridLayoutManager().LoadLayout(form.TabGrid, layoutStream);
				}

				AssertEquals("Z0_Number should not be visible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Z0_Description is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertEquals("Width of Description should have been serialised correctly", newWidth, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width);
			}
		}

		public void TestGetLastSavedLayoutStorage()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				//Last saved column layout was 'Last Customised'
				var currentlySelectedData = Factory.New<StmData>();
				currentlySelectedData.SD_Name = new LegacyDataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData;
				currentlySelectedData.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedData.SD_BinaryValue = ZBlob.FromAscii("TEST0");

				var currentlySelectedDataForDiffUser = Factory.New<StmData>();
				currentlySelectedDataForDiffUser.SD_Name = new LegacyDataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData;
				currentlySelectedDataForDiffUser.SD_Owner = ZGuid.NewZGuid();
				currentlySelectedDataForDiffUser.SD_BinaryValue = ZBlob.FromAscii("TEST0");

				//users set up two configurations
				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_ColumnLayoutData = ZBlob.FromAscii("TEST1");
				preConfiguredLayout.S9_FilterName = "TEST1";
				preConfiguredLayout.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				preConfiguredLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayout.S9_SaveColumnLayout = true;
				preConfiguredLayout.S9_SaveGridColourLayout = true;
				preConfiguredLayout.S9_GridColourLayoutID = ZGuid.NewZGuid();

				Factory.Save();

				new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid);
				AssertEquals("ColumnLayout is saved into StmData as users have not specified a preConfigured layout name", StmDataGridLayoutStorage.DefaultLayoutName, new DataGridLayoutDataAccessor().GetLastSavedLayoutStorage(Factory, form.TabGrid).ColumnLayoutName);

				form.TabGrid.CurrentColumnLayout = preConfiguredLayout;
				new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid);
				AssertEquals("ColumnLayout is now in StmModuleFilter", preConfiguredLayout, new DataGridLayoutDataAccessor().GetLastSavedLayoutStorage(Factory, form.TabGrid));
			}
		}

		public void TestSaveDataGridUserLayout()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				//Last saved column layout was 'Default'
				var currentlySelectedData = Factory.New<StmData>();
				currentlySelectedData.SD_Name = new LegacyDataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData;
				currentlySelectedData.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedData.SD_BinaryValue = ZBlob.FromAscii("TEST0");

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_ColumnLayoutData = ZBlob.FromAscii("TEST1");
				preConfiguredLayout.S9_FilterName = "TEST1";
				preConfiguredLayout.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				preConfiguredLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayout.S9_SaveColumnLayout = true;

				Factory.Save();

				form.TabGrid.CurrentColumnLayout = null;
				new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid);

				var factory2 = new BusinessObjectFactory();
				currentlySelectedData = factory2.Load<StmData>(currentlySelectedData.PK);
				AssertNotEquals("Key is updated", new LegacyDataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData, currentlySelectedData.SD_Name);
				AssertNotEquals("SD_BinaryValue should have been updated", ZBlob.FromAscii("TEST0"), currentlySelectedData.SD_BinaryValue);
				AssertEquals("SD_GuidValue should not point to any StmModuleFilter", ZGuid.Empty, currentlySelectedData.SD_GuidValue);

				form.TabGrid.CurrentColumnLayout = preConfiguredLayout;
				new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid);

				currentlySelectedData = factory2.Load<StmData>(currentlySelectedData.PK);
				AssertNotEquals("SD_BinaryValue should not have been cleared out as it will be used as Default layout", ZBlob.Empty, currentlySelectedData.SD_BinaryValue);
				AssertEquals("Instead it points to StmModuleFilter", preConfiguredLayout.PK, currentlySelectedData.SD_GuidValue);
			}
		}

		public void TestSaveDataGridUserLayoutSavesColumnDataCorrectly()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_FilterName = "TESTFILTER123";
				preConfiguredLayout.S9_SaveColumnLayout = true;
				preConfiguredLayout.S9_ColumnLayoutData = null;

				form.TabGrid = new GridLayoutConfigurableFalse(form.TabGrid.DataSource);
				form.TabGrid.CurrentColumnLayout = preConfiguredLayout;

				Factory.Save();

				var layout = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, preConfiguredLayout.S9_FilterName));
				AssertEquals("Shouldn't have any column layout data.", ZBlob.Empty, layout.S9_ColumnLayoutData);

				new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid);

				var factory2 = new BusinessObjectFactory();
				layout = factory2.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, preConfiguredLayout.S9_FilterName));

				AssertNotEquals("Should have saved column layout data.", ZBlob.Empty, layout.S9_ColumnLayoutData);

				// GridLayoutConfigurableFalse grid for test doesn't dispose by itself.
				form.TabGrid.Dispose();
			}
		}

		public void TestColourLayoutSavePreconfiguredLayout()
		{
			var bo = new GridColourSchemeTest.FilterStripBusinessObjectForTest();

			var strip = new GridColourStripBusinessObject(bo, null, null);
			((IFilterStripBusinessObjectInternals)strip).LayoutContext = "elvis" + GridColourFactory.ColorStripCode;
			strip.RuleName = "rule1";

			var filter = Factory.New<StmModuleFilter>();
			filter.S9_FilterName = "rule1";
			filter.S9_ModuleID = "elvis" + GridColourFactory.ColorStripCode;
			filter.S9_RelatedEntityID = ZGuid.Empty;

			Factory.Save();

			new DataGridLayoutDataAccessor().SavePreconfiguredLayout(strip, "rule1", false, false, SaveColumnLayout.No, SaveGridColourLayout.Yes);
			AssertEquals(filter.PK, strip.StmModuleFilter.PK);
			var data = new BusinessObjectFactory().LoadTop1<StmModuleFilterUserData>(new ZQuery(StmModuleFilterUserDataSchema.S0_S9, filter.PK));
			AssertEquals("S9_SaveGridColourLayout should have saved", true, data.Layout.S9_SaveGridColourLayout);
		}

		public void TestGetLastSavedGridColourStorage()
		{
			var scheme1 = Factory.New<GridColourScheme>();
			scheme1.S9_FilterName = "xyz";
			scheme1.S9_ModuleID = "shipment_CS";
			Factory.Save();

			var result = new DataGridLayoutDataAccessor().GetLastSavedGridColourStorage(Factory, scheme1.PK);
			AssertNotNull(result);
		}

		public void TestGridColourLayoutSavesDataCorrectly()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid = new GridLayoutConfigurableFalse(form.TabGrid.DataSource);

				var scheme1 = Factory.New<GridColourScheme>();
				scheme1.S9_FilterName = "xyz";
				scheme1.S9_ModuleID = "shipment_CS";

				((GridLayoutConfigurableFalse)form.TabGrid).SchemeForTest = scheme1;

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_FilterName = "TESTFILTER123";
				preConfiguredLayout.S9_SaveColumnLayout = true;
				preConfiguredLayout.S9_ColumnLayoutData = null;
				preConfiguredLayout.S9_SaveGridColourLayout = true;
				Factory.Save();

				form.TabGrid.CurrentColumnLayout = preConfiguredLayout;

				var factory1 = new BusinessObjectFactory();
				var layout1 = factory1.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, preConfiguredLayout.S9_FilterName));
				AssertEquals("S9_GridColourLayoutID should be empty.", ZGuid.Empty, layout1.S9_GridColourLayoutID);

				new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid);

				var factory2 = new BusinessObjectFactory();
				var layout2 = factory2.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, preConfiguredLayout.S9_FilterName));
				AssertEquals("Should have saved colour layout data.", scheme1.PK, layout2.S9_GridColourLayoutID);

				// GridLayoutConfigurableFalse grid for test doesn't dispose by itself.
				form.TabGrid.Dispose();
			}
		}

		public void TestSaveGridLayoutWhenSupersetIsCurrentlySelected()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = Guid.NewGuid();

				//Last saved column layout was 'Last Customised'
				var currentlySelectedData = Factory.New<StmData>();
				currentlySelectedData.SD_Name = new LegacyDataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData;
				currentlySelectedData.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedData.SD_BinaryValue = ZBlob.FromAscii("TEST0");
				currentlySelectedData.SD_DepartmentGuid = form.TabGrid.LayoutCategoryPK;

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_ColumnLayoutData = ZBlob.FromAscii("TEST1");
				preConfiguredLayout.S9_FilterName = "TEST1";
				preConfiguredLayout.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter;
				preConfiguredLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayout.S9_SaveColumnLayout = true;

				var preConfiguredLayoutSuperset = Factory.New<StmModuleFilter>();
				preConfiguredLayoutSuperset.S9_ColumnLayoutData = ZBlob.FromAscii("TEST2");
				preConfiguredLayoutSuperset.S9_FilterName = "TEST2";
				preConfiguredLayoutSuperset.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				preConfiguredLayoutSuperset.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayoutSuperset.S9_SaveColumnLayout = true;

				Factory.Save();

				form.TabGrid.CurrentColumnLayout = preConfiguredLayoutSuperset;
				new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid);

				var factory2 = new BusinessObjectFactory();
				currentlySelectedData = factory2.Load<StmData>(currentlySelectedData.PK);
				AssertEquals("It should point to StmModuleFilter whose name is 'TEST2'. It should find the layout even if it is a superset", preConfiguredLayoutSuperset.PK, currentlySelectedData.SD_GuidValue);
			}
		}

		public void TestGetSortedAllLayouts()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_FilterName = "B";
				preConfiguredLayout.S9_ModuleID = gridID;
				preConfiguredLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayout.S9_SaveColumnLayout = true;

				var preConfiguredPublishedLayout = Factory.New<StmModuleFilter>();
				preConfiguredPublishedLayout.S9_FilterName = "D";
				preConfiguredPublishedLayout.S9_ModuleID = gridID;
				preConfiguredPublishedLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredPublishedLayout.S9_SaveColumnLayout = true;
				preConfiguredPublishedLayout.S9_IsPublished = true;

				var preConfiguredPublishedLayout2 = Factory.New<StmModuleFilter>();
				preConfiguredPublishedLayout2.S9_FilterName = "C";
				preConfiguredPublishedLayout2.S9_ModuleID = gridID;
				preConfiguredPublishedLayout2.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredPublishedLayout2.S9_SaveColumnLayout = true;
				preConfiguredPublishedLayout2.S9_IsPublished = true;

				preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_FilterName = "A";
				preConfiguredLayout.S9_ModuleID = gridID;
				preConfiguredLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayout.S9_SaveColumnLayout = true;

				var currentlySelectedData = Factory.New<StmData>();

				currentlySelectedData.SD_Name = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData;
				currentlySelectedData.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedData.SD_GuidValue = preConfiguredLayout.PK;

				var layouts = new List<IGridLayoutStorage>(new DataGridLayoutDataAccessor().GetSortedAllLayoutsForFormGrid(Factory, form.TabGrid));
				AssertEquals("StmData does not have any BinaryValue therefore it is not a layoutStorage as such", 4, layouts.Count);
				//should have been sorted
				AssertEquals("C", layouts[0].ColumnLayoutName);
				AssertEquals("D", layouts[1].ColumnLayoutName);
				AssertEquals("A", layouts[2].ColumnLayoutName);
				AssertEquals("B", layouts[3].ColumnLayoutName);

				currentlySelectedData.SD_GuidValue = ZGuid.Empty;
				currentlySelectedData.SD_BinaryValue = ZBlob.FromAscii("JKldf789345hjk");
				layouts = new List<IGridLayoutStorage>(new DataGridLayoutDataAccessor().GetSortedAllLayoutsForFormGrid(Factory, form.TabGrid));
				AssertEquals("StmData has its own BinaryValue, which happens when users start to customise columns", 5, layouts.Count);
				AssertEquals(StmDataGridLayoutStorage.DefaultLayoutName, layouts[4].ColumnLayoutName);
			}
		}

		public void TestLayoutCacheWorksForGridWithAndWithoutLayoutCategoryPK()
		{
			var layoutCategoryPK = Guid.NewGuid();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				//in Customs, this is set to be an organisation if the organisation has Custom Attribute configuration for the current grid
				//if users remove the configuration against organisation, ZGrid.LayoutCategoryPK is removed.
				form.TabGrid.LayoutCategoryPK = layoutCategoryPK;

				AssertEquals("PreCondition:Z0_Number is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("PreCondition:Z0_Description is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				form.TabGrid.Columns.HasLayoutChanged = true;//should save Default layout with layoutCategoryPK
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.LayoutCategoryPK = layoutCategoryPK;
				form.Show();

				AssertEquals("Number field should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Desc field should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.LayoutCategoryPK = Guid.Empty;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;
				form.TabGrid.Columns.HasLayoutChanged = true;//should save Default layout without layoutCategoryPK
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("Number field should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Desc field should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestGetSortedAllLayoutsIncludingSupersets()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = Guid.NewGuid();

				var gridIDSuperset = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter;

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_FilterName = "A";
				preConfiguredLayout.S9_ModuleID = gridID;
				preConfiguredLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayout.S9_SaveColumnLayout = true;

				var preConfiguredSuperset = Factory.New<StmModuleFilter>();
				preConfiguredSuperset.S9_FilterName = "B";
				preConfiguredSuperset.S9_ModuleID = gridIDSuperset;
				preConfiguredSuperset.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredSuperset.S9_SaveColumnLayout = true;

				var currentlySelectedData = Factory.New<StmData>();
				currentlySelectedData.SD_Name = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmData;
				currentlySelectedData.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedData.SD_GuidValue = preConfiguredLayout.PK;
				currentlySelectedData.SD_DepartmentGuid = form.TabGrid.LayoutCategoryPK;

				var result = new List<IGridLayoutStorage>(new DataGridLayoutDataAccessor().GetSortedAllLayoutsForFormGrid(Factory, form.TabGrid));
				AssertEquals("two layouts expected", 2, result.Count);
				AssertEquals("should contain 'A'", "A", result[0].ColumnLayoutName);
				AssertEquals("should contain 'B'", "B", result[1].ColumnLayoutName);

				form.TabGrid.LayoutCategoryPK = Guid.NewGuid();
				result = new List<IGridLayoutStorage>(new DataGridLayoutDataAccessor().GetSortedAllLayoutsForFormGrid(Factory, form.TabGrid));
				AssertEquals("one layout expected", 1, result.Count);
				AssertEquals("should contain 'B' as it is a superset layout", "B", result[0].ColumnLayoutName);
			}
		}

		public void TestLoadUserLayoutWhenLastCustomisedWasASuperset()
		{
			var layoutCategoryPK = Guid.NewGuid();

			var descriptionVisible = Factory.New<StmModuleFilter>();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = layoutCategoryPK;

				var serialiser = new DataGridLayoutDataSetSerialiser();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				//saved with a key with LayoutCategoryPK
				descriptionVisible.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid, true).ContextKeyForStmModuleFilter;
				descriptionVisible.S9_ColumnLayoutData = serialiser.GetLayoutStream(form.TabGrid.Columns).ToArray();
				descriptionVisible.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				descriptionVisible.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				descriptionVisible.S9_FilterName = "DescriptionVisible";
				descriptionVisible.S9_SaveColumnLayout = true;

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				//saved with a key without LayoutCategoryPK. It is a superset
				var bothVisible = Factory.New<StmModuleFilter>();
				bothVisible.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				bothVisible.S9_ColumnLayoutData = serialiser.GetLayoutStream(form.TabGrid.Columns).ToArray();
				bothVisible.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				bothVisible.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				bothVisible.S9_FilterName = "BothVisible";
				bothVisible.S9_SaveColumnLayout = true;

				Factory.Save();

				form.TabGrid.CurrentColumnLayout = bothVisible;
				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = layoutCategoryPK;
				AssertEquals("CurrentColumnLayout", "BothVisible", form.TabGrid.CurrentColumnLayout.ColumnLayoutName);

				form.TabGrid.LoadUserLayoutSettings();
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.CurrentColumnLayout = descriptionVisible;
				form.TabGrid.LoadUserLayoutSettings();
				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = layoutCategoryPK;
				AssertEquals("CurrentColumnLayout", "DescriptionVisible", form.TabGrid.CurrentColumnLayout.ColumnLayoutName);

				form.TabGrid.LoadUserLayoutSettings();
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestGetCurrentUserLayoutStorageWhenDefaultIsSelected()
		{
			var layout = Factory.New<StmModuleFilter>();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var serialiser = new DataGridLayoutDataSetSerialiser();

				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					layout.S9_ColumnLayoutData = layoutStream.ToArray();
					layout.S9_FilterName = "TEST";
					layout.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					layout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
					layout.S9_SaveColumnLayout = true;
					layout.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
					Factory.Save();
				}

				//will save as a Default
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;
				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				//default is showing
				AssertEquals(true, StmDataGridLayoutStorage.IsDefaultLayout(form.TabGrid.CurrentColumnLayout.ColumnLayoutName));
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.CurrentColumnLayout = layout;
				form.TabGrid.LoadUserLayoutSettings();
				AssertEquals("should have loaded according to the saved layout", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("should have loaded according to the saved layout", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("TEST", form.TabGrid.CurrentColumnLayout.ColumnLayoutName);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.CurrentColumnLayout = null;
				form.TabGrid.LoadUserLayoutSettings();
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestConcurrencyFailure()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				using (var tempGrid = new GridLayoutConfigurableFalse(form.TabGrid.DataSource))
				{
					form.TabGrid = tempGrid;
					var filter1 = factory1.NewWithValidTestData<StmModuleFilter>();
					filter1.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					filter1.S9_SaveColumnLayout = true;
					filter1.S9_SaveGridColourLayout = true;
					form.TabGrid.CurrentColumnLayout = filter1;
					factory1.Save();

					var filter2 = factory2.Load<StmModuleFilter>(filter1.PK);
					filter2.S9_GridColourLayoutID = ZGuid.NewZGuid();
					factory2.Save();

					filter1.S9_GridColourLayoutID = ZGuid.NewZGuid();

					AssertNoExceptionThrown(() => new DataGridLayoutDataAccessor().SaveDataGridDefaultColumnLayout(form.TabGrid));
				}
			}
		}
	}
}
