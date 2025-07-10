using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class EntityTemplateSelectorTest : DataTemplateSelectorTestBase
	{
		public void TestTemplateSelectorReturnsLinkedEntityTemplate()
		{
			var selector = new EntityTemplateSelector();
			var expected = new DummyTemplate();
			selector.LinkedEntityTemplate = expected;

			var viewModel = new NodeViewModel();

			AssertTemplateSelector(selector, expected, viewModel);
		}

		public void TestTemplateSelectorReturnsNonLinkedEntityTemplate()
		{
			var selector = new EntityTemplateSelector();
			var expected = new DummyTemplate();
			selector.NonLinkedEntityTemplate = expected;

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var entity = new Entity();
			var viewModel = new AnnotationViewModel(entity, networkViewModel);

			AssertTemplateSelector(selector, expected, viewModel);
		}
	}
}
