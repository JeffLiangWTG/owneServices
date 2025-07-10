
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodePairRegistryItem : StronglyTypedRegistryItem<string>
	{
		public CodePairRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, RegistryStorageFlags storage)
			: this(name, category, caption, hint, lookUpEditType, storage, RegistryOptions.Default)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, RegistryStorageFlags storage)
			: this(name, category, caption, hint, lookUpList, false, true, null, storage, RegistryOptions.Default, null, true)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, lookUpEditType, false, true, null, storage, options, null, true)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, lookUpList, false, true, null, storage, options, null, true)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, RegistryStorageFlags storage, string defaultValue)
			: this(name, category, caption, hint, lookUpEditType, false, true, null, storage, RegistryOptions.Default, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, RegistryStorageFlags storage, string defaultValue)
			: this(name, categories, caption, hint, lookUpEditType, false, true, null, storage, RegistryOptions.Default, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: this(name, categories, caption, hint, lookUpEditType, false, true, null, storage, options, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, bool allowBlank, bool validate, ComboBoxRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, caption, hint, new CodePairRegistryDataType(lookUpEditType, allowBlank, validate), editorInfo, storage, options, defaultValue, useDefaultDefaultValue, categories))
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, RegistryStorageFlags storage, string defaultValue)
			: this(name, category, caption, hint, lookUpList, false, true, null, storage, RegistryOptions.Default, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, bool allowBlank, RegistryStorageFlags storage)
			: this(name, category, caption, hint, lookUpEditType, allowBlank, true, null, storage, RegistryOptions.Default, null, true)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: this(name, category, caption, hint, lookUpEditType, false, true, null, storage, options, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: this(name, category, caption, hint, lookUpList, false, true, null, storage, options, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, bool allowBlank, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: this(name, category, caption, hint, lookUpEditType, allowBlank, true, null, storage, options, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, bool allowBlank, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: this(name, category, caption, hint, lookUpList, allowBlank, true, null, storage, options, defaultValue, false)
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OLookUpEditType lookUpEditType, bool allowBlank, bool validate, ComboBoxRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodePairRegistryDataType(lookUpEditType, allowBlank, validate), editorInfo, storage, options, defaultValue, useDefaultDefaultValue))
		{
		}

		public CodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, bool allowBlank, bool validate, ComboBoxRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodePairRegistryDataType(lookUpList, allowBlank, validate), editorInfo, storage, options, defaultValue, useDefaultDefaultValue))
		{
		}
	}
}
