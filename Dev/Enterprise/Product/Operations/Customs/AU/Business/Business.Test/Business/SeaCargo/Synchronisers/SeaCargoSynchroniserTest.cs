using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class SeaCargoSynchroniserTest : SeaCargoTestCase
	{
		class SeaCargoSynchroniserForTest : SeaCargoSynchroniser
		{
			public SeaCargoSynchroniserForTest(CommonConsol consol) : base(consol, null)
			{
				NeedCheckLockerExtension = false;
			}

			public new OrgCusCode LoadBondIDCusCode(OrgAddress bondIDOrgAddress) => base.LoadBondIDCusCode(bondIDOrgAddress);

			public void DeleteOceanBill()
			{
				OceanBill.Delete();
				OceanBill = null;
			}

			#region Implementation

			protected override Type OceanBillType => typeof(CusSCAOceanBill);

			protected override HouseBillSynchroniser GetNewHouseBillSynchroniser(CommonShipment shipment, CusSCAHouse house)
			{
				return new CMRHouseBillSynchroniser(house, shipment);
			}

			protected override bool NeedCheckLocker => NeedCheckLockerExtension;

			public bool NeedCheckLockerExtension { get; set; }

			#endregion
		}

		public void TestCreateOceanBillWithoutAnyLocks()
		{
			try
			{
				Factory.NameForDebugging = "Factory_SeaCargoSynchroniserTest";
				ErrorReporter.Clear();

				var testSynchroniser = new SeaCargoSynchroniserForTest(consol);
				testSynchroniser.NeedCheckLockerExtension = true;
				_ = testSynchroniser.OceanBill;

				var message = $@"A new Ocean Bill is being created without any locks from {consol.HumanReadableName} in factory[Factory_SeaCargoSynchroniserTest] by {Env.CurrentUser.FullName}.";
				AssertEquals("Should report the key as there is not a locker.", "CreateNewOceanBillOnConsolWithoutLock", ErrorReporter.LastKeyReported);
				AssertEquals("Should report the message as there is not a locker.", message, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();

				using (var mutex = CusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.Australia))
				{
					Assert("Testing for a lock should not leave the mutex locked", mutex.Lock());

					testSynchroniser.DeleteOceanBill();
					_ = testSynchroniser.OceanBill;

					AssertEquals("Should create a valid ocean bill without any errors.", string.Empty, ErrorReporter.LastKeyReported);
					AssertEquals("Should create a valid ocean bill without any errors.", string.Empty, ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestLoadBondIDCusCode()
		{
			var synchroniser = new SeaCargoSynchroniserForTest(consol);
			OrgAddress anAddress = CreateValidOrgAddress("FRED");
			OrgCusCode orgCusCode1 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode1.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode1.OK_CustomsRegNo = "111";
			orgCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			OrgCusCode orgCusCode2 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode2.OK_CustomsRegNo = "222";
			orgCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			OrgCusCode orgCusCode3 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode3.OK_CustomsRegNo = "333";
			orgCusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			OrgCusCode orgCusCode4 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode4.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode4.OK_CustomsRegNo = "444";
			orgCusCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			AssertEquals("AU Code", "333", synchroniser.LoadBondIDCusCode(anAddress).OK_CustomsRegNo);
		}

		public void TestConcurrencySavingConsol()
		{
			SetupFactoryForConcurrencySavingConsolTest(Factory);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			SetupFactoryForConcurrencySavingConsolTest(factory2);

			CommonConsol consol2 = factory2.Load<ForwardingConsol>(consol.PK);
			AssertNotNull(consol2);

			SeaCargoSynchroniser testSynchroniser2 = GetSeaCargoSynchroniser(consol2);
			CusSCAOceanBill oceanBill2 = testSynchroniser2.GetExistingOceanBill();
			AssertNull(oceanBill2);

			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);
			AssertEquals(Factory, consol.Factory);
			CusSCAOceanBill oceanBill = testSynchroniser.OceanBill;
			AssertNotNull(oceanBill);
			Factory.Save();

			oceanBill2 = testSynchroniser2.OceanBill;
			AssertNotNull(oceanBill2);
			Assert(consol != consol2);
			Assert(oceanBill != oceanBill2);
			AssertEquals("Synchroniser has found our other ocean bill", oceanBill2.PK, oceanBill.PK);
		}

		void SetupFactoryForConcurrencySavingConsolTest(BusinessObjectFactory factoryToSetup)
		{
			factoryToSetup.RefreshEnabled = false;
			((IBusinessObjectFactoryInternals)factoryToSetup).DisableQueryCacheReset = true; // Pretending the two factories are more separate. (ie. two enterprise instances)
		}

		public void TestSynchroniseConsolDetails()
		{
			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = testSynchroniser.OceanBill;

			AssertEquals("Synchronise Consol", consol.PK, oceanBill.CB_ParentId);
			AssertEquals("Synchronise CB_RL_NKPortOfLoading", consol.JK_JX_JA_RL_NKPortOfLoading, oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Synchronise CB_RL_NKPortOfDischarge", consol.JK_JX_JB_RL_NKPortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("Synchronise CB_OceanBill", consol.JK_MasterBillNum, oceanBill.CB_OceanBill);
			AssertEquals("Synchronise CB_RL_NKVesselName", consol.JK_JX_JV_NKVessel, oceanBill.CB_VesselName);
			AssertEquals("Synchronise CB_Voyage", consol.JK_JX_JV_VoyageFlight, oceanBill.CB_Voyage);
			AssertEquals("Synchronise CB_OH_ShippingLine", consol.ShippingLinePK, oceanBill.CB_OH_ShippingLine);
			AssertEquals("Synchronise CB_PrincipalID", ExpectedPrincipalID(consol.ShippingLine), oceanBill.CB_PrincipalID);
			AssertEquals("Synchronise CB_MasterHouse only on CoLoad/GatewayCoLoad", "", oceanBill.CB_MasterHouseBill);

			consol.JK_CoLoadMasterBill = "H90901";
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			testSynchroniser.SynchroniseOceanBill();
			oceanBill = testSynchroniser.OceanBill;
			AssertEquals("Synchronise CB_MasterHouse on CoLoad", consol.JK_CoLoadMasterBill, oceanBill.CB_MasterHouseBill);

			consol.JK_CoLoadMasterBill = "H90909";
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			testSynchroniser.SynchroniseOceanBill();
			oceanBill = testSynchroniser.OceanBill;
			AssertEquals("Synchronise CB_MasterHouse on GatewayCoLoad", consol.JK_CoLoadMasterBill, oceanBill.CB_MasterHouseBill);
		}

		public void TestSynchroniseConsolDetailsWithDomesticLegDifferentVessel()
		{
			RefVessel testVessel2 = RefVessel.New(Factory);
			testVessel2.RV_Code = "TESTVESSEL2";
			consol.JK_RL_NKDischargePort = "AUMEL";
			Transport transport1 = consol.Transports[0];
			transport1.JW_ETD = new ZDateTime(2008, 1, 1);
			transport1.JW_ETA = new ZDateTime(2008, 1, 15);
			transport1.JW_RL_NKDiscPort = "AUSYD";
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_VoyageFlight = "24";
			transport2.JW_Vessel = testVessel2.RV_Code;
			transport2.JW_ETD = transport1.JW_ETA.AddDays(1);
			transport2.JW_ETA = transport1.JW_ETA.AddDays(2);
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_RL_NKLoadPort = transport1.JW_RL_NKDiscPort;
			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = testSynchroniser.OceanBill;

			AssertEquals("Synchronise Consol", consol.PK, oceanBill.CB_ParentId);
			AssertEquals("Synchronise CB_RL_NKPortOfLoading", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Synchronise CB_RL_NKPortOfDischarge", "AUSYD", oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("Synchronise CB_OceanBill", consol.JK_MasterBillNum, oceanBill.CB_OceanBill);
			AssertEquals("Synchronise CB_VesselName", transport1.JW_Vessel, oceanBill.CB_VesselName);
			AssertEquals("Synchronise CB_Voyage", transport1.JW_VoyageFlight, oceanBill.CB_Voyage);
			AssertEquals("Synchronise CB_OH_ShippingLine", consol.ShippingLinePK, oceanBill.CB_OH_ShippingLine);
			AssertEquals("Synchronise CB_PrincipalID", ExpectedPrincipalID(consol.ShippingLine), oceanBill.CB_PrincipalID);
		}

		public void TestSynchroniseConsolDetailsWithDomesticLegSameVessel()
		{
			consol.JK_RL_NKDischargePort = "AUMEL";
			Transport transport1 = consol.Transports[0];
			transport1.JW_ETD = new ZDateTime(2008, 1, 1);
			transport1.JW_ETA = new ZDateTime(2008, 1, 15);
			transport1.JW_RL_NKDiscPort = "AUSYD";
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_VoyageFlight = "24";
			transport2.JW_Vessel = transport1.JW_Vessel;
			transport2.JW_ETD = transport1.JW_ETA.AddDays(1);
			transport2.JW_ETA = transport1.JW_ETA.AddDays(2);
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_RL_NKLoadPort = transport1.JW_RL_NKDiscPort;
			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = testSynchroniser.OceanBill;

			AssertEquals("Synchronise Consol", consol.PK, oceanBill.CB_ParentId);
			AssertEquals("Synchronise CB_RL_NKPortOfLoading", "SGSIN", oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Synchronise CB_RL_NKPortOfDischarge", "AUMEL", oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("Synchronise CB_OceanBill", consol.JK_MasterBillNum, oceanBill.CB_OceanBill);
			AssertEquals("Synchronise CB_VesselName", transport2.JW_Vessel, oceanBill.CB_VesselName);
			AssertEquals("Synchronise CB_Voyage", transport2.JW_VoyageFlight, oceanBill.CB_Voyage);
			AssertEquals("Synchronise CB_OH_ShippingLine", consol.ShippingLinePK, oceanBill.CB_OH_ShippingLine);
			AssertEquals("Synchronise CB_PrincipalID", ExpectedPrincipalID(consol.ShippingLine), oceanBill.CB_PrincipalID);
		}

		protected abstract ZString ExpectedPrincipalID(OrgHeader principalOrg);
		public void TestSynchroniseConsolDetailsDoesNoStripping()
		{
			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = testSynchroniser.OceanBill;
			ZString masterBillUnstripped = "/.?<--__" + MasterBillNumber + "[]$%^;'";
			consol.JK_MasterBillNum = masterBillUnstripped;
			AssertEquals("No characters stripped from MasterBill before putting in CB_OceanBill", masterBillUnstripped, oceanBill.CB_OceanBill);
		}

		public void TestSynchroniseShipmentDetailsDoesNoStripping()
		{
			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);
			ZString unstrippedHouseBill = "@(*&(" + TestHouseBill + "&*@!$";
			shipment.JS_HouseBill = unstrippedHouseBill;

			CusSCAHouse houseBill = testSynchroniser.OceanBill.HouseBills.AddNew();
			testSynchroniser.SynchroniseHouse(houseBill, shipment);

			AssertEquals("Strip characters from Shipment before putting in CA_HouseBill", unstrippedHouseBill, houseBill.CA_HouseBill);
		}

		public void TestSynchroniseConsignorAddressFromShipmentAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var consignorOrg = Factory.New<OrgHeader>();
			consignorOrg.OH_FullName = "Test Consignor";
			var consigneeAddressPK = consignorOrg.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consigneeAddressPK;
			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.OceanBill.HouseBills.AddNew();

			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("Default to shipment Consignee address", consigneeAddressPK, houseBill.CA_OA_ConsignorAddress);
		}

		public void TestSynchroniseConsignorAddressFromShipmentOverrideAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "NAME OVERRIDE2";
			shipment.ConsignorDocumentaryAddress.E2_City = "CITY OVERRIDE2";
			shipment.ConsignorDocumentaryAddress.E2_Phone = "PHONE OVERRIDE2";
			shipment.ConsignorDocumentaryAddress.E2_Postcode = "PC OVR2";
			shipment.ConsignorDocumentaryAddress.E2_State = "STATE OVRRIDE2";
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "ADDRESS1 OVERRIDE2";
			shipment.ConsignorDocumentaryAddress.E2_Address2 = "ADDRESS2 OVERRIDE2";
			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.OceanBill.HouseBills.AddNew();

			synchroniser.SynchroniseHouse(houseBill, shipment);
			CombineAssertions("Populate from shipment", () =>
			{
				AssertEquals(ZGuid.Empty, houseBill.CA_OA_ConsignorAddress);
				AssertEquals("NAME OVERRIDE2", houseBill.CA_ConsignorName);
				AssertEquals("ADDRESS1 OVERRIDE2", houseBill.CA_ConsignorAddress1);
				AssertEquals("ADDRESS2 OVERRIDE2", houseBill.CA_ConsignorAddress2);
				AssertEquals("CITY OVERRIDE2 STATE OVRRIDE2", houseBill.CA_ConsignorSuburb);
				AssertEquals(ZString.Empty, houseBill.CA_ConsignorState);
				AssertEquals("PC OVR2", houseBill.CA_ConsignorPostcode);
				AssertEquals("PHONE OVERRIDE2", houseBill.CA_ConsignorPhone);
			});
		}

		public void TestSynchroniseConsigneeAddressFromShipmentAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_FullName = "Test Consignee";
			var deliveryOrg = Factory.New<OrgHeader>();
			deliveryOrg.OH_FullName = "Test Delivery";
			var consigneeAddressPK = consigneeOrg.MainAddress.PK;
			var deliveryAddressPK = deliveryOrg.MainAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeAddressPK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddressPK;
			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.OceanBill.HouseBills.AddNew();

			consigneeOrg.MiscServ.OM_IMSeaCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.Consignee;
			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("Default to shipment Consignee address", consigneeAddressPK, houseBill.CA_OA_ConsigneeAddress);

			consigneeOrg.MiscServ.OM_IMSeaCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.DeliverTo;
			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("Default to shipment Delivery address", deliveryAddressPK, houseBill.CA_OA_ConsigneeAddress);

			houseBill.CA_OA_ConsigneeAddress = ZGuid.Empty;
			consigneeOrg.MiscServ.OM_IMSeaCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.None;
			synchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("Doesn't default address", ZGuid.Empty, houseBill.CA_OA_ConsigneeAddress);

			consigneeOrg.MiscServ.OM_IMSeaCargoReportDefaultConsignee = ConsigneeDefaultOptionList.Codes.Default;
			using (AUCustomsDataRegistry.Instance.DefaultSeaCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.Consignee))
			{
				synchroniser.SynchroniseHouse(houseBill, shipment);
				AssertEquals("Default to shipment Consignee address based on registry setting", consigneeAddressPK, houseBill.CA_OA_ConsigneeAddress);
			}

			using (AUCustomsDataRegistry.Instance.DefaultSeaCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo))
			{
				synchroniser.SynchroniseHouse(houseBill, shipment);
				AssertEquals("Default to shipment Delivery address based on registry setting", deliveryAddressPK, houseBill.CA_OA_ConsigneeAddress);
			}

			using (AUCustomsDataRegistry.Instance.DefaultSeaCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.None))
			{
				houseBill.CA_OA_ConsigneeAddress = ZGuid.Empty;
				synchroniser.SynchroniseHouse(houseBill, shipment);
				AssertEquals("Doesn't default address based on registry setting", ZGuid.Empty, houseBill.CA_OA_ConsigneeAddress);
			}
		}

		public void TestSynchroniseConsigneeAddressFromShipmentOverrideAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = "NAME OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_City = "CITY OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Phone = "PHONE OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "PC OVR1";
			shipment.ConsigneeDeliveryAddress.E2_State = "STATE OVRRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "ADDRESS1 OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Address2 = "ADDRESS2 OVERRIDE1";
			shipment.ConsigneeDeliveryAddress.E2_Fax = "FAX OVERRIDE1";
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "NAME OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_City = "CITY OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Phone = "PHONE OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Postcode = "PC OVR2";
			shipment.ConsigneeDocumentaryAddress.E2_State = "STATE OVRRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "ADDRESS1 OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "ADDRESS2 OVERRIDE2";
			shipment.ConsigneeDocumentaryAddress.E2_Fax = "FAX OVERRIDE2";
			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.OceanBill.HouseBills.AddNew();

			using (AUCustomsDataRegistry.Instance.DefaultSeaCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.None))
			{
				synchroniser.SynchroniseHouse(houseBill, shipment);
				CombineAssertions("Doesn't default address based on registry setting", () =>
				{
					AssertEquals(ZGuid.Empty, houseBill.CA_OA_ConsigneeAddress);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeName);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeAddress1);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeAddress2);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeSuburb);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeState);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneePostcode);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeFax);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneePhone);
				});
			}

			using (AUCustomsDataRegistry.Instance.DefaultSeaCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.DeliverTo))
			{
				synchroniser.SynchroniseHouse(houseBill, shipment);
				CombineAssertions("Default to shipment Consignee address based on registry setting", () =>
				{
					AssertEquals(ZGuid.Empty, houseBill.CA_OA_ConsigneeAddress);
					AssertEquals("NAME OVERRIDE1", houseBill.CA_ConsigneeName);
					AssertEquals("ADDRESS1 OVERRIDE1", houseBill.CA_ConsigneeAddress1);
					AssertEquals("ADDRESS2 OVERRIDE1", houseBill.CA_ConsigneeAddress2);
					AssertEquals("CITY OVERRIDE1 STATE OVRRIDE1", houseBill.CA_ConsigneeSuburb);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeState);
					AssertEquals("PC OVR1", houseBill.CA_ConsigneePostcode);
					AssertEquals("FAX OVERRIDE1", houseBill.CA_ConsigneeFax);
					AssertEquals("PHONE OVERRIDE1", houseBill.CA_ConsigneePhone);
				});
			}

			using (AUCustomsDataRegistry.Instance.DefaultSeaCargoConsignee.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.AUCustoms.DefaultConsigneeOption.Consignee))
			{
				synchroniser.SynchroniseHouse(houseBill, shipment);
				CombineAssertions("Default to shipment Delivery address based on registry setting", () =>
				{
					AssertEquals(ZGuid.Empty, houseBill.CA_OA_ConsigneeAddress);
					AssertEquals("NAME OVERRIDE2", houseBill.CA_ConsigneeName);
					AssertEquals("ADDRESS1 OVERRIDE2", houseBill.CA_ConsigneeAddress1);
					AssertEquals("ADDRESS2 OVERRIDE2", houseBill.CA_ConsigneeAddress2);
					AssertEquals("CITY OVERRIDE2 STATE OVRRIDE2", houseBill.CA_ConsigneeSuburb);
					AssertEquals(ZString.Empty, houseBill.CA_ConsigneeState);
					AssertEquals("PC OVR2", houseBill.CA_ConsigneePostcode);
					AssertEquals("FAX OVERRIDE2", houseBill.CA_ConsigneeFax);
					AssertEquals("PHONE OVERRIDE2", houseBill.CA_ConsigneePhone);
				});
			}
		}

		public void TestSynchroniseDeliveryWithShipmentOverride()
		{
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDeliveryAddress.E2_CompanyName = "DLV Consignee";
			shipment.ConsigneeDeliveryAddress.E2_Address1 = "DLV Address 1";
			shipment.ConsigneeDeliveryAddress.E2_Address2 = "DLV Address 2";
			shipment.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsigneeDeliveryAddress.E2_Fax = "1111";
			shipment.ConsigneeDeliveryAddress.E2_Phone = "2222";
			shipment.ConsigneeDeliveryAddress.E2_Postcode = "3333";
			shipment.ConsigneeDeliveryAddress.E2_State = "DLVState";
			shipment.ConsigneeDeliveryAddress.E2_City = "DLVCity";

			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.OceanBill.HouseBills.AddNew();
			synchroniser.SynchroniseHouse(houseBill, shipment);

			CombineAssertions(() =>
			{
				AssertEquals(ZGuid.Empty, houseBill.CA_OH_Consignee);
				AssertEquals("DLV Consignee", houseBill.CA_ConsigneeName);
				AssertEquals("DLV Address 1", houseBill.CA_ConsigneeAddress1);
				AssertEquals("DLV Address 2", houseBill.CA_ConsigneeAddress2);
				AssertEquals("DLVCity DLVState", houseBill.CA_ConsigneeSuburb);
				AssertEquals("", houseBill.CA_ConsigneeState);
				AssertEquals("3333", houseBill.CA_ConsigneePostcode);
				AssertEquals("1111", houseBill.CA_ConsigneeFax);
				AssertEquals("2222", houseBill.CA_ConsigneePhone);
			});
		}

		public void TestSynchroniseConsignorOverride()
		{
			SetUpJobDocAddresses();

			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.OceanBill.HouseBills.AddNew();
			synchroniser.SynchroniseHouse(houseBill, shipment);

			CombineAssertions(() =>
			{
				AssertEquals(ZGuid.Empty, houseBill.CA_OH_Consignor);
				AssertEquals("Doc Consignor", houseBill.CA_ConsignorName);
				AssertEquals("CNOR Address 1", houseBill.CA_ConsignorAddress1);
				AssertEquals("CNOR Address 2", houseBill.CA_ConsignorAddress2);
				AssertEquals("CNORCity CNORState", houseBill.CA_ConsignorSuburb);
				AssertEquals("", houseBill.CA_ConsignorState);
				AssertEquals("AU", houseBill.CA_RN_NKConsignorCountryCode);
				AssertEquals("4545", houseBill.CA_ConsignorPostcode);
			});
		}

		public void TestSynchroniseNotifyPartyOverride()
		{
			SetUpJobDocAddresses();

			var synchroniser = GetSeaCargoSynchroniser(consol);
			var houseBill = synchroniser.OceanBill.HouseBills.AddNew();
			synchroniser.SynchroniseHouse(houseBill, shipment);

			CombineAssertions(() =>
			{
				AssertEquals(ZGuid.Empty, houseBill.CA_OH_Notify);
				AssertEquals("Doc NotifyParty", houseBill.CA_NotifyName);
				AssertEquals("NP Address 1", houseBill.CA_NotifyAddress1);
				AssertEquals("NP Address 2", houseBill.CA_NotifyAddress2);
				AssertEquals("NPCity NPState", houseBill.CA_NotifySuburb);
				AssertEquals("", houseBill.CA_NotifyState);
				AssertEquals("AU", houseBill.CA_RN_NKNotifyCountryCode);
				AssertEquals("7878", houseBill.CA_NotifyPostcode);
				AssertEquals("5656", houseBill.CA_NotifyFax);
				AssertEquals("6767", houseBill.CA_NotifyPhone);
			});
		}

		protected abstract ZString ExpectedNotSentState { get; }

		public void TestSynchroniseMultiplePacklineContainerShipment()
		{
			// Add Multiple Containers to Shipment

			SeaCargoSynchroniser testSynchroniser = GetSeaCargoSynchroniser(consol);

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_RC = Get40FootGPContainer().PK;
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_RC = Get40FootGPContainer().PK;
			container2.JC_ContainerNum = ContainerNumber2;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			CommonContainer container3 = consol.Containers.AddNew();
			container3.JC_RC = Get40FootGPContainer().PK;
			container3.JC_ContainerNum = ContainerNumber3;
			container3.JC_ContainerMode = Core.Constants.ContainerModes.FCL;

			PackLine packLine1 = null;
			if (shipment.OuterPackLines.Count == 0)
			{
				packLine1 = shipment.OuterPackLines.AddNew();
			}
			else
			{
				packLine1 = shipment.OuterPackLines[0];
			}
			packLine1.JL_Calc_ContainerNumber = ContainerNumber1;
			packLine1.JL_Description = PackLine1Description;
			packLine1.JL_PackageCount = 4;
			packLine1.JL_F3_NKPackType = "BOX";
			packLine1.JL_ActualWeight = 100m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_Calc_ContainerNumber = ContainerNumber2;
			packLine2.JL_Description = "";
			packLine2.JL_PackageCount = 2;
			packLine2.JL_F3_NKPackType = "PKG";

			PackLine packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_Description = "PackLine 3";
			packLine3.JL_Calc_ContainerNumber = ContainerNumber3;
			packLine3.JL_PackageCount = 6;
			packLine3.JL_ActualVolume = 2m;
			packLine3.JL_ActualVolumeUQ = Core.Constants.Volume.CubicFeet;
			packLine3.JL_F3_NKPackType = "CTN";

			CusSCAHouse houseBill = testSynchroniser.GetHouseBill(shipment);
			foreach (CusSCAPivot aPivot in houseBill.Pivot)
			{
				PackLine aPackLine = FindContainerFromPackline(aPivot.CV_AssociatedContainer);
				AssertContainersEquals(aPackLine, aPivot);
				AssertEquals("Marks And Numbers", MarksAndNumbersNoteFromPackLine(aPackLine), aPivot.CV_MarksAndNumbers);
			}
			testSynchroniser.SynchroniseHouse(houseBill, shipment);
			AssertEquals("Failed to force synchronisation", 3, houseBill.Pivot.Count);
		}

		public void TestDefaultContainerTypes_BuyersConsol_FCXContainers_SCAFCLContainers()
		{
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			CommonContainer fCLContainer1 = consol.Containers.AddNew();
			fCLContainer1.JC_ContainerNum = ContainerNumber1;
			fCLContainer1.JC_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;

			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			AssertEquals("Container Count", 1, oceanBill.Containers.Count);
			AssertEquals("Container Type", Core.Constants.ContainerModes.FCLMixedShipper, oceanBill.Containers[0].CN_ContainerMode);
		}

		public void TestContainerTypeNotSet()
		{
			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;

			CommonContainer fCLContainer1 = consol.Containers.AddNew();
			fCLContainer1.JC_ContainerNum = ContainerNumber1;
			fCLContainer1.JC_ContainerMode = Core.Constants.ContainerModes.FCLMixedShipper;

			ForwardingShipment shipment = (ForwardingShipment)consol.Shipments[0];

			CusSCAHouse house = synchroniser.GetHouseBill(shipment);
			AssertNotNull("House from Shipment with no containers", house);
		}

		public void TestAutomaticSynchBeforeMessagesSent()
		{
			CommonContainer addedContainer1 = consol.Containers.AddNew();
			shipment.OuterPackLines[0].SetContainer(consol, addedContainer1);
			addedContainer1.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			shipment.OuterPackLines[0].SetContainer(consol, addedContainer1);
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			shipment.JS_HouseBill = TestAltHouseBill;
			AssertEquals("House Bill Number", TestAltHouseBill, houseBill.CA_HouseBill);
			addedContainer1.JC_ContainerNum = ContainerNumber1;
			AssertEquals("House Bill Container 1 Number", ContainerNumber1, oceanBill.HouseBills[0].Pivot[0].CV_AssociatedContainer);
			CommonContainer addedContainer2 = consol.Containers.AddNew();
			addedContainer2.JC_ContainerNum = ContainerNumber2;
			addedContainer2.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[1].SetContainer(consol, addedContainer2);

			AssertEquals("House Bill Container 2 Number", ContainerNumber2, oceanBill.HouseBills[0].Pivot[1].CV_AssociatedContainer);
		}

		public void TestAutomaticSynchStopsAfterMessageSent()
		{
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			shipment.JS_HouseBill = TestAltHouseBill;
			AssertEquals("House Bill Number", TestAltHouseBill, houseBill.CA_HouseBill);
			houseBill.Messages.AddNew();

			shipment.JS_HouseBill = TestHouseBill;
			AssertEquals("House Bill Number", TestAltHouseBill, houseBill.CA_HouseBill);
		}

		public void TestGroupageConsolSynchronised()
		{
			OrgAddress anAddress = CreateValidOrgAddress("FRED");
			anAddress.Header.OH_RL_NKClosestPort = "AUBNE";
			anAddress.Header.OH_IsUnpackDepot = true;
			anAddress.Header.MiscServ.OM_SVServicesCategory = Core.Constants.ServicesCategory.Preferred;
			OrgCusCode orgCusCode1 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode1.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode1.OK_CustomsRegNo = "111";
			orgCusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			OrgCusCode orgCusCode2 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode2.OK_CustomsRegNo = "222";
			orgCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			OrgCusCode orgCusCode3 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode3.OK_CustomsRegNo = "333";
			orgCusCode3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			OrgCusCode orgCusCode4 = anAddress.Header.CustomsCodes.AddNew();
			orgCusCode4.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			orgCusCode4.OK_CustomsRegNo = "444";
			orgCusCode4.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Canada;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertNotNull("House Bill should be null", houseBill);
		}

		public void TestDeletingHouseBills()
		{
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			houseBill.Delete();
			shipment.JS_HouseBill = "DONOTSYNCH";
			AssertEquals("Synchroniser should be turned off", 0, synchroniser.OceanBill.HouseBills.Count);
		}

		public void TestDefaultingBulkPackageCount()
		{
			consol = CreateBulkConsol();
			shipment = (ForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 20;
			shipment.JS_F3_NKPackType = "BOX";
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals("Package count should be 0 for Bulk", 0, houseBill.Pivot[0].CV_PackageCount);
			AssertEquals("Bulk Container line should exist", 1, oceanBill.Containers.Count);
			AssertEquals("Should be a bulk container", CusSCAPivot.Bulk, oceanBill.Containers[0].CN_ContainerNumber);
		}

		public void TestDefaultingBreakBulkConsol()
		{
			consol = CreateBreakBulkConsol();
			shipment = (ForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;
			shipment.JS_F3_NKPackType = "PCE";
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals("Package count should be 4 for Break Bulk", 4, houseBill.Pivot[0].CV_PackageCount);
			AssertEquals("Break Bulk Container line should exist", 1, oceanBill.Containers.Count);
			AssertEquals("Should be a Break bulk container", CusSCAPivot.BreakBulk, oceanBill.Containers[0].CN_ContainerNumber);
			AssertEquals("Pivot CV_AssociatedContainer", CusSCAPivot.BreakBulk, houseBill.Pivot[0].CV_AssociatedContainer);
		}

		public void TestDefaultingROROConsol()
		{
			consol = CreateBreakBulkConsol();
			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.RollOnRollOff;
			shipment = (ForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_OuterPacks = 4;
			shipment.JS_F3_NKPackType = "PCE";
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAHouse houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals("Package count should be 4", 4, houseBill.Pivot[0].CV_PackageCount);
			AssertEquals("Break Bulk Container line should exist", 1, oceanBill.Containers.Count);
			AssertEquals("Should be a Break bulk container", CusSCAPivot.BreakBulk, oceanBill.Containers[0].CN_ContainerNumber);
		}

		public void TestCLoadMasterSetting()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			masterShipment.JS_HouseBill = "MASTERSHP";
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			masterShipment.JS_RL_NKOrigin = "SGSIN";
			masterShipment.JS_RL_NKDestination = "AUBNE";
			masterShipment.JS_RL_NKOrigin = "SG";
			masterShipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			masterShipment.JS_GoodsDescription = "Shipment Goods Description";
			masterShipment.JS_OuterPacks = 12;
			masterShipment.JS_F3_NKPackType = "BOX";
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var synchroniser = GetSeaCargoSynchroniser(consol);
			var oceanBill = synchroniser.OceanBill;
			var houseBill = synchroniser.GetHouseBill(shipment);
			AssertEquals("House bill set", TestHouseBill, houseBill.CA_HouseBill);
			AssertEquals("Master house NOT set for buyers consol lead", ZString.Empty, houseBill.CA_MasterHouseBill);

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertEquals("Master house IS set for non-buyers consol lead", "MASTERSHP", houseBill.CA_MasterHouseBill);

			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			shipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertEquals("Master house NOT set for buyers consol lead re-sync", ZString.Empty, houseBill.CA_MasterHouseBill);
		}

		public void TestGetHouseBillInDifferentFactory()
		{
			Factory.Save();
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var consol2 = factory2.Load<CommonConsol>(consol.PK);
			var shipment2 = (ForwardingShipment)consol2.Shipments.FindByPK(shipment.PK);
			SeaCargoSynchroniser synchroniser2 = GetSeaCargoSynchroniser(consol2);

			CusSCAHouse result2 = (CusSCAHouse)new BaseCusSCAHouse.Loader(factory2).LoadFromShipmentAndApplicationCode(shipment2.PK, CusSCAOceanBill.ApplicationCodes);
			ZGuid houseBill1PK = new ZGuid("EA579621-25D6-4F27-9ECC-6BC5DACDDD2E");

			var insertOceanBillAndHouseBill = $@"
INSERT INTO dbo.CusSCAOceanBill (CB_PK, CB_ApplicationCode, CB_OceanBill, CB_ParentId, CB_ParentTableCode, CB_GB, CB_SystemCreateTimeUtc, CB_SystemCreateUser, CB_SystemLastEditTimeUtc, CB_SystemLastEditUser) VALUES ('4661C4A2-E5E7-4CE1-B204-AD6AD94E9DB1', 'CMR', 'OBL373892029', '{consol.PK}', 'JK', '{GlbBranch.CurrentBranch.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.CusSCAHouse (CA_PK, CA_HouseBill, CA_BGMReference, CA_JS, CA_CB, CA_SystemCreateTimeUtc, CA_SystemCreateUser, CA_SystemLastEditTimeUtc, CA_SystemLastEditUser) VALUES ('{houseBill1PK.ToString()}', 'O200429188', 'S00001000', '{shipment.PK}', '4661C4A2-E5E7-4CE1-B204-AD6AD94E9DB1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
			CargoWise.Data.Db.Connection.ExecuteNonQuery(insertOceanBillAndHouseBill);
			var houseBill2 = synchroniser2.GetHouseBill(shipment2);
			AssertEquals("Do not create duplicated house bill", houseBill1PK, houseBill2.PK);
		}

		#region Implementation

		protected const string PackLine1Description = "PACK LINE 1 DESCRIPTION";
		protected ForwardingShipment shipment;
		protected CommonConsol consol;
		protected OrgHeader consignor;
		protected OrgAddress consignorPickupAddress;
		protected OrgHeader consignee;
		protected OrgAddress consigneeDevlieryAddress;

		protected abstract SeaCargoSynchroniser GetSeaCargoSynchroniser(CommonConsol consol);

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			SetUpConsol();
			SetUpShipment();
		}

		const string VesselLloydsNumber = "1278239";
		const string MasterBillNumber = "OBL373892029";
		protected const string TestManifestProviderID = "C399220143";
		protected void SetUpConsol()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];

			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = MasterBillNumber;
			consol.JK_BookingReference = "BOOKINGREF";
			consol.JK_CoLoadMasterBill = "COLOADMB1";
			transport.JW_VoyageFlight = "23";
			consol.SetDefaultReceivingForwarderAddress(CreateValidOrgHeader("Forwarder"));
			consol.ReceivingForwarder.LocalManifestID = TestManifestProviderID;

			RefVessel testVessel = RefVessel.New(Factory);
			testVessel.RV_Code = "TESTVESSEL";
			transport.JW_Vessel = testVessel.RV_Code;
			consol.Vessel.RV_LloydsNumber = VesselLloydsNumber;

			OrgHeader shippingLine = CreateValidOrgHeader("SHIPLINE");
			shippingLine.OH_FullName = "Test Shipping Line";
			shippingLine.SetLocalCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "C011920192");
			shippingLine.LocalBusinessRegNo = "85008945846";
			consol.SetDefaultShippingLineAddress(shippingLine);
		}

		const string TestHouseBill = "O200429188";
		const string TestAltHouseBill = "2393/11293FA";
		protected void SetUpShipment()
		{
			shipment = (ForwardingShipment)consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			shipment.JS_HouseBill = TestHouseBill;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_RL_NKOrigin = "SG";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard; //Prepaid
			shipment.JS_GoodsDescription = "Shipment Goods Description";
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = "BOX";

			consignor = CreateValidOrgHeader("CONSIGNOR");
			consignor.OH_FullName = "TEST CONSIGNOR";
			consignor.MainAddress.OA_Address1 = "addr1";
			consignor.MainAddress.OA_Address2 = "addr2";
			consignor.MainAddress.OA_City = "foocity";
			consignor.MainAddress.OA_Phone = "123";
			consignor.MainAddress.OA_PostCode = "3234";
			consignor.MainAddress.OA_State = "BOO";
			consignor.OH_IsConsignor = true;

			consignorPickupAddress = consignor.Addresses.AddNew(OrgAddressType.Pickup, false);
			consignorPickupAddress.OA_Address1 = "Pickup addr1";
			consignorPickupAddress.OA_Address2 = "Pickup addr2";
			consignorPickupAddress.OA_City = "Pickup foocity";
			consignorPickupAddress.OA_Phone = "321";
			consignorPickupAddress.OA_PostCode = "4323";
			consignorPickupAddress.OA_State = "NSW";

			shipment.ConsignorPK = consignor.PK;
			consignee = CreateValidOrgHeader("CONSIGNEE");
			consignee.OH_FullName = "TEST CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "1addr";
			consignee.MainAddress.OA_Address2 = "2addr";
			consignee.MainAddress.OA_City = "barcity";
			consignee.MainAddress.OA_Phone = "333";
			consignee.MainAddress.OA_Fax = "555";
			consignee.MainAddress.OA_PostCode = "6768";
			consignee.MainAddress.OA_State = "HEH";
			consignee.OH_IsConsignee = true;
			shipment.ConsigneePK = consignee.PK;

			consigneeDevlieryAddress = consignee.Addresses.AddNew(OrgAddressType.Delivery, false);
			consigneeDevlieryAddress.OA_Address1 = "Delivery 1addr";
			consigneeDevlieryAddress.OA_Address2 = "Delivery 2addr";
			consigneeDevlieryAddress.OA_City = "Delivery barcity";
			consigneeDevlieryAddress.OA_Phone = "444";
			consigneeDevlieryAddress.OA_Fax = "666";
			consigneeDevlieryAddress.OA_PostCode = "8687";
			consigneeDevlieryAddress.OA_State = "ACT";
		}

		void SetUpJobDocAddresses()
		{
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsigneeDocumentaryAddress.E2_CompanyName = "Doc Consignee";
			shipment.ConsigneeDocumentaryAddress.E2_Address1 = "CNEE Address 1";
			shipment.ConsigneeDocumentaryAddress.E2_Address2 = "CNEE Address 2";
			shipment.ConsigneeDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsigneeDocumentaryAddress.E2_Fax = "1212";
			shipment.ConsigneeDocumentaryAddress.E2_Phone = "2323";
			shipment.ConsigneeDocumentaryAddress.E2_Postcode = "3434";
			shipment.ConsigneeDocumentaryAddress.E2_State = "CNEEState";
			shipment.ConsigneeDocumentaryAddress.E2_City = "CNEECity";

			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment.ConsignorDocumentaryAddress.E2_CompanyName = "Doc Consignor";
			shipment.ConsignorDocumentaryAddress.E2_Address1 = "CNOR Address 1";
			shipment.ConsignorDocumentaryAddress.E2_Address2 = "CNOR Address 2";
			shipment.ConsignorDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.ConsignorDocumentaryAddress.E2_Postcode = "4545";
			shipment.ConsignorDocumentaryAddress.E2_State = "CNORState";
			shipment.ConsignorDocumentaryAddress.E2_City = "CNORCity";

			shipment.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			shipment.NotifyPartyDocumentaryAddress.E2_CompanyName = "Doc NotifyParty";
			shipment.NotifyPartyDocumentaryAddress.E2_Address1 = "NP Address 1";
			shipment.NotifyPartyDocumentaryAddress.E2_Address2 = "NP Address 2";
			shipment.NotifyPartyDocumentaryAddress.E2_RN_NKCountryCode = "AU";
			shipment.NotifyPartyDocumentaryAddress.E2_Fax = "5656";
			shipment.NotifyPartyDocumentaryAddress.E2_Phone = "6767";
			shipment.NotifyPartyDocumentaryAddress.E2_Postcode = "7878";
			shipment.NotifyPartyDocumentaryAddress.E2_State = "NPState";
			shipment.NotifyPartyDocumentaryAddress.E2_City = "NPCity";
		}

		protected OrgHeader CreateValidOrgHeader(ZString code)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_Code = code;
			result.MainAddress.OA_Address1 = "45 Somewhere Over";
			result.MainAddress.OA_Address2 = "The Rainbow";
			result.MainAddress.OA_City = "Emerald City";
			result.MainAddress.OA_State = "OZ";
			result.MainAddress.OA_PostCode = "21290";

			return result;
		}

		protected OrgAddress CreateValidOrgAddress(string code)
		{
			OrgHeader validOrgHeader = CreateValidOrgHeader(code);
			OrgAddress result = validOrgHeader.Addresses.AddNew();
			result.OA_Address1 = "Address at docks";
			result.OA_City = "Botany";
			result.OA_PostCode = "2014";
			result.OA_State = "NSW";
			result.OA_RL_NKRelatedPortCode = "AUSYD";
			return result;
		}

		#endregion

		protected PackLine FindContainerFromPackline(string containerNumber)
		{
			PackLine result = null;
			foreach (PackLine aPackLine in shipment.OuterPackLines)
			{
				if (aPackLine.JL_Calc_ContainerNumber == containerNumber)
				{
					result = aPackLine;
					break;
				}
			}
			return result;
		}

		protected ZString MarksAndNumbersNoteFromPackLine(PackLine packLine)
		{
			ZString result = "";
			if (result.IsEmpty && packLine.Containers.Count > 0 && packLine.Containers[0].JC_ContainerMode == Core.Constants.ContainerModes.FCL)
			{
				result = packLine.Containers[0].JC_ContainerNum;
			}
			return result;
		}

		protected void AssertContainersEquals(PackLine aPackLine, CusSCAPivot aPivot)
		{
			AssertEquals("Container GoodsDescription", shipment.JS_GoodsDescription, aPivot.CV_GoodsDescription);
			if (aPackLine.JL_PackageCount != 0)
			{
				AssertEquals("Container PackageCount", (int)aPackLine.JL_PackageCount, (int)aPivot.CV_PackageCount);
			}
			else
			{
				AssertEquals("Container PackageCount", shipment.JS_OuterPacks, aPivot.CV_PackageCount);
			}
			AssertEquals("Container Package Type", SeaCargoUtilities.ConvertPkgUnitToCMRPackageType(aPackLine.JL_F3_NKPackType), aPivot.CV_PackageType);
			AssertEquals("Container Weight", aPackLine.JL_ActualWeight, aPivot.CV_Weight);
			AssertEquals("Container Weight UQ", aPackLine.JL_ActualWeightUQ, aPivot.CV_WeightUQ);
			if (aPackLine.JL_ActualVolumeUQ == Core.Constants.Volume.CubicMetres)
			{
				AssertEquals("Container Volume", aPackLine.JL_ActualVolume, aPivot.CV_Volume);
			}
			else
			{
				var expectedValue = Core.Constants.Volume.Convert(aPackLine.JL_ActualVolume, aPackLine.JL_ActualVolumeUQ, Core.Constants.Volume.CubicMetres);
				expectedValue = Math.Round(expectedValue, 3);
				AssertEquals("Container Volume", expectedValue, aPivot.CV_Volume);
			}
		}

		protected RefContainer Get40FootGPContainer()
		{
			return Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP"));
		}

		#endregion
	}
}
