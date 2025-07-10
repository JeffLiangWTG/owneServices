using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MinimumIntervalSubLedgerTakeUpRegistryItem))]
	sealed class MinimumIntervalSubLedgerTakeUpRegistryItemTest : StronglyTypedRegistryItemTestCase<MinimumIntervalSubLedgerTakeUp>
	{
		protected override StronglyTypedRegistryItem<MinimumIntervalSubLedgerTakeUp, MinimumIntervalSubLedgerTakeUp> GetNewRegistryItem()
		{
			return new MinimumIntervalSubLedgerTakeUpRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, new MinimumIntervalSubLedgerTakeUp());
		}
	}
}
