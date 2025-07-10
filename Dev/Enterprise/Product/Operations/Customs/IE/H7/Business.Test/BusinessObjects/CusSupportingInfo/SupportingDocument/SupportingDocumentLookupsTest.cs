using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class SupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupportingDocumentCodeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";
			var documents = header.SupportingDocuments;
			var supportingDocument = documents.AddNew();
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC40E", "270", "Delivery note", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC44I", "271", "Packing list", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "DC44I", "325", "Proforma invoice", yesterday, tomorrow);
			var correctCusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC44I", "N952", "TIR Carnet", yesterday, tomorrow);
			Factory.Save();

			var codeList = (ZZRefCusCodeListCombinedCollection)supportingDocument.Lookups.CodeList;
			codeList.Load();

			CombineAssertions(() =>
			{
				AssertEquals(1, codeList.Count);
				AssertEquals(true, codeList.Contains(correctCusCode));
			});
		}
	}
}
