using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Contains the values passed by the MouseHook to the internal MouseProc,
	/// repackaged as .NET types
	/// </summary>
	public class MouseHookEventArgs
	{
		public bool DisableMessage;
		/// <summary>
		/// The button that is associated with the event
		/// </summary>
		public readonly MouseButtons Button = MouseButtons.None;

		/// <summary>
		/// The X location of the mouse when the event occured
		/// </summary>
		public readonly int X;

		/// <summary>
		/// The Y location of the mouse when the event occured
		/// </summary>
		public readonly int Y;

		/// <summary>
		/// The control that will ultimately receive the hooked message
		/// </summary>
		public readonly Control Control;

		/// <summary>
		/// The window area that the mouse is over
		/// </summary>
		public readonly HitTestCode HitTestCode = HitTestCode.HTNOWHERE;

		bool isNonClientArea;

		public MouseHookEventArgs(MouseButtons button, int x, int y, Control control, HitTestCode hitTestCode)
		{
			Button = button;
			X = x;
			Y = y;
			Control = control;
			HitTestCode = hitTestCode;
		}

		/// <summary>
		/// Did the event occur over a non-client area
		/// </summary>
		public bool IsNonClientArea
		{
			get { return isNonClientArea; }
		}

		internal void SetNonClient() { isNonClientArea = true; }
	}
}
