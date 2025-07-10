using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

#pragma warning disable CW1108 // Do Not Use DataSet

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridTest3 : TransactionedTestCase
	{
		#region Base Tests

		public void TestBaseGridColumns()
		{
			using (var testGrid = new ZGrid())
			{
				AssertNotNull("Columns", testGrid.Columns);
				AssertEquals("Column Count", 0, testGrid.Columns.Count);
			}
		}

		public void TestContextMenu_TotalMenuItems()
		{
			EnvProxy.Instance.Registry.ExportGridLayoutDetails = true;
			using (var testGrid = new ZGrid())
			{
				AssertNotNull("Context Menu", testGrid.ContextMenu);
				AssertEquals("Context Menu Items", 20, testGrid.ContextMenu.MenuItems.Count);
			}

			EnvProxy.Instance.Registry.ExportGridLayoutDetails = false;
			using (var testGrid = new ZGrid())
			{
				AssertNotNull("Context Menu", testGrid.ContextMenu);
				AssertEquals("Context Menu Items", 19, testGrid.ContextMenu.MenuItems.Count);
			}
		}

		public void TestMenuItem_ViewGridNotifications()
		{
			using (var testGrid = new ZGrid())
			{
				AssertNotNull("Context Menu should not be null", testGrid.ContextMenu);

				var viewNotificationsMenuItem = testGrid.ContextMenu.MenuItems.FindByText("View Grid Notifications");

				AssertNotNull("Menu item 'View Grid Notifications' should be in the context menu", viewNotificationsMenuItem);
				AssertEquals("Menu item 'View Grid Notifications' should be invisible", expected: false, viewNotificationsMenuItem.Visible);

				var separatorMenuItem = testGrid.ContextMenu.MenuItems[viewNotificationsMenuItem.Index - 1];

				AssertEquals("The previous menu item should be a separator", "-", separatorMenuItem.Text);
				AssertEquals("Separator menu item should be invisible as well", expected: false, separatorMenuItem.Visible);
			}

			var dummy = new BusinessObjectCollectionNotificationsViewerProvider(new BusinessObjectFactory());

			using (var form = new ZForm(dummy))
			using (var testGrid = new ZGrid())
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(300, 300);
				testGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
				testGrid.Size = ControlDpiScalingHelper.NewScaledSize(200, 200);
				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });
				form.Controls.Add(testGrid);
				testGrid.SetDataBinding(dummy, "");
				testGrid.ContextMenu_Popup(testGrid, EventArgs.Empty);

				AssertNotNull("Context Menu should not be null", testGrid.ContextMenu);

				var viewNotificationsMenuItem = testGrid.ContextMenu.MenuItems.FindByText("View Grid Notifications");

				AssertNotNull("Menu item 'View Grid Notifications' should be in the context menu", viewNotificationsMenuItem);
				AssertEquals("Menu item 'View Grid Notifications' should be visible", expected: true, viewNotificationsMenuItem.Visible);

				var separatorMenuItem = testGrid.ContextMenu.MenuItems[viewNotificationsMenuItem.Index - 1];

				AssertEquals("The previous menu item should be a separator", "-", separatorMenuItem.Text);
				AssertEquals("Separator menu item should be visible as well", expected: true, separatorMenuItem.Visible);
			}
		}

		public void TestContextMenu_IndexedMenuItemsAreInCorrectOrder()
		{
			using (var testGrid = new ZGrid())
			{
				AssertEquals("&Find", testGrid.ContextMenu.MenuItems[0].Text);
				AssertEquals("Find &Next", testGrid.ContextMenu.MenuItems[1].Text);
				AssertEquals("Find &Previous", testGrid.ContextMenu.MenuItems[2].Text);
				AssertEquals("-", testGrid.ContextMenu.MenuItems[3].Text);
				AssertEquals("&Tick All in Current Column", testGrid.ContextMenu.MenuItems[4].Text);
				AssertEquals("&Untick All in Current Column", testGrid.ContextMenu.MenuItems[5].Text);
				AssertEquals("-", testGrid.ContextMenu.MenuItems[6].Text);
				AssertEquals("&Customize Columns", testGrid.ContextMenu.MenuItems[7].Text);
				AssertEquals("&Copy to New Row", testGrid.ContextMenu.MenuItems[8].Text);
				AssertEquals("&Delete", testGrid.ContextMenu.MenuItems[9].Text);
			}
		}

		[ExpectNoExceptions]
		public void TestSuspendingRefreshTableStyles()
		{
			var mockGrid = new Mock<ZGrid>() { CallBase = true };
			using (var grid = mockGrid.Object)
			{
				using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
				{
					grid.RefreshTableStyles();
					grid.RefreshTableStyles();
					grid.RefreshTableStyles();
					mockGrid.Verify();
					mockGrid.Protected().Verify("RefreshTableStylesCore", Times.Never());
				}
				mockGrid.Protected().Verify("RefreshTableStylesCore", Times.Once());
				grid.RefreshTableStyles();
				mockGrid.Protected().Verify("RefreshTableStylesCore", Times.Exactly(2));
			}
		}

		[ExpectNoExceptions]
		public void TestSuspendingRefreshTableStylesAndNotRefreshAtDisposal()
		{
			var mockGrid = new Mock<ZGrid>() { CallBase = true };
			using (var grid = mockGrid.Object)
			{
				using (grid.SuspendRefreshTableStylesAndNotRefreshAtDisposal())
				{
					grid.RefreshTableStyles();
					grid.RefreshTableStyles();
					grid.RefreshTableStyles();
					mockGrid.Verify();
					mockGrid.Protected().Verify("RefreshTableStylesCore", Times.Never());
				}
				mockGrid.Protected().Verify("RefreshTableStylesCore", Times.Never());
				grid.RefreshTableStyles();
				mockGrid.Protected().Verify("RefreshTableStylesCore", Times.Exactly(1));
			}
		}

		public void TestPersistColumnsLayoutIncludingSortInformation()
		{
			var data = new DataSet(); // This is legacy architecture that will be removed
			var table1 = new DataTable("Table1");
			data.Tables.Add(table1);

			var testColumn1 = new DataColumn("TestColumn1");
			var testColumn2 = new DataColumn("TestColumn2");
			var testColumn3 = new DataColumn("TestColumn3");
			table1.Columns.Add(testColumn1);
			table1.Columns.Add(testColumn2);
			table1.Columns.Add(testColumn3);

			var row1 = table1.NewRow();
			row1[testColumn1] = 1;
			row1[testColumn2] = 1;
			table1.Rows.Add(row1);

			var row2 = table1.NewRow();
			row2[testColumn1] = 2;
			row2[testColumn2] = 2;
			table1.Rows.Add(row2);

			var row3 = table1.NewRow();
			row3[testColumn1] = 3;
			row3[testColumn2] = 3;
			table1.Rows.Add(row3);

			using (var testForm = new KForm())
			{
				var testGrid = new ZGrid();
				testGrid.Columns.AddTextColumn("TestColumn1", 100);
				testGrid.Columns.AddTextColumn("TestColumn2", 100);
				testGrid.Columns.AddTextColumn("TestColumn3", 100);
				testForm.Controls.Add(testGrid);

				testGrid.SetDataBinding(data, "Table1");
				testGrid.RefreshTableStyles();

				testForm.Show();

				var manager = testGrid.ListManager;

				var list = (IBindingList)testGrid.ListManager.List;

				testGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				testGrid.Columns[1].IsVisible = true;

				var testColumn2Descriptor = manager.GetItemProperties()["TestColumn2"];
				list.ApplySort(testColumn2Descriptor, ListSortDirection.Descending);
				AssertEquals("First Row", row3, ((DataRowView)list[0]).Row);

				using (var layoutStream = new TestDataGridLayoutManager().SerialiseAndGetCurrentLayoutStream(testGrid))
				{
					testGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
					testGrid.Columns[1].IsVisible = false;
					var testColumn1Descriptor = manager.GetItemProperties()["TestColumn1"];
					list.ApplySort(testColumn1Descriptor, ListSortDirection.Ascending);
					AssertEquals("First Row", row1, ((DataRowView)list[0]).Row);

					new TestDataGridLayoutManager().LoadLayout(testGrid, layoutStream);

					AssertEquals("Column width should be 100.", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), testGrid.Columns[0].ColumnStyle.Width);
					AssertEquals("Column should be visible.", true, testGrid.Columns[1].IsVisible);
					AssertEquals("SortProperty.Name", "TestColumn2", list.SortProperty.Name);
					AssertEquals("SortDirection", ListSortDirection.Descending, list.SortDirection);
					AssertEquals("First Row", row3, ((DataRowView)list[0]).Row);
				}
			}
		}

		public void TestMandatoryColumnsAlwaysAdded()
		{
			var data = new DataSet(); // This is legacy architecture that will be removed
			var table1 = new DataTable("Table1");
			data.Tables.Add(table1);

			var testColumn1 = new DataColumn("TestColumn1");
			var testColumn2 = new DataColumn("TestColumn2");
			var testColumn3 = new DataColumn("TestColumn3");
			table1.Columns.Add(testColumn1);
			table1.Columns.Add(testColumn2);
			table1.Columns.Add(testColumn3);

			using (var testForm = new KForm())
			{
				var testGrid = new ZGrid();
				testGrid.Columns.AddTextColumn("TestColumn1", 100);
				testGrid.Columns.AddTextColumn("TestColumn2", 100);
				testGrid.Columns.AddTextColumn("TestColumn3", 100);
				testForm.Controls.Add(testGrid);

				testGrid.SetDataBinding(data, "Table1");
				testGrid.RefreshTableStyles();

				testForm.Show();

				testGrid.Columns[0].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				testGrid.Columns[1].IsVisible = false;

				using (var layoutStream = new TestDataGridLayoutManager().SerialiseAndGetCurrentLayoutStream(testGrid))
				{
					AssertEquals(false, testGrid.Columns[1].IsVisible);

					new TestDataGridLayoutManager().LoadLayout(testGrid, layoutStream);
					AssertEquals("Mandatory Columns are reloaded as visible", true, testGrid.Columns[1].IsVisible);
				}
			}
		}

		#endregion

		#region ReadOnly Cells

		public void TestTabbing()
		{
			using (var data = new DataSet()) // This is legacy architecture that will be removed
			using (var table = data.Tables.Add("Table1"))
			using (var form = new ZForm())
			{
				InitialiseData_ForTestTabbing(table);

				ZTextBox textBox;
				ZGrid grid;
				InitialiseForm_ForTestTabbing(form, out textBox, out grid, data);

				form.ActiveControl = textBox;

				PostTabKeyToActiveControl(form);
				AssertEquals("Current Cell", new DataGridCell(0, 1), grid.CurrentCell);

				PostTabKeyToActiveControl(form);
				AssertEquals("Current Cell", new DataGridCell(0, 2), grid.CurrentCell);

				PostTabKeyToActiveControl(form);
				AssertEquals("Active Control", textBox, MostActiveControl(form));

				grid.CurrentCell = new DataGridCell(0, 2);

				PostTabKeyToActiveControl(form);
				AssertEquals("Tabbing into Grid, Current Cell", new DataGridCell(0, 1), grid.CurrentCell);

				grid.CurrentCell = new DataGridCell(0, 2);
				textBox.Focus();

				grid.Focus();
				grid.Select(0);

				grid.AllowReadOnlyRowsToBeDeleted = true;
				PostKeyToActiveControl(form, Keys.Delete);
				AssertEquals("Grid ListManager.Position", -1, grid.ListManager.Position);

				PostTabKeyToActiveControl(form);
				AssertEquals("Tabbing into Grid, Current Cell", new DataGridCell(0, 1), grid.CurrentCell);
				AssertEquals("Active Control", grid.LastFocusedColumn.EditControl, MostActiveControl(form));

				PostKeyToActiveControl(form, Keys.A);
				PostTabKeyToActiveControl(form);
				AssertEquals("Tabbing into Grid, Current Cell", new DataGridCell(0, 2), grid.CurrentCell);

				PostTabKeyToActiveControl(form);
				AssertEquals("Tabbing into Grid, Current Cell", new DataGridCell(1, 1), grid.CurrentCell);

				PostTabKeyToActiveControl(form);
				AssertEquals("Tabbing into Grid, Current Cell", new DataGridCell(1, 2), grid.CurrentCell);

				PostTabKeyToActiveControl(form);
				AssertEquals("Active Control", textBox, MostActiveControl(form));
			}
		}

		static void InitialiseData_ForTestTabbing(DataTable table)
		{
			table.Columns.Add("Column1");
			table.Columns.Add("Column2");
			table.Columns.Add("Column3");
		}

		static void InitialiseForm_ForTestTabbing(Control form, out ZTextBox textBox, out ZGrid grid, DataSet data)
		{
			form.Height = 250;

			textBox = new ZTextBox { TabIndex = 1 };

			grid = new ZGrid { TabIndex = 2, Width = form.ClientRectangle.Width, Top = (textBox.Bottom + 1) };
			grid.Columns.Add(new ZTextBoxColumnStyleInfo("Column1", 80) { IsReadOnly = true });
			grid.Columns.Add(new ZTextBoxColumnStyleInfo("Column2", 80));
			grid.Columns.Add(new ZTextBoxColumnStyleInfo("Column3", 80));

			form.Controls.Add(textBox);
			form.Controls.Add(grid);
			form.Show();
			grid.SetDataBinding(data, "Table1");
		}

		static void PostTabKeyToActiveControl(Control control)
		{
			PostKeyToActiveControl(control, Keys.Tab);
		}

		#endregion

		#region Selecting

		public void TestGetSelectedRows()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				dummy.Collection.Add(factory.New<DummyBusinessObject>());
				dummy.Collection.Add(factory.New<DummyBusinessObject>());
				dummy.Collection.Add(factory.New<DummyBusinessObject>());
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				form.Controls.Add(grid);
				form.Show();
				grid.Focus();
				Application.DoEvents();

				AssertNotNull(grid.GetSelectedRows());
				AssertEquals(0, grid.GetSelectedRows().Length);

				grid.Select(1);
				AssertNotNull(grid.GetSelectedRows());
				AssertEquals(1, grid.GetSelectedRows().Length);

				grid.Select(2);
				AssertNotNull(grid.GetSelectedRows());
				AssertEquals(2, grid.GetSelectedRows().Length);
			}
		}

		public void TestGetSelectedRowsAfterEnterIsPressed()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				dummy.Collection.Add(factory.New<DummyBusinessObject>());
				dummy.Collection.Add(factory.New<DummyBusinessObject>());
				dummy.Collection.Add(factory.New<DummyBusinessObject>());
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				form.Controls.Add(grid);
				form.Show();
				grid.Focus();
				Application.DoEvents();

				AssertNotNull(grid.GetSelectedRows());
				AssertEquals(0, grid.GetSelectedRows().Length);

				grid.Select(1);
				AssertNotNull(grid.GetSelectedRows());
				AssertEquals(1, grid.GetSelectedRows().Length);

				grid.Select(2);
				AssertNotNull(grid.GetSelectedRows());
				AssertEquals(2, grid.GetSelectedRows().Length);

				KeySender.PostKeyDown(grid, Keys.Enter);
				Application.DoEvents();
				AssertNotNull(grid.GetSelectedRows());
				AssertEquals(0, grid.GetSelectedRows().Length);
			}
		}

		public void TestEnterGoesToFirstEditableCellOnNextLine()
		{
			using (var testForm = new ZForm())
			{
				InitialiseTest(testForm, true);

				TestGrid.CurrentCell = new DataGridCell(0, 1);
				AssertEquals("Current Cell", new DataGridCell(0, 1), TestGrid.CurrentCell);

				KeySender.PostKeyDown(TestGrid, Keys.Enter);
				Application.DoEvents();

				AssertEquals("Current Cell", new DataGridCell(1, 0), TestGrid.CurrentCell);

				TestGrid.Columns[0].ColumnStyle.ReadOnly = true;
				KeySender.PostKeyDown(TestGrid, Keys.Enter);
				Application.DoEvents();

				AssertEquals("Current Cell when first column readonly", new DataGridCell(2, 1), TestGrid.CurrentCell);

				TestGrid.Columns[1].ColumnStyle.ReadOnly = true;
				KeySender.PostKeyDown(TestGrid, Keys.Enter);
				Application.DoEvents();

				AssertEquals("Current Cell when both columns readonly", new DataGridCell(3, 0), TestGrid.CurrentCell);
			}
		}

		public void TestEnterRemainsDefaultWhenIsWholeRowSelectedOnClickIsTrue()
		{
			using (var testForm = new ZForm())
			{
				InitialiseTest(testForm, true);
				TestGrid.IsWholeRowSelectedOnClick = true;

				TestGrid.CurrentCell = new DataGridCell(0, 1);
				KeySender.PostKeyDown(TestGrid, Keys.Enter);
				Application.DoEvents();

				AssertEquals("Current Cell", new DataGridCell(0, 1), TestGrid.CurrentCell);
			}
		}

		public void TestRightClickContextMenu()
		{
			using (var testForm = new ZForm())
			{
				InitialiseTest(testForm, true);

				TestGrid.CurrentCell = new DataGridCell(4, 3);
				AssertEquals("ListManager Position before MouseDown", 4, TestGrid.ListManager.Position);

				var mouseArgs = new MouseEventArgs(MouseButtons.Right, 1, 10, 30, 0);
				AssertEquals("Hit on Row 0", 0, TestGrid.HitTest(mouseArgs.X, mouseArgs.Y).Row);

				TestGrid.OnMouseDown(mouseArgs);
				AssertEquals("ListManager Position before MouseDown", 0, TestGrid.ListManager.Position);
			}
		}

		[ExpectNoExceptions()]
		public void TestClickingOnInvalidRowHeader()
		{
			using (var testForm = new ZForm())
			{
				InitialiseTest(testForm, false);
				var mousePointX = ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
				var mousePointY = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
				var mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, mousePointX, mousePointY, 0);

				var hitInfo = TestGrid.HitTest(mouseArgs.X, mouseArgs.Y);
				AssertEquals("Invalid RowHeader clicked", DataGrid.HitTestType.None, hitInfo.Type);
				AssertEquals("Invalid Row clicked", -1, hitInfo.Row);

				TestGrid.OnMouseDown(mouseArgs);
			}
		}

		public void TestReadOnlyWholeRowSelectGrid()
		{
			var data = new DataSet(); // this is only a unit test
			var table = data.Tables.Add("Table");
			table.Columns.Add("Column1");
			table.Columns.Add("Column2");
			table.Columns.Add("Column3");

			using (var form = new ZForm())
			{
				var testGrid = new TestZGrid();

				form.Controls.Add(testGrid);

				testGrid.Columns.Add(new ZTextBoxColumnStyleInfo("Column1", 80));
				testGrid.Columns.Add(new ZTextBoxColumnStyleInfo("Column2", 80));
				testGrid.Columns.Add(new ZTextBoxColumnStyleInfo("Column3", 80));

				testGrid.SetDataBinding(data, "Table");
				testGrid.Dock = DockStyle.Fill;
				testGrid.IsWholeRowSelectedOnClick = true;
				testGrid.ReadOnly = true;

				form.Show();
				testGrid.Focus();

				testGrid.ListManager.AddNew();
				testGrid.ListManager.AddNew();
				testGrid.ListManager.AddNew();
				testGrid.ListManager.AddNew();

				testGrid.CurrentCell = new DataGridCell(0, 1);
				AssertSelected(testGrid, 0);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.End), Keys.End);
				AssertSelected(testGrid, 3);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.End), Keys.End);
				AssertSelected(testGrid, 3);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Right), Keys.Right);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Right), Keys.Right);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Right), Keys.Right);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Right), Keys.Right);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Right), Keys.Right);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Right), Keys.Right);
				AssertSelected(testGrid, 3);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Home), Keys.Home);
				AssertSelected(testGrid, 0);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Home), Keys.Home);
				AssertSelected(testGrid, 0);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Left), Keys.Left);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Left), Keys.Left);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Left), Keys.Left);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Left), Keys.Left);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Left), Keys.Left);
				AssertSelected(testGrid, 0);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.PageDown), Keys.PageDown);
				AssertSelected(testGrid, 3);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.PageDown), Keys.PageDown);
				AssertSelected(testGrid, 3);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.PageUp), Keys.PageUp);
				AssertSelected(testGrid, 0);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.PageUp), Keys.PageUp);
				AssertSelected(testGrid, 0);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Down), Keys.Down);
				AssertSelected(testGrid, 1);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Down), Keys.Down);
				AssertSelected(testGrid, 2);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Down), Keys.Down);
				AssertSelected(testGrid, 3);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Down), Keys.Down);
				AssertSelected(testGrid, 3);

				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Up), Keys.Up);
				AssertSelected(testGrid, 2);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Up), Keys.Up);
				AssertSelected(testGrid, 1);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Up), Keys.Up);
				AssertSelected(testGrid, 0);
				testGrid.PreProcessMessage(KeySender.GetKeyDownMessage(testGrid.Handle, Keys.Up), Keys.Up);
				AssertSelected(testGrid, 0);
			}
		}

		public void TestAllowMultiSelectTrue_Ctrl_A()
		{
			using (var testForm = new ZForm())
			{
				InitialiseTest(testForm, true);

				Assert("Row 0 is not Selected", !TestGrid.IsSelected(0));
				Assert("Row 1 is not Selected", !TestGrid.IsSelected(1));
				Assert("Row 2 is not Selected", !TestGrid.IsSelected(2));
				Assert("Row 3 is not Selected", !TestGrid.IsSelected(3));
				Assert("Row 4 is not Selected", !TestGrid.IsSelected(4));

				KeySender.PostKeyDown(TestGrid, TestGrid.Handle, Keys.Control | Keys.A);
				Application.DoEvents();

				Assert("Row 0 is Selected", TestGrid.IsSelected(0));
				Assert("Row 1 is Selected", TestGrid.IsSelected(1));
				Assert("Row 2 is Selected", TestGrid.IsSelected(2));
				Assert("Row 3 is Selected", TestGrid.IsSelected(3));
				Assert("Row 4 is not Selected", !TestGrid.IsSelected(4));

				TestGrid.ResetSelection();

				KeySender.PostKeyDown(TestGrid, TestGrid.Handle, Keys.Control | Keys.Shift | Keys.A);
				Application.DoEvents();

				Assert("Row 0 is Selected", TestGrid.IsSelected(0));
				Assert("Row 1 is Selected", TestGrid.IsSelected(1));
				Assert("Row 2 is Selected", TestGrid.IsSelected(2));
				Assert("Row 3 is Selected", TestGrid.IsSelected(3));
				Assert("Row 4 is not Selected", !TestGrid.IsSelected(4));
			}
		}

		public void TestAllowMultiSelectTrue()
		{
			using (var testForm = new ZForm())
			{
				InitialiseTest(testForm, true);
				KeySender.PostKeyDown(TestGrid, TestGrid.Handle, Keys.Shift | Keys.Down);
				Application.DoEvents();

				Assert("Row 0 is Selected", TestGrid.IsSelected(0));
				Assert("Row 1 is Selected", TestGrid.IsSelected(1));
				Assert("Row 2 is not Selected", !TestGrid.IsSelected(2));
				Assert("Row 3 is not Selected", !TestGrid.IsSelected(3));
				Assert("Row 4 is not Selected", !TestGrid.IsSelected(4));

				KeySender.PostKeyDown(TestGrid, TestGrid.Handle, Keys.Control | Keys.Shift | Keys.Down);
				Application.DoEvents();

				Assert("Row 0 is not Selected", !TestGrid.IsSelected(0));
				Assert("Row 1 is Selected", TestGrid.IsSelected(1));
				Assert("Row 2 is Selected", TestGrid.IsSelected(2));
				Assert("Row 3 is Selected", TestGrid.IsSelected(3));
				Assert("Row 4 is not Selected", !TestGrid.IsSelected(4));

				KeySender.PostKeyDown(TestGrid, TestGrid.Handle, Keys.Shift | Keys.Up);
				Application.DoEvents();

				Assert("Row 0 is not Selected", !TestGrid.IsSelected(0));
				Assert("Row 1 is Selected", TestGrid.IsSelected(1));
				Assert("Row 2 is Selected", TestGrid.IsSelected(2));
				Assert("Row 3 is not Selected", !TestGrid.IsSelected(3));
				Assert("Row 4 is not Selected", !TestGrid.IsSelected(4));

				KeySender.PostKeyDown(TestGrid, TestGrid.Handle, Keys.Control | Keys.Shift | Keys.Up);
				Application.DoEvents();

				Assert("Row 0 is Selected", TestGrid.IsSelected(0));
				Assert("Row 1 is Selected", TestGrid.IsSelected(1));
				Assert("Row 2 is Selected", TestGrid.IsSelected(2));
				Assert("Row 3 is not Selected", !TestGrid.IsSelected(3));
				Assert("Row 4 is not Selected", !TestGrid.IsSelected(4));
			}
		}

		void AssertSelected(ZGrid testGrid, int rowIndex)
		{
			Assert("Row " + rowIndex + " should be selected", testGrid.IsSelected(rowIndex));
		}

		void InitialiseTest(ZForm testForm, bool allowMultiSelect)
		{
			var data = new DataSet(); // for testing only
			var table = data.Tables.Add("Table");
			table.Columns.Add("Column1");
			table.Columns.Add("Column2");

			TestGrid.Dispose();
			TestGrid = new TestZGrid();
			testForm.Controls.Add(TestGrid);

			TestGrid.Dock = DockStyle.Fill;
			TestGrid.Columns.Add(new ZTextBoxColumnStyleInfo("Column1", 80));
			TestGrid.Columns.Add(new ZTextBoxColumnStyleInfo("Column2", 80));

			TestGrid.SetDataBinding(data, "Table");
			testForm.Show();

			TestGrid.ListManager.AddNew();
			TestGrid.ListManager.AddNew();
			TestGrid.ListManager.AddNew();

			TestGrid.Focus();
			TestGrid.CurrentCell = new DataGridCell(0, 0);
		}

		#endregion

		#region Columns with negative width

		public void TestThrowErrorReport_WhenColumnsWithNegativeWidth()
		{
			var data = new DataSet();
			var table1 = new DataTable("Table1");
			data.Tables.Add(table1);

			var testColumn1 = new DataColumn("TestColumn1");
			var testColumn2 = new DataColumn("TestColumn2");
			table1.Columns.Add(testColumn1);
			table1.Columns.Add(testColumn2);

			using (var testForm = new KForm())
			{
				var testGrid = new ZGrid();
				var column1Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
				var defaultWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				testGrid.Columns.AddTextColumn("TestColumn1", column1Width);
				testGrid.Columns.AddTextColumn("TestColumn2", -150);
				testForm.Controls.Add(testGrid);

				testGrid.SetDataBinding(data, "Table1");
				testGrid.RefreshTableStyles();

				AssertEquals("Column width should be 80.", column1Width, testGrid.Columns[0].ColumnStyle.Width);
				AssertEquals("Column width should be 100.", defaultWidth, testGrid.Columns[1].ColumnStyle.Width);
			}
		}

		#endregion

		#region Implementation

		TestZGrid TestGrid;

		protected override void SetUp()
		{
			base.SetUp();

			TestGrid = new TestZGrid();
			TestGrid.Columns.AddTextColumn("PK", 200, true, true);
			TestGrid.Columns.AddTextColumn("Name", 200, true, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestGrid.Dispose();
		}

		static void PostKeyToActiveControl(Control control, Keys key)
		{
			KeySender.PostKeyDown(MostActiveControl(control), key);
			Application.DoEvents();
		}

		static Control MostActiveControl(Control control)
		{
			return control is IContainerControl ? MostActiveControl(((IContainerControl)control).ActiveControl) : control;
		}

		#endregion
	}
}
