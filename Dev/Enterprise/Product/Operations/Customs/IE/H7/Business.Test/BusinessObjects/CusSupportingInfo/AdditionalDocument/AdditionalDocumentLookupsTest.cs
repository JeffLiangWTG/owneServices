using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentLookups))]
	class AdditionalDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestCodeList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "AI44E", "111", "111", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "AI44I", "222", "222", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "AI44I", "333", "333", yesterday, tomorrow);
			var correctAI44ICode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "AI44I", "444", "444", yesterday, tomorrow);
			var correctDC44ICode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC44I", "555", "555", yesterday, tomorrow);
			var correctTD44ICode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "TD44I", "666", "666", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";
			var bill = header.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();

			CombineAssertions(() =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				var codeList = (ZZRefCusCodeListCombinedCollection)additionalDocument.Lookups.CodeList;
				codeList.Load();
				AssertEquals("There should only be one CusCode that meets the conditions", 1, codeList.Count);
				Assert("Should include the correct CusCode", codeList.Contains(correctAI44ICode));

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				codeList = (ZZRefCusCodeListCombinedCollection)additionalDocument.Lookups.CodeList;
				codeList.Load();
				AssertEquals("There should only be one CusCode that meets the conditions", 1, codeList.Count);
				Assert("Should include the correct CusCode", codeList.Contains(correctDC44ICode));

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				codeList = (ZZRefCusCodeListCombinedCollection)additionalDocument.Lookups.CodeList;
				codeList.Load();
				AssertEquals("There should only be one CusCode that meets the conditions", 1, codeList.Count);
				Assert("Should include the correct CusCode", codeList.Contains(correctTD44ICode));
			});
		}
	}
}
