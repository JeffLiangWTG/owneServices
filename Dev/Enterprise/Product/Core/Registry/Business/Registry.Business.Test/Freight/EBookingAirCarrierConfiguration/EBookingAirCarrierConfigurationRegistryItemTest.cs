using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EBookingAirCarrierConfigurationRegistryItem))]
	sealed class EBookingAirCarrierConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<EBookingCarrierConfiguration>
	{
		protected override StronglyTypedRegistryItem<EBookingCarrierConfiguration, EBookingCarrierConfiguration> GetNewRegistryItem()
		{
			return new EBookingAirCarrierConfigurationRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.Default, new EBookingCarrierConfiguration());
		}

		public void TestGetNewRegistryItemSecond()
		{
			var obj = new EBookingAirCarrierConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System);
			AssertNotNull(obj);
		}
	}
}
