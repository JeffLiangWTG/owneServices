using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class OrgContactPatternMatchingSubscriberUtilities<T> : OrgPatternMatchingSubscriberUtilities<T> where T : BusinessObject, IPatternMatchingBusinessObjects
	{
		readonly ZGuid personPK;

		public OrgContactPatternMatchingSubscriberUtilities(ZGuid personPK)
		{
			this.personPK = personPK;
		}

		protected override void SetMasterId(T patternMatchingObjects, ZGuid id)
		{
			patternMatchingObjects.OrganisationPK = id;
			patternMatchingObjects.PersonPK = personPK;
		}
	}
}
