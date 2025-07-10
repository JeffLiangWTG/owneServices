using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using GlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestNctsTransitStatusList()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var nctsTransitStatusList = nctsHeader.Lookups.NctsTransitStatusList;
		AssertType<NctsTransitStatusList>("NctsTransitStatusList Type", nctsTransitStatusList);
		AssertSame("NctsTransitStatusList should be cached", nctsTransitStatusList, nctsHeader.Lookups.NctsTransitStatusList);
	}

	public void TestAuthorizationList()
	{
		var header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureMovement = header.MovementHeader;
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var requirement = header.DocAddresses.FindOrCreateWithRequirement(header.ConsignorJobDocAddressRequirement);
		requirement.E2_OA_Address = orgHeader.MainAddress.PK;

		var authorizationTypeACR = IT.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
		var authorizationTypeIssuer = IT.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;
		var permitHolderPK = header.Consignor.OrganisationPK;
		var permitHolder2PK = ZGuid.NewZGuid();
		var startDate = ZDate.Today.AddDays(-10);
		var endDate = ZDate.Today.AddDays(10);

		var authorisationHeader1 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: authorizationTypeACR, permitHolder: permitHolderPK, "1111111", startDate: startDate, endDate: endDate);
		var authorisationHeader2 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: authorizationTypeACR, permitHolder: ZGuid.Empty, "2222222", startDate: startDate, endDate: endDate);
		var authorisationHeader3 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: authorizationTypeIssuer, permitHolder: permitHolderPK, "3333333", startDate: startDate, endDate: endDate);
		var authorisationHeader4 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: authorizationTypeACR, permitHolder: permitHolder2PK, "4444444", startDate: startDate, endDate: endDate);

		CombineAssertions(() =>
		{
			var authorizationList = header.Lookups.AuthorisationNumberList;
			AssertSame("Cached", authorizationList, header.Lookups.AuthorisationNumberList);
			AssertEquals("One Authorization expected", 1, authorizationList.Count);
			AssertEquals("authorizations Holder is Consignor", "1111111", authorizationList[0].Code);
		});
	}

	public void TestRepresentationTypeList()
	{
		Assert(ReferenceEquals(Factory.GetCachedValue<RepresentationTypeList>(), Factory.New<NctsHeader>().Lookups.RepresentationTypeList));
	}

	public void TestSubscribers()
	{
		Factory.New<OrgHeader>().OH_Code = "AA";
		Factory.New<OrgHeader>().OH_Code = "BB";

		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_Code = "STF";
		staff.GS_FullName = "STAFF FULL NAME";
		var staff2Wrapper = IT.Business.GlbStaffWrapper.Get(staff);
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "1234";
		staff2Wrapper.PasswordCollection.AddNew().GP_UserID = "5678";
		Factory.Save();

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-AA", "AA")
			.AppendAccount("22222222222-001", "5678").AppendAccountDetail("5678-BB", "BB")
			.Build();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		var nctsHeaderLookups = nctsHeader.Lookups;
		nctsHeader.BH_CustomsProfile = "";
		AssertEquals(0, nctsHeaderLookups.Subscribers.Count);

		nctsHeader.BH_CustomsProfile = "9999";
		AssertEquals(0, nctsHeaderLookups.Subscribers.Count);

		nctsHeader.BH_CustomsProfile = "1234-AA";
		AssertArrayEqualsByElements(new GlbStaff[] { staff }, nctsHeaderLookups.Subscribers.ToArray());

		nctsHeader.BH_CustomsProfile = "5678-BB";
		AssertArrayEqualsByElements(new GlbStaff[] { staff }, nctsHeaderLookups.Subscribers.ToArray());
	}

	public void TestProfileList()
	{
		var declarantAddressAA = Factory.NewWithValidTestData<OrgAddress>();
		declarantAddressAA.Header.OH_Code = "AA";
		var declarantAddressBB = Factory.NewWithValidTestData<OrgAddress>();
		declarantAddressBB.Header.OH_Code = "BB";
		Factory.Save();

		var currentCompanyPk = GlbCompany.CurrentCompany.PK.ToGuid();

		new AccountCollectionTestBuilder(currentCompanyPk)
			.AppendAccount("11111111111-001", "1234").AppendAccountDetail("1234-AA", "AA")
			.AppendAccount("22222222222-001", "5678").AppendAccountDetail("5678-BB", "BB")
			.Build();

		CustomsProfileListTestHelper.ClearCustomsProfilesLookupsCache(Factory, currentCompanyPk);
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

		CombineAssertions("Declarant not selected", () =>
		{
			nctsHeader.DeclarantAddressPK = ZGuid.Empty;
			var profileList = nctsHeader.Lookups.ProfileList;
			AssertEquals("Two available account details", 2, profileList.Count);
			Assert("1234-AA account detail is available", profileList.ContainsCode("1234-AA"));
			Assert("5678-BB account detail is available", profileList.ContainsCode("5678-BB"));
		});

		CombineAssertions("'AA' declarant selected", () =>
		{
			nctsHeader.DeclarantAddressPK = declarantAddressAA.PK;
			var profileList = nctsHeader.Lookups.ProfileList;
			AssertEquals("Only one available account detail", 1, profileList.Count);
			Assert("1234-AA account detail is available", profileList.ContainsCode("1234-AA"));
			Assert("5678-BB account detail is NOT available", !profileList.ContainsCode("5678-BB"));
		});

		CombineAssertions("'BB' declarant selected", () =>
		{
			nctsHeader.DeclarantAddressPK = declarantAddressBB.PK;
			var profileList = nctsHeader.Lookups.ProfileList;
			AssertEquals("Only one available account detail", 1, profileList.Count);
			Assert("1234-AA account detail is NOT available", !profileList.ContainsCode("1234-AA"));
			Assert("5678-BB account detail is available", profileList.ContainsCode("5678-BB"));
		});
	}

	public void TestPhase5ProfileList_WhenOrganizationMatches()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
		AddNewAccountDetail(companyWrapper, "1111", "AA", "11111111111-001");
		AddNewAccountDetail(companyWrapper, "2222", "BB", "22222222222-001");
		AddNewAccountDetail(companyWrapper, "3333", "CC", "33333333333-001");
		AddNewAccountDetail(companyWrapper, "4444", "DD", "44444444444-001");
		currentCompany.Factory.Save();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		var organizationAA = Factory.New<OrgHeader>();
		organizationAA.OH_Code = "AA";

		var organizationBB = Factory.New<OrgHeader>();
		organizationBB.OH_Code = "BB";

		var organizationCC = Factory.New<OrgHeader>();
		organizationCC.OH_Code = "CC";

		var organizationDD = Factory.New<OrgHeader>();
		organizationDD.OH_Code = "DD";

		CombineAssertions(() =>
		{
			AssertContainsInternalCode("When no trader has been selected", nctsHeader, 4, "1111", "2222", "3333", "4444");
			nctsHeader.Principal.OrganisationPK = organizationAA.PK;
			AssertContainsInternalCode("When Principal has been selected", nctsHeader, 1, "1111");

			nctsHeader.Consignor.OrganisationPK = organizationBB.PK;
			AssertContainsInternalCode("When Principal and Consignor have been selected", nctsHeader, 2, "1111", "2222");

			nctsHeader.Consignee.OrganisationPK = organizationCC.PK;
			AssertContainsInternalCode("When Principal, Consignor and Consignee have been selected", nctsHeader, 3, "1111", "2222", "3333");

			nctsHeader.MovementHeader.Representative.OrganisationPK = organizationDD.PK;
			AssertContainsInternalCode("When Principal, Consignor, Consignee and Representative have been selected", nctsHeader, 4, "1111", "2222", "3333", "4444");
		});
	}

	public void TestPhase5DepartureProfileList_WhenNoOrganizationMatches()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
		AddNewAccountDetail(companyWrapper, "1111", "AA", "11111111111-001");
		AddNewAccountDetail(companyWrapper, "2222", "BB", "22222222222-001");
		AddNewAccountDetail(companyWrapper, "3333", "CC", "33333333333-001");
		AddNewAccountDetail(companyWrapper, "4444", "DD", "44444444444-001");
		currentCompany.Factory.Save();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		var organizationEE = Factory.New<OrgHeader>();
		organizationEE.OH_Code = "EE";

		var organizationFF = Factory.New<OrgHeader>();
		organizationFF.OH_Code = "FF";

		var organizationGG = Factory.New<OrgHeader>();
		organizationGG.OH_Code = "GG";

		var organizationHH = Factory.New<OrgHeader>();
		organizationHH.OH_Code = "HH";

		CombineAssertions(() =>
		{
			AssertContainsInternalCode("When no trader has been selected", nctsHeader, 4, "1111", "2222", "3333", "4444");

			nctsHeader.Principal.OrganisationPK = organizationEE.PK;
			AssertContainsInternalCode("When Principal has been selected but not matches with company accounts", nctsHeader, 4, "1111", "2222", "3333", "4444");

			nctsHeader.Consignor.OrganisationPK = organizationFF.PK;
			AssertContainsInternalCode("When Principal and Consignor have been selected but not matches with company accounts", nctsHeader, 4, "1111", "2222", "3333", "4444");

			nctsHeader.Consignee.OrganisationPK = organizationGG.PK;
			AssertContainsInternalCode("When Principal, Consignor and Consignee have been selected but not matches with company accounts", nctsHeader, 4, "1111", "2222", "3333", "4444");

			nctsHeader.MovementHeader.Representative.OrganisationPK = organizationHH.PK;
			AssertContainsInternalCode("When Principal, Consignor, Consignee and Representative have been selected byt not matches with company accounts", nctsHeader, 4, "1111", "2222", "3333", "4444");
		});
	}

	void AssertContainsInternalCode(string assertionMessage, NctsHeader nctsHeader, int expectedProfilesCount, params string[] expectedInternalCodes)
	{
		var profiles = nctsHeader.Lookups.ProfileList;
		AssertEquals("Available profiles count", expectedProfilesCount, profiles.Count);
		AssertContainsExactElementsInAnyOrder(assertionMessage, expectedInternalCodes, profiles.GetAllCodes());
	}

	GlbMauExternalPassword AddNewAccountDetail(GlbCompanyWrapper companyWrapper, string internalCode, string declarantCode, string authorizedUser)
	{
		var accountDetail = companyWrapper.PasswordCollection.AddNew();
		accountDetail.GP_UserID = internalCode;
		accountDetail.GP_Name = declarantCode;
		accountDetail.GP_MailBoxID = authorizedUser;
		return accountDetail;
	}
}
