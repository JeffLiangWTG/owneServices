using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DataGridColumnLayoutTest : TestCaseWithDummy
	{
		public void TestLoadAndSaveUserLayoutSettings()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;
				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("Description should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				AssertEquals("Description should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestLoadAndSaveUserLayoutSettings_NewColumnInAnAlreadyVisibleGroup()
		{
			// Default ZTestGridForm has only Number and Description columns. So need to add more columns, or remove existing ones.
			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Number, new ResourceStringData("", "Triple"));

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Decimal);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Decimal, new ResourceStringData("", "Triple"));

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Code);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Code, new ResourceStringData("", "Lonely"));

				form.TabGrid.SetColumnVisible(true, [DummyBizoSchema.Constants.Z0_Number, DummyBizoSchema.Constants.Z0_Decimal, DummyBizoSchema.Constants.Z0_Code]);

				form.Show();

				form.TabGrid.Columns.Remove(DummyBizoSchema.Constants.Z0_Description);

				form.TabGrid.Columns.HasLayoutChanged = true;

				AssertOrderOfGrid("Pre-condition: Order is correct", form.TabGrid,
					DummyBizoSchema.Constants.Z0_Number,
					DummyBizoSchema.Constants.Z0_Decimal,
					DummyBizoSchema.Constants.Z0_Code);
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Number, new ResourceStringData("", "Triple"));

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Decimal);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Decimal, new ResourceStringData("", "Triple"));

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Code);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Code, new ResourceStringData("", "Lonely"));

				form.TabGrid.SetColumnVisible(true, [DummyBizoSchema.Constants.Z0_Number, DummyBizoSchema.Constants.Z0_Decimal, DummyBizoSchema.Constants.Z0_Code]);

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Description);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Description, new ResourceStringData("", "Triple"));
				form.TabGrid.SetColumnVisible(false, DummyBizoSchema.Constants.Z0_Description); // audit columns are by default set to false in visibility

				form.Show();

				AssertEquals("Pre-condition: Number is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Pre-condition: Decimal is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Decimal].IsVisible);
				AssertEquals("Description should be visible because its group-columns, Number and Decimal, are visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertOrderOfGrid("Order is correct - new group columns are at the end of existing group-columns", form.TabGrid,
					DummyBizoSchema.Constants.Z0_Number,
					DummyBizoSchema.Constants.Z0_Decimal,
					DummyBizoSchema.Constants.Z0_Description,
					DummyBizoSchema.Constants.Z0_Code);
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();
		}

		public void TestLoadAndSaveUserLayoutSettings_NewColumnReplacesOldColumnsInSameGroup()
		{
			// Default ZTestGridForm has only Number and Description columns. So need to add more columns, or remove existing ones.
			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Number, new ResourceStringData("", "Group"));

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Decimal);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Decimal, new ResourceStringData("", "Group"));

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Code);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Code, new ResourceStringData("", "Group"));

				form.TabGrid.SetColumnVisible(true, [DummyBizoSchema.Constants.Z0_Number, DummyBizoSchema.Constants.Z0_Decimal, DummyBizoSchema.Constants.Z0_Code]);

				form.Show();

				form.TabGrid.Columns.Remove(DummyBizoSchema.Constants.Z0_Description);

				form.TabGrid.Columns.HasLayoutChanged = true;

				AssertOrderOfGrid("Pre-condition: Order is correct", form.TabGrid,
					DummyBizoSchema.Constants.Z0_Number,
					DummyBizoSchema.Constants.Z0_Decimal,
					DummyBizoSchema.Constants.Z0_Code);
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Number, new ResourceStringData("", "Group"));
				form.TabGrid.SetColumnVisible(true, DummyBizoSchema.Constants.Z0_Number);

				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_Description);
				form.TabGrid.SetColumnGroupName(DummyBizoSchema.Constants.Z0_Description, new ResourceStringData("", "Group"));
				form.TabGrid.SetColumnVisible(false, DummyBizoSchema.Constants.Z0_Description); // audit columns are by default set to false in visibility

				form.Show();

				AssertEquals("Number is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Description should be visible because its remaining group-column, Number, is visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertOrderOfGrid("Order is correct - new group columns are at the end of existing group-columns", form.TabGrid,
					DummyBizoSchema.Constants.Z0_Number,
					DummyBizoSchema.Constants.Z0_Description);
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();
		}

		void AssertOrderOfGrid(string message, ZGrid grid, params string[] columnNames)
		{
			AssertEquals("Column count", columnNames.Length, grid.Columns.Count);
			for (var i = 0; i < columnNames.Length; i++)
			{
				AssertEquals(string.Concat(message, " - Column ", i), columnNames[i], grid.Columns[i].ColumnName);
			}
		}

		public void TestCurrentColumnLayoutName()
		{
			var tool = new DataGridLayoutDataSetSerialiser();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("CurrentColumnLayoutName", StmDataGridLayoutStorage.DefaultLayoutName, form.TabGrid.CurrentColumnLayout.ColumnLayoutName);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				using (var layoutStream = tool.GetLayoutStreamFromDataSet(tool.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					preConfiguredLayout.S9_ColumnLayoutData = layoutStream.ToArray();
				}
				preConfiguredLayout.S9_FilterName = "OnlyDescriptionVisible";
				preConfiguredLayout.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				preConfiguredLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				preConfiguredLayout.S9_SaveColumnLayout = true;

				Factory.Save();//StmModuleFilter will be accessed by a different factory

				form.TabGrid.CurrentColumnLayout = preConfiguredLayout;
				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("OnlyDescriptionVisible", form.TabGrid.CurrentColumnLayout.ColumnLayoutName);

				AssertEquals("IT should have updated columns", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertEquals("IT should have updated columns", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
			}
		}

		public void TestLoadUserLayoutSettingsAccordingToZGridCurrentColumnLayoutName()
		{
			StmModuleFilter filter1;
			StmModuleFilter filter2;
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				filter1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "DescriptionVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				filter2 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "NumberVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				form.TabGrid.CurrentColumnLayout = filter1;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);

				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertEquals("visible columns first", true, form.TabGrid.Columns[0].IsVisible);
				AssertEquals("hidden columns last", false, form.TabGrid.Columns[1].IsVisible);

				form.TabGrid.CurrentColumnLayout = filter2;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertEquals("visible columns first", true, form.TabGrid.Columns[0].IsVisible);
				AssertEquals("hidden columns last", false, form.TabGrid.Columns[1].IsVisible);
			}
		}

		public void TestLoadUserLayoutSettings_VisibleColumnsFirst()
		{
			StmModuleFilter filter1;
			StmModuleFilter filter2;
			using (var form = new ZTestGridForm(Dummy))
			{
				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_NVarCharMax).IsVisible = false;
				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_NVarChar);

				form.Show();

				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
				AssertEquals("visible columns", DummyBizoSchema.Constants.Z0_Description, form.TabGrid.Columns[0].ColumnName);
				AssertEquals("visible columns", DummyBizoSchema.Constants.Z0_Number, form.TabGrid.Columns[1].ColumnName);
				AssertEquals("visible columns", DummyBizoSchema.Constants.Z0_NVarChar, form.TabGrid.Columns[2].ColumnName);
				AssertEquals("hidden columns", DummyBizoSchema.Constants.Z0_NVarCharMax, form.TabGrid.Columns[3].ColumnName);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				filter1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "DescriptionVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				filter2 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "NumberVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_NVarCharMax).IsVisible = false;
				AddColumn(form.TabGrid, DummyBizoSchema.Constants.Z0_NVarChar);

				form.Show();

				form.TabGrid.CurrentColumnLayout = filter1;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);

				AssertEquals("visible columns", DummyBizoSchema.Constants.Z0_Description, form.TabGrid.Columns[0].ColumnName);
				AssertEquals("visible columns", DummyBizoSchema.Constants.Z0_NVarChar, form.TabGrid.Columns[1].ColumnName);
				AssertEquals("hidden columns", DummyBizoSchema.Constants.Z0_Number, form.TabGrid.Columns[2].ColumnName);
				AssertEquals("hidden columns", DummyBizoSchema.Constants.Z0_NVarCharMax, form.TabGrid.Columns[3].ColumnName);

				form.TabGrid.CurrentColumnLayout = filter2;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
				AssertEquals("visible columns", DummyBizoSchema.Constants.Z0_Number, form.TabGrid.Columns[0].ColumnName);
				AssertEquals("visible columns", DummyBizoSchema.Constants.Z0_NVarChar, form.TabGrid.Columns[1].ColumnName);
				AssertEquals("hidden columns", DummyBizoSchema.Constants.Z0_Description, form.TabGrid.Columns[2].ColumnName);
				AssertEquals("hidden columns", DummyBizoSchema.Constants.Z0_NVarCharMax, form.TabGrid.Columns[3].ColumnName);
			}
		}

		ZTextBoxColumnStyleInfo AddColumn(ZGrid grid, string name)
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo1.ColumnName = name;
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			return zTextBoxColumnStyleInfo1;
		}

		public void TestLoadUserLayoutSettings_RestoreCustomFields()
		{
			StmModuleFilter filter1;

			using (var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>())
			{
				var customOld = dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
				addCustomField(("Rtfield@ +.1", "INT"));
				addCustomField(("Rtfield@ +.1", "STR"));

				// Add some collisions with legacy naming
				addCustomField(("Rtfield1", "STR"));
				addCustomField(("Rt field 1", "STR"));

				var workflowCustomFieldsGridInitializer = ObjectFactory.Get<IWorkflowCustomFieldsGridInitializer>();
				using (var form = new ZTestGridForm(customOld))
				{
					var dummyCollection = new DummyBusinessObjectCollection(Factory);
					dummyCollection.Add(customOld);
					workflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(form.TabGrid, dummyCollection, "DUM", true, true);

					form.Show();
					new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
					var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
					filter1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "AllHidden", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);
				}
			}

			using (var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>())
			{
				var customNew = dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
				addCustomField(("Rtfield@ +.1", "INT"));
				addCustomField(("Rtfield@ +.1", "STR"));
				addCustomField(("Rtfield1", "STR"));
				addCustomField(("Rt field 1", "STR"));
				var newCustomFieldCode1 = getCustomFieldIdentifier(("Rtfield@ +.1", "INT"));
				var newCustomFieldCode2 = getCustomFieldIdentifier(("Rtfield@ +.1", "STR"));
				var newCustomFieldCode3 = getCustomFieldIdentifier(("Rtfield1", "STR"));
				var newCustomFieldCode4 = getCustomFieldIdentifier(("Rt field 1", "STR"));

				var workflowCustomFieldsGridInitializer = ObjectFactory.Get<IWorkflowCustomFieldsGridInitializer>();
				using (var form = new ZTestGridForm(customNew))
				{
					var dummyCollection = new DummyBusinessObjectCollection(Factory);
					dummyCollection.Add(customNew);
					workflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(form.TabGrid, dummyCollection, "DUM", false, true);
					form.Show();
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode3).IsVisible);
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode4).IsVisible);
					form.TabGrid.CurrentColumnLayout = filter1;
					new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode3).IsVisible);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode4).IsVisible);
				}
			}
		}

		public void TestLoadUserLayoutSettings_RestoreOldCustomFormat1()
		{
			StmModuleFilter filter1;

			var customFieldCode1 = "__RTFIELD1__prop__ZInt";
			var customFieldCode2 = "__rtfield1__prop__ZString";

			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.ColumnStyles.Clear();
				AddColumn(form.TabGrid, customFieldCode1).IsVisible = true;
				AddColumn(form.TabGrid, customFieldCode2).IsVisible = true;
				form.Show();
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				filter1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "AllVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);
			}

			using (var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>())
			{
				var customNew = dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
				addCustomField(("Rtfield@ +.1", "INT"));
				addCustomField(("Rtfield@ +.1", "STR"));
				var newCustomFieldCode1 = getCustomFieldIdentifier(("Rtfield@ +.1", "INT"));
				var newCustomFieldCode2 = getCustomFieldIdentifier(("Rtfield@ +.1", "STR"));

				var workflowCustomFieldsGridInitializer = ObjectFactory.Get<IWorkflowCustomFieldsGridInitializer>();
				using (var form = new ZTestGridForm(customNew))
				{
					var dummyCollection = new DummyBusinessObjectCollection(Factory);
					dummyCollection.Add(customNew);
					workflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(form.TabGrid, dummyCollection, "DUM", false, true);
					form.Show();
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);
					form.TabGrid.CurrentColumnLayout = filter1;
					new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);
				}
			}
		}

		public void TestLoadUserLayoutSettings_RestoreOldCustomFormat2()
		{
			StmModuleFilter filter1;

			var customFieldCode1 = "__RTFIELD@ 加点1__prop__ZInt";
			var customFieldCode2 = "__rtfield@_加点1__prop__ZString";

			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.ColumnStyles.Clear();
				AddColumn(form.TabGrid, customFieldCode1).IsVisible = true;
				AddColumn(form.TabGrid, customFieldCode2).IsVisible = true;
				form.Show();
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				filter1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "AllVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);
			}

			using (var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>())
			{
				var customNew = dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
				addCustomField(("Rtfield@ +.1", "INT"));
				addCustomField(("Rtfield@ +.1", "STR"));
				var newCustomFieldCode1 = getCustomFieldIdentifier(("Rtfield@ +.1", "INT"));
				var newCustomFieldCode2 = getCustomFieldIdentifier(("Rtfield@ +.1", "STR"));

				var workflowCustomFieldsGridInitializer = ObjectFactory.Get<IWorkflowCustomFieldsGridInitializer>();
				using (var form = new ZTestGridForm(customNew))
				{
					var dummyCollection = new DummyBusinessObjectCollection(Factory);
					dummyCollection.Add(customNew);
					workflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(form.TabGrid, dummyCollection, "DUM", false, true);
					form.Show();
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
					AssertEquals(false, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);
					form.TabGrid.CurrentColumnLayout = filter1;
					new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
					AssertEquals(true, form.TabGrid.Columns.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);
				}
			}
		}

		public void TestLoadUserLayoutSettings_CustomizeColumnsInGrid()
		{
			StmModuleFilter filter1;
			StmModuleFilter filter2;

			var customFieldCode1 = "__RTFIELD@ 加点1__prop__ZInt";
			var customFieldCode2 = "__rtfield@_加点1__prop__ZString";

			using (var form = new ZTestGridForm(Dummy))
			{
				form.TabGrid.ColumnStyles.Clear();
				AddColumn(form.TabGrid, customFieldCode1).IsVisible = true;
				AddColumn(form.TabGrid, customFieldCode2).IsVisible = true;
				form.Show();

				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				filter1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "AllVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);

				foreach (var column in form.TabGrid.Columns)
				{
					column.IsVisible = false;
				}

				filter2 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "AllHidden", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);
			}

			using (var dummyCustomFactory = ObjectFactory.Get<IDummyWithCustomFieldsFactory>())
			{
				var customNew = dummyCustomFactory.GetDummyWithCustomFields(Factory, out var addCustomField, out var setCustomField, out var getCustomField, out var getCustomFieldIdentifier);
				addCustomField(("Rtfield@ +.1", "INT"));
				addCustomField(("Rtfield@ +.1", "STR"));
				var newCustomFieldCode1 = getCustomFieldIdentifier(("Rtfield@ +.1", "INT"));
				var newCustomFieldCode2 = getCustomFieldIdentifier(("Rtfield@ +.1", "STR"));

				var workflowCustomFieldsGridInitializer = ObjectFactory.Get<IWorkflowCustomFieldsGridInitializer>();
				using (var form = new ZTestGridForm(customNew))
				{
					var dummyCollection = new DummyBusinessObjectCollection(Factory);
					dummyCollection.Add(customNew);
					workflowCustomFieldsGridInitializer.AddWorkflowCustomFieldsColumns(form.TabGrid, dummyCollection, "DUM", false, true);
					form.Show();

					var helper = new GridLayoutContextKeyProviderHelper();
					var customiseBizObj = new ZGridCustomiseBizObj(helper.GetAllGridIDsForStmModuleFilter(form.TabGrid), helper.GetAllGridIDsForStmData(form.TabGrid), form.TabGrid.CurrentColumnLayout, form.TabGrid.LayoutCategoryPK);
					using (var gridCustomiseForm = new ZGridCustomiseTester(form.TabGrid.Columns, form.TabGrid.DefaultColumns, customiseBizObj))
					{
						gridCustomiseForm.Show();
						var visibleColumns = ZColumnsCustomise.GetVisibleColumns(Factory, filter1, form.TabGrid.Columns.Select(x => (ICustomizableColumn)x).ToList());
						AssertEquals(true, visibleColumns.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
						AssertEquals(true, visibleColumns.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);

						visibleColumns = ZColumnsCustomise.GetVisibleColumns(Factory, filter2, form.TabGrid.Columns.Select(x => (ICustomizableColumn)x).ToList());
						AssertEquals(false, visibleColumns.Any());

						customiseBizObj.CurrentLayoutNameDisplay = "AllHidden";
						customiseBizObj.CurrentLayoutNameDisplay = "AllVisible";
						gridCustomiseForm.SaveColumns();

						AssertEquals(true, gridCustomiseForm.Result.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
						AssertEquals(true, gridCustomiseForm.Result.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);

						customiseBizObj.CurrentLayoutNameDisplay = "AllHidden";
						gridCustomiseForm.SaveColumns();

						AssertEquals(false, gridCustomiseForm.Result.Single(c => c.ColumnName == newCustomFieldCode1).IsVisible);
						AssertEquals(false, gridCustomiseForm.Result.Single(c => c.ColumnName == newCustomFieldCode2).IsVisible);
					}
				}
			}
		}

		public void TestGetSortedAllLayouts()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "NumberVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				var filter = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "DescriptionVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);

				form.TabGrid.CurrentColumnLayout = filter;
				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var stmData = new StmData.Loader(Factory).LoadTop1(gridID, EnvProxy.Instance.CurrentUser.PK, CargoWise.Types.ZGuid.Empty);
				AssertNotNull("PreCondition:StmData created to indicate which layout is active", stmData);
				AssertEquals("However stmData.SD_BinayValue is empty. It is not LastCustomised", true, stmData.SD_BinaryValue.IsEmpty);

				var manager = new DataGridLayoutManager();
				var layoutStorages = new List<IGridLayoutStorage>(manager.GetSortedAllLayouts(form.TabGrid));
				AssertEquals(2, layoutStorages.Count);
				AssertEquals("DescriptionVisible", layoutStorages[0].ColumnLayoutName);
				AssertEquals("NumberVisible", layoutStorages[1].ColumnLayoutName);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				form.TabGrid.CurrentColumnLayout = null;
				form.TabGrid.Columns.HasLayoutChanged = true;
				form.TabGrid.SaveLayoutEvenIfColumnsAreUnchanged();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				var manager = new DataGridLayoutManager();
				var layoutStorages = new List<IGridLayoutStorage>(manager.GetSortedAllLayouts(form.TabGrid));
				AssertEquals("including LastCustomised layout", 3, layoutStorages.Count);
				AssertEquals(StmDataGridLayoutStorage.DefaultLayoutName, layoutStorages[2].ColumnLayoutName);
			}
		}

		#region TestRestoreSortingOnNonSortableProperty

		public void TestRestoreSortingOnNonSortableProperty()
		{
			const string gridLayout =
@"<DocumentElement>
  <OGridColumnSettings>
	<MappingName>NonSortableProperty</MappingName>
	<Width>80</Width>
	<IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridSortSettings>
	<SortPropertyName>NonSortableProperty</SortPropertyName>
	<SortDirection>0</SortDirection>
  </OGridSortSettings>
</DocumentElement>";

			using (var form = new ZForm(Dummy))
			using (var grid = new ZGrid())
			{
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("NonSortableProperty", 80) { IsVisible = true, IsSortable = true });
				form.Controls.Add(grid);
				grid.SetDataBinding(new DummyWithNonSortablePropertyCollection(Factory), "");
				form.Show();

				var layoutData = System.Text.Encoding.Default.GetBytes(gridLayout);
				AssertNoExceptionThrown(() => new TestDataGridLayoutManager().LoadLayout(grid, new MemoryStream(layoutData)));
			}
		}

		class DummyWithNonSortableProperty : DummyBusinessObject
		{
			public DummyWithNonSortableProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public NonComparableString NonSortableProperty { get; set; }
		}

		class DummyWithNonSortablePropertyCollection : BusinessObjectCollection<DummyWithNonSortableProperty>
		{
			public DummyWithNonSortablePropertyCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		struct NonComparableString
		{
			public NonComparableString(object value)
			{
				this.value = value;
			}

			readonly object value;

			public override string ToString()
			{
				return value != null ? value.ToString() : string.Empty;
			}
		}

		#endregion

		public void TestLoadGridLayoutWithNonVisibleVolumn()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				foreach (var column in form.TabGrid.Columns)
				{
					column.IsVisible = false;
				}
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				var filter1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "NonColumnVisible", false, false, SaveColumnLayout.Yes, SaveGridColourLayout.Yes);

				form.TabGrid.CurrentColumnLayout = filter1;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);

				AssertEquals("Cannot use layout 'NonColumnVisible' because there is no any visible column selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, form.TabGrid.Columns.Where(x => x.IsVisible).Count());
				foreach (DataGridColumnStyle columnStyle in form.TabGrid.TableStyles[0].GridColumnStyles)
				{
					if (columnStyle.PropertyDescriptor != null)
					{
						Assert(columnStyle is ZTextBoxColumnStyle);
					}
				}
			}
		}
	}
}
