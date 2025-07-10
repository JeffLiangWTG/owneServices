using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobInvoiceDescriptionRegistryItem : StronglyTypedRegistryItem<JobInvoiceDescriptionCollection>
	{
		public JobInvoiceDescriptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, JobInvoiceDescriptionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobInvoiceDescriptionRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobInvoiceDescriptionRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class JobInvoiceDescriptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobInvoiceDescriptionCollection>
	{
		public JobInvoiceDescriptionRegistryDataType()
		{
		}
	}
}
