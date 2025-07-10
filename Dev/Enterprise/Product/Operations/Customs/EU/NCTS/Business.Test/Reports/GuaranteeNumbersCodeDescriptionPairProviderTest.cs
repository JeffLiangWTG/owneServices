using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class GuaranteeNumbersCodeDescriptionPairProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var provider = new GuaranteeNumbersCodeDescriptionPairProvider();
			var result = provider.GetCodeDescriptionPairList();
			AssertEquals("CodesAsString", "0000, 3333, 4444, 6666", result.CodesAsString);
		}

		public void TestGetDependenceCodeDescriptionPairListArgument()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var provider = new GuaranteeNumbersCodeDescriptionPairProvider();
			var result = provider.GetDependenceCodeDescriptionPairList(permitHolder1.PK.ToString());
			AssertEquals($"CodesAsString filtering by {nameof(permitHolder1)}", "0000, 3333", result.CodesAsString);

			result = provider.GetDependenceCodeDescriptionPairList(permitHolder2.PK.ToString());
			AssertEquals($"CodesAsString filtering by {nameof(permitHolder2)}", "4444, 6666", result.CodesAsString);
		}

		public void TestGetDependenceCodeDescriptionPairListNullArgument()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var provider = new GuaranteeNumbersCodeDescriptionPairProvider();
			var result = provider.GetDependenceCodeDescriptionPairList(null);
			AssertEquals("CodesAsString", "0000, 3333, 4444, 6666", result.CodesAsString);
		}

		public void TestGetDependenceCodeDescriptionPairListInvalidGuidArgument()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var provider = new GuaranteeNumbersCodeDescriptionPairProvider();
			var result = provider.GetDependenceCodeDescriptionPairList("xyz");
			AssertEquals("EDGE-CASE (should not happen in production): CodesAsString when filter is not a valid ZGuid, filter is ignored", "0000, 3333, 4444, 6666", result.CodesAsString);
		}

		public void TestGetCodeDescriptionPairListNoC009Countries()
		{
			var provider = new GuaranteeNumbersCodeDescriptionPairProvider();
			var result = provider.GetCodeDescriptionPairList();
			AssertEquals("CodesAsString", "", result.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();

			permitHolder1 = Factory.NewWithValidTestData<OrgHeader>();
			permitHolder2 = Factory.NewWithValidTestData<OrgHeader>();
			SetUpGuarantee("TRA", "3333", Core.Constants.CountryCodes.Spain, permitHolder1.PK);
			SetUpGuarantee("TRA", "2222", Core.Constants.CountryCodes.Australia, permitHolder1.PK);
			SetUpGuarantee("XXX", "1111", Core.Constants.CountryCodes.Italy, permitHolder1.PK);
			SetUpGuarantee("TRA", "0000", Core.Constants.CountryCodes.Italy, permitHolder1.PK);
			SetUpGuarantee("TRA", "4444", Core.Constants.CountryCodes.Germany, permitHolder2.PK);
			SetUpGuarantee("COD", "5555", Core.Constants.CountryCodes.Germany, permitHolder2.PK);
			SetUpGuarantee("COD", "6666", Core.Constants.CountryCodes.France, permitHolder2.PK);
			Factory.Save();
		}
		OrgHeader permitHolder1;
		OrgHeader permitHolder2;

		void SetUpGuarantee(ZString type, ZString number, ZString countryCode, ZGuid permitHolderPK)
		{
			var guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guarantee.CPH_Type = type;
			guarantee.CPH_Number = number;
			guarantee.CPH_RN_NKCountryCode = countryCode;
			guarantee.CPH_OH_PermitHolder = permitHolderPK;
		}
	}
}
