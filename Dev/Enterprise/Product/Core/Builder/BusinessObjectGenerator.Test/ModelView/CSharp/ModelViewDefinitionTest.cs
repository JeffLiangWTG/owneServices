namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class ModelViewDefinitionTest : ModelViewAbstractTest<ModelViewDefinition>
	{
		public void TestIndexedViewClassShouldNotBeCreated_WhenHasNoIndex()
		{
			ModelViewContextForTest = ContextGenerator.GetCodeGeneratorContext(ModelViewWithoutIndexXmlFileName);
			var expectedCode = EmbeddedResourceRetriever.GetString(TestResources.SampleBizO.Cs);

			var generatedView = CodeGenerator.SourceCode;

			AssertEquals(expectedCode, generatedView);
		}

		protected override string ExpectedOutputResourceName => TestResources.DummyBizO.Cs;

		protected override ModelViewDefinition GetCodeGeneratorForTest()
		{
			return new ModelViewDefinition(ModelViewContextForTest);
		}
	}
}
