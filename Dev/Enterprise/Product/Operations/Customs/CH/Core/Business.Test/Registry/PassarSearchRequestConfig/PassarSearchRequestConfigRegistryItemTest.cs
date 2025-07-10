using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PassarSearchRequestConfigRegistryItem))]
sealed class PassarSearchRequestConfigRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<PassarSearchRequestConfig>
{
	protected override StronglyTypedRegistryItem<PassarSearchRequestConfig, PassarSearchRequestConfig> GetNewRegistryItem()
	{
		return new PassarSearchRequestConfigRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System);
	}
}
