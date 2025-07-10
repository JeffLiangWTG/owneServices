using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public abstract class GlbStaffSubscriber : PatternMatchingSubscriber<GlbStaff>
	{
		protected override string PKColumn => GlbStaffSchema.Constants.PK;

		public override ITableSchema Table => GlbStaffSchema.Instance;

		protected override PatternMasterType MasterType => PatternMasterType.GlbPerson;
	}
}
