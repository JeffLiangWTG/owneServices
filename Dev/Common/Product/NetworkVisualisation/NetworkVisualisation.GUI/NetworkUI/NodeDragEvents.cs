using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NodeDragEventArgs : RoutedEventArgs
	{
		protected NodeDragEventArgs(RoutedEvent routedEvent, object source, ICollection nodes)
			: base(routedEvent, source)
		{
			Nodes = nodes;
		}

		public ICollection Nodes { get; }
	}

	public class NodeDragStartedEventArgs : NodeDragEventArgs
	{
		internal NodeDragStartedEventArgs(RoutedEvent routedEvent, object source, ICollection nodes)
			: base(routedEvent, source, nodes)
		{
		}

		/// <summary>
		/// Set to 'false' to disallow dragging.
		/// </summary>
		public bool Cancel { get; set; }
	}

	[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void NodeDragStartedEventHandler(object sender, NodeDragStartedEventArgs e);

	public class NodeDraggingEventArgs : NodeDragEventArgs
	{
		internal NodeDraggingEventArgs(RoutedEvent routedEvent, object source, ICollection nodes, double horizontalChange, double verticalChange)
			: base(routedEvent, source, nodes)
		{
			HorizontalChange = horizontalChange;
			VerticalChange = verticalChange;
		}

		public double HorizontalChange { get; }
		public double VerticalChange { get; }
	}

	[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void NodeDraggingEventHandler(object sender, NodeDraggingEventArgs e);

	public class NodeDragCompletedEventArgs : NodeDragEventArgs
	{
		public NodeDragCompletedEventArgs(RoutedEvent routedEvent, object source, ICollection nodes)
			: base(routedEvent, source, nodes)
		{
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void NodeDragCompletedEventHandler(object sender, NodeDragCompletedEventArgs e);
}
