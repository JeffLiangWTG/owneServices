using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZToolStripTest : TestCase
	{
#if !WINZOR
		public void TestCjkVerticalLayout()
		{
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("k1", new ResourceStringData("k1", "根据重量分段"));

				using (var form = new ZForm())
				{
					var toolStrip = new ZToolStrip();
					toolStrip.TextDirection = ToolStripTextDirection.Vertical270;

					var button1 = new ZToolStripButton();
					button1.CaptionResourceString = Res.GetData("k1", "Test");
					toolStrip.Items.Add(button1);

					var button2 = new ZToolStripButton();
					button2.CaptionResourceString = Res.GetData("k2", "Bla");
					toolStrip.Items.Add(button2);

					form.Controls.Add(toolStrip);

					form.Show();
					form.Refresh();

					AssertEquals(ToolStripTextDirection.Horizontal, button1.TextDirection);
					AssertEquals("根\r\n据\r\n重\r\n量\r\n分\r\n段\r\n", button1.Text);
					AssertEquals(ToolStripTextDirection.Vertical270, button2.TextDirection);
					AssertEquals("Bla", button2.Text);
				}
			}
		}
#endif

		public void AssertSelectToolStrip(int count, bool hideGripStyle)
		{
			using (var form = new ZForm())
			{
				var userControl = new ZUserControlForTest();

				var textBox = new ZTextBox();
				textBox.TabIndex = 1;

				var toolStrip = new ZToolStrip();
				toolStrip.TabStop = true;
				toolStrip.TabIndex = 2;
				toolStrip.AlwaysSelectLast = true;
				if (hideGripStyle)
				{
					toolStrip.GripStyle = ToolStripGripStyle.Hidden;
				}

				var buttons = new List<ZToolStripButton>();
				for (var i = 0; i < count; ++i)
				{
					var button = new ZToolStripButton();
					button.Name = "button" + i;
					toolStrip.Items.Add(button);
					buttons.Add(button);
				}

				userControl.Controls.Add(textBox);
				userControl.Controls.Add(toolStrip);
				form.Controls.Add(userControl);

				form.Show();

				textBox.Focus();
				userControl.ProcessTabKeyCoreExposed(true);
				Assert(buttons.Last().Selected);

				textBox.Focus();
				toolStrip.AlwaysSelectLast = false;
				userControl.ProcessTabKeyCoreExposed(true);
				Assert(buttons.First().Selected);
			}
		}

#if !WINZOR
		public void TestSelectToolStrip()
		{
			AssertSelectToolStrip(2, false);
			AssertSelectToolStrip(2, true);
			AssertSelectToolStrip(3, false);
			AssertSelectToolStrip(3, true);
			AssertSelectToolStrip(4, false);
			AssertSelectToolStrip(4, true);
		}

		public void TestModifyImageScalingSize()
		{
			using (var form1 = new ZForm())
			{
				var toolStrip = new ZToolStrip();
				AssertEquals("Default ImageScalingSize should be 16 * 16", new System.Drawing.Size(16, 16), toolStrip.ImageScalingSize);
			}

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(192, 192))
			using (var form2 = new ZForm())
			{
				var toolStrip = new ZToolStrip();
				AssertEquals("when DPI is 192 the ImageScalingSize should be 32 * 32", new System.Drawing.Size(32, 32), toolStrip.ImageScalingSize);
			}
		}
#endif
	}
}
