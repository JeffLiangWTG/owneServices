using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobConShipLinkDeleteCheckerTest : SeaCargoTestCase// TestCaseWithFactory
	{
		public void TestJobConShipLinkDeleteCheckAndCannotDeleteExceptionNotCaughtDuringDataRefresh()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();

			var master = Factory.New<CusSCAOceanBill>();
			master.CB_ParentId = consol.PK;
			master.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			var houseBill = master.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;
			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var houseBillLoaded = factory2.Load<CusSCAHouse>(houseBill.PK);
			houseBillLoaded.CA_MessageStatus = CMRBaseStatuses.Codes.OriginalRejected;

			var consolLoaded = factory2.Load<ForwardingConsol>(consol.PK);
			var shipmentLoaded = consolLoaded.Shipments[0];
			consolLoaded.Shipments.Remove(shipmentLoaded);
			AssertNoExceptionThrown(@"during data refresh, CannotDeleteException is not caught.
1. Open a shipment form with sea cargo house bill with non-deletable status. Modify the sea cargo house bill and save. Have the shipment form open
2. Open a consol form. Withdraw the sea cargo house bill.
3. When a response comes back, try to detach the shipment. And Save. DataRefresh Bus is trying to update details in form opened in 1. The status will remain non-deletable status.
", delegate
			{ factory2.Save(); });
		}

		const string TestLloydsNumber = "1234567";
		const string TestVesselName = "SCOTTSFLOATINGBROTHEL";
		const string TestOceanBill = "OCEANTEST123";
		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "AUSYD";
		const string TestPrincipalID = "C067764014";
		const string TestHouseBillNumber = "TESTHOUSE123";
		CommonShipment shipment;
		ForwardingConsol consol;
		CusSCAOceanBill oceanBill;

		public void TestDeleteDetailsWithoutSeaCargoEntry()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;

			var testDeleteChecker = new JobConShipLinkDeleteChecker();
			var detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("The Result should be empty", detail.CanDelete);
		}

		public void TestDeleteDetailsWithSeaCargoEntryThatCanBeDetached()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;
			CreateSeaCargoJob();

			var testDeleteChecker = new JobConShipLinkDeleteChecker();
			var detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("The Result should be empty", detail.CanDelete);
		}

		public void TestDeleteDetailsWithVariousStatusCodes()
		{
			var pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;
			CreateSeaCargoJob();

			AssertErrorOnDetach(CMRBaseStatuses.Codes.AwaitingResponseToOriginal, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			AssertErrorOnDetach(CMRBaseStatuses.Codes.AwaitingResponseToAmendment, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			AssertErrorOnDetach(CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			AssertErrorOnDetach(CMRBaseStatuses.Codes.OriginalRejected, ZString.Empty, pivotRecord, "");
			AssertErrorOnDetach(CMRBaseStatuses.Codes.WithdrawalAccepted, CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn, pivotRecord, "");
			AssertErrorOnDetach(CMRBaseStatuses.Codes.OriginalRejected, "NOT", pivotRecord, "");
			AssertErrorOnDetach(CMRBaseStatuses.Codes.OriginalAccepted, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			AssertErrorOnDetach(CMRBaseStatuses.Codes.AmendmentAccepted, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			AssertErrorOnDetach(CMRBaseStatuses.Codes.AmendmentRejected, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			AssertErrorOnDetach(CMRBaseStatuses.Codes.WithdrawalRejected, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			AssertErrorOnDetach(CMRBaseStatuses.Codes.WithdrawalAccepted, ZString.Empty, pivotRecord, "");
			AssertErrorOnDetach(ZString.Empty, ZString.Empty, pivotRecord, "");

			var pivot = houseBill.Pivot[0];
			var underbond = (Customs.Business.CusUnderbond)pivot.Underbonds.AddNew();
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			AssertErrorOnDetach(ZString.Empty, ZString.Empty, pivotRecord, JobConShipLinkDeleteChecker.CannotBeDetached);
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;
			AssertErrorOnDetach(ZString.Empty, ZString.Empty, pivotRecord, "");

			var testDeleteChecker = new JobConShipLinkDeleteChecker();
			testDeleteChecker.BeforeSuccessfulDelete(pivotRecord);
		}

		void AssertErrorOnDetach(string messageStatusToSet, string shipmentStatusToSet, JobConShipLink pivotRecord, string errorMessageToCheck)
		{
			houseBill.CA_MessageStatus = messageStatusToSet;
			houseBill.CA_ShipmentStatus = shipmentStatusToSet;

			var testDeleteChecker = new JobConShipLinkDeleteChecker();
			var detail = testDeleteChecker.DeleteDetails(pivotRecord);
			if (string.IsNullOrEmpty(errorMessageToCheck))
			{
				Assert("Can Delete", detail.CanDelete);
			}
			else
			{
				Assert("Can not Delete", !detail.CanDelete);
				AssertEquals("The Result should not be empty", errorMessageToCheck, detail.Reason);
			}
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			CreateTestVessel(TestVesselName);
			consol = Factory.New<ForwardingConsol>();

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = TestPortOfLoading;
			transport.JW_RL_NKDiscPort = TestPortOfDischarge;
			consol.JK_MasterBillNum = TestOceanBill;
			transport.JW_Vessel = TestVesselName;
			transport.JW_VoyageFlight = "23";

			shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = TestHouseBillNumber;
			shipment.JS_RL_NKOrigin = TestPortOfLoading;
			shipment.JS_RL_NKDestination = TestPortOfDischarge;
			shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

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

		void CreateTestVessel(string vesselName)
		{
			var vessel = RefVessel.New(Factory);
			vessel.RV_Code = vesselName;
			vessel.RV_LloydsNumber = TestLloydsNumber;
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "TESTSHIP";
			shippingLine.MainAddress.OA_Address1 = "TEST SHIP ADDRESS";
			shippingLine.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, TestPrincipalID);
			vessel.RV_OH = shippingLine.PK;
		}

		void CreateSeaCargoJob()
		{
			oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "SUDU400014772110";
			oceanBill.CB_PrincipalID = "C065301902";
			oceanBill.CB_VesselName = CusSCAOceanBillTest.TestVessel;
			oceanBill.CB_Voyage = "442";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;

			houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "49104160312018";
			houseBill.CA_RN_NKGoodsOrigin = "NZ";
			houseBill.CA_RL_NK_PortOfDestination = "AUBNE";
			houseBill.CA_RL_NK_PortOfOrigin = "NZAKL";
			houseBill.CA_PrepaidCollectOther = CMRMethodsOfPayment.Codes.PrepaidOnly;
			houseBill.CA_ConsigneeName = "TRITEC PTY LTD";
			houseBill.CA_ConsigneeAddress1 = "C O NEW CENTURY PACKING";
			houseBill.CA_ConsigneeAddress2 = "UNIT 5 370 NUDGEE ROAD";
			houseBill.CA_ConsigneeSuburb = "HENDRA QLD AUSTRALIA";
			houseBill.CA_ConsigneePostcode = "4011";
			houseBill.CA_ConsignorName = "HUHTAMAKI VAN LEER  NZ  LTD";
			houseBill.CA_ConsignorAddress1 = "FLEXIBLE PACKAGING DIVISION";
			houseBill.CA_ConsignorAddress2 = "BAG 93 002";
			houseBill.CA_ConsignorSuburb = "NEW LYNN AUCKLAND";
			houseBill.CA_NotifyName = "TRITEC PTY LTD";
			houseBill.CA_NotifyAddress1 = "C O NEW CENTURY PACKING";
			houseBill.CA_NotifyAddress2 = "UNIT 5 370 NUDGEE ROAD";
			houseBill.CA_NotifySuburb = "HENDRA QLD AUSTRALIA";
			houseBill.CA_NotifyPostcode = "4011";
			houseBill.CA_JS = shipment.PK;

			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container.CN_RC_NKContainerType = "40GP";
			container.CN_ShipperOwnedContainer = false;
			container.CN_SealNumber = "89644";
			container.CN_ContainerNumber = "FSCU6400235";

			var houseContainerPivot = houseBill.Pivot.AddNew();
			houseContainerPivot.CV_CN = container.PK;
			houseContainerPivot.CV_PackageCount = 40;
			houseContainerPivot.CV_PackageType = "BX";
			houseContainerPivot.CV_MarksAndNumbers = "MARKS AND NUMBERS : THIS END UP";
			houseContainerPivot.CV_GoodsDescription = "STC 40 BAGS OF PLASTIC REGRIND";
			houseContainerPivot.CV_WeightUQ = Enterprise.Core.Constants.Weight.Kilograms;
			houseContainerPivot.CV_Weight = 23900.00m;
			houseContainerPivot.CV_Volume = 68.000m;
		}

		#endregion
	}
}
