using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONCusTempStorageDecProvider))]
	class PRLCONCusTempStorageDecProviderTest : CusTempStorageDecProviderAbstractTest<PRLCONCusTempStorageDecProvider>
	{
		public void TestConsolidatedTempStorageLineDetails()
		{
			var consolidateLine = ((PRLCONCusTempStorageDec)TempStorageDec).ConsolidatedLine;
			consolidateLine.TSL_PackageQty = 98;
			var consolidatedTempStorageLineDetail = TempStorageDecWrapped.ConsolidatedTempStorageLineDetails;
			AssertEquals("Package Quantity", 98, consolidatedTempStorageLineDetail.PackageQuantity);
		}

		protected override CusTempStorageDec GetTempStorageDecToTest() => Factory.New<PRLCONCusTempStorageDec>();

		protected override ITempStorageDec GetTempStorageDecWrapped() => new PRLCONCusTempStorageDecProvider(TempStorageDec);

		protected new IPRLCONTempStorageDec TempStorageDecWrapped => (IPRLCONTempStorageDec)base.TempStorageDecWrapped;
	}
}
