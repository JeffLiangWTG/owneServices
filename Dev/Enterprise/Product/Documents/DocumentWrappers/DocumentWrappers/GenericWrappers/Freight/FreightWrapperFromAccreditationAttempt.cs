using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Recruiter.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromAccreditationAttempt : FreightWrapper
	{
		public FreightWrapperFromAccreditationAttempt(GlbAccreditationAttempt attempt, BusinessObjectFactory factory)
			: base(attempt, factory)
		{
			this.attempt = attempt;
		}
		readonly GlbAccreditationAttempt attempt;

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return attempt.PK;
		}

		protected override AccreditationAttemptWrapper GetAccreditationAttemptWrapper()
		{
			return new AccreditationAttemptWrapper(attempt, Factory);
		}

		protected override ZString GetJobNumber()
		{
			return attempt.Person.PER_FullName;
		}
	}
}
