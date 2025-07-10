using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.ELG
{
	public class SageAccountCodeMappingRegistryItem : StronglyTypedRegistryItem<SageAccountCodeMappingRegistryBusinessObjectCollection>
	{
		public SageAccountCodeMappingRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new SageAccountCodeMappingRegistryDataType(), storage, RegistryOptions.NotCached))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.ELG.SageAccountCodeMappingRegistryItemEditor,  ZClientELG")]
	public class SageAccountCodeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SageAccountCodeMappingRegistryBusinessObjectCollection>
	{
		public SageAccountCodeMappingRegistryDataType()
		{
		}
	}
}
