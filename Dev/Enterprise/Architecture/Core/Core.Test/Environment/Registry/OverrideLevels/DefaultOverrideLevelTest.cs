using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels.Testing
{
	sealed class DefaultOverrideLevelTest : OverrideLevelTestCase<DefaultOverrideLevel>
	{
		protected override void SetAtValue(IRegistryItem item, object value)
		{
			throw new InvalidOperationException("Cant reset default value");
		}

		protected override void SetValueForFallback(IRegistryItem item, object value)
		{
			throw new InvalidOperationException("Default value has no fallback");
		}

		protected override IEnumerable<IOverrideLevel> ExpectedChildren(DefaultOverrideLevel level)
		{
			return Enumerable.Empty<IOverrideLevel>();
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatApplyAtThisLevel
		{
			get { return AllStorageFlags.Select(RegistryItemWithStorage); }
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatDoNotAtThisLevel
		{
			get { return Enumerable.Empty<IRegistryItem>(); }
		}

		public override void TestGetValueAtLevel()
		{
			var item = new StringRegistryItem("BAL", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "My Default");
			var level = new DefaultOverrideLevel();

			AssertEquals("My Default", level.GetValueOf(item));
		}

		public override void TestGetNearestFallback()
		{
			Assert(true); //No fallback to test
		}

		public override void TestSetValue()
		{
			Assert(true); //Cant set the default value
		}

		public override void TestGetPkOfEntry()
		{
			Assert(true); // You cant set an entry so you wont get one back
		}

		public override void TestCanSetValueOf()
		{
			var level = GetNewLevel();

			CombineAssertions(() =>
			{
				foreach (var storage in AllStorageFlags)
				{
					var item = RegistryItemWithStorage(storage);
					Assert("Cant set the default value - " + storage, !level.CanSetValueOf(item));
				}
			});
		}
	}
}
