using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class AEONumberHelperTest : TestCaseWithFactory
	{
		public void TestGetAEONumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, "AEO Mutual Recognition Country List");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Switzerland, "瑞士", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Austria, "奥地利", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Singapore, "新加坡", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ChinaAEOMutualRecognitionCountries, Core.Constants.CountryCodes.Belarus, "白俄罗斯", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var header = Factory.New<OrgHeader>();
			header.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;

			var cusCode1 = header.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Norway;
			cusCode1.OK_CodeType = "AEO";
			cusCode1.OK_CustomsRegNo = "111111";
			AssertEquals(ZString.Empty, AEONumberHelper.GetAEONumber(header, ZDateTime.Today));

			var cusCode2 = header.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Austria;
			cusCode2.OK_CodeType = "EOR";
			cusCode2.OK_CustomsRegNo = "222222";
			AssertEquals("AT222222", AEONumberHelper.GetAEONumber(header, ZDateTime.Today));

			var cusCode3 = header.CustomsCodes.AddNew();
			cusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Singapore;
			cusCode3.OK_CodeType = "AEO";
			cusCode3.OK_CustomsRegNo = "333333";
			var result = AEONumberHelper.GetAEONumber(header, ZDateTime.Today);
			Assert(result == "AT222222" || result == "SG333333");

			var cusCode4 = header.CustomsCodes.AddNew();
			cusCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Switzerland;
			cusCode4.OK_CodeType = "AEO";
			cusCode4.OK_CustomsRegNo = "444444";
			AssertEquals("CH444444", AEONumberHelper.GetAEONumber(header, ZDateTime.Today));

			header.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belarus;
			var cusCode5 = header.CustomsCodes.AddNew();
			cusCode5.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belarus;
			cusCode5.OK_CodeType = "AEO";
			cusCode5.OK_CustomsRegNo = "1234";
			AssertEquals("BY1234", AEONumberHelper.GetAEONumber(header, ZDateTime.Today));
		}
	}
}
