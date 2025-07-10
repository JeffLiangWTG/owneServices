using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DbHealthWarningListRegistryItem : StronglyTypedRegistryItem<DbHealthWarningRegistryCollection>
	{
		public DbHealthWarningListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, DbHealthWarningRegistryCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DbHealthWarningListRegistryDataType(), RegistryStorageFlags.System, (EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyEditableBySupportIfHosted : RegistryOptions.IsOnlyForController) | RegistryOptions.MustOverrideDefaultValue, defaultValue))
		{
		}

		public void SetValue(DbHealthWarningRegistryCollection value)
		{
			this.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DbHealthWarningListRegistryItemEditor, Enterprise.Registry.GUI")]
	class DbHealthWarningListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DbHealthWarningRegistryCollection>
	{
		public DbHealthWarningListRegistryDataType()
		{
		}
	}
}
