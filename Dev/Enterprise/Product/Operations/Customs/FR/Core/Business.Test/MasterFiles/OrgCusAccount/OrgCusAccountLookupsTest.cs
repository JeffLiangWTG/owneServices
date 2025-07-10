using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MasterFiles.Testing
{
	public class OrgCusAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.CodeList;
				AssertEquals("CodeList", "DEC, DGE, DGI, DTA, DXE, DXI, ECS", list.CodesAsString);
				AssertSame("Cached", list, orgCusAccount.Lookups.CodeList);
			});
		}

		public void TestCodeListDescriptionForDEC()
		{
			AssertEquals("OrgCusAccountCodeList DEC description", "DECO for Delta IE", OrgCusAccountCodeList.Descriptions.DEC);
		}

		public void TestAccountTypeListForDeltaIE()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DEC;
			orgCusAccount.CZ_Type = string.Empty;
			orgCusAccount.CZ_Account = "0000186";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = orgHeader.PK;

			var list = lookups.AccountTypeList;
			AssertEquals("AccountTypeList contains DCC, DCN, HDN", "DCC, DCN, HDN", list.CodesAsString);
			AssertEquals("AccountTypeList description for DCC", "European Centralized Customs Clearance", OrgCusAccountDeltaIETypeList.Descriptions.DCC);
			AssertEquals("AccountTypeList description for DCN", "National Centralized Customs Clearance", OrgCusAccountDeltaIETypeList.Descriptions.DCN);
			AssertEquals("AccountTypeList description for HDN", "Excluding Centralized Customs Clearance", OrgCusAccountDeltaIETypeList.Descriptions.HDN);
			AssertSame("Cached AccountTypeList", list, orgCusAccount.Lookups.AccountTypeList);
		}

		public void TestAccountTypeListForDeltaT()
		{
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			var list = lookups.AccountTypeList;
			AssertEquals("AccountTypeList", OrgCusAccountDeltaTTypeList.Codes.TR, list.CodesAsString);
		}

		public void TestAccountTypeListForDeltaG()
		{
			CombineAssertions(() =>
			{
				orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
				var list = lookups.AccountTypeList;
				AssertEquals("AccountTypeList", "G1, G2", list.CodesAsString);
				AssertEquals("AccountType", ZString.Empty, orgCusAccount.CZ_Type);
				AssertSame("Cached", list, orgCusAccount.Lookups.AccountTypeList);
			});
		}

		public void TestReportingPeriodList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.ReportingPeriodList;
				AssertEquals("ReportingPeriodList", "DAY, TEN, MON", list.CodesAsString);
				AssertSame("Cached", list, orgCusAccount.Lookups.ReportingPeriodList);
			});
		}

		public void TestCustomsOfficesList()
		{
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000002", "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = (ZZRefCusCodeListCombinedCollection)lookups.IssuerList;
				list.Load();
				AssertContainsExactElementsInAnyOrder("Customs Offices", new[] { "FR000001", "FR000002" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, orgCusAccount.Lookups.IssuerList);
			});
		}

		protected override void SetUp()
		{
			orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
		}
		OrgCusAccount orgCusAccount;
		OrgCusAccountLookups lookups => (OrgCusAccountLookups)orgCusAccount.Lookups;
	}
}
