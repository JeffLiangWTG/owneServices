using System.Collections.Generic;

namespace Enterprise.BusinessObjectGenerator.ModelView.Testing
{
	sealed class SqlViewTest : ModelViewAbstractTest<SqlView>
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

			var addInfos = new List<AddInfo> { addInfo, indexedAddInfo };
			var view = new View { AddInfos = addInfos };

			AssertContainsExactElementsInAnyOrder(addInfos, CodeGenerator.AddInfosToAdd(view));
		}

		public void TestGetColumnsClause()
		{
			ModelViewContextForTest.ModelView.AddInfos.RemoveAll(a => !a.Name.EndsWith("AddInfoDecimal122"));
			ModelViewContextForTest.Columns.Add("SampleCol");
			ModelViewContextForTest.Columns.Add("Z0_AddInfoDecimal122");
			ModelViewContextForTest.Columns.Add("Z0_IAddInfoDecimal122");
			var expectedResult = EmbeddedResourceRetriever.GetString(TestResources.SampleColumnsClause).Trim();

			var columnsClause = CodeGenerator.GetColumnsClause();
			AssertEquals(expectedResult, columnsClause);
		}

		public void TestGetJoinClause()
		{
			ModelViewContextForTest.ClusterKeyColumnName = null;
			var expectedValue = EmbeddedResourceRetriever.GetString(TestResources.SampleJoinClause.WithoutClusterKey).Trim();
			var joinClause = CodeGenerator.GetJoinClause().Trim();

			AssertEquals(expectedValue, joinClause);

			ModelViewContextForTest.ClusterKeyColumnName = "CLSTRKEY";
			expectedValue = EmbeddedResourceRetriever.GetString(TestResources.SampleJoinClause.WithClusterKey).Trim();
			joinClause = CodeGenerator.GetJoinClause().Trim();

			AssertEquals(expectedValue, joinClause);
		}

		public void TestGetWhereClause()
		{
			CombineAssertions(() =>
			{
				AssertWhereClause(null, "");
				AssertWhereClause("", "");
				AssertWhereClause(" ", "");
				AssertWhereClause(" [Z0_Code]='ZZ' ", "WHERE [Z0_Code]='ZZ'");
				AssertWhereClause("[Z0_Code]='ZZ'", "WHERE [Z0_Code]='ZZ'");
			});
		}

		void AssertWhereClause(string searchCondition, string expectedResult)
		{
			var codeGenerator = CodeGenerator;

			var modelView = new View { SearchCondition = searchCondition };
			var whereClause = codeGenerator.GetWhereClause(modelView);
			AssertEquals(expectedResult, whereClause);
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

			var columnDefinitionExpected =
				"Z0_AddInfoString35 = CONVERT(VARCHAR(35), CASE WHEN CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) > 0 THEN REPLACE(SUBSTRING('*'+Z0_AddInfo, CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17, CHARINDEX('*', '*'+Z0_AddInfo+'*', CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 1) - (CHARINDEX('*AddInfoString35=', '*'+Z0_AddInfo) + 17)), '¤', '*') ELSE '' END)";
			var indexedColumnDefinitionExpected = "I.Z0_AddInfoString35";

			AssertEquals(columnDefinitionExpected, columnDefinition);
			AssertEquals(indexedColumnDefinitionExpected, indexedColumnDefinition);
		}

		public void TestGetViewName()
		{
			ModelViewContextForTest.ModelName = "Sample";

			AssertEquals("Sample", CodeGenerator.ViewName);
		}

		public void TestGetTableName()
		{
			var view = new View { Table = "SampleTable" };
			ModelViewContextForTest.ModelView = view;

			AssertEquals("SampleTable", CodeGenerator.TableName);
		}

		public void TestGetIndexName()
		{
			ModelViewContextForTest.ModelName = "Sample";

			AssertEquals("Sample_Idx", CodeGenerator.IndexedViewName);
		}

		public void TestTableAlias()
		{
			AssertEquals("T", SqlView.TableAlias);
		}

		public void TestIndexAlias()
		{
			AssertEquals("I", SqlView.IndexedViewAlias);
		}

		public void TestIncludeUnderlyingColumns()
		{
			Assert(CodeGenerator.IncludeUnderlyingColumns);
		}

		public void TestQualifyColumns()
		{
			Assert(CodeGenerator.QualifyColumns);
		}

		public void TestNeedGeneration()
		{
			Assert(CodeGenerator.NeedGeneration());

			ModelViewContextForTest.ModelView.AddInfos.Clear();
			Assert(!CodeGenerator.NeedGeneration());
		}

		protected override string ExpectedOutputResourceName => TestResources.DummyBizO.Model;

		protected override SqlView GetCodeGeneratorForTest() => new SqlView(ModelViewContextForTest);
	}
}
