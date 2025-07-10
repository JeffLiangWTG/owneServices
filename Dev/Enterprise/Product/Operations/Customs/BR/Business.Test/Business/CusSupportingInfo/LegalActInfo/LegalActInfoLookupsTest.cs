using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LegalActInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAcceptanceCriteriasList()
		{
			var parent = Factory.New<LegalActInfo>();
			var additionalTaxTypeList = parent.Lookups.AdditionalTaxTypeList;
			CombineAssertions(() =>
			{
				AssertEquals(5, additionalTaxTypeList.Count);
				AssertEquals("1, 2, 3, 4, 6", additionalTaxTypeList.CodesAsString);
			});
		}

		public void TestLegalActList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "Ex Tariff Legal Act");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "E1", "EXTLA Test 001", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, "E2", "EXTLA Test 002", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var parent = Factory.New<LegalActInfo>();
			var legalActList = parent.Lookups.ExTariffLegalActList;
			CombineAssertions(() =>
			{
				AssertEquals(2, legalActList.Count);
				AssertEquals("E1, E2", legalActList.CodesAsString);
			});
		}

		public void TestIssuingBodyList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "Legal Act Issuing Authority");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "L1", "LAIA Test 001", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, "L2", "LAIA Test 002", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var parent = Factory.New<LegalActInfo>();
			var issuingBodyList = parent.Lookups.LegalActIssuingAuthorityList;
			CombineAssertions(() =>
			{
				AssertEquals(2, issuingBodyList.Count);
				AssertEquals("L1, L2", issuingBodyList.CodesAsString);
			});
		}
	}
}
