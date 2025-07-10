using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CreditTemporaryIncreaseAuthorisationSettingsRegistryItem : StronglyTypedRegistryItem<CreditTemporaryIncreaseAuthorisationSettingsCollection>
	{
		public CreditTemporaryIncreaseAuthorisationSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CreditTemporaryIncreaseAuthorisationSettingsRegistryItemEditor, Enterprise.Registry.GUI")]
	class CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CreditTemporaryIncreaseAuthorisationSettingsCollection>
	{
		public CreditTemporaryIncreaseAuthorisationSettingsRegistryDataType()
		{
		}
	}
}
