using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CUSPCSConsolidatedCusTempStorageLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOwnerReferenceTypeList()
		{
			var ownerReferenceTypeList = storageLine.Lookups.OwnerReferenceTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AWB, ULD", ownerReferenceTypeList.CodesAsString);
				AssertSame("Cached", ownerReferenceTypeList, Factory.GetCachedValue("CUSPCSConsolidatedCusTempStorageLineLookups.OwnerReferenceTypeList", () => new CodeDescriptionPairList()));
			});
		}

		public void TestCusTempStorageRegLineCollection()
		{
			var cusTempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader.SRH_Reference = "TEST1";
			cusTempStorageRegHeader.SRH_CustomsOffice = "OFFICE1";
			var cusTempStorageRegLine = cusTempStorageRegHeader.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine.SRL_LineNumber = 1;
			Factory.Save();

			CombineAssertions(() =>
			{
				storageLine.StorageHeader.SJH_CustomsOffice = "OFFICE1";
				var collection = (ActiveBusinessObjectCollection<CusTempStorageRegLine>)storageLine.Lookups.CusTempStorageRegLineCollection;
				AssertEquals("SJH_CustomsOffice is valid", expected: true, cusTempStorageRegLine.MatchesFilter(collection.CompleteFilter));

				storageLine.StorageHeader.SJH_CustomsOffice = "OFFICE2";
				collection = (ActiveBusinessObjectCollection<CusTempStorageRegLine>)storageLine.Lookups.CusTempStorageRegLineCollection;
				AssertEquals("SJH_CustomsOffice is invalid", expected: false, cusTempStorageRegLine.MatchesFilter(collection.CompleteFilter));

				storageLine.StorageHeader.SJH_CustomsOffice = ZString.Empty;
				collection = (ActiveBusinessObjectCollection<CusTempStorageRegLine>)storageLine.Lookups.CusTempStorageRegLineCollection;
				AssertEquals("SJH_CustomsOffice is empty", expected: true, cusTempStorageRegLine.MatchesFilter(collection.CompleteFilter));
				AssertSame("Cached", collection, (ActiveBusinessObjectCollection<CusTempStorageRegLine>)storageLine.Lookups.CusTempStorageRegLineCollection);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var dec = storageHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageLine = dec.CusTempStorageLines.AddNew();
		}
		CUSPCSConsolidatedCusTempStorageLine storageLine;
	}
}
