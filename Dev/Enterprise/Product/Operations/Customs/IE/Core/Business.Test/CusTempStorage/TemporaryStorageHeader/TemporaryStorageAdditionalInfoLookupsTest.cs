using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	class TemporaryStorageAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			AssertEquals("INF, REF, TRA", addInfo.Lookups.SubTypeList.CodesAsString);
		}

		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ireland = Core.Constants.CountryCodes.Ireland;
			var ai44n = "AI44N";
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingDataGrouping(ireland);
			helper.CreateNewOrGetExistingCusCodeType(ai44n, "NCTS Additional Information", ireland);
			helper.CreateNewOrGetExistingCusCodeList(ireland, ai44n, "20100", "Export from one EFTA country subject to restriction or export from the Union subject to restriction", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(ireland, ai44n, "20200", "Export from one EFTA country subject to duties or export from the Union subject to duties", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(ireland, ai44n, "20300", "Export", startDate, endDate);
			var ar44n = "AR44N";
			helper.CreateCusCodeType(ar44n, "NCTS Additional Reference", ireland);
			helper.CreateNewOrGetExistingCusCodeList(ireland, ar44n, "Y015", "The rough diamonds are contained in tamper-resistant containers, and the seals applied at export by the participant (Kimberley process) are not broken", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(ireland, ar44n, "Y022", "Consignor / exporter (AEO certificate number)", startDate, endDate);
			var europeanUnion = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var td44t = "TD44T";
			helper.CreateNewOrGetExistingDataGrouping(europeanUnion);
			helper.CreateNewOrGetExistingCusCodeType(td44t, "Transport document Type", europeanUnion);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnion, td44t, "C624", "Form 302", startDate, endDate);
			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var addInfo = bill.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = string.Empty;
			AssertEquals(string.Empty, GetCodeList());

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			AssertEquals("20100, 20200, 20300", GetCodeList());

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			AssertEquals("Y015, Y022", GetCodeList());

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			AssertEquals("C624", GetCodeList());

			string GetCodeList()
			{
				var codeList = addInfo.Lookups.CodeList;
				if (codeList is ZZRefCusCodeListCombinedCollection refCodeList && !refCodeList.Any())
				{
					refCodeList.Load();
				}
				return string.Join(", ", codeList.Cast<ICodeDescription>().Select(pair => pair.Code).OrderBy(code => code));
			}
		}
	}
}
