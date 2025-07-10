using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobInvoicingDefaultGatewayDepartmentsRegistryItem : StronglyTypedRegistryItem<JobInvoicingDefaultGatewayDepartmentsCollection>
	{
		public JobInvoicingDefaultGatewayDepartmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, JobInvoicingDefaultGatewayDepartmentsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobInvoicingDefaultGatewayDepartmentsRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobInvoicingDefaultGatewayDepartmentsRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class JobInvoicingDefaultGatewayDepartmentsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobInvoicingDefaultGatewayDepartmentsCollection>
	{
	}
}
