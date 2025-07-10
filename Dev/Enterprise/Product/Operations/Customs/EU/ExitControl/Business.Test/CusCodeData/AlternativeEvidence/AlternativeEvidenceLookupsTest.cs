using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	class AlternativeEvidenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170, "AlternativeEvidence");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170, "A1", "A1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170, "A2", "A2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170, "B1", "B1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170, "B2", "B2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var alternativeEvidence1 = Factory.New<CusExitReport>().AlternativeEvidences.AddNew();
			var list1 = alternativeEvidence1.Lookups.CY_CodeList;

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			var header = Factory.New<CusExitHeader>();
			header.CXH_GC_Company = company.PK;
			var alternativeEvidence2 = header.CusExitReports.AddNew().AlternativeEvidences.AddNew();
			var list2 = alternativeEvidence2.Lookups.CY_CodeList;

			CombineAssertions(() =>
			{
				AssertSame("Cached", list1, alternativeEvidence1.Lookups.CY_CodeList);
				var codeList = string.Join(",", list1.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).OrderBy(p => p));
				AssertEquals("codeList", "A1,A2", codeList);

				AssertSame("Cached", list2, alternativeEvidence2.Lookups.CY_CodeList);
				codeList = string.Join(",", list2.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code).OrderBy(p => p));
				AssertEquals("codeList", "B1,B2", codeList);
			});
		}
	}
}
