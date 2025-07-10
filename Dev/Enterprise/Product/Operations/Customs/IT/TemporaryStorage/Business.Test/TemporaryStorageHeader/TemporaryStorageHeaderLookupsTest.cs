using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using GlbCompanyWrapper = Enterprise.Customs.IT.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransportTypeList()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		var lookups = header.Lookups;

		CombineAssertions("TransportTypeList filtered on the basis of TransportMode", () =>
		{
			header.AMA_TransportMode = ZString.Empty;
			var applicableCodes = lookups.TransportTypeList.CodesAsString;
			AssertEquals("AMA_TransportMode is ZString.Empty", "10, 11, 20, 30, 40, 41, 80, 81", applicableCodes);

			header.AMA_TransportMode = "AIR";
			applicableCodes = lookups.TransportTypeList.CodesAsString;
			AssertEquals("AMA_TransportMode is AIR", "40, 41", applicableCodes);

			header.AMA_TransportMode = "SEA";
			applicableCodes = lookups.TransportTypeList.CodesAsString;
			AssertEquals("AMA_TransportMode is SEA", "10, 11", applicableCodes);

			header.AMA_TransportMode = "RAI";
			applicableCodes = lookups.TransportTypeList.CodesAsString;
			AssertEquals("AMA_TransportMode is RAI", "20", applicableCodes);

			header.AMA_TransportMode = "ROA";
			applicableCodes = lookups.TransportTypeList.CodesAsString;
			AssertEquals("AMA_TransportMode is ROA", "30", applicableCodes);

			header.AMA_TransportMode = "IWT";
			applicableCodes = lookups.TransportTypeList.CodesAsString;
			AssertEquals("AMA_TransportMode is IWT", "80, 81", applicableCodes);
		});
	}

	public void TestCustomsProfileList()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		var companyWrapper = GlbCompanyWrapper.Get(currentCompany);
		AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "10", "AA", "11-001");
		AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "20", "BB", "22-001");
		AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "30", "CC", "33-001");
		AccountBuilderTestHelper.AddNewAccountDetail(companyWrapper, "40", "DD", "44-001");
		currentCompany.Factory.Save();

		CombineAssertions(() =>
		{
			var header = Factory.New<TemporaryStorageHeader>();

			var accountCodes = header.Lookups.CustomsProfileList.CodesAsString;
			AssertEquals("Declarant and representative not selected, list all company accounts", "10, 20, 30, 40", accountCodes);

			var declarantAddressXX = Factory.NewWithValidTestData<OrgAddress>();
			declarantAddressXX.Header.OH_Code = "XX";
			header.AMA_OA_Declarant = declarantAddressXX.PK;

			accountCodes = header.Lookups.CustomsProfileList.CodesAsString;
			AssertEquals("Declarant selected, code does not match any company account, list all company accounts", "10, 20, 30, 40", accountCodes);

			var representativeAddressYY = Factory.NewWithValidTestData<OrgAddress>();
			representativeAddressYY.Header.OH_Code = "YY";
			header.AMA_OA_Representative = representativeAddressYY.PK;

			accountCodes = header.Lookups.CustomsProfileList.CodesAsString;
			AssertEquals("Declarant and representative selected, code do not match any company account, list all company accounts", "10, 20, 30, 40", accountCodes);

			var declarantAddressAA = Factory.NewWithValidTestData<OrgAddress>();
			declarantAddressAA.Header.OH_Code = "AA";
			header.AMA_OA_Declarant = declarantAddressAA.PK;

			accountCodes = header.Lookups.CustomsProfileList.CodesAsString;
			AssertEquals("Declarant selected, code match company account, list only matching company accounts", "10", accountCodes);

			var representativeAddressCC = Factory.NewWithValidTestData<OrgAddress>();
			representativeAddressCC.Header.OH_Code = "CC";
			header.AMA_OA_Representative = representativeAddressCC.PK;

			accountCodes = header.Lookups.CustomsProfileList.CodesAsString;
			AssertEquals("Declarant and representative selected, code match company account, list only matching company accounts", "10, 30", accountCodes);
		});
	}

	public void TestRepresentativeQualificationList()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		AssertEquals("DIR, IND", header.Lookups.RepresentativeQualificationList.CodesAsString);
	}

	public void TestPNTSMessageStatusList()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		AssertContainsExactElementsInAnyOrder("PNTSMessageStatusList should be made of codes from PNTSMessageStatusList.", new string[] { PNTSMessageStatusList.Codes.Acknowledged, PNTSMessageStatusList.Codes.TechnicalFailure, PNTSMessageStatusList.Codes.FunctionalRejection, PNTSMessageStatusList.Codes.Sent, PNTSMessageStatusList.Codes.AcceptedBySystem }, header.Lookups.PNTSMessageStatusList.GetAllCodes());
	}
}

static class AccountBuilderTestHelper
{
	internal static void AddNewAccountDetail(GlbCompanyWrapper companyWrapper, string internalCode, string declarantCode, string authorizedUser)
	{
		var accountDetail = companyWrapper.PasswordCollection.AddNew();
		accountDetail.GP_UserID = internalCode;
		accountDetail.GP_Name = declarantCode;
		accountDetail.GP_MailBoxID = authorizedUser;
	}
}
