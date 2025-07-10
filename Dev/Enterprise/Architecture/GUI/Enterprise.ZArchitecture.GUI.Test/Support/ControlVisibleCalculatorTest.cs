using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ControlVisibleCalculatorTest : NUnit.Framework.TestCase
	{
		public void TestIsSetVisible()
		{
			using (var form = new KForm())
			{
				var tabControl = new ZTabControl();
				var tabPage1 = new ZTabPage();
				var tabPage2 = new ZTabPage();
				var groupBox1 = new ZGroupBox();
				var userControl2 = new UserControl();
				var control1 = new ZLabel();
				var control2 = new ZLabel();

				groupBox1.Controls.Add(control1);
				userControl2.Controls.Add(control2);
				tabPage1.Controls.Add(groupBox1);
				tabPage2.Controls.Add(userControl2);
				tabControl.TabPages.Add(tabPage1);
				tabControl.TabPages.Add(tabPage2);
				form.Controls.Add(tabControl);

				form.Show();

				// this is currently how the control visible works, and this means that if you set visible on a control, and then call the get visible, you may not get the same value back
				AssertEquals("TabPage 1 WinForm visible", true, tabPage1.TabVisible);
				AssertEquals("TabPage 2 WinForm visible", true, tabPage2.TabVisible);
				AssertEquals("Group box 1 WinForm visible", true, groupBox1.Visible);
				AssertEquals("User control 2 WinForm visible", false, userControl2.Visible);
				AssertEquals("Control 1 WinForm visible", true, control1.Visible);
				AssertEquals("Control 2 WinForm visible", false, control2.Visible);

				AssertEquals("Group box 1 set visible", true, ControlVisibleCalculator.IsSetVisible(groupBox1));
				AssertEquals("User control 2 set visible", true, ControlVisibleCalculator.IsSetVisible(userControl2));
				AssertEquals("Control 1 set visible", true, ControlVisibleCalculator.IsSetVisible(control1));
				AssertEquals("Control 2 set visible", true, ControlVisibleCalculator.IsSetVisible(control2));

				groupBox1.Visible = false;
				userControl2.Visible = false;

				AssertEquals("Group box 1 WinForm visible", false, groupBox1.Visible);
				AssertEquals("User control 2 WinForm visible", false, userControl2.Visible);
				AssertEquals("Control 1 WinForm visible", false, control1.Visible);
				AssertEquals("Control 2 WinForm visible", false, control2.Visible);

				AssertEquals("Group box 1 set visible", false, ControlVisibleCalculator.IsSetVisible(groupBox1));
				AssertEquals("User control 2 set visible", false, ControlVisibleCalculator.IsSetVisible(userControl2));
				AssertEquals("Control 1 set visible", false, ControlVisibleCalculator.IsSetVisible(control1));
				AssertEquals("Control 2 set visible", false, ControlVisibleCalculator.IsSetVisible(control2));

				groupBox1.Visible = true;
				userControl2.Visible = true;

				AssertEquals("Group box 1 WinForm visible", true, groupBox1.Visible);
				AssertEquals("User control 2 WinForm visible", false, userControl2.Visible);
				AssertEquals("Control 1 WinForm visible", true, control1.Visible);
				AssertEquals("Control 2 WinForm visible", false, control2.Visible);

				AssertEquals("Group box 1 set visible", true, ControlVisibleCalculator.IsSetVisible(groupBox1));
				AssertEquals("User control 2 set visible", true, ControlVisibleCalculator.IsSetVisible(userControl2));
				AssertEquals("Control 1 set visible", true, ControlVisibleCalculator.IsSetVisible(control1));
				AssertEquals("Control 2 set visible", true, ControlVisibleCalculator.IsSetVisible(control2));

				control1.Visible = false;
				control2.Visible = false;

				AssertEquals("Control 1 WinForm visible", false, control1.Visible);
				AssertEquals("Control 2 WinForm visible", false, control2.Visible);

				AssertEquals("Control 1 set visible", false, ControlVisibleCalculator.IsSetVisible(control1));
				AssertEquals("Control 2 set visible", false, ControlVisibleCalculator.IsSetVisible(control2));

				control1.Visible = true;
				control2.Visible = true;

				AssertEquals("Control 1 WinForm visible", true, control1.Visible);
				AssertEquals("Control 2 WinForm visible", false, control2.Visible);

				AssertEquals("Control 1 set visible", true, ControlVisibleCalculator.IsSetVisible(control1));
				AssertEquals("Control 2 set visible", true, ControlVisibleCalculator.IsSetVisible(control2));
			}
		}
	}
}
