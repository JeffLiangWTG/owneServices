using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class CountryTierPriceCodeMappingRegistryItem : StronglyTypedRegistryItem<CountryTierPriceCodeMappingCollection, CountryTierPriceCodeMappingCollection>
	{
		public CountryTierPriceCodeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, CountryTierPriceCodeMappingRegistryEditorInfo editorInfo, RegistryStorageFlags storage)
			: this(name, category, caption, hint, editorInfo, storage, new CountryTierPriceCodeMappingCollection())
		{
		}

		public CountryTierPriceCodeMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, CountryTierPriceCodeMappingRegistryEditorInfo editorInfo, RegistryStorageFlags storage, CountryTierPriceCodeMappingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CountryTierPriceCodeMappingRegistryDataType(), editorInfo, storage, RegistryOptions.Default, defaultValue))
		{
		}
	}

	public class CountryTierPriceCodeMappingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CountryTierPriceCodeMappingCollection>
	{
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.CountryTierPriceCodeMappingRegistryEditor, ZClientEDI")]
	public class CountryTierPriceCodeMappingRegistryEditorInfo : IRegistryEditorInfo
	{
		public Type BaseDataTypeToBeEdited => typeof(CountryTierPriceCodeMappingCollection);
	}
}
