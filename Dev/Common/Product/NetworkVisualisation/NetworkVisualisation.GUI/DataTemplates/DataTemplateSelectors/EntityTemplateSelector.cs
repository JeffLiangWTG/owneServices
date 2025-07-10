using System.Windows;
using System.Windows.Controls;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class EntityTemplateSelector : DataTemplateSelector
	{
		public DataTemplate LinkedEntityTemplate { get; set; }
		public DataTemplate NonLinkedEntityTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var node = item as NodeViewModel;

			if (node != null)
			{
				if (node is AnnotationViewModel)
				{
					return NonLinkedEntityTemplate;
				}
				else
				{
					return LinkedEntityTemplate;
				}
			}
			return null;
		}
	}
}
