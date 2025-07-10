using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// ZMessageBoxWithFixedSize(Better Performance With Huge Message Content)
	/// </summary>
	public sealed class ZMessageBoxWithFixedSize : ZMessageBoxWithFixedSizeWithoutMultilingualString
	{
		public ZMessageBoxWithFixedSize(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
			: base(message, caption, buttons, icon)
		{
		}

		public ZMessageBoxWithFixedSize(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
			: base(message, caption, buttons, icon)
		{
		}

		public ZMessageBoxWithFixedSize(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text)
			: base(message, caption, buttons, icon, button1Text)
		{
		}

		public ZMessageBoxWithFixedSize(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			: base(message, caption, buttons, icon, defaultButton)
		{
		}

		public ZMessageBoxWithFixedSize(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			: base(message, caption, buttons, icon, defaultButton)
		{
		}

		public ZMessageBoxWithFixedSize(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text, string button2Text)
			: base(message, caption, buttons, icon, button1Text, button2Text)
		{
		}
	}
}
