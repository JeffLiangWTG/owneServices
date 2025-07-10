using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection()
		{
		}

		public KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new KoreaSouthEInvoicingDSBSummaryAppendingRule this[int i]
		{
			get { return (KoreaSouthEInvoicingDSBSummaryAppendingRule)Elements[i]; }
		}

		public new KoreaSouthEInvoicingDSBSummaryAppendingRule AddNew()
		{
			return (KoreaSouthEInvoicingDSBSummaryAppendingRule)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(fallbackLevel, CurrentFactory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRule(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
