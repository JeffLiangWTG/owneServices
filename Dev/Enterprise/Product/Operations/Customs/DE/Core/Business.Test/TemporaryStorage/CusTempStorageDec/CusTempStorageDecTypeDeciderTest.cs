using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CusTempStorageDecTypeDeciderTest : TestCaseWithFactory
	{
		public void TestDefaultTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = "BLANKS";
			AssertEquals(typeof(EU.Business.CusTempStorage.CusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
			AssertEquals("The CusTempStorageDecTypeDecider for DE could not load the object as the STH_DeclarationType 'BLANKS' is unknown. A base EU.CusTempStorageDec was returned instead.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestCUSPRLTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger;
			AssertEquals(typeof(CUSPRLCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		public void TestCHGTSTTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeCustodyInformation;
			AssertEquals(typeof(CHGTSTCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		public void TestCUSPCSTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;
			AssertEquals(typeof(CUSPCSCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		public void TestREXDISTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ReExportDispatch;
			AssertEquals(typeof(REXDISCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		public void TestCHGSPOTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeOfSpecificOrderTerm;
			AssertEquals(typeof(CHGSPOCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		public void TestCHGOFFTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.ChangeDisposalEntitledTrader;
			AssertEquals(typeof(CHGOFFCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		public void TestPRLCONTypeDecider()
		{
			var storageDec = Factory.New<EU.Business.CusTempStorage.CusTempStorageDec>();
			storageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.PresentationLedgerConsolidation;
			AssertEquals(typeof(PRLCONCusTempStorageDec), decider.GetTypeForLoad(((INeedRow)storageDec).Row, Factory));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			decider = new CusTempStorageDecTypeDecider();
		}
		CusTempStorageDecTypeDecider decider;

		#endregion
	}
}
