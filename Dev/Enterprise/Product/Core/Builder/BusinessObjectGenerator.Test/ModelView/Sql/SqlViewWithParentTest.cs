namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class SqlViewWithParentTest : ModelViewAbstractTest<SqlView>
	{
		public void TestGetColumnsClause()
		{
			ModelViewContextForTest.Columns.Add("SampleCol");
			ModelViewContextForTest.Columns.Add("Z0_AddInfoDecimal122");
			ModelViewContextForTest.Columns.Add("Z0_IAddInfoDecimal122");
			var expectedResult = EmbeddedResourceRetriever.GetString(TestResources.SampleColumnsClauseWithParent).Trim();

			var columnsClause = CodeGenerator.GetColumnsClause();
			AssertEquals(expectedResult, columnsClause);
		}

		protected override string ExpectedOutputResourceName => TestResources.DummyBizOWithParent.Model;

		protected override SqlView GetCodeGeneratorForTest() => new SqlView(ModelViewContextForTest);

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
