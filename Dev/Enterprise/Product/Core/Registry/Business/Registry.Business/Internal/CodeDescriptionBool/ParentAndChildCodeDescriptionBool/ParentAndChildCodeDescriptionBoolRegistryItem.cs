using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ParentAndChildCodeDescriptionBoolRegistryItem : StronglyTypedRegistryItem<IParentCodeDescriptionBoolList, ParentCodeDescriptionBoolCollection>
	{
		public ParentAndChildCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryStorageFlags storage)
			: this(name, category, caption, hint, editorInfo, storage, new ParentCodeDescriptionBoolCollection())
		{
		}

		public ParentAndChildCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryStorageFlags storage, ParentCodeDescriptionBoolCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ParentAndChildCodeDescriptionBoolRegistryDataType(), storage, defaultValue))
		{
			this.EditorInfo = editorInfo;
		}

		public ParentAndChildCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, editorInfo, storage, options, new ParentCodeDescriptionBoolCollection())
		{
		}

		public ParentAndChildCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ParentAndChildCodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, ParentCodeDescriptionBoolCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ParentAndChildCodeDescriptionBoolRegistryDataType(), storage, options, defaultValue))
		{
			this.EditorInfo = editorInfo;
		}
	}

	public class ParentAndChildCodeDescriptionBoolRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ParentCodeDescriptionBoolCollection>
	{
		public ParentAndChildCodeDescriptionBoolRegistryDataType()
		{
		}
	}
}
