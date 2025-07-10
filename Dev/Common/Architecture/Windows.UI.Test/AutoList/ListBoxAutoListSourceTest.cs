using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ListBoxAutoListSourceTest : TestCase
	{
		public void TestClickSelectsItem()
		{
			TestListBoxAutoListSource source = new TestListBoxAutoListSource(null);
			TestListBox ctrl = (TestListBox)source.NewListControl();
			source.UpdateListControl(ctrl, new TextBox());

			Point screen_point = ctrl.PointToScreen(new Point(5, 30));
			source.OnListBoxClick(ctrl, screen_point);
			Assert("Should select the second item", ctrl.SelectedIndex > 0);
		}

		public void TestGetListControlSelectedText()
		{
			TestListBoxAutoListSource source = new TestListBoxAutoListSource(null);
			ListBox ctrl = source.NewListControl();
			source.UpdateListControl(ctrl, new TextBox());

			ctrl.SelectedIndex = 1;
			AssertEquals("Should returned second selected item", "2", source.GetListControlSelectedText(ctrl));
		}

		public void TestReplaceText()
		{
			TestListBoxAutoListSource source = new TestListBoxAutoListSource(null);
			ListBox ctrl = source.NewListControl();
			source.UpdateListControl(ctrl, new TextBox());
			ctrl.SelectedIndex = 0;

			TextBoxBase textbox = new TextBox();
			textbox.Text = "old_text";
			textbox.SelectionStart = 5;
			textbox.SelectionLength = 3;
			source.ReplaceText(ctrl, textbox);

			AssertEquals("Resultant text", "1", textbox.Text);
			AssertEquals("Resultant selectionStart", 1, textbox.SelectionStart);
			AssertEquals("Resultant selectionLength", 0, textbox.SelectionLength);
		}

		public void TestUpDownKeyPress()
		{
			TestListBoxAutoListSource source = new TestListBoxAutoListSource(null);
			ListBox ctrl = source.NewListControl();
			source.UpdateListControl(ctrl, new TextBox());
			ctrl.SelectedIndex = 0;

			source.OnTextBoxKeyDown(ctrl, new KeyEventArgs(Keys.Down));
			AssertEquals("Selected item should be the next one", 1, ctrl.SelectedIndex);
			source.OnTextBoxKeyDown(ctrl, new KeyEventArgs(Keys.Up));
			AssertEquals("Selected item should be the previous one", 0, ctrl.SelectedIndex);
		}

		public void TestDoubleClick_FiresValueCommitRequired()
		{
			TestListBoxAutoListSource source = new TestListBoxAutoListSource(null);
			ListBox box = source.NewListControl();
			box.Items.Add("splaty1");
			box.Items.Add("splaty2");
			box.Items.Add("splaty3");
			box.Items.Add("splaty4");
			box.CreateControl();
			box.SelectedIndex = 2;

			source.ValueCommitRequired += new EventHandler(OnSource_ValueCommitRequired);
			source.OnListBoxDoubleClick(box, box.PointToScreen(new Point(5, 5)));
			AssertEquals("Should have called the event", true, OnSource_ValueCommitRequiredCalled);
		}

		bool OnSource_ValueCommitRequiredCalled;
		void OnSource_ValueCommitRequired(object sender, EventArgs e)
		{ OnSource_ValueCommitRequiredCalled = true; }

		class TestListBoxAutoListSource : ListBoxAutoListSourceForTest
		{
			public TestListBoxAutoListSource(ITypeDescriptorContext context) : base(context)
			{ }

			public new string GetListControlSelectedText(ListBox listControl)
			{ return base.GetListControlSelectedText(listControl); }

			public new void OnListBoxClick(ListBox sender, Point mouseScreenPosition)
			{ base.OnListBoxClick(sender, mouseScreenPosition); }

			public new void OnListBoxDoubleClick(ListBox sender, Point mouseScreenPosition)
			{ base.OnListBoxDoubleClick(sender, mouseScreenPosition); }

			internal new ListBox NewListControl() => base.NewListControl();

			internal new bool UpdateListControl(ListBox listControl, TextBoxBase textBox) =>
				base.UpdateListControl(listControl, textBox);
		}

		class TestListBox : ListBox
		{
			public new void OnMouseMove(MouseEventArgs e)
			{ base.OnMouseMove(e); }

			public new void OnClick(EventArgs e)
			{ base.OnClick(e); }
		}

		class ListBoxAutoListSourceForTest : ListBoxAutoListSource
		{
			protected ListBoxAutoListSourceForTest(ITypeDescriptorContext context) : base(context)
			{ }

			protected override ListBox NewListControl()
			{
				return new TestListBox();
			}

			internal new void OnListBoxDoubleClick(ListBox sender, Point mouseScreenPosition) =>
				base.OnListBoxDoubleClick(sender, mouseScreenPosition);

			protected override bool UpdateListControl(ListBox listControl, TextBoxBase textBox)
			{
				listControl.Items.Add("1");
				listControl.Items.Add("2");
				listControl.Items.Add("3");
				listControl.Items.Add("4");
				listControl.Items.Add("5");
				return false;
			}

			internal new void ReplaceText(ListBox listControl, TextBoxBase textBox) =>
				base.ReplaceText(listControl, textBox);

			internal new void OnTextBoxKeyDown(ListBox listControl, KeyEventArgs e) =>
				base.OnTextBoxKeyDown(listControl, e);
		}
	}
}
