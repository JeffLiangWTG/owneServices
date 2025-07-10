using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class TransportDocumentMasterLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportNumberTypeList()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "C0754");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "DE01", "DE01 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_C0754, "DE02", "DE02 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var lookups = transportDocumentMaster.Lookups;
			var transportNumberTypeList = lookups.TransportNumberTypeList;
			transportNumberTypeList.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { "DE01", "DE02" }, transportNumberTypeList.Select(x => x.ZZD_Code));
				AssertSame("Cached", transportNumberTypeList, lookups.TransportNumberTypeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			cusprlStorageLine = storageDec.CusTempStorageLines.AddNew();
			transportDocumentMaster = cusprlStorageLine.TransportDocumentMaster;
		}
		CusTempStorageJobHeader storageHeader;
		CUSPRLCusTempStorageLine cusprlStorageLine;
		TransportDocumentMaster transportDocumentMaster;
	}
}
