using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class EnableComplianceWiseRegistryItem : StronglyTypedRegistryItem<EnableComplianceWiseRegistryBusinessObject>
	{
		public EnableComplianceWiseRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, EnableComplianceWiseRegistryBusinessObject defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new EnableComplianceWiseRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
