using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	static class ExtensionMethods
	{
#pragma warning disable CS0618 // Type or member is obsolete
		public static AdornedControl FindShapeControl(this NetworkUserControl diagramControl, string shapeName)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return diagramControl.FindChildren<AdornedControl>().Single(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == shapeName);
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public static NodeItem FindNodeItem(this NetworkUserControl diagramControl, string shapeName)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return diagramControl.FindChildren<NodeItem>().Single(n => n.FindChildren<AdornedControl>().Any(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == shapeName));
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public static NodeViewModel FindShapeViewModel(this NetworkUserControl diagramControl, string shapeName)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return diagramControl.FindShapeControl(shapeName).GetViewModel();
		}

		public static NodeViewModel GetViewModel(this AdornedControl node)
		{
			return (NodeViewModel)node.DataContext;
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public static NodeItem FindNodeItemOrDefault(this NetworkUserControl diagramControl, string shapeName)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			return diagramControl.FindChildren<NodeItem>().SingleOrDefault(n => n.FindChildren<AdornedControl>().Any(x => x.DataContext is NodeViewModel && ((NodeViewModel)x.DataContext).Name == shapeName));
		}
	}
}
