using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// ZMessageBoxWithFixedSizeWithoutMultilingualString(Better Performance With Huge Message Content)
	/// </summary>
	public class ZMessageBoxWithFixedSizeWithoutMultilingualString : ZMessageBox
	{
		public ZMessageBoxWithFixedSizeWithoutMultilingualString(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
			: base(message, caption, buttons, icon)
		{
		}

		public ZMessageBoxWithFixedSizeWithoutMultilingualString(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text)
			: base(message, caption, buttons, icon, button1Text)
		{
		}

		public ZMessageBoxWithFixedSizeWithoutMultilingualString(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			: base(message, caption, buttons, icon, defaultButton)
		{
		}

		public ZMessageBoxWithFixedSizeWithoutMultilingualString(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text, string button2Text)
			: base(message, caption, buttons, icon, button1Text, button2Text)
		{
		}

		public int DefaultWidth { get; set; } = 600;

		public int DefaultHeight { get; set; } = 360;

		protected override void SetHeightWidthSettings()
		{
			TextBox.ScrollBars = ScrollBars.Vertical;
			TextBox.AutoSize = false;
			TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

			Size = ControlDpiScalingHelper.NewScaledSize(DefaultWidth, DefaultHeight);
		}
	}
}
