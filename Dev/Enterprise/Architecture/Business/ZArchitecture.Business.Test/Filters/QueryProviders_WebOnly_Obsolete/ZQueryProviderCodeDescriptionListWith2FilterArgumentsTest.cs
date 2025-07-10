using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZQueryProviderCodeDescriptionListWith2FilterArgumentsTest : TestCase
	{
		#region Add Overloads

		public void TestAdd_WithQueryProvider()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("Code", (NoResString)"Desc", new DummyQueryProvider1(), new DummyQueryProvider2());
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, new DummyQueryProvider1(), new DummyQueryProvider2());

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Default Operator", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator Override", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("2 query providers should be produced", 2, list[0].QueryProviders.Length);
			AssertEquals("Correct QueryProvider type", typeof(DummyQueryProvider1), list[0].QueryProviders[0].GetType());
			AssertEquals("Correct QueryProvider type", typeof(DummyQueryProvider2), list[0].QueryProviders[1].GetType());
		}

		public void TestAdd_WithSchemaColumnQueryProvider()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("Code", (NoResString)"Desc", OrgHeaderSchema.OH_Code, OrgHeaderSchema.OH_Code);
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, OrgHeaderSchema.OH_Code, OrgHeaderSchema.OH_FullName);

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Default Operator", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator Override", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("2 query providers should be produced", 2, list[0].QueryProviders.Length);

			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), list[0].QueryProviders[0].GetType());
			ZQuery query1 = list[1].QueryProviders[0].GetQuery(SQLComparisonOperator.EndsWith, "endswth1");
			AssertEquals("Query provider should produce appropriate query", "OH_Code like '%endswth1'", query1.LiteralTextADO);
			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), list[0].QueryProviders[1].GetType());
			ZQuery query2 = list[1].QueryProviders[1].GetQuery(SQLComparisonOperator.EndsWith, "endswth2");
			AssertEquals("Query provider should produce appropriate query", "OH_FullName like '%endswth2'", query2.LiteralTextADO);
		}

		public void TestAdd_WithDelegateQueryProvider()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("Code", (NoResString)"Desc", new AddToQueryDelegate(AddToQueryForTest1), new AddToQueryDelegate(AddToQueryForTest2));
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, new AddToQueryDelegate(AddToQueryForTest1), new AddToQueryDelegate(AddToQueryForTest2));

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Default Operator", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator Override", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("2 query providers should be produced", 2, list[0].QueryProviders.Length);

			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), list[1].QueryProviders[0].GetType());
			ZQuery query1 = list[1].QueryProviders[0].GetQuery(SQLComparisonOperator.EndsWith, "endswth1");
			AssertEquals("Query provider should produce appropriate query", "OH_Code like '%endswth1'", query1.LiteralTextADO);
			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), list[1].QueryProviders[1].GetType());
			ZQuery query2 = list[1].QueryProviders[1].GetQuery(SQLComparisonOperator.EndsWith, "endswth2");
			AssertEquals("Query provider should produce appropriate query", "OH_FullName like '%endswth2'", query2.LiteralTextADO);
		}

		public void TestAdd_With2SameQueryProviders()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			DummyQueryProvider1 queryProvider1 = new DummyQueryProvider1();
			DummyQueryProvider2 queryProvider2 = new DummyQueryProvider2();
			list.Add("Code", (NoResString)"Desc", queryProvider1);
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, queryProvider2);

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Default Operator", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator Override", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("2 query providers should be produced", 2, list[0].QueryProviders.Length);
			AssertEquals("Correct QueryProvider", queryProvider1, list[0].QueryProviders[0]);
			AssertEquals("Correct QueryProvider", queryProvider1, list[0].QueryProviders[1]);

			AssertEquals("2 query providers should be produced", 2, list[1].QueryProviders.Length);
			AssertEquals("Correct QueryProvider", queryProvider2, list[1].QueryProviders[0]);
			AssertEquals("Correct QueryProvider", queryProvider2, list[1].QueryProviders[1]);
		}

		public void TestAdd_With2SameSchemaColumnQueryProviders()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("Code", (NoResString)"Desc", OrgHeaderSchema.OH_Code);
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, OrgHeaderSchema.OH_FullName);

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Default Operator", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator Override", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("2 query providers should be produced", 2, list[0].QueryProviders.Length);
			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), list[0].QueryProviders[0].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_Code like '%code%'", list[0].QueryProviders[0].GetQuery(SQLComparisonOperator.Contains, "code").LiteralTextADO);
			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), list[0].QueryProviders[1].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_Code like '%code%'", list[0].QueryProviders[1].GetQuery(SQLComparisonOperator.Contains, "code").LiteralTextADO);

			AssertEquals("2 query providers should be produced", 2, list[1].QueryProviders.Length);
			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), list[1].QueryProviders[0].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_FullName = 'name'", list[1].QueryProviders[0].GetQuery(SQLComparisonOperator.Equal, "name").LiteralTextADO);
			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), list[1].QueryProviders[1].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_FullName = 'name'", list[1].QueryProviders[1].GetQuery(SQLComparisonOperator.Equal, "name").LiteralTextADO);
		}

		public void TestAdd_With2SameDelegateQueryProviders()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("Code", (NoResString)"Desc", AddToQueryForTest1);
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, AddToQueryForTest2);

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Default Operator", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator Override", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("2 query providers should be produced", 2, list[0].QueryProviders.Length);
			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), list[0].QueryProviders[0].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_Code like '%code'", list[0].QueryProviders[0].GetQuery(SQLComparisonOperator.EndsWith, "code").LiteralTextADO);
			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), list[0].QueryProviders[1].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_Code like '%code'", list[0].QueryProviders[1].GetQuery(SQLComparisonOperator.EndsWith, "code").LiteralTextADO);

			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), list[1].QueryProviders[0].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_FullName like 'name%'", list[1].QueryProviders[0].GetQuery(SQLComparisonOperator.StartsWith, "name").LiteralTextADO);
			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), list[1].QueryProviders[1].GetType());
			AssertEquals("Query provider should produce appropriate query", "OH_FullName like 'name%'", list[1].QueryProviders[1].GetQuery(SQLComparisonOperator.StartsWith, "name").LiteralTextADO);
		}

		void AddToQueryForTest1(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(OrgHeaderSchema.OH_Code, @operator, value);
		}

		void AddToQueryForTest2(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(OrgHeaderSchema.OH_FullName, @operator, value);
		}

		class DummyQueryProvider1 : DummyQueryProvider { }
		class DummyQueryProvider2 : DummyQueryProvider { }

		#endregion

		public void TestAddEmptySelection()
		{
			List.AddEmptySelection();

			AssertEquals("Has one elemet", 1, List.Count);
			AssertEquals("Element code is None", "None", List[0].Code);
			AssertEquals("Element desc is None", "None", List[0].Description);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			List = new ZQueryProviderCodeDescriptionList();
		}

		ZQueryProviderCodeDescriptionList List;

		#endregion
	}
}
