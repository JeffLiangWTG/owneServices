using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSDISQueryLookupTests : TestCaseWithFactory
	{
		public void TestProfileLookup()
		{
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "ABC", "12345123451234", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, GlbCompany.CurrentCompany.PK, "DEF", "12345123451234", PasswordTypesList.Codes.CDS);
			var testCompany = TestDataHelper.CreateCompanyAndBranch(Factory, "X");
			TestDataHelper.CreateCredentials(Factory, testCompany.PK, "QWE", "987876765654", PasswordTypesList.Codes.CDS);
			TestDataHelper.CreateCredentials(Factory, testCompany.PK, "RTY", "987876765654", PasswordTypesList.Codes.CDS);

			var profiles = message.Lookups.ProfileList;
			Func<string, string> genProfileCode = (badge) => $"12345123451234.{badge}";

			AssertEquals("Expecting 2", 2, profiles.Count);
			Assert("Check Content", profiles.ContainsOnly(genProfileCode("ABC"), genProfileCode("DEF")));
		}

		public void TestDeclarationCategoriesLookup()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "IM", "EX", "CO", "ALL" }, message.Lookups.DeclarationCategories.GetAllCodes());
		}

		public void TestDeclarationStatusesLookup()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "Cleared", "Uncleared", "Rejected", "All" }, message.Lookups.DeclarationStatuses.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<CDSDISQueryMessage>();
		}

		CDSDISQueryMessage message;
	}
}
