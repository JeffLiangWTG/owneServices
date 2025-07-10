using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TIP
{
	public class OrgPartRelationRegistryItem : StronglyTypedRegistryItem<OrgPartRelationRegistryBusinessObjectCollection>
	{
		public OrgPartRelationRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new OrgPartRelationRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.TIP.OrgPartRelationRegistryItemEditor, ZClientTIP")]
	class OrgPartRelationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OrgPartRelationRegistryBusinessObjectCollection>
	{
		public OrgPartRelationRegistryDataType()
		{
		}
	}
}
