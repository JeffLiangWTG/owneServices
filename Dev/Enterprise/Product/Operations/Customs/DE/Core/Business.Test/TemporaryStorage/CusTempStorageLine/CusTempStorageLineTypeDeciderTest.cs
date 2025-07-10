using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CusTempStorageLineTypeDeciderTest : TestCaseWithFactory
	{
		public void TestDefaultTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = "BLANKS";
			var storageLine = storageDec.CusTempStorageLines.AddNew();
			AssertEquals(typeof(EU.Business.CusTempStorage.CusTempStorageLine), decider.GetTypeForLoad(((INeedRow)storageLine).Row, Factory));
		}

		public void TestCUSPRLTypeDecider()
		{
			var storageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var storageDec = CUSPRLCusTempStorageDec.LoadOrCreate(storageHeader);
			var storageLine = storageDec.CusTempStorageLines.AddNew();

			AssertEquals(typeof(CUSPRLCusTempStorageLine), decider.GetTypeForLoad(((INeedRow)storageLine).Row, Factory));
		}

		public void TestCUSPCSTypeDecider()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageHeader.CUSPCSCusTempStorageDecs.AddNew();
			var splitedStorageLine = storageDec.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.AddNew();

			AssertEquals(typeof(CUSPCSConsolidatedCusTempStorageLine), decider.GetTypeForLoad(((INeedRow)storageDec.ConsolidatedCusTempStorageLine).Row, Factory));
			AssertEquals(typeof(CUSPCSSplitCusTempStorageLine), decider.GetTypeForLoad(((INeedRow)splitedStorageLine).Row, Factory));

			var lineWithoutDivot = Factory.New<EU.Business.CusTempStorage.CusTempStorageLine>();
			lineWithoutDivot.TSL_STH = storageDec.PK;
			AssertEquals(typeof(CUSPCSConsolidatedCusTempStorageLine), decider.GetTypeForLoad(((INeedRow)lineWithoutDivot).Row, Factory));
		}

		public void TestPRLCONTypeDecider()
		{
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			var storageDec = storageHeader.PRLCONCusTempStorageDecs.AddNew();
			var lineToConsolidate = storageDec.CusTempStorageLines.AddNew();
			var consolidatedLine = storageDec.ConsolidatedLine;

			AssertEquals(typeof(PRLCONCusTempStorageLineToConsolidate), decider.GetTypeForLoad(((INeedRow)lineToConsolidate).Row, Factory));
			AssertEquals(typeof(PRLCONConsolidatedCusTempStorageLine), decider.GetTypeForLoad(((INeedRow)consolidatedLine).Row, Factory));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			decider = new CusTempStorageLineTypeDecider();
		}
		CusTempStorageLineTypeDecider decider;

		#endregion
	}
}
