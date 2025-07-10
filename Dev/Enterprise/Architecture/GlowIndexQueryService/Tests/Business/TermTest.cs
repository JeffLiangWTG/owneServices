using System;
using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests
{
	public class TermTest : TestCase
	{
		public void TestEmptySearchField()
		{
			AssertExceptionThrown<ArgumentException>(() => new Term(null, "HAY"));
			AssertExceptionThrown<ArgumentException>(() => new Term(string.Empty, "HAY"));
		}

		public void TestEmptyKeyword()
		{
			AssertNoExceptionThrown(() => new Term("Hello", null));
		}

		#region IEquatable tests

		public void TestEquals_ReturnsTrue_WhenObjectsAreEqual()
		{
			var term1 = new Term("field", "keyword");
			var term2 = new Term("field", "keyword");

			var result = term1.Equals(term2);

			Assert(result);
		}

		public void TestEquals_ReturnsFalse_WhenObjectsAreNotEqual()
		{
			var term1 = new Term("field1", "keyword1");
			var term2 = new Term("field2", "keyword2");

			var result = term1.Equals(term2);

			Assert(!result);
		}

		public void TestEquals_ReturnsFalse_WhenObjectIsNull()
		{
			var term = new Term("field", "keyword");

			var result = term.Equals(null);

			Assert(!result);
		}

		public void TestEquals_ReturnsTrue_WhenObjectIsSameInstance()
		{
			var term = new Term("field", "keyword");

			var result = term.Equals(term);

			Assert(result);
		}

		public void TestEquals_ReturnsFalse_WhenObjectIsOfDifferentType()
		{
			var term = new Term("field", "keyword");
			var otherObject = new object();

			var result = term.Equals(otherObject);

			Assert(!result);
		}

		public void TestGetHashCode_ReturnsSameValue_WhenObjectsAreEqual()
		{
			var term1 = new Term("field", "keyword");
			var term2 = new Term("field", "keyword");

			var hashCode1 = term1.GetHashCode();
			var hashCode2 = term2.GetHashCode();

			AssertEquals(hashCode1, hashCode2);
		}

		#endregion
	}
}
