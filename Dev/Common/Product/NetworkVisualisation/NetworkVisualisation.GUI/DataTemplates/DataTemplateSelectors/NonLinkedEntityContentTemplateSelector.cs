using System.Windows;
using System.Windows.Controls;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NonLinkedEntityContentTemplateSelector : DataTemplateSelector
	{
		public DataTemplate AnnotationNodeTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			return AnnotationNodeTemplate;
		}
	}
}
