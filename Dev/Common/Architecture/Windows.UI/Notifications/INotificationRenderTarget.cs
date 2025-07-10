using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on a control that delegates it's notification icon rendering to another control.
	/// </summary>
	public interface INotificationRenderTarget
	{
		/// <summary>
		/// Get the control to render the notification on. Null to have the control handle it's own
		/// error rendering.
		/// </summary>
		Control ControlToRenderNotificationOn { get; }
	}
}
