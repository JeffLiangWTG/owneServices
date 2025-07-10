using System.Windows;
using System.Windows.Controls;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class LinkedEntityContentTemplateSelector : DataTemplateSelector
	{
		public DataTemplate LeafNodeTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var node = item as NodeViewModel;
			INetworkEntity nodeEntityLayout = null;
			DataTemplateSelector alternativeSelector = null;
			if (node != null)
			{
				nodeEntityLayout = node.Entity;
#pragma warning disable CS0618 // Disable CS0618
				alternativeSelector = ((NetworkUserControlViewModel)container?.FindParent<NetworkUserControl>()?.DataContext)?.TemplateSelector;
#pragma warning restore CS0618 // Restore CS0618
			}

			if (alternativeSelector != null)
			{
				var template = alternativeSelector.SelectTemplate(item, container);
				if (template != null)
				{
					return template;
				}
			}

			if (nodeEntityLayout != null)
			{
				return LeafNodeTemplate;
			}
			else
			{
				return null;
			}
		}
	}
}
