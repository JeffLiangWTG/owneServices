using System;
using System.Drawing;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IClickableNestedMenuItem
	{
		string Text { get; set; }
		event EventHandler Click;
		void AddChildItem(IClickableNestedMenuItem child);
		Image Image { get; set; }
	}
}
