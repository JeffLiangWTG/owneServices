using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CodeDescriptionPairListProviderTest : TransactionedTestCase
	{
		public void TestCodeDescriptionPairList()
		{
			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				var newList = new CodeDescriptionPairList();
				newList.AddPair("ABC", "Alphabet");
				newList.AddPair("ZYX", "Reverse Alphabet");
				newList.AddPair("ZUB", "ZUBS");
				return newList;
			});
			var list = listProvider.CodeDescriptionPairList;

			AssertEquals(3, list.Count);
			AssertPairEquals(new CodeDescriptionPair("ABC", "Alphabet"), list[0]);
			AssertPairEquals(new CodeDescriptionPair("ZYX", "Reverse Alphabet"), list[1]);
			AssertPairEquals(new CodeDescriptionPair("ZUB", "ZUBS"), list[2]);
		}

		public void TestCodeDescriptionPairListIsLazyLoaded()
		{
			var isLazyListLoaded = false;
			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				isLazyListLoaded = true;
				var newList = new CodeDescriptionPairList();
				newList.AddPair("ABC", "Alphabet");
				newList.AddPair("ZYX", "Reverse Alphabet");
				newList.AddPair("ZUB", "ZUBS");
				return newList;
			});

			Assert("List should not be created before it is accessed", !isLazyListLoaded);

			var list = listProvider.CodeDescriptionPairList;
			Assert("Now the list should be loaded", isLazyListLoaded);
		}

		public void TestCodeDescriptionPairListIsCached()
		{
			var newListsCreated = 0;
			var listProvider = new CodeDescriptionPairListProvider(() =>
			{
				++newListsCreated;
				var newList = new CodeDescriptionPairList();
				newList.AddPair("ABC", "Alphabet");
				newList.AddPair("ZYX", "Reverse Alphabet");
				newList.AddPair("ZUB", "ZUBS");
				return newList;
			});

			var firstLookUp = listProvider.CodeDescriptionPairList;
			AssertEquals(1, newListsCreated);

			var secondLookUp = listProvider.CodeDescriptionPairList;
			AssertEquals("The function creating a new CodeDescriptionPairList should not be called again", 1, newListsCreated);
		}

		void AssertPairEquals(ICodeDescription expectedPair, ICodeDescription actualPair)
		{
			AssertEquals(expectedPair.Code + expectedPair.Description, actualPair.Code + actualPair.Description);
		}
	}
}
