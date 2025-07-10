using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONConsolidatedCusTempStorageLineProvider))]
	public class PRLCONConsolidatedCusTempStorageLineProviderTest : CusTempStorageLineProviderAbstractTest<PRLCONConsolidatedCusTempStorageLineProvider>
	{
		public void TestCustomsAuthorisationNumber_NoCustodian()
		{
			AssertEquals(PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
		}

		public void TestCustomsAuthorisationNumber()
		{
			CombineAssertions(() =>
			{
				var custodian = Factory.New<OrgHeader>();
				TempStorageLine.StorageHeader.SJH_PresentationDate = ZDate.Today;
				TempStorageLine.TSL_OA_Custodian = custodian.MainAddress.PK;
				AssertEquals("Has no AuthorisationNumber", PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
				custodian.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "AUT123456");
				AssertEquals("Has AuthorisationNumber", "AUT123456", TempStorageLineWrapped.CustomsAuthorisationNumber);
				TempStorageLine.StorageHeader.SJH_PresentationDate = ZDate.Today.AddDays(-2);
				AssertEquals("Has AuthorisationNumber but PresentationDate out of range.", PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
			});
		}

		protected override CusTempStorageLine GetTempStorageLineToTest()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageHeader.PRLCONCusTempStorageDecs.AddNew();
			return storageDec.CusTempStorageLines.AddNew();
		}

		protected override PRLCONConsolidatedCusTempStorageLineProvider GetTempStorageLineWrapped() => new PRLCONConsolidatedCusTempStorageLineProvider(TempStorageLine);

		protected new IPRLCONConsolidatedTempStorageLine TempStorageLineWrapped => (IPRLCONConsolidatedTempStorageLine)base.TempStorageLineWrapped;
	}
}
