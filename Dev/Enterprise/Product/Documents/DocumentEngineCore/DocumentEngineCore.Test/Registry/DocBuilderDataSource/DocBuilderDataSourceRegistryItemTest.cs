using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocBuilderDataSourceRegistryItem))]
	class DocBuilderDataSourceRegistryItemTest : StronglyTypedRegistryItemTestCase<DocBuilderDataSource>
	{
		protected override StronglyTypedRegistryItem<DocBuilderDataSource, DocBuilderDataSource> GetNewRegistryItem()
		{
			return new DocBuilderDataSourceRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new DocBuilderDataSource());
		}
	}
}
