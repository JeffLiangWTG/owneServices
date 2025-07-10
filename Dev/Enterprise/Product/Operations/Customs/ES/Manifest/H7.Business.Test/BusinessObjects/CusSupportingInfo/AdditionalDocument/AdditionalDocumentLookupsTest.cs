using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(AdditionalDocumentLookups))]
	sealed class AdditionalDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestCodeList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, "XXXXX", "111", "111", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, "TD44G", "222", "222", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "TD44G", "333", "333", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, "TD44G", "444", "444", yesterday, tomorrow);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var additionalDocument = header.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			var codeList = (CodeDescriptionPairList)additionalDocument.Lookups.CodeList;
			Assert("Should include the correct CusCode", codeList.ContainsCode("444"));
			Assert("Should not include the incorrect CusCode", !codeList.ContainsCode("111"));
			Assert("Should not include the incorrect CusCode", !codeList.ContainsCode("222"));
			Assert("Should not include the incorrect CusCode", !codeList.ContainsCode("333"));
		}
	}
}
