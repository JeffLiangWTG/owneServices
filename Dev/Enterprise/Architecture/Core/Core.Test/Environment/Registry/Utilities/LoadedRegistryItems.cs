using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Registry.Testing
{
	public class TestLoadedRegistryItems : TestCase
	{
		public void TestTotalItems()
		{
			var loaded = new LoadedRegistryItems(Enumerable.Range(0, 10).Select(i => new LoadedRegistryItemValue("Blah" + i, "Blah", null, null, i % 2 == 0)));

			AssertEquals(10, loaded.TotalCount);
		}

		public void TestGetPkReturnsEmpty()
		{
			var loadedItems = Enumerable.Range(0, 5)
				.Select(i => NewRegistryItem("Successful" + i))
				.Select(item => new LoadedRegistryItemValue(item.Name, item.Caption, item, 2, true))
				.ToArray();

			var loaded = new LoadedRegistryItems(loadedItems.Skip(2)); // Skip the first two so we can test some unincluded items as well
			var level = loaded.AsOverrideLevel(new DefaultOverrideLevel());

			foreach (var item in loadedItems)
			{
				AssertEquals(Guid.Empty, level.GetPkOfEntry(item.registryItem));
			}
		}

		public void TestHasTheRightValues()
		{
			var successfull = Enumerable.Range(0, 5)
				.Select(i => NewRegistryItem("Successful" + i))
				.Select(item => new LoadedRegistryItemValue(item.Name, item.Caption, item, 2, true))
				.ToArray();

			var unsuccessful = Enumerable.Range(0, 3)
				.Select(i => NewRegistryItem("Failed" + i))
				.Select(item => new LoadedRegistryItemValue(item.Name, item.Caption, item, null, false))
				.ToArray();

			var loaded = new LoadedRegistryItems(successfull.Concat(unsuccessful));

			CombineAssertions(() =>
			{
				AssertEquals("Successful Count", 5, loaded.SuccessfulItems.Count);
				AssertEquals("Failed count", 3, loaded.FailedItems.Count);
			});
		}

		public void TestAsOverrideLevel()
		{
			var successfull = NewRegistryItem("Successful");
			var failedItem = NewRegistryItem("Failed");

			var successfullLoaded = new LoadedRegistryItemValue(successfull.Name, successfull.Caption, successfull, 23, true);
			var failedLoaded = new LoadedRegistryItemValue(failedItem.Name, failedItem.Caption, failedItem, null, false);

			var loaded = new LoadedRegistryItems(new[] { successfullLoaded, failedLoaded });
			var overrideLevel = loaded.AsOverrideLevel(new DefaultOverrideLevel());

			AssertEquals("Override level", 23, overrideLevel.GetValueOf(successfull));
			AssertEquals("Failed level", failedItem.DefaultValue, overrideLevel.GetValueOf(failedItem));
		}

		public void TestAsComparison()
		{
			var successfull = NewRegistryItem("Successful");
			var failedItem = NewRegistryItem("Failed");

			var successfullLoaded = new LoadedRegistryItemValue(successfull.Name, successfull.Caption, successfull, 23, true);
			var failedLoaded = new LoadedRegistryItemValue(failedItem.Name, failedItem.Caption, failedItem, null, false);

			var loaded = new LoadedRegistryItems(new[] { successfullLoaded, failedLoaded });
			var baseLevel = new DefaultOverrideLevel();

			var comparison = loaded.AsComparison(baseLevel);

			AssertEquals(baseLevel, comparison.BaseLevel);
			AssertEquals(successfull, comparison.ItemsThatDifferBetweenBaseAndOverride.Single());
			AssertEquals(23, comparison.OverrideLevel.GetValueOf(successfull));
			AssertEquals(baseLevel.GetValueOf(failedItem), comparison.OverrideLevel.GetValueOf(failedItem));
		}

		IRegistryItem NewRegistryItem(string code)
		{
			return new IntRegistryItem(code, (NoResString)"Ca/te/gory", (NoResString)"Caption", (NoResString)"", RegistryStorageFlags.All, 0);
		}
	}
}
