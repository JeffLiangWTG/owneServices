using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmNotePopupEditTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructor() => new ZStmNotePopupEdit().Dispose();

		[ExpectNoExceptions]
		public void TestShow()
		{
			using (var form = new ZTestForm())
			{
				form.Controls.Add(control);
				form.Show();
			}
		}

		public void TestButtonTextDefault() => AssertEquals(string.Empty, control.ButtonText);

		public void TestTextBoxIsVisibleByDefault()
		{
			AssertEquals(true, control.ShowTextBox);
			AssertEquals(true, TextBox.Visible);
		}

		public void TestButtonTabStopIsFalse() => AssertEquals(false, Button.TabStop);

#if !WINZOR

		[DeveloperOnlyTest]
		public void TestPasteMessage()
		{
			using (var form = new ZForm(Dummy))
			{
				control.NoteTypeDescription = PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description;
				form.Controls.Add(control);
				control.SetDataBinding(Dummy, string.Empty);
				form.Show();

				_ = SafeClipboard.SetText("TestText");
				_ = UnsafeNativeMethods.PostMessage(new HandleRef(TextBox, TextBox.Handle), WindowsMessage.WM_PASTE, IntPtr.Zero, IntPtr.Zero);
				Application.DoEvents();

				// a paste message should change the text
				AssertEquals("TestText", TextBox.Text);

				// the bug was that while the text would change, the internal note buffer would 
				// not be in sync with the text and so subsequent changes to the text would cause
				// the buffer to be incorrectly manipulated

				// a simple way to change the text is to invoke a "manual" paste which was already handled by the control
				_ = ((IPastableControl)TextBox).TryPaste();
				AssertEquals("TestTextTestText", TextBox.Text);
			}
		}

#endif

		[DeveloperOnlyTest]
		public void TestCut()
		{
			_ = UnsafeNativeMethods.PostMessage(new HandleRef(TextBox, TextBox.Handle), WindowsMessage.WM_CUT, IntPtr.Zero, IntPtr.Zero);
			Application.DoEvents();

			AssertEquals(string.Empty, TextBox.Text);

			KeySender.PostKeyDown(TextBox, Keys.D1);
			KeySender.PostKeyDown(TextBox, Keys.D2);
			KeySender.PostKeyDown(TextBox, Keys.D3);
			Application.DoEvents();

			AssertEquals("123", TextBox.Text);

			TextBox.SelectAll();
			_ = UnsafeNativeMethods.PostMessage(new HandleRef(TextBox, TextBox.Handle), WindowsMessage.WM_CUT, IntPtr.Zero, IntPtr.Zero);
			Application.DoEvents();

			AssertEquals(string.Empty, TextBox.Text);
			AssertEquals("123", SafeClipboard.GetDataObject().GetData(typeof(string)));
		}

		#region Implementation

#if !WINZOR
		DummyEnterpriseBusinessObject Dummy => dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>());
		DummyEnterpriseBusinessObject dummy;
#endif

		protected override void SetUp()
		{
			base.SetUp();
			control = new ZStmNotePopupEdit();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ZTextBox TextBox => (ZTextBox)control.GetType().GetField("TextBox", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);

		ZButton Button => (ZButton)typeof(ZStmNotePopupBase).GetField("popupButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);

		ZStmNotePopupEdit control;

		#endregion
	}
}
