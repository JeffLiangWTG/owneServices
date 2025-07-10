using System.Collections;
using System.Windows;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NodeResizeEventArgs : RoutedEventArgs
	{
		protected NodeResizeEventArgs(RoutedEvent routedEvent, object source, ICollection nodes)
			: base(routedEvent, source)
		{
			Nodes = nodes;
		}

		public ICollection Nodes { get; }
	}

	public class NodeResizeStartedEventArgs : NodeResizeEventArgs
	{
		internal NodeResizeStartedEventArgs(RoutedEvent routedEvent, object source, ICollection nodes)
			: base(routedEvent, source, nodes)
		{
		}
	}

	public class NodeResizingEventArgs : NodeDragEventArgs
	{
		internal NodeResizingEventArgs(RoutedEvent routedEvent, object source, ICollection nodes, double horizontalChange, double verticalChange)
			: base(routedEvent, source, nodes)
		{
			HorizontalChange = horizontalChange;
			VerticalChange = verticalChange;
		}

		public double HorizontalChange { get; }
		public double VerticalChange { get; }
	}

	public class NodeResizeCompletedEventArgs : NodeResizeEventArgs
	{
		public NodeResizeCompletedEventArgs(RoutedEvent routedEvent, object source, ICollection nodes)
			: base(routedEvent, source, nodes)
		{
		}
	}
}
