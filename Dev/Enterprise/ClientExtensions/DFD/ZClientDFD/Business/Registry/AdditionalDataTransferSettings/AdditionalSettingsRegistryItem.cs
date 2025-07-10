using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DFD.Registry
{
	internal class AdditionalSettingsRegistryItem : StronglyTypedRegistryItem<AdditionalSettingsRegistryBusinessObject>
	{
		public AdditionalSettingsRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new AdditionalSettingsRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.DFD.Registry.AdditionalSettingsRegistryItemEditor, ZClientDFD")] // assembly path, assembly name
	class AdditionalSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AdditionalSettingsRegistryBusinessObject>
	{
	}
}
