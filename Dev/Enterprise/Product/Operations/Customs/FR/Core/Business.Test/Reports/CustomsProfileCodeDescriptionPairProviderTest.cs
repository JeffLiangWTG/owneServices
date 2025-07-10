using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Reports.Testing
{
	class CustomsProfileCodeDescriptionPairProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "OH1";

			var orgCusAccountDGI = Factory.New<OrgCusAccount>();
			orgCusAccountDGI.CZ_Code = "DGI";
			orgCusAccountDGI.CZ_Type = "G1";
			orgCusAccountDGI.CZ_OH = orgHeader1.PK;
			orgCusAccountDGI.CZ_Account = "DGI001";
			orgCusAccountDGI.CZ_Issuer = "CUSOF001";
			orgCusAccountDGI.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "OH2";

			var orgCusAccountDGE = Factory.New<OrgCusAccount>();
			orgCusAccountDGE.CZ_Code = "DGE";
			orgCusAccountDGE.CZ_Type = "G1";
			orgCusAccountDGE.CZ_OH = orgHeader2.PK;
			orgCusAccountDGE.CZ_Account = "DGI002";
			orgCusAccountDGE.CZ_Issuer = "CUSOF002";
			orgCusAccountDGE.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var orgCusAccountXXX = Factory.New<OrgCusAccount>();
			orgCusAccountXXX.CZ_Code = "DTA";
			orgCusAccountXXX.CZ_Type = "G1";
			orgCusAccountXXX.CZ_OH = orgHeader1.PK;
			orgCusAccountXXX.CZ_Account = "XXX";
			orgCusAccountXXX.CZ_Issuer = "CUSX";
			orgCusAccountXXX.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			Factory.Save();

			CombineAssertions(() =>
			{
				var tester = new CustomsProfileCodeDescriptionPairProvider().GetCodeDescriptionPairList();
				AssertEquals(2, tester.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "DGI001", "DGI002" }, tester.GetAllCodes());
			});
		}
	}
}
