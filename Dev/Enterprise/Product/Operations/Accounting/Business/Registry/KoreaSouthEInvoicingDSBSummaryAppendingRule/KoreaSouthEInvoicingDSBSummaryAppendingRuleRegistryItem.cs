using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItem : StronglyTypedRegistryItem<KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection>
	{
		public KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new KoreaSouthEInvoicingDSBSummaryAppendingRuleDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class KoreaSouthEInvoicingDSBSummaryAppendingRuleDataType : NonPersistentBusinessObjectRegistryDataType<KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection>
	{
		public KoreaSouthEInvoicingDSBSummaryAppendingRuleDataType()
		{
		}
	}
}
