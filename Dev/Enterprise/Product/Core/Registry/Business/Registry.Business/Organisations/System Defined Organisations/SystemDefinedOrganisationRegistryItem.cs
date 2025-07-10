using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SystemDefinedOrganisationRegistryItem : StronglyTypedRegistryItem<SystemDefinedOrganisation>
	{
		public SystemDefinedOrganisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, Type systemDefinedOrganisationType)
			: base(new RegistryItemImpl(name, category, caption, hint, new SystemDefinedOrganisationRegistryDataType(systemDefinedOrganisationType), storage))
		{
		}

		public SystemDefinedOrganisationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Type systemDefinedOrganisationType)
			: base(new RegistryItemImpl(name, category, caption, hint, new SystemDefinedOrganisationRegistryDataType(systemDefinedOrganisationType), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.SystemDefinedOrganisationRegistryItemEditor, Enterprise.Registry.GUI")]
	class SystemDefinedOrganisationRegistryDataType : WeaklyTypedNonPersistentBusinessObjectRegistryDataType
	{
		public SystemDefinedOrganisationRegistryDataType(Type dataType)
			: base(dataType)
		{
		}
	}
}
