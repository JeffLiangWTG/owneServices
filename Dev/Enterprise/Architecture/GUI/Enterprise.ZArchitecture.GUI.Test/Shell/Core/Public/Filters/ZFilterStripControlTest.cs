using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class ZFilterStripControlTest : TestCaseWithFactory
	{
		public static void AssertColumn(ZDisplayGrid grid, string columnName, ResourceStringData groupName, string caption, bool visible, string value)
		{
			var column = grid.Columns[columnName];
			AssertNotNull(columnName + " column should exist", column);

			var columnStyle = ((ZGridColumnStyle)column.ColumnStyle);
			AssertEquals(columnName + " GroupName", groupName.Caption, column.GroupName.Caption);
			AssertEquals(columnName + " Caption", caption, columnStyle.HeaderText);
			AssertEquals(columnName + " Visible", visible, column.IsVisible);

			column.IsVisible = true;
			grid.RefreshTableStyles();
			AssertEquals(columnName + " Value", value, columnStyle.GetValueAsString(grid.ListManager, 0));
		}

		public static ZGridColumn AssertColumn(ZDisplayGrid grid, string columnName)
		{
			var column = grid.Columns[columnName];
			AssertNotNull(columnName + " column should exist", column);
			return column;
		}

		public static void AssertNoColumn(ZDisplayGrid grid, string columnName)
		{
			var column = grid.Columns[columnName];
			AssertNull(columnName + " column should not exist", column);
		}
	}

	sealed class ZFilterStripControlWithDummyTest : TestCaseWithFactory
	{
		#region IsFilterVisible

		public void TestFilterBoxIsCollapsedWhenHidden()
		{
			var filterBizO = new DummyFilterBusinessObject();

			using (var form = new ZForm())
			using (var module = new DummyFilterGridModule())
			{
				var filter = (DummyFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var grid = filter.RelatedGrid;
				var topWithOneStrip = grid.Top;

				filter.AddNewFilterStrip();
				filter.AddNewFilterStrip();
				Application.DoEvents();

				AssertGreaterThan("PRE: Height is increased to accomodate new strips", grid.Top, topWithOneStrip);

				filter.IsFilterVisible = false;
				Application.DoEvents();

				AssertLessThan("After collapsing the filter strips, the space that they consumed should be gone", grid.Top, topWithOneStrip);
			}
		}

		#endregion

		public void TestFilterGridColorContextKey()
		{
			using (var form = new ZForm())
			using (var module = new DummyFilterGridModule())
			{
				var filter = (DummyFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var filterBizO = (DummyFilterBusinessObject)filter.FilterBusinessObject;
				ZGrid grid = module.Grid;

				AssertEquals("", filter.FilteredGrid.ColorContextKey);
			}
		}

		public void TestSaveLayoutWhenSaveColumnLayoutChanges()
		{
			using (var form = new ZForm())
			using (var module = new DummyFilterGridModule())
			{
				var filter = (DummyFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				var filterBizO = (DummyFilterBusinessObject)filter.FilterBusinessObject;
				ZGrid grid = module.Grid;

				var layoutFactory = ((IModifyModuleAndGridLayout)filterBizO).Factory;

				var layout = layoutFactory.New<StmModuleFilter>();
				layout.S9_ModuleID = "Dummy";
				layout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				layout.S9_FilterName = "Test";
				layout.S9_SaveColumnLayout = true;
				layout.S9_ColumnLayoutData = ZBlob.FromAscii("AHJD*");
				layoutFactory.Save();

				grid.CurrentColumnLayout = layout;

				filter.AddOrUpdateExistingFindDropListItemExposed(layout);

				var currentlySelectedFilter = layoutFactory.New<StmData>();

				foreach (var key in new GridLayoutContextKeyProviderHelper().GetAllGridIDsForStmData(grid))
				{
					currentlySelectedFilter.SD_Name = key;
					break;
				}
				currentlySelectedFilter.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
				currentlySelectedFilter.SD_GuidValue = layout.PK;

				layout.S9_SaveColumnLayout = false;

				layoutFactory.Save();
				AssertEquals("layout.S9_ColumnLayoutData should be cleared out", ZBlob.Empty, layout.S9_ColumnLayoutData);
				AssertNotEquals("currentlyselectedFilter should have the grid layout information instead", ZBlob.Empty, currentlySelectedFilter.SD_BinaryValue);
			}
		}

		public void TestDragDropIfNotAllowCreateBizo()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var controller = new DummyControllerWithNotAllowCreate();
				module.ControllerOverride = controller;
			
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				module.SetNewMenuItem(null);
				AssertNoExceptionThrown(() => SimulateDragDropOnBlankSpaceOfGrid(filter));
			}
		}

		public void TestFindButtonDropDownItemClickedNotSavesLayout()
		{
			var testFilter = new ModuleTextFilter("Test", DummyBizoSchema.GenericStringSchemaColumn);
			testFilter.Visibility = FilterVisibility.AlwaysVisible;
			testFilter.IsActive = true;
			testFilter.Property = "ABC";

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new Form())
			{
				var mockFilterControl = new Mock<FilterStripControlTest.DummyZFilterStripControl>(collection, filterBizO) { CallBase = true };
				var filterControl = mockFilterControl.Object;

				form.Controls.Add(filterControl);
				form.Show();

				var mockHandler = new Mock<SaveLayoutUserQueryHandler>() { CallBase = true };
				var mockSaveLayoutBizObj = new Mock<SaveLayoutBizO>(filterBizO) { CallBase = true };
				mockSaveLayoutBizObj.Object.LayoutName = "hiwre7984wrhuj";
				mockSaveLayoutBizObj.Object.PublishLayout = ZBool.True;

				using (var layoutForm = new SaveLayoutForm(mockSaveLayoutBizObj.Object, true, true))
				{
					layoutForm.IsOkToSave = true;

					mockHandler.Protected().Setup<SaveLayoutBizO>("GetSaveLayoutBizObj", ItExpr.IsAny<IModifyModuleAndGridLayout>()).Returns(mockSaveLayoutBizObj.Object);
					mockHandler.Protected().Setup<SaveLayoutForm>("GetSaveLayoutForm", ItExpr.IsAny<SaveLayoutBizO>(), ItExpr.IsAny<bool>(), ItExpr.IsAny<bool>()).Returns(layoutForm);
					mockFilterControl.Protected().Setup<SaveLayoutUserQueryHandler>("GetQueryHander").Returns(mockHandler.Object);

					AssertNull("PreCondition", filterBizO.FindLayout(mockSaveLayoutBizObj.Object.LayoutName, true));

					filterControl.HandleFindButtonDropDownItemClickExposed(filterControl.ToolStripFindDropButtonExposed.DropDownItems[0]);

					AssertEquals("LayoutManaget.SaveLayout not called", 0, filterBizO.Layouts.Count);
				}
			}
		}

		public void TestGrid()
		{
			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNotNull(filter.Grid);
				AssertEquals(filter.FilteredGrid, filter.Grid);
				Assert("Grid should be visible", filter.Grid.Visible);
				AssertEquals(DummyModuleIDs.Dummy.Name, filter.FilteredGrid.ColorContextKey);

				var formSize = form.Size = ControlDpiScalingHelper.NewScaledSize(400, 400);
				var gridSize = filter.Grid.Size;

				form.Size = ControlDpiScalingHelper.NewScaledSize(500, 500);
				var expectedGridHeight = gridSize.Height + (form.Size.Height - formSize.Height);
				var expectedGridWidth = gridSize.Width + (form.Size.Width - formSize.Width);
				AssertEquals("Grid height should have resized", expectedGridHeight, filter.Grid.Size.Height);
				AssertEquals("Grid width should have resized", expectedGridWidth, filter.Grid.Size.Width);
			}
		}

		public void TestSaveLayoutWithoutProperSecurity()
		{
			var staff = Factory.New<MasterFiles.Integration.IGlbStaff>();
			staff.GS_LoginName = "TestName";
			((BusinessObject)staff).FillWithValidTestData();
			Factory.Save();

			var testFilter = new ModuleTextFilter("Test", DummyBizoSchema.Z0_Code);
			testFilter.Visibility = FilterVisibility.AlwaysVisible;
			testFilter.IsActive = true;
			testFilter.Property = "ABC";

			var filterBizO = new DummyFilterStripBusinessObject();
			filterBizO.AddModuleFilterForTest(testFilter);

			var collection = new DummyBusinessObjectCollection(Factory);

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)))
			using (var module = new OverridenFilterGridModule())
			using (var form = new MainFormForTesting(module))
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				filter.BypassSecruityCheckForTest = false;
				form.Controls.Add(filter);
				form.Show();

				filter.ToolStripSaveLayoutButtonExposed.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains(Security.SecurityCore.SecurityErrorMessage));
			}
		}

		#region Drag and Drop

		public void TestDragOver_RowIndexIsChanged()
		{
			var dummy = new DummyBusinessObjectCollection(Factory);

			for (var i = 0; i < 10; ++i)
			{
				dummy.AddNew();
			}

			using (var form = new ZForm(dummy))
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				Factory.Save();
				module.PerformSearch();

				form.Show();

				filter.Grid.CurrentRowIndex = 0;

				// Simulate drag Over
				SimulateDragOver(filter, 1, dragFile: false);
				AssertEquals("The current row should still be 0 as no data can be accepted", 0, filter.Grid.CurrentRowIndex);

				Env.Security.eDocsModify.IsAllowed = true;

				SimulateDragOver(filter, -1);
				AssertEquals("The current row should still be 0 as the mouse is out of the Bounds", 0, filter.Grid.CurrentRowIndex);

				SimulateDragOver(filter, 1);
				AssertEquals(1, filter.Grid.CurrentRowIndex);

				SimulateDragOver(filter, 2);
				AssertEquals(2, filter.Grid.CurrentRowIndex);

				SimulateDragOver(filter, filter.Grid.ListManager.Count);
				AssertEquals(2, filter.Grid.CurrentRowIndex);
			}
		}

		public void TestCanAcceptCommonData()
		{
			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				Env.Security.eDocsModify.IsAllowed = false;

				AssertEquals("Should be false as security eDocsModify is not allowed", false, filter.CanAcceptCommonDataExposed(new DataObject()));

				Env.Security.eDocsModify.IsAllowed = true;

				AssertEquals("Should be false as DataFormat is not FileDrop", false, filter.CanAcceptCommonDataExposed(new DataObject()));

				var dataObject = new DataObject(DataFormats.FileDrop, new[] { "Test.PDF" });

				AssertEquals("Should be true", true, filter.CanAcceptCommonDataExposed(dataObject));
			}
		}

		public void TestOnDragDrop_ShouldOpenEditForm_WhenDroppingOnRow()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				SimulateDragDropOnCell(filter);

				AssertControllerTargetedAtBusinessObjectFromGrid(module, filter);
				AssertEditFormOpenedForSavedBusinessObject_AndCloseForm();
			}
		}

		public void TestOnDragDrop_ShouldNotOpenEditOrCreateForm_WhenDroppingOnOutsideGrid()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				SimulateDragDropOnOutsideGrid(filter);

				AssertNoEditOrCreateFormOpened();
			}
		}

		public void TestOnDragDrop_ShouldOpenCreateForm_WhenDroppingOnGridColumnHeaders()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				SimulateDragDropOnGridColumnsHeader(filter);

				AssertControllerTargetedAtNewBusinessObject(module);
				AssertCreateFormOpenedForNewBusinessObject_AndCloseForm();
			}
		}

		public void TestOnDragDrop_ShouldOpenCreateForm_WhenDroppingOnWhiteSpaceOfTheGrid()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				SimulateDragDropOnBlankSpaceOfGrid(filter);

				AssertControllerTargetedAtNewBusinessObject(module);
				AssertCreateFormOpenedForNewBusinessObject_AndCloseForm();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		public void TestOnDragDrop_WhenFormDoesNotSupportEDocs()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessages();
				SimulateDragDropOnCell(filter);

				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Form 'Dummy' does not support eDocs"));
				AssertControllerTargetedAtBusinessObjectFromGrid(module, filter);

				var childForm = Application.OpenForms.OfType<ZDummyForm>().SingleOrDefault();
				childForm?.Close();
			}
		}

		public void TestOnDragDrop_WhenFormCannotBeCreated()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule())
			{
				var controller = new DummyControllerWithNullForm();
				module.ControllerOverride = controller;

				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				AssertNoExceptionThrown("When the form is null, we should just ignore the drop. We don't need to show the user anything, as the controller tends to do this already.", () => SimulateDragDropOnCell(filter));
				AssertGreaterThan("We should have called GetForm in order to check the edocs stuff", controller.GetFormCount, 0);
				AssertControllerTargetedAtBusinessObjectFromGrid(module, filter);
			}
		}

		public void TestShouldAcceptDataOnControllerFactory()
		{
			FillGridWithDummies();

			using (var form = new ZForm())
			using (var module = new OverridenFilterGridModule(canAcceptData: true))
			{
				var controller = new DummyController();
				module.ControllerOverride = controller;

				var filter = (OverridenFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				filter.FirePerformSearch();
				Application.DoEvents();

				SimulateDragDropOnCell(filter);

				AssertControllerTargetedAtBusinessObjectFromGrid(module, filter);
				AssertEditFormOpenedForSavedBusinessObject_AndCloseForm();

				AssertNotNull("Should accept data", filter.TargetBizoToAcceptData);
				AssertEquals(controller.Factory, filter.TargetBizoToAcceptData.Factory);
			}
		}

		void FillGridWithDummies()
		{
			Factory.New<DummyBusinessObject>();
			Factory.New<DummyBusinessObject>();
			Factory.Save();
		}

		void SimulateDragDropOnCell(ZFilterStripControl filter, int row = 0, int column = 0)
		{
			filter.Grid.SetCurrentHitTestForTest(row, column);
			var cellPosition = filter.Grid.GetCellBounds(row, column).Location;
			var point = filter.Grid.PointToScreen(cellPosition);
			SimulateDragDrop(filter, point);
		}

		void SimulateDragDropOnOutsideGrid(ZFilterStripControl filter)
		{
			var point = filter.Grid.PointToScreen(new Point(-1, -1));
			SimulateDragDrop(filter, point);
		}

		void SimulateDragDropOnGridColumnsHeader(ZFilterStripControl filter)
		{
			var point = filter.Grid.PointToScreen(new Point(1, 1));
			SimulateDragDrop(filter, point);
		}

		void SimulateDragDropOnBlankSpaceOfGrid(ZFilterStripControl filter)
		{
			var point = filter.Grid.PointToScreen(new Point(1, filter.Grid.Bounds.Height - 1));
			SimulateDragDrop(filter, point);
		}

		void SimulateDragDrop(ZFilterStripControl control, Point point)
		{
			var eventArgs = GetDragEventArgs(point);
			Invoke("OnDragOver", control, eventArgs);
			Invoke("OnDragDrop", control, eventArgs);
		}

		void SimulateDragOver(ZFilterStripControl control, int row, int column = -1, bool dragFile = true)
		{
			control.Grid.SetCurrentHitTestForTest(row, column);
			int x, y;

			if (row == -1)
			{
				y = control.Bounds.Y - 1;
			}
			else if (row < control.Grid.ListManager.Count)
			{
				y = control.Grid.GetCellBounds(row, 0).Y + 1;
			}
			else
			{
				y = control.Bounds.Bottom + 1;
			}

			if (column == -1)
			{
				x = control.Grid.GetRowNotificationRectangle(0).X + 1;
			}
			else
			{
				x = control.Grid.GetCellBounds(0, column).X + 1;
			}

			Invoke("OnDragOver", control, GetDragEventArgs(control.Grid.PointToScreen(new Point(x, y)), dragFile));
		}

		DragEventArgs GetDragEventArgs(Point point, bool dragFile = true)
		{
			var dataObject = dragFile ? new DataObject(DataFormats.FileDrop, new[] { "TestTextFile.txt" }) : new DataObject();
			return new (dataObject, 0, point.X, point.Y, DragDropEffects.Copy | DragDropEffects.Move, DragDropEffects.None);
		}

		void Invoke(string name, ZFilterStripControl control, object e) => control.GetType().InvokeMember(name, BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, control, new object[] { e });

		void AssertControllerTargetedAtNewBusinessObject(OverridenFilterGridModule module)
		{
			AssertNull("The controller should be targeted at a new business object", module.TargetBizo);
		}

		void AssertControllerTargetedAtBusinessObjectFromGrid(OverridenFilterGridModule module, ZFilterStripControl filter)
		{
			var currentBizo = (BusinessObject)filter.FilteredGrid.ListManager.List[filter.FilteredGrid.CurrentRowIndex];
			AssertNotNull(currentBizo);
			AssertType<DummyBusinessObject>(currentBizo);
			AssertEquals("The controller should be targeted at the business object from the grid", currentBizo, module.TargetBizo);
		}

		void AssertEditFormOpenedForSavedBusinessObject_AndCloseForm()
		{
			var bizo = GetOpenedFormBizoAndCloseForm();
			Assert("Opened bizo should be already saved in the database", bizo.IsInDatabaseIncludingChildren);
		}

		void AssertCreateFormOpenedForNewBusinessObject_AndCloseForm()
		{
			var bizo = GetOpenedFormBizoAndCloseForm();
			Assert("Opened bizo should not be saved in the database", !bizo.IsInDatabaseIncludingChildren);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		IBusiness GetOpenedFormBizoAndCloseForm()
		{
			var childForm = Application.OpenForms.OfType<ZDummyForm>().SingleOrDefault();
			AssertNotNull(childForm);
			var bizo = childForm.BusinessEntity;
			childForm.Close();
			return bizo;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:NoApplicationOpenFormsRule", Justification = "Testing")]
		void AssertNoEditOrCreateFormOpened()
		{
			AssertNull(Application.OpenForms.OfType<ZDummyForm>().SingleOrDefault());
		}

		#endregion

#if !WINZOR
		[DeveloperOnlyTest]
		public void TestZToolStripSplitButtonAndZDropButtonArrow()
		{
			using (var form = new ZForm())
			using (var module = new DummyFilterGridModule())
			{
				form.Width = 1200;
				form.Height = 800;

				var filter = (DummyFilterControl)module.EmbeddedControl;
				filter.AddNewFilterStrip();
				form.Controls.Add(filter);

				form.Controls.RemoveByKey("RecentItemsPanel");
				form.Show();
				Application.DoEvents();

				Thread.Sleep(2000);

				var toolStripControl = filter.Controls.Find("ToolStripHelp", false).FirstOrDefault();
				if (toolStripControl is ZToolStrip toolStripHelp)
				{
					var toolStripItem = toolStripHelp.Items.Find("ToolStripManageDropButton", false).FirstOrDefault();
					if (toolStripItem is ZToolStripSplitButton tollStripSplitButton)
					{
						var dropDownButton = tollStripSplitButton.DropDownButtonBounds;
						using (var bmp = new Bitmap(toolStripControl.Width, toolStripControl.Height))
						using (var graphics = Graphics.FromImage(bmp))
						{
							graphics.CopyFromScreen(toolStripControl.PointToScreen(Point.Empty), Point.Empty, toolStripControl.Size);

							var dropDownButtonStartX = tollStripSplitButton.Bounds.X + dropDownButton.X;
							var dropDownButtonStartY = tollStripSplitButton.Bounds.Y + dropDownButton.Y;
							var dropDownButtonEndX = tollStripSplitButton.Bounds.X + dropDownButton.X + dropDownButton.Width;
							var dropDownButtonEndY = tollStripSplitButton.Bounds.Y + dropDownButton.Y + dropDownButton.Height;

							var arrowPixelCount = 0;
							for (var i = dropDownButtonStartX; i < dropDownButtonEndX; i++)
							{
								for (var j = dropDownButtonStartY; j < dropDownButtonEndY; j++)
								{
									var color = bmp.GetPixel(i, j);
									if (SystemColors.ControlText.ToArgb() == color.ToArgb())
									{
										arrowPixelCount++;
									}
								}
							}
							AssertGreaterThanOrEqualTo("Arrow pixel count should be at least 16(7 + 5 + 3 + 1, scale 100% scene)", arrowPixelCount, 16);
						}
					}
				}

				var zDropButton = filter.FindSingle<ZDropButton>();
				var buttonRectangleField = typeof(ZDropButton).GetProperty("ButtonRectangle", BindingFlags.NonPublic | BindingFlags.Instance);
				if (buttonRectangleField.GetValue(zDropButton) is Rectangle buttonRectangle)
				{
					var borderPixelCount = 0;
					var backgroundPixelCount = 0;
					var arrowPixelCount = 0;
					using (var bmp = new Bitmap(zDropButton.Width, zDropButton.Height))
					using (var graphics = Graphics.FromImage(bmp))
					{
						graphics.CopyFromScreen(zDropButton.PointToScreen(Point.Empty), Point.Empty, zDropButton.Size);

						var buttonRectangleStartX = buttonRectangle.X;
						var buttonRectangleStartY = buttonRectangle.Y;
						var buttonRectangleEndX = buttonRectangle.X + buttonRectangle.Width;
						var buttonRectangleEndY = buttonRectangle.Y + buttonRectangle.Height;

						for (var i = buttonRectangleStartX; i < buttonRectangleEndX; i++)
						{
							for (var j = buttonRectangleStartY; j < buttonRectangleEndY; j++)
							{
								var color = bmp.GetPixel(i, j);
								if (Color.FromArgb(255, 210, 210, 210).ToArgb() == color.ToArgb())
								{
									borderPixelCount++;
								}

								if (Color.FromArgb(255, 253, 253, 253).ToArgb() == color.ToArgb())
								{
									backgroundPixelCount++;
								}

								if (Color.FromArgb(255, 110, 110, 110).ToArgb() == color.ToArgb())
								{
									arrowPixelCount++;
								}
							}
						}
						AssertGreaterThanOrEqualTo("Arrow pixel count should be at least 7(2 + 2 + 2 + 1, , scale 100% scene", arrowPixelCount, 7);
					}
				}
			}
		}
#endif

		#region Implementation

		protected override void TearDown()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			base.TearDown();
		}

		#endregion

		#region OverridenFilterGridModule

		public class OverridenFilterGridModule : DummyFilterGridModule
		{
			public OverridenFilterGridModule(bool canAcceptData = false)
				: base()
			{
				this.canAcceptData = canAcceptData;
			}

			readonly bool canAcceptData;

			public void SetNewMenuItem(ZMenuItem menuItem)
			{
				NewMenuItem = menuItem;
			}

			protected override IFilterControl GetNewFilterControl()
			{
				return new OverridenFilterControl(GridCollection, FilterBusinessObject, canAcceptData);
			}

			protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				TargetBizo = selectedBusinessObject;
				return ControllerOverride ?? base.GetNewController(selectedBusinessObject);
			}

			public ZController ControllerOverride { get; set; }

			public BusinessObject TargetBizo { get; private set; }
		}

		class DummyControllerWithNullForm : DummyController
		{
			public int GetFormCount { get; set; }

			public override IZForm GetFormCore(IBusiness businessEntity)
			{
				GetFormCount++;
				return null;
			}
		}

		class DummyControllerWithNotAllowCreate : DummyController
		{
			public override IZForm GetFormCore(IBusiness businessEntity)
			{
				throw new Exception("The controller does not support creating");
			}
		}
		#endregion

		#region OverridenFilterControl

		[SuppressFormDesignerAnalysis]
		[ToolboxItem(false)]
		public class OverridenFilterControl : DummyFilterControl, IFilterStripControlForTest
		{
			public OverridenFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
				: this(gridCollection, filterBusinessObject, canAcceptData: false)
			{
			}

			public OverridenFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject, bool canAcceptData)
				: base(gridCollection, filterBusinessObject)
			{
				InitializeComponent();
				this.canAcceptData = canAcceptData;
			}

#pragma warning disable IDE0001 // Simplify Names
			#region Component Designer generated code

			readonly System.ComponentModel.Container components;

			protected override void Dispose(bool isNotFinalizing)
			{
				if (isNotFinalizing && components != null)
				{
					components.Dispose();
				}

				base.Dispose(isNotFinalizing);
			}

			private void InitializeComponent()
			{
				var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				var zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();

				((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
				this.SuspendLayout();
				//
				// FilteredGrid
				//
				zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
				zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
				zCalcEditColumnStyleInfo1.ColumnName = "Z0_Number";
				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
				this.FilteredGrid.Name = "FilterGridForResize";
				this.FilteredGrid.Size = new System.Drawing.Size(556, 264);
				this.FilteredGrid.TabIndex = 5;
				// DummyFilterControl
				//
				this.Name = "OverridenFilterControl";
				this.Controls.SetChildIndex(this.FilteredGrid, 0);
				((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
				this.ResumeLayout(false);
			}

			#endregion
#pragma warning restore IDE0001 // Simplify Names

			#region Expose

			public CargoWise.GUI.TileBar.RecentItemsControl RecentItemsControlExposed
			{
				get { return RecentItemsControl; }
			}

			public void SetShouldPerformSearch(bool shouldPerformSearch)
			{
				this.shouldPerformSearch = shouldPerformSearch;
			}
			bool shouldPerformSearch = true;

			protected override ZBool ShouldPerformSearch()
			{
				return shouldPerformSearch;
			}

			public void SetMaximumAllowableQueriesPerSqlStatement(int maxRecords)
			{
				this.maxRecords = maxRecords;
			}
			int maxRecords;

			public int MaximumAllowableQueriesPerSqlStatementExposed
			{
				get { return MaximumAllowableQueriesPerSqlStatement; }
			}

			protected override int MaximumAllowableQueriesPerSqlStatement
			{
				get { return maxRecords == 0 ? base.MaximumAllowableQueriesPerSqlStatement : maxRecords; }
			}

			public void HandleFindButtonDropDownItemClickExposed(ToolStripItem item)
			{
				HandleFindButtonDropDownItemClick(item);
			}

			public ToolStripSplitButton ToolStripFindDropButtonExposed
			{
				get { return ToolStripFindDropButton; }
			}

			public ToolStripSplitButton FindButtonExposed
			{
				get { return base.ToolStripFindDropButton; }
			}

			public ToolStripButton ToolStripSaveLayoutButtonExposed
			{
				get { return base.ToolStripSaveLayoutButton; }
			}

			public ToolStripMenuItem ToolStripManageLayoutsButtonExposed
			{
				get { return base.ToolStripManageLayoutsMenuItem; }
			}

			public bool CanSaveColumnLayoutsExposed
			{
				get { return CanSaveColumnLayouts; }
			}

			public bool CanSaveGridColoursExposed
			{
				get { return CanSaveGridColours; }
			}

			public KPanel FilterStripsPanelExposed
			{
				get { return FilterStripsPanel; }
			}

			public ZToolStrip ToolStripHelpExposed
			{
				get { return ToolStripHelp; }
			}

			public ToolStripButton ToolStripClearButtonExposed
			{
				get { return ToolStripClearButton; }
			}

			public bool ProcessDialogKeyExposed(Keys keyData)
			{
				return ProcessDialogKey(keyData);
			}

			protected override bool CanSaveColumnLayouts
			{
				get { return canSaveColumnLayouts; }
			}

			bool canSaveColumnLayouts = true;

			public void SetCanSaveColumnLayouts(bool canSaveColumnLayouts)
			{
				this.canSaveColumnLayouts = canSaveColumnLayouts;
			}

			protected override bool CanSaveGridColours
			{
				get { return canSaveGridColours; }
			}

			bool canSaveGridColours = true;

			public void SetCanSaveGridColours(bool canSaveGridColours)
			{
				this.canSaveGridColours = canSaveGridColours;
			}

			#endregion

			#region Accept Data

			readonly bool canAcceptData;

			protected override bool CanAcceptDataCore(IDataObject dataObject) => canAcceptData;

			protected override void AcceptDataCore(IDataObject dataObject, BusinessObject targetBizo)
			{
				base.AcceptDataCore(dataObject, targetBizo);
				TargetBizoToAcceptData = targetBizo;
			}

			public BusinessObject TargetBizoToAcceptData { get; private set; }

			#endregion

			#region ShouldSetColorContextKeyFromParentModuleID

			protected override ZBool ShouldSetColorContextKeyFromParentModuleID => true;

			#endregion
		}

		#endregion

		#region MainFormForTesting

		internal class MainFormForTesting : ZForm, IMainForm
		{
			public MainFormForTesting(ZFilterModule module) : base()
			{
				CurrentModule = module;
				module.SetFormsModalTo(this);
			}

			public INamedModule CurrentModule { get; }

			public string CurrentModuleLicenceCheckPointName => CurrentModule?.LicenceCheckpoint?.Name ?? string.Empty;

			public void UpdateToolBarDeleteButton(ZEmbeddedModule embeddedModule) { }
		}

		#endregion
	}
}
