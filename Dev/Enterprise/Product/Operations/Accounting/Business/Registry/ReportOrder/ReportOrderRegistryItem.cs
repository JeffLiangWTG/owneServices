using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ReportOrderRegistryItem : StronglyTypedRegistryItem<ReportOrderCollection>
	{
		public ReportOrderRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ReportOrderRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ReportOrderRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class ReportOrderRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ReportOrderCollection>
	{
		public ReportOrderRegistryDataType()
		{
		}
	}
}
