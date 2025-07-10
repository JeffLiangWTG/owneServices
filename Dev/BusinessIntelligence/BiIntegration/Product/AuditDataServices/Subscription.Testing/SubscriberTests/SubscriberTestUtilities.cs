using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	public static class SubscriberTestUtilities
	{
		public static OrgHeader CreateTestOrgHeader(BusinessObjectFactory factory, string fullName = "TEST ORGANISATION")
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "TESTER";
			orgHeader.OH_FullName = fullName;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			orgHeader.OH_IsActive = true;
			return orgHeader;
		}

		public static GlbPerson CreateTestGlbPerson(BusinessObjectFactory factory)
		{
			var glbPerson = factory.NewWithValidTestData<GlbPerson>();
			glbPerson.PER_FullName = "TESTPER";
			glbPerson.PER_RN_NKCountry = "AU";
			glbPerson.PER_IsActive = true;
			return glbPerson;
		}

		public static GlbStaff CreateTestGlbStaff(BusinessObjectFactory factory)
		{
			var glbStaff = factory.NewWithValidTestData<GlbStaff>();
			glbStaff.GS_FullName = "TESTSTAFF";
			glbStaff.GS_RN_NKCountryCode = "AU";
			return glbStaff;
		}
		public static OrgContact CreateTestOrgContact(BusinessObjectFactory factory)
		{
			var orgContact = factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_ContactName = "TESTCON";
			orgContact.OC_RN_NKNationality = "AU";
			return orgContact;
		}
		public static HRJobApplicant CreateTestHRJobApplicant(BusinessObjectFactory factory)
		{
			var hrJobApplicant = factory.NewWithValidTestData<HRJobApplicant>();
			hrJobApplicant.HA_FullName = "TESTAPP";
			hrJobApplicant.HA_RN_NKCountry = "AU";
			return hrJobApplicant;
		}

		public static void CreatePatternMatchingBusinessObject<T>(BusinessObjectFactory factory, ZGuid orgHeaderPK, ZInt hashedValue, ZString parentTableCode, ZGuid parentId, ZString countryCode) where T : BusinessObject
		{
			var patternMatchingBusinessObject = (IPatternMatchingBusinessObjects)factory.NewWithValidTestData<T>();
			patternMatchingBusinessObject.OrganisationPK = orgHeaderPK;
			patternMatchingBusinessObject.HashedValue = hashedValue;
			patternMatchingBusinessObject.ParentTableCode = parentTableCode;
			patternMatchingBusinessObject.ParentId = parentId;
			patternMatchingBusinessObject.PatternMatchingCountryCode = countryCode;
		}

		public static void CreatePatternMatchingBusinessObjectForPerson<T>(BusinessObjectFactory factory, ZGuid glbPersonPK, ZInt hashedValue, ZString parentTableCode, ZGuid parentId, ZString countryCode) where T : BusinessObject
		{
			var patternMatchingBusinessObject = (IPatternMatchingBusinessObjects)factory.NewWithValidTestData<T>();
			patternMatchingBusinessObject.PersonPK = glbPersonPK;
			patternMatchingBusinessObject.HashedValue = hashedValue;
			patternMatchingBusinessObject.ParentTableCode = parentTableCode;
			patternMatchingBusinessObject.ParentId = parentId;
			patternMatchingBusinessObject.PatternMatchingCountryCode = countryCode;
		}

		public static void CreatePatternMatchingBusinessObjectForOrgAndPerson<T>(BusinessObjectFactory factory, ZGuid orgHeaderPK, ZGuid glbPersonPK, ZInt hashedValue, ZString parentTableCode, ZGuid parentId, ZString countryCode) where T : BusinessObject
		{
			var patternMatchingBusinessObject = (IPatternMatchingBusinessObjects)factory.NewWithValidTestData<T>();
			patternMatchingBusinessObject.OrganisationPK = orgHeaderPK;
			patternMatchingBusinessObject.PersonPK = glbPersonPK;
			patternMatchingBusinessObject.HashedValue = hashedValue;
			patternMatchingBusinessObject.ParentTableCode = parentTableCode;
			patternMatchingBusinessObject.ParentId = parentId;
			patternMatchingBusinessObject.PatternMatchingCountryCode = countryCode;
		}

		public static int GetHash(string valueToHash)
		{
			return !string.IsNullOrEmpty(valueToHash) ? TextStandardizerHelper.ComputeStringHashFast(valueToHash) : 0;
		}
	}
}
