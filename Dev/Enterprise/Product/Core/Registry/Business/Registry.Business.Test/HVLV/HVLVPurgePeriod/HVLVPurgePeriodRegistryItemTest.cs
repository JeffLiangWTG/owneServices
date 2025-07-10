using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPurgePeriodRegistryItem))]
	sealed class HVLVPurgePeriodRegistryItemTest : StronglyTypedRegistryItemTestCase<HVLVPurgePeriod>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<HVLVPurgePeriod, HVLVPurgePeriod> GetNewRegistryItem()
		{
			return new HVLVPurgePeriodRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		#endregion
	}
}
