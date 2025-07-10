using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ExceptionKeyStacktraceDepthRegistryItem : StronglyTypedRegistryItem<ExceptionKeyStacktraceDepthCollection>
	{
		public ExceptionKeyStacktraceDepthRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, new ExceptionKeyStacktraceDepthCollection())
		{
		}

		public ExceptionKeyStacktraceDepthRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ExceptionKeyStacktraceDepthCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExceptionKeyStacktraceDepthRegistryDataType(), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.ExceptionKeyStacktraceDepthRegistryEditor, ZClientEDI")]
		#if DEBUG
		public
		#endif
		class ExceptionKeyStacktraceDepthRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ExceptionKeyStacktraceDepthCollection>
		{
		}
	}
}

