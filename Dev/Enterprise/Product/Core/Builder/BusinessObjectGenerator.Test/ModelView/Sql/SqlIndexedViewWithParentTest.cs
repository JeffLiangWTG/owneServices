namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class SqlIndexedViewWithParentTest : ModelViewAbstractTest<SqlIndexedView>
	{
		protected override string ExpectedOutputResourceName => TestResources.DummyBizOWithParent.Index;

		protected override SqlIndexedView GetCodeGeneratorForTest() => new SqlIndexedView(ModelViewContextForTest);

		protected override void SetUp()
		{
			base.SetUp();
			ContextGenerator = new ModelViewContextGenerator(EmbeddedResourceRetriever.DirectoryPath);
			var parentContext = ContextGenerator.GetCodeGeneratorContext(modelViewXmlFileName);
			parentContext.ModelView.AddInfos.RemoveAll(a => !a.Name.EndsWith("AddInfoString35"));
			ModelViewContextForTest.ModelView.ParentContext = parentContext;
			ModelViewContextForTest.ModelView.AddInfos.RemoveAll(a => !a.Name.EndsWith("AddInfoDecimal122"));
		}
	}
}
