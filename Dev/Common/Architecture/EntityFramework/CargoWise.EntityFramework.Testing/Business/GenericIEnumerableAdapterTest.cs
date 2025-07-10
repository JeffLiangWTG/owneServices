using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class GenericIEnumerableAdapterTest : TestCase
	{
		public void TestEnumerator()
		{
			ListDictionary dictionary = new ListDictionary();
			dictionary.Add(1, "test1");
			dictionary.Add(2, "test2");
			dictionary.Add(3, "test3");

			GenericIEnumerableAdapter<int> genericIntEnumerable = new GenericIEnumerableAdapter<int>(dictionary.Keys);
			IEnumerator<int> intEnumerator = genericIntEnumerable.GetEnumerator();
			for (int i = 1; i < 4; i++)
			{
				Assert("Should return true", intEnumerator.MoveNext());
				AssertEquals(i, intEnumerator.Current);
			}
			Assert("Should only contain 3 elements", !intEnumerator.MoveNext());

			GenericIEnumerableAdapter<string> genericStringEnumerable = new GenericIEnumerableAdapter<string>(dictionary.Values);
			IEnumerator<string> stringEnumerator = genericStringEnumerable.GetEnumerator();
			for (int i = 1; i < 4; i++)
			{
				Assert("Should return true", stringEnumerator.MoveNext());
				AssertEquals("test" + i.ToString(), stringEnumerator.Current);
			}
			Assert("Should only contain 3 elements", !stringEnumerator.MoveNext());
		}
	}
}
