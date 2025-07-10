using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PremiseIDMatcherTest : TestCaseWithFactory
	{
		public void TestOrgIsDepotWithPremiseID()
		{
			AssertEquals(true, PremiseIDMatcher.OrgIsDepotWithPremiseID(GetOrg(true, OurPremiseID, OtherPremiseID1), OurPremiseID));
			AssertEquals(true, PremiseIDMatcher.OrgIsDepotWithPremiseID(GetOrg(true, OtherPremiseID1, OurPremiseID), OurPremiseID));
			AssertEquals(false, PremiseIDMatcher.OrgIsDepotWithPremiseID(GetOrg(false, OurPremiseID, OtherPremiseID1), OurPremiseID));
			AssertEquals(false, PremiseIDMatcher.OrgIsDepotWithPremiseID(GetOrg(true, OtherPremiseID1, OtherPremiseID2), OurPremiseID));
			AssertEquals(false, PremiseIDMatcher.OrgIsDepotWithPremiseID(GetOrg(true, OtherPremiseID2, OtherPremiseID1), OurPremiseID));
			AssertEquals(false, PremiseIDMatcher.OrgIsDepotWithPremiseID(null, OurPremiseID));
		}

		public void TestOtherMethods()
		{
			const string abn1 = "98765432109";
			const string abn2 = "54321678904";

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			var org1 = currentCompany.OrgProxy;
			var org2 = Factory.New<OrgHeader>();

			currentCompany.Branches.AddNew().GB_OH_OrgProxy = org2.PK;

			Assert("precondition: orgproxies are different", org1 != org2);
			AssertNotNull("precondition: org1 proxy is not null", org1);
			AssertNotNull("precondition: org2 proxy is not null", org2);

			org1.OH_IsUnpackDepot = true;
			org2.OH_IsUnpackDepot = true;

			org1.MainAddress.LocalControlledPremisesID = OurPremiseID;
			org2.MainAddress.LocalControlledPremisesID = OtherPremiseID1;

			org1.LocalBusinessRegNo = abn1;
			org2.LocalBusinessRegNo = abn2;

			AssertEquals(true, PremiseIDMatcher.CompanyHasDepotWithPremiseID(currentCompany, OurPremiseID));
			AssertEquals(abn1, PremiseIDMatcher.ABNForCompany(currentCompany, OurPremiseID));

			org1.MainAddress.LocalControlledPremisesID = OtherPremiseID2;

			AssertEquals(false, PremiseIDMatcher.CompanyHasDepotWithPremiseID(currentCompany, OurPremiseID));
			AssertEquals(ZString.Empty, PremiseIDMatcher.ABNForCompany(currentCompany, OurPremiseID));

			org2.MainAddress.LocalControlledPremisesID = OurPremiseID;

			AssertEquals(true, PremiseIDMatcher.CompanyHasDepotWithPremiseID(currentCompany, OurPremiseID));
			AssertEquals(abn2, PremiseIDMatcher.ABNForCompany(currentCompany, OurPremiseID));
		}

		OrgHeader GetOrg(ZBool isUnpack, ZString premise1, ZString premise2)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_IsUnpackDepot = isUnpack;
			var address1 = result.MainAddress;
			var address2 = result.Addresses.AddNew();
			AssertEquals(2, result.Addresses.Count);
			address1.LocalControlledPremisesID = premise1;
			address2.LocalControlledPremisesID = premise2;
			return result;
		}

		const string OurPremiseID = "1000";
		const string OtherPremiseID1 = "2000";
		const string OtherPremiseID2 = "3000";
	}
}
