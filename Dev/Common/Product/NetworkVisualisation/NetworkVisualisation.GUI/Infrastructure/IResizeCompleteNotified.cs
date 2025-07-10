using System.Windows.Controls.Primitives;

namespace CargoWise.NetworkVisualisation.GUI
{
	public interface IDragResizable
	{
		void NotifyResizeStarted(DragStartedEventArgs e);
		void NotifyResizing(DragDeltaEventArgs e);
		void NotifyResizeCompleted(DragCompletedEventArgs e);
	}
}
