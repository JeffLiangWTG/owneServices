using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHJobConShipLinkDeleteCheckerTest : TestCaseWithFactory
	{
		CommonShipment shipment;
		ForwardingConsol consol;
		CusCAeMHMaster eMHMaster;
		CusCAeMHHouse eMHHouse;
		CusCAeMHJobConShipLinkDeleteChecker testDeleteChecker;

		public void TestDoNotDeleteBillsFromUndetechedeManifest()
		{
			var consol2 = Factory.New<ForwardingConsol>();
			consol2.Shipments.Add(shipment);

			var eMHMaster2 = Factory.New<CusCAeMHMaster>();
			eMHMaster2.BP_ParentID = consol2.PK;
			eMHMaster2.BP_ParentTableCode = consol2.TablePrefix;
			var eMHHouse2 = eMHMaster2.HouseBills.AddNew();
			eMHHouse2.BW_ParentID = shipment.PK;
			eMHHouse2.BW_MessageReference = "TEST";
			Factory.Save();

			var pivotRecord2 = Factory.LoadTop1<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JK, consol2.PK));
			testDeleteChecker.BeforeSuccessfulDelete(pivotRecord2);
			AssertEquals(true, eMHMaster.HouseBills.Contains(eMHHouse));
		}

		public void TestDetachConsolWithouteManifest()
		{
			var pivotRecord = Factory.LoadTop1<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JS, shipment.PK));
			consol.JK_RL_NKDischargePort = "CATOR";
			eMHHouse.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.Shipments.Add(shipment);
			Factory.Save();

			var pivotRecord2 = Factory.LoadTop1<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JK, consol2.PK));
			var detail1 = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("Cannot Delete the pivot if Consol has eManifest", !detail1.CanDelete);
			AssertEquals("The shipment has a Canadian House Bill reported and has not been canceled. Please send a withdrawal message and try again.", detail1.Reason);

			var detail2 = testDeleteChecker.DeleteDetails(pivotRecord2);
			Assert("Cannot Delete the pivot if Consol has eManifest", detail2.CanDelete);
		}

		public void TestDeleteDetails()
		{
			var pivotRecord = Factory.LoadTop1<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JS, shipment.PK));
			eMHHouse.BW_ParentID = ZGuid.Empty;
			var detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("Can Delete, the result should be empty.", detail.CanDelete);

			consol.JK_RL_NKDischargePort = "CATOR";
			eMHHouse.BW_ParentID = shipment.PK;
			eMHMaster.BP_ParentID = consol.PK;
			eMHHouse.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			Factory.Save();

			detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("Can not Delete, the result should be not empty.", !detail.CanDelete);
		}

		public void TestBeforeSuccessfulDelete()
		{
			Assert("True", eMHMaster.HouseBills.Contains(eMHHouse));
			var pivotRecord = Factory.LoadTop1<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JS, shipment.PK));
			testDeleteChecker.BeforeSuccessfulDelete(pivotRecord);
			Assert("True", !eMHMaster.HouseBills.Contains(eMHHouse));
		}

		protected override void SetUp()
		{
			base.SetUp();
			ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
			consol = Factory.New<ForwardingConsol>();
			shipment = consol.Shipments.AddNew();
			eMHMaster = Factory.New<CusCAeMHMaster>();
			eMHMaster.BP_ParentID = consol.PK;
			eMHMaster.BP_ParentTableCode = consol.TablePrefix;
			eMHHouse = eMHMaster.HouseBills.AddNew();
			eMHHouse.BW_ParentID = shipment.PK;
			testDeleteChecker = new CusCAeMHJobConShipLinkDeleteChecker();
			Factory.Save();
		}
	}
}
