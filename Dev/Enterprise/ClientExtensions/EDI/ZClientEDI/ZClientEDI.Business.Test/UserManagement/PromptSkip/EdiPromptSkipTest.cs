namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Client.EDI.MasterFiles.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(EdiCustomerUserAccount))]
	internal class EdiPromptSkipTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAddSkip()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			AssertNull(EdiPromptSkip.AddSkip(person, "AAA", ZDateTime.Empty));
			AssertNull(EdiPromptSkip.AddSkip(person, EdiPromptSkipTypes.Codes.PersonalEmail, ZDateTime.Invalid));
			AssertNotNull(EdiPromptSkip.AddSkip(person, EdiPromptSkipTypes.Codes.PersonalEmail, ZDateTime.Empty));
			AssertNotNull(EdiPromptSkip.AddSkip(person, EdiPromptSkipTypes.Codes.PersonalEmail, ZDateTime.Today));
		}

		public void TestAddSkipShouldUpdateExistingSkipDate()
		{
			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			var existingSkip = EdiPromptSkip.AddSkip(person1, EdiPromptSkipTypes.Codes.PersonalEmail, ZDateTime.Today);
			AssertNotNull(existingSkip);
			Factory.Save();

			var dataQuery = new ZQuery(EdiPromptSkipSchema.EPS_Owner, person1.PK);
			dataQuery.AddToFilter(EdiPromptSkipSchema.EPS_Type, EdiPromptSkipTypes.Codes.PersonalEmail);
			AssertEquals("Should create 1 record", 1, Factory.GetDatabaseCount(typeof(EdiPromptSkip), dataQuery));

			var newSkipDate = ZDateTime.Today.AddDays(11);

			var newSkip = EdiPromptSkip.AddSkip(person1, EdiPromptSkipTypes.Codes.PersonalEmail, newSkipDate);
			AssertNotNull(newSkip);
			AssertEquals("Should reuse the same skip record", existingSkip.PK, newSkip.PK);
			AssertEquals("Skip date should be updated", newSkipDate, newSkip.EPS_SkipUntilDate);
			AssertEquals("Should be no new records", 1, Factory.GetDatabaseCount(typeof(EdiPromptSkip), dataQuery));
			Factory.Save();

			var permanentSkipDate = ZDateTime.Empty;

			var person2Skip = EdiPromptSkip.AddSkip(person2, EdiPromptSkipTypes.Codes.PersonalEmail, permanentSkipDate);
			AssertNotNull(person2Skip);
			AssertNotEquals("Should use a different record since the skip is for a different person", existingSkip.PK, person2Skip.PK);
			AssertEquals(permanentSkipDate, person2Skip.EPS_SkipUntilDate);

			var newPerson2Skip = EdiPromptSkip.AddSkip(person2, EdiPromptSkipTypes.Codes.PersonalEmail, newSkipDate);
			AssertNotNull(newPerson2Skip);
			AssertEquals("Should reuse the same skip record", person2Skip.PK, newPerson2Skip.PK);
			AssertEquals("Skip date should be updated", newSkipDate, newPerson2Skip.EPS_SkipUntilDate);
		}

		[TestDate(2020, 01, 01)]
		public void TestShouldSkipPrompt()
		{
			var person = Factory.NewWithValidTestData<EDIGlbPerson>();
			var promptSkip = EdiPromptSkip.AddSkip(person, EdiPromptSkipTypes.Codes.PersonalEmail, ZDateTime.Today.AddDays(1));
			AssertNotNull(promptSkip);
			Factory.Save();

			AssertEquals("Should skip the personal password prompt for today and tomorrow", true, EdiPromptSkip.ShouldSkipPrompt(person, EdiPromptSkipTypes.Codes.PersonalEmail));
			TestDateAttribute.AddDays(1);
			AssertEquals("Should skip the personal password prompt for today and tomorrow", true, EdiPromptSkip.ShouldSkipPrompt(person, EdiPromptSkipTypes.Codes.PersonalEmail));
			TestDateAttribute.AddDays(1);
			AssertEquals("Skip should no longer be valid", false, EdiPromptSkip.ShouldSkipPrompt(person, EdiPromptSkipTypes.Codes.PersonalEmail));
		}

		[TestDate(2020, 01, 01)]
		public void TestShouldSkipPromptPermanently()
		{
			var person = Factory.NewWithValidTestData<EDIGlbPerson>();
			var promptSkip = EdiPromptSkip.AddSkip(person, EdiPromptSkipTypes.Codes.PersonalEmail, ZDateTime.Empty);
			AssertNotNull(promptSkip);
			Factory.Save();

			AssertEquals("Should skip the personal password prompt forever", true, EdiPromptSkip.ShouldSkipPrompt(person, EdiPromptSkipTypes.Codes.PersonalEmail));
			TestDateAttribute.AddDays(30);
			AssertEquals("Should skip the personal password prompt forever", true, EdiPromptSkip.ShouldSkipPrompt(person, EdiPromptSkipTypes.Codes.PersonalEmail));
			TestDateAttribute.AddYears(10);
			AssertEquals("Should skip the personal password prompt forever", true, EdiPromptSkip.ShouldSkipPrompt(person, EdiPromptSkipTypes.Codes.PersonalEmail));
		}
	}
}
