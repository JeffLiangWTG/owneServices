using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	public class UniqueListTest : TestCase
	{
		public void TestAddUniqueList()
		{
			UniqueList<string> collection = new UniqueList<string>();
			collection.Add("Value1");
			collection.Add("Value1");
			collection.Add("Value1");
			collection.Add("Value1");
			collection.Add("Value2");
			collection.Add("Value3");
			Assert("Correct count", collection.Count == 3);
			string[] expectedStringArray = new string[] { "Value1", "Value2", "Value3" };
			string[] actualStringArray = collection.ToArray();
			CombineAssertions(delegate
			{
				AssertContentsEquals("ToArray", expectedStringArray, actualStringArray);
			});
		}

		void AssertContentsEquals(string message, string[] expectedStringArray, string[] actualStringArray)
		{
			for (int idx = 0; idx < 3; idx++)
			{
				AssertEquals(string.Format("{0}[{1}]", message, idx), expectedStringArray[idx], actualStringArray[idx]);
			}
		}

		public void TestCtor()
		{
			UniqueList<string> a = new UniqueList<string>();
			Assert("Empty", a.Count.Equals(0));
			a = new UniqueList<string>(3);
			Assert("Empty", a.Count.Equals(0));
			a.Add("test");
			UniqueList<string> b = new UniqueList<string>(a);
			Assert("1 item", b.Count.Equals(1));
			List<String> duplicateList = new List<string>(4);
			duplicateList.Add("abc");
			duplicateList.Add("abc");
			duplicateList.Add("efg");
			duplicateList.Add("xyz");
			AssertEquals("Precondition - has 4 elements.", 4, duplicateList.Count);
			a = new UniqueList<string>(duplicateList);
			AssertEquals("Only has unique elements.", 3, a.Count);
		}

		public void TestForEach()
		{
			UniqueList<String> a = new UniqueList<string>();
			a.Add("abc");
			a.Add("efg");
			a.Add("xyz");
			String result = String.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("Unchanged item 0", "abc", a[0]);
				AssertEquals("Unchanged item 1", "efg", a[1]);
				AssertEquals("Unchanged item 2", "xyz", a[2]);
				a.ForEach(i => result += i.ToUpper());
				AssertEquals("ForEach outcome", "ABCEFGXYZ", result);
			});
		}
	}
}