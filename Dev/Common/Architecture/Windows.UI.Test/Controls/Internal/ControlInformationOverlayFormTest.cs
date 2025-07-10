using System.Drawing;
using NUnit.Framework;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlInformationOverlayFormTest : TestCase
	{
		public void TestCenterOnControl()
		{
			using (var form = new KForm())
			{
				var textbox = new KTextBox { Location = NewScaledPoint(10, 10) };
				form.Controls.Add(textbox);

				var radio = new KRadioButton();
				radio.Location = NewScaledPoint(textbox.Left, textbox.Bottom + ScaleToCurrentDpiX(5));
				form.Controls.Add(radio);

				form.Show();

				using (var overlay = new ControlInformationOverlayForm())
				{
					AssertEquals(false, overlay.hideInformation);
					overlay.CenterOnControl(textbox, form);
					AssertEquals(false, overlay.hideInformation);

					CombineAssertions(() =>
					{
						Assert("Parent should still have focus", form.ContainsFocus);
						Assert("Visible", overlay.Visible);
						AssertEquals("Location", form.Location, overlay.Location);
						AssertEquals("Size", form.Size + PaddingForPen, overlay.Size);
						AssertEquals("Control", textbox, overlay.HighlightedControl);
					});

					overlay.Hide();
					form.Location = NewScaledPoint(15, 23);
					form.Size = NewScaledSize(300, 300);
					form.Name = "InfoDiggerForm";

					overlay.CenterOnControl(radio, form);
					AssertEquals(true, overlay.hideInformation);

					CombineAssertions(() =>
					{
						Assert("Parent should still have focus", form.ContainsFocus);
						Assert("Visible", overlay.Visible);
						AssertEquals("Location", form.Location, overlay.Location);
						AssertEquals("Size", form.Size + PaddingForPen, overlay.Size);
						AssertEquals("Control", radio, overlay.HighlightedControl);
					});

					form.Name = "fdgfdgdf";
					overlay.CenterOnControl(radio, form);
					AssertEquals(false, overlay.hideInformation);
				}
			}
		}

		Size PaddingForPen => new Size(ControlInformationOverlayForm.OutlineThickness, ControlInformationOverlayForm.OutlineThickness);
	}
}
