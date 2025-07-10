using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.SpellCheck.Testing
{
	sealed class StringsToExcludeTest : TestCaseWithFactory
	{
		public void TestUpdateWordsToIgnore()
		{
			stringsToExclude.UpdateWordsToIgnore(new[] { "James Wen", "James Bond" });
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC", "James", "Wen", "Bond" }, stringsToExclude);

			stringsToExcludeWithoutDictionary.UpdateWordsToIgnore(new[] { "James Wen", "James Bond" });
			AssertContainsExactElementsInAnyOrder(new[] { "James", "Wen", "Bond" }, stringsToExcludeWithoutDictionary);

			stringsToExcludeWithoutDictionary.UpdateWordsToIgnore(System.Array.Empty<string>());
			AssertNull(stringsToExcludeWithoutDictionary.GetEnumerator());

			stringsToExcludeWithoutDictionary.UpdateWordsToIgnore(null);
			AssertNull(stringsToExcludeWithoutDictionary.GetEnumerator());
		}

		public void TestCount()
		{
			AssertEquals(3, ((ICollection<string>)stringsToExclude).Count);

			stringsToExclude.UpdateWordsToIgnore(new[] { "James Wen", "James Bond" });
			AssertEquals(6, ((ICollection<string>)stringsToExclude).Count);

			stringsToExclude.UpdateWordsToIgnore(null);
			AssertEquals(3, ((ICollection<string>)stringsToExclude).Count);

			stringsToExcludeWithoutDictionary.UpdateWordsToIgnore(null);
			AssertEquals(0, ((ICollection<string>)stringsToExcludeWithoutDictionary).Count);
		}

		public void TestIsReadOnly()
		{
			Assert(!((ICollection<string>)stringsToExclude).IsReadOnly);

			stringsToExclude.UpdateWordsToIgnore(null);
			Assert(!((ICollection<string>)stringsToExclude).IsReadOnly);

			stringsToExcludeWithoutDictionary.UpdateWordsToIgnore(null);
			Assert(!((ICollection<string>)stringsToExclude).IsReadOnly);
		}

		public void TestAdd()
		{
			((ICollection<string>)stringsToExclude).Add("DDD");
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC", "DDD" }, stringsToExclude);

			((ICollection<string>)stringsToExcludeWithoutDictionary).Add("DDD");
			AssertNull(stringsToExcludeWithoutDictionary.GetEnumerator());
		}

		public void TestRemove()
		{
			((ICollection<string>)stringsToExclude).Remove("AAA");
			AssertCollectionNotContains("AAA", stringsToExclude);

			((ICollection<string>)stringsToExcludeWithoutDictionary).Remove("DDD");
			AssertNull(stringsToExcludeWithoutDictionary.GetEnumerator());
		}

		public void TestClear()
		{
			stringsToExclude.UpdateWordsToIgnore(new[] { "James Wen", "James Bond" });
			((ICollection<string>)stringsToExclude).Clear();

			AssertEquals(0, stringsToExclude.Count());

			((ICollection<string>)stringsToExcludeWithoutDictionary).Clear();
			AssertNull(stringsToExcludeWithoutDictionary.GetEnumerator());
		}

		public void TestContains()
		{
			stringsToExclude.UpdateWordsToIgnore(new[] { "DDD" });
			((ICollection<string>)stringsToExclude).Add("EEE");
			Assert(((ICollection<string>)stringsToExclude).Contains("AAA"));
			Assert(((ICollection<string>)stringsToExclude).Contains("DDD"));
			Assert(((ICollection<string>)stringsToExclude).Contains("EEE"));

			Assert(!((ICollection<string>)stringsToExcludeWithoutDictionary).Contains("AAA"));
		}

		public void TestCopyTo()
		{
			var result = new string[3];
			((ICollection<string>)stringsToExclude).CopyTo(result, 0);
			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB", "CCC" }, result);

			result = new string[3];
			((ICollection<string>)stringsToExcludeWithoutDictionary).CopyTo(result, 0);
			AssertContainsExactElementsInAnyOrder(new string[] { null, null, null }, result);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var dictionary = WordDictionary.GetOrAdd(Factory, "ABC");
			dictionary.AddWord("AAA");
			dictionary.AddWord("BBB");
			dictionary.AddWord("CCC");

			stringsToExclude = new StringsToExclude(dictionary);

			stringsToExcludeWithoutDictionary = new StringsToExclude(null);
		}
		StringsToExclude stringsToExclude;
		StringsToExclude stringsToExcludeWithoutDictionary;
	}
}
