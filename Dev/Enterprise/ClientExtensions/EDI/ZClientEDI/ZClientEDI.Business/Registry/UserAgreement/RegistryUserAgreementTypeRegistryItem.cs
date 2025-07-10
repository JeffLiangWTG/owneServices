using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class RegistryUserAgreementTypeRegistryItem : StronglyTypedRegistryItem<RegistryUserAgreementTypeCollection>
	{
		public RegistryUserAgreementTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, RegistryUserAgreementTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RegistryUserAgreementTypeDataType(), storage, options, defaultValue))
		{
		}
	}
}
