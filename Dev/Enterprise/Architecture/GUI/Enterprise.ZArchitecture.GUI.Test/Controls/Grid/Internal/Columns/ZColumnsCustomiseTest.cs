using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZColumnsCustomise))]
	sealed class ZColumnsCustomiseTest : ZFormBasherTest
	{
		public void TestSearchBox()
		{
			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X12", IsVisible = false, IsMandatory = true, IsCustomColumn = true },
					new CustomizableColumn { Caption = "C2", ColumnName = "X23", IsVisible = false, IsMandatory = true,  IsCustomColumn = false },
					new CustomizableColumn { Caption = "C3", ColumnName = "X34", IsVisible = true, IsMandatory = true, IsCustomColumn = true },
					new CustomizableColumn { Caption = "C4", ColumnName = "X45", IsVisible = true, IsMandatory = true, IsCustomColumn = false },
				}))
			{
				columnsCustomise.Show();

				AssertEquals(2, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);

				columnsCustomise.SearchColumnExposed("1");
				AssertEquals(1, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);
				AssertEquals("X12", (columnsCustomise.AvailableColumnsListBoxExposed.Items[0] as ICustomizableColumn).ColumnName);

				columnsCustomise.SearchColumnExposed("2");
				AssertEquals(2, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);
				AssertEquals("X12", (columnsCustomise.AvailableColumnsListBoxExposed.Items[0] as ICustomizableColumn).ColumnName);
				AssertEquals("X23", (columnsCustomise.AvailableColumnsListBoxExposed.Items[1] as ICustomizableColumn).ColumnName);

				columnsCustomise.SearchColumnExposed("3");
				AssertEquals(1, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);
				AssertEquals("X23", (columnsCustomise.AvailableColumnsListBoxExposed.Items[0] as ICustomizableColumn).ColumnName);

				columnsCustomise.SearchColumnExposed("4");
				AssertEquals(0, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);

				columnsCustomise.SearchColumnExposed("x");
				AssertEquals(2, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);
				AssertEquals("X12", (columnsCustomise.AvailableColumnsListBoxExposed.Items[0] as ICustomizableColumn).ColumnName);
				AssertEquals("X23", (columnsCustomise.AvailableColumnsListBoxExposed.Items[1] as ICustomizableColumn).ColumnName);

				columnsCustomise.SearchColumnExposed("xx");
				AssertEquals(0, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);

				columnsCustomise.SearchColumnExposed("");
				AssertEquals(2, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);
				AssertEquals("X12", (columnsCustomise.AvailableColumnsListBoxExposed.Items[0] as ICustomizableColumn).ColumnName);
				AssertEquals("X23", (columnsCustomise.AvailableColumnsListBoxExposed.Items[1] as ICustomizableColumn).ColumnName);

				columnsCustomise.SetCustomColumnsCheckbox(false);
				columnsCustomise.SearchColumnExposed("");
				AssertEquals(1, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);
				AssertEquals("X23", (columnsCustomise.AvailableColumnsListBoxExposed.Items[0] as ICustomizableColumn).ColumnName);

				columnsCustomise.SetCustomColumnsCheckbox(true);
				columnsCustomise.SearchColumnExposed("");
				AssertEquals(2, columnsCustomise.AvailableColumnsListBoxExposed.Items.Count);
				AssertEquals("X12", (columnsCustomise.AvailableColumnsListBoxExposed.Items[0] as ICustomizableColumn).ColumnName);
				AssertEquals("X23", (columnsCustomise.AvailableColumnsListBoxExposed.Items[1] as ICustomizableColumn).ColumnName);
			}
		}

		public void TestSave()
		{
			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = false },
				}))
			{
				AssertNull(columnsCustomise.Result);

				columnsCustomise.SaveColumnsExposed();

				AssertNotNull(columnsCustomise.Result);
				AssertEquals(2, columnsCustomise.Result.Count);
				AssertEquals("C1", columnsCustomise.Result[0].ToString());
				AssertEquals("C2", columnsCustomise.Result[1].ToString());
				AssertEquals("X1", columnsCustomise.Result[0].ColumnName);
				AssertEquals("X2", columnsCustomise.Result[1].ColumnName);
				AssertEquals(true, columnsCustomise.Result[0].IsVisible);
				AssertEquals(false, columnsCustomise.Result[1].IsVisible);
			}
		}

		public void TestCustomColumnsCheckBox()
		{
			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true, IsCustomColumn = true },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true, IsCustomColumn = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = true, IsCustomColumn = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = false, IsCustomColumn = true },
					new CustomizableColumn { Caption = "C5", ColumnName = "X5", IsVisible = false, IsCustomColumn = true },
					new CustomizableColumn { Caption = "C6", ColumnName = "X6", IsVisible = false, IsCustomColumn = false },
				}))
			{
				AssertEquals(true, columnsCustomise.CustomColumnsCheckBoxCheckedExposed);
				AssertEquals("C1", columnsCustomise.CurrentColumnsListBoxExposed.Items[0].ToString());
				AssertEquals("C2", columnsCustomise.CurrentColumnsListBoxExposed.Items[1].ToString());
				AssertEquals("C3", columnsCustomise.CurrentColumnsListBoxExposed.Items[2].ToString());
				AssertEquals("C4", columnsCustomise.AvailableColumnsListBoxExposed.Items[0].ToString());
				AssertEquals("C5", columnsCustomise.AvailableColumnsListBoxExposed.Items[1].ToString());
				AssertEquals("C6", columnsCustomise.AvailableColumnsListBoxExposed.Items[2].ToString());

				columnsCustomise.SetCustomColumnsCheckbox(false);
				AssertEquals(false, columnsCustomise.CustomColumnsCheckBoxCheckedExposed);
				columnsCustomise.UpdateCustomColumnsVisibilityExposed();

				AssertEquals("C1", columnsCustomise.CurrentColumnsListBoxExposed.Items[0].ToString());
				AssertEquals("C2", columnsCustomise.CurrentColumnsListBoxExposed.Items[1].ToString());
				AssertEquals("C3", columnsCustomise.CurrentColumnsListBoxExposed.Items[2].ToString());
				AssertEquals("C6", columnsCustomise.AvailableColumnsListBoxExposed.Items[0].ToString());
			}
		}

		public void TestSaveWithSubmissiveColumns()
		{
			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new SubmissiveCustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true, IsSubmissive = true },
					new SubmissiveCustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = false, IsSubmissive = false },
				}))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				columnsCustomise.FirePostButtonForTest();
				AssertEquals("Selected columns cannot be shown by themselves. Use them with some other columns.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new SubmissiveCustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true, IsSubmissive = false },
					new SubmissiveCustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = false, IsSubmissive = true },
				}))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				columnsCustomise.FirePostButtonForTest();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveLayoutWithNonColumnsShown()
		{
			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new SubmissiveCustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new SubmissiveCustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = false },
				}))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				columnsCustomise.FireSaveLayoutButtonForTest();
				AssertEquals("At least one column must be shown in the current grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveWithZDescriptionCodeFindBoxColumnStyle()
		{
			var myColumn = new ZGridColumn();
			var propertyDescriptor = TypeDescriptor.GetProperties(Factory.New<DummyBusinessObject>())[DummyBizoSchema.Z0_Code.Name];
			var columnStyleInfo = new ZDescriptionCodeFindBoxColumnStyleInfo(propertyDescriptor)
			{
				DescriptionColumnName = DummyBizoSchema.Z0_Description.Name,
				ColumnName = "P9_GS_NKAssignedStaffMember"
			};

			using (myColumn.ColumnStyle = new ZDescriptionCodeFindBoxColumnStyle(columnStyleInfo))
			{
				myColumn.IsVisible = true;
				var groups = new ZGridColumnGroup(myColumn);
				using (var columnsCustomise = new ZColumnsCustomiseTester(new List<ICustomizableColumn> { groups }))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					columnsCustomise.FirePostButtonForTest();
					AssertEquals("Selected columns cannot be shown by themselves. Use them with some other columns.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestReset()
		{
			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = false },
				},
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "K1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "K2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "K3", ColumnName = "X3", IsVisible = true },
				}))
			{
				AssertNull(columnsCustomise.Result);
				AssertEquals(DialogResult.None, columnsCustomise.DialogResult);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				columnsCustomise.ResetColumnsExposed();

				AssertNotNull(columnsCustomise.Result);
				AssertEquals("Only X1 and X2, but no X3", 2, columnsCustomise.Result.Count);
				AssertEquals("X1", columnsCustomise.Result[0].ColumnName);
				AssertEquals("X2", columnsCustomise.Result[1].ColumnName);
				AssertEquals(false, columnsCustomise.Result[0].IsVisible);
				AssertEquals(true, columnsCustomise.Result[1].IsVisible);

				AssertEquals(DialogResult.OK, columnsCustomise.DialogResult);
			}
		}

		public void TestCanRemove()
		{
			using (var columnsCustomise = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true, IsMandatory = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true, IsMandatory = true },
				}))
			{
				AssertEquals(2, columnsCustomise.CurrentColumnsListBoxExposed.Items.Count);

				columnsCustomise.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				columnsCustomise.RemoveColumnsExposed();

				AssertEquals(1, columnsCustomise.CurrentColumnsListBoxExposed.Items.Count);

				columnsCustomise.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				columnsCustomise.RemoveColumnsExposed();

				AssertEquals("Should not remove mandatory column", 1, columnsCustomise.CurrentColumnsListBoxExposed.Items.Count);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("This column is mandatory and cannot be removed: C2."));
			}
		}

		public void TestManageLayoutButtonClick()
		{
			using (var customiseForm = new ZColumnsCustomiseTester(new List<ICustomizableColumn>(), null, true, new ZGridCustomiseBizObj("xxx", null)))
			{
				customiseForm.Show();
				customiseForm.ToolStripManageLayoutsButtonExposed.PerformClick();
				AssertEquals("ManageLayoutForm is shown", typeof(ManageLayoutsForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestSaveLayout_ShouldNotShowIsUserDefinedFilterCheckBox()
		{
			var customiseBizO = new ZGridCustomiseBizObj("gridID", null);
			using (var customiseForm = new ZColumnsCustomiseTester(new List<ICustomizableColumn> {
				new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true },
				}, null, true, customiseBizO))
			{
				bool? isUserDefinedFilterCheckBoxVisibility = null;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var saveLayoutForm = f as SaveLayoutForm;

					saveLayoutForm.Shown += (_, x_) =>
					{
						isUserDefinedFilterCheckBoxVisibility = saveLayoutForm.FindSingleOrDefault<ZCheckBox>("IsUserDefinedFilterCheckBox").Visible;
					};
				});

				customiseForm.Show();
				customiseForm.FireSaveLayoutButtonForTest();

				AssertEquals("IsUserDefinedFilterCheckBox should not be visible", false, isUserDefinedFilterCheckBoxVisibility);
			}
		}

		public void TestSaveLayoutButtonClick()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);
			using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
			{
				customiseForm.Show();

				AssertNull("PreCondition:CurrentLayoutName", customiseBizO.CurrentLayout);

				var gridLayoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);
				var layoutBizO = new SaveLayoutBizO(gridLayoutManageable) { LayoutName = "TEST", PublishLayout = true };

				customiseForm.SaveLayoutBizObjExposed = layoutBizO;
				customiseForm.ToolStripSaveLayoutButtonExposed.PerformClick();

				AssertEquals("CurrentLayoutName in CustomiseBizO is updated too", "TEST", customiseBizO.CurrentLayout.ColumnLayoutName);
				var savedFilter = new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "TEST", true);
				AssertNotNull("PreCondition:Layout 'TEST' is saved", savedFilter);

				var visibleColumns = ZColumnsCustomise.GetVisibleColumnNames(Factory, savedFilter).ToList();
				AssertEquals(2, visibleColumns.Count);
				AssertEquals("X2", visibleColumns[0]);
				AssertEquals("X4", visibleColumns[1]);

				layoutBizO = new SaveLayoutBizO(gridLayoutManageable) { LayoutName = "TEST", PublishLayout = true };

				customiseForm.SaveLayoutBizObjExposed = layoutBizO;
				AssertNoExceptionThrown("Should attempt to save with [TEST] without exceptions", () => customiseForm.ToolStripSaveLayoutButtonExposed.PerformClick());
			}
		}

		public void TestDeletingLayoutResetsParentGridsCurrentColumnLayout()
		{
			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var collection = new DummyBusinessObjectCollection(Factory);
			using (var form = new ZForm(collection))
			using (var grid = new ZGrid())
			{
				form.Show();

				grid.DataSource = collection;

				var keyProvider = new LegacyDataGridLayoutContextKeyProvider(grid);
				var customiseBizObj = new ZGridCustomiseBizObj(new string[] { keyProvider.ContextKeyForStmModuleFilter }, new string[] { keyProvider.ContextKeyForStmData }, null, ZGuid.Empty);

				var gridLayoutManageable = new ZColumnsLayoutModification(columns, Factory, null, keyProvider.ContextKeyForStmModuleFilter);
				var layoutBizO = new SaveLayoutBizO(gridLayoutManageable) { LayoutName = "TEST", PublishLayout = true };

				using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizObj, grid))
				{
					customiseForm.Show();

					AssertNull("PreCondition:CurrentLayoutName", customiseBizObj.CurrentLayout);

					customiseForm.SaveLayoutBizObjExposed = layoutBizO;
					customiseForm.ToolStripSaveLayoutButtonExposed.PerformClick();

					var savedFilter = new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(keyProvider.ContextKeyForStmModuleFilter, "TEST", true);
					AssertNotNull("PostCondition:Layout 'TEST' is saved", savedFilter);

					grid.CurrentColumnLayout = savedFilter;
				}

				using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizObj, grid))
				{
					customiseForm.Show();

					AssertEquals("PreCondition: Layout is set to active/current", "TEST", customiseBizObj.CurrentLayout.ColumnLayoutName);
					AssertNotNull("PreCondition: CurrentColumnLayout should not be null", customiseForm.ParentGridExposed.CurrentColumnLayout);

					var filter = customiseBizObj.CurrentLayout as StmModuleFilter;
					AssertNotNull("PreCondition: filter not null", filter);

					filter.Delete();
					Factory.Save();

					AssertNull("CurrentColumnLayout should be null", customiseForm.ParentGridExposed.CurrentColumnLayout);
				}
			}
		}

		public void TestGetVisibleColumns()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var allColumns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "K1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "K2", ColumnName = "X2", IsVisible = false },
					new CustomizableColumn { Caption = "K3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "K4", ColumnName = "X4", IsVisible = false },
				};

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);
			using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
			{
				customiseForm.Show();

				AssertNull("PreCondition:CurrentLayoutName", customiseBizO.CurrentLayout);

				var gridLayoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);
				var layoutBizO = new SaveLayoutBizO(gridLayoutManageable) { LayoutName = "TEST", PublishLayout = true };

				customiseForm.SaveLayoutBizObjExposed = layoutBizO;
				customiseForm.ToolStripSaveLayoutButtonExposed.PerformClick();

				AssertEquals("CurrentLayoutName in CustomiseBizO is updated too", "TEST", customiseBizO.CurrentLayout.ColumnLayoutName);
				var savedLayout = new StmModuleFilter.Loader(Factory).FindTop1ByIDAndName(gridID, "TEST", true);
				AssertNotNull("PreCondition:Layout 'TEST' is saved", savedLayout);

				var visibleColumns = ZColumnsCustomise.GetVisibleColumns(Factory, savedLayout, allColumns).ToList();
				AssertEquals(2, visibleColumns.Count);
				AssertEquals("X2", visibleColumns[0].ColumnName);
				AssertEquals("Should not pay attention to different caption", "K2", visibleColumns[0].ToString());
				AssertEquals("X4", visibleColumns[1].ColumnName);
				AssertEquals("Should not pay attention to different caption", "K4", visibleColumns[1].ToString());
			}
		}

		public void TestClearCurrentLayoutNameIfDeleted()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
				};

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);

			var gridLayoutManageable = new ZColumnsLayoutModification(columns, customiseBizO.Factory, null, gridID);
			var layout = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "Layout1", false, false, SaveColumnLayout.Ignore);

			using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
			{
				customiseForm.Show();
				customiseBizO.CurrentLayout = layout;
				layout.Delete();
				AssertNull("CurrentLayoutName should have been cleared", customiseBizO.CurrentLayout);
			}
		}

		public void TestWhenChangingCurrentLayoutName()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var gridLayoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);
			var layout1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "Layout1", false, false, SaveColumnLayout.Ignore);

			columns[0].IsVisible = true;
			columns[2].IsVisible = true;

			gridLayoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);
			var layout2 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "Layout2", false, false, SaveColumnLayout.Ignore);

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);
			using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
			{
				customiseForm.Show();

				customiseBizO.CurrentLayout = layout1;

				AssertEquals(2, customiseForm.SelectedColumns.Count);
				AssertEquals("C2", customiseForm.SelectedColumns[0].ToString());
				AssertEquals("C4", customiseForm.SelectedColumns[1].ToString());

				AssertEquals(2, customiseForm.AvailableColumns.Count);
				AssertEquals("C1", customiseForm.AvailableColumns[0].ToString());
				AssertEquals("C3", customiseForm.AvailableColumns[1].ToString());

				customiseBizO.CurrentLayout = layout2;

				AssertEquals(4, customiseForm.SelectedColumns.Count);
				AssertEquals("C1", customiseForm.SelectedColumns[0].ToString());
				AssertEquals("C2", customiseForm.SelectedColumns[1].ToString());
				AssertEquals("C3", customiseForm.SelectedColumns[2].ToString());
				AssertEquals("C4", customiseForm.SelectedColumns[3].ToString());

				AssertEquals(0, customiseForm.AvailableColumns.Count);
			}
		}

		public void TestWhenCurrentlySelectedLayoutNameIsRenamed()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);

			var gridLayoutManageable = new ZColumnsLayoutModification(columns, customiseBizO.Factory, null, gridID);
			var layout1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "Layout1", false, false, SaveColumnLayout.Ignore);

			using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
			{
				customiseForm.Show();

				customiseBizO.CurrentLayout = layout1;

				AssertEquals(2, customiseForm.SelectedColumns.Count);
				AssertEquals("C2", customiseForm.SelectedColumns[0].ToString());
				AssertEquals("C4", customiseForm.SelectedColumns[1].ToString());

				AssertEquals(2, customiseForm.AvailableColumns.Count);
				AssertEquals("C1", customiseForm.AvailableColumns[0].ToString());
				AssertEquals("C3", customiseForm.AvailableColumns[1].ToString());

				var newLayout = Factory.LoadTop1<StmModuleFilter>(new ZQuery(StmModuleFilterSchema.S9_FilterName, "Layout1"));
				var layoutsChanged = new Dictionary<ZGuid, string> { { newLayout.PK, "Layout2" } };
				new LayoutSaveManager().SaveLayoutChanges(gridLayoutManageable, layoutsChanged, new List<ZGuid>(), new List<(ZGuid, ZBool, ZBool)>(), ZGuid.Empty);

				AssertEquals("Current Layoutname is changed", "Layout2", customiseBizO.CurrentLayout.ColumnLayoutName);
			}
		}

		public void TestInvalidateCurrentLayoutNameWhenCurrentColumnChanges()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var gridLayoutManageable = new ZColumnsLayoutModification(columns, Factory, null, gridID);
			var layout1 = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "Layout1", false, false, SaveColumnLayout.Ignore);

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);
			using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
			{
				customiseForm.Show();

				AssertEquals("Precondition", 2, customiseForm.SelectedColumns.Count);
				AssertEquals("Precondition", 2, customiseForm.AvailableColumns.Count);

				customiseBizO.CurrentLayout = layout1;
				customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.RemoveColumnsExposed();
				AssertNull("CurrentLayoutName is invalidated", customiseBizO.CurrentLayout);
				AssertEquals("CurrentColumns should only have one column", 1, customiseForm.SelectedColumns.Count);
				AssertEquals("CurrentColumns should only have one column", 3, customiseForm.AvailableColumns.Count);

				customiseBizO.CurrentLayout = layout1;

				AssertEquals("Layout is reloaded - SelectedColumns", 2, customiseForm.SelectedColumns.Count);
				AssertEquals("Layout is reloaded - AvailableColumns", 2, customiseForm.AvailableColumns.Count);

				customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.MoveColumnDownExposed();
				AssertNull("CurrentLayoutName is invalidated", customiseBizO.CurrentLayout);

				customiseBizO.CurrentLayout = layout1;
				customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 1;
				customiseForm.MoveColumnUpExposed();
				AssertNull("CurrentLayoutName is invalidated", customiseBizO.CurrentLayout);

				customiseBizO.CurrentLayout = layout1;
				customiseForm.AvailableColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.AddColumnExposed();
				AssertNull("CurrentLayoutName is invalidated", customiseBizO.CurrentLayout);
				AssertEquals("CurrentColumns should only have one column", 3, customiseForm.SelectedColumns.Count);
				AssertEquals("CurrentColumns should only have one column", 1, customiseForm.AvailableColumns.Count);
			}
		}

		public void TestCustomisingColumnsDoesNotCreateUnnecessaryCopiesOfGridLayoutStorageBizO()
		{
			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var layout1 = Factory.New<StmModuleFilter>();
			layout1.S9_ModuleID = "1";
			layout1.S9_FilterName = "A";

			var defaultLayout = Factory.New<StmData>();
			defaultLayout.SD_Name = "1";
			defaultLayout.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
			defaultLayout.SD_BinaryValue = ZBlob.FromAscii("Juio45897kljrew");
			Factory.Save();

			var collection = new DummyBusinessObjectCollection(Factory);
			using (var form = new ZForm(collection))
			using (var grid = new ZGrid())
			{
				form.Show();

				grid.DataSource = collection;

				var customiseBizObj = new ZGridCustomiseBizObj(new string[] { "1" }, new string[] { "1" }, null, ZGuid.Empty);

				using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizObj, grid))
				{
					customiseForm.Show();

					customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
					customiseForm.RemoveColumnsExposed();
					AssertEquals("There should be 1 StmModuleFilter", 1, ((IBusinessObjectFactoryInternals)customiseForm.BusinessEntity.Factory).AllBusinessObjects.OfType<StmModuleFilter>().Count());
					AssertEquals("There should be 1 StmData", 1, ((IBusinessObjectFactoryInternals)customiseForm.BusinessEntity.Factory).AllBusinessObjects.OfType<StmData>().Count());
					AssertEquals("There should only be 2 BizO's: 1 for StmModuleFilter and 1 for StmData in the keyProvider", 2, ((IBusinessObjectFactoryInternals)customiseForm.BusinessEntity.Factory).AllBusinessObjects.OfType<GridLayoutStorageBizO>().Count());

					customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
					customiseForm.RemoveColumnsExposed();
					AssertEquals("There should still be 2 as the BizO's are now cached", 2, ((IBusinessObjectFactoryInternals)customiseForm.BusinessEntity.Factory).AllBusinessObjects.OfType<GridLayoutStorageBizO>().Count());

					customiseForm.AvailableColumnsListBoxExposed.SelectedIndex = 0;
					customiseForm.AddColumnExposed();
					AssertEquals("There should still be 2 as the BizO's are now cached", 2, ((IBusinessObjectFactoryInternals)customiseForm.BusinessEntity.Factory).AllBusinessObjects.OfType<GridLayoutStorageBizO>().Count());

					customiseForm.ResetColumnsExposed();
					AssertEquals("There should still be 2 as the BizO's are now cached", 2, ((IBusinessObjectFactoryInternals)customiseForm.BusinessEntity.Factory).AllBusinessObjects.OfType<GridLayoutStorageBizO>().Count());
				}
			}
		}

		class ZGridForCustomizeColumnsTest : ZGrid
		{
			protected override ZGridCustomise CreateNewGridCustomise()
			{
				var helper = new GridLayoutContextKeyProviderHelper();
				customiseBizObj = new ZGridCustomiseBizObj(helper.GetAllGridIDsForStmModuleFilter(this), helper.GetAllGridIDsForStmData(this), CurrentColumnLayout, LayoutCategoryPK);
				return customiseForm = new ZArchitecture.Testing.ZGridCustomiseTester(Columns, Columns, customiseBizObj);
			}

			public ZArchitecture.Testing.ZGridCustomiseTester customiseForm;
		}
		public void TestCustomiseColumnsFormIsDisposedWhenClosed()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			using (var form = new ZForm(collection))
			using (var grid = new ZGridForCustomizeColumnsTest())
			{
				form.Controls.Add(grid);
				form.Show();
				grid.Columns.Add(new ZCalcEditColumnStyleInfo("Column1", 80, 3));
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("Column2", 80));
				grid.Columns.Add(new ZCheckBoxColumnStyleInfo("Column3", 80));

				grid.DataSource = collection;

				grid.CustomiseColumns();
				var customiseForm = grid.customiseForm;
				customiseForm.Show();
				customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.RemoveColumnsExposed();
				customiseForm.CancelButtonExposed.PerformClick();
				AssertNull("There are no customise columns forms after clicking the Cancel button", customiseForm.BusinessEntity);

				grid.CustomiseColumns();
				customiseForm = grid.customiseForm;
				customiseForm.Show();
				customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.RemoveColumnsExposed();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				customiseForm.ResetButtonExposed.PerformClick();
				AssertNull("There are no customise columns forms after clicking the Reset button", customiseForm.BusinessEntity);

				grid.CustomiseColumns();
				customiseForm = grid.customiseForm;
				customiseForm.Show();
				customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.RemoveColumnsExposed();
				customiseForm.PostButtonExposed.PerformClick();
				AssertNull("There are no customise columns forms after clicking the OK button", customiseForm.BusinessEntity);

				grid.CustomiseColumns();
				customiseForm = grid.customiseForm;
				customiseForm.Show();
				customiseForm.CurrentColumnsListBoxExposed.SelectedIndex = 0;
				customiseForm.RemoveColumnsExposed();
				customiseForm.Close();
				AssertNull("There are no customise columns forms after clicking the X button", customiseForm.BusinessEntity);
			}
		}

		public void TestResizingFormKeepsCorrectLook()
		{
			using (var columnsCustomiseForm = new ZColumnsCustomiseTester(
					new List<ICustomizableColumn>
					{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true, IsMandatory = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true, IsMandatory = true },
					}))
			{
				var oldColumnsCustomizeSize = columnsCustomiseForm.Size;

				columnsCustomiseForm.Size = new System.Drawing.Size { Height = columnsCustomiseForm.Size.Height + 30, Width = columnsCustomiseForm.Size.Width };

				AssertEquals("ListBox new size should be 30 pixels higher", 30, columnsCustomiseForm.Size.Height - oldColumnsCustomizeSize.Height);

				AssertEquals("ListBox and Post Button should have same Location.X", 0, columnsCustomiseForm.PostButtonExposed.Location.X - columnsCustomiseForm.CurrentColumnsListBoxExposed.Location.X);
				AssertEquals("There should be exactly 20 pixels between ListBox and Cancel Button", ControlDpiScalingHelper.ScaleToCurrentDpiY(50), columnsCustomiseForm.CancelButtonExposed.Location.Y - (columnsCustomiseForm.CurrentColumnsListBoxExposed.Location.Y + columnsCustomiseForm.CurrentColumnsListBoxExposed.Size.Height));

				AssertEquals("Post and Cancel Button should have same Location.Y", 0, columnsCustomiseForm.PostButtonExposed.Location.Y - columnsCustomiseForm.CancelButtonExposed.Location.Y);

				AssertEquals("There should be exactly the height of the Reset textbox between ListBox Location.Y + Location.Size.Height and Reset Button Location.Y", columnsCustomiseForm.ResetButtonExposed.Size.Height, columnsCustomiseForm.CurrentColumnsListBoxExposed.Location.Y + columnsCustomiseForm.CurrentColumnsListBoxExposed.Size.Height - columnsCustomiseForm.ResetButtonExposed.Location.Y);
			}
		}

		public void TestThatHeightIsScaled()
		{
			const string gridID = "xxx-yyy";

			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = false },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = true },
					new CustomizableColumn { Caption = "C3", ColumnName = "X3", IsVisible = false },
					new CustomizableColumn { Caption = "C4", ColumnName = "X4", IsVisible = true },
				};

			var customiseBizO = new ZGridCustomiseBizObj(gridID, null);
			var gridLayoutManageable = new ZColumnsLayoutModification(columns, customiseBizO.Factory, null, gridID);
			int currentColumnOriginalSize;
			int availableColumnOriginalSize;

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(100, 100))
			{
				using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
				{
					currentColumnOriginalSize = customiseForm.CurrentColumnsListBoxExposed.ItemHeight;
					availableColumnOriginalSize = customiseForm.AvailableColumnsListBoxExposed.ItemHeight;
				}
			}
			CheckColumnScaling(125, currentColumnOriginalSize, availableColumnOriginalSize, columns, customiseBizO);
			CheckColumnScaling(350, currentColumnOriginalSize, availableColumnOriginalSize, columns, customiseBizO);
		}

		void CheckColumnScaling(int scale, int currentColumnOriginalSize, int availableColumnOriginalSize, List<ICustomizableColumn> columns, ZGridCustomiseBizObj customiseBizO)
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(scale, scale))
			{
				using (var customiseForm = new ZColumnsCustomiseTester(columns, null, true, customiseBizO))
				{
					AssertCorrectSize(currentColumnOriginalSize * scale / 100, customiseForm.CurrentColumnsListBoxExposed.ItemHeight);
					AssertCorrectSize(availableColumnOriginalSize * scale / 100, customiseForm.AvailableColumnsListBoxExposed.ItemHeight);
				}
			}
		}

		void AssertCorrectSize(int expected, int actual, int margin = 1)
		{
			Assert($"Expected: {expected}+-{margin}, but was: {actual}", actual >= expected - margin && actual <= expected + margin);
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var form = new ZColumnsCustomiseTester(
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "C1", ColumnName = "X1", IsVisible = true },
					new CustomizableColumn { Caption = "C2", ColumnName = "X2", IsVisible = false },
				});
			form.MoveUpButtonExposed.Enabled = false;
			form.MoveDownButtonExposed.Enabled = false;
			return form;
		}

		#endregion
	}
}
