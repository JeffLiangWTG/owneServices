using System.Windows.Forms;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class TextBoxControlSizeTest : TestCase
	{
		public void TestResizeControl()
		{
			using (var control = new ZTextBox())
			{
				control.Width = 1;
				AssertEquals("Initial Width", 1, control.Width);

				TextBoxControlSize.ResizeControl(control, 1);
				Assert("Width should be greater than 1.", control.Width > 1);

				var width = control.Width;
				TextBoxControlSize.ResizeControl(control, 2);
				Assert("Width should be greater than width of 1 character.", control.Width > width);
				width = control.Width;

				TextBoxControlSize.ResizeControl(control, 0);
				AssertEquals("Width should remain the same.", width, control.Width);
			}
		}

		public void TestResizeControlWhenObjectsAreDisposed()
		{
			using (var control = new TextBox())
			{
				control.Width = 50;
				Assert("GetControlWidth should return a non-zero value.", TextBoxControlSize.GetControlWidth(control, 10) > 0);
				control.Dispose();
				var expectedWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(82);
				AssertEquals("GetControlSize", expectedWidth, TextBoxControlSize.GetControlWidth(control, 10));
				TextBoxControlSize.ResizeControl(control, 10);
				AssertEquals("Control should not be resized.", expectedWidth, control.Width);
			}
		}
	}
}
