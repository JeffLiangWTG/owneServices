
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobProfitLossReasonCodeRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<JobProfitLossReasonCodeCollection, JobProfitLossReasonCodeCollection>
	{
		public JobProfitLossReasonCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, JobProfitLossReasonCodeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JobProfitLossReasonCodeRegistryDataType(), storage, defaultValue))
		{
		}

		public override int MaxLength
		{
			get { return 80; }
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.JobProfitLossReasonCodeRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class JobProfitLossReasonCodeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<JobProfitLossReasonCodeCollection>
	{
		public JobProfitLossReasonCodeRegistryDataType()
		{
		}
	}
}
