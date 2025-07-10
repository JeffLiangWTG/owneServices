using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CHGTSTCusTempStorageLineProvider))]
	class CHGTSTCusTempStorageLineProviderTest : CusTempStorageLineProviderAbstractTest<CHGTSTCusTempStorageLineProvider>
	{
		public void TestCustomsAuthorisationNumber_BranchOrgProxy()
		{
			AssertEquals(PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
		}

		public void TestCustomsAuthorisationNumber()
		{
			CombineAssertions(() =>
			{
				var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
				Factory.Save();
				TempStorageLine.StorageHeader.SJH_PresentationDate = ZDate.Today;
				AssertEquals("Has no AuthorisationNumber", PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
				branchOrgProxy.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "AUT123456");
				AssertEquals("Has AuthorisationNumber", "AUT123456", TempStorageLineWrapped.CustomsAuthorisationNumber);
				TempStorageLine.StorageHeader.SJH_PresentationDate = ZDate.Today.AddDays(-2);
				AssertEquals("Has AuthorisationNumber but PresentationDate out of range.", PreviousReferenceType.Codes._OHNE, TempStorageLineWrapped.CustomsAuthorisationNumber);
			});
		}

		protected override CusTempStorageLine GetTempStorageLineToTest()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageHeader.CHGTSTCusTempStorageDecs.AddNew();
			return storageDec.CusTempStorageLines.AddNew();
		}

		protected override CHGTSTCusTempStorageLineProvider GetTempStorageLineWrapped() => new CHGTSTCusTempStorageLineProvider(TempStorageLine);

		protected new ICHGTSTTempStorageLine TempStorageLineWrapped => (ICHGTSTTempStorageLine)base.TempStorageLineWrapped;
	}
}
