using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment.OverrideLevels.Testing
{
	sealed class SystemOverrideLevelTest : OverrideLevelTestCase<SystemOverrideLevel>
	{
		protected override void SetAtValue(IRegistryItem item, object value)
		{
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		protected override SystemOverrideLevel GetNewLevel()
		{
			return new SystemOverrideLevel();
		}

		protected override void SetValueForFallback(IRegistryItem item, object value)
		{
			throw new InvalidOperationException("Default is only fallback for system, and it can not be set");
		}

		protected override IEnumerable<IOverrideLevel> ExpectedChildren(SystemOverrideLevel level)
		{
			return Enumerable.Empty<IOverrideLevel>();
		}

		public override void TestGetNearestFallback()
		{
			var item = new StringRegistryItem("BAL", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, "My Default");
			var level = new SystemOverrideLevel();

			AssertEquals("Should fall back to system", "My Default", level.GetValueOf(item));
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatApplyAtThisLevel
		{
			get { return AllStorageFlags.Select(flag => flag | RegistryStorageFlags.System).Select(RegistryItemWithStorage); }
		}

		protected override IEnumerable<IRegistryItem> ExampleItemsThatDoNotAtThisLevel
		{
			get { return AllStorageFlags.Where(flag => !flag.HasFlag(RegistryStorageFlags.System)).Select(RegistryItemWithStorage); }
		}
	}
}
