using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CusTempStorageDecLookupsBaseOnlyTest : BusinessObjectLookupsTestCase
	{
		public void TestCusTempStorageRegLineCollection()
		{
			var cusTempStorageRegHeader = Factory.New<CusTempStorageRegHeader>();
			cusTempStorageRegHeader.SRH_Reference = "TEST1";
			cusTempStorageRegHeader.SRH_CustomsOffice = "OFFICE1";
			var cusTempStorageRegLine = cusTempStorageRegHeader.CusTempStorageRegLines.AddNew();
			cusTempStorageRegLine.SRL_LineNumber = 1;
			Factory.Save();

			var storageDec = Factory.New<CusTempStorageDecForTest>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader;
			var storageJobHeader = Factory.New<CusTempStorageJobHeader>();
			storageDec.STH_SJH = storageJobHeader.PK;

			CombineAssertions(() =>
			{
				storageJobHeader.SJH_CustomsOffice = "OFFICE1";
				var collection = (ActiveBusinessObjectCollection<CusTempStorageRegLine>)storageDec.Lookups.CusTempStorageRegLineCollection;
				AssertEquals("SJH_CustomsOffice is valid", true, cusTempStorageRegLine.MatchesFilter(collection.CompleteFilter));

				storageJobHeader.SJH_CustomsOffice = "OFFICE2";
				collection = (ActiveBusinessObjectCollection<CusTempStorageRegLine>)storageDec.Lookups.CusTempStorageRegLineCollection;
				AssertEquals("SJH_CustomsOffice is invalid", false, cusTempStorageRegLine.MatchesFilter(collection.CompleteFilter));

				storageJobHeader.SJH_CustomsOffice = ZString.Empty;
				collection = (ActiveBusinessObjectCollection<CusTempStorageRegLine>)storageDec.Lookups.CusTempStorageRegLineCollection;
				AssertEquals("SJH_CustomsOffice is empty", true, cusTempStorageRegLine.MatchesFilter(collection.CompleteFilter));
			});
		}
	}
}
