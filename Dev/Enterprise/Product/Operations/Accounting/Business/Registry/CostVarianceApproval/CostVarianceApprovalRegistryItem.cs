using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CostVarianceApprovalRegistryItem : StronglyTypedRegistryItem<CostVarianceApproval>
	{
		public CostVarianceApprovalRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new CostVarianceApprovalRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CostVarianceApprovalRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class CostVarianceApprovalRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CostVarianceApproval>
	{
	}
}
