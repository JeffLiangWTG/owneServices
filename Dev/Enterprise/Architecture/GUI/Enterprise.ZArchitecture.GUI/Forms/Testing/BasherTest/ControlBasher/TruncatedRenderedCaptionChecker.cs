#if DEBUG

using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class TruncatedRenderedCaptionChecker
	{
		public static void Check(Control control, INotifications notifications)
		{
			var resourceStringData = (control as IResCaptionedControl)?.CaptionResourceString;
			var renderer = control.GetExtension<ILabelCaptionRenderer>();
			if (renderer != null && renderer.Visible && renderer.IsCaptionTruncated && renderer.Options != StringRenderingOptions.Truncate)
			{
				notifications.AddError(string.Format(
					"Resource string caption rendered, and overlapping\n" +
					"Control Path: {0}\n" +
					"Control CaptionResourceString: {1}\n" +
					"Control DataKey: {2}\n" +
					"Captions: {3}",
					ControlDescription.GetControlPath(control),
					resourceStringData != null ? resourceStringData.Key : "null",
					new ResourceStringKeyCalculator(control).DataString.Key,
					string.Join(",", renderer.Captions)));

				notifications.Add(BasherTestNotificationType.UniqueFooterMessage, "Current DPI X,Y: " + ControlDpiScalingHelper.DpiX + ", " + ControlDpiScalingHelper.DpiY);
				notifications.Add(BasherTestNotificationType.UniqueFooterMessage, "For more information, read the resource strings Wiki article at https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/ResourceStringEditing.aspx");
			}
		}
	}
}
#endif
