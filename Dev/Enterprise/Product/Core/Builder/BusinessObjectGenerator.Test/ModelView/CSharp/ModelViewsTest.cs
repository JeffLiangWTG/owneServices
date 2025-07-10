namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class ModelViewsTest : ModelViewAbstractTest<ModelViews>
	{
		protected override string ExpectedOutputResourceName => TestResources.SampleModelViews;

		protected override ModelViews GetCodeGeneratorForTest()
		{
			var allContext = ContextGenerator.GetAllCodeGeneratorContexts(EmbeddedResourceRetriever.DirectoryPath);
			return new ModelViews(allContext);
		}
	}
}
