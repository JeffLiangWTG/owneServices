using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

#pragma warning disable CW1108 // Do Not Use DataSet

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridTest2 : TestCaseWithFactory
	{
		public void TestSuspendCancelOfNonEditedRowOnLeaving()
		{
			using (var myForm = new ZForm())
			{
				var textBox = new ZTextBox();
				myForm.Controls.Add(textBox);

				TestGrid.Parent = myForm;
				TestGrid.SetDataBinding(DataSetToBind, GridTableName);
				myForm.Show();

				TestGrid.Focus();
				AssertEquals("Grid should have one row", 1, TestGrid.ListManager.Count);
				textBox.Focus();
				AssertEquals("Cancel of Non-edited row on leaving is not suspended, Grid should have zero rows", 0, TestGrid.ListManager.Count);

				using (TestGrid.SuspendCancelOfNonEditedRowOnLeaving())
				{
					TestGrid.Focus();
					AssertEquals("Grid should have one row", 1, TestGrid.ListManager.Count);
					textBox.Focus();
					AssertEquals("Cancel of Non-edited row on leaving is suspended, Grid should have one row", 1, TestGrid.ListManager.Count);
				}

				TestGrid.Focus();
				textBox.Focus();
				AssertEquals("Cancel of Non-edited row on leaving resumed, Grid should have 0 rows", 0, TestGrid.ListManager.Count);
			}
		}

		public void TestOnLeave()
		{
			using (var grid = new TestZGrid())
			{
				grid.OnLeave();
				AssertEquals("OnLeave executed without exceptions", true, grid.HasLeft);
			}
		}

		public void TestOnEnter()
		{
			var data = new DataSet(); // This is legacy architecture that will be removed
			var table = data.Tables.Add("Table");
			table.Columns.Add("PK");
			table.Columns.Add("Name");
			table.Columns.Add("Age");

			var row1 = table.NewRow();
			var row2 = table.NewRow();
			var row3 = table.NewRow();

			table.Rows.Add(row1);
			table.Rows.Add(row2);
			table.Rows.Add(row3);

			using (var testForm = new ZForm())
			{
				TestGrid.Parent = testForm;
				TestGrid.SetDataBinding(data, "Table");
				testForm.Show();

				TestGrid.CurrentCell = new DataGridCell(0, 0);
				TestGrid.CurrentCell = new DataGridCell(2, 1);

				TestGrid.IsHidingControl = false;

				var gridStateField = typeof(DataGrid).GetField("gridState", BindingFlags.NonPublic | BindingFlags.Instance);
				var gridState = (BitVector32)gridStateField.GetValue(TestGrid);
				gridState[0x10000] = true;
				gridStateField.SetValue(TestGrid, gridState);
				TestGrid.OnEnter();

				AssertEquals("Current row index should be 2", 2, TestGrid.CurrentCell.RowNumber);
				AssertEquals("Grid state is changing edit control, column index should NOT be reset to initial position (should be 1)", 1, TestGrid.CurrentCell.ColumnNumber);

				gridState[0x10000] = false;
				gridStateField.SetValue(TestGrid, gridState);
				TestGrid.OnEnter();

				AssertEquals("Current row index should be 2", 2, TestGrid.CurrentCell.RowNumber);
				AssertEquals("Grid state is not changing edit control, column index should be reset to initial position (0)", 0, TestGrid.CurrentCell.ColumnNumber);

				TestGrid.CurrentCell = new DataGridCell(2, 1);
				TestGrid.IsHidingControl = true;
				TestGrid.OnEnter();

				AssertEquals("Current row index should be 2", 2, TestGrid.CurrentCell.RowNumber);
				AssertEquals("Grid state is not changing edit control, but control is hidden, column index should NOT be reset to initial position (should be 1)", 1, TestGrid.CurrentCell.ColumnNumber);
			}
		}

		#region TestDeleteMenuItemEnabled

		public void TestDeleteMenuItemEnabled()
		{
			using (var form = new ZForm())
			{
				TestGrid.Dock = DockStyle.Fill;
				form.Controls.Add(TestGrid);
				form.Show();

				TestGrid.PopupContextMenu();
				Assert("Delete Menu Item should not be Visible when List is null", !TestGrid.DeleteMenuItem.Visible);
				TestGrid.Focus();

				var data = new DataSet(); // This is legacy architecture that will be removed
				var table = data.Tables.Add("Table");
				table.Columns.Add("PK");
				table.Columns.Add("Name");
				table.Rows.Add(new object[] { new Guid(), "ASD" });

				TestGrid.SetDataBinding(data, "Table");

				TestGrid.RemoveAction = RemoveAction.NoRemovePossible;
				TestGrid.PopupContextMenu();
				Assert("Delete Menu Item should not be Visible when AllowDelete is false", !TestGrid.DeleteMenuItem.Visible);
				TestGrid.Focus();

				TestGrid.RemoveAction = RemoveAction.RemoveAndDelete;
				TestGrid.PopupContextMenu();
				Assert("Delete Menu Item should be Visible when AllowDelete is true", TestGrid.DeleteMenuItem.Visible);
				Assert("Delete Menu should be Enabled", TestGrid.DeleteMenuItem.Enabled);
				TestGrid.Focus();

				TestGrid.ReadOnly = true;
				TestGrid.PopupContextMenu();
				Assert("Delete Menu should not be Enabled when grid is ReadOnly", !TestGrid.DeleteMenuItem.Enabled);
				TestGrid.Focus();

				TestGrid.AllowReadOnlyRowsToBeDeleted = true;
				TestGrid.PopupContextMenu();
				Assert("Delete Menu should not be Enabled when grid is ReadOnly despite of AllowReadOnlyRowsToBeDeleted.", !TestGrid.DeleteMenuItem.Enabled);
				TestGrid.Focus();
			}
		}

		public void TestDeleteMenuItemEnabled_IsRowReadOnly()
		{
			AssertDeleteMenuItemEnabled_IsRowReadOnly(false);
		}

		public void TestDeleteMenuItemEnabled_AllowReadOnlyRowsToBeDeleted()
		{
			AssertDeleteMenuItemEnabled_IsRowReadOnly(true);
		}

		void AssertDeleteMenuItemEnabled_IsRowReadOnly(bool allowReadOnlyRowsToBeDeleted)
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var object1 = collection.AddNew();
			var object2 = collection.AddNew();
			var object3 = collection.AddNew();
			AfterRowDeletedIsImplemented = false;

			using (var testForm = new ZForm())
			{
				TestGrid.Parent = testForm;
				TestGrid.AllowReadOnlyRowsToBeDeleted = allowReadOnlyRowsToBeDeleted;
				TestGrid.SetDataBinding(collection, "");
				testForm.Show();

				var list = TestGrid.ListManager.List;

				TestGrid.RemoveAction = RemoveAction.RemoveAndDelete;
				TestGrid.PopupContextMenu();
				Assert(TestGrid.DeleteMenuItem.Visible);
				Assert(TestGrid.DeleteMenuItem.Enabled);

				collection.SetReadOnlyIncludingChildren(true);
				TestGrid.PopupContextMenu();
				Assert(!TestGrid.DeleteMenuItem.Visible);
				Assert(!TestGrid.DeleteMenuItem.Enabled);
				collection.SetReadOnlyIncludingChildren(false);
				TestGrid.PopupContextMenu();
				Assert(TestGrid.DeleteMenuItem.Visible);
				Assert(TestGrid.DeleteMenuItem.Enabled);

				((ZGridColumnInfo)TestGrid.ColumnStyles[0]).IsReadOnly = true;
				((ZGridColumnInfo)TestGrid.ColumnStyles[1]).IsReadOnly = true;
				TestGrid.PopupContextMenu();
				Assert(TestGrid.DeleteMenuItem.Visible);
				AssertEquals(allowReadOnlyRowsToBeDeleted, TestGrid.DeleteMenuItem.Enabled);
				((ZGridColumnInfo)TestGrid.ColumnStyles[0]).IsReadOnly = false;
				((ZGridColumnInfo)TestGrid.ColumnStyles[1]).IsReadOnly = false;
				TestGrid.PopupContextMenu();
				Assert(TestGrid.DeleteMenuItem.Visible);
				Assert(TestGrid.DeleteMenuItem.Enabled);

				object1.ReadOnly = true;
				object2.ReadOnly = true;
				object3.ReadOnly = true;
				TestGrid.PopupContextMenu();
				Assert(TestGrid.DeleteMenuItem.Visible);
				AssertEquals(allowReadOnlyRowsToBeDeleted, TestGrid.DeleteMenuItem.Enabled);
				object1.ReadOnly = false;
				object2.ReadOnly = false;
				object3.ReadOnly = false;
				TestGrid.PopupContextMenu();
				Assert(TestGrid.DeleteMenuItem.Visible);
				Assert(TestGrid.DeleteMenuItem.Enabled);
			}
		}

		#endregion

		public void TestDeleteMenuItemClick()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var object1 = collection.AddNew();
			var object2 = collection.AddNew();
			var object3 = collection.AddNew();
			AfterRowDeletedIsImplemented = false;

			using (var testForm = new ZForm())
			{
				TestGrid.Parent = testForm;
				TestGrid.SetDataBinding(collection, "");
				testForm.Show();

				var list = TestGrid.ListManager.List;

				TestGrid.RowsDeleting += TestGrid_RowDeleting;
				TestGrid.RowsDeleted += TestGrid_RowDeleted;

				var hitInfo = TestGrid.HitTest(TestGrid.RowHeaderWidth - 1, TestGrid.PreferredRowHeight + 10);
				AssertEquals("Hit On Row 0", 0, hitInfo.Row);
				AssertEquals("Hit On Row Header", -1, hitInfo.Column);
				AssertEquals("Hit On Row Header", DataGrid.HitTestType.RowHeader, hitInfo.Type);

				RowDeletingCancelResult = true;
				TestGrid.DeleteRow(hitInfo);

				AssertEquals("Should be 1 Row in RowDeletingEventArgs", 1, PreviousArgs.Objects.Count());
				AssertEquals("Row should be Row1 in RowDeletingEventArgs", object1, PreviousArgs.Objects.ElementAt(0));

				AssertEquals("Row1 should still be the first row of the grid", object1, list[0]);
				Assert(AfterRowDeletedIsImplemented);

				TestGrid.Select(1);
				TestGrid.Select(2);

				Assert("Index 0 should not be selected", !TestGrid.IsSelected(0));
				Assert("Index 1 should be selected", TestGrid.IsSelected(1));
				Assert("Index 2 should be selected", TestGrid.IsSelected(2));

				RowDeletingCancelResult = false;
				TestGrid.DeleteRow(hitInfo);

				AssertEquals("Should be 2 Rows in RowDeletingEventArgs", 2, PreviousArgs.Objects.Count());
				AssertEquals("Row at 0 should be Row2 in RowDeletingEventArgs", object3, PreviousArgs.Objects.ElementAt(0));
				AssertEquals("Row at 1 should be Row3 in RowDeletingEventArgs", object2, PreviousArgs.Objects.ElementAt(1));

				AssertEquals("Only one row should be left in the Grid", 1, list.Count);
				AssertEquals("Row1 should still be the first row of the grid", object1, list[0]);
			}
		}

		public void TestDeleteMenuItemClick_MixData_DeleteTheEditableAndLeaveTheReadonly()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var object1 = collection.AddNew();
			var object2 = collection.AddNew();
			var object3 = collection.AddNew();
			var object4 = collection.AddNew();
			var object5 = collection.AddNew();
			var object6 = collection.AddNew();

			object1.ReadOnly = true;
			object2.ReadOnly = false;
			object3.ReadOnly = true;
			object4.ReadOnly = false;
			object5.ReadOnly = true;
			object6.ReadOnly = false;

			using (var testForm = new ZForm())
			{
				TestGrid.Parent = testForm;
				TestGrid.SetDataBinding(collection, "");
				testForm.Show();

				var list = TestGrid.ListManager.List;

				var hitInfo = TestGrid.HitTest(TestGrid.RowHeaderWidth - 1, TestGrid.PreferredRowHeight + 10);
				TestGrid.Select(1);
				TestGrid.Select(2);
				TestGrid.Select(3);
				TestGrid.DeleteRow(hitInfo);

				AssertEquals(4, list.Count);
				AssertEquals(object1, collection[0]);
				AssertEquals(object3, collection[1]);
				AssertEquals(object5, collection[2]);
				AssertEquals(object6, collection[3]);
				AssertEquals("Error Cannot delete read-only rows.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestChangingCellWithDoubleBinding()
		{
			var data = GetTestDataSetWithDataRelation(); // This is legacy architecture that will be removed

			using (var testForm = new ZForm())
			{
				var masterGrid = new ZGrid();
				var detailGrid = new TestZGrid();
				var textBox = new ZTextBox();

				testForm.Show();
				PopulateAndBindGridControls(testForm, masterGrid, detailGrid, data);
				PopulateAndBindTextBox(testForm, textBox, data);

				masterGrid.Focus();
				AssertNotNull("LastFocusedColumn should be set when Grid gets Focus", masterGrid.LastFocusedColumn);
				AssertEquals("Should be Editing Column 0", 0, masterGrid.CurrentCell.ColumnNumber);
				AssertEquals("Should be Editing Row 0", 0, masterGrid.CurrentCell.RowNumber);

				KeySender.PostKeyDown(masterGrid.LastFocusedColumn.EditControl, masterGrid.LastFocusedColumn.EditControl.Handle, Keys.A);
				KeySender.PostKeyDown(masterGrid.LastFocusedColumn.EditControl, masterGrid.LastFocusedColumn.EditControl.Handle, Keys.U);
				KeySender.PostKeyDown(masterGrid.LastFocusedColumn.EditControl, masterGrid.LastFocusedColumn.EditControl.Handle, Keys.D);

				Application.DoEvents(); // Allow message loop to be processed

				masterGrid.CurrentCell = new DataGridCell(0, 1);
				AssertEquals("Should be Editing Column 1", 1, masterGrid.CurrentCell.ColumnNumber);
				AssertEquals("Should be Editing Row 0", 0, masterGrid.CurrentCell.RowNumber);

				var valueFromGrid = ((DataRowView)masterGrid.ListManager.List[0]).Row["MasterColumn1"].ToString();
				var valueFromTextBox = textBox.Text;

				AssertEquals("Grid Value", "AUD", valueFromGrid);
				AssertEquals("TextBox Value", "AUD", valueFromTextBox);

				masterGrid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("Cell change with no data change doesn't cause edit of new row", true, ((DataRowView)masterGrid.ListManager.List[1]).IsNew);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule", Justification = "Testing")]
		public void TestSortEatsNonEditedDetachedRow()
		{
			var data = GetTestDataSetWithDataRelation(); // This is legacy architecture that will be removed

			using (var testForm = new ZForm())
			{
				var masterGrid = new TestZGrid();
				var detailGrid = new TestZGrid();

				testForm.Show();
				Application.DoEvents();
				PopulateAndBindGridControls(testForm, masterGrid, detailGrid, data);

				masterGrid.Focus();
				var masterRowView = masterGrid.ListManager.List[0] as DataRowView;

				masterRowView[0] = "1";
				masterRowView[1] = "Master";
				masterRowView[2] = 100.01M;

				masterGrid.ListManager.AddNew();
				masterRowView = masterGrid.ListManager.List[1] as DataRowView;
				masterRowView[0] = "2";
				masterRowView[1] = "Master";
				masterRowView[2] = 100.02M;
				masterRowView.EndEdit();
				masterGrid.SetCellEdited();

				detailGrid.Focus();
				var detailManager = detailGrid.BindingContext[data, "MasterTable.MasterDetail"] as CurrencyManager;
				detailGrid.CurrentCell = new DataGridCell(0, 1);
				KeySender.SendKeyPress(detailGrid, Keys.D2);
				KeySender.SendKeyPress(detailGrid, Keys.Tab);
				detailGrid.CurrentCell = new DataGridCell(1, 1);
				Application.DoEvents();
				var detailRowView = detailManager.Current as DataRowView;

				AssertEquals("DetailRow FK", "2", detailRowView[1].ToString());

				var clickX = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
				var clickY = ControlDpiScalingHelper.ScaleToCurrentDpiY(10);

				Assert("Grid should not yet be sorted", !((IBindingList)detailGrid.ListManager.List).IsSorted);

				AssertEquals("Click will hit ColumnHeader", DataGrid.HitTestType.ColumnHeader, detailGrid.HitTest(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(clickX, clickY)).Type);
				detailGrid.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, clickX, clickY, 0));
				detailGrid.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, clickX, clickY, 0));

				Assert("Grid should now be sorted", ((IBindingList)detailGrid.ListManager.List).IsSorted);
				AssertEquals("Rows Not Eaten by Sort Bug", 1, detailGrid.ListManager.List.Count);
			}
		}

		public void TestImplementsIDataBoundControl()
		{
			IDataBoundControl testInterface = TestGrid;
			AssertNotNull("ZGrid does not implement IDataBoundControl.", testInterface);
		}

		public void TestDataBinding()
		{
			AssertNull("DataSource was not null.", TestGrid.DataSource);

			using (var myForm = new ZForm())
			{
				TestGrid.Parent = myForm;
				TestGrid.SetDataBinding(DataSetToBind, GridTableName);
				myForm.Show();
				AssertEquals("AllowSorting for default table style should match grids table style", TestGrid.AllowSorting, TestGrid.TableStyles[0].AllowSorting);
				TestGrid.SetDataBinding(null, "");
				TestGrid.AllowSorting = false;
				TestGrid.SetDataBinding(DataSetToBind, GridTableName);
				AssertEquals("AllowSorting for default table style should match grids table style", TestGrid.AllowSorting, TestGrid.TableStyles[0].AllowSorting);
				AssertNotNull("DataSource was null.", TestGrid.DataSource as DataSet); // This is legacy architecture that will be removed
			}
		}

		public void TestDataBindingWithTableName()
		{
			AssertNull("DataSource", TestGrid.DataSource);
			using (var myForm = new ZForm())
			{
				TestGrid.Parent = myForm;
				TestGrid.SetDataBinding(DataSetToBind, "BaseTable." + DataSetToBind.Relations[0].RelationName, "GridTable");
				myForm.Show();
				AssertNotNull("DataSource", TestGrid.DataSource as DataSet); // This is legacy architecture that will be removed
			}
		}

		public void TestContextMenuItemsAreAdded()
		{
			Assert("Context menu items should be present before binding", TestGrid.ContextMenu.MenuItems.Count >= 2);
			using (var myForm = new ZForm())
			{
				TestGrid.Parent = myForm;
				TestGrid.SetDataBinding(DataSetToBind, GridTableName);
				myForm.Show();

				Assert("Context menu items should be added", TestGrid.ContextMenu.MenuItems.Count >= 2);
				AssertNotNull("Customize columns context menu item should be added", TestGrid.ContextMenu.MenuItems.FindByText("&Customize Columns"));
				AssertNotNull("Delete context menu item should be added", TestGrid.ContextMenu.MenuItems.FindByText("&Delete"));
			}

			AssertNull("No context menu should be present after being disposed", TestGrid.ContextMenu);
		}

		public void TestCustomiseMenuProperty()
		{
			TestGrid.IsCustomiseMenuVisible = true;
			AssertEquals("Customize menu item is not visible.", true, TestGrid.IsCustomiseMenuVisible);
			TestGrid.IsCustomiseMenuVisible = false;
			AssertEquals("Customize menu item is visible.", false, TestGrid.IsCustomiseMenuVisible);
		}

		public void TestSaveLoadColumnsLayout()
		{
			using (var myForm = new ZForm())
			{
				TestGrid.Parent = myForm;
				TestGrid.SetDataBinding(DataSetToBind, GridTableName);
				myForm.Show();

				TestGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				TestGrid.Columns[1].IsVisible = true;

				using (var layoutStream = new TestDataGridLayoutManager().SerialiseAndGetCurrentLayoutStream(TestGrid))
				{
					TestGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
					TestGrid.Columns[1].IsVisible = false;

					new TestDataGridLayoutManager().LoadLayout(TestGrid, layoutStream);

					AssertEquals("Column width should be 100.", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), TestGrid.Columns[0].ColumnStyle.Width);
					AssertEquals("Column should be visible.", true, TestGrid.Columns[1].IsVisible);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule", Justification = "Testing")]
		public void TestBindingToDataRelationship()
		{
			var data = GetTestDataSetWithDataRelation(); // This is legacy architecture that will be removed

			using (var testForm = new ZForm())
			{
				var masterGrid = new ZGrid();
				var detailGrid = new ZGrid();

				testForm.Show();
				PopulateAndBindGridControls(testForm, masterGrid, detailGrid, data);

				masterGrid.Focus();
				var masterManager = masterGrid.BindingContext[data, "MasterTable"] as CurrencyManager;
				masterManager.AddNew();

				var masterRowView = masterManager.Current as DataRowView;
				masterRowView[0] = "1";
				masterRowView[1] = "Master";
				masterRowView[1] = 100.01M;

				masterManager.EndCurrentEdit();
				masterManager.Refresh(); // simulates the onleave refresh manager call
				detailGrid.Focus();

				var detailManager = detailGrid.BindingContext[data, "MasterTable.MasterDetail"] as CurrencyManager;
				detailManager.AddNew();
				var detailRowView = detailManager.Current as DataRowView;

				AssertEquals("DetailRow FK", "1", detailRowView[1].ToString());
			}
		}

		public void TestBindingWithoutColumnStylesWillThrowException()
		{
			var data = new DataSet(); // This is legacy architecture that will be removed
			var table = new DataTable("TestTable");
			var column1 = new DataColumn("Column1");
			var column2 = new DataColumn("Column2");

			data.Tables.Add(table);
			table.Columns.Add(column1);
			table.Columns.Add(column2);

			using (var form = new ZForm())
			using (var testGrid1 = new ZGrid())
			{
				try
				{
					form.Controls.Add(testGrid1);
					form.Show();
					testGrid1.SetDataBinding(data, "TestTable");
					Fail("Exception was not thrown.");
				}
				catch (InvalidOperationException e)
				{
					AssertEquals("Please use the Grid Designer to setup your columns in the ZGrid.", e.Message);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1020:DontUseCurrencyManagerCurrentRule", Justification = "Testing")]
		public void TestSelectedRowsRemainSelectedAfterLosingFocus()
		{
			var data = new DataSet(); // This is legacy architecture that will be removed
			var table = new DataTable("Table");
			var column = new DataColumn("Column");
			table.Columns.Add(column);
			data.Tables.Add(table);

			using (var aForm = new ZForm())
			{
				var aGrid = new ZGrid();
				aGrid.Columns.AddTextColumn("Column", ControlDpiScalingHelper.ScaleToCurrentDpiX(50));
				var aTextBox = new ZTextBox();
				aForm.Controls.Add(aGrid);
				aForm.Controls.Add(aTextBox);

				aForm.Show();
				aGrid.SetDataBinding(data, "Table");
				aGrid.Focus();

				var manager = aGrid.BindingContext[data, "Table"] as CurrencyManager;
				manager.AddNew();
				var rowView = manager.Current as DataRowView;
				rowView[0] = "1";
				manager.AddNew();
				rowView = manager.Current as DataRowView;
				rowView[0] = "2";
				manager.AddNew();
				rowView = manager.Current as DataRowView;
				rowView[0] = "3";
				manager.EndCurrentEdit();

				aGrid.Select(0);
				aGrid.Select(1);

				Assert("Row 0 - Selected Before", aGrid.IsSelected(0));
				Assert("Row 1 - Selected Before", aGrid.IsSelected(1));
				Assert("Row 2 - Not Selected Before", !aGrid.IsSelected(2));

				aTextBox.Focus();

				Assert("Row 0 - Selected After", aGrid.IsSelected(0));
				Assert("Row 1 - Selected After", aGrid.IsSelected(1));
				Assert("Row 2 - Not Selected After", !aGrid.IsSelected(2));

				Assert("TextBox should be focused", aTextBox.Focused);
			}
		}

		[ExpectNoExceptions]
		public void TestIsErrored()
		{
			using (var form = new ZForm())
			{
				var grid = new TestZGrid();
				form.Controls.Add(grid);
				form.Show();
				grid.IsErrored = true;
				Assert(grid.IsErrored);
				grid.IsErrored = false;
				Assert(!grid.IsErrored);
			}
		}

		#region Custom Row Background Colour

		public void TestGetCustomRowBackgroundColour()
		{
			using (var form = new ZForm())
			{
				var collection = new GridElementCollection();
				collection.AddNew("Y");
				collection.AddNew("N");

				var grid = new ZGrid();
				grid.Dock = DockStyle.Fill;
				grid.Columns.AddTextColumn("Code", 80);
				grid.ColourDeciding += Grid_ColourDeciding;

				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(collection, "", "");

				AssertEquals("GetCustomRowBackgroundColour(0)", Color.White, grid.GetCustomRowBackgroundColour(0));
				AssertEquals("GetCustomRowBackgroundColour(1)", Color.Black, grid.GetCustomRowBackgroundColour(1));
			}
		}

		static void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var element = e.ObjectAtRow as GridElement;
			e.Colour = (element != null && element.Code == "Y") ? Color.White : Color.Black;
		}

		#endregion

		#region Custom Cell Font

		public void TestGetCustomCellFont()
		{
			using (var form = new ZForm())
			{
				var collection = new GridElementCollection();
				collection.AddNew("BOLD", "Bold Description");
				collection.AddNew("ITALIC", "Italic Description");
				collection.AddNew("STANDARD", "Standard Description");

				var grid = new ZGrid();
				grid.Dock = DockStyle.Fill;

				grid.Columns.AddTextColumn("Code", 80);
				grid.Columns.AddTextColumn("Description", 150);
				grid.FontDeciding += Grid_FontDeciding;

				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(collection, "", "");

				AssertNull("Should only customise Description cell", grid.GetCustomCellFont(0, "Code"));
				AssertEquals("Should be bold", FontStyle.Bold, grid.GetCustomCellFont(0, "Description").Style);
				AssertNull("Should only customise Description cell", grid.GetCustomCellFont(1, "Code"));
				AssertEquals("Should be italic", FontStyle.Italic, grid.GetCustomCellFont(1, "Description").Style);
				AssertNull("Should only customise Description cell", grid.GetCustomCellFont(2, "Code"));
				AssertEquals("Should use the default grid's font", grid.Font, grid.GetCustomCellFont(2, "Description"));
			}
		}

		public void TestFontDecidingEventNotHooked()
		{
			using (var form = new ZForm())
			{
				var collection = new GridElementCollection();
				collection.AddNew("BOLD", "Bold Description");
				collection.AddNew("ITALIC", "Italic Description");

				var grid = new ZGrid();
				grid.Dock = DockStyle.Fill;
				grid.Columns.AddTextColumn("Code", 80);
				grid.Columns.AddTextColumn("Description", 150);

				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(collection, "", "");

				Application.DoEvents();

				AssertNull("Should return null if unhooked", grid.GetCustomCellFont(0, "Code"));
				AssertNull("Should return null if unhooked", grid.GetCustomCellFont(0, "Description"));
				AssertNull("Should return null if unhooked", grid.GetCustomCellFont(1, "Code"));
				AssertNull("Should return null if unhooked", grid.GetCustomCellFont(1, "Description"));
			}
		}

		static void Grid_FontDeciding(object sender, FontDecidingEventArgs e)
		{
			var element = e.ObjectAtRow as GridElement;
			if (e.DataMember == "Description")
			{
				switch (element.Code)
				{
					case "BOLD":
						e.Font = new Font(e.OriginalFont, FontStyle.Bold);
						break;

					case "ITALIC":
						e.Font = new Font(e.OriginalFont, FontStyle.Italic);
						break;

					case "STANDARD":
						e.Font = e.OriginalFont;
						break;
				}
			}
		}

		#endregion

		#region Implementation

		bool RowDeletingCancelResult;
		RowsDeletingEventArgs PreviousArgs;
		bool AfterRowDeletedIsImplemented;

		void TestGrid_RowDeleting(object sender, RowsDeletingEventArgs e)
		{
			e.Cancel = RowDeletingCancelResult;
			PreviousArgs = e;
		}

		void TestGrid_RowDeleted(object sender, RowsDeletingEventArgs e)
		{
			AfterRowDeletedIsImplemented = true;
		}

		internal TestZGrid TestGrid;
		DataTable GridTable;
		DataTable BaseTable;
		DataSet DataSetToBind; // This is legacy architecture that will be removed
		readonly string GridTableName = "GridTable";

		protected override void SetUp()
		{
			base.SetUp();

			TestGrid = new TestZGrid();
			//TestGrid.Columns.AddTextColumn("PK", 200, true, true);
			//TestGrid.Columns.AddTextColumn("Name", 200, true, true);
			TestGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("PK", 200));
			TestGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("Name", 200));
			DataSetToBind = GetTestDataSet();
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestGrid.Dispose();
		}

		DataSet GetTestDataSet() // This is legacy architecture that will be removed
		{
			var result = new DataSet(); // This is legacy architecture that will be removed
			BaseTable = new DataTable("BaseTable");
			var c1 = new DataColumn("PK", typeof(Guid));
			BaseTable.Columns.Add(c1);

			GridTable = new DataTable(GridTableName);
			var c2 = new DataColumn("PK", typeof(Guid));
			var c3 = new DataColumn("FK", typeof(Guid));
			var c4 = new DataColumn("Name", typeof(string));
			GridTable.Columns.Add(c2);
			GridTable.Columns.Add(c3);
			GridTable.Columns.Add(c4);
			result.Tables.Add(BaseTable);
			result.Tables.Add(GridTable);

			result.Relations.Add(c1, c3);
			return result;
		}

		static DataSet GetTestDataSetWithDataRelation() // This is legacy architecture that will be removed
		{
			var data = new DataSet(); // This is legacy architecture that will be removed
			var masterTable = new DataTable("MasterTable");
			var masterColumn1 = new DataColumn("MasterColumn1");
			var masterColumn2 = new DataColumn("MasterColumn2");
			var masterColumn3 = new DataColumn("MasterColumn3", typeof(Decimal));

			data.Tables.Add(masterTable);
			masterTable.Columns.Add(masterColumn1);
			masterTable.Columns.Add(masterColumn2);
			masterTable.Columns.Add(masterColumn3);

			var detailTable = new DataTable("DetailTable");
			var detailColumn1 = new DataColumn("DetailColumn1");
			var detailColumn2 = new DataColumn("DetailColumn2");

			data.Tables.Add(detailTable);
			detailTable.Columns.Add(detailColumn1);
			detailTable.Columns.Add(detailColumn2);

			var relation = new DataRelation("MasterDetail", masterColumn1, detailColumn2, true);
			data.Relations.Add(relation);

			return data;
		}

		static void PopulateAndBindGridControls(ZForm testForm, ZGrid masterGrid, ZGrid detailGrid, DataSet data) // This is legacy architecture that will be removed
		{
			masterGrid.Columns.AddTextColumn("MasterColumn1", 100);
			masterGrid.Columns.AddTextColumn("MasterColumn2", 100);
			masterGrid.Columns.AddCalcEditColumn("MasterColumn3", 100, 2);
			masterGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10);
			masterGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 150);
			testForm.Controls.Add(masterGrid);
			masterGrid.SetDataBinding(data, "MasterTable");

			detailGrid.Columns.AddTextColumn("DetailColumn1", 100);
			detailGrid.Columns.AddTextColumn("DetailColumn2", 100);
			detailGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 200);
			detailGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 150);
			testForm.Controls.Add(detailGrid);
			detailGrid.SetDataBinding(data, "MasterTable.MasterDetail", "DetailTable");

			testForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 400);
		}

		static void PopulateAndBindTextBox(ZForm testForm, ZTextBox textBox, DataSet data) // This is legacy architecture that will be removed
		{
			textBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 180);
			textBox.BindTo = "MasterTable.MasterColumn1";
			DataBoundControl.GetDefaultImplementation(textBox).SetDataBinding(data, textBox.BindTo);
			testForm.Controls.Add(textBox);
		}

		#endregion

		#region class GridElement

		sealed class GridElement
		{
			public string Code { get; set; }
			public string Description { get; set; }
		}

		#endregion

		#region class GridElementCollection

		sealed class GridElementCollection : CollectionBase
		{
			public void AddNew(string code)
			{
				AddNew(code, "");
			}

			public void AddNew(string code, string description)
			{
				List.Add(new GridElement { Code = code, Description = description });
			}
		}

		#endregion
	}
}
