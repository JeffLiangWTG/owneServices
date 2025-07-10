using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture
{
	class CopyPasteRowTest : TestCaseWithDummy
	{
		[DeveloperOnlyTest]
		public void TestCanCopyPasteDataFromCell()
		{
			var bizobj = Factory.New<DummyBusinessObject>();
			bizobj.Collection.AddNew();

			using (var form = new ZTestForm(bizobj))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				SafeClipboard.Clear();

				var box = form.ActiveControl as DataGridTextBox;
				AssertNotNull("Should be DataGridTextBox", box);

				box.Select(2, 3);
				Application.DoEvents();

				UnsafeNativeMethods.SendMessage(new HandleRef(form.ActiveControl, form.ActiveControl.Handle), WindowsMessage.WM_COPY, 0, 0);
				Application.DoEvents();

				AssertEquals("Should have contents of DataGridTextBox", "fau", SafeClipboard.GetText());
			}
		}

		public void TestCanCopyPasteDataFromCellOnFormsWithoutEditMenuItems()
		{
			var bizobj = Factory.New<DummyBusinessObject>();
			bizobj.Collection.AddNew();

			using (var form = new ZTestFormWithoutEditMenuItems(bizobj))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				SafeClipboard.Clear();

				var box = form.ActiveControl as DataGridTextBox;
				AssertNotNull("Should be DataGridTextBox", box);

				box.Select(2, 3);
				Application.DoEvents();

				KeySender.PostKeyDown(box, box.Handle, Keys.Control | Keys.C);
				Application.DoEvents();

				Assert("Key stroke is not marked as handled in ProcessDialogKey().", !form.Grid.processDialogKeyResult);
			}
		}

		public void TestEditCellListIsNull()
		{
			var bizobj = Factory.New<DummyBusinessObject>();
			bizobj.Collection.AddNew();

			using (var form = new ZTestFormWithoutEditMenuItems(bizobj))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				KeySender.PostKeyDown(form, form.Handle, Keys.Tab);
				Application.DoEvents();

				SafeClipboard.Clear();

				var box = form.ActiveControl as DataGridTextBox;
				box.Select(2, 3);
				Application.DoEvents();

				KeySender.PostKeyDown(box, box.Handle, Keys.Control | Keys.C);
				form.Grid.IsListManagerNotNull = false;
				AssertNoExceptionThrown(() => Application.DoEvents());
			}
		}

		// DeveloperOnlyTests do not run on DAT!

		[DeveloperOnlyTest]
		public void TestEnsureCellIsNotSelectedWhenRowHeaderIsClicked()
		{
			var bizobj = Factory.New<DummyBusinessObject>();
			bizobj.Collection.AddNew();

			using (var form = new ZTestForm(bizobj))
			{
				form.Show();
				Application.DoEvents();

				var coordinatesOfTheFirstRow = //1966101; // 0x001E0015 // y coordinate 0x1E is too close to row-resize area, changing it to 0x18 == 24.
					ControlDpiScalingHelper.ScaleToCurrentDpiY(24) << 16 |
					ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
				MouseSender.SendMessage(form.Grid, form.Grid.Handle, WindowsMessage.WM_LBUTTONDOWN, 0, coordinatesOfTheFirstRow);
				MouseSender.SendMessage(form.Grid, form.Grid.Handle, WindowsMessage.WM_LBUTTONUP, 1, coordinatesOfTheFirstRow); // Sometimes on first click grid is focused only on mouse up event.
				Application.DoEvents();

				AssertSame("Should be Grid", form.Grid, form.ActiveControl);

				SafeClipboard.Clear();
				UnsafeNativeMethods.SendMessage(new HandleRef(form.Grid, form.Grid.Handle), WindowsMessage.WM_COPY, 0, 0);
				Application.DoEvents();

				AssertEquals("Should contain a string value of the selected row", "Default	0" + System.Environment.NewLine, SafeClipboard.GetText());
			}
		}

#if !WINZOR
		[DeveloperOnlyTest]
		public void TestRowCopy_WM_COPY_Message_FromWndProc()
		{
			using (TestForm)
			{
				var grid = ((ZGuidFindBoxColumnStyleTestForm)TestForm).zGrid1;

				superDummy.RegisterEditableChildObject(superDummy.Collection);
				superDummy.SetReadOnlyIncludingChildren(true);

				TestForm.Show();

				grid.Select(0);
				grid.CurrentRowIndex = 0;

				var cutMessage = new Message { Msg = WindowsMessage.WM_CUT };
				grid.WndProcExposed(ref cutMessage);
				AssertEquals(string.Empty, SafeClipboard.GetText());
				SafeClipboard.Clear();

				grid.Select(0);
				grid.CurrentRowIndex = 0;

				var copyMessage = new Message { Msg = WindowsMessage.WM_COPY };
				grid.WndProcExposed(ref copyMessage);
				AssertEquals("copy value was incorrect", "CH1	NCODE" + System.Environment.NewLine, SafeClipboard.GetText());

				SafeClipboard.Clear();
				grid.CopySelectedRowsAllowed = false;
				grid.WndProcExposed(ref copyMessage);
				AssertEquals("should be empty", "", SafeClipboard.GetText());
			}
		}
#endif

		[DeveloperOnlyTest]
		public void TestRowCopy()
		{
			using (TestForm)
			{
				var grid = ((ZGuidFindBoxColumnStyleTestForm)TestForm).zGrid1;

				superDummy.RegisterEditableChildObject(superDummy.Collection);
				superDummy.SetReadOnlyIncludingChildren(true);

				TestForm.Show();

				grid.Select(0);
				grid.CurrentRowIndex = 0;

				grid.ProcessDialogKeyExposed(Keys.Control | Keys.Alt | Keys.C);
				Application.DoEvents();
				AssertEquals(string.Empty, SafeClipboard.GetText());
				SafeClipboard.Clear();

				grid.ProcessDialogKeyExposed(Keys.C);
				Application.DoEvents();
				AssertEquals(string.Empty, SafeClipboard.GetText());
				SafeClipboard.Clear();

				grid.ProcessDialogKeyExposed(Keys.B);
				Application.DoEvents();
				AssertEquals(string.Empty, SafeClipboard.GetText());
				SafeClipboard.Clear();

				grid.UnSelect(0);
				grid.ProcessDialogKeyExposed(Keys.Control | Keys.C);
				Application.DoEvents();
				AssertEquals(string.Empty, SafeClipboard.GetText());
				SafeClipboard.Clear();

				grid.Select(0);
				grid.CurrentRowIndex = 0;

				var action = new Action(() =>
				{
					grid.ProcessDialogKeyExposed(Keys.Control | Keys.C);
					Application.DoEvents();
				});
				action.Invoke();
				var text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertEquals("copy value was incorrect", "CH1	NCODE" + System.Environment.NewLine, text);

				SafeClipboard.Clear();
				grid.CopySelectedRowsAllowed = false;
				grid.ProcessDialogKeyExposed(Keys.Control | Keys.C);
				Application.DoEvents();
				AssertEquals("should be empty CopySelectedRowsAllowedis false", "", SafeClipboard.GetText());
			}
		}

		[DeveloperOnlyTest]
		public void TestMultipleRowCopy()
		{
			using (TestForm)
			{
				var grid = ((ZGuidFindBoxColumnStyleTestForm)TestForm).zGrid1;

				superDummy.RegisterEditableChildObject(superDummy.Collection);
				superDummy.SetReadOnlyIncludingChildren(true);

				TestForm.Show();

				grid.SelectAllElements();
				grid.CurrentRowIndex = 0;
				Application.DoEvents();
				grid.ProcessDialogKeyExposed(Keys.Control | Keys.C);
				Application.DoEvents();

				var expectedResult = "CH1	NCODE" + System.Environment.NewLine
					+ "CH2	NCODE" + System.Environment.NewLine
					+ "CH3	NCODE" + System.Environment.NewLine;

				AssertEquals("copy value was incorrect", expectedResult, SafeClipboard.GetText());
			}
		}

		[DeveloperOnlyTest]
		public void TestRowCopyWithCtrlShiftCPressed()
		{
			using (TestForm)
			{
				var grid = ((ZGuidFindBoxColumnStyleTestForm)TestForm).zGrid1;
				superDummy.RegisterEditableChildObject(superDummy.Collection);
				superDummy.SetReadOnlyIncludingChildren(true);
				TestForm.Show();
				grid.Select(0);
				grid.CurrentRowIndex = 0;

				var action = new Action(() => grid.ProcessDialogKeyExposed(Keys.Control | Keys.Shift | Keys.C));
				action.Invoke();

				var text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertEquals("copy value was incorrect", "CH1	NCODE" + System.Environment.NewLine, text);
				SafeClipboard.Clear();
			}
		}

		[DeveloperOnlyTest]
		public void TestCtrlCShouldNotCopySensitiveColumns()
		{
			using (TestForm)
			{
				foreach (DummyBusinessObject bizObj in superDummy.Collection)
				{
					bizObj.Z0_Description = "this is a password!";
				}
				var grid = ((ZGuidFindBoxColumnStyleTestForm)TestForm).zGrid1;

				var sensitiveColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				sensitiveColumnStyleInfo.Caption = "Sensitive Column";
				sensitiveColumnStyleInfo.ColumnName = "Z0_Description";
				sensitiveColumnStyleInfo.PasswordChar = '*';
				grid.Columns.Add(sensitiveColumnStyleInfo);
				superDummy.RegisterEditableChildObject(superDummy.Collection);
				superDummy.SetReadOnlyIncludingChildren(true);
				TestForm.Show();
				grid.Select(0);
				grid.CurrentRowIndex = 0;
				grid.ProcessDialogKeyExposed(Keys.Control | Keys.Shift | Keys.C);

				var action = new Action(() => grid.ProcessDialogKeyExposed(Keys.Control | Keys.Shift | Keys.C));
				action.Invoke();

				var text = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertNotContains("this is a password!", text);
				AssertContains("***", text);
				SafeClipboard.Clear();
			}
		}

		public void TestGetSelectedRowsAsTextShouldNotIncludeSensitiveColumns()
		{
			using (TestForm)
			{
				foreach (DummyBusinessObject bizObj in superDummy.Collection)
				{
					bizObj.Z0_Description = "this is a password!";
				}
				var grid = ((ZGuidFindBoxColumnStyleTestForm)TestForm).zGrid1;

				var sensitiveColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				sensitiveColumnStyleInfo.Caption = "Sensitive Column";
				sensitiveColumnStyleInfo.ColumnName = "Z0_Description";
				sensitiveColumnStyleInfo.PasswordChar = '*';
				grid.Columns.Add(sensitiveColumnStyleInfo);
				superDummy.RegisterEditableChildObject(superDummy.Collection);
				superDummy.SetReadOnlyIncludingChildren(true);
				TestForm.Show();
				grid.Select(0);
				grid.CurrentRowIndex = 0;

				var text = grid.GetSelectedRowsAsText(grid.SelectedElements, false);
				AssertNotContains("this is a password!", text);
				AssertContains("***", text);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			SafeClipboard.Clear();

			superDummy = Factory.New<SuperDummyBusinessObject>();
			child1 = Factory.New<DummyChildBusinessObject>();
			child1.Z0_Code = "CH1";
			child2 = Factory.New<DummyChildBusinessObject>();
			child2.Z0_Code = "CH2";
			child3 = Factory.New<DummyChildBusinessObject>();
			child3.Z0_Code = "CH3";

			var dummy = superDummy.Collection.AddNew();
			dummy.Z0_Guid = child1.PK;
			var dummy2 = superDummy.Collection.AddNew();
			dummy2.Z0_Guid = child2.PK;
			var dummy3 = superDummy.Collection.AddNew();
			dummy3.Z0_Guid = child3.PK;

			ZGuidFindBoxColumnStyleTestForm.AddGuidColumnFirst = true;
			try
			{
				TestForm = new ZGuidFindBoxColumnStyleTestForm(superDummy);
			}
			finally
			{
				ZGuidFindBoxColumnStyleTestForm.AddGuidColumnFirst = false;
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (TestForm != null)
			{
				TestForm.Dispose();
			}
		}

		ZForm TestForm;
		SuperDummyBusinessObject superDummy;
		DummyChildBusinessObject child1;
		DummyChildBusinessObject child2;
		DummyChildBusinessObject child3;

		#endregion
	}
}
