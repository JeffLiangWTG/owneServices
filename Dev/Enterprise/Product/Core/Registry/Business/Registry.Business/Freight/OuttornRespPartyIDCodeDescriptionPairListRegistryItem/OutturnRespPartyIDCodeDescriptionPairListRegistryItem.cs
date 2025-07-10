using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OutturnRespPartyIDCodeDescriptionPairListRegistryItem : CodeDescriptionPairListRegistryItem
	{
		public OutturnRespPartyIDCodeDescriptionPairListRegistryItem(string name, MultilingualString caption, MultilingualString hint, int maxCodeLength, CodeDescriptionPairListEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultDefaultValue, params MultilingualString[] categories)
			: base(new RegistryItemImpl(name, caption, hint, new OutturnRespPartyIDCodeDescriptionPairListRegistryDataType(maxCodeLength), editorInfo, storage, options, defaultValue, useDefaultDefaultValue, categories), true, defaultValue)
		{
		}
	}
}
