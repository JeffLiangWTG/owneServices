
namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class NonLinkedEntityContentTemplateSelectorTest : DataTemplateSelectorTestBase
	{
		public void TestTemplateSelectorReturnsAnnotation()
		{
			var selector = new NonLinkedEntityContentTemplateSelector();
			var expected = new DummyTemplate();
			selector.AnnotationNodeTemplate = expected;

			AssertTemplateSelector(selector, expected, null);
		}
	}
}
