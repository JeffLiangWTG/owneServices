using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Registry.Business.JobStatusUpdateRestrictionRuleLookups;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobStatusUpdateRestrictionRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new JobStatusUpdateRestrictionRule this[int i] => (JobStatusUpdateRestrictionRule)Elements[i];

		public new JobStatusUpdateRestrictionRule AddNew() => (JobStatusUpdateRestrictionRule)base.AddNew();

		protected override bool AllowSort => false;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new JobStatusUpdateRestrictionRuleCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new JobStatusUpdateRestrictionRule();

		public static JobStatusUpdateRestrictionRuleCollection GetDefault()
		{
			var collection = new JobStatusUpdateRestrictionRuleCollection();
			var jobStatusList = new JobHeaderStatusRestrictionList();
			jobStatusList.GetAllCodes().ForEach(x => collection.AddNew().SetDefaults(x));
			return collection;
		}
	}
}
