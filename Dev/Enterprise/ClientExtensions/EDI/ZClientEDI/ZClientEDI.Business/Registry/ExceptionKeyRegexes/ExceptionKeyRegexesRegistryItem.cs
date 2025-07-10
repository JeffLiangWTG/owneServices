using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ExceptionKeyRegexesRegistryItem : StronglyTypedRegistryItem<ExceptionKeyRegexCollection>
	{
		public ExceptionKeyRegexesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, new ExceptionKeyRegexCollection())
		{
		}

		public ExceptionKeyRegexesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ExceptionKeyRegexCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExceptionKeyRegexesRegistryDataType(), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.ExceptionKeyRegexesRegistryEditor, ZClientEDI")]
		#if DEBUG
		public
		#endif
		class ExceptionKeyRegexesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ExceptionKeyRegexCollection>
		{
		}
	}
}
