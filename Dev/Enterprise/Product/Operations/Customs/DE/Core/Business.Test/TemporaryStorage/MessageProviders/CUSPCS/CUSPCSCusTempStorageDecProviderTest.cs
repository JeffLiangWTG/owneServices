using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSCusTempStorageDecProvider))]
	class CUSPCSCusTempStorageDecProviderTest : CusTempStorageDecProviderAbstractTest<CUSPCSCusTempStorageDecProvider>
	{
		public void TestFirstStorageLineType()
		{
			TempStorageDec.CusTempStorageLines.AddNew();
			AssertType<CUSPCSConsolidatedCusTempStorageLineProvider>(TempStorageDecWrapped.FirstTempStorageLineDetails);
		}

		protected override CusTempStorageDec GetTempStorageDecToTest() => Factory.New<CUSPCSCusTempStorageDec>();

		protected override ITempStorageDec GetTempStorageDecWrapped() => new CUSPCSCusTempStorageDecProvider(TempStorageDec);
	}
}
