using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class OrgPatternMatchingSubscriberUtilities<T> : PatternMatchingSubscriberUtilities<T> where T : BusinessObject, IPatternMatchingBusinessObjects
	{
		protected override void SetMasterId(T patternMatchingObjects, ZGuid id)
		{
			patternMatchingObjects.OrganisationPK = id;
		}

		public override SchemaGuidColumn PatternMatchingAddressMasterIdColumn => PatternMatchingAddressSchema.PMA_OH;

		public override SchemaGuidColumn PatternMatchingDomainMasterIdColumn => PatternMatchingDomainSchema.PMD_OH;

		public override SchemaGuidColumn PatternMatchingEmailMasterIdColumn => PatternMatchingEmailSchema.PME_OH;

		public override SchemaGuidColumn PatternMatchingNameMasterIdColumn => PatternMatchingNameSchema.PMN_OH;

		public override SchemaGuidColumn PatternMatchingPhoneMasterIdColumn => PatternMatchingPhoneSchema.PMP_OH;

		public override SchemaGuidColumn PatternMatchingRegCodeMasterIdColumn => PatternMatchingRegCodeSchema.PMR_OH;
	}
}
