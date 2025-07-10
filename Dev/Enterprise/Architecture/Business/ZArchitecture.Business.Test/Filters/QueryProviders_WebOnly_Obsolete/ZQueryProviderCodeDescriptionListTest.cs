using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZQueryProviderCodeDescriptionListTest : TestCase
	{
		#region Add Overloads

		public void TestAdd_WithQueryProvider()
		{
			ZQueryProviderCodeDescriptionList list = new ZQueryProviderCodeDescriptionList();
			list.Add("Code", (NoResString)"Desc", new DummyQueryProvider());
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, new DummyQueryProvider());

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Operator when using default", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator when overriden", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("Only 1 query provider should be produced", 1, list[0].QueryProviders.Length);
			AssertEquals("Correct QueryProvider type", typeof(DummyQueryProvider), list[0].QueryProviders[0].GetType());
		}

		public void TestAdd_WithSchemaColumnQueryProvider()
		{
			ZQueryProviderCodeDescriptionList list = new ZQueryProviderCodeDescriptionList();
			list.Add("Code", (NoResString)"Desc", OrgHeaderSchema.OH_Code);
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, OrgHeaderSchema.OH_Code);

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Operator when using default", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator when overriden", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("Only 1 query provider should be produced", 1, list[0].QueryProviders.Length);
			IQueryProvider queryProvider = list[0].QueryProviders[0];
			AssertEquals("Correct QueryProvider type", typeof(ZComparisonQueryProvider), queryProvider.GetType());
			ZQuery query = queryProvider.GetQuery(SQLComparisonOperator.EndsWith, "endswith");
			AssertEquals("Query provider should produce the correct query", "OH_Code like '%endswith'", query.LiteralTextADO);
		}

		public void TestAdd_WithDelegateQueryProvider()
		{
			ZQueryProviderCodeDescriptionList list = new ZQueryProviderCodeDescriptionList();
			list.Add("Code", (NoResString)"Desc", new AddToQueryDelegate(AddToQueryForTest));
			list.Add("Code", (NoResString)"Desc", SQLComparisonOperator.EndsWith, new AddToQueryDelegate(AddToQueryForTest));

			AssertEquals("Code", "Code", list[0].Code);
			AssertEquals("Description", "Desc", list[0].Description);
			AssertEquals("Operator when using default", SQLComparisonOperator.NotSpecified, list[0].Operator);
			AssertEquals("Operator when overriden", SQLComparisonOperator.EndsWith, list[1].Operator);

			AssertEquals("Only 1 query provider should be produced", 1, list[0].QueryProviders.Length);
			IQueryProvider queryProvider = list[0].QueryProviders[0];
			AssertEquals("Correct QueryProvider type", typeof(ZDelegateQueryProvider), queryProvider.GetType());
			ZQuery query = queryProvider.GetQuery(SQLComparisonOperator.EndsWith, "endswith");
			AssertEquals("Query provider should produce the correct query", "OH_Code like '%endswith'", query.LiteralTextADO);
		}

		void AddToQueryForTest(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(OrgHeaderSchema.OH_Code, @operator, value);
		}

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
