using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	public class EqualQueryTest : TestCase
	{
		public void TestText()
		{
			var term = new Term("city", "Boston");
			var equalQuery = new EqualQuery(term);
			AssertEquals("(city eq 'Boston')", equalQuery.ToUrlComponent());
		}

		public void TestUseQuotes()
		{
			var term = new Term("isActive", "true");
			var equalQuery = new EqualQuery(term, useQuotes: false);
			AssertEquals("(isActive eq true)", equalQuery.ToUrlComponent());
		}

		public void TestAllExact()
		{
			var term = new Term("city", "Boston");
			var equalQuery = new EqualQuery(term, useQuotes: true, allExact: true);
			AssertEquals("(city eq '\"Boston\"')", equalQuery.ToUrlComponent());
		}

		public void TestEmpty()
		{
			var term = new Term("city", "");
			var equalQuery = new EqualQuery(term);
			AssertEquals("", equalQuery.ToUrlComponent());
		}

		#region IEquatable tests

		public void TestEquals_ReturnsTrue_WhenEqualQueryIsComparedWithItself()
		{
			var term = new Term("keyword", "searchField");
			var equalQuery = new EqualQuery(term, true, false);

			var result = equalQuery.Equals(equalQuery);

			Assert(result);
		}

		public void TestEquals_ReturnsTrue_WhenEqualQueryIsComparedWithEqualQuery()
		{
			var term1 = new Term("keyword", "searchField");
			var equalQuery1 = new EqualQuery(term1, true, false);

			var term2 = new Term("keyword", "searchField");
			var equalQuery2 = new EqualQuery(term2, true, false);

			var result = equalQuery1.Equals(equalQuery2);

			Assert(result);
		}

		public void TestEquals_ReturnsFalse_WhenEqualQueryIsComparedWithNull()
		{
			var term = new Term("keyword", "searchField");
			var equalQuery = new EqualQuery(term, true, false);

			var result = equalQuery.Equals(null);

			Assert(!result);
		}

		public void TestEquals_ReturnsFalse_WhenEqualQueryIsComparedWithDifferentType()
		{
			var term = new Term("keyword", "searchField");
			var equalQuery = new EqualQuery(term, true, false);

			var otherObject = new object();

			var result = equalQuery.Equals(otherObject);

			Assert(!result);
		}

		public void TestEquals_ReturnsFalse_WhenEqualQueryHasDifferentTerm()
		{
			var term1 = new Term("keyword1", "searchField");
			var equalQuery1 = new EqualQuery(term1, true, false);

			var term2 = new Term("keyword2", "searchField");
			var equalQuery2 = new EqualQuery(term2, true, false);

			var result = equalQuery1.Equals(equalQuery2);

			Assert(!result);
		}

		public void TestEquals_ReturnsFalse_WhenEqualQueryHasDifferentUseQuotes()
		{
			var term1 = new Term("keyword", "searchField");
			var equalQuery1 = new EqualQuery(term1, true, false);

			var term2 = new Term("keyword", "searchField");
			var equalQuery2 = new EqualQuery(term2, false, false);

			var result = equalQuery1.Equals(equalQuery2);

			Assert(!result);
		}

		public void TestEquals_ReturnsFalse_WhenEqualQueryHasDifferentAllExact()
		{
			var term1 = new Term("keyword", "searchField");
			var equalQuery1 = new EqualQuery(term1, true, false);

			var term2 = new Term("keyword", "searchField");
			var equalQuery2 = new EqualQuery(term2, true, true);

			var result = equalQuery1.Equals(equalQuery2);

			Assert(!result);
		}

		public void TestGetHashCode_ReturnsSameValue_WhenEqualQueryIsComparedWithItself()
		{
			var term = new Term("keyword", "searchField");
			var equalQuery = new EqualQuery(term, true, false);

			var hashCode1 = equalQuery.GetHashCode();
			var hashCode2 = equalQuery.GetHashCode();

			AssertEquals(hashCode1, hashCode2);
		}

		public void TestGetHashCode_ReturnsSameValue_WhenEqualQueryIsComparedWithEqualQuery()
		{
			var term1 = new Term("keyword", "searchField");
			var equalQuery1 = new EqualQuery(term1, true, false);

			var term2 = new Term("keyword", "searchField");
			var equalQuery2 = new EqualQuery(term2, true, false);

			var hashCode1 = equalQuery1.GetHashCode();
			var hashCode2 = equalQuery2.GetHashCode();

			AssertEquals(hashCode1, hashCode2);
		}

		#endregion
	}
}
