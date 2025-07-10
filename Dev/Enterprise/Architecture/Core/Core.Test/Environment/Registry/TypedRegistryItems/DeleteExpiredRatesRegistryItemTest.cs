using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DeleteExpiredRatesRegistryItem))]
	sealed class DeleteExpiredRatesRegistryItemTest : StronglyTypedRegistryItemTestCase<DeleteExpiredRates>
	{
		protected override StronglyTypedRegistryItem<DeleteExpiredRates, DeleteExpiredRates> GetNewRegistryItem()
		{
			return new DeleteExpiredRatesRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.IsOnlyForController, new DeleteExpiredRates { ExpiredRatesPeriodInYears = 1, BatchSize = 100 });
		}
	}
}
