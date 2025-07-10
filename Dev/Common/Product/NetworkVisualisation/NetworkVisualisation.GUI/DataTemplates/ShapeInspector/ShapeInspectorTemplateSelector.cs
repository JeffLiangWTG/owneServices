using System.Windows;
using System.Windows.Controls;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class ShapeInspectorTemplateSelector : DataTemplateSelector
	{
		public DataTemplate ShapeInspectorDefaultMessageTemplate { get; set; }

		public DataTemplate ShapeInspectorAnnotationTemplate { get; set; }

		public DataTemplate ShapeInspectorShapeTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			if (item == null)
			{
				return ShapeInspectorDefaultMessageTemplate;
			}

			var model = item as NodeViewModel;
			return (model is AnnotationViewModel) ? ShapeInspectorAnnotationTemplate : ShapeInspectorShapeTemplate;
		}
	}
}
