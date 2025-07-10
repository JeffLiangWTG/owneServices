using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ExportStatementSettingRegistryItem : StronglyTypedRegistryItem<CountryExportStatementSettingCollection>
	{
		public ExportStatementSettingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExportStatementSettingRegistryDataType(), storage))
		{
		}

		public ExportStatementSettingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExportStatementSettingRegistryDataType(), storage, options))
		{
		}

		public ExportStatementSettingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CountryExportStatementSettingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExportStatementSettingRegistryDataType(), storage, defaultValue))
		{
		}

		public ExportStatementSettingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CountryExportStatementSettingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExportStatementSettingRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
