
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobProfitLossRequiringReasonParametersRegistryItem : StronglyTypedRegistryItem<JobProfitLossRequiringReasonParameters>
	{
		public JobProfitLossRequiringReasonParametersRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, JobProfitLossRequiringReasonParameters defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobProfitLossRequiringReasonParametersRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobProfitLossRequiringReasonParametersRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class JobProfitLossRequiringReasonParametersRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobProfitLossRequiringReasonParameters>
	{
		public JobProfitLossRequiringReasonParametersRegistryDataType()
		{
		}
	}
}
