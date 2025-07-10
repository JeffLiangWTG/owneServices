using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.AirCargo.Test
{
	sealed class JobConShipLinkDeleteCheckerTest : TestCaseWithFactory
	{
		public void TestDeleteDetailsWithNZAirCargoEntry()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;

			var nzMAWB = (Customs.Business.CusMAWB)Factory.New<Integration.Customs.NZ.ICusMAWB>();
			nzMAWB.CM_JK = consol.PK;
			var nzHAWB = nzMAWB.ChildBills.AddNew();
			nzHAWB.CS_JS = shipment.PK;

			AirCargoJobConShipLinkDeleteChecker deleteChecker = new AirCargoJobConShipLinkDeleteChecker();
			DeleteDetails detail = deleteChecker.DeleteDetails(pivotRecord);
			AssertEquals("Can Delete Because CusHAWB is NZ.", true, detail.CanDelete);

			CreateAirCargoJob();
			AssertEquals("Precondition: Master created points to the consol", consol, masterBill.Consol);

			houseBill.CS_MsgStatus = "ACO";
			deleteChecker = new AirCargoJobConShipLinkDeleteChecker();
			detail = deleteChecker.DeleteDetails(pivotRecord);
			AssertEquals("Cannot Delete the pivot between Shipment and Consol", false, detail.CanDelete);
			AssertEquals(AirCargoJobConShipLinkDeleteChecker.HAWBCannotBeDeleted, detail.Reason);
		}

		public void TestDeleteDetailsWithoutAirCargoEntry()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;

			AirCargoJobConShipLinkDeleteChecker testDeleteChecker = new AirCargoJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("The Result should be empty", detail.CanDelete);
		}

		public void TestDeleteDetailsWithAirCargoEntryThatCanBeDetached()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;
			CreateAirCargoJob();

			AirCargoJobConShipLinkDeleteChecker testDeleteChecker = new AirCargoJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("The Result should be empty", detail.CanDelete);
		}

		public void TestDeleteDetailsWithAirCargoEntryThatCannotBeDetached()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;
			CreateAirCargoJob();

			houseBill.CS_IsResponsePending = true;
			AirCargoJobConShipLinkDeleteChecker testDeleteChecker = new AirCargoJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("Can Delete Because this is a legacy flag", detail.CanDelete);
		}

		public void TestAllowDetachFromConsolIfMasterDoesNotPointToTheConsol()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;

			var consol2 = Factory.New<ForwardingConsol>();
			var pivotRecord2 = Factory.New<JobConShipLink>();
			pivotRecord2.JN_JS = shipment.PK;
			pivotRecord2.JN_JK = consol2.PK;

			CreateAirCargoJob();
			AssertEquals("Master created points to the consol", consol, masterBill.Consol);

			houseBill.CS_MsgStatus = "ACO";
			AirCargoJobConShipLinkDeleteChecker testDeleteChecker = new AirCargoJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
			AssertEquals("Cannot Delete the pivot between Shipment and Consol", false, detail.CanDelete);

			DeleteDetails detail2 = testDeleteChecker.DeleteDetails(pivotRecord2);
			AssertEquals("Can Delete the pivot between Shipment and Consol2", true, detail2.CanDelete);
		}

		public void TestDeleteDetailsWithZeroLandedAirCargoEntry()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;
			CreateAirCargoJob();

			houseBill.CS_CustomsStatus = AirCargoMessage.NewStatus.Zero_landed;
			AirCargoJobConShipLinkDeleteChecker testDeleteChecker = new AirCargoJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
			AssertEquals("Can Delete as a house bill is deleted", true, detail.CanDelete);
		}

		public void TestRemoveHAWBBeforeSuccessfulDelete()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_JS = shipment.PK;
			hawb.SynchroniseData();

			ZQuery query = new ZQuery(CusHAWBSchema.CS_JS, shipment.PK);
			query.AddToFilter(CusHAWBSchema.CS_CM, mawb.PK);
			var hawbQueried = Factory.LoadTop1<CusHAWB>(query);
			AssertEquals(hawb, hawbQueried);

			pivotRecord.Delete();

			query = new ZQuery(CusHAWBSchema.CS_JS, shipment.PK);
			query.AddToFilter(CusHAWBSchema.CS_CM, null);
			hawbQueried = Factory.LoadTop1<CusHAWB>(query);
			AssertNotNull("PreCondition", hawbQueried);
			AssertEquals(hawb, hawbQueried);

			AssertEquals(0, mawb.ChildBills.Count);
			AssertEquals(0, mawb.FilteredChildBills.Count);
		}

		ForwardingShipment shipment;
		ForwardingConsol consol;
		CusHAWB houseBill;
		CusMAWB masterBill;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = TestPortOfLoading;
			transport.JW_RL_NKDiscPort = TestPortOfDischarge;
			transport.JW_VoyageFlight = "QF23";
			consol.JK_MasterBillNum = "081-10010000";

			shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = TestHouseBillNumber;
			shipment.JS_RL_NKOrigin = TestPortOfLoading;
			shipment.JS_RL_NKDestination = TestPortOfDischarge;
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard; //Prepaid

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESTIGNEE";
			consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
			shipment.ConsigneePK = consignee.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();
			shipment.Consols.Load();
		}

		void CreateAirCargoJob()
		{
			masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_JS = shipment.PK;
			houseBill.SynchroniseData();
		}

		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "AUSYD";
		const string TestHouseBillNumber = "TESTHOUSE123";
	}
}
