using NUnit.Framework;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class CSharpHelperTest : TestCase
	{
		public void TestGetViewClassName()
		{
			modelViewContext.ModelName = "DummyName";

			AssertEquals("DummyName", CSharpHelper.GetViewClassName(modelViewContext));
		}

		public void TestGetFullViewClassName()
		{
			modelViewContext.DefinitionNamespace = $"{ModelViewConstants.NamespacePrefix}.Plus.Definition";
			modelViewContext.ModelName = "DummyName";

			AssertEquals("Plus.Definition.DummyName", CSharpHelper.GetFullViewClassName(modelViewContext));
		}

		public void TestGetIndexedViewClassName()
		{
			modelViewContext.ModelName = "DummyName";

			AssertEquals("DummyName_Idx", CSharpHelper.GetIndexedViewClassName(modelViewContext));
		}

		public void TestGetFullIndexedViewClassName()
		{
			modelViewContext.DefinitionNamespace = $"{ModelViewConstants.NamespacePrefix}.Plus.Definition";
			modelViewContext.ModelName = "DummyName";

			AssertEquals("Plus.Definition.DummyName_Idx", CSharpHelper.GetFullIndexedViewClassName(modelViewContext));
		}

		public void TestGetFullClassName()
		{
			modelViewContext.DefinitionNamespace = $"{ModelViewConstants.NamespacePrefix}.Plus.Definition";
			const string className = "DummyName";

			AssertEquals("Plus.Definition.DummyName", CSharpHelper.GetFullClassName(modelViewContext, className));
		}

		public void TestNamespaceSeparator()
		{
			AssertEquals(".", CSharpHelper.NamespaceSeparator);
		}

		protected override void SetUp()
		{
			base.SetUp();

			modelViewContext = new ModelViewContext();
		}

		ModelViewContext modelViewContext;
	}
}
