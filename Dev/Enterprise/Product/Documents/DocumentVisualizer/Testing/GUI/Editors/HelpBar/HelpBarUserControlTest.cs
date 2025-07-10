using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class HelpBarUserControlTest : TestCase
	{
		public void TestHints()
		{
			using (var panel = new Panel())
			{
				using (var control = new DummyIHaveHelpBar())
				{
					panel.Controls.Add(control);

					HelpBarUserControl.AttachToParent(control);

					var helpBar = control.HelpBar;
					AssertNotNull(helpBar);
					AssertEquals(4, helpBar.Controls.Count);

					var label1 = (ZLabel)helpBar.Controls[0];
					AssertLabel(label1, "Tab", true, 0, 0);

					var label2 = (ZLabel)helpBar.Controls[1];
					AssertLabel(label2, "Tab to another control", false, label1.Width, 0);

					var expectedSecondRowTop = ControlDpiScalingHelper.ScaleToCurrentDpiY(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(label1.Height) + 4);

					var label3 = (ZLabel)helpBar.Controls[2];
					AssertLabel(label3, "Ctrl+K", true, 0, expectedSecondRowTop);

					var label4 = (ZLabel)helpBar.Controls[3];
					AssertLabel(label4, "Make the world explode", false, label3.Width, expectedSecondRowTop);
				}

				AssertCollectionNotContains(typeof(DummyIHaveHelpBar), panel.Controls.Cast<Control>().Select(x => x.GetType()));
			}
		}

		public void TestNoHints()
		{
			using (var panel = new Panel())
			using (var control = new DummyIHaveHelpBar())
			{
				panel.Controls.Add(control);

				control.Empty = true;
				HelpBarUserControl.AttachToParent(control);

				var helpBar = control.HelpBar;
				AssertNotNull(helpBar);
				AssertEquals(1, helpBar.Controls.Count);

				var noHelpLabel = (ZLabel)helpBar.Controls[0];
				AssertLabel(noHelpLabel, "No help available for this control.", false, 0, 0);
			}
		}

		void AssertLabel(ZLabel label, string expectedText, bool bold, int x, int y)
		{
			AssertEquals(expectedText, label.Text);
			AssertEquals(bold, label.Font.Bold);
			AssertEquals(new Point(x, y), label.Location);
		}

		public void TestLocation()
		{
			using (var panel = new Panel())
			using (var control = new DummyIHaveHelpBar())
			{
				panel.Size = ControlDpiScalingHelper.NewScaledSize(600, 900);
				control.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);

				panel.Controls.Add(control);

				HelpBarUserControl.AttachToParent(control);

				var helpBar = control.HelpBar;
				AssertNotNull(helpBar);

				helpBar.Size = ControlDpiScalingHelper.NewScaledSize(150, 50);

				control.Location = new Point(0, 0);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(0, 100), helpBar.Location);

				control.Location = ControlDpiScalingHelper.NewScaledPoint(20, 20);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(20, 120), helpBar.Location);

				control.Location = ControlDpiScalingHelper.NewScaledPoint(480, 790);
				AssertEquals(ControlDpiScalingHelper.NewScaledPoint(430, 740), helpBar.Location);
			}
		}
	}
}
