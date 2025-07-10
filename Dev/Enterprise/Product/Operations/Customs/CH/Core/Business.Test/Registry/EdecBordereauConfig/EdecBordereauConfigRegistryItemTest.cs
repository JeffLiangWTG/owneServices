using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EdecBordereauConfigRegistryItem))]
sealed class EdecBordereauConfigRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<EdecBordereauConfig>
{
	protected override StronglyTypedRegistryItem<EdecBordereauConfig, EdecBordereauConfig> GetNewRegistryItem()
	{
		return new EdecBordereauConfigRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System);
	}
}
