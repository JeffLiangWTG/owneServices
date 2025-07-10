using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	class SubscriberUtilitiesTest : TestCase
	{
		public void TestOrgPatternMatchingSubscriberUtilitiesContainsCorrectSchema()
		{
			var orgUtilities = new OrgPatternMatchingSubscriberUtilities<PatternMatchingAddress>();

			AssertEquals(PatternMatchingAddressSchema.PMA_OH, orgUtilities.PatternMatchingAddressMasterIdColumn);
			AssertEquals(PatternMatchingDomainSchema.PMD_OH, orgUtilities.PatternMatchingDomainMasterIdColumn);
			AssertEquals(PatternMatchingEmailSchema.PME_OH, orgUtilities.PatternMatchingEmailMasterIdColumn);
			AssertEquals(PatternMatchingNameSchema.PMN_OH, orgUtilities.PatternMatchingNameMasterIdColumn);
			AssertEquals(PatternMatchingPhoneSchema.PMP_OH, orgUtilities.PatternMatchingPhoneMasterIdColumn);
			AssertEquals(PatternMatchingRegCodeSchema.PMR_OH, orgUtilities.PatternMatchingRegCodeMasterIdColumn);
		}

		public void TestPersonPatternMatchingSubscriberUtilitiesContainsCorrectSchema()
		{
			var personUtilities = new PersonPatternMatchingSubscriberUtilities<PatternMatchingAddress>();

			AssertEquals(PatternMatchingAddressSchema.PMA_PER, personUtilities.PatternMatchingAddressMasterIdColumn);
			AssertEquals(PatternMatchingDomainSchema.PMD_PER, personUtilities.PatternMatchingDomainMasterIdColumn);
			AssertEquals(PatternMatchingEmailSchema.PME_PER, personUtilities.PatternMatchingEmailMasterIdColumn);
			AssertEquals(PatternMatchingNameSchema.PMN_PER, personUtilities.PatternMatchingNameMasterIdColumn);
			AssertEquals(PatternMatchingPhoneSchema.PMP_PER, personUtilities.PatternMatchingPhoneMasterIdColumn);
			AssertEquals(PatternMatchingRegCodeSchema.PMR_PER, personUtilities.PatternMatchingRegCodeMasterIdColumn);
		}

		public void TestPatternMatchingSubscriberUtilitiesFactoryProviderUtilitiesCorrectly()
		{
			var utilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingAddress>.Provider(PatternMasterType.GlbPerson);
			AssertType<PersonPatternMatchingSubscriberUtilities<PatternMatchingAddress>>(utilities);

			utilities = PatternMatchingSubscriberUtilitiesFactory<PatternMatchingAddress>.Provider(PatternMasterType.OrgHeader);
			AssertType<OrgPatternMatchingSubscriberUtilities<PatternMatchingAddress>>(utilities);
		}
	}
}
