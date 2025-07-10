using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZGridCustomiseWDummyTest : TestCaseWithDummy
	{
		public void TestUpdateLayoutViewForALayoutIncludingSuperset()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var grid = form.TabGrid;
				grid.LayoutCategoryPK = Guid.NewGuid();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var serialiser = new DataGridLayoutDataSetSerialiser();

				//saved with a key without LayoutCategoryPK
				var bothVisible = Factory.New<StmModuleFilter>();
				bothVisible.S9_ModuleID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;
				bothVisible.S9_ColumnLayoutData = serialiser.GetLayoutStream(grid.Columns).ToArray();
				bothVisible.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				bothVisible.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				bothVisible.S9_FilterName = "BothVisible";

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				//saved with a key with LayoutCategoryPK
				var descriptionVisible = Factory.New<StmModuleFilter>();
				descriptionVisible.S9_ModuleID = new DataGridLayoutContextKeyProvider(grid, true).ContextKeyForStmModuleFilter;
				descriptionVisible.S9_ColumnLayoutData = serialiser.GetLayoutStream(grid.Columns).ToArray();
				descriptionVisible.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				descriptionVisible.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				descriptionVisible.S9_FilterName = "DescriptionVisible";
				Factory.Save();

				var customiseBizO = new ZGridCustomiseBizObj(new string[] { bothVisible.S9_ModuleID, descriptionVisible.S9_ModuleID }, new string[] { descriptionVisible.S9_FilterName }, null, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();

					customiseBizO.CurrentLayout = bothVisible;
					AssertEquals("Should have updated grid view", 2, customiseForm.SelectedColumns.Count);

					customiseBizO.CurrentLayout = descriptionVisible;
					AssertEquals("Should have updated grid view", 1, customiseForm.SelectedColumns.Count);
				}
			}
		}

		public void TestResultType()
		{
			using (var form = new ZForm(Dummy))
			{
				form.Size = new Size(300, 500);

				var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) };
				form.Controls.Add(grid);

				var info = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
				grid.ColumnStyles.Add(info);

				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, grid))
				{
					customiseForm.SaveColumns();
					var result = customiseForm.Result;
					AssertEquals(typeof(ZGridColumns), result.GetType());
				}
			}
		}

		public void TestLocalizedColumnUsesFullCaption()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGrid())
			{
				form.Size = new Size(300, 500);
				form.Controls.Add(grid);
				form.Name = "TestForm";

				var info =
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = DummyBizoSchema.Constants.Z0_Code,
						CaptionResourceString = new ResourceStringData("", "123", "", "Full caption long enough to be not choosen.", ""),
						Width = 60,
						IsVisible = true
					};
				grid.ColumnStyles.Add(info);
				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				((ZGridColumnStyle)grid.Columns[DummyBizoSchema.Constants.Z0_Code].ColumnStyle).RefreshHeader();
				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, grid))
				{
					var visibleElement = customiseForm.SelectedColumns[0];

					AssertEquals(info.CaptionResourceString.ShortCaption, grid.Columns[0].ToString());
					AssertEquals(info.CaptionResourceString.Caption, visibleElement.ToString());
				}
			}
		}

		//CurrentLayoutName in CustomiseColumn form is not visible in module grids
		public void TestChangeColumnsDoesNotCauseCurrentColumnLayoutNameForModuleGrid()
		{
			using (var testForm = new ZForm())
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				grid.SetParentFilterGridModule(module);

				testForm.Controls.Add(grid);
				var codeColumnInfo = new ZTextBoxColumnStyleInfo();
				codeColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;
				grid.ColumnStyles.Add(codeColumnInfo);

				var descColumnInfo = new ZTextBoxColumnStyleInfo();
				descColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Description;
				grid.ColumnStyles.Add(descColumnInfo);

				grid.SetDataBinding(Dummy, "Collection");

				testForm.Show();
				AssertNotNull("PreCondition: Bound correctly.", grid.ListManager);

				var gridID = DummyModuleIDs.Dummy.Name;

				var gridLayoutManageable = new ZGridLayoutModification(grid, grid.Columns, Factory, null);
				var bothVisible = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "BothVisible", false, false, SaveColumnLayout.Ignore);

				grid.CurrentColumnLayout = bothVisible;
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID }, new string[] { gridID }, bothVisible, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();

					AssertEquals("Two columns should be available in CurrentColumns", 2, customiseForm.CurrentColumnsListBoxExposed.Items.Count);

					customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
					customiseForm.RemoveColumnsExposed();

					AssertNotNull("CurrentLayoutName should not clear Grid.CurrentColumnLayoutName", grid.CurrentColumnLayout);
				}
			}
		}

		public void TestPerformConversionFromLegacyKeyToControlIDKey()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var grid = form.TabGrid;
				grid.LayoutCategoryPK = Guid.NewGuid();
				grid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return false; });
				grid.QueryHasNoOtherCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return true; });

				var legacyGridID = new LegacyDataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;

				var layoutSavedWithLegacyKey = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey.S9_ModuleID = legacyGridID;
				layoutSavedWithLegacyKey.S9_FilterName = "BothVisible";
				layoutSavedWithLegacyKey.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layoutSavedWithLegacyKey.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				layoutSavedWithLegacyKey.S9_SaveColumnLayout = true;
				Factory.Save();

				var gridID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;

				//legacyGridID is passed
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID, legacyGridID }, new string[] { gridID, legacyGridID }, null, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					AssertEquals("PreCondition:Form.TabGrid has a control id", false, string.IsNullOrEmpty(grid.GridId));

					customiseForm.Show();

					AssertNull("The layout now saved with a new key", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(legacyGridID, "BothVisible", false));
					AssertNotNull("The layout now saved with a new key", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "BothVisible", false));
				}
			}
		}

		public void TestPerformConversionFromLegacyKeyForRelevantLayoutsOnly()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var grid = form.TabGrid;
				grid.LayoutCategoryPK = Guid.NewGuid();
				grid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return false; });
				grid.QueryHasNoOtherCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return true; });

				var legacyGridID = new LegacyDataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;

				var layoutSavedWithLegacyKey = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey.S9_ModuleID = legacyGridID;
				layoutSavedWithLegacyKey.S9_FilterName = "BothVisible";
				layoutSavedWithLegacyKey.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layoutSavedWithLegacyKey.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				layoutSavedWithLegacyKey.S9_SaveColumnLayout = true;
				Factory.Save();

				var gridID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;

				//legacyGridID is passed
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID, legacyGridID }, new string[] { gridID, legacyGridID }, null, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					AssertEquals("PreCondition:Form.TabGrid has a control id", false, string.IsNullOrEmpty(grid.GridId));

					customiseForm.Show();

					AssertNull("The layout now saved with a new key", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(legacyGridID, "BothVisible", false));
					AssertNotNull("The layout now saved with a new key", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "BothVisible", false));
				}
			}
		}

		public void TestPerformConversionFromLegacyKeyWhenDuplicateExists()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var grid = form.TabGrid;
				grid.LayoutCategoryPK = Guid.NewGuid();
				grid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return false; });
				grid.QueryHasNoOtherCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return true; });

				var legacyGridID = new LegacyDataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;
				var gridID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;

				var layoutSavedWithLegacyKey = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey.S9_ModuleID = legacyGridID;
				layoutSavedWithLegacyKey.S9_FilterName = "BothVisible";
				layoutSavedWithLegacyKey.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layoutSavedWithLegacyKey.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				layoutSavedWithLegacyKey.S9_SaveColumnLayout = true;

				var layoutSavedWithNewKey = Factory.New<StmModuleFilter>();
				layoutSavedWithNewKey.S9_ModuleID = gridID;
				layoutSavedWithNewKey.S9_FilterName = "BothVisible";
				layoutSavedWithNewKey.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layoutSavedWithNewKey.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
				layoutSavedWithNewKey.S9_SaveColumnLayout = true;
				Factory.Save();

				//legacyGridID is passed
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID, legacyGridID }, new string[] { gridID, legacyGridID }, null, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					AssertEquals("PreCondition:Form.TabGrid has a control id", false, string.IsNullOrEmpty(grid.GridId));

					customiseForm.Show();

					AssertNull("The layout now saved with a new key", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(legacyGridID, "BothVisible", false));
					AssertNotNull("The layout now saved with a new key", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "BothVisible (Legacy)", false));
					AssertNotNull("non-legacy layout not clobbered", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "BothVisible", false));
					AssertNull("also this didn't happen", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(legacyGridID, "BothVisible (Legacy)", false));
				}
			}
		}

		public void TestGlobalFilterStillWorksWithUpdatedKey()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				var grid = form.TabGrid;
				grid.LayoutCategoryPK = Guid.NewGuid();
				grid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return false; });
				grid.QueryHasNoOtherCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(delegate
				{ return true; });
				var legacyGridID = new LegacyDataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;
				var serialiser = new DataGridLayoutDataSetSerialiser();
				var layoutSavedWithLegacyKey = Factory.New<StmModuleFilter>();
				layoutSavedWithLegacyKey.S9_ModuleID = legacyGridID;
				layoutSavedWithLegacyKey.S9_FilterName = "BothVisible";
				layoutSavedWithLegacyKey.S9_GC = ZGuid.Empty;
				layoutSavedWithLegacyKey.S9_RelatedEntityID = ZGuid.Empty;
				layoutSavedWithLegacyKey.S9_SaveColumnLayout = true;
				layoutSavedWithLegacyKey.S9_IsPublished = true;
				layoutSavedWithLegacyKey.S9_FilterData = ZBlob.Empty;
				layoutSavedWithLegacyKey.S9_ColumnLayoutData = serialiser.GetLayoutStream(grid.Columns).ToArray();
				Factory.Save();
				var gridID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID, legacyGridID }, new string[] { gridID, legacyGridID }, null, grid.LayoutCategoryPK);
				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();
					AssertNull("Layout should work with saved form", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(legacyGridID, "BothVisible", true));
					AssertNotNull("Layout should work with saved form", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "BothVisible", true));
				}
			}
			var dummy2 = (DummyBusinessObject)Factory.New(TypeOfDummy);
			using (var form2 = new ZTestGridForm(dummy2))
			{
				form2.Show();
				var grid2 = form2.TabGrid;
				grid2.LayoutCategoryPK = Guid.NewGuid();
				var legacyGridID2 = new LegacyDataGridLayoutContextKeyProvider(grid2).ContextKeyForStmModuleFilter;
				var gridID2 = new DataGridLayoutContextKeyProvider(grid2).ContextKeyForStmModuleFilter;
				var customiseBizO2 = new ZGridCustomiseBizObj(new string[] { gridID2, legacyGridID2 }, new string[] { gridID2, legacyGridID2 }, null, grid2.LayoutCategoryPK);
				using (var customiseForm2 = new ZGridCustomiseTester(grid2.Columns, grid2.Columns, customiseBizO2))
				{
					customiseForm2.Show();
					AssertNull("Layout should work with other form", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(legacyGridID2, "BothVisible", true));
					AssertNotNull("Layout should work with other form", new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID2, "BothVisible", true));
				}
			}
		}

		public void TestInvalidCurrentLayoutNameWhenCurrentColumnChanges()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var grid = form.TabGrid;
				var gridID = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var gridLayoutManageable = new ZGridLayoutModification(grid, grid.Columns, Factory, null);
				var bothVisible = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "BothVisible", false, false, SaveColumnLayout.Ignore);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				gridLayoutManageable = new ZGridLayoutModification(grid, grid.Columns, Factory, null);
				var descriptionVisible = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "DescriptionVisible", false, false, SaveColumnLayout.Ignore);

				grid.CurrentColumnLayout = descriptionVisible;

				var customiseBizO = new ZGridCustomiseBizObj(new[] { gridID }, new[] { gridID }, null, ZGuid.Empty);
				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();

					customiseBizO.CurrentLayout = bothVisible;

					var currentColumns = customiseForm.SelectedColumns;
					var gridColumn = (ZGridColumnGroup)currentColumns[0];
					AssertEquals("Description", DummyBizoSchema.Constants.Z0_Description, gridColumn.Columns[0].ColumnStyle.MappingName);

					customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
					customiseForm.RemoveColumnsExposed();
					AssertNull("CurrentLayoutName is invalidated", customiseBizO.CurrentLayout);
					AssertEquals("Grid.CurrentLayoutName should not change until CustomiseForm is closed with DialogResult.OK", "DescriptionVisible", grid.CurrentColumnLayout.ColumnLayoutName);
					AssertEquals("CurrentColumns should only have one column", 1, customiseForm.SelectedColumns.Count);

					customiseBizO.CurrentLayout = bothVisible;
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
					customiseForm.MoveColumnDownExposed();
					AssertEquals("CurrentLayoutName is invalidated", null, customiseBizO.CurrentLayout);

					customiseBizO.CurrentLayout = bothVisible;
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 1;
					customiseForm.MoveColumnUpExposed();
					AssertNull("CurrentLayoutName is invalidated", customiseBizO.CurrentLayout);

					customiseBizO.CurrentLayout = descriptionVisible;
					customiseForm.AvailableColumnsListBoxExposed.SelectedIndex = 0;
					customiseForm.AddColumnExposed();
					AssertNull("CurrentLayoutName is invalidated", customiseBizO.CurrentLayout);
					AssertEquals("CurrentColumns should have two columns now", 2, customiseForm.SelectedColumns.Count);
				}
			}
		}

#if !WINZOR

		public void TestDragAndDropCurrentColumnsListBox()
		{
			using (var testForm = new ZForm())
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				grid.SetParentFilterGridModule(module);

				testForm.Controls.Add(grid);
				var codeColumnInfo = new ZTextBoxColumnStyleInfo();
				codeColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;
				grid.ColumnStyles.Add(codeColumnInfo);

				var descColumnInfo = new ZTextBoxColumnStyleInfo();
				descColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Description;
				grid.ColumnStyles.Add(descColumnInfo);

				var dateColumnInfo = new ZTextBoxColumnStyleInfo();
				dateColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Date;
				grid.ColumnStyles.Add(dateColumnInfo);

				var decimalColumnInfo = new ZTextBoxColumnStyleInfo();
				decimalColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Decimal;
				grid.ColumnStyles.Add(decimalColumnInfo);

				var numberColumnInfo = new ZTextBoxColumnStyleInfo();
				numberColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Number;
				grid.ColumnStyles.Add(numberColumnInfo);

				grid.SetDataBinding(Dummy, "Collection");

				testForm.Show();
				AssertNotNull("PreCondition: Bound correctly.", grid.ListManager);

				var gridID = DummyModuleIDs.Dummy.Name;

				var gridLayoutManageable = new ZGridLayoutModification(grid, grid.Columns, Factory, null);
				var bothVisible = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "BothVisible", false, false, SaveColumnLayout.Ignore);

				grid.CurrentColumnLayout = bothVisible;
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID }, new string[] { gridID }, bothVisible, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();

					AssertEquals("Five columns should be available in CurrentColumns", 5, customiseForm.CurrentColumnsListBoxExposed.Items.Count);
					AssertEquals("Code", customiseForm.CurrentColumnsListBoxExposed.Items[0].ToString());
					AssertEquals("Description", customiseForm.CurrentColumnsListBoxExposed.Items[1].ToString());
					AssertEquals("Date", customiseForm.CurrentColumnsListBoxExposed.Items[2].ToString());
					AssertEquals("Decimal", customiseForm.CurrentColumnsListBoxExposed.Items[3].ToString());
					AssertEquals("Number", customiseForm.CurrentColumnsListBoxExposed.Items[4].ToString());

					// Move items up
					Assert("MoveUpButton should be enabled", customiseForm.MoveUpButtonExposed.Enabled);
					Assert("MoveDownButton should be enabled", customiseForm.MoveDownButtonExposed.Enabled);
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Add(2);
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Add(4);

					AssertNotNull("Precondition CurrentLayout", customiseBizO.CurrentLayout);
					var dataObj = new DataObject();
					dataObj.SetData(customiseForm.CurrentColumnsListBoxExposed);
					var drgevent = new DragEventArgs(dataObj, 0, 5, 10, DragDropEffects.Move, DragDropEffects.Move);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragOver(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragDrop(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);
					AssertNull("Current layout should be reseted after we change a column order", customiseBizO.CurrentLayout);

					AssertEquals("Five columns should be available in CurrentColumns", 5, customiseForm.CurrentColumnsListBoxExposed.Items.Count);
					AssertEquals("Date", customiseForm.CurrentColumnsListBoxExposed.Items[0].ToString());
					AssertEquals("Number", customiseForm.CurrentColumnsListBoxExposed.Items[1].ToString());
					AssertEquals("Code", customiseForm.CurrentColumnsListBoxExposed.Items[2].ToString());
					AssertEquals("Description", customiseForm.CurrentColumnsListBoxExposed.Items[3].ToString());
					AssertEquals("Decimal", customiseForm.CurrentColumnsListBoxExposed.Items[4].ToString());

					// Move items down
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Clear();
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Add(0);
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Add(1);

					dataObj = new DataObject();
					dataObj.SetData(customiseForm.CurrentColumnsListBoxExposed);
					drgevent = new DragEventArgs(dataObj, 0, 5, 70, DragDropEffects.Move, DragDropEffects.Move);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragOver(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragDrop(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);

					AssertEquals("Five columns should be available in CurrentColumns", 5, customiseForm.CurrentColumnsListBoxExposed.Items.Count);
					AssertEquals("Code", customiseForm.CurrentColumnsListBoxExposed.Items[0].ToString());
					AssertEquals("Description", customiseForm.CurrentColumnsListBoxExposed.Items[1].ToString());
					AssertEquals("Date", customiseForm.CurrentColumnsListBoxExposed.Items[2].ToString());
					AssertEquals("Number", customiseForm.CurrentColumnsListBoxExposed.Items[3].ToString());
					AssertEquals("Decimal", customiseForm.CurrentColumnsListBoxExposed.Items[4].ToString());

					// Move items to the end
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Clear();
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Add(1);
					customiseForm.CurrentColumnsListBoxExposed.SelectedIndices.Add(2);

					dataObj = new DataObject();
					dataObj.SetData(customiseForm.CurrentColumnsListBoxExposed);
					drgevent = new DragEventArgs(dataObj, 0, 5, 100, DragDropEffects.Move, DragDropEffects.Move);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragOver(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragDrop(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);

					AssertEquals("Five columns should be available in CurrentColumns", 5, customiseForm.CurrentColumnsListBoxExposed.Items.Count);
					AssertEquals("Code", customiseForm.CurrentColumnsListBoxExposed.Items[0].ToString());
					AssertEquals("Number", customiseForm.CurrentColumnsListBoxExposed.Items[1].ToString());
					AssertEquals("Decimal", customiseForm.CurrentColumnsListBoxExposed.Items[2].ToString());
					AssertEquals("Description", customiseForm.CurrentColumnsListBoxExposed.Items[3].ToString());
					AssertEquals("Date", customiseForm.CurrentColumnsListBoxExposed.Items[4].ToString());
				}
			}
		}

		public void TestDragAndDropAvailableColumnsListBoxToCurrentColumnsListBox()
		{
			using (var testForm = new ZForm())
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				grid.SetParentFilterGridModule(module);

				testForm.Controls.Add(grid);
				var codeColumnInfo = new ZTextBoxColumnStyleInfo();
				codeColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;
				grid.ColumnStyles.Add(codeColumnInfo);

				var descColumnInfo = new ZTextBoxColumnStyleInfo();
				descColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Description;
				grid.ColumnStyles.Add(descColumnInfo);

				var dateColumnInfo = new ZTextBoxColumnStyleInfo();
				dateColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Date;
				dateColumnInfo.IsVisible = false;
				grid.ColumnStyles.Add(dateColumnInfo);

				var decimalColumnInfo = new ZTextBoxColumnStyleInfo();
				decimalColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Decimal;
				grid.ColumnStyles.Add(decimalColumnInfo);

				var numberColumnInfo = new ZTextBoxColumnStyleInfo();
				numberColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Number;
				numberColumnInfo.IsVisible = false;
				grid.ColumnStyles.Add(numberColumnInfo);

				grid.SetDataBinding(Dummy, "Collection");

				testForm.Show();
				AssertNotNull("PreCondition: Bound correctly.", grid.ListManager);

				var gridID = DummyModuleIDs.Dummy.Name;

				var gridLayoutManageable = new ZGridLayoutModification(grid, grid.Columns, Factory, null);
				var bothVisible = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "BothVisible", false, false, SaveColumnLayout.Ignore);

				grid.CurrentColumnLayout = bothVisible;
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID }, new string[] { gridID }, bothVisible, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();

					AssertEquals("Three columns should be available in CurrentColumns", 3, customiseForm.CurrentColumnsListBoxExposed.Items.Count);
					AssertEquals("Code", customiseForm.CurrentColumnsListBoxExposed.Items[0].ToString());
					AssertEquals("Description", customiseForm.CurrentColumnsListBoxExposed.Items[1].ToString());
					AssertEquals("Decimal", customiseForm.CurrentColumnsListBoxExposed.Items[2].ToString());
					AssertEquals("Two columns should be available in AvailableColumns", 2, customiseForm.AvailableColumnsListBoxExposed.Items.Count);
					AssertEquals("Date", customiseForm.AvailableColumnsListBoxExposed.Items[0].ToString());
					AssertEquals("Number", customiseForm.AvailableColumnsListBoxExposed.Items[1].ToString());

					Assert("AvailableColumnsListBox should allow drop", customiseForm.AvailableColumnsListBoxExposed.AllowDrop);
					customiseForm.AvailableColumnsListBoxExposed.SelectedIndices.Add(0);

					var dataObj = new DataObject();
					dataObj.SetData(customiseForm.AvailableColumnsListBoxExposed);
					var drgevent = new DragEventArgs(dataObj, 0, 5, 30, DragDropEffects.Move, DragDropEffects.Move);

					Assert("isDragging should be false", !customiseForm.isDragging);
					customiseForm.AvailableColumnsListBox_MouseDown(null, null);
					Assert("isDragging should be true", customiseForm.isDragging);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragOver(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragDrop(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);
					customiseForm.AvailableColumnsListBox_MouseUp(null, null);
					Assert("isDragging should be false", !customiseForm.isDragging);

					AssertEquals("Four columns should be available in CurrentColumns", 4, customiseForm.CurrentColumnsListBoxExposed.Items.Count);
					AssertEquals("Code", customiseForm.CurrentColumnsListBoxExposed.Items[0].ToString());
					AssertEquals("Date", customiseForm.CurrentColumnsListBoxExposed.Items[1].ToString());
					AssertEquals("Description", customiseForm.CurrentColumnsListBoxExposed.Items[2].ToString());
					AssertEquals("Decimal", customiseForm.CurrentColumnsListBoxExposed.Items[3].ToString());
					AssertEquals("One columns should be available in AvailableColumns", 1, customiseForm.AvailableColumnsListBoxExposed.Items.Count);
					AssertEquals("Number", customiseForm.AvailableColumnsListBoxExposed.Items[0].ToString());
				}

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();

					var dataObj = new DataObject();
					dataObj.SetData(customiseForm.AvailableColumnsListBoxExposed);
					var drgevent = new DragEventArgs(dataObj, 0, 5, 30, DragDropEffects.Move, DragDropEffects.Move);

					Assert("isDragging should be false", !customiseForm.isDragging);
					customiseForm.AvailableColumnsListBox_MouseDown(null, null);
					Assert("isDragging should be true", customiseForm.isDragging);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragOver(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragDrop(customiseForm.CurrentColumnsListBoxExposed.SelectedItems, drgevent);

					customiseForm.CurrentColumnsListBox_MouseDown(null, null);
					Assert("isDragging should be false", !customiseForm.isDragging);
				}
			}
		}

		public void TestCustomiseColumnsResetIsDraggingAfterDragDrop()
		{
			using (var testForm = new ZForm())
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				grid.SetParentFilterGridModule(module);

				testForm.Controls.Add(grid);
				var codeColumnInfo = new ZTextBoxColumnStyleInfo();
				codeColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Code;
				grid.ColumnStyles.Add(codeColumnInfo);

				var descColumnInfo = new ZTextBoxColumnStyleInfo();
				descColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Description;
				grid.ColumnStyles.Add(descColumnInfo);

				var dateColumnInfo = new ZTextBoxColumnStyleInfo();
				dateColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Date;
				dateColumnInfo.IsVisible = false;
				grid.ColumnStyles.Add(dateColumnInfo);

				var decimalColumnInfo = new ZTextBoxColumnStyleInfo();
				decimalColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Decimal;
				grid.ColumnStyles.Add(decimalColumnInfo);

				var numberColumnInfo = new ZTextBoxColumnStyleInfo();
				numberColumnInfo.ColumnName = DummyBusinessObject.Schema.Z0_Number;
				numberColumnInfo.IsVisible = false;
				grid.ColumnStyles.Add(numberColumnInfo);

				grid.SetDataBinding(Dummy, "Collection");

				testForm.Show();
				AssertNotNull("PreCondition: Bound correctly.", grid.ListManager);

				var gridID = DummyModuleIDs.Dummy.Name;

				var gridLayoutManageable = new ZGridLayoutModification(grid, grid.Columns, Factory, null);
				var bothVisible = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "BothVisible", false, false, SaveColumnLayout.Ignore);

				grid.CurrentColumnLayout = bothVisible;
				var customiseBizO = new ZGridCustomiseBizObj(new string[] { gridID }, new string[] { gridID }, bothVisible, ZGuid.Empty);

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, customiseBizO))
				{
					customiseForm.Show();

					customiseForm.isDragging = true;
					customiseForm.AvailableColumnsListBoxExposed.SelectedIndex = 1;
					customiseForm.CurrentColumnsListBoxExposed.ZListBox_DragDrop(null, null);
					AssertEquals("isDragging should be reset after ZListBox_DragDrop is run.", false, customiseForm.isDragging);
				}
			}
		}

#endif

		public void TestCanAddColumnWhenColumnGroupIsUnavailable()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				var grid = form.TabGrid;
				var isUnitTestingProductionFunctionality = Globals.GetIsUnitTestingProductionFunctionality();

				using (var customiseForm = new ZGridCustomiseTester(grid.Columns, grid.Columns, grid))
				using (new DisposableAction(() => Globals.SetIsUnitTestingProductionFunctionality(isUnitTestingProductionFunctionality)))
				{
					Globals.SetIsUnitTestingProductionFunctionality(true);
					var column = new ZGridColumn { IsUnavailable = true, IsVisible = true, GroupName = new ResourceStringData("Test", "test") };
					var columnGroup = new ZGridColumnGroup(column);

					AssertNoExceptionThrown("No ArgumentNullException is thrown", () => customiseForm.CanAddColumnExposed(columnGroup));
				}
			}
		}

		public void TestFilterResetsUponClosing()
		{
			using (var form = new ZTestGridForm(Dummy))
			using (var grid = new TestGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_Description.Name, 50) { IsVisible = true });
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Z0_Number.Name, 50, 0) { IsVisible = true });
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Z0_Decimal.Name, 50, 2) { IsVisible = false });
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo(DummyBizoSchema.Z0_Date.Name, 50) { IsVisible = false });
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Z0_AnotherNumber.Name, 50, 0) { IsVisible = false });
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Z0_AnotherDecimal.Name, 50, 2) { IsVisible = false });
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo(DummyBizoSchema.Z0_AnotherDate.Name, 50) { IsVisible = false });

				grid.DataSource = Dummy;
				form.Controls.Add(grid);

				Factory.Save();

				var before = grid.Columns.Count;

				grid.CustomiseColumns();
				AssertEquals(ZFormModaliser.LastFormShownForTest.GetType(), typeof(ZGridCustomiseTester));
				var customiseForm = (ZGridCustomiseTester)ZFormModaliser.LastFormShownForTest;
				customiseForm.Show();
				customiseForm.SearchColumnExposed("Z0_A");
				customiseForm.DialogResult = DialogResult.OK;
				customiseForm.FirePostButtonForTest();

				AssertEquals(before, grid.Columns.Count);
			}
		}

		public void TestCustomizeCallsRefreshTableStyles_IsGridLayoutConfigurable()
		{
			AssertCustomizeCallsRefreshTableStyles(true);
		}

		public void TestCustomizeCallsRefreshTableStyles_NotIsGridLayoutConfigurable()
		{
			AssertCustomizeCallsRefreshTableStyles(false);
		}

		public void AssertCustomizeCallsRefreshTableStyles(bool isGridLayoutConfigurable)
		{
			using (var form = new ZTestGridForm(Dummy))
			using (var grid = new TestGrid { IsGridLayoutConfigurableOverride = isGridLayoutConfigurable })
			{
				form.Controls.Add(grid);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_Description.Name, 50) { IsVisible = true });
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Z0_Number.Name, 50, 0) { IsVisible = true });
				grid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo(DummyBizoSchema.Z0_Decimal.Name, 50, 2) { IsVisible = false });

				grid.DataSource = Dummy;
				grid.SetDataBinding(Dummy, nameof(Dummy.Collection));

				form.Show();
				Application.DoEvents(); // Finish form initialization

				grid.RefreshTableStylesCoreCallCount = 0;

				grid.CustomiseColumns();
				AssertEquals(ZFormModaliser.LastFormShownForTest.GetType(), typeof(ZGridCustomiseTester));

				var customiseForm = (ZGridCustomiseTester)ZFormModaliser.LastFormShownForTest;
				customiseForm.Show();

				customiseForm.AvailableColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.AddColumnExposed();

				customiseForm.DialogResult = DialogResult.OK;
				customiseForm.FirePostButtonForTest();

				Application.DoEvents(); // Wait for methods started with BeginInvoke()

				AssertEquals("Grid should call RefreshTableStyles after Customize Columns form is closed", 1, grid.RefreshTableStylesCoreCallCount);
			}
		}

		class TestGrid : ZGrid
		{
			protected override ZGridCustomise CreateNewGridCustomise()
			{
				return new ZGridCustomiseTester(Columns, Columns, this);
			}

			protected internal override bool IsGridLayoutConfigurable => IsGridLayoutConfigurableOverride;

			public bool IsGridLayoutConfigurableOverride { get; set; } = true;

			protected override void RefreshTableStylesCore()
			{
				base.RefreshTableStylesCore();
				RefreshTableStylesCoreCallCount++;
			}

			public int RefreshTableStylesCoreCallCount { get; set; }
		}
	}
}
