using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	class GridLayoutTest : TestCaseWithDummy
	{
		public void TestAddOperationalActionsIntoGridActionsMenu()
		{
			var entity = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			using (var form = new ZForm(entity))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Shipments";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo { ColumnName = "JS_HouseBill" };
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				grid.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				MenuAssertion.AssertHasMenu("Operational Actions", grid.ContextMenu);
			}
		}

		public void TestRefreshLayoutWhenColumnLayoutContextChanges()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var serialiser = new DataGridLayoutDataSetSerialiser();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var layoutForAAA = Factory.New<StmData>();
				layoutForAAA.SD_Name = "DataGridLayout|" + form.TabGrid.GridId + "|" + "AAA";
				layoutForAAA.SD_Owner = EnvProxy.Instance.CurrentUser.PK;

				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					layoutForAAA.SD_BinaryValue = layoutStream.ToArray();
				}

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				var layoutForBBB = Factory.New<StmData>();
				layoutForBBB.SD_Name = "DataGridLayout|" + form.TabGrid.GridId + "|" + "BBB";
				layoutForBBB.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					layoutForBBB.SD_BinaryValue = layoutStream.ToArray();
				}

				Factory.Save();

				form.TabGrid.ColumnLayoutContext = "AAA";
				AssertEquals("Both columns should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Both columns should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.ColumnLayoutContext = "BBB";
				AssertEquals("Both columns should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Both columns should be visible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestSaveUserLayoutSettingsWhenLayoutDeletedDoesntThrowException()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var preConfiguredLayout = Factory.New<StmModuleFilter>();
				preConfiguredLayout.S9_SaveColumnLayout = true;

				form.TabGrid.CurrentColumnLayout = preConfiguredLayout;
				Factory.Save();

				form.TabGrid.CurrentColumnLayout.Delete();
				Factory.Save();

				AssertNoExceptionThrown(() => form.TabGrid.SaveUserLayoutSettings());
			}
		}

		public void TestUpdateGridLayoutManageImage()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "Z0_Description"
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				grid.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();
				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				AssertEquals("ActiveImage is not drawn to start with", false, grid.isActiveGridLayoutIconDrawn);

				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseHover", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { EventArgs.Empty });
				Application.DoEvents();
				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				AssertEquals("ActiveImage is drawn as it is hovered over", true, grid.isActiveGridLayoutIconDrawn);

				//outside the rectangle
				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X + ZGrid.TopLeftCornerGridLayoutRectangle.Width + 1, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseHover", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { EventArgs.Empty });
				Application.DoEvents();
				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				AssertEquals("ActiveImage is not drawn as it is not hovered over", false, grid.isActiveGridLayoutIconDrawn);

				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseHover", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { EventArgs.Empty });
				Application.DoEvents();
				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				AssertEquals("ActiveImage is drawn as it is hovered over", true, grid.isActiveGridLayoutIconDrawn);

				typeof(Control).InvokeMember("OnMouseLeave", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { EventArgs.Empty });
				//outside the grid
				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y - 50);
				grid.Focus();//forces Paint
				Application.DoEvents();
				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				AssertEquals("ActiveImage is not drawn as it is left", false, grid.isActiveGridLayoutIconDrawn);
			}
		}

		public void TestCustomiseColumnsFormPoppedUpWhenRectangleHit()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "Z0_Description"
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				grid.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();

				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y, 0) });
				Application.DoEvents();
				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));

				AssertEquals("ActiveImage is drawn as it is hovered over", true, grid.isActiveGridLayoutIconDrawn);
				AssertEquals("CustomiseColumns form is shown", typeof(ZGridCustomise), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		public void TestCustomiseColumnsFormWhenGridBindingFailed()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo()
				{
					ColumnName = "Z0_Description",
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				grid.DataSource = null; // if there was an exception during binding

				grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();

				ZFormModaliser.LastFormShownForTest = null;
				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y, 0) });
				Application.DoEvents();
				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				AssertNull("CustomiseColumns form was not shown", ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestCustomiseColumnsFormWhenGridDataSourceIsNull()
		{
			using (var form = new ZForm())
			using (var grid = new TestGrid())
			{
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Test" });
				grid.DataSource = null;
				form.Controls.Add(grid);

				grid.CustomiseColumns();
				AssertEquals("error message show", "Grid is not bound to data.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCustomiseColumnsForm_LastFocusedColumnShouldBeHidden()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_NVarChar = nameof(FieldType.Text);
			var columnStyleInfo = new ZMultiControlColumnStyleInfo();
			columnStyleInfo.ColumnName = "__CONTRACTNUMBER__prop__ZString";
			columnStyleInfo.Caption = "ContractNumber";
			((IOverridablePropertyDescriptor)columnStyleInfo).PropertyDescriptor = new WorkflowCustomPropertyDescriptorForTesting("__CONTRACTNUMBER__prop__ZString", typeof(ZString), true);

			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnStyleInfo);
				testForm.Grid.SetDataBinding(Dummy, "Collection");

				var propertyCollection = new CustomPropertyCollectionImpl(propertyName => "A", null);
				propertyCollection.Add(typeof(ZString), "__CONTRACTNUMBER__prop__ZString", DynamicMetaData.ListDataSource(new[] { "A", "B" }));
				var customBusinessObject = new CustomBusinessObject(Factory, null, propertyCollection);
				dummyChild.RegisterEditableChildObject(customBusinessObject);

				testForm.Show();
				testForm.Grid.Focus();
				testForm.Grid.CurrentCell = new DataGridCell(0, 2);

				AssertNotEquals(0, testForm.Grid.LastFocusedColumn.EditControl.Width);

				testForm.Grid.OnPopup_CallForTesting();
				var menu = testForm.Grid.ContextMenu.MenuItems.FindByText("Customize Columns");

				AssertNotNull(menu);

				menu.PerformClick();

				AssertEquals(0, testForm.Grid.LastFocusedColumn.EditControl.Width);
			}
		}

#if !WINZOR

		Point AddMargin(Point point)
		{
			return new Point(point.X + 2, point.Y - 2);
		}

		public void TestDragDropColumns()
		{
			using (var testForm = new ZForm())
			using (var grid = new ZDisplayGrid())
			using (var module = new DummyFilterGridModule())
			{
				grid.Size = new Size(200, 100);

				grid.SetParentFilterGridModule(module);

				testForm.Controls.Add(grid);
				var codeColumnInfo = new ZTextBoxColumnStyleInfo(AutoDummyBizo.Schema.Z0_Code, 10);
				grid.ColumnStyles.Add(codeColumnInfo);

				var descColumnInfo = new ZTextBoxColumnStyleInfo(AutoDummyBizo.Schema.Z0_Description, 50);
				grid.ColumnStyles.Add(descColumnInfo);

				var dateColumnInfo = new ZTextBoxColumnStyleInfo(AutoDummyBizo.Schema.Z0_Date, 20);
				grid.ColumnStyles.Add(dateColumnInfo);

				var decimalColumnInfo = new ZTextBoxColumnStyleInfo(AutoDummyBizo.Schema.Z0_Decimal, 15);
				grid.ColumnStyles.Add(decimalColumnInfo);

				var numberColumnInfo = new ZTextBoxColumnStyleInfo(AutoDummyBizo.Schema.Z0_Number, 10);
				grid.ColumnStyles.Add(numberColumnInfo);

				grid.SetDataBinding(Dummy, "Collection");

				testForm.Show();
				AssertNotNull("PreCondition: Bound correctly.", grid.ListManager);

				var gridLayoutManageable = new ZGridLayoutModification(grid, grid.Columns, Factory, null);

				grid.CurrentColumnLayout = new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "BothVisible", false, false, SaveColumnLayout.Ignore);

				var expectedOrder = new[] { "Code", "Description", "Date", "Decimal", "Number" };
				var actualOrder = grid.Columns.Select(c => c.ToString());
				AssertArrayEqualsByElements(expectedOrder, actualOrder.ToArray());

				var pos1 = AddMargin(grid.GetCellBounds(0, 2).Location);
				var pos2 = AddMargin(grid.GetCellBounds(0, 0).Location);

				grid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos1.X, pos1.Y, 0));
				grid.PerformMouseUpForTest(new MouseEventArgs(MouseButtons.Left, 1, pos2.X, pos2.Y, 0));

				pos1 = AddMargin(grid.GetCellBounds(0, 4).Location);
				pos2 = AddMargin(grid.GetCellBounds(0, 1).Location);

				grid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos1.X, pos1.Y, 0));
				grid.PerformMouseUpForTest(new MouseEventArgs(MouseButtons.Left, 1, pos2.X, pos2.Y, 0));

				expectedOrder = new[] { "Date", "Number", "Code", "Description", "Decimal" };
				AssertArrayEqualsByElements(expectedOrder, actualOrder.ToArray());

				pos1 = AddMargin(grid.GetCellBounds(0, 0).Location);
				pos2 = AddMargin(grid.GetCellBounds(0, 3).Location);

				grid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos1.X, pos1.Y, 0));
				grid.PerformMouseUpForTest(new MouseEventArgs(MouseButtons.Left, 1, pos2.X, pos2.Y, 0));

				pos1 = AddMargin(grid.GetCellBounds(0, 0).Location);
				pos2 = AddMargin(grid.GetCellBounds(0, 3).Location);

				grid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, pos1.X, pos1.Y, 0));
				grid.PerformMouseUpForTest(new MouseEventArgs(MouseButtons.Left, 1, pos2.X, pos2.Y, 0));

				expectedOrder = new[] { "Code", "Description", "Date", "Number", "Decimal" };
				AssertArrayEqualsByElements(expectedOrder, actualOrder.ToArray());
			}
		}

#endif

		public void TestColumnStyleIsSetBeforeBinding()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();

				zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
				zTextBoxColumnStyleInfo2.ColumnName = "Z0_Code";
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

				grid.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				var listControls = UIResources.Instance.GetAllControls(grid).ToList();
				AssertEquals("Grid should contain one textbox per column style", 2, listControls.Count(c => c == typeof(DataGridTextBox).ToString()));
			}
		}

		public void TestColumnStyleIsSetBeforeBindingReportError()
		{
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			using (grid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();

				zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
				zTextBoxColumnStyleInfo2.ColumnName = "Z0_Code";
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

				form.Controls.Add(grid);
				form.Show();

				Assert(ErrorReporter.LastMessageReported.Contains("Please set the TableStyle before binding."));
				ErrorReporter.Instance.Clear();
			}
		}

		public void TestSelectionResetWhenClickingOnRowHeader()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "Z0_Description"
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				grid.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();

				grid.SetSelectedRows(new int[] { 0, 1 });
				AssertEquals("First two rows are selected.", 2, grid.SelectedRowCount);
				var row1Rectangle = grid.GetRowNotificationRectangle(1);
				grid.MousePosition = new Point(row1Rectangle.X, row1Rectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
				Application.DoEvents();
				AssertEquals("Selection is not reset. First two rows are still selected when left button is down on row header.", 2, grid.SelectedRowCount);
				typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X + 100, row1Rectangle.Y + 100, 0) });
				Application.DoEvents();
				AssertEquals("Selection is not reset. First two rows are still selected when left button is up on somewhere outside the grid.", 2, grid.SelectedRowCount);

				grid.SetSelectedRows(new int[] { 0, 1 });
				AssertEquals("First two rows are selected.", 2, grid.SelectedRowCount);
				grid.MousePosition = new Point(row1Rectangle.X, row1Rectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
				typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
				Application.DoEvents();
				AssertEquals("Selection is reset when left button clicking on the row header.", 1, grid.SelectedRowCount);
				AssertEquals("Second row is selected.", 1, grid.GetSelectedRowIndexes()[0]);

				grid.SetSelectedRows(new int[] { 0, 1 });
				AssertEquals("First two rows are selected.", 2, grid.SelectedRowCount);
				var row2Rectangle = grid.GetRowNotificationRectangle(2);
				grid.MousePosition = new Point(row2Rectangle.X, row2Rectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row2Rectangle.X, row2Rectangle.Y, 0) });
				typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, row2Rectangle.X, row2Rectangle.Y, 0) });
				Application.DoEvents();
				AssertEquals("Selection is reset", 1, grid.SelectedRowCount);
				AssertEquals("Third row is selected.", 2, grid.GetSelectedRowIndexes()[0]);
			}

			Dummy.Collection.RemoveAll();
		}

		public void TestExceptionOnGridBinding()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridForExceptionTest())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "Z0_Description"
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				form.Controls.Add(grid);
				form.Show();
				grid.ShouldThrow = true;
				AssertNoExceptionThrown(() => form.SetDataBinding(Dummy, ""));
			}
		}

		class ZGridForExceptionTest : ZGridNotificationsTestCase.DummyZGrid
		{
			public bool ShouldThrow { get; set; }
			int exceptions;
			public override BindingContext BindingContext
			{
				get
				{
					if (ShouldThrow && exceptions == 0)
					{
						exceptions++;
						throw new IndexOutOfRangeException("Index -1 does not have a value.");
					}
					return base.BindingContext;
				}
				set => base.BindingContext = value;
			}
		}

		public void TestSelectionNotResetWhenRightClickingOnRowHeader()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "Z0_Description"
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				grid.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();

				grid.SetSelectedRows(new int[] { 0, 1 });
				AssertEquals("First two rows are selected.", 2, grid.SelectedRowCount);
				var row1Rectangle = grid.GetRowNotificationRectangle(1);
				grid.MousePosition = new Point(row1Rectangle.X, row1Rectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Right, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
				typeof(Control).InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Right, 1, row1Rectangle.X, row1Rectangle.Y, 0) });
				Application.DoEvents();
				AssertEquals("Selection is not reset. First two rows are still selected when right button is clicked on row header.", 2, grid.SelectedRowCount);
			}

			Dummy.Collection.RemoveAll();
		}

		public void TestGridLayoutEndToEndTestWhenThereIsAGridControlID()
		{
			var newWidth = 0;

			using (var form = new ZTestGridForm(Dummy))
			{
				AssertEquals("PreCondition:Id exists", false, string.IsNullOrEmpty(form.TabGrid.GridId));
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width += 10;
				newWidth = form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width;
				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				AssertEquals("Number field should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Description field should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertEquals("Description field width", newWidth, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width);
			}
		}

		public void TestElementTypeFromCollection()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();

			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				grid.Columns.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_VarCharMax.Name, 10));
				form.Controls.Add(grid);

				grid.BindTo = "Dependents";
				grid.SetDataBinding(new NonPersistentWithDummy(Factory), "Dummies.Dependents");

				form.Show();
				Application.DoEvents();
				AssertEquals(typeof(DummyDependantBusinessObject), grid.ElementTypeFromCollection);
			}
		}

		public void TestIsMouseOnAValidRow()
		{
			var parentBizo = Factory.New<DummyBusinessObject>();
			using (var grid = new ZGrid())
			using (var form = new ZTestForm(parentBizo))
			{
				form.Controls.Add(grid);
				grid.BindTo = "Collection";
				grid.Columns.AddTextColumn("Z0_Code", 30);
				AssertEquals("Precondition", 0, parentBizo.Collection.Count);

				form.Show();
				grid.SetCurrentHitTestForTest(0, 0);

				Assert("Mouse is not on a valid row", !grid.IsMouseOnAValidRow);

				parentBizo.Collection.AddNew();
				Assert("Mouse is on a valid row now", grid.IsMouseOnAValidRow);
			}
		}

		class NonPersistentWithDummy : NonPersistentBusinessObject
		{
			public NonPersistentWithDummy(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject> Dummies
			{
				get { return dummies ?? (dummies = new ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject>(Factory, ZQuery.NoResultQuery)); }
			}
			ActiveBusinessObjectCollection<DummyWithDependentsBusinessObject> dummies;
		}

		public void TestGridLayoutEndToEndTestWhenThereIsNotAGridControlID()
		{
			var newWidth = 0;

			using (var form = new ZTestGridFormWithoutGridID(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width += 10;
				newWidth = form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width;
				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

			using (var form = new ZTestGridFormWithoutGridID(Dummy))
			{
				form.Show();

				AssertEquals("Number field should be invisible", false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals("Description field should be visible", true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
				AssertEquals("Description field width", newWidth, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width);
			}
		}

		public void TestRunAfterBind()
		{
			using (var form = new ZForm())
			using (var grid = new ZGrid())
			{
				var column = new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_Code.Name, 10);
				grid.ColumnStyles.Add(column);

				form.Controls.Add(grid);
				var runAfterBind = false;
				grid.RunAfterBind((s, e) => runAfterBind = true);

				AssertEquals("delegate in RunAfterBind() not called before binding", false, runAfterBind);
				grid.SetDataBinding(Dummy, "");
				AssertEquals("delegate in RunAfterBind() called after binding", true, runAfterBind);

				var runWhenAlreadyBound = false;
				grid.RunAfterBind((s, e) => runWhenAlreadyBound = true);
				AssertEquals("delegate in RunAfterBind() called when already bound", true, runWhenAlreadyBound);
			}
		}

		public void TestReOrderingDoesNotCausesMemoryLeak()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				var columns = form.TabGrid.Columns;
				AssertEquals(2, columns.Count);
				AssertEquals(DummyBizoSchema.Constants.Z0_Description, columns[0].ColumnName);
				AssertEquals(DummyBizoSchema.Constants.Z0_Number, columns[1].ColumnName);
				form.TabGrid.ReOrderColumns([DummyBizoSchema.Constants.Z0_Number, DummyBizoSchema.Constants.Z0_Description]);
				columns = form.TabGrid.Columns;
				AssertEquals(2, columns.Count);
				AssertEquals(DummyBizoSchema.Constants.Z0_Number, columns[0].ColumnName);
				AssertEquals(DummyBizoSchema.Constants.Z0_Description, columns[1].ColumnName);
			}
		}

		public void TestRefreshLayoutWhenLayoutContextIsChanged()
		{
			var layoutContext = "IMP";
			string gridID;

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				form.TabGrid.ColumnLayoutContext = layoutContext;
				form.TabGrid.Columns.HasLayoutChanged = true;
			}

			AssertNotNull("PreCondition:StmData is saved with LayoutCategoryPK", new StmData.Loader(Factory).LoadTop1(gridID, EnvProxy.Instance.CurrentUser.PK, ZGuid.Empty));

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				form.TabGrid.ColumnLayoutContext = layoutContext;
				//When LayoutCategoryPK is set, then it should save the previous layout and load to the current layout
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.ColumnLayoutContext = "";
				//When LayoutCategoryPK is set, then it should save the previous layout and load to the current layout
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		public void TestRefreshCurrentLayoutNameWhenLayoutContextPKisChanged()
		{
			var layoutCategoryPK = Guid.NewGuid();

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();
				form.TabGrid.LayoutCategoryPK = layoutCategoryPK;

				var serialiser = new DataGridLayoutDataSetSerialiser();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				//saved with a key with LayoutCategoryPK
				var descriptionVisible = Factory.New<StmModuleFilter>();
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
				AssertNull("CurrentColumnLayout", form.TabGrid.CurrentColumnLayout);

				form.TabGrid.LayoutCategoryPK = layoutCategoryPK;
				AssertEquals("CurrentColumnLayout", "BothVisible", form.TabGrid.CurrentColumnLayout.ColumnLayoutName);
			}
		}

		[GuiTest]
		public void TestLoadLayoutForModule()
		{
			using (var form = new ZTestForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				form.Controls.Add(module.EmbeddedControl);

				form.Show();

				var filterControl = (DummyFilterControl)module.EmbeddedControl;

				var moduleFilters = filterControl.FilterBusinessObject.ModuleFilters;
				moduleFilters.AddTextFilter("text filter", DummyBizoSchema.Z0_Description);

				// simluate a user creating and populating a filter strip
				var strips = filterControl.FilterBusinessObject.FilterStrips;
				var textStrip = filterControl.FilterBusinessObject.FilterStrips[0];
				textStrip.FilterDescription = DummyBizoSchema.Z0_Description.Name;

				filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;
				filterControl.FilteredGrid.RefreshTableStyles();

				var numberOnlyLayout = CreateLayout(Factory, filterControl.FilteredGrid, "Number Only", true);
				numberOnlyLayout.S9_FilterData = filterControl.DummyFilterBizObj.FilterStrips.GetLayoutAsXml();

				var descriptionOnlyLayout = CreateLayout(Factory, filterControl.FilteredGrid, "Description Only", false);
				descriptionOnlyLayout.S9_FilterData = filterControl.DummyFilterBizObj.FilterStrips.GetLayoutAsXml();

				Factory.Save();

				filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				filterControl.FilteredGrid.Columns.HasLayoutChanged = true;//last saved grid layout will be 'number: Invisible, Description:Visible's
			}

			using (var form = new ZTestForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				form.Controls.Add(module.EmbeddedControl);

				form.Show();

				var filterControl = (DummyFilterControl)module.EmbeddedControl;
				new DataGridLayoutDataAccessor().ClearGridColumnSettingsCacheForTesting();

				AssertEquals("Number Only", filterControl.FindButton.DropDownItems[2].Text.Trim());
				filterControl.FindButton.ShowDropDown(); // so that it doesn't click on the Favorite image
				filterControl.FindButton.DropDownItems[2].PerformClick();
				AssertEquals("Number Only", filterControl.FilteredGrid.CurrentColumnLayout.ColumnLayoutName);
				AssertEquals(true, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				//Save Column Layout = false >> therefore default layout is to be loaded
				AssertEquals("Description Only", filterControl.FindButton.DropDownItems[1].Text.Trim());
				filterControl.FindButton.DropDownItems[1].PerformClick();
				AssertEquals("Grid Layout name should be default", true, StmDataGridLayoutStorage.IsDefaultLayout(filterControl.FilteredGrid.CurrentColumnLayout.ColumnLayoutName));
				AssertEquals(false, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				filterControl.FindButton.DropDownItems[2].PerformClick();
				AssertEquals(true, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				filterControl.FindButton.DropDownItems[1].PerformClick();
				AssertEquals(false, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, filterControl.FilteredGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		StmModuleFilter CreateLayout(BusinessObjectFactory factory, ZGrid grid, string filterName, bool saveColumnLayout)
		{
			var serialiser = new DataGridLayoutDataSetSerialiser();

			var layout = Factory.New<StmModuleFilter>();
			layout.S9_ModuleID = DummyModuleIDs.Dummy.Name;
			layout.S9_FilterName = filterName;

			layout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			layout.S9_SaveColumnLayout = saveColumnLayout;

			if (saveColumnLayout)
			{
				layout.S9_ColumnLayoutData = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(grid)).ToArray();
			}
			return layout;
		}

		public void TestColorContextKey()
		{
			using (var grid = new ZGrid())
			{
				Assert("ColorContextKey should be empty", string.IsNullOrEmpty(grid.ColorContextKey));

				grid.ColorContextKey = "abcde";
				AssertEquals("abcde", grid.ColorContextKey);

				grid.ColorContextKey = "";
				Assert("ColorContextKey should be empty", string.IsNullOrEmpty(grid.ColorContextKey));
			}
		}

		public void TestShareActiveColorScheme()
		{
			using (var grid = new ZGrid())
			{
				AssertEquals("ShareActiveColorScheme should be false", false, grid.ShareActiveColorScheme);

				grid.ShareActiveColorScheme = true;
				AssertEquals("ShareActiveColorScheme should be true", true, grid.ShareActiveColorScheme);

				grid.ShareActiveColorScheme = false;
				AssertEquals("ShareActiveColorScheme should be false", false, grid.ShareActiveColorScheme);
			}
		}

		public void TestSuspendGridState()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "Z0_Description"
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				var originalRowIndex = 1;

				form.Controls.Add(grid);
				form.Show();

				grid.CurrentRowIndex = originalRowIndex;

				using (ZGrid.SuspendGridState(new ZGrid[] { grid }))
				{
					grid.CurrentRowIndex = 2;
					AssertEquals("Current-row-index must change accordingly", 2, grid.CurrentRowIndex);
				}

				AssertEquals("Current-row-index must not changed", originalRowIndex, grid.CurrentRowIndex);
			}

			Dummy.Collection.RemoveAll();
		}

		#region TestSetupAdditionalColumns

		public void TestSetupAdditionalColumns()
		{
			using (var form = new ZForm())
			using (var grid = new TestGrid())
			{
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Code1" });
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Code2" });
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Key" });
				grid.ColumnStyles.Add(new ZMultiControlColumnStyleInfo { ColumnName = "Some trash" });

				form.Controls.Add(grid);

				grid.SetDataBinding(new DummiesWithCodes(Factory), "");

				AssertEquals(6, grid.Columns.Count);
				AssertEquals(6, grid.DefaultColumnsExposed.Count);
				AssertEquals(typeof(ZDescriptionCodeFindBoxColumnStyle), grid.Columns[4].ColumnStyle.GetType());
				AssertEquals("Code1" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix, grid.Columns[4].ColumnStyle.MappingName);
				AssertEquals(false, grid.Columns[4].IsVisible);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(120), grid.Columns[4].Width);
				AssertEquals(typeof(ZDescriptionPkFindBoxColumnStyle), grid.Columns[5].ColumnStyle.GetType());
				AssertEquals("Key" + ZDescriptionPkFindBoxColumnStyle.ColumnNameSuffix, grid.Columns[5].ColumnStyle.MappingName);
				AssertEquals(false, grid.Columns[5].IsVisible);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(120), grid.Columns[5].Width);
			}
		}

		public void TestAdditionalColumnCaption()
		{
			using (var form = new ZForm())
			using (var grid = new TestGrid())
			{
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Code1" });
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Code2" });
				grid.ColumnStyles.Add(new ZCodeFindBoxColumnStyleInfo { ColumnName = "Key" });
				grid.ColumnStyles.Add(new ZMultiControlColumnStyleInfo { ColumnName = "Some trash" });

				form.Controls.Add(grid);

				grid.SetDataBinding(new DummiesWithCodes(Factory), "");

				AssertEquals(6, grid.Columns.Count);
				AssertEquals(6, grid.DefaultColumnsExposed.Count);
				AssertEquals(typeof(ZDescriptionCodeFindBoxColumnStyle), grid.Columns[4].ColumnStyle.GetType());
				AssertEquals("Code1" + ZDescriptionCodeFindBoxColumnStyle.ColumnNameSuffix, grid.Columns[4].ColumnStyle.MappingName);
				AssertEquals("Emu is da best", grid.Columns[4].ColumnStyle.HeaderText);
			}
		}

		class TestGrid : ZGrid
		{
			public ZGridColumns DefaultColumnsExposed
			{
				get { return DefaultColumns; }
			}
		}

		class DummyWithCodes : DummyBusinessObject
		{
			public DummyWithCodes(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[List("Codes1")]
			[ListColumnCaption(typeof(DummyWithCodes), nameof(FNC))]
			public ZString Code1 { get; set; }

			public static string FNC() => "Emu is da best";

			public ReferenceableCollection Codes1 { get { return new ReferenceableCollection(Factory); } }

			[List("Codes2")]
			public ZString Code2 { get; set; }

			public DummyBusinessObjectCollection Codes2 { get { return new DummyBusinessObjectCollection(Factory); } }

			[List("Keys")]
			public ZGuid Key { get; set; }

			public ReferenceableCollection Keys { get { return new ReferenceableCollection(Factory); } }
		}

		class DummiesWithCodes : BusinessObjectCollection<DummyWithCodes>
		{
			public DummiesWithCodes(BusinessObjectFactory factory) : base(factory) { }
		}

		[CodeProperty("Z0_Code"), DescriptionProperty("Z0_Description", CanBeReferencedBy = true)]
		class ReferenceableDummy : DummyBusinessObject
		{
			public ReferenceableDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class ReferenceableCollection : BusinessObjectCollection<ReferenceableDummy>
		{
			public ReferenceableCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		#endregion

		#region Column & Row Resizer

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "Testing")]
		public void TestColumnWidthResizer()
		{
			Dummy.Collection.AddNew();

			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid(true, new[] { "Z0_Description" }))
			{
				// anchor the grid, so it can auto-resize
				grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

				grid.Size = ControlDpiScalingHelper.NewScaledSize(600, 200, true);
				grid.BindTo = "Collection";

				foreach (var columName in new[] { "Z0_Date", "Z0_Description" })
				{
					var textBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo
					{
						ColumnName = columName
					};
					grid.ColumnStyles.Add(textBoxColumnStyleInfo);
				}

				form.Controls.Add(grid);
				form.MinimumSize = ControlDpiScalingHelper.NewScaledSize(600, 200, true);
				form.Show();

				var gridLeftEmptySpace = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
				var gridRightEmptySpace = ControlDpiScalingHelper.ScaleToCurrentDpiX(22);
				var firstRow = ControlDpiScalingHelper.ScaleToCurrentDpiY(5);
				var defaultDateColumnWidth = grid.Columns[0].Width;
				var defaultDescriptionColumnWidth = grid.Columns[1].Width;

				for (var formWidth = form.MinimumSize.Width;
					formWidth <= Screen.FromControl(form).WorkingArea.Width;
					formWidth += 5)
				{
					form.Width = formWidth;
					Assert("Grid should stretch with the form", form.Width - grid.Width < 50);

					var hitTest = grid.HitTest(grid.TableStyles[0].RowHeaderWidth + gridLeftEmptySpace, firstRow);
					AssertEquals("Z0_Date column header should anchor on the left edge of the grid", DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Z0_Date column header should anchor on the left edge of the grid", DummyBizoSchema.Z0_Date.Name, grid.Columns[hitTest.Column].ColumnName);
					AssertEquals("Z0_Date column header should not resize ", defaultDateColumnWidth, grid.Columns[hitTest.Column].ColumnStyle.Width);

					hitTest = grid.HitTest(grid.Size.Width - gridRightEmptySpace, firstRow);
					AssertEquals("Z0_Description Column header should stretch to the right edge of the grid", DataGrid.HitTestType.ColumnHeader, hitTest.Type);
					AssertEquals("Z0_Description column header should stretch to the right edge of the grid", DummyBizoSchema.Z0_Description.Name, grid.Columns[hitTest.Column].ColumnName);
					Assert("Z0_Description column header should resize", grid.Columns[hitTest.Column].ColumnStyle.Width > defaultDescriptionColumnWidth);

					AssertEquals("grid.IsHorizontalScrollBarVisible", false, grid.IsHorizontalScrollBarVisible);
				}
			}

			Dummy.Collection.RemoveAll();
		}

		public void TestRowHeightResizer()
		{
			Dummy.Collection.RemoveAll();

			var dummy = Dummy.Collection.AddNew();
			dummy.Z0_Description = "Single line test";
			dummy = Dummy.Collection.AddNew();
			dummy.Z0_Description = "Test Line 1\r\nTest Line 2";

			var expectedResultForMultiLineColumn = new[] {
				new RowHeightResizerTestCase { Comment = "RowResizer on 'description' column with multi-lines: row 1 must return record-1: Single line test", ExpectedRowNumber = 0 },
				new RowHeightResizerTestCase { Comment = "RowResizer on 'description' column with multi-lines: row 2 must return record-2 line 1: Test Line 1", ExpectedRowNumber = 1 },
				new RowHeightResizerTestCase { Comment = "RowResizer on 'description' column with multi-lines: row 3 must return record-2 line 2: Test Line 2", ExpectedRowNumber = 1 },
				new RowHeightResizerTestCase { Comment = "RowResizer on 'description' column with multi-lines: row 4 must return record-2 line 3: Test Line 3", ExpectedRowNumber = 2 },
				new RowHeightResizerTestCase { Comment = "row 5 is out of bound", ExpectedRowNumber = -1 }
			};

			var rowHeightBuffer = ControlDpiScalingHelper.ScaleToCurrentDpiY(6);
			var textRowHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(13) + rowHeightBuffer;
			TestRowHeightResizerCore(columnToResize: "Z0_Description", rowHeight: textRowHeight, expectedResults: expectedResultForMultiLineColumn);

			var expectedResultsForSingleLineColumn = new[] {
				new RowHeightResizerTestCase { Comment = "RowResizer on 'date' column with 1 line: row 1 must return record-1: Single line test", ExpectedRowNumber = 0 },
				new RowHeightResizerTestCase { Comment = "RowResizer on 'date' column with 1 line: Test Line 1", ExpectedRowNumber = 1 },
				new RowHeightResizerTestCase { Comment = "RowResizer on 'date' column with 1 line: empty line", ExpectedRowNumber = 2 },
				new RowHeightResizerTestCase { Comment = "row 4 is out of bound", ExpectedRowNumber = -1 },
			};
			var dateRowHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(26); // M.K: Have not found yet why it is not scaled in this case, and why the main height == 0?
			TestRowHeightResizerCore(columnToResize: "Z0_Date", rowHeight: dateRowHeight, expectedResults: expectedResultsForSingleLineColumn);
		}

		public void TestRowHeightResizerShouldSetDefaultHeightWhenColumnValueIsEmpty()
		{
			Dummy.Collection.RemoveAll();

			var dummy = Dummy.Collection.AddNew();
			dummy.Z0_Description = "";

			var rowHeightBuffer = ControlDpiScalingHelper.ScaleToCurrentDpiY(6);
			var textRowHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(20) + rowHeightBuffer;

			var expectedResultForMultiLineColumn = new[] {
				new RowHeightResizerTestCase { Comment = "RowResizer on 'date' column with 1 line: row 1 must return record-1: Single line test", ExpectedRowNumber = 0 }
			};

			TestRowHeightResizerCore(columnToResize: "Z0_Description", rowHeight: textRowHeight, expectedResults: expectedResultForMultiLineColumn, true);
		}

		#endregion

		public void TestGetCurrentColumnStyle()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				var zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo()
				{
					ColumnName = "Z0_Description",
				};
				grid.Columns.Add(zTextBoxColumnStyleInfo);
				var dataSource = new List<object> { { new { Z0_Description = "test" } } };
				grid.DataSource = dataSource;
				form.Controls.Add(grid);
				grid.SetDataBinding(Dummy, "Collection");
				form.Show();
				grid.Focus();
				Application.DoEvents();

				var dataGridColumnStyle = grid.GetCurrentColumnStyle();
				AssertEquals("Z0_Description", dataGridColumnStyle.MappingName);
			}
		}

		public void TestGetCurrentColumnStyleWhenColumnVisibilityIsChanged()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				var info1 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Description };
				var info2 = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
				var info3 = new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax };
				grid.ColumnStyles.Add(info1);
				grid.ColumnStyles.Add(info2);
				grid.ColumnStyles.Add(info3);
				form.Controls.Add(grid);
				grid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);
				form.Show();
				grid.Focus();
				Application.DoEvents();
				grid.SetColumnVisible(false, info1.ColumnName);
				grid.SetColumnVisible(true, info2.ColumnName);
				grid.SetColumnVisible(false, info3.ColumnName);
				var dataGridColumnStyle = grid.GetCurrentColumnStyle();
				AssertEquals("Current ColumnStyle is info2", info2.ColumnName, dataGridColumnStyle.MappingName);
				grid.SetColumnVisible(true, info3.ColumnName);
				grid.SetColumnVisible(false, info2.ColumnName);
				dataGridColumnStyle = grid.GetCurrentColumnStyle();
				AssertEquals("Current ColumnStyle is info3", info3.ColumnName, dataGridColumnStyle.MappingName);
			}
		}

		public void TestSelectedLayoutIsSaved()
		{
			using (var form = new ZTestGridForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_Code.Name, 10));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_Description.Name, 50));
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();

				var newLayout = Factory.New<StmModuleFilter>();
				newLayout.S9_FilterName = "NewLayout";
				grid.NewColumnLayout = newLayout;

				AssertNull("Precondition - No layout selected", grid.CurrentColumnLayout);

				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y, 0) });
				Application.DoEvents();

				AssertEquals("ZGridCustomise form is shown", typeof(ZGridCustomise), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var gridCustomise = (ZGridCustomise)ZFormModaliser.LastFormShownForTest;
				gridCustomise.FireSaveLayoutButtonForTest();
				gridCustomise.DialogResult = DialogResult.OK;
				gridCustomise.Close();

				AssertEquals("SaveLastSelectedLayout should be true", true, grid.SaveLastSelectedLayout);
				AssertEquals("CurrentColumnLayout should have a new layout", "NewLayout", grid.CurrentColumnLayout.ColumnLayoutName);
			}
		}

		public void TestSelectedLayoutIsSaved_DeletedBO()
		{
			using (var form = new ZTestGridForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_Code.Name, 10));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_Description.Name, 50));
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();

				var newLayout = Factory.New<StmModuleFilter>();
				newLayout.S9_SaveColumnLayout = true;
				newLayout.S9_ModuleID = "ZGrid";
				newLayout.S9_FilterName = "NewLayout";
				Factory.Save();
				grid.NewColumnLayout = newLayout;

				grid.OnPaintExposed(new PaintEventArgs(grid.CreateGraphics(), ZGrid.TopLeftCornerGridLayoutRectangle));
				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y, 0) });
				Application.DoEvents();

				AssertEquals("", ErrorReporter.LastMessageReported);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var gridCustomise = (ZGridCustomise)ZFormModaliser.LastFormShownForTest;
				gridCustomise.FireSaveLayoutButtonForTest();
				newLayout.Delete();
				Factory.Save();
				gridCustomise.DialogResult = DialogResult.OK;
				AssertNoExceptionThrown(() => gridCustomise.Close());

				AssertEquals(false, ErrorReporter.LastMessageReported.Contains("Should not be accessing a property on a deleted business object", StringComparison.InvariantCultureIgnoreCase));
				AssertEquals("", ErrorReporter.LastMessageReported);
			}
		}

#if !WINZOR

		public void TestMinimumDistance()
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				grid.BindTo = "Collection";
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "Z0_Description"
				};
				grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				grid.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
				form.Controls.Add(grid);
				form.Show();

				grid.Focus();
				Application.DoEvents();

				grid.MousePosition = new Point(ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y);
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { new MouseEventArgs(MouseButtons.Left, 1, ZGrid.TopLeftCornerGridLayoutRectangle.X, ZGrid.TopLeftCornerGridLayoutRectangle.Y, 0) });
				Application.DoEvents();

				var movedFrom = grid.GetMovedFromForTest();

				Assert("Not moved far enough", !grid.MouseMovedFarEnoughForTest(new Point((int)(movedFrom.X + (System.Windows.SystemParameters.MinimumHorizontalDragDistance / 2)), (int)(movedFrom.Y + (System.Windows.SystemParameters.MinimumVerticalDragDistance / 2)))));
				Assert("Moved far enough", grid.MouseMovedFarEnoughForTest(new Point((int)(movedFrom.X + System.Windows.SystemParameters.MinimumHorizontalDragDistance + 1), (int)(movedFrom.Y + System.Windows.SystemParameters.MinimumVerticalDragDistance + 1))));
			}
		}

#endif

		public void TestCachedBindingContextCleared()
		{
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				var bindingContext1 = new BindingContext();
				var bindingContext2 = new BindingContext();

				grid.BindingContext = bindingContext1;
				_ = grid.BindingContext;
				AssertEquals("BindingContext should be bindingContext1", bindingContext1, grid.BindingContext_Exposed);

				grid.BindingContext = bindingContext2;
				AssertEquals("BindingContext should be null", null, grid.BindingContext_Exposed);

				_ = grid.BindingContext;
				AssertEquals("BindingContext should be bindingContext2", bindingContext2, grid.BindingContext_Exposed);
			}
		}
		public void TestResetCachedBindingContext()
		{
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				var bindingContext1 = new BindingContext();
				var bindingContext2 = new BindingContext();

				grid.BindingContext = bindingContext1;
				_ = grid.BindingContext;
				AssertEquals("BindingContext should be bindingContext1", bindingContext1, grid.BindingContext_Exposed);

				grid.BindingContext = bindingContext2;
				AssertEquals("BindingContext should be null", null, grid.BindingContext_Exposed);
				grid.ResetCachedBindingContext();
				AssertEquals("BindingContext should be bindingContext2", bindingContext2, grid.BindingContext_Exposed);
			}
		}

		public void TestResetCachedBindingContextInSetDataBindingInternal()
		{
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid())
			{
				var bindingContext1 = new BindingContext();

				grid.BindingContext = bindingContext1;
				_ = grid.BindingContext;
				AssertEquals("BindingContext should be bindingContext1", bindingContext1, grid.BindingContext_Exposed);

				grid.isResetCachedBindingContextCalled = false;
				var zTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo()
				{
					ColumnName = "Z0_Description",
				};
				grid.Columns.Add(zTextBoxColumnStyleInfo);
				var dataSource = new List<object> { { new { Z0_Description = "test" } } };
				grid.SetDataBindingInternalExposed(dataSource, "", "");
				AssertEquals("ResetCachedBindingContext should be called", true, grid.isResetCachedBindingContextCalled);
			}
		}

		public void TestViewGridNotificationsMenuItem()
		{
			var dummyCollection = Factory.NewWithValidTestData<DummyWithActiveCollection>();
			var dummyBizObj1 = dummyCollection.AddChildDummy();
			dummyBizObj1.Z0_Code = "Code1";
			var dummyBizObj2 = dummyCollection.AddChildDummy();
			dummyBizObj2.Z0_Code = "Code2";
			var dummyBizObj3 = dummyCollection.AddChildDummy();
			dummyBizObj3.Z0_Code = "Code3";

			using (dummyBizObj1.SuspendValidationTesting())
			using (dummyBizObj2.SuspendValidationTesting())
			using (dummyBizObj3.SuspendValidationTesting())
			{
				dummyBizObj1.Z0_CodeInfo.AddWarning("Test Warning");
				dummyBizObj2.Z0_CodeInfo.AddMessageError("Test Message Error");
			}

			using (var form = new TestForm(dummyCollection))
			{
				form.Show();

				var grid = form.grid1;
				grid.ContextMenu.DoPopup();
				var viewNotificationsMenu = grid.ContextMenu.MenuItems.FindByText("View Grid Notifications");
				AssertEquals(true, viewNotificationsMenu.Visible);
				AssertEquals(true, viewNotificationsMenu.Enabled);

				viewNotificationsMenu.PerformClick();
				var openedForms = ZApplication.GetOpenForms();
				var notificationsViewerForm = openedForms.FirstOrDefault(x => x is BusinessObjectNotificationsViewerForm);
				AssertNotNull(notificationsViewerForm);

				var notificationsGrid = (ZGrid)notificationsViewerForm.Controls.Find("NotificationsGrid", true)[0];
				notificationsGrid.Focus();
				Application.DoEvents();
				AssertEquals("No dummy row is selected", 0, grid.GetSelectedRows().Length);

				var eventArgs = new MouseEventArgs(MouseButtons.Left, 1, 20, 40, 0);
				typeof(DataGrid).InvokeMember("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, grid, new object[] { eventArgs });
				typeof(DataGrid).InvokeMember("OnMouseUp", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, grid, new object[] { eventArgs });
				Application.DoEvents();
				AssertEquals("One dummy row is selected", 1, grid.GetSelectedRows().Length);
			}
		}

		#region Implementation

		void TestRowHeightResizerCore(string columnToResize, int rowHeight, RowHeightResizerTestCase[] expectedResults, bool needTestHeight = false)
		{
			using (var form = new ZForm(Dummy))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid(true, null, new[] { columnToResize }))
			{
				grid.Size = ControlDpiScalingHelper.NewScaledSize(600, 600, true);
				grid.BindTo = "Collection";
				grid.ColumnHeadersVisible = false;

				foreach (var columName in new[] { "Z0_Date", "Z0_Description" })
				{
					var textBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo
					{
						ColumnName = columName
					};
					grid.ColumnStyles.Add(textBoxColumnStyleInfo);
				}

				form.Controls.Add(grid);
				form.AutoScaleDimensions = new SizeF(6F, 13F);
				form.AutoScaleMode = AutoScaleMode.None;
				form.CaptionRenderingEnabled = true;
				form.MinimumSize = ControlDpiScalingHelper.NewScaledSize(600, 600, true);

				form.Show();

				var positionX = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				var positionY = ControlDpiScalingHelper.OnePixel * 5;

				foreach (var expectedResult in expectedResults)
				{
					var hitTest = grid.HitTest(positionX, positionY);
					AssertEquals(expectedResult.Comment, expectedResult.ExpectedRowNumber, hitTest.Row);
					positionY += rowHeight;
				}

				if (needTestHeight)
				{
					var dataGridRowsProperty = typeof(DataGrid).GetProperty("DataGridRows", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic);
					var dataGridRows = (Array)dataGridRowsProperty.GetValue(grid, null);

					foreach (var dataGridRow in dataGridRows)
					{
						var property = dataGridRow.GetType().GetProperty("Height");
						var height = (int)property.GetValue(dataGridRow);
						AssertEquals(height, rowHeight);
					}
				}
			}
		}

		struct RowHeightResizerTestCase
		{
			public string Comment;
			public int ExpectedRowNumber;
		}

		#endregion
	}
}
