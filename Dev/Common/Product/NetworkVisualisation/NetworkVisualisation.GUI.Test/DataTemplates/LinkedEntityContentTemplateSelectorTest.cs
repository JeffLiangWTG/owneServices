using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class LinkedEntityContentTemplateSelectorTest : DataTemplateSelectorTestBase
	{
		public void TestTemplateSelector_ReturnsLeafNode()
		{
			var selector = new LinkedEntityContentTemplateSelector();
			var expected = new DummyTemplate();
			selector.LeafNodeTemplate = expected;

			var entity = new Entity();
			var viewModel = new NodeViewModel(entity, new NetworkViewModel(new DummyNetwork(), null));

			AssertTemplateSelector(selector, expected, viewModel);
		}

		public void TestTemplateSelector_ReturnsNull_WhenUnsure()
		{
			var selector = new LinkedEntityContentTemplateSelector();
			var expected = new DummyTemplate();
			selector.LeafNodeTemplate = expected;

			var template = selector.SelectTemplate(new object(), null);

			AssertNull(template);
		}

#pragma warning disable CS0618
		public void TestTemplateSelector_ReturnsTemplateFromSubstituteSelector()
		{
			var selector = new LinkedEntityContentTemplateSelector();
			var linkedEntityTemplate = new DummyTemplate();
			selector.LeafNodeTemplate = linkedEntityTemplate;

			var alternativeSelector = new AlternativeTemplateSelector();
			var alternativeSelectorTemplate = new DifferentDummyTemplate();
			alternativeSelector.Template = alternativeSelectorTemplate;

			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				config.NetworkUserControlViewModel.TemplateSelector = alternativeSelector;
				var entity = new Entity();
				var nodeViewModel = ((DiagramAreaUserControlViewModel)config.Control.MainDiagramControl.DataContext).CreateNewNode(new Location(), entity);

				AssertTemplateSelector(selector, alternativeSelectorTemplate, nodeViewModel, config.Control.FindChildren<DiagramAreaUserControl>().First());
			}
		}
#pragma warning restore CS0618

		class AlternativeTemplateSelector : DataTemplateSelector
		{
			public DataTemplate Template { get; set; }

			public override DataTemplate SelectTemplate(object item, DependencyObject container)
			{
				return Template;
			}
		}
	}
}
