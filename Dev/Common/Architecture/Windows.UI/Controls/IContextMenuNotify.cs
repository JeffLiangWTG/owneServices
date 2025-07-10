using System.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implement on a control to be notified of a KContextMenuStrip opening.
	/// For example, if the user right-clicks on a TreeView, IContextMenuNotify is implemented on the TreeView to highlight
	/// the node the mouse is currently under.
	/// </summary>
	public interface IContextMenuNotify
	{
		void OnContextMenuOpening(CancelEventArgs e);
	}
}
