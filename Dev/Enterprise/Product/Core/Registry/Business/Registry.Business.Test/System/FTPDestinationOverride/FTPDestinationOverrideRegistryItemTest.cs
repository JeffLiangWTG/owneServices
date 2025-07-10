using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FTPDestinationOverrideRegistryItem))]
	sealed class FTPDestinationOverrideRegistryItemTest : StronglyTypedRegistryItemTestCase<FTPDestinationOverrideInfo>
	{
		protected override StronglyTypedRegistryItem<FTPDestinationOverrideInfo, FTPDestinationOverrideInfo> GetNewRegistryItem()
		{
			var ftpDestinationOverrideInfo = new FTPDestinationOverrideInfo();
			return new FTPDestinationOverrideRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.NotCached | RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue, ftpDestinationOverrideInfo);
		}
	}
}
