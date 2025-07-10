using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(EDIMessageTextModuleFilter))]
	sealed class EDIMessageTextFilterTest : ModuleTextFilterTest
	{
		public void TestOverrides()
		{
			var filter = new EDIMessageTextModuleFilter("AAA", string.Empty);
			var operators = filter.AllowedComparisonOperators;
			AssertEquals("AllowedComparisonOperators.Length", 7, operators.Count);
			AssertEquals("Empty", true, operators.Contains(string.Empty));
			AssertEquals("Exact", true, operators.Contains(EDIMessageTextModuleFilter.ComparisonConstants.Exact));
			AssertEquals("StartsWith", true, operators.Contains(EDIMessageTextModuleFilter.ComparisonConstants.StartsWith));
			AssertEquals("Contains", true, operators.Contains(EDIMessageTextModuleFilter.ComparisonConstants.Contains));
			AssertEquals("NotEqual", true, operators.Contains(EDIMessageTextModuleFilter.ComparisonConstants.NotEqual));
			AssertEquals("NotStartsWith", true, operators.Contains(EDIMessageTextModuleFilter.ComparisonConstants.NotStartsWith));
			AssertEquals("NotContain", true, operators.Contains(EDIMessageTextModuleFilter.ComparisonConstants.NotContain));

			AssertEquals("IsExpensiveQuery", false, filter.IsExpensiveQuery);
		}

		public void TestSegmentQuery()
		{
			const string segmentPattern = "LOC+22+____:129::{0}'";
			const string description = "Warehouse Code";

			var filter = new EDIMessageTextModuleFilter(description, segmentPattern, 4);
			AssertEquals("Description", description, filter.Description);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "1234";
			AssertEquals("Db only query", true, filter.Query is ZDBOnlyQuery);
			AssertEquals("Exact query", "EM_MessageText LIKE '%LOC+22+____:129::1234''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			AssertEquals("Not Equal query", "EM_MessageText NOT LIKE '%LOC+22+____:129::1234''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.StartsWith;
			filter.Property = "12";
			AssertEquals("Exact length Starts With query", "EM_MessageText LIKE '%LOC+22+____:129::12__''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotStartsWith;
			AssertEquals("Exact length Not Starts With query", "EM_MessageText NOT LIKE '%LOC+22+____:129::12__''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Contains;
			AssertEquals("Contains query", "EM_MessageText LIKE '%LOC+22+____:129::%12%''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotContain;
			AssertEquals("Not Contains query", "EM_MessageText NOT LIKE '%LOC+22+____:129::%12%''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter = new EDIMessageTextModuleFilter(description, segmentPattern);
			filter.Property = "12";
			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.StartsWith;
			AssertEquals("Various length Starts With query", "EM_MessageText LIKE '%LOC+22+____:129::12%''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotStartsWith;
			AssertEquals("Various length Not Starts With query", "EM_MessageText NOT LIKE '%LOC+22+____:129::12%''%'\r\n", filter.Query.LiteralTextSqlFormatted);

			filter.Property = "%Hel~lo%";
			AssertEquals("Escaped Sql Value", "EM_MessageText NOT LIKE '%LOC+22+____:129::~%Hel~~lo~%%''%'\r\n", filter.Query.LiteralTextSqlFormatted);
		}

		public void TestMultipleSegmentQuery()
		{
			var filter = new EDIMessageTextModuleFilter("Transaction Number", new[] { "BGM+:::%+{0}+11'", "RFF+TN:{0}'" }, 14);
			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.Exact;
			filter.Property = "12345000012345";

			var expected = "(\r\n\tEM_MessageText LIKE '%BGM+:::%+12345000012345+11''%'\r\n)\r\nOR\r\n(\r\n\tEM_MessageText LIKE '%RFF+TN:12345000012345''%'\r\n)\r\n";
			AssertEquals("Exact query", expected, filter.Query.LiteralTextSqlFormatted);

			filter.ComparisonOperator = EDIMessageTextModuleFilter.ComparisonConstants.NotEqual;
			expected = "(\r\n\tEM_MessageText NOT LIKE '%BGM+:::%+12345000012345+11''%'\r\n)\r\nAND\r\n(\r\n\tEM_MessageText NOT LIKE '%RFF+TN:12345000012345''%'\r\n)\r\n";
			AssertEquals("Not Equal query", expected, filter.Query.LiteralTextSqlFormatted);
		}

		public override void TestIsExpensiveQuery()
		{
			Assert(!Filter.IsExpensiveQuery);
		}

		public override void TestSqlComparisonOperator()
		{
			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals(Filter.GetComparisonOperatorDefault(), Filter.ComparisonOperator);

			Filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals(Filter.GetComparisonOperatorDefault(), Filter.ComparisonOperator);
		}

		protected override ModuleTextFilter GetNewModuleFilter() => new EDIMessageTextModuleFilter("moo", string.Empty);
	}
}
