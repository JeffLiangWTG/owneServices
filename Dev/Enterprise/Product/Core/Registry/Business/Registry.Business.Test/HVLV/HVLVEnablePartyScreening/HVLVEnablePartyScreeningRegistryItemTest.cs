using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVEnablePartyScreeningRegistryItem))]
	sealed class HVLVEnablePartyScreeningRegistryItemTest : StronglyTypedRegistryItemTestCase<HVLVEnablePartyScreening>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<HVLVEnablePartyScreening, HVLVEnablePartyScreening> GetNewRegistryItem()
		{
			return new HVLVEnablePartyScreeningRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}

		#endregion
	}
}
