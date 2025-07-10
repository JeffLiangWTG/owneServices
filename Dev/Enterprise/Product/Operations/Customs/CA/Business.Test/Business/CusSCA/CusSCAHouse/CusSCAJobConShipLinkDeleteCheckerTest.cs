using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAJobConShipLinkDeleteCheckerTest : TestCaseWithFactory
	{
		const string TestLloydsNumber = "1234567";
		const string TestVesselName = "SCOTTSFLOATINGBROTHEL";
		const string TestOceanBill = "OCEANTEST123";
		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "CATOR";
		const string TestPrincipalID = "C067764014";
		const string TestHouseBillNumber = "TESTHOUSE123";
		CommonShipment shipment;
		ForwardingConsol consol;
		CusSCAOceanBill oceanBill;
		CusSCAHouse houseBill;

		public void TestDeleteDetailsWithoutSeaCargoEntry()
		{
			JobConShipLink pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;

			CusSCAJobConShipLinkDeleteChecker testDeleteChecker = new CusSCAJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("The Result should be empty", detail.CanDelete);
		}

		public void TestDeleteDetailsACIEntryThatCanBeDetached()
		{
			JobConShipLink pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;
			CreateSeaCargoJob();

			CusSCAJobConShipLinkDeleteChecker testDeleteChecker = new CusSCAJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
			Assert("The Result should be empty", detail.CanDelete);

			Assert("pre-condition", !houseBill.IsDeleted);
			Assert("pre-condition", houseBill.PackLines.Count > 0);
			CusSCAPivot pivot1 = houseBill.PackLines[0];
			testDeleteChecker.BeforeSuccessfulDelete(pivotRecord);
			Assert("House deleted", houseBill.IsDeleted);
			Assert("Pack Line deleted", pivot1.IsDeleted);
		}

		public void TestDeleteDetailsWithVariousStatusCodes()
		{
			JobConShipLink pivotRecord = Factory.New<JobConShipLink>();
			pivotRecord.JN_JS = shipment.PK;
			pivotRecord.JN_JK = consol.PK;
			CreateSeaCargoJob();

			AssertErrorOnDetach(MessageStatusList.Codes.AwaitingOriginal, ZString.Empty, pivotRecord, CusSCAJobConShipLinkDeleteChecker.HouseBillInProgress);
			AssertErrorOnDetach(MessageStatusList.Codes.ErrorOriginal, ZString.Empty, pivotRecord, "");
			AssertErrorOnDetach(ZString.Empty, SupplementaryCargoReportJobStatusList.Codes.Cancelled, pivotRecord, "");
			AssertErrorOnDetach(ZString.Empty, ZString.Empty, pivotRecord, "");
			AssertErrorOnDetach(ZString.Empty, SupplementaryCargoReportJobStatusList.Codes.Error, pivotRecord, CusSCAJobConShipLinkDeleteChecker.HouseBillAcknowledged);
		}

		void AssertErrorOnDetach(string messageStatusToSet, string shipmentStatusToSet, JobConShipLink pivotRecord, string errorMessageToCheck)
		{
			houseBill.CA_MessageStatus = messageStatusToSet;
			houseBill.CA_ShipmentStatus = shipmentStatusToSet;

			CusSCAJobConShipLinkDeleteChecker testDeleteChecker = new CusSCAJobConShipLinkDeleteChecker();
			DeleteDetails detail = testDeleteChecker.DeleteDetails(pivotRecord);
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

			Transport transport = consol.Transports[0];
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

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESTIGNEE";
			consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
			shipment.ConsigneePK = consignee.PK;

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();
			shipment.Consols.Load();
		}

		void CreateTestVessel(string vesselName)
		{
			RefVessel vessel = RefVessel.New(Factory);
			vessel.RV_Code = vesselName;
			vessel.RV_LloydsNumber = TestLloydsNumber;
			OrgHeader shippingLine = Factory.New<OrgHeader>();
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
			oceanBill.CB_VesselName = TestLloydsNumber;
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

			CusSCAContainer container = oceanBill.Containers.AddNew();
			container.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			container.CN_RC_NKContainerType = "40GP";
			container.CN_ShipperOwnedContainer = false;
			container.CN_SealNumber = "89644";
			container.CN_ContainerNumber = "FSCU6400235";

			CusSCAPivot houseContainerPivot = houseBill.PackLines.AddNew();
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
