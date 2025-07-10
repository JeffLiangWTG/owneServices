using System;
using System.Drawing;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Implemented by ZButton and ZToolStripButton so they can be treated the same for the purpose of Posting Buttons
	/// </summary>
	public interface IButton : IButtonControl
	{
		string Text { get; set; }
		Image Image { get; set; }
		bool Enabled { get; set; }
		bool Visible { get; set; }
		Color BackColor { get; set; }
		Color ForeColor { get; set; }
		Control Parent { get; }
		event EventHandler Click;
		bool Focus();
		bool ShouldSetImage { get; }
	}
}
