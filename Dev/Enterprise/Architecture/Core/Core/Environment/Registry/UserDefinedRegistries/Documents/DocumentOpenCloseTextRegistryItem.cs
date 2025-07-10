using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class DocumentOpenCloseTextRegistryItem : MultilingualStringRegistryItem
	{
		public DocumentOpenCloseTextRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: this(name, category, caption, hint, RegistryStorageFlags.All, null)
		{
		}

		public DocumentOpenCloseTextRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, MultilingualString defaultText)
			: this(name, category, caption, hint, RegistryStorageFlags.All, defaultText)
		{
		}

		public DocumentOpenCloseTextRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, null)
		{
		}

		public DocumentOpenCloseTextRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, null)
		{
		}

		public DocumentOpenCloseTextRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString defaultText)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultText)
		{
		}

		public DocumentOpenCloseTextRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, MultilingualString defaultText)
			: base(name, category, caption, hint, storage, options, defaultText)
		{
			EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
		}
	}
}
