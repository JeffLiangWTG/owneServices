using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolRelatedItemRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionBoolRelatedItemCollection, CodeDescriptionBoolRelatedItemCollection>
	{
		public CodeDescriptionBoolRelatedItemRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, int codeMaxLength,
			CodeDescriptionBoolRelatedItemRegistryEditorInfo editorInfo, CodeDescriptionBoolRelatedItemCollection defaultValue, ReadOnlyCodeDescriptionPairList relatedItemLookup)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, codeMaxLength, editorInfo, defaultValue, relatedItemLookup)
		{
		}

		public CodeDescriptionBoolRelatedItemRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int codeMaxLength,
			CodeDescriptionBoolRelatedItemRegistryEditorInfo editorInfo, CodeDescriptionBoolRelatedItemCollection defaultValue, ReadOnlyCodeDescriptionPairList relatedItemLookup)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolRelatedItemDataType(codeMaxLength, relatedItemLookup), storage, options, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionBoolRelatedItemRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int codeMaxLength,
			CodeDescriptionBoolRelatedItemRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter, ReadOnlyCodeDescriptionPairList relatedItemLookup)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new CodeDescriptionBoolRelatedItemDataType(codeMaxLength, relatedItemLookup), storage, options, defaultValueGetter), editorInfo)
		{
		}

		protected CodeDescriptionBoolRelatedItemRegistryItem(IRegistryItem item, IRegistryEditorInfo editorInfo)
			: base(item, editorInfo)
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	public class CodeDescriptionBoolRelatedItemDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolRelatedItemCollection>
	{
		public CodeDescriptionBoolRelatedItemDataType(int codeMaxLength)
			: this(codeMaxLength, null)
		{
		}

		public CodeDescriptionBoolRelatedItemDataType(int codeMaxLength, ReadOnlyCodeDescriptionPairList relatedItemLookup)
			: base()
		{
			this.codeMaxLength = codeMaxLength;
			this.RelatedItemLookup = relatedItemLookup;
		}
		readonly int codeMaxLength;

		protected override CodeDescriptionBoolRelatedItemCollection DeserialiseCore(byte[] value)
		{
			var collection = base.DeserialiseCore(value);
			SetupCollection(collection);
			return collection;
		}

		void SetupCollection(CodeDescriptionBoolRelatedItemCollection collection)
		{
			collection.CodeMaxLength = codeMaxLength;
			collection.RelatedItemLookup = RelatedItemLookup;
		}

		#region Related Item Lookup

		public readonly ReadOnlyCodeDescriptionPairList RelatedItemLookup;

		#endregion
	}
}
