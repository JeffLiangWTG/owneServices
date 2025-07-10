using System.Windows;
using System.Windows.Controls;
using CargoWise.NetworkVisualisation.Business;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public abstract class DataTemplateSelectorTestBase : TestCase
	{
		protected void AssertTemplateSelector(DataTemplateSelector selector, DataTemplate expectedTemplate, NodeViewModel viewModel, DependencyObject container = null)
		{
			var template = selector.SelectTemplate(viewModel, container);

			AssertNotNull(template);
			AssertEquals(expectedTemplate, template);
			AssertType(expectedTemplate.GetType(), template);
		}

		protected class DummyTemplate : DataTemplate
		{
		}

		protected class DifferentDummyTemplate : DataTemplate
		{
		}
	}
}
