using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	sealed class JPAFRBillsCollectionSynchroniserTest : AFRSynchroniserTestCase
	{
		public void TestSynchronisation_DuplicatedHouseBill()
		{
			AssertEquals(0, header.Bills.Count);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "OTT1ABCD1234";

			AssertEquals(1, header.Bills.Count);
			var bill1 = header.Bills[0];
			AssertBill(bill1, "OTT1ABCD1234");

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "OTT1ABCD1234";

			AssertEquals(2, header.Bills.Count);
			var bill2 = header.Bills[1];
			if (bill2 == bill1)
			{
				bill2 = header.Bills[0];
			}
			else
			{
				AssertEquals(bill1, header.Bills[0]);
			}
			AssertBill(bill1, "OTT1ABCD1234");
			AssertBill(bill2, "OTT1ABCD1234");

			shipment2.JS_HouseBill = "OTT1ABCD5678";
			AssertEquals(2, header.Bills.Count);
			AssertBill(bill1, "OTT1ABCD1234");
			AssertBill(bill2, "OTT1ABCD5678");

			shipment2.JS_HouseBill = "OTT1ABCD1234";
			AssertEquals(2, header.Bills.Count);
			AssertBill(bill1, "OTT1ABCD1234");
			AssertBill(bill2, "OTT1ABCD1234");

			synchroniser.SetEnabled(false, false);
			shipment1.Delete();

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_HouseBill = "OTT1ABCD1234";

			synchroniser.SetEnabled(true, false);
			AssertEquals(2, header.Bills.Count);
			AssertBill(bill1, "OTT1ABCD1234");
			AssertBill(bill2, "OTT1ABCD1234");

			synchroniser.Synchronise(true);
			AssertEquals("Is deleted as shipment1 is deleted", true, bill1.IsDeleted);
			AssertEquals(2, header.Bills.Count);
			var bill3 = header.Bills[1];
			if (bill3 == bill2)
			{
				bill3 = header.Bills[0];
			}
			else
			{
				AssertEquals(bill2, header.Bills[0]);
			}
			AssertBill(bill2, "OTT1ABCD1234");
			AssertBill(bill3, "OTT1ABCD1234");
		}

		public void TestSynchronisationViaDataRefreshBus()
		{
#pragma warning disable
			((IBusinessObjectState)consol).UpdatedByDataRefreshIncludingChildren += new EventHandler(JPAFRBillsCollectionSynchroniserTest_UpdatedByDataRefreshIncludingChildren);
#pragma warning restore
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "ABCDHB1";
			synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			var bill = header.Bills[0];
			AssertEquals("ABCDHB1", bill.JPB_BillNumber);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var shipmentInOtherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			AssertEquals("ABCDHB1", shipmentInOtherFactory.JS_HouseBill);
			shipmentInOtherFactory.JS_HouseBill = "EFGHHB2";
			newFactory.Save();
			AssertEquals("EFGHHB2", shipment.JS_HouseBill);
			AssertEquals(1, header.Bills.Count);
			AssertEquals(false, bill.IsDeleted);
			AssertEquals(bill, header.Bills["EFGHHB2"]);
		}

		public void TestSynchronisationWithNVOCCRegistry()
		{
			JPAFRRegistry.Instance.AFRNVOCCIDtoAddtoBillDuringSync.SetValue(header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "J07J");
			AssertEquals(0, header.Bills.Count);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "ABCD1234";

			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "J07JABCD1234");

			synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "J07JABCD1234");

			synchroniser = new JPAFRBillsCollectionSynchroniser(header);
			header.Bills[0].JPB_MessageStatus = "AHR";
			synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "J07JABCD1234");
			AssertEquals("AHR", header.Bills[0].JPB_MessageStatus);

			synchroniser = new JPAFRBillsCollectionSynchroniser(header);
			header.Bills[0].JPB_ReleaseStatus = "REG";
			synchroniser.Synchronise(true);
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "J07JABCD1234");
			AssertEquals("AHR", header.Bills[0].JPB_MessageStatus);
			AssertEquals("REG", header.Bills[0].JPB_ReleaseStatus);
		}

		public void TestBillsAlreadyRegisteredOrMessageInProcessShouldNotBeDeletedBySynchronisation()
		{
			AssertEquals(0, header.Bills.Count);
			AssertEquals("OTT1", orgProxyCarrierCode.OK_CustomsRegNo);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "ABCD1234";

			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], "ABCD1234");

			synchroniser.SetEnabled(false, false);
			var bill1 = header.Bills[0];
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "OTT1MB2";
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			synchroniser.Synchronise(true);
			AssertEquals("Should not be deleted as it's on Customs File", false, bill2.IsDeleted);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
			AssertBill(header.Bills[1], bill2.PK, "OTT1MB2");

			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.NotRegistered;
			synchroniser.Synchronise(true);
			AssertEquals("Should be deleted as it's not on Customs File", true, bill2.IsDeleted);
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "ABCD1234");

			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "OTT1MB3";
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillRegistration;

			synchroniser.Synchronise(true);
			AssertEquals("Should not be deleted as it's awaiting response", false, bill3.IsDeleted);
			AssertEquals(2, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
			AssertBill(header.Bills[1], bill3.PK, "OTT1MB3");

			bill3.JPB_MessageStatus = MessageStatusList.Codes.ClearMasterBillDelete;
			synchroniser.Synchronise(true);
			AssertEquals("Should be deleted as it's not awaiting response", true, bill3.IsDeleted);
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
		}

		public void TestSynchronisation_CoLoadMaster()
		{
			AssertEquals(0, header.Bills.Count);

			using (JPAFRRegistry.Instance.IncludeBCNSubShipmentsInManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONT1";
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_HouseBill = "ABCD1234";
				var shipment1Packline = shipment1.OuterPackLines.AddNew();
				shipment1Packline.SetContainer(consol, container);
				shipment1Packline.JL_MarksAndNumbers = "MARKS1";
				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_HouseBill = "ABCD5678";
				var shipment2Packline = shipment2.OuterPackLines.AddNew();
				shipment2Packline.SetContainer(consol, container);
				shipment2Packline.JL_MarksAndNumbers = "MARKS2";

				shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
				AssertEquals(1, header.Bills.Count);
				var bill1 = header.Bills[0];
				AssertBill(bill1, "ABCD1234");

				shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
				AssertEquals(2, header.Bills.Count);
				AssertBill(bill1, "ABCD1234");

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_HouseBill = "ABCD9012";
				var shipment3Packline = shipment3.OuterPackLines.AddNew();
				shipment3Packline.SetContainer(consol, container);
				shipment3Packline.JL_MarksAndNumbers = "MARKS3";
				AssertEquals(3, header.Bills.Count);
				AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
				var bill3 = header.Bills[2];
				AssertBill(bill3, "ABCD9012");

				shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				AssertEquals(3, header.Bills.Count);
				AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
				AssertEquals(false, bill3.IsDeleted);

				var org1 = Factory.New<OrgHeader>();
				org1.OH_IsForwarder = true;
				org1.MiscServ.OM_FWDirectAMSReporter = true;

				var org2 = Factory.New<OrgHeader>();
				org2.OH_IsForwarder = true;
				org2.MiscServ.OM_FWDirectAMSReporter = false;

				shipment3.ConsignorPK = org1.PK;
				shipment2.JS_JS_ColoadMasterShipment = shipment3.PK;
				AssertEquals(2, header.Bills.Count);
				AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
				AssertEquals(false, bill1.IsDeleted);

				shipment3.ConsignorPK = org2.PK;
				AssertEquals(2, header.Bills.Count);
				AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
				var bill2 = header.Bills[1];
				AssertBill(bill2, "ABCD9012");

				// AssemblyMaster should be like coload master
				shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				AssertEquals(2, header.Bills.Count);
				AssertBill(header.Bills[0], bill1.PK, "ABCD1234");
				AssertBill(header.Bills[1], bill2.PK, "ABCD9012");
			}
		}

		public void TestSynchronisation_AssemblyMaster()
		{
			using (JPAFRRegistry.Instance.IncludeBCNSubShipmentsInManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(0, header.Bills.Count);

				var container = consol.Containers.AddNew();
				container.JC_ContainerNum = "CONT1";
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_HouseBill = "ABCD1234";
				var shipment1Packline = shipment1.OuterPackLines.AddNew();
				shipment1Packline.SetContainer(consol, container);
				shipment1Packline.JL_MarksAndNumbers = "MARKS1";
				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_HouseBill = "ABCD5678";
				var shipment2Packline = shipment2.OuterPackLines.AddNew();
				shipment2Packline.SetContainer(consol, container);
				shipment2Packline.JL_MarksAndNumbers = "MARKS2";

				shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
				shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
				AssertEquals(1, header.Bills.Count);
				var bill1 = header.Bills[0];
				AssertBill(bill1, "ABCD1234");
			}
		}

		public void TestSynchronisation_SubShipment()
		{
			AssertEquals(0, header.Bills.Count);
			AssertEquals("Default value of IncludeAsmCldClbSubShipmentsInManifest", false, JPAFRRegistry.Instance.IncludeAsmCldClbSubShipmentsInManifest.Value);
			AssertEquals("Default value of IncludeBCNSubShipmentsInManifest", true, JPAFRRegistry.Instance.IncludeBCNSubShipmentsInManifest.Value);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_IsForwarder = true;
			org1.MiscServ.OM_FWDirectAMSReporter = true;

			var asmShipment = consol.Shipments.AddNew();
			asmShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			asmShipment.JS_HouseBill = "ABCD1231";
			asmShipment.ConsignorPK = org1.PK;

			var cldShipment = consol.Shipments.AddNew();
			cldShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			cldShipment.JS_HouseBill = "ABCD1232";
			cldShipment.ConsignorPK = org1.PK;

			var clbShipment = consol.Shipments.AddNew();
			clbShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BlindCoLoadMaster;
			clbShipment.JS_HouseBill = "ABCD1233";
			clbShipment.ConsignorPK = org1.PK;

			var asmSubShipment = consol.Shipments.AddNew();
			asmSubShipment.JS_JS_ColoadMasterShipment = asmShipment.PK;
			asmSubShipment.JS_HouseBill = "ABCD1234";
			asmSubShipment.ConsignorPK = org1.PK;

			var cldSubShipment = consol.Shipments.AddNew();
			cldSubShipment.JS_JS_ColoadMasterShipment = cldShipment.PK;
			cldSubShipment.JS_HouseBill = "ABCD1235";
			cldSubShipment.ConsignorPK = org1.PK;

			var clbSubShipment = consol.Shipments.AddNew();
			clbSubShipment.JS_JS_ColoadMasterShipment = clbShipment.PK;
			clbSubShipment.JS_HouseBill = "ABCD1236";
			clbSubShipment.ConsignorPK = org1.PK;

			var bcnShipment = consol.Shipments.AddNew();
			bcnShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			bcnShipment.JS_HouseBill = "ABCD1237";
			bcnShipment.ConsignorPK = org1.PK;

			var bcnSubShipment = consol.Shipments.AddNew();
			bcnSubShipment.JS_JS_ColoadMasterShipment = bcnShipment.PK;
			bcnSubShipment.JS_HouseBill = "ABCD1238";
			bcnSubShipment.ConsignorPK = org1.PK;

			AssertNull(asmShipment.CoLoadMasterShipment);
			AssertNull(cldShipment.CoLoadMasterShipment);
			AssertNull(clbShipment.CoLoadMasterShipment);
			AssertNull(bcnShipment.CoLoadMasterShipment);

			AssertEquals(asmShipment, asmSubShipment.CoLoadMasterShipment);
			AssertEquals(cldShipment, cldSubShipment.CoLoadMasterShipment);
			AssertEquals(clbShipment, clbSubShipment.CoLoadMasterShipment);
			AssertEquals(bcnShipment, bcnSubShipment.CoLoadMasterShipment);

			synchroniser.Synchronise(true);

			var expectedBillNumbers = new[]
			{
				"ABCD1231",
				"ABCD1232",
				"ABCD1233",
				"ABCD1237",
				"ABCD1238"
			};

			var actualBillNumbers = header.Bills.Select(c => c.JPB_BillNumber);
			AssertContainsExactElementsInAnyOrder(expectedBillNumbers, actualBillNumbers);

			using (JPAFRRegistry.Instance.IncludeAsmCldClbSubShipmentsInManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (JPAFRRegistry.Instance.IncludeBCNSubShipmentsInManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				synchroniser.Synchronise(true);

				expectedBillNumbers = new[]
				{
					"ABCD1231",
					"ABCD1232",
					"ABCD1233",
					"ABCD1237",
					"ABCD1234",
					"ABCD1235",
					"ABCD1236"
				};

				actualBillNumbers = header.Bills.Select(c => c.JPB_BillNumber);
				AssertContainsExactElementsInAnyOrder(expectedBillNumbers, actualBillNumbers);
			}
		}

		void JPAFRBillsCollectionSynchroniserTest_UpdatedByDataRefreshIncludingChildren(object sender, EventArgs e)
		{
			synchroniser.Synchronise();
		}

		JPAFRBillsCollectionSynchroniser synchroniser;

		protected override void SetUp()
		{
			base.SetUp();
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			synchroniser = new JPAFRBillsCollectionSynchroniser(header);
		}
	}
}
