using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public abstract class GlbPersonSubscriber : PatternMatchingSubscriber<GlbPerson>
	{
		protected override string PKColumn => GlbPersonSchema.Constants.PK;

		public override ITableSchema Table => GlbPersonSchema.Instance;

		protected override PatternMasterType MasterType => PatternMasterType.GlbPerson;
	}
}
