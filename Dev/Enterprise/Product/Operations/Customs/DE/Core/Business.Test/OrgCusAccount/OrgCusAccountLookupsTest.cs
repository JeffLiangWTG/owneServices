using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class OrgCusAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			CombineAssertions(() =>
			{
				var list = Lookups.CodeList;
				AssertEquals("CodeList", "10, 15, 20", list.CodesAsString);
				AssertSame(list, orgCusAccount.Lookups.CodeList);
			});
		}

		public void TestAccountTypeList()
		{
			CombineAssertions(() =>
			{
				var list = Lookups.AccountTypeList;
				AssertEquals("AccountTypeList", "E, F", list.CodesAsString);
				AssertSame(list, orgCusAccount.Lookups.AccountTypeList);
			});
		}

		public void TestPrefixList()
		{
			CombineAssertions(() =>
			{
				var list = Lookups.IssuerList;
				AssertEquals("IssuerList", "B, C, CB, D, EF, F, FR, H, HB, HH, HR, K, KA, KI, KO, M, MD, MS, N, S, SB", ((CodeDescriptionPairList)list).CodesAsString);
				AssertSame(list, orgCusAccount.Lookups.IssuerList);
			});
		}

		protected override void SetUp()
		{
			orgCusAccount = Factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		}
		OrgCusAccount orgCusAccount;
		OrgCusAccountLookups Lookups => (OrgCusAccountLookups)orgCusAccount.Lookups;
	}
}
