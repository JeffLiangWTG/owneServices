using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class GuidRegistryItem : StronglyTypedRegistryItem<Guid>
	{
		public GuidRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public GuidRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, Guid.Empty)
		{
		}

		public GuidRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, categories, caption, hint, null, storage, RegistryOptions.Default, Guid.Empty)
		{
		}

		public GuidRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, Guid.Empty)
		{
		}

		public GuidRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, Guid defaultValue)
			: this(name, category, caption, hint, null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public GuidRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, GuidRegistryDataType dataType, RegistryStorageFlags storage, Guid defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, null, storage, RegistryOptions.IsValueMandatory, defaultValue))
		{
		}

		public GuidRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, Guid defaultValue)
			: this(name, category, caption, hint, null, storage, options, defaultValue)
		{
		}

		public GuidRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, GuidRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, Guid defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GuidRegistryDataType(), editorInfo, storage, GetRegistryOptions(options), defaultValue))
		{
		}

		public GuidRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, GuidRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, Guid defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, new GuidRegistryDataType(), editorInfo, storage, GetRegistryOptions(options), defaultValue, false, categories))
		{
		}

		static RegistryOptions GetRegistryOptions(RegistryOptions options)
		{
			return (options & RegistryOptions.IsValueOptional) == RegistryOptions.IsValueOptional ? options : options | RegistryOptions.IsValueMandatory;
		}
	}
}
