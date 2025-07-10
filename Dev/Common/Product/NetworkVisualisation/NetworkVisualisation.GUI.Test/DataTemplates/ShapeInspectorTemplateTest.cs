using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class ShapeInspectorTemplateSelectorTest : DataTemplateSelectorTestBase
	{
		public void TestTemplateSelectorReturnsDefaultMessageTemplate()
		{
			var selector = new ShapeInspectorTemplateSelector();
			var expected = new DummyTemplate();
			selector.ShapeInspectorDefaultMessageTemplate = expected;

			AssertTemplateSelector(selector, expected, null);
		}

		public void TestTemplateSelectorReturnsAnnotationTemplate()
		{
			var selector = new ShapeInspectorTemplateSelector();
			var expected = new DummyTemplate();
			selector.ShapeInspectorAnnotationTemplate = expected;

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var entity = new Entity();
			var viewModel = new AnnotationViewModel(entity, networkViewModel);

			AssertTemplateSelector(selector, expected, viewModel);
		}

		public void TestTemplateSelectorReturnsShapeTemplate()
		{
			var selector = new ShapeInspectorTemplateSelector();
			var expected = new DummyTemplate();
			selector.ShapeInspectorShapeTemplate = expected;

			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var entity = new Entity();
			var viewModel = new NodeViewModel(entity, networkViewModel);

			AssertTemplateSelector(selector, expected, viewModel);
		}
	}
}
