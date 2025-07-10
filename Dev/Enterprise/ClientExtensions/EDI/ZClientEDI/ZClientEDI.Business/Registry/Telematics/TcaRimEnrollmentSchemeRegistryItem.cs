using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business.Registry
{
	public class TcaRimEnrollmentSchemeRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionBoolCollection, TcaRimEnrollmentSchemeCollection>
	{
		protected TcaRimEnrollmentSchemeRegistryItem(IRegistryItem item, IRegistryEditorInfo editorInfo)
			: base(item, editorInfo)
		{
		}

		public TcaRimEnrollmentSchemeRegistryItem(string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions option,
			int codeMaxLength,
			CodeDescriptionBoolRegistryEditorInfo editorInfo,
			TcaRimEnrollmentSchemeCollection defaultValue)
		: base(
			new RegistryItemImpl(
				name,
				category,
				caption,
				hint,
				new TcaRimEnrollmentSchemeRegistryDataType(),
				storage,
				option,
				defaultValue),
			editorInfo)
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}
}
