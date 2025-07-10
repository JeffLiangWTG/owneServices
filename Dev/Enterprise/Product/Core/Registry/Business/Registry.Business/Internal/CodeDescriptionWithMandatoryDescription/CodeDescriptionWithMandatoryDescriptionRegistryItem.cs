using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionWithMandatoryDescriptionRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionWithMandatoryDescriptionCollection, CodeDescriptionWithMandatoryDescriptionCollection>
	{
		public CodeDescriptionWithMandatoryDescriptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionWithMandatoryDescriptionCollection defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public CodeDescriptionWithMandatoryDescriptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionWithMandatoryDescriptionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionWithMandatoryDescriptionRegistryDataType(), storage, options, defaultValue))
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CodeDescriptionWithMandatoryDescriptionRegistryItemEditor, Enterprise.Registry.GUI")]
	class CodeDescriptionWithMandatoryDescriptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionWithMandatoryDescriptionCollection>
	{
		public CodeDescriptionWithMandatoryDescriptionRegistryDataType()
		{
		}
	}
}
