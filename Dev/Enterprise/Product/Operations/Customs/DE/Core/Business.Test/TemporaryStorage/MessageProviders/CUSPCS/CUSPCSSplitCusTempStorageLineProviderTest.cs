using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSSplitCusTempStorageLineProvider))]
	class CUSPCSSplitCusTempStorageLineProviderTest : CusTempStorageLineProviderAbstractTest<CUSPCSSplitCusTempStorageLineProvider>
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
				AssertEquals("Has AuthorisationNumber but PresentationDate out of range", PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
			});
		}

		protected override CusTempStorageLine GetTempStorageLineToTest()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageHeader.CUSPCSCusTempStorageDecs.AddNew();
			var consolidatedLine = storageDec.CusTempStorageLines.AddNew();
			return consolidatedLine.CusTempStorageLinesTo.AddNew();
		}

		protected override CUSPCSSplitCusTempStorageLineProvider GetTempStorageLineWrapped() => new CUSPCSSplitCusTempStorageLineProvider(TempStorageLine);

		protected new ICUSPCSSplitTempStorageLine TempStorageLineWrapped => (ICUSPCSSplitTempStorageLine)base.TempStorageLineWrapped;
	}
}
