using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	public class AdditionalInfoSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupportingDocumentCodeList()
		{
			var testCountryCode = Core.Constants.CountryCodes.Ireland;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(testCountryCode))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = "LV2";
				var bill = header.Bills.AddNew();
				var requestedDocument = bill.RequestedDocuments.AddNew();
				var sendingObject = new AdditionalInfoSendingObject(bill, null, requestedDocument);

				var today = ZDateTime.Now;
				var yesterday = today.AddDays(-1);
				var tomorrow = today.AddDays(1);
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingCusCodeList(testCountryCode, "DC40E", "270", "Delivery note", yesterday, tomorrow);
				helper.CreateNewOrGetExistingCusCodeList(testCountryCode, "DC44I", "271", "Packing list", yesterday, yesterday);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "DC44I", "325", "Proforma invoice", yesterday, tomorrow);
				var correctCusCode = helper.CreateNewOrGetExistingCusCodeList(testCountryCode, "DC44I", "N952", "TIR Carnet", yesterday, tomorrow);
				Factory.Save();

				var codeList = (ZZRefCusCodeListCombinedCollection)sendingObject.Lookups.DocumentTypeList;
				codeList.Load();
				var temp = codeList.ToArray();

				CombineAssertions(() =>
				{
					AssertEquals(1, codeList.Count);
					Assert(codeList.Contains(correctCusCode));
				});
			}
		}
	}
}
