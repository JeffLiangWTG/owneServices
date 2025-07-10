using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class RequireReasonForCLRRegistryItem : StronglyTypedRegistryItem<RequireReasonForCLRWrapper>
	{
		public RequireReasonForCLRRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOption, RequireReasonForCLRWrapper defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RequireReasonForCLRDataType(), storage, registryOption, defaultValue))
		{
		}
	}
}
