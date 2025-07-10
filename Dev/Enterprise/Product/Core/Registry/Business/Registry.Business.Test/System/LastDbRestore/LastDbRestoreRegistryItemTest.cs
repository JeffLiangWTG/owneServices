using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LastDbRestoreRegistryItem))]
	sealed class LastDbRestoreRegistryItemTest : StronglyTypedRegistryItemTestCase<LastDbRestoreInfo>
	{
		protected override StronglyTypedRegistryItem<LastDbRestoreInfo, LastDbRestoreInfo> GetNewRegistryItem()
		{
			var lastDbRestoreInfo = new LastDbRestoreInfo();
			return new LastDbRestoreRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.IsReadOnly, lastDbRestoreInfo);
		}
	}
}
