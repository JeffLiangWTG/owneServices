using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.TNT.NZ.Testing
{
	class NZOrgMatcherTest : TestCaseWithFactory
	{
		public void TestAdditionalCheckForMatchedOrganisation()
		{
			OrgHeader orgForMatch = SetupAnOrgForMatching("ABC Company", "Main Address Line 1", "Main Address Line 2", "Auckland", "123456");
			OrgPatternMatch patternMatch = Factory.New<OrgPatternMatch>();
			patternMatch.OS_OH = RoyalAcmeOrg.PK;
			patternMatch.EncodeOrganisation(RoyalAcmeOrg, new StringWithLanguage(RoyalAcmeOrg.OH_FullName, RoyalAcmeOrg.OH_Language), RoyalAcmeOrg.MainAddress, "");
			NZOrgMatcherForTest orgMatcher = new NZOrgMatcherForTest(Factory);
			AssertEquals("return false as different company name", false, orgMatcher.AdditionalOrgPatternMatch(orgForMatch, patternMatch));
			orgForMatch.OH_FullName = "ROYAL ACME MEGA FIREWORKS CO";
			AssertEquals("return true as company name matched by soundex", true, orgMatcher.AdditionalOrgPatternMatch(orgForMatch, patternMatch));
			orgForMatch.OH_FullName = "ROYAL At MEGA FIREWORKS CO";
			AssertEquals("return false as company name is not an exact match by soundex", false, orgMatcher.AdditionalOrgPatternMatch(orgForMatch, patternMatch));
			orgForMatch.OH_FullName = "RILEY AXON MEGA FIREWORKS CO";
			AssertEquals("return true as company name matched by soundex", true, orgMatcher.AdditionalOrgPatternMatch(orgForMatch, patternMatch));
			orgForMatch.OH_FullName = "RILEY AXON MEGA COMPANY PTY LTD";
			AssertEquals("return false as company name is not an exact match by soundex", false, orgMatcher.AdditionalOrgPatternMatch(orgForMatch, patternMatch));
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupOrganisation();
		}

		OrgHeader SetupAnOrgForMatching(ZString companyName, ZString addressLine1, ZString addressLine2, ZString city, ZString phoneNo)
		{
			ABCOrg = Factory.NewWithValidTestData<OrgHeader>();
			ABCOrg.OH_FullName = companyName;
			ABCOrg.MainAddress.OA_Address1 = addressLine1;
			ABCOrg.MainAddress.OA_Address2 = addressLine2;
			ABCOrg.MainAddress.OA_City = city;
			ABCOrg.MainAddress.OA_Phone = phoneNo;
			ABCOrg.OH_IsConsignee = true;
			return ABCOrg;
		}

		void SetupOrganisation()
		{
			RoyalAcmeOrg = Factory.NewWithValidTestData<OrgHeader>();
			RoyalAcmeOrg.OH_FullName = "ROYAL ACME MEGA FIREWORKS COMPANY PTY LTD";
			RoyalAcmeOrg.MainAddress.OA_Address1 = "Main Address line 1";
			RoyalAcmeOrg.MainAddress.OA_Address2 = "Main Address line 2";
			RoyalAcmeOrg.MainAddress.OA_City = "Auckland";
			RoyalAcmeOrg.MainAddress.OA_Phone = "123456";
			RoyalAcmeOrg.OH_RL_NKClosestPort = "NZAKL";
			Factory.Save();
		}

		OrgHeader RoyalAcmeOrg, ABCOrg;
		class NZOrgMatcherForTest : NZOrgMatcher
		{
			public NZOrgMatcherForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new bool AdditionalOrgPatternMatch(OrgHeader orgToMatch, OrgPatternMatch match)
			{
				return base.AdditionalOrgPatternMatch(orgToMatch, match);
			}
		}
	}
}
