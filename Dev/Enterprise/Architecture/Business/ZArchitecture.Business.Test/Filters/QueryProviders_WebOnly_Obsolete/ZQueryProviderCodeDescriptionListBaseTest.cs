using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZQueryProviderCodeDescriptionListBaseTest : TestCase
	{
		#region AddInternal Overloads

		public void TestAddInternal_WithQueryProvider()
		{
			ZQueryProviderCodeDescriptionList list = new ZQueryProviderCodeDescriptionList();
			list.Add("Code", (NoResString)"Desc", new DummyQueryProvider());
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, new DummyQueryProvider());

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Operator when using default", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator when overriden", SQLComparisonOperator.EndsWith, list[1].Operator);
			AssertEquals("Correct QueryProvider type", typeof(DummyQueryProvider), list[0].QueryProviders[0].GetType());
		}

		public void TestAddInternal_WithSchemaColumnQueryProvider()
		{
			ZQueryProviderCodeDescriptionList list = new ZQueryProviderCodeDescriptionList();
			list.Add("Code", (NoResString)"Desc", OrgHeaderSchema.OH_Code);
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, OrgHeaderSchema.OH_Code);

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Operator when using default", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator when overriden", SQLComparisonOperator.EndsWith, list[1].Operator);

			IQueryProvider queryProvider = list[1].QueryProviders[0];
			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), queryProvider.GetType());
			ZQuery query = queryProvider.GetQuery(SQLComparisonOperator.EndsWith, "endswith");
			AssertEquals("Query provider should produce the correct query", "OH_Code like '%endswith'", query.LiteralTextADO);
		}

		public void TestAddInternal_WithDelegateQueryProvider()
		{
			ZQueryProviderCodeDescriptionList list = new ZQueryProviderCodeDescriptionList();
			list.Add("Code", (NoResString)"Desc", new AddToQueryDelegate(AddToQueryForTest));
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, new AddToQueryDelegate(AddToQueryForTest));

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Default Operator", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator Override", SQLComparisonOperator.EndsWith, list[1].Operator);

			IQueryProvider queryProvider = list[1].QueryProviders[0];
			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), queryProvider.GetType());
			ZQuery query = queryProvider.GetQuery(SQLComparisonOperator.EndsWith, "endswith");
			AssertEquals("Query provider should produce the correct query", "OH_Code like '%endswith'", query.LiteralTextADO);
		}

		void AddToQueryForTest(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(OrgHeaderSchema.OH_Code, @operator, value);
		}

		#endregion

		public void TestAddWithPropNamesOnly()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("x", (NoResString)"x", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			ZQueryProviderCodeDescription pair = list[0];
			ZQuery query = pair.QueryProviders[0].GetQuery(SQLComparisonOperator.Contains, "x");
			string filterString = query.LiteralTextADO;
			AssertEquals("First property filter correct", "Z0_Code like '%x%'", filterString);
			AssertEquals("Second property filter correct", "Z0_Description = 'y'", pair.QueryProviders[1].GetQuery(SQLComparisonOperator.Equal, "y").LiteralTextADO);
		}

		public void TestGetElementFromCode()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("x", (NoResString)"desc1", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			list.Add("y", (NoResString)"desc2", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			AssertEquals("Correct element retrieved", "desc1", list.GetElementFromCode("x").Description);
			AssertEquals("Correct element retrieved", "desc2", list.GetElementFromCode("y").Description);
		}

		public void TestAddQueryProviderComposition()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("x", (NoResString)"descx", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			list.Add("y", (NoResString)"descy", DummyBizoSchema.Z0_FK_Code, DummyBizoSchema.Z0_Bool);

			list.AddQueryProviderComposition("composite", (NoResString)"desccomp", 1, "x", "y");
			Assert("AddQueryProviderComposition", list[1].QueryProviders[0] is ZCompositeQueryProvider);
			Assert("AddQueryProviderComposition", list[1].QueryProviders[1] is ZCompositeQueryProvider);

			ZQuery query1 = list[1].QueryProviders[0].GetQuery(SQLComparisonOperator.Equal, "1");
			ZQuery query2 = list[1].QueryProviders[1].GetQuery(SQLComparisonOperator.Equal, "N");
			AssertEquals("AddQueryProviderComposition Query1", "Z0_Code = '1' or Z0_FK_Code = '1'", query1.LiteralTextADO);
			AssertEquals("AddQueryProviderComposition Query2", "Z0_Description = 'N' or Z0_Bool = 0", query2.LiteralTextADO);

			AssertEquals("descx (desccomp)", list[0].Description);
			AssertEquals("descy (desccomp)", list[2].Description);
		}

		public void TestAddQueryProviderCompositionForAll()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("x", (NoResString)"descx", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			list.Add("y", (NoResString)"descy", DummyBizoSchema.Z0_FK_Code, DummyBizoSchema.Z0_VarCharMax);

			list.AddQueryProviderCompositionForAll(1);
			Assert("AddQueryProviderComposition", list[1].QueryProviders[0] is ZCompositeQueryProvider);
			Assert("AddQueryProviderComposition", list[1].QueryProviders[1] is ZCompositeQueryProvider);

			ZQuery query1 = list[1].QueryProviders[0].GetQuery(SQLComparisonOperator.Equal, "1");
			ZQuery query2 = list[1].QueryProviders[1].GetQuery(SQLComparisonOperator.Equal, "2");
			AssertEquals("AddQueryProviderComposition Query1", "Z0_Code = '1' or Z0_FK_Code = '1'", query1.LiteralTextADO);
			AssertEquals("AddQueryProviderComposition Query2", "Z0_Description = '2' or Z0_VarCharMax = '2'", query2.LiteralTextADO);
		}

		public void TestQueryProviderCompositionIncludesIdentification()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("x", (NoResString)"descx", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			list.Add("y", (NoResString)"descy", DummyBizoSchema.Z0_FK_Code, DummyBizoSchema.Z0_Bool);

			list.AddQueryProviderComposition("composite", (NoResString)"desccomp", 1, "x", "y");

			AssertEquals("Composition name appended to descriptions", "descx (desccomp)", list.GetElementFromCode("x").Description);
		}

		public void TestRemoveCode()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("X", (NoResString)"descx", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			list.Add("y", (NoResString)"descy", DummyBizoSchema.Z0_FK_Code, DummyBizoSchema.Z0_Bool);
			AssertEquals("2 items in the list", 2, list.Count);

			list.RemoveCode("z");
			AssertEquals("still 2 items in the list", 2, list.Count);

			list.RemoveCode("x");
			AssertEquals("1 item left", 1, list.Count);
			AssertEquals("correct item left", "y", list[0].Code);
		}

		public void TestContainsCode()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("X", (NoResString)"descx", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			list.Add("y", (NoResString)"descy", DummyBizoSchema.Z0_FK_Code, DummyBizoSchema.Z0_Bool);

			AssertEquals("Should contain X", true, list.ContainsCode("X"));
			AssertEquals("Should contain y", true, list.ContainsCode("y"));
			AssertEquals("Does not contain Z", false, list.ContainsCode("Z"));
		}

		public void TestGetDescriptionFromCode()
		{
			ZQueryProviderCodeDescriptionListWith2FilterArguments list = new ZQueryProviderCodeDescriptionListWith2FilterArguments();
			list.Add("GMC", (NoResString)"Glen MacLarty", DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
			list.Add("AVE", (NoResString)"Alexander Verich", DummyBizoSchema.Z0_FK_Code, DummyBizoSchema.Z0_Bool);

			AssertEquals("Should return Description for GMC", "Glen MacLarty", list.GetDescriptionFromCode("GMC"));
			AssertEquals("Should return Description for AVE", "Alexander Verich", list.GetDescriptionFromCode("AVE"));
			AssertEquals("Should return empty string for missing code", "", list.GetDescriptionFromCode(""));
		}
	}
}
