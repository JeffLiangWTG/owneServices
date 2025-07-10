using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SqlSystemConfigurationRegistryItem : StronglyTypedRegistryItem<PersistedSqlConfigurationsCollection>
	{
		public SqlSystemConfigurationRegistryItem(string name, MultilingualString category, NoResString caption, NoResString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, null)
		{
		}

		public SqlSystemConfigurationRegistryItem()
			: this(
				(NoResString)"SqlSystemConfigurations",
				RawDataRegistry.Categories.System,
				(NoResString)"SQL Server Configurations",
				(NoResString)"SQL Server Configurations Proposed Values",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				new PersistedSqlConfigurationsCollection()
			)
		{
		}

		public SqlSystemConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PersistedSqlConfigurationsCollection defaultValue)
			: base(new SqlSystemConfigurationRegistryItemImpl(name, category, caption, hint, storage, options, defaultValue))
		{
		}

		class SqlSystemConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public SqlSystemConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PersistedSqlConfigurationsCollection defaultValue)
				: base(name, category, caption, hint, new PersistedSqlConfigurationsDataType(), storage, options, defaultValue ?? new PersistedSqlConfigurationsCollection())
			{
			}
		}
	}
}
