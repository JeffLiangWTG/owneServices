using System.Security.Principal;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SecurityIdentifierRegistryItem : StringRegistryItem
	{
		readonly SecurityIdentifier sidDefaultValue;

		public SecurityIdentifierRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, SecurityIdentifier sidDefaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SecurityIdentifierRegistryDataType(), storage, options))
		{
			this.sidDefaultValue = sidDefaultValue;
		}

		public SecurityIdentifier SecurityIdentifier => SecurityIdentifierRegistryDataType.Convert(Value) ?? sidDefaultValue;
	}
}
