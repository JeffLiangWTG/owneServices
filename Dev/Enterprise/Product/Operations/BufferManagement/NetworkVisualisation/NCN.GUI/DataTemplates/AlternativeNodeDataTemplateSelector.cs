using System.Windows;
using System.Windows.Controls;
using CargoWise.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class AlternativeNodeDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate BufferDataTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			var nodeViewModel = (NodeViewModel)item;
			var shape = nodeViewModel.Entity.AsShape();

			return shape.IsBufferShape ? BufferDataTemplate : null;
			// Always return null if not template is suitable so that base selector can handle in networkvisualisation
		}
	}
}
