using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CreateMissingProductsRegistryItem))]
	sealed class CreateMissingProductsRegistryItemTest : StronglyTypedRegistryItemTestCase<CreateMissingProductsInfo>
	{
		protected override StronglyTypedRegistryItem<CreateMissingProductsInfo, CreateMissingProductsInfo> GetNewRegistryItem()
		{
			return new CreateMissingProductsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CreateMissingProductsInfo());
		}
	}
}
