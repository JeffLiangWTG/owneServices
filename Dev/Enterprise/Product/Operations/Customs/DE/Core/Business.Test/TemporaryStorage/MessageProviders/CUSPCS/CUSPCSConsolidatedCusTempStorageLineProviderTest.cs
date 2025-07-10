using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSConsolidatedCusTempStorageLineProvider))]
	class CUSPCSConsolidatedCusTempStorageLineProviderTest : CusTempStorageLineProviderAbstractTest<CUSPCSConsolidatedCusTempStorageLineProvider>
	{
		public void TestSplitLines()
		{
			TempStorageLine.CusTempStorageLinesTo.AddNew();
			TempStorageLine.CusTempStorageLinesTo.AddNew();
			AssertEquals("Split lines count", 2, ((ICUSPCSTempStorageLine)TempStorageLineWrapped).SplitLines.Count);
		}

		protected override CusTempStorageLine GetTempStorageLineToTest() => Factory.New<CUSPCSCusTempStorageDec>().ConsolidatedCusTempStorageLine;

		protected override CUSPCSConsolidatedCusTempStorageLineProvider GetTempStorageLineWrapped() => new CUSPCSConsolidatedCusTempStorageLineProvider(TempStorageLine);

		protected new CUSPCSConsolidatedCusTempStorageLine TempStorageLine => (CUSPCSConsolidatedCusTempStorageLine)base.TempStorageLine;
	}
}
