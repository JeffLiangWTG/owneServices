using CargoWise.Schema;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public abstract class HRJobApplicantSubscriber : PatternMatchingSubscriber<HRJobApplicant>
	{
		protected override string PKColumn => HRJobApplicantSchema.Constants.PK;

		public override ITableSchema Table => HRJobApplicantSchema.Instance;

		protected override PatternMasterType MasterType => PatternMasterType.GlbPerson;
	}
}
