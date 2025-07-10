using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ConsentingProcessLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConsentingBodyList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consenting Body");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consent001", "Consenting Body Test 001", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, "Consent002", "Consenting Body Test 002", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			Factory.Save();

			var parent = Factory.New<ConsentingProcess>();
			var consentingBodyList = parent.Lookups.ConsentingBodyList;
			CombineAssertions(() =>
			{
				AssertEquals(2, consentingBodyList.Count);
				AssertEquals("Consent001, Consent002",
					consentingBodyList.CodesAsString);
			});
		}
	}
}
