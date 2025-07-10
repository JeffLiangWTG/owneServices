using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AccountCustomsProfileListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Required factory", () => new AccountCustomsProfileListProvider(factory: null, GetSupportingData(new List<OrgHeader>().AsReadOnly())));
		AssertExceptionThrown<ArgumentNullException>("Required supportingData", () => new AccountCustomsProfileListProvider(Factory, supportingData: null));
	}

	public void TestGetAccountDetailsFilteredByEligibleOrganizations()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			SetUpCompanyAccounts();

			var supportingData = GetSupportingData(new List<OrgHeader> { orgHeaderAA, orgHeaderREP1 }.AsReadOnly());
			var customsProfileListProvider = (ICustomsProfileListProvider)new AccountCustomsProfileListProvider(Factory, supportingData);
			var availableAccountDetails = customsProfileListProvider.GetAccountDetails();

			const string expectedElementsAsString =
				"INTCODE1 - 11111111111-001 - AA\r\n" +
				"INTCODE1REP1 - 11111111111-001 - REP1\r\n" +
				"INTCODE2REP1 - 22222222222-001 - REP1";

			AssertEquals("ElementsAsString", expectedElementsAsString, availableAccountDetails.ElementsAsString);
			AssertSame("Cached", availableAccountDetails, customsProfileListProvider.GetAccountDetails());
		}
	}

	public void TestGetAccountDetailsFilteredByCompany()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			SetUpCompanyAccounts();

			var supportingData = GetSupportingData(new List<OrgHeader>() { null }.AsReadOnly());
			var customsProfileListProvider = (ICustomsProfileListProvider)new AccountCustomsProfileListProvider(Factory, supportingData);
			var availableAccountDetails = customsProfileListProvider.GetAccountDetails();

			const string expectedElementsAsString =
				"INTCODE1 - 11111111111-001 - AA\r\n" +
				"INTCODE1REP1 - 11111111111-001 - REP1\r\n" +
				"INTCODE2 - 22222222222-001 - BB\r\n" +
				"INTCODE2REP1 - 22222222222-001 - REP1";

			AssertEquals("ElementsAsString", expectedElementsAsString, availableAccountDetails.ElementsAsString);
			AssertSame("Cached", availableAccountDetails, customsProfileListProvider.GetAccountDetails());
		}
	}

	public void TestGetAccountDetailsWhenEligibleOrganizationsNotMatchesCompany()
	{
		using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
		{
			SetUpCompanyAccounts();

			var supportingData = GetSupportingData(new List<OrgHeader>() { orgHeaderREPZ }.AsReadOnly());
			var customsProfileListProvider = (ICustomsProfileListProvider)new AccountCustomsProfileListProvider(Factory, supportingData);
			var availableAccountDetails = customsProfileListProvider.GetAccountDetails(true);

			const string expectedElementsAsString =
				"INTCODE1 - 11111111111-001 - AA\r\n" +
				"INTCODE1REP1 - 11111111111-001 - REP1\r\n" +
				"INTCODE2 - 22222222222-001 - BB\r\n" +
				"INTCODE2REP1 - 22222222222-001 - REP1";

			AssertEquals("ElementsAsString", expectedElementsAsString, availableAccountDetails.ElementsAsString);
			AssertSame("Cached", availableAccountDetails, customsProfileListProvider.GetAccountDetails(true));
		}
	}

	public void TestDefaultCode()
	{
		SetUpCompanyAccounts();

		CombineAssertions(() =>
		{
			AssertDefaultCode("When AccountDetails only has 1 element", expectedDefaultCode: "INTCODE1", orgHeaderAA);
			AssertDefaultCode("When AccountDetails has 0 or more than 1 element", expectedDefaultCode: "", orgHeaderAA, orgHeaderBB);
		});

		void AssertDefaultCode(string assertionMessage, string expectedDefaultCode, params OrgHeader[] organizations)
		{
			var supportingData = GetSupportingData(organizations.ToList().AsReadOnly());
			var customsProfileListProvider = (ICustomsProfileListProvider)new AccountCustomsProfileListProvider(Factory, supportingData);
			var availableAccountDetails = customsProfileListProvider.GetAccountDetails();
			AssertEquals(assertionMessage, expectedDefaultCode, availableAccountDetails.DefaultCode);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		orgHeaderAA = Factory.New<OrgHeader>();
		orgHeaderAA.OH_Code = "AA";
		orgHeaderBB = Factory.New<OrgHeader>();
		orgHeaderBB.OH_Code = "BB";
		orgHeaderREP1 = Factory.New<OrgHeader>();
		orgHeaderREP1.OH_Code = "REP1";
		orgHeaderREPZ = Factory.New<OrgHeader>();
		orgHeaderREPZ.OH_Code = "REPZ";
		}

	OrgHeader orgHeaderAA;
	OrgHeader orgHeaderBB;
	OrgHeader orgHeaderREP1;
	OrgHeader orgHeaderREPZ;

	ICustomsProfileListProviderSupportingData GetSupportingData(IReadOnlyCollection<OrgHeader> orgHeaders)
	{
		var mock = new Mock<ICustomsProfileListProviderSupportingData>();
		mock.Setup(m => m.CompanyPK).Returns(GlbCompany.CurrentCompany.PK);
		mock.Setup(m => m.GetEligibleOrganizations()).Returns(orgHeaders);
		return mock.Object;
	}

	void SetUpCompanyAccounts()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
		CreateAccountDetail(companyWrapper, "INTCODE2", "BB", "22222222222-001");
		CreateAccountDetail(companyWrapper, "INTCODE2REP1", "REP1", "22222222222-001");
		CreateAccountDetail(companyWrapper, "INTCODE1", "AA", "11111111111-001");
		CreateAccountDetail(companyWrapper, "INTCODE1REP1", "REP1", "11111111111-001");
		currentCompany.Factory.Save();
	}

	GlbMauExternalPassword CreateAccountDetail(GlbCompanyWrapper companyWrapper, string internalCode, string declarantCode, string authorizedUser)
	{
		var accountDetail = companyWrapper.PasswordCollection.AddNew();
		accountDetail.GP_UserID = internalCode;
		accountDetail.GP_Name = declarantCode;
		accountDetail.GP_MailBoxID = authorizedUser;
		return accountDetail;
	}
}
