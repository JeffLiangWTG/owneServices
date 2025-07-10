using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class UsageCodeKeyTest : TestCase
	{
		public void TestToDatabaseText()
		{
			AssertEquals("STL-SHP", new UsageCodeKey("STL", "SHP").ToDatabaseText());
			AssertEquals("", new UsageCodeKey("", "").ToDatabaseText());
			AssertEquals("", new UsageCodeKey(null, "").ToDatabaseText());
			AssertEquals("", new UsageCodeKey(null, null).ToDatabaseText());
		}

		public void TestFromDatabaseText()
		{
			AssertEquals("STL", UsageCodeKey.FromDatabaseText("STL-SHP").Category);
			AssertEquals("SHP", UsageCodeKey.FromDatabaseText("STL-SHP").Code);

			AssertEquals("STL", UsageCodeKey.FromDatabaseText("STL-").Category);
			AssertEquals("STL", UsageCodeKey.FromDatabaseText("STL-").Code);

			AssertEquals("", UsageCodeKey.FromDatabaseText("-SHP").Category);
			AssertEquals("SHP", UsageCodeKey.FromDatabaseText("-SHP").Code);

			AssertEquals("", UsageCodeKey.FromDatabaseText("").Category);
			AssertEquals("", UsageCodeKey.FromDatabaseText("").Code);

			AssertEquals("", UsageCodeKey.FromDatabaseText(null).Category);
			AssertEquals("", UsageCodeKey.FromDatabaseText(null).Code);
		}

		public void TestCaseInsensitiveMethods()
		{
			var key1 = new UsageCodeKey("STL", "ABCD");
			var key2 = new UsageCodeKey("stl", "abcd");
			var key3 = new UsageCodeKey("StL", "aBcD");
			var key4 = new UsageCodeKey("STL", "ABCDE");

			Assert(key1.Equals(key2));
			Assert(key2.Equals(key3));
			Assert(!key3.Equals(key4));

			Assert(key1.EqualsIgnoringCase(key2));
			Assert(key2.EqualsIgnoringCase(key3));
			Assert(!key3.EqualsIgnoringCase(key4));

			AssertEquals(key1.GetHashCode(), key2.GetHashCode());
			AssertEquals(key2.GetHashCode(), key3.GetHashCode());
			AssertNotEquals(key3.GetHashCode(), key4.GetHashCode());

			var dic = new Dictionary<UsageCodeKey, string>();
			dic.Add(key1, "STL-ABC");
			dic.Add(key4, "STL-ABCD");

			AssertEquals("STL-ABC", dic[key1]);
			AssertEquals("STL-ABC", dic[key2]);
			AssertEquals("STL-ABC", dic[key3]);
			AssertEquals("STL-ABCD", dic[key4]);
		}
	}
}
