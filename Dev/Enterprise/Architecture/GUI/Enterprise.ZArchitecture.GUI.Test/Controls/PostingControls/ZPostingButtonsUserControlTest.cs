using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ZPostingButtonsUserControlTest : TestCase
	{
		public void TestButtons()
		{
			using (var form = new Form())
			{
				form.Controls.Add(UserControl);
				var toolStrip = (ZToolStrip)UserControl.Controls.Find("toolStrip", true)[0];
				foreach (ZToolStripButton item in toolStrip.Items)
				{
					AssertEquals(ToolStripItemImageScaling.SizeToFit, item.ImageScaling);
					var expectedPadding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0);
					AssertEquals(expectedPadding, item.Margin);
					AssertEquals(false, item.AutoToolTip);
					AssertEquals(ToolStripItemAlignment.Right, item.Alignment);
				}
			}
		}

		public void TestInsertAdditionalButton()
		{
			using (var form = new Form())
			{
				form.Controls.Add(UserControl);
				var toolStrip = (ZToolStrip)UserControl.Controls.Find("toolStrip", true)[0];
				var insertedButton = UserControl.InsertAdditionalButton("insertedButton", "insertedButton", Icons.GetImage(IconTypes.SaveNewButtonRest), 3);
				CombineAssertions(() =>
				{
					AssertEquals(insertedButton, toolStrip.Items[3]);
					AssertEquals(ToolStripItemImageScaling.SizeToFit, insertedButton.ImageScaling);
					var expectedPadding = ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0);
					AssertEquals(expectedPadding, insertedButton.Margin);
					AssertEquals(false, insertedButton.AutoToolTip);
					AssertEquals(ToolStripItemAlignment.Right, insertedButton.Alignment);
				});
			}
		}

		// ToolTip must be given focus so that the last edited value is committed - otherwise it is lost
		public void TestToolStripGetsFocusWhenItemClicked()
		{
			using (var form = new Form())
			{
				var textBox = new TextBox();
				form.Controls.Add(UserControl);
				form.Controls.Add(textBox);

				form.Show();

				var toolStrip = (ZToolStrip)UserControl.Controls.Find("toolStrip", true)[0];

				textBox.Focus();
				Application.DoEvents();
				AssertEquals(false, toolStrip.Focused);

				FireMouseClick((ZToolStripButton)toolStrip.Items[0]);
				AssertEquals(true, toolStrip.Focused);

				textBox.Focus();
				Application.DoEvents();
				AssertEquals(false, toolStrip.Focused);
				FireMouseClick(toolStrip);
				AssertEquals(true, toolStrip.Focused);

				textBox.Focus();
				Application.DoEvents();
				AssertEquals(false, toolStrip.Focused);
				toolStrip.Items[0].PerformClick(); // Emulate invoking item without mouse (e.g. with shortcut)
				AssertEquals(true, toolStrip.Focused);
			}
		}

		public void TestTabStop()
		{
			AssertEquals(false, UserControl.TabStop);
		}

		#region Implementation

		void FireMouseClick(ZToolStripButton control)
		{
			var onMouseDown = typeof(ToolStripItem).GetMethod("OnClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			onMouseDown.Invoke(control, new object[] { EventArgs.Empty });
		}

		void FireMouseClick(ZToolStrip toolStrip)
		{
			var onMouseClick = toolStrip.GetType().GetMethod("OnMouseClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			onMouseClick.Invoke(toolStrip, new object[] { new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0) });
		}

		ZPostingButtonsUserControl UserControl
		{
			get { return userControl ?? (userControl = new ZPostingButtonsUserControl()); }
		}
		ZPostingButtonsUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (userControl != null)
			{
				userControl.Dispose();
			}
		}

		#endregion
	}
}
