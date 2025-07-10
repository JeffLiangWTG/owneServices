using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(RegistryComparisonBusinessObject))]
	sealed class RegistryComparisonBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var itemsWithOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();
			var itemsWithout = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();
			var registryItems = itemsWithOverrides.Concat(itemsWithout);

			return new RegistryComparisonBusinessObject(registryItems.ToImmutableList(), new DummyOverrideLevel(itemsWithOverrides), new DefaultOverrideLevel());
		}

		StringRegistryItem NewItem()
		{
			return new StringRegistryItem(ZGuid.NewZGuid().ToString(), (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "DefaultValue");
		}

		public void TestDifferentItemsAreDifferent()
		{
			var itemsWithOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();
			var itemsWithout = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();

			var baseLevel = new DefaultOverrideLevel();
			var overrideLevel = new DummyOverrideLevel(itemsWithOverrides);

			var registryItems = itemsWithout.Concat(itemsWithOverrides);
			var bizo = new RegistryComparisonBusinessObject(registryItems.ToImmutableList(), overrideLevel, baseLevel);

			AssertContainsExactElementsInAnyOrder(item => item.Name, itemsWithOverrides, bizo.ItemsThatDifferBetweenBaseAndOverride);
		}

		public void TestMoreDifferentItems()
		{
			var itemsWithOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();
			var itemsWithoutOverrides = Enumerable.Range(0, 10).Select(i => NewItem()).ToList();

			var random = new Random();
			var shuffledItems = itemsWithOverrides.Concat(itemsWithoutOverrides).OrderBy(_ => random.Next()).ToList();

			var bizo = new RegistryComparisonBusinessObject(shuffledItems, new DummyOverrideLevel(itemsWithOverrides), new DefaultOverrideLevel());

			AssertContainsExactElementsInAnyOrder(item => item.Name, itemsWithOverrides, bizo.ItemsThatDifferBetweenBaseAndOverride);
		}

		public void TestProgressIsReported()
		{
			const int howManyItems = 1000;

			int amountThatHasBeenReported = 0;
			var progress = new Mock<IProgress<int>>();
			progress.Setup(p => p.Report(1)).Callback<int>(x => amountThatHasBeenReported += x);

			var itemsWithOverrides = Enumerable.Range(0, howManyItems).Select(i => NewItem());

			var bizo = new RegistryComparisonBusinessObject(itemsWithOverrides.ToImmutableList(), new DefaultOverrideLevel(), new DefaultOverrideLevel(), progress.Object);

			AssertEquals(howManyItems, amountThatHasBeenReported);
		}
	}
}
