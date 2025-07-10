using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTextBoxTest : ZControlBaseTestCase<ZTextBox>
	{
		#region Focus

		#if !WINZOR // equivalent test is Enterprise.Winzor.Architecture.Test.ZTextBoxTest.TestFocusedOnMouseClick
		public void TestFocusedOnMouseClick()
		{
			using (var form = new ZForm())
			{
				var textBox1 = new ZTextBox();
				form.Controls.Add(textBox1);

				var textBox2 = new ZTextBox();
				form.Controls.Add(textBox2);
				form.Show();

				textBox2.Focus();
				Assert(!textBox1.Focused);

				MouseSender.SendMessage(textBox1, textBox1.Handle, WindowsMessage.WM_LBUTTONDOWN, 0, 0);
				Assert(textBox1.Focused);

				textBox2.Focus();
				Assert(!textBox1.Focused);

				MouseSender.SendMessage(textBox1, textBox1.Handle, WindowsMessage.WM_RBUTTONDOWN, 0, 0);
				Assert(textBox1.Focused);
			}
		}
		#endif

		#endregion

		#region Override tooltip message for password ZTextBox

		public void TestCustomizedBalloonMessageShownForPasswordTextBox()
		{
			using (var form = new ZForm())
			using (var textBox = new ZTextBox())
			{
				ZTextBox.EnableMockCapsLockIsOnForTesting.Value = true;
				textBox.PasswordChar = '*';
				form.Controls.Add(textBox);
				form.Show();

				textBox.Focus();
				if (!ZTextBox.IsCapsLockOn())
				{
					AssertNoToolTipShown(textBox);
					ZTextBox.MockCapsLockIsOn.Value = true;
					typeof(Control).InvokeMember("OnKeyUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox, new object[] { new KeyEventArgs(Keys.CapsLock) });
					Application.DoEvents();
				}
				AssertCapsLockToolTipShown(textBox);

				KeySender.SendKeyPress(textBox, textBox.Handle, Keys.A);
				Application.DoEvents();
				AssertNoToolTipShown(textBox);
				ZTextBox.MockCapsLockIsOn.Value = false;

				typeof(Control).InvokeMember("OnKeyUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox, new object[] { new KeyEventArgs(Keys.Control | Keys.C) });
				Application.DoEvents();
				AssertCannotCopyToolTipShown(textBox);

				KeySender.SendKeyPress(textBox, textBox.Handle, Keys.A);
				Application.DoEvents();
				AssertNoToolTipShown(textBox);

				ZTextBox.MockCapsLockIsOn.Value = true;
				typeof(Control).InvokeMember("OnKeyUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox, new object[] { new KeyEventArgs(Keys.CapsLock) });
				Application.DoEvents();
				typeof(Control).InvokeMember("OnKeyUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox, new object[] { new KeyEventArgs(Keys.Control | Keys.C) });
				Application.DoEvents();
				AssertCannotCopyToolTipShown(textBox);
			}
		}

		public void TestCustomizedCapsLockIsOnToolTipAppearOnce()
		{
			using (var form = new ZForm())
			using (var textBox1 = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			{
				ZTextBox.EnableMockCapsLockIsOnForTesting.Value = true;
				textBox1.PasswordChar = '*';
				textBox2.PasswordChar = '*';
				form.Controls.Add(textBox1);
				form.Controls.Add(textBox2);
				form.Show();

				textBox1.Focus();
				if (!ZTextBox.IsCapsLockOn())
				{
					AssertNoToolTipShown(textBox1);
					ZTextBox.MockCapsLockIsOn.Value = true;
					typeof(Control).InvokeMember("OnKeyUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox1, new object[] { new KeyEventArgs(Keys.CapsLock) });
					Application.DoEvents();
				}
				AssertCapsLockToolTipShown(textBox1);

				textBox2.Focus();
				Application.DoEvents();
				AssertNoToolTipShown(textBox1);
				AssertCapsLockToolTipShown(textBox2);
			}
		}

		[UseSnapshotProtection]
		public void TestTextTemplateIsDisabledWhenPasswordCharIsSet()
		{
			using (var form = new ZForm())
			using (var textBox = new ZTextBox())
			{
				form.Controls.Add(textBox);
				form.Show();

				textBox.contextMenuManager.InitializeContextMenu();
				var template = textBox.contextMenuManager.TextTemplatesFactory.New();
				template.S8_Description = "Wibble";
				template.Factory.Save();
				textBox.ContextMenuStrip.Show();

				AssertEquals("PasswordChar Is Not Set, TextTemplate Is Enable", true, textBox.contextMenuManager.GetTextTemplateEnabled_ForTest());
			}

			using (var form = new ZForm())
			using (var textBox = new ZTextBox())
			{
				textBox.PasswordChar = '*';
				form.Controls.Add(textBox);
				form.Show();

				textBox.contextMenuManager.InitializeContextMenu();
				var template = textBox.contextMenuManager.TextTemplatesFactory.New();
				template.S8_Description = "Wibble";
				template.Factory.Save();
				textBox.ContextMenuStrip.Show();

				AssertEquals("PasswordChar Is Set, TextTemplate Is Disable", false, textBox.contextMenuManager.GetTextTemplateEnabled_ForTest());
			}
		}

		void AssertNoToolTipShown(ZTextBox textBox)
		{
			Assert("Caps Lock Is On is not shown", string.IsNullOrEmpty(textBox.capsLockOnToolTip?.GetToolTip(textBox)));
			Assert("Cannot Copy is not shown", string.IsNullOrEmpty(textBox.cannotCopyPasswordToolTip?.GetToolTip(textBox)));
		}
		void AssertCapsLockToolTipShown(ZTextBox textBox)
		{
			Assert("Caps Lock Is On is shown", !string.IsNullOrEmpty(textBox.capsLockOnToolTip.GetToolTip(textBox)));
			Assert("Cannot Copy is not shown", string.IsNullOrEmpty(textBox.cannotCopyPasswordToolTip?.GetToolTip(textBox)));
		}
		void AssertCannotCopyToolTipShown(ZTextBox textBox)
		{
			Assert("Caps Lock Is On is not shown", string.IsNullOrEmpty(textBox.capsLockOnToolTip?.GetToolTip(textBox)));
			Assert("Cannot Copy is shown", !string.IsNullOrEmpty(textBox.cannotCopyPasswordToolTip.GetToolTip(textBox)));
		}

		#endregion

		#region Popup when Multiline

		public void TestAllowPopupWhenMultiLine()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				form.Show();

				AssertNull(ZFormModaliser.ActiveForm);
				try
				{
					Assert("Multiline default", !textBox.Multiline);
					Assert("AllowPopupWhenMultiline default", textBox.AllowPopupWhenMultiLine);

					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)Keys.F3);
					Application.DoEvents();
					AssertNull(ZFormModaliser.ActiveForm);

					textBox.Multiline = true;
					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)Keys.F3);
					Application.DoEvents();
					AssertNotNull(ZFormModaliser.ActiveForm);

					((ZForm)ZFormModaliser.ActiveForm).Close();
					AssertNull(ZFormModaliser.ActiveForm);

					textBox.ReadOnly = true;
					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)Keys.F3);
					Application.DoEvents();
					AssertNull(ZFormModaliser.ActiveForm);

					textBox.ReadOnly = false;
					textBox.AllowPopupWhenMultiLine = false;
					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)Keys.F3);
					Application.DoEvents();
					AssertNull(ZFormModaliser.ActiveForm);
				}
				finally
				{
					var disposable = ZFormModaliser.ActiveForm as IDisposable;
					disposable?.Dispose();
				}
			}
		}

		#endregion

		#region ReadOnly color when first showing form

#if !WINZOR

		// Note that this test didn't actually pick up the problem.
		public void TestReadOnlyWhenFirstOpeningForm()
		{
			var bizo = new BusinessEntityWithReadOnlyString { Text_ReadOnly = true };
			using (var form = new FormWithReadOnlyTextBox(bizo))
			{
				form.Size = new Size(200, 200);
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.TextBox.ReadOnly);

				var textBoxBitmap = new Bitmap(200, 200);
				form.TextBox.DrawToBitmap(textBoxBitmap, new Rectangle(new Point(0, 0), form.TextBox.Size));

				var firstLineColor = textBoxBitmap.GetPixel(100, 5);
				var lastLineColor = textBoxBitmap.GetPixel(100, form.TextBox.Height - 5);
				AssertEquals("First line of text read only in color", SystemColors.Control.ToArgb(), firstLineColor.ToArgb());
				AssertEquals("Last line of text read only in color", SystemColors.Control.ToArgb(), lastLineColor.ToArgb());
			}
		}

#endif

		public void TestEmailAddress_EmailAddressNotSet()
		{
			var bizo = new BusinessEntityWithReadOnlyEmailAddressString { Text_ReadOnly = true };
			using (var form = new FormWithReadOnlyTextBox(bizo))
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.TextBox.ReadOnly);
				AssertEquals("", form.TextBox.Text);

				form.TextBox.Hotkeys.ProcessCmdKey(form.TextBox, Keys.Control | Keys.E);
				AssertEquals("", form.TextBox.Text);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				bizo.Text_ReadOnly = false;
				AssertEquals(false, form.TextBox.ReadOnly);

				form.TextBox.Hotkeys.ProcessCmdKey(form.TextBox, Keys.Control | Keys.E);
				AssertEquals("", form.TextBox.Text);
				AssertEquals("Your email address has not been set", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		[UseSnapshotProtection]
		public void TestEmailAddress_EmailAddressField()
		{
			var bizo = new BusinessEntityWithReadOnlyEmailAddressString { Text_ReadOnly = false };
			SetCurrentUsersEmail("A@B.COM");

			using (var form = new FormWithReadOnlyTextBox(bizo))
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Show();
				Application.DoEvents();
				AssertEquals(false, form.TextBox.ReadOnly);
				AssertEquals("", form.TextBox.Text);

				form.TextBox.Hotkeys.ProcessCmdKey(form.TextBox, Keys.Control | Keys.E);
				AssertEquals("A@B.COM", form.TextBox.Text);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		public void TestEmailAddress_NonEmailAddressField()
		{
			var bizo = new BusinessEntityWithReadOnlyString { Text_ReadOnly = false };
			SetCurrentUsersEmail("A@B.COM");

			using (var form = new FormWithReadOnlyTextBox(bizo))
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(200, 200);
				form.Show();
				Application.DoEvents();
				AssertEquals(false, form.TextBox.ReadOnly);
				AssertEquals("", form.TextBox.Text);

				form.TextBox.Hotkeys.ProcessCmdKey(form.TextBox, Keys.Control | Keys.E);
				AssertEquals("", form.TextBox.Text);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		void SetCurrentUsersEmail(string email)
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff["GS_EmailAddress"] = email;
			factory.Save();

			Env.SetUserContext(new UserContext((IUser)staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}

		class FormWithReadOnlyTextBox : ZForm
		{
			public FormWithReadOnlyTextBox(BusinessObject businessEntity)
				: base(businessEntity)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Controls.Add(TextBox);
			}

			public ZTextBox TextBox => textBox ?? (textBox = new ZTextBox
			{
				Multiline = true,
				Dock = DockStyle.Fill,
				BindTo = "Text"
			});

			ZTextBox textBox;
		}

		class BusinessEntityWithReadOnlyString : NonPersistentBusinessObject
		{
			[ReadOnlyMember(nameof(Text_ReadOnly))]
			public ZString Text
			{
				get => text;
				set
				{
					text = value;
					TextInfo.RefreshBinding();
				}
			}
			ZString text;

			public ZPropertyInfo TextInfo => GetZPropertyInfo(nameof(Text));

			public bool Text_ReadOnly
			{
				get => text_ReadOnly;
				set
				{
					text_ReadOnly = value;
					TextInfo.RefreshBinding();
				}
			}
			bool text_ReadOnly;

			public ZPropertyInfo Text_ReadOnlyInfo => GetZPropertyInfo(nameof(Text_ReadOnly));
		}

		class BusinessEntityWithReadOnlyEmailAddressString : NonPersistentBusinessObject
		{
			[ReadOnlyMember(nameof(Text_ReadOnly))]
			[EmailAddress]
			public ZString Text
			{
				get => text;
				set
				{
					text = value;
					TextInfo.RefreshBinding();
				}
			}
			ZString text;

			public ZPropertyInfo TextInfo => GetZPropertyInfo(nameof(Text));

			public bool Text_ReadOnly
			{
				get => text_ReadOnly;
				set
				{
					text_ReadOnly = value;
					TextInfo.RefreshBinding();
				}
			}
			bool text_ReadOnly;

			public ZPropertyInfo Text_ReadOnlyInfo => GetZPropertyInfo(nameof(Text_ReadOnly));
		}

		#endregion

		#region TestMultilineAndCaptionWrap

		public void TestMultilineAndCaptionWrap()
		{
			using (var textBox = new ZTextBox())
			{
				var labelCaptionRenderer = textBox.Extensions.Get<ILabelCaptionRenderer>();
				AssertNotNull(labelCaptionRenderer);
				Assert("Precondition", !textBox.Multiline);
				AssertEquals("Precondition", LabelCaptionAlignment.Default, labelCaptionRenderer.Alignment);
				AssertEquals("Precondition", StringRenderingOptions.Truncate, labelCaptionRenderer.Options);

				textBox.Multiline = true;
				AssertEquals(StringRenderingOptions.Wrap, labelCaptionRenderer.Options);

				labelCaptionRenderer.Alignment = LabelCaptionAlignment.Top;
				AssertEquals(StringRenderingOptions.Wrap, labelCaptionRenderer.Options);

				textBox.Multiline = false;
				AssertEquals(StringRenderingOptions.Wrap, labelCaptionRenderer.Options);

				labelCaptionRenderer.Alignment = LabelCaptionAlignment.Left;
				AssertEquals(StringRenderingOptions.Wrap, labelCaptionRenderer.Options);

				textBox.Multiline = false;
				AssertEquals(StringRenderingOptions.Truncate, labelCaptionRenderer.Options);

				labelCaptionRenderer.Alignment = LabelCaptionAlignment.Top;
				AssertEquals(StringRenderingOptions.Truncate, labelCaptionRenderer.Options);

				textBox.Multiline = true;
				AssertEquals(StringRenderingOptions.Truncate, labelCaptionRenderer.Options);

				labelCaptionRenderer.Alignment = LabelCaptionAlignment.Left;
				AssertEquals(StringRenderingOptions.Truncate, labelCaptionRenderer.Options);

				textBox.Multiline = true;
				AssertEquals(StringRenderingOptions.Wrap, labelCaptionRenderer.Options);
			}
		}

		#endregion

		#region TestResetSelectionStartOnLeave

		public void TestResetSelectionStartOnLeave()
		{
			using (var form = new ZForm())
			using (var textBox1 = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			{
				textBox1.Location = new Point(10, 10);
				textBox1.Location = new Point(10, 40);

				form.Controls.Add(textBox1);
				form.Controls.Add(textBox2);

				form.Show();
				Application.DoEvents();

				textBox1.Text = "aaa aaa aaa";
				textBox2.Text = "bbb bbb bbb";

				textBox1.Focus();
				textBox1.SelectionStart = 5;
				textBox1.SelectionLength = 0;
				textBox1.HideSelection = true;

				AssertEquals(5, textBox1.SelectionStart);

				textBox2.Focus();

				AssertEquals("Should reset SelectionStart", 0, textBox1.SelectionStart);

				textBox1.Focus();
				textBox1.SelectionStart = 5;
				textBox1.SelectionLength = 3;

				AssertEquals(5, textBox1.SelectionStart);

				textBox2.Focus();

				AssertEquals("Should reset SelectionStart", 0, textBox1.SelectionStart);
				AssertEquals("Should reset SelectionLength", 0, textBox1.SelectionLength);

				textBox1.HideSelection = false;
				textBox1.Focus();
				textBox1.SelectionStart = 5;
				textBox1.SelectionLength = 0;

				AssertEquals(5, textBox1.SelectionStart);

				textBox2.Focus();

				AssertEquals("Should reset SelectionStart", 0, textBox1.SelectionStart);

				textBox1.Focus();
				textBox1.SelectionStart = 5;
				textBox1.SelectionLength = 3;

				AssertEquals(5, textBox1.SelectionStart);

				textBox2.Focus();

				AssertEquals("Should not reset SelectionLength", 3, textBox1.SelectionLength);
				AssertEquals("Should not reset SelectionStart", 5, textBox1.SelectionStart);
			}
		}

		#endregion

		#region Dynamic Multiline

		public void TestShowDynamicMultilineTextBoxFormWhenF3Pressed()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox { IsDynamicMultiline = true };
				form.Controls.Add(textBox);
				form.Show();

				Assert(!textBox.IsDynamicMultilineTextBoxFormVisible);
				textBox.Focus();
				KeySender.SendKeyDownToProcessCmdKey(textBox, (int)Keys.F3);
				Application.DoEvents();
				Assert(textBox.IsDynamicMultilineTextBoxFormVisible);
			}
		}

#if !WINZOR

		public void TestShowDynamicMultiline_ReadOnly()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox
				{
					IsDynamicMultiline = true,
					ReadOnly = true
				};
				form.Controls.Add(textBox);
				form.Show();

				AssertEquals("GIVEN multiline-textbox is readonly", true, textBox.ReadOnly);
				AssertEquals("GIVEN multiline-textbox-form is not visible", false, textBox.IsDynamicMultilineTextBoxFormVisible);

				textBox.Focus();
				KeySender.SendKeyDownToProcessCmdKey(textBox, (int)Keys.F3);
				Application.DoEvents();

				AssertEquals("WHEN Pressing F3, THEN multiline-textbox-form should visible", true, textBox.IsDynamicMultilineTextBoxFormVisible);
				AssertEquals("THEN multiline-textbox-form should not read-only", false, textBox.TestDynamicMultilineTextBoxForm.GetReadOnly());
				AssertEquals("THEN multiline-textbox-form textbox should read-only", true, textBox.TestDynamicMultilineTextBoxForm.MultilineTextBox.ReadOnly);
			}
		}

#endif

		#endregion

		#region TestResetPosition

		public void TestResetPosition()
		{
			using (var testForm = new ZForm())
			using (var textBox1 = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			{
				textBox1.Location = new Point(10, 10);
				textBox1.ResetPosition = true;
				testForm.Controls.Add(textBox1);

				textBox2.Location = new Point(10, 40);
				textBox2.ResetPosition = false;
				testForm.Controls.Add(textBox2);

				textBox1.Text = textBox2.Text = "ABCDEFGH";

				testForm.Show();
				Application.DoEvents();

				textBox1.Focus();

				textBox1.SelectionStart = 4;
				textBox1.SelectionLength = 0;
				AssertEquals(4, textBox1.SelectionStart);
				textBox2.Focus();
				AssertEquals("Should reset position", 0, textBox1.SelectionStart);

				textBox2.SelectionStart = 4;
				textBox2.SelectionLength = 0;
				AssertEquals(4, textBox2.SelectionStart);
				textBox1.Focus();
				AssertEquals("Should keep position", 4, textBox2.SelectionStart);
			}
		}

#if !WINZOR
		[DeveloperOnlyTest]
		public void TestTheLastKoreanCharacterKeepInTheCorrectPosition()
		{
			using (var testForm = new ZForm())
			using (var textBox1 = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			{
				textBox1.Location = new Point(10, 10);
				textBox1.ResetPosition = true;
				testForm.Controls.Add(textBox1);

				textBox2.Location = new Point(10, 40);
				testForm.Controls.Add(textBox2);

				textBox1.Text = textBox2.Text = "부산기업";

				testForm.Show();
				Application.DoEvents();

				InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("ko-KR"));

				textBox1.Focus();

				textBox1.SelectionStart = 4;
				textBox1.SelectionLength = 0;
				AssertEquals(4, textBox1.SelectionStart);
				textBox2.Focus();

				AssertEquals("Should keep position", 4, textBox1.SelectionStart);
			}
		}
#endif

#endregion

		#region IDataBoundControl

		public void TestBindAndUnbind()
		{
			using (var form = new ZForm())
			{
				TestControl.Parent = form;
				form.Show();

				const string testEntry = "test entry";
				const string tableName = "TestTable";
				var table = new DataTable(tableName);
				table.Columns.Add("Test");
				table.Rows.Add(testEntry);

				TestControl.BindTo = "Test";
				form.DataSourceType = typeof(object);
				form.SetDataBinding(table, "");
				var row = table.NewRow();
				row[0] = testEntry;
				table.AcceptChanges();

				AssertEquals(testEntry.ToUpper(), TestControl.Text);

				form.SetDataBinding(null, "");
				row[0] = "some random value";
				table.AcceptChanges();

				AssertEquals(testEntry.ToUpper(), TestControl.Text);
			}
		}

		public void TestDataSourceType()
		{
			using (var textBox = new ZTextBox())
			{
				AssertEquals(typeof(string), ((IDataBoundControl)textBox).DataSourceType);
			}
		}

		#endregion

		#region TestReadOnly
#if !WINZOR
		public void TestWindowsMessageChangeReadOnlyProperty()
		{
			using (var form = new ZForm())
			{
				TestControl.Parent = form;
				form.Show();

				const string testEntry = "AAAA";
				const string tableName = "TestTable";
				var table = new DataTable(tableName);
				table.Columns.Add("Test");
				table.Rows.Add(testEntry);

				TestControl.BindTo = "Test";
				form.DataSourceType = typeof(object);
				form.SetDataBinding(table, "");
				var row = table.NewRow();
				row[0] = testEntry;
				table.AcceptChanges();

				AssertEquals("AAAA", TestControl.Text);
				AssertEquals("AAAA", table.Rows[0][0]);
				
				var strPtr = Marshal.StringToCoTaskMemUni("BBBB");
				using (new DisposableAction(() => Marshal.FreeHGlobal(strPtr)))
				{
					UnsafeNativeMethods.SendMessage(new HandleRef(form, TestControl.Handle), 0x000c, IntPtr.Zero, strPtr);
				}
				TestControl.PerformControlValidation();
				AssertEquals("BBBB", TestControl.Text);
				AssertEquals("BBBB", table.Rows[0][0]);

				TestControl.ReadOnly = true;
				strPtr = Marshal.StringToCoTaskMemUni("CCCC");
				using (new DisposableAction(() => Marshal.FreeHGlobal(strPtr)))
				{
					UnsafeNativeMethods.SendMessage(new HandleRef(form, TestControl.Handle), 0x000c, IntPtr.Zero, strPtr);
				}
				TestControl.PerformControlValidation();
				AssertEquals("BBBB", TestControl.Text);
				AssertEquals("BBBB", table.Rows[0][0]);

				TestControl.Text = "DDDD";
				TestControl.PerformControlValidation();
				AssertEquals("BBBB", TestControl.Text);
				AssertEquals("BBBB", table.Rows[0][0]);

				table.Rows[0][0] = "EEEE";
				table.AcceptChanges();
				AssertEquals("EEEE", TestControl.Text);
				AssertEquals("EEEE", table.Rows[0][0]);

				TestControl.ReadOnly = false;
				strPtr = Marshal.StringToCoTaskMemUni("CCCC");
				using (new DisposableAction(() => Marshal.FreeHGlobal(strPtr)))
				{
					UnsafeNativeMethods.SendMessage(new HandleRef(form, TestControl.Handle), 0x000c, IntPtr.Zero, strPtr);
				}
				TestControl.PerformControlValidation();
				AssertEquals("CCCC", TestControl.Text);
				AssertEquals("CCCC", table.Rows[0][0]);

				TestControl.Text = "DDDD";
				TestControl.PerformControlValidation();
				AssertEquals("DDDD", TestControl.Text);
				AssertEquals("DDDD", table.Rows[0][0]);

				TestControl.Dispose();
				table.Dispose();
			}
		}
#endif
#endregion

		#region IBackColorMutable

		public void TestColors()
		{
			using (var form = new ZForm())
			{
				var testControl2 = new ZTextBox();
				TestControl.Parent = form;
				testControl2.Parent = form;
				form.Show();
				TestControl.Focus();
				AssertEquals("Must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, TestControl.BackColor);
				AssertEquals("Must be colored in standard color", SystemColors.Window, testControl2.BackColor);

				testControl2.Focus();
				AssertEquals("Must be colored in standard color", SystemColors.Window, TestControl.BackColor);
				AssertEquals("Must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, testControl2.BackColor);
			}
		}

		#endregion

		#region IGridControl

		public void TestShouldHandleKey()
		{
			using (var textBox = new ZTextBox())
			{
				IGridControl gridControl = textBox;
				Assert("Not a multiline text box", !gridControl.ShouldHandleKey(Keys.Enter));

				textBox.Multiline = true;
				Assert("ENTER key", gridControl.ShouldHandleKey(Keys.Enter));
				Assert("DOWN key", gridControl.ShouldHandleKey(Keys.Down));
				Assert("UP key", gridControl.ShouldHandleKey(Keys.Up));
				Assert("LEFT key", gridControl.ShouldHandleKey(Keys.Left));
				Assert("RIGHT key", gridControl.ShouldHandleKey(Keys.Right));
				Assert("END key", !gridControl.ShouldHandleKey(Keys.End));
				Assert("D0 key", !gridControl.ShouldHandleKey(Keys.D0));
			}
		}

		#endregion

		#region IDisposeStackProvider

		public void TestAccessDisposedZTextBoxTrackOpenDisposedAccess()
		{
			using (Db.DisposableActionForDbConnection())
			using (Db.DisableSchemaVersionCheck())
			using (var form = new ZForm { Name = "FormA" })
			using (var panel = new ZPanel { Name = "PanelB" })
			using (var textBox = new ZTextBox { TrackDisposedAccess = true })
			{
				form.Controls.Add(panel);
				textBox.Dispose();
				panel.Controls.Add(textBox);
				AssertExceptionThrown(typeof(ObjectDisposedException), () => form.Show());
				Application.DoEvents();

				var lastException = ErrorReporter.LastExceptionReported as ObjectDisposedException;
				var lastMessage = ErrorReporter.LastMessageReported;
				AssertNotNull("A ObjectDisposedException was reported", lastException);
				AssertStartsWith("Exception Message Should Start With", "Control [name:'' type:'Enterprise.ZArchitecture.ZTextBox'] is already disposed.", lastMessage);
				AssertNotContains("Control Path: Empty\r\nControl Dispose stack trace:\r\nEmpty", lastException.Message);
				ErrorReporter.Clear();
			}
		}

		public void TestAccessDisposedZTextBoxTrackCloseDisposedAccess()
		{
			using (Db.DisposableActionForDbConnection())
			using (Db.DisableSchemaVersionCheck())
			using (var form = new ZForm { Name = "FormA" })
			using (var panel = new ZPanel { Name = "PanelB" })
			using (var textBox = new ZTextBox())
			{
				form.Controls.Add(panel);
				textBox.Dispose();
				panel.Controls.Add(textBox);
				AssertExceptionThrown(typeof(ObjectDisposedException), () => form.Show());
				Application.DoEvents();

				var lastException = ErrorReporter.LastExceptionReported as ObjectDisposedException;
				var lastMessage = ErrorReporter.LastMessageReported;
				AssertNotNull("A ObjectDisposedException was reported", lastException);
				AssertStartsWith("Exception Message Should Start With", "Control [name:'' type:'Enterprise.ZArchitecture.ZTextBox'] is already disposed.", lastMessage);
				AssertContains("Control Path: Empty\r\nControl Dispose stack trace:\r\nEmpty", lastException.Message);
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region Regression Testing

		public void TestHideToolTip()
		{
			ZTextBox.EnableMockCapsLockIsOnForTesting.Value = true;
			ZTextBox.MockCapsLockIsOn.Value = true;

			using (var form = new ZForm())
			using (var textBox1 = new ZTextBox())
			using (var textBox2 = new ZTextBox())
			{
				textBox1.PasswordChar = '*';

				form.Controls.Add(textBox1);
				form.Controls.Add(textBox2);
				form.Show();

				textBox1.GetType().InvokeMember("OnClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox1, new object[] { new KeyEventArgs(Keys.CapsLock) });
				Application.DoEvents();
				AssertCapsLockToolTipShown(textBox1);

				textBox2.Focus();
				Application.DoEvents();
				AssertNoToolTipShown(textBox1);
			}
		}

		[DeveloperOnlyTest]
		public void TestPasteInvisibleControlChars_VsMaxLength()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox
				{
					Multiline = true,
					Parent = form,
					CharacterCasing = CharacterCasing.Normal,
					MaxLength = 35
				};
				form.Show();

				string a = "0123456789";
				var action = new Action(() => SafeClipboard.SetText(a + a + a + a + "\x1"));
				action.Invoke();
				ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);

				textBox.Paste();
				AssertEquals("01234567890123456789012345678901234", textBox.Text);
				textBox.Select(1, 1);
				textBox.Paste();
				AssertEquals("00234567890123456789012345678901234", textBox.Text);
			}
		}

		[DeveloperOnlyTest]
		public void TestPasteInvisibleControlChars()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox
				{
					Multiline = true,
					Parent = form,
					CharacterCasing = CharacterCasing.Normal
				};
				form.Show();

				SafeClipboard.SetText("-\x1\x2\x1c\x1d\x1e\x1f-");
				textBox.Paste();
				AssertEquals("-\x2-", textBox.Text);
				textBox.Select(1, 1);

				SafeClipboard.SetText("\x1tab1\tline1\r\nline2\x1f");
				textBox.Paste();
				AssertEquals("-tab1\tline1\r\nline2-", textBox.Text);
			}
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestMaxLengthForPaste()
		{
			var maxLengthDummyBizo = new MaxLengthDummyBizo();
			using (var form = new ZForm(maxLengthDummyBizo))
			{
				var textBox = new ZTextBox { BindTo = "Text" };
				form.Controls.Add(textBox);
				form.Show();

				var maxLengthString = ZString.Replicate('A', maxLengthDummyBizo.TextInfo.MaxLength + 10);

				SafeClipboard.SetText(maxLengthString);
				textBox.Paste();

				Assert("text is truncated down to MaxLength", textBox.Text.Length == maxLengthDummyBizo.TextInfo.MaxLength);
			}
		}

		[DeveloperOnlyTest]
		public void TestPasteWhenMaxLengthLeftLessThanZero()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox
				{
					Multiline = true,
					Parent = form,
					CharacterCasing = CharacterCasing.Normal
				};
				textBox.MaxLength = 0;
				form.Controls.Add(textBox);
				form.Show();
				textBox.Text = "TextA";
				var maxLengthString = "TextB\x1";
				SafeClipboard.SetText(maxLengthString);

				AssertNoExceptionThrown("Exception \"Length cannot be less than zero\" should not be thrown out", () => textBox.Paste());
				AssertEquals("TextBTextA", textBox.Text);

				textBox.MaxLength = 5;
				textBox.Text = "TextC";
				textBox.MaxLength = 2;
				maxLengthString = "\x1QQQ";
				SafeClipboard.SetText(maxLengthString);
				textBox.Select(0, 2);

				AssertNoExceptionThrown("Exception \"Length cannot be less than zero\" should not be thrown out", () => textBox.Paste());
				AssertEquals("xt", textBox.Text);
			}
		}

		class MaxLengthDummyBizo : DummyNonPersistentBusinessObject
		{
			[MaxLength(128)]
			public ZString Text
			{
				get => fText;
				set
				{
					CheckMaximumLength(TextInfo, value);
					SetNonPersistentPropertyValue(TextInfo, ref fText, value);
					RefreshBinding();
				}
			}
			ZString fText;

			public ZPropertyInfo TextInfo => GetZPropertyInfo(nameof(Text));
		}

#if !WINZOR

		public void TestOnResize()
		{
			using (var form = new ZForm { Name = "FormA" })
			using (var textBox = new ZTextBox())
			{
				form.Controls.Add(textBox);
				form.Show();
				Application.DoEvents();

				textBox.Paint += (sender, args) => { };
				textBox.PaintExtension.Skip = false;
				textBox.Width += 10;
				textBox.PaintExtension.Dispose();

				AssertNoExceptionThrown("Exception \"Parameter is not valid\" shoud not be thrown out", () => textBox.Width += 20);
			}
		}

		public void TestOnPaintEvent()
		{
			using (var form = new ZForm { Name = "FormA" })
			using (var textBox = new ZTextBox())
			{
				form.Controls.Add(textBox);
				form.Show();
				Application.DoEvents();

				textBox.Paint += (sender, args) => { };
				textBox.PaintExtension.Skip = false;
				textBox.Refresh();
				textBox.PaintExtension.Dispose();

				AssertNoExceptionThrown("Exception \"Parameter is not valid\" shoud not be thrown out", () => textBox.Refresh());
			}
		}

#endif

		public void TestControlIsEditableInViewMode()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZChildForm(dummy))
			using (var control = new ZTextBox())
			{
				control.EditableInViewMode = true;
				form.Controls.Add(control);

				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				AssertEquals(false, control.ReadOnly);
			}
		}

		#endregion

		#region Macro Support

		public void TestMacroSelected()
		{
			Dummy.Collection.AddNew();
			Dummy.Z0_Description = "SOME MEANINGLESS TEXT<BR />";
			Dummy.Z0_Number = 12345;

			using (var testForm = new ZTestForm(Dummy))
			{
				var textBox = testForm.TextBox;
				textBox.SupportsMacroTemplates = true;
				textBox.MacroOpeningBracket = "(*";
				textBox.MacroClosingBracket = "*)";
				testForm.Show();
				Application.DoEvents();

				textBox.SelectionStart = 10;
				textBox.SelectionLength = 0;
				textBox.MacroSelected("<Z0_Number>");

				AssertEquals("SOME MEANI(*Z0_NUMBER*)NGLESS TEXT<BR />", textBox.Text);
				AssertEquals("SOME MEANI12345NGLESS TEXT<BR />", textBox.GetPreviewText());
			}
		}

		#endregion

		public void TestDirectTextSetCantExceedMaxLength()
		{
			using (var textBox = new ZTextBox())
			{
				textBox.MaxLength = 100;
				textBox.CharacterCasing = CharacterCasing.Normal;

				textBox.Text = new string('a', 100);
				AssertEquals("text of max length is handled correctly", 0, ErrorReporter.TotalErrorCount);
				textBox.Text = new string('a', 101);
				CombineAssertions("text exceeding max length", () =>
				{
					AssertEquals("Text does not overflow", new string('a', 100), textBox.Text);
					AssertEquals("no error", 0, ErrorReporter.TotalErrorCount);
				});
			}
		}

#if !WINZOR

		public void TestTextAreTrimmed_WhenCtrlAndBackspaceArePressedDown()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox
				{
					Multiline = true,
					Text = "This is a test\r\nEnglish 中文",
					CharacterCasing = CharacterCasing.Normal,
				};

				textBox.SelectionStart = textBox.Text.Length;
				form.Controls.Add(textBox);
				form.Show();
				textBox.Focus();

				var keys = new byte[256];
				InputSimulatorUnsafeNativeMethods.GetKeyboardState(keys);
				keys[(int)Keys.ControlKey] = 0x81;
				InputSimulatorUnsafeNativeMethods.SetKeyboardState(keys);

				using (new DisposableAction(() =>
				{
					keys[(int)Keys.ControlKey] = 0;
					InputSimulatorUnsafeNativeMethods.SetKeyboardState(keys);
				}))
				{
					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)(Keys.Control | Keys.Back));
					AssertEquals("Only one Chinese word is removed", "This is a test\r\nEnglish 中", textBox.Text);

					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)(Keys.Control | Keys.Back));
					AssertEquals("One more Chinese word is removed but space is not removed along", "This is a test\r\nEnglish ", textBox.Text);

					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)(Keys.Control | Keys.Back));
					AssertEquals("Space is removed with the English word", "This is a test\r\n", textBox.Text);

					var scanCode = InputSimulatorUnsafeNativeMethods.MapVirtualKey((uint)Keys.Z, 0);
					var lParam = (0x00000001 | (scanCode << 16));
					UnsafeNativeMethods.SendMessage(new HandleRef(this, textBox.Handle), WindowsMessage.WM_CHAR, new IntPtr(0x1a), new IntPtr(lParam));
					AssertEquals("Undo last deleted word", "This is a test\r\nEnglish ", textBox.Text);

					UnsafeNativeMethods.SendMessage(new HandleRef(this, textBox.Handle), WindowsMessage.WM_CHAR, new IntPtr(0x1a), new IntPtr(lParam));
					AssertEquals("Undo is withdrawn", "This is a test\r\n", textBox.Text);

					KeySender.SendKeyDownToProcessCmdKey(textBox, (int)(Keys.Control | Keys.Back));
					AssertEquals("Line break is removed", "This is a test", textBox.Text);
				}
			}
		}

#endif

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZTextBox.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZTextBox)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZTextBox).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		public void TestEnableFindDialog()
		{
			using (var textBox = new ZTextBox())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Default should be false", false, textBox.EnableFindDialog);

					textBox.EnableFindDialog = true;
					AssertEquals("enabled", true, textBox.EnableFindDialog);
				});
			}
		}

		public void TestGetText()
		{
			using (var textBox = new ZTextBox())
			{
				textBox.CharacterCasing = CharacterCasing.Normal;
				textBox.Text = "This is expected text";

				AssertEquals("This is expected text", textBox.GetText());
			}
		}

		public void TestHighlightText()
		{
			using (var textBox = new ZTextBox())
			{
				textBox.EnableFindDialog = true;
				textBox.CharacterCasing = CharacterCasing.Normal;
				CombineAssertions(() =>
				{
					textBox.Text = string.Empty;
					textBox.HighlightText(0, 5);
					AssertEquals("empty text selected", string.Empty, textBox.SelectedText);

					textBox.Text = "This is expected text";
					textBox.HighlightText(0, 5);
					AssertEquals("not empty text selected", "This ", textBox.SelectedText);

					textBox.Text = "This ";
					textBox.HighlightText(3, 5);
					AssertEquals("cut text selected", "s ", textBox.SelectedText);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestFocusedOnMouseClickWhenIsDisposed()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				textBox.Dispose();

				textBox.Focus();
			}
		}

		[ExpectNoExceptions]
		public void TestFocusWhenNotHandled()
		{
			using (var textBox = new ZTextBoxWithEnter())
			{
				Assert("pre-conditon: Handle not Created", !textBox.IsHandleCreated);
				textBox.OnEnter();
				Assert("After OnEnter(),IsHandleCreated will not change", !textBox.IsHandleCreated);
			}
		}

#if !WINZOR
		public void TestCtrlFShouldNotBeProcessedWhenEnableFindDialogIsFalse()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox() { Text = "This is test", EnableFindDialog = false };
				form.Controls.Add(textBox);
				form.Show();
				textBox.Focus();

				var message = Message.Create(textBox.Handle, WindowsMessage.WM_KEYDOWN, new IntPtr((int)(Keys.Control | Keys.F)), new IntPtr(0));
				var isProcessed = textBox.PreProcessMessage(ref message);

				AssertEquals("Ctrl+F should not be processed in ZTextBox", false, isProcessed);
			}
		}

		public void TestCtrlFShouldBeProcessedWhenEnableFindDialogIsTrue()
		{
			using (var form = new ZForm())
			{
				var textBox = new ZTextBox() { Text = "This is test", EnableFindDialog = true };
				form.Controls.Add(textBox);
				form.Show();
				textBox.Focus();

				var message = Message.Create(textBox.Handle, WindowsMessage.WM_KEYDOWN, new IntPtr((int)(Keys.Control | Keys.F)), new IntPtr(0));
				var isProcessed = textBox.PreProcessMessage(ref message);

				AssertEquals("Ctrl+F should be processed in ZTextBox", true, isProcessed);
			}
		}
#endif

		class ZTextBoxWithEnter : ZTextBox
		{
			public void OnEnter()
			{
				base.OnEnter(EventArgs.Empty);
			}
		}

		#region Implementation

		protected override string[] BindablePropertyNames => new[] { "Text", "ReadOnly", "MaxLength", "IsVisibleForBinding" };

		protected override string InvalidBindablePropertyName => "Size";

		ZTextBox TestControl => testControl ?? (testControl = new ZTextBox());
		ZTextBox testControl;

		protected override void BindControl()
		{
			base.BindControl();
			Control.BindTo = DummyBizoSchema.Z0_Description.Name;
			DataBoundControl.Get(Control).SetDataBinding(Dummy, Control.BindTo);
		}

		protected override void TearDown()
		{
			base.TearDown();
			testControl?.Dispose();
		}

		#endregion
	}
}
