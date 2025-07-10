using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	public class AlternativeNodeDataTemplateSelectorTest : NetworkTestCase
	{
		public void TestReturnsBuffer()
		{
			var selector = new AlternativeNodeDataTemplateSelector();
			var expected = new DummyTemplate();
			selector.BufferDataTemplate = expected;

			var entity = CreateShape(ShapeTypeList.Codes.Buffer);
			var viewModel = new NodeViewModel(entity, null);

			var template = selector.SelectTemplate(viewModel, null);

			AssertNotNull(template);
			AssertEquals(expected, template);
		}

		public void TestReturnsNullOnUnHandledCondition()
		{
			var selector = new AlternativeNodeDataTemplateSelector();
			var expected = new DummyTemplate();
			selector.BufferDataTemplate = expected;

			var entity = CreateDiagram(Factory);
			var viewModel = new NodeViewModel(entity, null);

			var template = selector.SelectTemplate(viewModel, null);

			AssertNull(template);
		}

		protected class DummyTemplate : DataTemplate
		{
		}
	}
}
