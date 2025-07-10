using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ExportStatementSettingRegistryItem))]
	sealed class ExportStatementSettingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<CountryExportStatementSettingCollection>
	{
		protected override StronglyTypedRegistryItem<CountryExportStatementSettingCollection, CountryExportStatementSettingCollection> GetNewRegistryItem()
		{
			return new ExportStatementSettingRegistryItem("", null, null, null, RegistryStorageFlags.System, new CountryExportStatementSettingCollection());
		}
	}
}
