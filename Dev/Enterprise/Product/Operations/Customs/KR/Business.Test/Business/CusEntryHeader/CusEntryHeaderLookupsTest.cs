using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Lookups.EntryHeader, parent);
		}

		public void TestCH_MessageTypeList()
		{
			var entry = Factory.New<CusEntryHeader>();
			var messageTypeList = entry.Lookups.CH_MessageTypeList;
			AssertEquals(5, messageTypeList.Count);
			AssertEquals("EXP, IMP, 5DQ, 5DP, D87", messageTypeList.CodesAsString);
		}

		public void TestCountryOfOriginsReqDHRList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.DetailedFTACountries, "Detailed FTA Countries (DHR)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "CN", "중국", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "ID", "인도네시아", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "IN", "인도", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "VN", "베트남", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var entry = Factory.New<CusEntryHeader>();
			var countryOfOriginsReqDHRList = entry.Lookups.CountryOfOriginsReqDHRList;
			AssertEquals(4, countryOfOriginsReqDHRList.Count);

			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "CN"));
			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "중국"));
			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "ID"));
			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "인도네시아"));
			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "IN"));
			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "인도"));
			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "VN"));
			Assert(countryOfOriginsReqDHRList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "베트남"));
		}
	}
}
