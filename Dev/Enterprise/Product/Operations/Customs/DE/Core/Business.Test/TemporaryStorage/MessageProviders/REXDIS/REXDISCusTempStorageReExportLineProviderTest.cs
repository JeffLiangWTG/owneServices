using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(REXDISCusTempStorageReExportLineProvider))]
	class REXDISCusTempStorageReExportLineProviderTest : CusTempStorageLineProviderAbstractTest<REXDISCusTempStorageReExportLineProvider>
	{
		public void TestSumALine()
		{
			var sumALine = ((REXDISCusTempStorageReExportLine)TempStorageLine).SumALine;
			sumALine.TSL_PackageQty = 34;
			AssertEquals(34, ((IREXDISTempStorageLine)TempStorageLineWrapped).SumALine.PackageQuantity);
		}

		protected override CusTempStorageLine GetTempStorageLineToTest() => Factory.New<REXDISCusTempStorageDec>().CusTempStorageLines.AddNew();

		protected override REXDISCusTempStorageReExportLineProvider GetTempStorageLineWrapped() => new REXDISCusTempStorageReExportLineProvider(TempStorageLine);
	}
}
