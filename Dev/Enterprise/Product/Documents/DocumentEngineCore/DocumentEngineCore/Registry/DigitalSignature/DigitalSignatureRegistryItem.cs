using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DigitalSignatureRegistryItem : StronglyTypedRegistryItem<DigitalSignatureRegistry>
	{
		public DigitalSignatureRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, DigitalSignatureRegistry defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DigitalSignatureRegistryDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, defaultValue))
		{ }
	}
}
