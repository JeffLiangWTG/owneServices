using System.Windows.Forms;

namespace Enterprise.Customs.CN.GUI.Testing
{
	public static class FormHelper
	{
		public static void AssertMinimumSizeNotTooBig(Form form)
		{
			var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1224);
			var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(860);

			NUnit.Framework.Assertion.Assert("CN Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
			NUnit.Framework.Assertion.Assert("CN Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
		}
	}
}
