using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRHeaderConsolSynchroniserTest : AFRSynchroniserTestCase
	{
		public void TestSynchroniseCarrier()
		{
			header.Carrier.E2_AddressOverride = ZBool.True;
			header.Synchroniser.Synchronise(true);
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals(ZGuid.Empty, header.Carrier.E2_OA_Address);

			AssertEquals(1, consol.Transports.Count);
			consol.JK_OA_ShippingLineAddress = TestShippingLine.MainAddress.PK;
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals(TestShippingLine.MainAddress.PK, header.Carrier.E2_OA_Address);

			var t1 = consol.Transports[0];
			t1.JW_OA_CarrierAddress = TestShippingLine2.MainAddress.PK;
			t1.JW_RL_NKLoadPort = AUMEL.Code;
			t1.JW_RL_NKDiscPort = AUSYD.Code;
			AssertEquals(TestShippingLine.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals(TestShippingLine.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals(TestShippingLine.MainAddress.PK, header.Carrier.E2_OA_Address);

			var t2 = consol.Transports.AddNew();
			t2.JW_OA_CarrierAddress = TestShippingLine3.MainAddress.PK;
			t2.JW_RL_NKLoadPort = AUSYD.Code;
			t2.JW_RL_NKDiscPort = JPTKY.Code;
			AssertEquals(TestShippingLine.MainAddress.PK, consol.JK_OA_ShippingLineAddress);
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals("Carrier must remain synchronised to main consol carrier.", consol.JK_OA_ShippingLineAddress, header.Carrier.E2_OA_Address);

			t1.JW_RL_NKDiscPort = JPHAO.Code;
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals("Carrier must remain synchronised to main consol carrier.", consol.JK_OA_ShippingLineAddress, header.Carrier.E2_OA_Address);

			synchroniser.SetEnabled(false, false);
			AssertEquals(false, header.Carrier.ReadOnly);
		}

		public void TestCarrierSynchronisesFromConsolScreen()
		{
			AssertEquals("Pre-condition: ensure 2 different carrier values", false, TestShippingLine.MainAddress.PK == TestShippingLine2.MainAddress.PK);
			consol.JK_OA_ShippingLineAddress = TestShippingLine.MainAddress.PK;
			var transportLeg = consol.Transports[0];
			transportLeg.JW_OA_CarrierAddress = TestShippingLine2.MainAddress.PK;
			transportLeg.JW_RL_NKLoadPort = AUMEL.Code;
			transportLeg.JW_RL_NKDiscPort = AUSYD.Code;

			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals("Carrier must synchronise to main consol carrier.", consol.JK_OA_ShippingLineAddress, header.Carrier.E2_OA_Address);
		}

		public void TestSynchroniseJPH_CarrierCode()
		{
			AssertEquals(string.Empty, header.JPH_CarrierCode);
			TestShippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			AssertEquals(string.Empty, header.JPH_CarrierCode);
			consol.JK_OA_ShippingLineAddress = TestShippingLine.MainAddress.PK;
			TestShippingLine2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "KYRA", Core.Constants.CountryCodes.Japan);
			var transportLeg = consol.Transports[0];
			transportLeg.JW_OA_CarrierAddress = TestShippingLine2.MainAddress.PK;
			transportLeg.JW_RL_NKLoadPort = AUMEL.Code;
			transportLeg.JW_RL_NKDiscPort = AUSYD.Code;

			AssertEquals("Carrier must synchronise to main consol carrier - code should therefore be from consol carrier.", "SPQA", header.JPH_CarrierCode);
		}

		public void TestSynchroniseJPH_CarrierCode_WhenChangingFromOverridenToDefault()
		{
			TestShippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			AssertEquals(string.Empty, header.JPH_CarrierCode);

			consol.JK_OA_ShippingLineAddress = TestShippingLine.MainAddress.PK;

			synchroniser.SetEnabled(false, false);
			AssertEquals("SPQA", header.JPH_CarrierCode);
			header.JPH_CarrierCode = "SPQB";
			AssertEquals("SPQB", header.JPH_CarrierCode);

			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise(true);
			AssertEquals("SPQA", header.JPH_CarrierCode);
		}

		public void TestSynchroniseJPH_VesselName()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL 1";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals("VESSEL 1", header.JPH_VesselName);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL 2";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = JPTKY.RL_Code;
			AssertEquals("VESSEL 2", header.JPH_VesselName);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL 3";
			transport3.JW_RL_NKLoadPort = JPTKY.RL_Code;
			transport3.JW_RL_NKDiscPort = JPHTR.RL_Code;
			AssertEquals("VESSEL 2", header.JPH_VesselName);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_Vessel = "VESSEL 4";
			transport4.JW_RL_NKLoadPort = JPHTR.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals("VESSEL 2", header.JPH_VesselName);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("VESSEL 1", header.JPH_VesselName);

			transport1.JW_Vessel = "VESSEL 1A";
			AssertEquals("VESSEL 1A", header.JPH_VesselName);

			transport3.JW_RL_NKLoadPort = SGSIN.RL_Code;
			AssertEquals("VESSEL 3", header.JPH_VesselName);

			synchroniser.SetEnabled(false, false);
			transport3.JW_Vessel = "VESSEL 3A";
			AssertEquals("VESSEL 3", header.JPH_VesselName);
		}

		public void TestSynchroniseJPH_Voyage()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_VoyageFlight = "V1";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals("V1", header.JPH_Voyage);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_VoyageFlight = "V2";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = JPTKY.RL_Code;
			AssertEquals("V2", header.JPH_Voyage);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_VoyageFlight = "V3";
			transport3.JW_RL_NKLoadPort = JPTKY.RL_Code;
			transport3.JW_RL_NKDiscPort = JPHTR.RL_Code;
			AssertEquals("V2", header.JPH_Voyage);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_VoyageFlight = "V4";
			transport4.JW_RL_NKLoadPort = JPHTR.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals("V2", header.JPH_Voyage);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("V1", header.JPH_Voyage);

			transport1.JW_VoyageFlight = "V1A";
			AssertEquals("V1A", header.JPH_Voyage);

			transport3.JW_RL_NKLoadPort = SGSIN.RL_Code;
			AssertEquals("V3", header.JPH_Voyage);

			synchroniser.SetEnabled(false, false);
			transport3.JW_VoyageFlight = "V3A";
			AssertEquals("V3", header.JPH_Voyage);
		}

		public void TestSynchroniseJPH_RL_NKDischarge()
		{
			AssertEquals(ZString.Empty, header.JPH_RL_NKDischarge);
			consol.JK_RL_NKPortOfFirstArrival = JPTKY.RL_Code;
			AssertEquals(JPTKY.RL_Code, header.JPH_RL_NKDischarge);
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2011, 4, 10);
			AssertEquals(JPTKY.RL_Code, header.JPH_RL_NKDischarge);

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = JPHAO.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("Consol's PortOfFirstArrival is earlier", JPTKY.RL_Code, header.JPH_RL_NKDischarge);

			transport1.JW_ATA = new ZDateTime(2011, 4, 8);
			AssertEquals(JPHAO.RL_Code, header.JPH_RL_NKDischarge);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = JPHTR.RL_Code;
			transport2.JW_ATA = new ZDateTime(2011, 4, 7);

			AssertEquals("First Leg", JPHAO.RL_Code, header.JPH_RL_NKDischarge);

			transport1.JW_RL_NKLoadPort = JPHAO.RL_Code;
			AssertEquals("First Leg is not JP Bound", JPHTR.RL_Code, header.JPH_RL_NKDischarge);

			synchroniser.SetEnabled(false, false);
			transport2.JW_RL_NKDiscPort = JPTKY.RL_Code;
			AssertEquals(JPHTR.RL_Code, header.JPH_RL_NKDischarge);
		}

		public void TestSynchroniseJPH_RL_NKLoading()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL 1";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(AUSYD.RL_Code, header.JPH_RL_NKLoading);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL 2";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = JPTKY.RL_Code;
			AssertEquals(SGSIN.RL_Code, header.JPH_RL_NKLoading);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL 3";
			transport3.JW_RL_NKLoadPort = JPTKY.RL_Code;
			transport3.JW_RL_NKDiscPort = JPHTR.RL_Code;
			AssertEquals(SGSIN.RL_Code, header.JPH_RL_NKLoading);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_Vessel = "VESSEL 4";
			transport4.JW_RL_NKLoadPort = JPHTR.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals(SGSIN.RL_Code, header.JPH_RL_NKLoading);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(AUSYD.RL_Code, header.JPH_RL_NKLoading);

			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			AssertEquals(AUMEL.RL_Code, header.JPH_RL_NKLoading);

			transport3.JW_RL_NKLoadPort = AUSYD.RL_Code;
			AssertEquals(AUSYD.RL_Code, header.JPH_RL_NKLoading);

			synchroniser.SetEnabled(false, false);
			transport3.JW_RL_NKLoadPort = AUMEL.RL_Code;
			AssertEquals(AUSYD.RL_Code, header.JPH_RL_NKLoading);
		}

		public void TestSynchroniseJPH_ETD()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_ETD = new ZDateTime(2013, 10, 8);
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(new ZDateTime(2013, 10, 8), header.JPH_ETD);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_ETD = new ZDateTime(2013, 10, 15);
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = JPTKY.RL_Code;
			AssertEquals(new ZDateTime(2013, 10, 15), header.JPH_ETD);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_ETD = new ZDateTime(2013, 10, 18);
			transport3.JW_RL_NKLoadPort = JPTKY.RL_Code;
			transport3.JW_RL_NKDiscPort = JPHTR.RL_Code;
			AssertEquals(new ZDateTime(2013, 10, 15), header.JPH_ETD);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_ETD = new ZDateTime(2013, 10, 25);
			transport4.JW_RL_NKLoadPort = JPHTR.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals(new ZDateTime(2013, 10, 15), header.JPH_ETD);

			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(new ZDateTime(2013, 10, 8), header.JPH_ETD);

			transport1.JW_ETD = new ZDateTime(2013, 11, 25);
			AssertEquals(new ZDateTime(2013, 11, 25), header.JPH_ETD);

			transport3.JW_RL_NKLoadPort = SGSIN.RL_Code;
			AssertEquals(new ZDateTime(2013, 10, 18), header.JPH_ETD);

			transport3.JW_ATD = new ZDateTime(2013, 10, 17);
			AssertEquals(new ZDateTime(2013, 10, 17), header.JPH_ETD);

			synchroniser.SetEnabled(false, false);
			transport3.JW_ATD = new ZDateTime(2013, 10, 16);
			AssertEquals(new ZDateTime(2013, 10, 17), header.JPH_ETD);
		}

		public void TestSynchroniseJPH_ETA()
		{
			AssertEquals(ZDateTime.Empty, header.JPH_ETA);
			consol.JK_RL_NKPortOfFirstArrival = JPTKY.RL_Code;
			consol.JK_DatePortOfFirstArrival = new ZDateTime(2011, 4, 10);
			AssertEquals(new ZDateTime(2011, 4, 10), header.JPH_ETA);

			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = JPHAO.RL_Code;
			transport1.JW_ETA = new ZDateTime(2011, 4, 11);
			AssertEquals("Consol's PortOfFirstArrival is earlier", new ZDateTime(2011, 4, 10), header.JPH_ETA);

			transport1.JW_ATA = new ZDateTime(2011, 4, 8);
			AssertEquals(new ZDateTime(2011, 4, 8), header.JPH_ETA);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = JPHTR.RL_Code;
			transport2.JW_ATA = new ZDateTime(2011, 4, 7);

			AssertEquals("First Leg", new ZDateTime(2011, 4, 8), header.JPH_ETA);

			transport1.JW_RL_NKLoadPort = JPHAO.RL_Code;
			AssertEquals("First Leg is not JP Bound", new ZDateTime(2011, 4, 7), header.JPH_ETA);

			synchroniser.SetEnabled(false, false);
			transport2.JW_ATA = new ZDateTime(2011, 4, 6);
			AssertEquals(new ZDateTime(2011, 4, 7), header.JPH_ETA);
		}

		public void TestSynchroniseJPH_MasterBillNumber()
		{
			AssertEquals(ZString.Empty, header.JPH_MasterBillNumber);
			consol.JK_MasterBillNum = "MB3242";
			AssertEquals("MB3242", header.JPH_MasterBillNumber);
			header.Synchroniser.SetEnabled(false, false);
			header.JPH_MasterBillNumber = ZString.Empty;
			header.JPH_IsShippingLineEntry = ZBool.True;
			header.Synchroniser.SetEnabled(true, false);
			header.Synchroniser.Synchronise(true);
			AssertEquals(ZString.Empty, header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "MB3242";
			header.JPH_CarrierCode = "SPQA";
			AssertEquals("SPQAMB3242", header.JPH_MasterBillNumber);
			consol.JK_MasterBillNum = "  MB3242";
			AssertEquals("SPQAMB3242", header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "MBBC123456789012345678901234567890";
			header.JPH_CarrierCode = "SPQA";
			AssertEquals("SPQAC123456789012345678901234567890", header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "SPQA12345678901234567890123456";
			header.JPH_CarrierCode = "SPQA";
			AssertEquals("SPQA12345678901234567890123456", header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "SPQA12345678901234567890123456";
			header.JPH_CarrierCode = "XXX";
			AssertEquals("XXX-SPQA12345678901234567890123456", header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "SPQA12345678901234567890123456";
			header.JPH_CarrierCode = "XX";
			AssertEquals("XX--SPQA12345678901234567890123456", header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "SPQA12345678901234567890123456";
			header.JPH_CarrierCode = "X";
			AssertEquals("X---SPQA12345678901234567890123456", header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "SPQA12345678901234567890123456";
			header.JPH_CarrierCode = ZString.Empty;
			AssertEquals("SPQA12345678901234567890123456", header.JPH_MasterBillNumber);

			JPAFRRegistry.Instance.AFRAddSCACtoBillsDuringSync.SetValue(header.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			header.Synchroniser.SetEnabled(true, false);
			consol.JK_MasterBillNum = "MB3242";
			header.JPH_CarrierCode = "SPQB";
			AssertEquals("MB3242", header.JPH_MasterBillNumber);
			consol.JK_MasterBillNum = "  MB3242";
			AssertEquals("MB3242", header.JPH_MasterBillNumber);
		}

		public void TestSynchroniseBills()
		{
			AssertEquals(0, header.Bills.Count);
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "OTT1ABCD1234";

			AssertEquals(1, header.Bills.Count);
			var bill1 = header.Bills[0];
			AssertBill(bill1, "OTT1ABCD1234");

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "OTT1ABCD5678";

			AssertEquals(2, header.Bills.Count);
			AssertEquals(bill1, header.Bills["OTT1ABCD1234"]);
			var bill2 = header.Bills["OTT1ABCD5678"];
			AssertNotNull(bill2);

			shipment1.Delete();
			AssertEquals(1, header.Bills.Count);
			AssertBill(header.Bills[0], bill2.PK, "OTT1ABCD5678");
		}

		JPAFRHeaderConsolSynchroniser synchroniser;

		public OrgHeader TestShippingLine2
		{
			get
			{
				if (testShippingLine2 == null)
				{
					testShippingLine2 = Factory.New<OrgHeader>();
					testShippingLine2.OH_FullName = "Synchroniser Shipping Line 2";
					testShippingLine2.OH_Code = "TSTSHPLNE2";
					testShippingLine2.OH_IsShippingLine = true;
				}
				return testShippingLine2;
			}
		}
		OrgHeader testShippingLine2;

		public OrgHeader TestShippingLine3
		{
			get
			{
				if (testShippingLine3 == null)
				{
					testShippingLine3 = Factory.New<OrgHeader>();
					testShippingLine3.OH_FullName = "Synchroniser Shipping Line 3";
					testShippingLine3.OH_Code = "TSTSHPLNE3";
					testShippingLine3.OH_IsShippingLine = true;
				}
				return testShippingLine3;
			}
		}
		OrgHeader testShippingLine3;

		protected override void SetUp()
		{
			base.SetUp();

			synchroniser = new JPAFRHeaderConsolSynchroniser(header);
			synchroniser.Synchronise(true);
		}
	}
}
