using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1040:MixedDpiAwareAndUnawareRule", Justification = "Test code")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1041:DoubleScalingComponentsRule", Justification = "Test code")]
	sealed class AutoListManagerTest : TestCase
	{
		public void TestShowAutoListForm()
		{
			try
			{ DoTestShowAutoListForm(); }
			catch { DoTestShowAutoListForm(); }
		}

		void DoTestShowAutoListForm()
		{
			if (SetupTest(true))
			{
				AssertEquals("Auto-list form should now be showing", true, manager.frmAutoList.Visible);
				AssertEquals(
					"The form the text box is on should still be the active form",
					Form.ActiveForm, form);

				Point textBoxLocation = textBox.PointToScreen(new Point(0, 0));
				int xoffset = (int)textBox.CreateGraphics().MeasureString("some", textBox.Font).Width;
				Point expectedAutoformLocation = textBoxLocation;
				int expectedy = expectedAutoformLocation.Y + (textBox.Height - 1);

				DateTime before = DateTime.Now; // This is only a unit test
				while (DateTime.Now.Subtract(before) > new TimeSpan(0, 0, 2))
				{
					Application.DoEvents();
				}
				AssertEquals(
					"Auto-list form should be in the right location",
					expectedy, manager.frmAutoList.Location.Y);
			}
		}

		public void TestReplaceText()
		{
			try
			{
				DoTestReplaceText();
			}
			catch
			{
				DoTestReplaceText();
			}
		}

		void DoTestReplaceText()
		{
			if (SetupTest(true))
			{
				KSendKeys.SendWait("{DOWN}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{DOWN}", textBox);
				KSendKeys.SendWait("{ENTER}", textBox);

				AssertEquals("sometext", source.TextReplaced.ToLower());
				AssertEquals(8, source.SelectionStartReplaced);
				AssertEquals(0, source.SelectionLengthReplaced);

				AssertEquals("Should replace the text properly", "2", textBox.Text);
			}
		}

		public void TestReplaceText_AfterCommitValueRequiredFired()
		{
			try
			{ DoTestReplaceText_AfterCommitValueRequiredFired(); }
			catch { TearDown(); DoTestReplaceText_AfterCommitValueRequiredFired(); }
		}

		void DoTestReplaceText_AfterCommitValueRequiredFired()
		{
			if (SetupTest(true))
			{
				KSendKeys.SendWait("{DOWN}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{UP}", textBox);
				KSendKeys.SendWait("{DOWN}", textBox);
				source.OnValueCommitRequired(EventArgs.Empty);
				Application.DoEvents();

				AssertEquals("sometext", source.TextReplaced.ToLower());
				AssertEquals(8, source.SelectionStartReplaced);
				AssertEquals(0, source.SelectionLengthReplaced);

				AssertEquals("Should replace the text properly", "2", textBox.Text);
			}
		}

		public void TestMovingFormMovesAutoListForm()
		{
			try
			{ DoTestMovingFormMovesAutoListForm(); }
			catch { TearDown(); DoTestMovingFormMovesAutoListForm(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "This is only a unit test")]
		void DoTestMovingFormMovesAutoListForm()
		{
			if (SetupTest(true))
			{
				Screen screen = Screen.FromControl(form);

				int fromFormTopToTextboxBottom = textBox.Bottom;
				int newTop =
					screen.WorkingArea.Height -
					fromFormTopToTextboxBottom -
					manager.frmAutoList.Size.Height;
				form.Location = new Point(form.Left, newTop);

				DateTime before = DateTime.Now; // This is only a unit test
				while (manager.frmAutoList == null && DateTime.Now.Subtract(before) < new TimeSpan(0, 0, 3))
				{
					Application.DoEvents();
				}

				Point textboxScreenLocation = textBox.Parent.PointToScreen(textBox.Location);
				int bottom = manager.frmAutoList.Location.Y + manager.frmAutoList.Size.Height;
				AssertEquals(
					"frmAutoList should now be above the text box after the form has moved to the bottom",
					textboxScreenLocation.Y, bottom - 1);
			}
		}

		public void TestTextBoxLostFocusCallsEvent()
		{
			try
			{ DoTestTextBoxLostFocusCallsEvent(); }
			catch { TearDown(); DoTestTextBoxLostFocusCallsEvent(); }
		}

		void DoTestTextBoxLostFocusCallsEvent()
		{
			if (SetupTest(false))
			{
				textBox.Focus();

				AssertEquals("Event should not be raise initially for test", false, source.OnTextBoxLostFocusCalled);
				TextBox decoyControl = new TextBox();
				form.Controls.Add(decoyControl);
				decoyControl.Focus();

				AssertEquals(
					"Should call the lost focus event as the text box has lost focus",
					true, source.OnTextBoxLostFocusCalled);
			}
		}

		public void TestTextBoxDisposeCallsTextBoxLostFocusEvent()
		{
			if (SetupTest(false))
			{
				AssertEquals("Event should not be raise initially for test", false, source.OnTextBoxLostFocusCalled);
				textBox.Dispose();
				AssertEquals(
					"Should call the lost focus event as the text box no longer exists",
					true, source.OnTextBoxLostFocusCalled);
			}
		}

		#region Test Classes

		class TestAutoListManager : AutoListManager
		{
			public new Form frmAutoList
			{ get { return base.frmAutoList; } }
		}

		class TestAutoListSource : ListBoxAutoListSource
		{
			public string TextReplaced;
			public int SelectionStartReplaced;
			public int SelectionLengthReplaced;

			public bool OnTextBoxLostFocusCalled;

			public TestAutoListSource(ITypeDescriptorContext context)
				: base(context)
			{
			}

			public new void OnValueCommitRequired(EventArgs e)
			{ base.OnValueCommitRequired(e); }

			protected override bool UpdateListControl(ListBox listControl, TextBoxBase textBox)
			{
				listControl.Items.Clear();
				listControl.Items.Add("1");
				listControl.Items.Add("2");
				listControl.Items.Add("3");
				listControl.Items.Add("4");
				listControl.Items.Add("5");
				listControl.Items.Add("6");
				listControl.Items.Add("7");
				listControl.Items.Add("8");
				listControl.Items.Add("9");
				return true;
			}

			protected override void ReplaceText(ListBox listControl, TextBoxBase textBox)
			{
				TextReplaced = textBox.Text;
				SelectionStartReplaced = textBox.SelectionStart;
				SelectionLengthReplaced = textBox.SelectionLength;
				base.ReplaceText(listControl, textBox);
			}

			protected override void OnTextBoxLostFocus()
			{
				base.OnTextBoxLostFocus();
				OnTextBoxLostFocusCalled = true;
			}
		}

		#endregion

		#region Implementation

		Form form;
		TextBoxBase textBox;
		TestAutoListManager manager;
		TestAutoListSource source;

		protected override void TearDown()
		{
			base.TearDown();
			if (textBox != null)
			{
				textBox.Dispose();
			}

			if (form != null)
			{
				form.Dispose();
			}

			if (manager != null)
			{
				manager.Dispose();
			}
		}

		bool SetupTest(bool checkAutoListShown)
		{
			form = new Form(); // This is only a unit test
			textBox = new TextBox();
			form.Controls.Add(textBox);

			bool result = form.ShowAndCheckFormIsActive();
			if (result)
			{
				textBox.Focus();

				source = new TestAutoListSource(null);
				manager = new TestAutoListManager();
				manager.TextBox = textBox;
				manager.AutoListSource = source;

				Assert(
					"Auto-list form should be invisible at first",
					manager.frmAutoList == null || !manager.frmAutoList.Visible);

				KSendKeys.SendWait("sometext", textBox);
				textBox.SelectionStart = 2;
				textBox.SelectionLength = 2;

				if (checkAutoListShown)
				{
					AssertEquals(2, textBox.SelectionStart);
					AssertEquals(2, textBox.SelectionLength);
					if (manager.frmAutoList == null || !manager.frmAutoList.Visible)
					{
						DateTime before = DateTime.Now; // This is only a unit test
						while (DateTime.Now.Subtract(before) < new TimeSpan(0, 0, 2))
						{
							Application.DoEvents();
						}
					}
					AssertEquals("Should be showing the auto-list form now", true, manager.frmAutoList.Visible);
				}
			}
			return result;
		}

		#endregion
	}
}
