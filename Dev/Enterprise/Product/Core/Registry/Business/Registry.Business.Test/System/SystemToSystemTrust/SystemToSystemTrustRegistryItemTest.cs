using CargoWise.SystemToSystemTrust;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemToSystemTrustRegistryItem))]
	sealed class SystemToSystemTrustRegistryItemTest : StronglyTypedRegistryItemTestCase<ISystemToSystemTrustInfo>
	{
		protected override StronglyTypedRegistryItem<ISystemToSystemTrustInfo, ISystemToSystemTrustInfo> GetNewRegistryItem()
		{
			return new SystemToSystemTrustRegistryItem(
				"name",
				(NoResString)"category",
				(NoResString)"caption",
				(NoResString)"hint",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
		}
	}
}
