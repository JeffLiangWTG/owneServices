using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BillImportAction))]
	class BillImportActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetBill()
		{
			var testBill = header.Bills.AddNew();
			var testImportAction = new BillImportAction(testBill);
			AssertEquals(testBill, testImportAction.Bill);
			AssertType(typeof(JPAFRBills), testImportAction.Bill);
		}

		public void TestGetBillNumber()
		{
			var testBill = header.Bills.AddNew();
			testBill.JPB_BillNumber = "testBill1";
			var testImportAction = new BillImportAction(testBill);
			AssertEquals("testBill1", testImportAction.BillNumber);
			testBill.JPB_BillNumber = "testBill2";
			AssertEquals("testBill2", testImportAction.BillNumber);
		}

		public void TestIsSelected()
		{
			var testBill = header.Bills.AddNew();
			var testImportAction = new BillImportAction(testBill);
			Assert("CASE: Normal - should tick", testImportAction.IsSelected);

			testBill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingMasterBillDelete;
			testImportAction = new BillImportAction(testBill);
			Assert("CASE: awaiting response - should not tick", !testImportAction.IsSelected);

			testBill.JPB_MessageStatus = ZString.Empty;
			testBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			testImportAction = new BillImportAction(testBill);
			Assert("CASE: already registered - should not tick", !testImportAction.IsSelected);

			testBill.JPB_MessageStatus = ZString.Empty;
			testBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			testImportAction = new BillImportAction(testBill);
			Assert("CASE: HLD - should not tick", !testImportAction.IsSelected);

			testBill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingMasterBillDelete;
			testBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			testImportAction = new BillImportAction(testBill);
			Assert("CASE: already registered and awaiting - should not tick", !testImportAction.IsSelected);
		}

		public void TestIsSelected_ReadOnly()
		{
			var testBill = header.Bills.AddNew();
			var testImportAction = new BillImportAction(testBill);
			Assert("CASE: Normal - should not be readonly", !testImportAction.IsSelectedInfo.ReadOnly);

			testBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Assert("CASE: Registered - should be readonly", testImportAction.IsSelectedInfo.ReadOnly);

			testBill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			Assert("CASE: HLD - should be readonly", testImportAction.IsSelectedInfo.ReadOnly);

			testBill.JPB_ReleaseStatus = ZString.Empty;
			testBill.JPB_MessageStatus = MessageStatusList.Codes.AwaitingMasterBillDelete;
			Assert("CASE: Awaiting - should be readonly", testImportAction.IsSelectedInfo.ReadOnly);

			AssertEquals(true, testImportAction.IsSelected);
			testImportAction.IsSelected = false;
			AssertEquals(true, testImportAction.IsSelected);
		}

		public void TestActionCodeChangeWhenChangeSailing()
		{
			header.JPH_IsShippingLineEntry = true;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "AALSMEERGRACHT";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "123SD";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2006, 4, 10);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2006, 4, 20);

			voyage.GenerateSailings();
			var sailing = voyage.Sailings[0];

			var fCLSailingBill = Factory.New<BillOfLading>();
			fCLSailingBill.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			fCLSailingBill.JS_NKLoadPort = "AUSYD";
			fCLSailingBill.JS_JX = sailing.PK;
			fCLSailingBill.JS_HouseBill = "SCACAAA";

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_VoyageFlight = "123SD";
			voyage2.JV_RV_NKVessel = vessel.RV_FK;
			var origin2 = voyage2.Origins.AddNew();
			origin2.JA_RL_NKPortOfLoading = "AUSYD";
			origin2.JA_E_DEP = new ZDateTime(2006, 4, 10);
			var destination2 = voyage2.Destinations.AddNew();
			destination2.JB_RL_NKPortOfDischarge = "NZAKL";
			destination2.JB_E_ARV = new ZDateTime(2006, 4, 20);

			voyage2.GenerateSailings();
			var sailing2 = voyage2.Sailings[0];

			header.ChangeSailing(sailing.PK);
			AssertEquals(0, header.Bills.Count);
			header.ImportBillsOfLadingLinkedToTheSameSailing(new BillImportActionCollection(header.Bills));
			AssertEquals(1, header.Bills.Count);
			var testImportCollection = new BillImportActionCollection(header.Bills);
			AssertEquals(1, testImportCollection.Count);
			AssertEquals(ImportAction.Replace, testImportCollection[0].Action);
			header.ChangeSailing(sailing2.PK);
			testImportCollection = new BillImportActionCollection(header.Bills);
			AssertEquals(1, testImportCollection.Count);
			AssertEquals(ImportAction.Delete, testImportCollection[0].Action);
		}

		protected override BusinessObject GetNewBusinessObject() => new BillImportAction(header.Bills.AddNew());

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<JPAFRHeader>();
		}
		JPAFRHeader header;
	}
}
