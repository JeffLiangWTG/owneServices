using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.MDM.Subscribers
{
	public class PersonPatternMatchingSubscriberUtilities<T> : PatternMatchingSubscriberUtilities<T> where T : BusinessObject, IPatternMatchingBusinessObjects
	{
		protected override void SetMasterId(T patternMatchingObjects, ZGuid id)
		{
			patternMatchingObjects.PersonPK = id;
		}

		public override SchemaGuidColumn PatternMatchingAddressMasterIdColumn => PatternMatchingAddressSchema.PMA_PER;

		public override SchemaGuidColumn PatternMatchingDomainMasterIdColumn => PatternMatchingDomainSchema.PMD_PER;

		public override SchemaGuidColumn PatternMatchingEmailMasterIdColumn => PatternMatchingEmailSchema.PME_PER;

		public override SchemaGuidColumn PatternMatchingNameMasterIdColumn => PatternMatchingNameSchema.PMN_PER;

		public override SchemaGuidColumn PatternMatchingPhoneMasterIdColumn => PatternMatchingPhoneSchema.PMP_PER;

		public override SchemaGuidColumn PatternMatchingRegCodeMasterIdColumn => PatternMatchingRegCodeSchema.PMR_PER;
	}
}
