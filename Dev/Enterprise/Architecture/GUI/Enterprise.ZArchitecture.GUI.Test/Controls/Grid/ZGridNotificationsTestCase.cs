using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture
{
	[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "TestCase with TearDown")]
	public class ZGridNotificationsTestCase : SetVisibleOnControlUpdatesNotificationsTestCase
	{
		class BadlyBehavingControl : TextBox
		{
#if !WINZOR
			const int WM_REFLECT = 0x2000;

			protected override void WndProc(ref Message m)
			{
				if (m.Msg == WindowsMessage.WM_COMMAND + WM_REFLECT)
				{
					throw new ArgumentOutOfRangeException(nameof(m), m, "Caused by developer test code, ignore any issue this message is found in");
				}
				else
				{
					base.WndProc(ref m);
				}
			}
#endif
		}

		public void TestHotkeyManagerIsCalled()
		{
			var wasCalled = false;
			grid.Hotkeys.RegisterHotKey(Keys.Control | Keys.J, (o, k) => wasCalled = true);

			var msg = new Message();
			grid.ProcessCmdKey(ref msg, Keys.Control | Keys.J);

			Assert("Should be called", wasCalled);
		}

		public void TestTraceReportArgumentOutOfRangeException()
		{
			var previousSuppressReportingOfErrors = ErrorReporter.SuppressReportingOfErrors;
			ErrorReporter.SuppressReportingOfErrors = true;
			ErrorReporter.Clear();

			form.Show();
			using (var badlyBehavingControl = new BadlyBehavingControl())
			{
				grid.Controls.Add(badlyBehavingControl);
				badlyBehavingControl.Text = "killer";
			}

			Assert("Should have triggered an error report", ErrorReporter.TotalErrorCount > 0);
			ErrorReporter.Clear();
			ErrorReporter.SuppressReportingOfErrors = previousSuppressReportingOfErrors;
		}

		public void TestValidateLightValidationRowWhenCurrentChanges_AffectedByRegistry()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();

			EnvProxy.Instance.Registry.LightValidationEnabled = false;
			form.Show();
			grid.Focus();
			Application.DoEvents();

			grid.ListManager.Position = 0;
			grid.ListManager.Position = 1;
			AssertEquals(0, grid.ValidateOnPositionChangedIfIncreasesSavePerformanceCallCount);

			EnvProxy.Instance.Registry.LightValidationEnabled = true;

			grid.ListManager.Position = 0;
			grid.ListManager.Position = 1;
			AssertEquals(2, grid.ValidateOnPositionChangedIfIncreasesSavePerformanceCallCount);
		}

		public void TestValidateLightValidationRowWhenCurrentChanges()
		{
			EnvProxy.Instance.Registry.LightValidationEnabled = true;

			var child1 = Dummy.Collection.AddNew();
			var child2 = Dummy.Collection.AddNew();
			var child3 = Dummy.Collection.AddNew();
			form.Show();
			grid.Focus();
			Application.DoEvents();

			AssertEquals("ShouldValidateOnSave", true, child1.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", true, child2.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", true, child3.ShouldValidateOnSave);

			grid.ListManager.Position = 1;
			AssertEquals("ShouldValidateOnSave", false, child1.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", true, child2.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", true, child3.ShouldValidateOnSave);

			grid.ListManager.Position = 2;
			AssertEquals("ShouldValidateOnSave", false, child1.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", false, child2.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", true, child3.ShouldValidateOnSave);

			grid.ListManager.Position = 1;
			AssertEquals("ShouldValidateOnSave", false, child1.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", false, child2.ShouldValidateOnSave);
			AssertEquals("ShouldValidateOnSave", false, child3.ShouldValidateOnSave);
		}

		public void TestNonComittedElementDoesNotGetValidated()
		{
			EnvProxy.Instance.Registry.LightValidationEnabled = true;

			form.Show();
			grid.Focus(); // will add non-committed row
			Application.DoEvents();

			var bo = (DummyChildBusinessObject)grid.List[0];
			AssertEquals("Should not have already been validated", true, bo.ShouldValidateOnSave);
		}

		public void TestNotificationsAreOnlyUpdatedOnceForMultipleChanges()
		{
			form.Show();
			button.Focus();
			var child1 = Dummy.Collection.AddNew();
			var child2 = Dummy.Collection.AddNew();
			var child3 = Dummy.Collection.AddNew();
			Application.DoEvents();

			var startCount = grid.UpdateNonCellNotificationsCountForTesting;
			using (child1.SuspendValidationTesting())
			using (child2.SuspendValidationTesting())
			using (child3.SuspendValidationTesting())
			{
				child1.Z0_DescriptionInfo.AddWarning("This is a warn");
				child2.Z0_DescriptionInfo.AddError("This is an error");
				child3.Z0_NumberInfo.AddError("err");
				child1.AddRowError("error");
				child3.AddRowError("more");
			}
			Application.DoEvents();
			AssertEquals(startCount + 1, grid.UpdateNonCellNotificationsCountForTesting);
		}

		public void TestNavigateToFirstCellWithHighestPriorityNotificationStandard()
		{
			form.Show();
			button.Focus();
			var child1 = Dummy.Collection.AddNew();
			var child2 = Dummy.Collection.AddNew();
			var child3 = Dummy.Collection.AddNew();
			using (child1.SuspendValidationTesting())
			using (child2.SuspendValidationTesting())
			using (child3.SuspendValidationTesting())
			{
				child1.Z0_DescriptionInfo.AddWarning("This is a warn");
				child2.Z0_DescriptionInfo.AddError("This is an error");
				child3.Z0_NumberInfo.AddError("err");
			}
			Application.DoEvents();
			grid.NavigateToFirstCellWithHighestPriorityNotification();
			AssertEquals(new DataGridCell(1, 0), grid.CurrentCell);
		}

		public void TestOpeningContextMenuFinalisesCurrentEdit_CancelBizoCreation()
		{
			GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true); // TODO: Check this, not sure if this fixes the test failure
			Dummy.Collection.AddNew();
			Factory.Save();

			form.Show();
			grid.Select(0);
			KeySender.PostKeyDown(grid, Keys.Down);
			Application.DoEvents();

			var uncommittedRow = (BusinessObject)grid.List[1];
			Assert("PRE: the row should be uncommitted", ((IBusinessObjectInternals)uncommittedRow).IsUnCommittedRow);

			var wasCalled = false;
			grid.ContextMenu.Popup += (o, e) =>
			{
				wasCalled = true;
				Assert("Uncommitted row should be cancelled and Uncommitted pivot should be cancelled.", uncommittedRow.IsDeleted);
				form.ForceClose();
			};

			// Emulate click(or KeyUp) in a cell in first row
			EmulateMouseClick(GetCenterOfCell(grid, new DataGridCell(1, 1)), MouseButtons.Right);
			Assert("The assertions should happen on popup", wasCalled);
		}

		public void TestBeforeProcessingMenuItemCommitsAnyChanges()
		{
			Dummy.Collection.AddNew();
			Factory.Save();

			form.Show();
			grid.Select(0);

			KeySender.PostKeyDown(grid, Keys.Down);
			Application.DoEvents();

			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D1);
			Application.DoEvents();

			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D1);
			Application.DoEvents();

			KeySender.PostKeyDown(grid, Keys.Right);
			Application.DoEvents();

			var row = (IBusinessObjectInternals)grid.List[1];

			Assert("PRE: the row should be uncommitted", row.IsUnCommittedRow);

			((IMenuParent)grid).BeforeProcessingMenuItem();

			Assert("The row should be committed in BeforeProcessingMenuItem", !row.IsUnCommittedRow);
		}

		public void TestOpeningContextMenuDoesNotChangeSelectedItems()
		{
			var committedBizo = Dummy.Collection.AddNew();
			Factory.Save();

			form.Show();
			grid.Select(0);
			KeySender.PostKeyDown(grid, Keys.Down);
			Application.DoEvents();

			var uncommittedBizo = (DummyChildBusinessObject)grid.List[1];
			Assert("PRE: the row should be uncommitted", ((IBusinessObjectInternals)uncommittedBizo).IsUnCommittedRow);

			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D1);
			Application.DoEvents();

			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D1);
			Application.DoEvents();

			KeySender.PostKeyDown(grid, Keys.Right);
			Application.DoEvents();

			grid.Select(0);
			grid.Select(1);

			AssertContainsExactElementsInAnyOrder("PRE: We are selecting the rows", new[] { committedBizo, uncommittedBizo }, grid.SelectedElements);

			grid.ContextMenu.Popup += (o, e)
				=> AssertContainsExactElementsInAnyOrder("The selected rows remains unchanged within the event", new[] { committedBizo, uncommittedBizo }, grid.SelectedElements);

			grid.ContextMenu.ShowPopupMenu();

			AssertContainsExactElementsInAnyOrder("The selected rows remains unchanged", new[] { committedBizo, uncommittedBizo }, grid.SelectedElements);
		}

		public void TestOpeningContextMenuFinalisesCurrentEdit_CommitBizo()
		{
			Dummy.Collection.AddNew();
			Factory.Save();

			form.Show();
			grid.Select(0);
			KeySender.PostKeyDown(grid, Keys.Down);
			Application.DoEvents();

			var freshRow = (DummyChildBusinessObject)grid.List[1];
			Assert("PRE: the row should be uncommitted", ((IBusinessObjectInternals)freshRow).IsUnCommittedRow);

			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D1);
			Application.DoEvents();

			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, Keys.D1);
			Application.DoEvents();

			KeySender.PostKeyDown(grid, Keys.Right);
			Application.DoEvents();

			var wasCalled = false;
			grid.ContextMenu.Popup += (o, e) =>
			{
				wasCalled = true;

				Assert("New row should not be cancelled/deleted if it is commit-able", !freshRow.IsDeleted);
				Assert("New row should now be committed", !((IBusinessObjectInternals)freshRow).IsUnCommittedRow);

				form.ForceClose();
			};

			EmulateMouseClick(GetCenterOfCell(grid, new DataGridCell(1, 2)), MouseButtons.Right);

			Assert("The assertions should happen on popup", wasCalled);
		}

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "IntPtr redundant cast required")]
		void EmulateMouseDown(Point location, MouseButtons b = MouseButtons.Left)
		{
#if !WINZOR
			var msg = b == MouseButtons.Left ? WindowsMessage.WM_LBUTTONDOWN :
				b == MouseButtons.Right ? WindowsMessage.WM_RBUTTONDOWN :
				WindowsMessage.WM_MBUTTONDOWN;

			var mouseClickMessage = new Message { Msg = msg, HWnd = grid.Handle, LParam = ((IntPtr)((location.X << 16) + ControlDpiScalingHelper.ScaleToCurrentDpiY(location.Y))) };
			grid.WndProc(ref mouseClickMessage);
			Application.DoEvents();
#endif
		}

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "IntPtr redundant cast required")]
		void EmulateMouseUp(Point location, MouseButtons b = MouseButtons.Left)
		{
#if !WINZOR
			var msg = b == MouseButtons.Left ? WindowsMessage.WM_LBUTTONUP :
				b == MouseButtons.Right ? WindowsMessage.WM_RBUTTONUP :
				WindowsMessage.WM_MBUTTONUP;

			var mouseClickMessage = new Message { Msg = msg, HWnd = grid.Handle, LParam = ((IntPtr)((location.X << 16) + ControlDpiScalingHelper.ScaleToCurrentDpiY(location.Y))) };
			grid.WndProc(ref mouseClickMessage);
			Application.DoEvents();
#endif
		}

		void EmulateMouseClick(Point location, MouseButtons b = MouseButtons.Left)
		{
			EmulateMouseDown(location, b);
			EmulateMouseUp(location, b);
		}

		Point GetCenterOfCell(ZGrid g, DataGridCell cell)
		{
			var bounds = g.GetCellBounds(cell);
			return bounds.Location + ControlDpiScalingHelper.NewScaledSize(bounds.Width / 2, bounds.Height / 2, false);
		}

		public void TestContextMenuHandlesLazyMenuItemShortcuts()
		{
			var menuItemRoot = grid.ContextMenu.MenuItems.Add("Root Menu Item");
			menuItemRoot.MenuItems.Add("Placeholder");

			var subMenuClickCount = 0;
			menuItemRoot.Popup += (o, e) =>
			{
				var old = new DisposableList(menuItemRoot.MenuItems.Cast<MenuItem>());
				menuItemRoot.MenuItems.Clear();
				old.Dispose();

				var subMenuItem = menuItemRoot.MenuItems.Add("Sub Menu Item");
				subMenuItem.Shortcut = Shortcut.CtrlShiftM;
				subMenuItem.Click += (so, se) => subMenuClickCount++;
			};

			form.Show();
			Application.DoEvents();

			AssertEquals("Before pressing anything the count should be zero", 0, subMenuClickCount);

			KeySender.SendKeyDownToProcessCmdKey(grid, (int)(Keys.Control | Keys.Shift | Keys.M));
			Application.DoEvents();
			AssertEquals("The menu item should have been created and pressed", 1, subMenuClickCount);

			KeySender.SendKeyDownToProcessCmdKey(grid, (int)(Keys.Control | Keys.Shift | Keys.M));
			Application.DoEvents();
			AssertEquals("The menu item should have been recreated and pressed", 2, subMenuClickCount);
		}

		public void TestNavigateToErrorCellByMouseClickInGridLeftCorner()
		{
			form.Show();
			button.Focus();
			var child1 = Dummy.Collection.AddNew();
			var child2 = Dummy.Collection.AddNew();
			var child3 = Dummy.Collection.AddNew();
			using (child1.SuspendValidationTesting())
			using (child2.SuspendValidationTesting())
			using (child3.SuspendValidationTesting())
			{
				child1.Z0_DescriptionInfo.AddWarning("This is a warn");
				child2.Z0_DescriptionInfo.AddError("This is an error");
				child3.Z0_NumberInfo.AddError("err");
			}
			Application.DoEvents();

			EmulateMouseDown(ControlDpiScalingHelper.NewScaledPoint(4, 4));

			AssertEquals(new DataGridCell(1, 0), grid.CurrentCell);
		}

#if !WINZOR

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "IntPtr redundant cast required")]
		public void TestWndProcInvisibleControl()
		{
			form.Show();
			var message = new Message { Msg = WindowsMessage.WM_CONTEXTMENU, LParam = (IntPtr)(-1) };

			grid.Visible = false;
			AssertNotNull(grid.ContextMenu);
			Assert(!grid.Visible);

			AssertNoExceptionThrown(() => grid.WndProc(ref message));
		}

#endif

		public void TestNavigateToHiddenErrorCellByMouseClickInGridLeftCorner()
		{
			form.Show();
			button.Focus();
			var child1 = Dummy.Collection.AddNew();
			var child2 = Dummy.Collection.AddNew();
			var child3 = Dummy.Collection.AddNew();
			using (child1.SuspendValidationTesting())
			using (child2.SuspendValidationTesting())
			using (child3.SuspendValidationTesting())
			{
				child1.Z0_DescriptionInfo.AddWarning("This is a warn");
				child2.Z0_DescriptionInfo.AddError("This is an error");
				child3.Z0_NumberInfo.AddError("err");
			}
			Application.DoEvents();

			AssertEquals(3, grid.TableStyles[0].GridColumnStyles.Count);

			grid.Columns[0].IsVisible = false;
			grid.Columns.HasLayoutChanged = true;
			grid.RefreshTableStyles();
			Application.DoEvents();

			AssertEquals(2, grid.TableStyles[0].GridColumnStyles.Count);

			EmulateMouseDown(new Point(4, 4));
			Application.DoEvents();

			AssertEquals(3, grid.TableStyles[0].GridColumnStyles.Count);
			AssertEquals(new DataGridCell(1, grid.TableStyles[0].GridColumnStyles.IndexOf(grid.Columns[0].ColumnStyle)), grid.CurrentCell);
		}

		public void TestNavigateToFirstCellWithHighestPriorityNotificationWhenNeedsToPullInColumn()
		{
			form.Show();
			button.Focus();

			grid.Columns[AutoDummyBizo.Schema.Z0_Number].IsVisible = false;
			grid.RefreshTableStyles();

			var child1 = Dummy.Collection.AddNew();
			var child2 = Dummy.Collection.AddNew();
			var child3 = Dummy.Collection.AddNew();
			using (child1.SuspendValidationTesting())
			using (child2.SuspendValidationTesting())
			using (child3.SuspendValidationTesting())
			{
				child1.Z0_DescriptionInfo.AddWarning("This is a warn");
				child3.Z0_NumberInfo.AddMessageError("mess err");
			}
			Application.DoEvents();
			AssertEquals("Visible", false, grid.Columns[AutoDummyBizo.Schema.Z0_Number].IsVisible);
			grid.NavigateToFirstCellWithHighestPriorityNotification();
			Application.DoEvents();
			AssertEquals("Visible", true, grid.Columns[AutoDummyBizo.Schema.Z0_Number].IsVisible);
			AssertEquals(new DataGridCell(2, 1), grid.CurrentCell);
		}

		public void TestNavigateToFirstCellWithHighestPriorityNotificationWhenCannotBeFound()
		{
			form.Show();
			button.Focus();
			Dummy.Collection.AddNew();
			Dummy.Collection.AddNew();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_NVarCharMaxInfo.AddWarning("This is a warn");
			}
			Application.DoEvents();
			grid.NavigateToFirstCellWithHighestPriorityNotification();
			AssertEquals("The warning is not in a cell in this grid. Have a look at other fields or grids on the form which display further information about the currently selected row in this grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(2, grid.ListManager.Position);
		}

		public void TestNavigateToFirstCellWithHighestPriorityNotification_ColumnsShouldBeSortedAfterMakeOneColumnVisible()
		{
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 50));
			form.Show();
			button.Focus();

			var gridColumnsCount = grid.Columns.Count;
			AssertEquals("There should be at least 3 columns in the grid", true, gridColumnsCount >= 3);

			grid.SetAllColumnsVisible(false);
			grid.Columns[0].IsVisible = true;
			grid.RefreshTableStyles();

			AssertEquals("The last column should be Z0_Code", "Z0_Code", grid.Columns.Last().ColumnName);
			AssertEquals("The last column should be invisible", false, grid.Columns.Last().IsVisible);

			var child = Dummy.Collection.AddNew();

			using (child.SuspendValidationTesting())
			{
				child.Z0_CodeInfo.AddWarning("This is a warning");
			}

			Application.DoEvents();
			grid.NavigateToFirstCellWithHighestPriorityNotification();

			AssertEquals("Grid columns count should not change", gridColumnsCount, grid.Columns.Count);
			AssertEquals("The second column should be Z0_Code", "Z0_Code", grid.Columns[1].ColumnName);
			AssertEquals("The second column should be visible", true, grid.Columns[1].IsVisible);
		}
#if !WINZOR
		public void TestCornerBalloonError()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddError("This is an error");
			}
			Application.DoEvents();
			grid.MousePosition = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are errors in the grid.", descriptor.Caption);
			AssertEquals("description", "Click on the icon to navigate to the first error.", descriptor.Description);
			AssertEquals("notifications: errors count", 1, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 0, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: message errors count", 0, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(100), 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}

		public void TestCornerBalloonMessageError()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddMessageError("This is an MessageError");
			}
			Application.DoEvents();
			grid.MousePosition = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are message errors in the grid.", descriptor.Caption);
			AssertEquals("description", "Click on the icon to navigate to the first message error.", descriptor.Description);
			AssertEquals("notifications: error count", 0, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 0, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: MessageErrors count", 1, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(100), 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}

		public void TestCornerBalloonWarning()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddWarning("This is an Warning");
			}
			Application.DoEvents();
			grid.MousePosition = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are warnings in the grid.", descriptor.Caption);
			AssertEquals("description", "Click on the icon to navigate to the first warning.", descriptor.Description);
			AssertEquals("notifications: error count", 0, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 1, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: message error count", 0, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(100), 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}

		public void TestCornerBalloonPriority()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddError("This is an error");
				child.Z0_DescriptionInfo.AddWarning("warn");
				child.Z0_DescriptionInfo.AddMessageError("message error");
			}
			Application.DoEvents();
			grid.MousePosition = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are errors in the grid.", descriptor.Caption);
			AssertEquals("description", "Click on the icon to navigate to the first error.", descriptor.Description);
			AssertEquals("notifications: errors count", 1, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 1, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: message errors count", 1, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(100), ControlDpiScalingHelper.ScaleToCurrentDpiY(100), 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}
#endif
		static protected int GetEnumerableCount(IEnumerable enumerable)
		{
			var i = 0;
			foreach (var e in enumerable)
			{
				i++;
			}

			return i;
		}

		public void TestINotificationTypeWithFormShown()
		{
			form.Show();
			button.Focus();

			AssertEquals("Dummy collection count", 0, Dummy.Collection.Count);
			AssertEquals("Notification state", null, grid.notificationType);
			AssertEquals("Tab page image", -1, tabPage.ImageIndex);
			AssertEquals("Left corner icon", null, grid.LastDrawnCornerIconForTesting);
			BusinessObject child1 = Dummy.Collection.AddNew();
			BusinessObject child2 = Dummy.Collection.AddNew();

			child1.AddRowWarning("warn");
			Application.DoEvents();
			AssertEquals("Notification state", CargoWise.ComponentModel.NotificationType.Warning, grid.notificationType);
			AssertEquals("Tab page image", Icons.GetImageIndex(IconTypes.Warning), tabPage.ImageIndex);
			AssertEquals("Left corner icon", NotificationIconScheme.Instance.GetMiniImage(CargoWise.ComponentModel.NotificationType.Warning), grid.LastDrawnCornerIconForTesting);

			child2.AddRowMessageError("me");
			Application.DoEvents();
			AssertEquals("Notification state", CargoWise.EntityFramework.NotificationType.MessageError, grid.notificationType);
			AssertEquals("Tab page image", Icons.GetImageIndex(IconTypes.MessageError), tabPage.ImageIndex);
			AssertEquals("Left corner icon", NotificationIconScheme.Instance.GetMiniImage(CargoWise.EntityFramework.NotificationType.MessageError), grid.LastDrawnCornerIconForTesting);

			child1.AddRowError("err");
			Application.DoEvents();
			AssertEquals("Notification state", CargoWise.ComponentModel.NotificationType.Error, grid.notificationType);
			AssertEquals("Tab page image", Icons.GetImageIndex(IconTypes.Error), tabPage.ImageIndex);
			AssertEquals("Left corner icon", NotificationIconScheme.Instance.GetMiniImage(CargoWise.ComponentModel.NotificationType.Error), grid.LastDrawnCornerIconForTesting);

			child1.Delete();
			Application.DoEvents();
			AssertEquals("Notification state", CargoWise.EntityFramework.NotificationType.MessageError, grid.notificationType);
			AssertEquals("Tab page image", Icons.GetImageIndex(IconTypes.MessageError), tabPage.ImageIndex);
			AssertEquals("Left corner icon", NotificationIconScheme.Instance.GetMiniImage(CargoWise.EntityFramework.NotificationType.MessageError), grid.LastDrawnCornerIconForTesting);

			BusinessObject child3 = Factory.New<DummyBusinessObject>();
			child3.AddRowError("blah");
			Dummy.Collection.Add(child3);
			Application.DoEvents();
			AssertEquals("Notification state", CargoWise.EntityFramework.NotificationType.Error, grid.notificationType);
			AssertEquals("Tab page image", Icons.GetImageIndex(IconTypes.Error), tabPage.ImageIndex);
			AssertEquals("Left corner icon", NotificationIconScheme.Instance.GetMiniImage(CargoWise.EntityFramework.NotificationType.Error), grid.LastDrawnCornerIconForTesting);

			Dummy.Collection.Remove(child3);
			Dummy.Collection.Remove(child2);
			Application.DoEvents();
			AssertEquals("Notification state", null, grid.notificationType);
			AssertEquals("Tab page image", -1, tabPage.ImageIndex);
			AssertEquals("Left corner icon", null, grid.LastDrawnCornerIconForTesting);
		}

		public void TestINotificationTypeWithErrorsBeforeBinding()
		{
			AssertEquals("Dummy collection count", 0, Dummy.Collection.Count);
			AssertEquals("Notification state", null, grid.notificationType);

			BusinessObject child = Factory.New<DummyBusinessObject>();
			child.AddRowError("blah");
			Dummy.Collection.Add(child);

			form.Show();
			Application.DoEvents();
			AssertEquals("Notification state", CargoWise.EntityFramework.NotificationType.Error, grid.notificationType);
			AssertEquals("Tab page image", Icons.GetImageIndex(IconTypes.Error), tabPage.ImageIndex);
			AssertEquals("Left corner icon", NotificationIconScheme.Instance.GetMiniImage(CargoWise.EntityFramework.NotificationType.Error), grid.LastDrawnCornerIconForTesting);
		}
#if !WINZOR
		public void TestRowHeaderBalloonWithError()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddError("This is an error");
				var dummy2 = Factory.New<DummyBusinessObject>();
				dummy2.AddRowError("another error");
				child.RegisterEditableChildObject(dummy2);
			}
			Application.DoEvents();
			grid.MousePosition = new Point(ControlDpiScalingHelper.ScaleToCurrentDpiX(16) + 3, ControlDpiScalingHelper.ScaleToCurrentDpiY(19) + 3); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are errors on this row.", descriptor.Caption);
			AssertEquals("description", "These errors may be on another grid or fields that shows further information about this row.", descriptor.Description);
			AssertEquals("notifications: errors count", 2, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 0, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: message errors count", 0, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, 100, 100, 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}

		public void TestRowHeaderBalloonWithWarning()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddWarning("This is an Warning");
				var dummy2 = Factory.New<DummyBusinessObject>();
				dummy2.AddRowWarning("another Warning");
				child.RegisterEditableChildObject(dummy2);
			}

			var errorChild = Dummy.Collection.AddNew(); // used to check that we are getting row notification state on balloon, not the grid state
			using (errorChild.SuspendValidationTesting())
			{
				errorChild.Z0_DescriptionInfo.AddError("This is an error");
			}

			Application.DoEvents();
			grid.MousePosition = new Point(ControlDpiScalingHelper.ScaleToCurrentDpiX(16) + 3, ControlDpiScalingHelper.ScaleToCurrentDpiY(19) + 3); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are warnings on this row.", descriptor.Caption);
			AssertEquals("description", "These warnings may be on another grid or fields that shows further information about this row.", descriptor.Description);
			AssertEquals("notifications: errors count", 0, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 2, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: message errors count", 0, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, 100, 100, 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}

		public void TestRowHeaderBalloonWithMessageError()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddMessageError("This is an MessageError");
				var dummy2 = Factory.New<DummyBusinessObject>();
				dummy2.AddRowMessageError("another MessageError");
				child.RegisterEditableChildObject(dummy2);
			}
			Application.DoEvents();
			grid.MousePosition = new Point(ControlDpiScalingHelper.ScaleToCurrentDpiX(16) + 3, ControlDpiScalingHelper.ScaleToCurrentDpiY(19) + 3); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are message errors on this row.", descriptor.Caption);
			AssertEquals("description", "These message errors may be on another grid or fields that shows further information about this row.", descriptor.Description);
			AssertEquals("notifications: errors count", 0, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 0, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: message errors count", 2, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, 100, 100, 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}

		public void TestRowHeaderBalloonPriority()
		{
			form.Show();
			button.Focus();
			var child = Dummy.Collection.AddNew();
			using (child.SuspendValidationTesting())
			{
				child.Z0_DescriptionInfo.AddWarning("This is an warn");
				child.Z0_DescriptionInfo.AddMessageError("This is an me");
				child.Z0_DescriptionInfo.AddError("This is an error");
				var dummy2 = Factory.New<DummyBusinessObject>();
				dummy2.AddRowWarning("another warn");
				child.RegisterEditableChildObject(dummy2);
			}
			Application.DoEvents();
			grid.MousePosition = new Point(ControlDpiScalingHelper.ScaleToCurrentDpiX(16) + 3, ControlDpiScalingHelper.ScaleToCurrentDpiY(19) + 3); // inside notification rect
			grid.OnMouseHoverExposed(EventArgs.Empty);
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Ballon showing", descriptor);
			AssertEquals("anchor control for balloon", grid, descriptor.AnchorControl);
			AssertEquals("anchor rect", grid.NotificationRectForBalloon, descriptor.AnchorRectOnControl);
			AssertEquals("caption", "There are errors on this row.", descriptor.Caption);
			AssertEquals("description", "These errors may be on another grid or fields that shows further information about this row.", descriptor.Description);
			AssertEquals("notifications: errors count", 1, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications: warnings count", 2, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications: message errors count", 1, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.None, 0, 100, 100, 0));
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Ballon not showing", descriptor);
		}

		public void TestDoDragDrop()
		{
			form.Show();
			grid.Focus();
			Dummy.Collection.AddNew();
			Application.DoEvents();

			grid.SetSelectedRows(new int[] { 0 });
			AssertEquals("First row is selected.", 1, grid.SelectedRowCount);

			var row1Rectangle = grid.GetRowNotificationRectangle(0);
			grid.MousePosition = new Point(row1Rectangle.X, row1Rectangle.Y);
			grid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X, row1Rectangle.Y, 0));
			Application.DoEvents();

			grid.OnMouseMoveExposed(new MouseEventArgs(MouseButtons.Left, 1, 100, 100, 0));
			Assert(grid.DoDragDropCalled);
		}

		public void TestOnMouseDownSetMovedFromPointCorrectly()
		{
			form.Show();
			grid.Focus();
			Dummy.Collection.AddNew();
			Application.DoEvents();

			grid.SetSelectedRows(new int[] { 0 });
			AssertEquals("First row is selected.", 1, grid.SelectedRowCount);

			var row1Rectangle = grid.GetRowNotificationRectangle(0);
			grid.MousePosition = new Point(row1Rectangle.X + 20, row1Rectangle.Y);
			grid.IsWholeRowSelectedOnClick = true;
			grid.PerformMouseDownForTest(new MouseEventArgs(MouseButtons.Left, 1, row1Rectangle.X + 20, row1Rectangle.Y, 0));

			var movedFromPoint = grid.GetMovedFromForTest();
			AssertNotEquals("Should not set moved from point to row header's location", grid.RowHeaderClickX, movedFromPoint.X);
			AssertEquals("Should set moved from point to current mouse's location", row1Rectangle.X + 20, movedFromPoint.X);
		}
#endif
		[ToolboxItem(false)]
		public class DummyZGrid : ZGrid
		{
			public bool isResetCachedBindingContextCalled;
			public DummyZGrid()
				: base(true)
			{
			}

			public DummyZGrid(bool importLicenceCheckArg, string[] columnsToExpand = null, string[] rowsToExpand = null)
				: base(importLicenceCheckArg, columnsToExpand, rowsToExpand)
			{
			}

			public bool DoDragDropCalled { get; private set; }

			protected override void DoDragDrop()
			{
				DoDragDropCalled = true;
			}

			public new Point MousePosition;
			protected override Point GetMousePositionInGrid()
			{
				return MousePosition;
			}

			[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is being exposed for a test")]
			public void FireMouseHover()
			{
				OnMouseHover(EventArgs.Empty);
			}

			[SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This is being exposed for a test")]
			public void FireMouseMove()
			{
				OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, MousePosition.X, MousePosition.Y, 0));
			}

			public int ValidateOnPositionChangedIfIncreasesSavePerformanceCallCount;
			protected override void ValidateOnPositionChangedIfIncreasesSavePerformance()
			{
				base.ValidateOnPositionChangedIfIncreasesSavePerformance();
				ValidateOnPositionChangedIfIncreasesSavePerformanceCallCount++;
			}

			[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "This is being exposed for a test")]
			public new bool ProcessCmdKey(ref Message message, Keys keyData)
			{
				return base.ProcessCmdKey(ref message, keyData);
			}

#if !WINZOR
			[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "This is being exposed for a test")]
			public new void WndProc(ref Message m)
			{
				base.WndProc(ref m);
			}
#endif

			public void OnPaintExposed(PaintEventArgs e)
			{
				this.OnPaint(e);
			}

			protected override ZGridCustomise CreateNewGridCustomise()
			{
				ZGridCustomise gridCustomise = null;
				if (NewColumnLayout == null)
				{
					gridCustomise = base.CreateNewGridCustomise();
				}
				else
				{
					var helper = new GridLayoutContextKeyProviderHelper();
					customiseBizObj = new ZGridCustomiseBizObj(helper.GetAllGridIDsForStmModuleFilter(this), helper.GetAllGridIDsForStmData(this), NewColumnLayout, LayoutCategoryPK);
					gridCustomise = new ZGridCustomise(Columns, DefaultColumns, IsGridLayoutConfigurable, customiseBizObj);
				}
				return gridCustomise;
			}
			public IGridLayoutStorage NewColumnLayout;

			public BindingContext BindingContext_Exposed => bindingContext;
			public void SetDataBindingInternalExposed(object dataSource, string dataMember, string tableName)
			{
				this.SetDataBindingInternal(dataSource, dataMember, tableName);
			}

			public override void ResetCachedBindingContext()
			{
				base.ResetCachedBindingContext();
				isResetCachedBindingContextCalled = true;
			}
		}

		protected override Control GetNewControl()
		{
			return new ZGrid();
		}

		#region CopyValueandMoveHotkeyTests
		public void TestCopyAboveValueAndMoveDownHotkey_ValueCopiedAndMovedDown()
		{
			var rowNo = 1;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false);

			AssertEquals("This record should contain the value of the above record", rowNo - 1, Dummy.Collection[rowNo].Z0_Number);
			AssertEquals("Scope into the correct cell, one cell lower", rowNo + 1, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false));
		}

		public void TestCopyAboveValueAndMoveDownHotkey_TopRecord_ValueNotCopiedAndMovedDown()
		{
			var rowNo = 0;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false);

			AssertEquals("This record should have not changed its value", rowNo, Dummy.Collection[rowNo].Z0_Number);
			AssertEquals("Scope into the correct cell, one cell lower", rowNo + 1, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false));
		}

		public void TestCopyAboveValueAndMoveDownHotkey_BottomRecord_ValueCopiedAndCreateNewRecord()
		{
			var rowNo = 3;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false);

			AssertEquals("This record should contain the value of the above record", rowNo - 1, Dummy.Collection[rowNo].Z0_Number);
			AssertEquals("Scope into the correct cell, one cell lower, which is a new cell", rowNo + 1, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false));
		}

		public void TestCopyAboveValueAndMoveDownHotkey_ReadOnly()
		{
			var rowNo = 1;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: true);

			AssertEquals("This record should have not changed its value", rowNo, Dummy.Collection[rowNo].Z0_Number);
			AssertEquals("Scope into the correct cell, one cell lower", rowNo + 1, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F9, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: true));
		}

		public void TestCopyBelowValueAndMoveUpHotkey_ValueCopiedAndMovedUp()
		{
			var rowNo = 1;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false);

			AssertEquals("This record should contain the value of the below record", rowNo + 1, Dummy.Collection[rowNo].Z0_Number);
			AssertEquals("Scope into the correct cell, one cell above", rowNo - 1, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false));
		}

		public void TestCopyBelowValueAndMoveUpHotkey_TopRecord_ValueCopiedAndNotMovedUp()
		{
			var rowNo = 0;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false);

			AssertEquals("This record should contain the value of the below record", rowNo + 1, Dummy.Collection[rowNo].Z0_Number);
			AssertEquals("Scope into the correct cell, unchanged from the top cell", rowNo, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false));
		}

		public void TestCopyBelowValueAndMoveUpHotkey_BottomRecord_ValueNotCopiedAndMovedUp()
		{
			var rowNo = 3;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false);

			AssertEquals("This record should have not changed its value", rowNo, Dummy.Collection[rowNo].Z0_Number);
			AssertEquals("Scope into the correct cell, one cell above", rowNo - 1, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: false));
		}

		public void TestCopyBelowValueAndMoveUpHotkey_ReadOnly()
		{
			var rowNo = 1;
			var colNo = 1;
			ArrangeGridAndActOnHotkey(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: true);

			AssertEquals("This record should have not changed its value", rowNo, Dummy.Collection[1].Z0_Number);
			AssertEquals("Scope into the correct cell, one cell above", rowNo - 1, grid.CurrentCell.RowNumber);

			AssertNoExceptionThrown(() => ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys.Alt | Keys.F8, initialCell: new DataGridCell(rowNo, colNo), isCollectionReadonly: true));
		}

		void ArrangeGridAndActOnHotkey(Keys hotkey, DataGridCell initialCell, bool isCollectionReadonly)
		{
			// Arrange
			// Note RowNumber is the same as the value
			Dummy.Collection.AddNew().Z0_Number = 0;
			Dummy.Collection.AddNew().Z0_Number = 1;
			Dummy.Collection.AddNew().Z0_Number = 2;
			Dummy.Collection.AddNew().Z0_Number = 3;
			if (isCollectionReadonly)
			{
				Dummy.Collection.ForEach(r => r.ReadOnly = true);
			}
			form.Show();

			// Act				
			grid.CurrentCell = initialCell;
			KeySender.SendKeyDownToProcessCmdKey(grid, (int)hotkey);
			Application.DoEvents();
		}

		void ArrangeGridAndActOnHotkey_BindToActiveBusinessObjectCollection(Keys hotkey, DataGridCell initialCell, bool isCollectionReadonly)
		{
			// Arrange
			// Note RowNumber is the same as the value
			var dummy = Factory.NewWithValidTestData<Testing.DummyWithActiveCollection>();
			dummy.AddChildDummy().Z0_Number = 0;
			dummy.AddChildDummy().Z0_Number = 1;
			dummy.AddChildDummy().Z0_Number = 2;
			dummy.AddChildDummy().Z0_Number = 3;
			if (isCollectionReadonly)
			{ dummy.Collection.ForEach(r => r.ReadOnly = true); }
			using (var testForm = new Testing.TestForm(dummy))
			{
				testForm.Show();

				// Act				
				testForm.grid1.CurrentCell = initialCell;
				KeySender.SendKeyDownToProcessCmdKey(testForm.grid1, (int)hotkey);
				Application.DoEvents();
			}
		}
		#endregion

		public void TestSelectCurrentRowHotKey()
		{
			// Arrange
			Dummy.Collection.AddNew().Z0_Number = 0;
			form.Show();
			grid.CurrentCell = new DataGridCell(0, 0);

			// Assert Precondition
			AssertEquals("No row is selected, just a cell", 0, grid.SelectedRowCount);
			AssertEquals("No row is selected, just a cell", false, grid.IsSelected(0));

			// Act
			var hotkey = Keys.Control | Keys.Shift | Keys.L;
			KeySender.SendKeyDownToProcessCmdKey(grid, (int)hotkey);
			Application.DoEvents();

			// Assert Postcondition
			AssertEquals("One row should be selected", 1, grid.SelectedRowCount);
			AssertEquals("The first row should be selected", true, grid.IsSelected(0));
		}

		public void TestSelectCurrentRowHotkeyEmptyGrid()
		{
			var gridWithNoRow = new ZGrid();
			gridWithNoRow.ReadOnly = true;

			form.Controls.Add(gridWithNoRow);

			// Assert Precondition
			AssertEquals("DataGrid does not contains any row.", 0, gridWithNoRow.DataGridRowsLength);

			var hotkey = Keys.Control | Keys.Shift | Keys.L;
			KeySender.SendKeyDownToProcessCmdKey(gridWithNoRow, (int)hotkey);

			AssertNoExceptionThrown("IndexOutOfRangeException not thrown.", () => Application.DoEvents());
		}

		protected override void SetUp()
		{
			base.SetUp();
			form = new ZForm(Dummy);
			grid = new DummyZGrid { Dock = DockStyle.Fill };

			var info1 = new ZTextBoxColumnStyleInfo();
			var info2 = new ZCalcEditColumnStyleInfo();
			var info3 = new ZTextBoxColumnStyleInfo();
			info1.ColumnName = DummyBizoSchema.Constants.Z0_Description;
			info2.ColumnName = DummyBizoSchema.Constants.Z0_Number;
			info3.ColumnName = "Z0_DescriptionValidationCount";
			grid.ColumnStyles.Add(info1);
			grid.ColumnStyles.Add(info2);
			grid.ColumnStyles.Add(info3);

			tabPage = new ZTabPage();
			tabControl = new ZTabControl { Dock = DockStyle.Fill };
			grid.BindTo = "Collection";
			grid.GridId = Guid.NewGuid().ToString();
			form.Controls.Add(tabControl);
			tabControl.TabPages.Add(tabPage);
			tabPage.Controls.Add(grid);
			button = new ZButton();
			tabPage.Controls.Add(button);
			button.Focus();
			validationTestingSuspend = Dummy.SuspendValidationTesting();
		}
		DummyZGrid grid;
		ZForm form;
		ZTabControl tabControl;
		ZTabPage tabPage;
		ZButton button;
		IDisposable validationTestingSuspend;

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
			validationTestingSuspend.Dispose();
			Balloon.Instance.Hide();
		}
	}
}
