using System.Collections.Generic;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class SqlIndexedViewTest : ModelViewAbstractTest<SqlIndexedView>
	{
		public void TestAddInfosToAdd()
		{
			var addInfo = new AddInfo
			{
				Name = "Z0_AddInfoString35",
				DataType = "String",
				Precision = null,
				Scale = null,
				MaxLength = 35,
				Indexed = false,
				IsUnicode = false
			};

			var indexedAddInfo = new AddInfo
			{
				Name = "Z0_IAddInfoString35",
				DataType = "String",
				Precision = null,
				Scale = null,
				MaxLength = 35,
				Indexed = true,
				IsUnicode = false
			};

			var view = new View { AddInfos = new List<AddInfo> { addInfo, indexedAddInfo } };

			AssertContainsExactElementsInAnyOrder(new List<AddInfo> { indexedAddInfo }, CodeGenerator.AddInfosToAdd(view));
		}

		public void TestGetJoinClause()
		{
			AssertEquals(string.Empty, CodeGenerator.GetJoinClause());
		}

		public void TestGetColumnDefinition()
		{
			var addInfo = new AddInfo
			{
				Name = "Z0_AddInfoString35",
				DataType = "String",
				Precision = null,
				Scale = null,
				MaxLength = 35,
				Indexed = false,
				IsUnicode = false
			};

			var addInfoIndexed = new AddInfo
			{
				Name = "Z0_AddInfoString35",
				DataType = "String",
				Precision = null,
				Scale = null,
				MaxLength = 35,
				Indexed = true,
				IsUnicode = false
			};

			var columnDefinition = CodeGenerator.GetColumnDefinition(addInfo, "Z0_AddInfo");
			var indexedColumnDefinition = CodeGenerator.GetColumnDefinition(addInfoIndexed, "Z0_AddInfo");

			const string expectedResult = "Z0_AddInfoString35 = CONVERT(VARCHAR(35), CASE WHEN CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 1) - (CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17)), '¤', '*') ELSE '' END)";

			AssertEquals(expectedResult, columnDefinition);
			AssertEquals(expectedResult, indexedColumnDefinition);
		}

		public void TestGetViewName()
		{
			ModelViewContextForTest.ModelName = "Sample";

			AssertEquals("Sample_Idx", CodeGenerator.ViewName);
		}

		public void TestGetColumnsClause()
		{
			ModelViewContextForTest.ModelView.AddInfos.RemoveAll(a => !a.Name.EndsWith("AddInfoDecimal122"));
			ModelViewContextForTest.Columns.Add("SampleCol");
			ModelViewContextForTest.Columns.Add("Z0_AddInfoDecimal122");
			ModelViewContextForTest.Columns.Add("Z0_IAddInfoDecimal122");
			var expectedResult = EmbeddedResourceRetriever.GetString(TestResources.IndexedViewSampleColumnsClause).Trim();

			var columnsClause = CodeGenerator.GetColumnsClause();

			AssertEquals(expectedResult, columnsClause);
		}

		public void TestGenerateCode_WhenThereIsNoIndex()
		{
			ModelViewContextForTest = ContextGenerator.GetCodeGeneratorContext(ModelViewWithoutIndexXmlFileName);
			var expectedViewCode = EmbeddedResourceRetriever.GetString(TestResources.SampleBizO.Index);

			var generatedView = CodeGenerator.SourceCode;

			AssertEquals(expectedViewCode, generatedView);
		}

		public void TestNeedGeneration()
		{
			Assert(CodeGenerator.NeedGeneration());

			ModelViewContextForTest = ContextGenerator.GetCodeGeneratorContext(ModelViewWithoutIndexXmlFileName);
			Assert(!CodeGenerator.NeedGeneration());
		}

		public void TestIncludeUnderlyingColumns()
		{
			Assert(!CodeGenerator.IncludeUnderlyingColumns);
		}

		public void TestQualifyColumns()
		{
			Assert(!CodeGenerator.QualifyColumns);
		}

		protected override string ExpectedOutputResourceName => TestResources.DummyBizO.Index;

		protected override SqlIndexedView GetCodeGeneratorForTest() => new SqlIndexedView(ModelViewContextForTest);
	}
}
