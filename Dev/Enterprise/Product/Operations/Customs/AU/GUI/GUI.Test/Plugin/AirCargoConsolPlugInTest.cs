using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	class AirCargoConsolPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		[ExpectNoExceptions()]
		public void TestDeleteConsolAndMasterBill()
		{
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				testPlugIn.OnGUIShown();
				consol.Delete();
				Factory.Save();
			}
		}

		public void TestContinueWithSaveWhenNoMasterExists()
		{
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				AssertEquals(ContinueWithSave.Yes, testPlugIn.ShowPreSaveDialogs());
			}
		}

		public void TestWhenNZCusMAWBExist()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "~NZ";
			company.GC_RN_NKCountryCode = "NZ";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "~NZ";
			Factory.Save();
			using (new TemporaryUserContext()
			{ BranchPK = branch.PK.ToGuid() }.Set())
			{
				var factory = new BusinessObjectFactory();
				var mAWB = factory.New<Integration.Customs.NZ.ICusMAWB>();
				mAWB.CM_JK = consol.PK;
				factory.Save();
			}

			var factory2 = new BusinessObjectFactory();
			var consolLoaded = factory2.Load<ForwardingConsol>(consol.PK);
			var loaded = factory2.Load<Business.CusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, consolLoaded.PK));
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consolLoaded))
			{
				AssertEquals(ContinueWithSave.Yes, testPlugIn.ShowPreSaveDialogs());
			}
		}

		public void TestHAWBsAreRegisteredWhenAutoCreated()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "HOUSE1";
			shipment2.JS_HouseBill = "HOUSE2";
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				testPlugIn.OnGUIShown();
				Assert(mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));
			}
		}

		public void TestWhenDetachedCusHAWBExists()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HOUSE1";
			consol.Shipments.Add(shipment); //attached again
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_HAWB = "UNKNOWN";
			hawb.CS_JS = shipment.PK;
			Factory.Save();
			using (var plugIn = GetAirCargoConsolPlugin(consol))
			{
				plugIn.OnGUIShown();
				AssertEquals(1, CusHAWB.Load(Factory, new[] { shipment.PK }).Length);
				plugIn.OnGUIShown();
				AssertEquals("Should not create another one", 1, CusHAWB.Load(Factory, new[] { shipment.PK }, true).Length);
			}
		}

		public void TestCreateCusMAWBForTheFirstTime()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSE1";
			Factory.Save();
			using (var plugIn = new AirCargoConsolPlugInForTest(consol))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
				AssertEquals(1, CusHAWB.Load(Factory, new[] { shipment.PK }).Length);
			}
		}

		public void TestAShipmentWithouCusHAWBIsAddedAndSaved()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			using (var plugIn = new AirCargoConsolPlugInForTest(consol))
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HOUSE1";
				Factory.Save();
				plugIn.OnMenuShown();
				AssertEquals(1, CusHAWB.Load(Factory, new[] { shipment.PK }).Length);
				var factory2 = new BusinessObjectFactory();
				var consolLoaded = factory2.Load<ForwardingConsol>(consol.PK);
				using (var plugIn2 = new AirCargoConsolPlugInForTest(consolLoaded))
				{
					plugIn2.OnMenuShown();
					Assert("the first plugin obtained the mutex and this should not create its own CusHAWB", !plugIn2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					Factory.Save(); //first factory saves
					plugIn2.OnMenuShown();
					Assert("mutex should have been released", plugIn2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					factory2.Save();
					AssertEquals(1, CusHAWB.Load(factory2, new[] { shipment.PK }).Length);
				}
			}
		}

		public void TestAutoCreationOfHAWBsUseFetchHint()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			for (int i = 0; i < 10; i++)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_HouseBill = "HOUSE" + i.ToString();
			}

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "UNKNOWN";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			using (newFactory.EnableFetchHintsProcessingWithoutTableHitCounter())
			{
				var consolInOtherFactory = newFactory.Load<ForwardingConsol>(consol.PK);
				using (var plugIn = GetAirCargoConsolPlugin(consolInOtherFactory))
				{
					plugIn.OnMenuShown();
					AssertEquals("Should be one", 1, newFactory.GetTableHitCount(JobShipmentSchema.Constants.TableName));
					AssertEquals("CusMAWB Hit Count", 1, newFactory.GetTableHitCount(CusMAWBSchema.Constants.TableName));
					AssertEquals("CusHAWB Hit Count", 3, newFactory.GetTableHitCount(CusHAWBSchema.Constants.TableName));
					AssertEquals(10, consolInOtherFactory.Shipments.Count);
					AssertEquals(11, plugIn.MasterBill.ChildBills.Count);
					newFactory.Save();
				}
			}

			newFactory = new BusinessObjectFactory();
			using (newFactory.EnableFetchHintsProcessingWithoutTableHitCounter())
			{
				var consolInOtherFactory = newFactory.Load<ForwardingConsol>(consol.PK);
				using (var plugIn = GetAirCargoConsolPlugin(consolInOtherFactory))
				{
					plugIn.OnMenuShown();
					AssertEquals("Should be one", 1, newFactory.GetTableHitCount(JobShipmentSchema.Constants.TableName));
					AssertEquals("CusMAWB Hit Count", 1, newFactory.GetTableHitCount(CusMAWBSchema.Constants.TableName));
					AssertEquals("CusHAWB Hit Count: Once CusHAWBs are created, it should not hit more than once", 2, newFactory.GetTableHitCount(CusHAWBSchema.Constants.TableName));
					AssertEquals(10, consolInOtherFactory.Shipments.Count);
					AssertEquals(11, plugIn.MasterBill.ChildBills.Count);
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestGetMasterBillFromConsolWhenStandAloneRecordsThere()
		{
			CusMAWB masterBill = Factory.New<CusMAWB>();
			masterBill.CM_JK = consol.PK;
			CusMAWB standAloneMAWB1 = Factory.New<CusMAWB>();
			CusMAWB standAloneMAWB2 = Factory.New<CusMAWB>(); //CM_JK is not unique any more
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				testPlugIn.OnGUIShown();
			}
		}

		public void TestGetterMasterBillWithLazyCreatedMasterBill()
		{
			consol.Shipments.Load();
			coLoadShipment.Consols.Load();
			using (ZForm form = new ZForm())
			{
				using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
				{
					form.Controls.Add(testPlugIn.UserControl);
					using (AirCargoShipmentPlugIn shipmentPlugIn = new AirCargoShipmentPlugIn(coLoadShipment))
					{
						form.Controls.Add(shipmentPlugIn.UserControl);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						shipmentPlugIn.OnGUIShown();
						AssertNotNull("Master Bill created", shipmentPlugIn.MasterBill);
						AssertNotNull("Master bill loaded for consol plugin", testPlugIn.MasterBill);
					}
				}
			}
		}

		public void TestHAWBsGetAutoLoaded()
		{
			consol.Shipments.Load();
			CusMAWB masterBill = CusMAWB.CreateNew(consol);
			using (ZForm form = new ZForm())
			{
				using (AirCargoConsolPlugIn testPlugin = GetAirCargoConsolPlugin(consol))
				{
					testPlugin.OnMenuShown();
					AssertEquals(2, masterBill.ChildBills.Count);
				}
			}
		}

		public void TestLoadZPlugInDoesntCreateBOForThisPlugIn()
		{
			using (ZForm dummyForm = new ZForm(consol))
			{
				dummyForm.PlugIns.Add(ControllerIDs.Customs.AU.AirCargo);
				dummyForm.Show();
				AirCargoConsolPlugIn testPlugIn = dummyForm.PlugIns.Instances[0] as AirCargoConsolPlugIn;
				testPlugIn.OnGUIShown();
				AssertNull("Master bill is created when a specific menu is clicked", testPlugIn.BusinessEntity);
			}
		}

		public void TestShowPreSaveDialogsCoreChecksArePerformed()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			CommonShipment masterShipment = consol.Shipments.AddNew();
			CusMAWB masterBill = CusMAWB.CreateNew(consol);
			consol.JK_RL_NKLoadPort = "USLAX";
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consignee.OH_IsConsignee = true;
			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consignee.PK));
			consignor.OH_IsConsignor = true;
			consignor.Addresses[0].OA_RL_NKRelatedPortCode = "UAIEV";
			Transport transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			transport.JW_VoyageFlight = "QF123";
			consol.JK_MasterBillNum = "08142938273";
			masterShipment.JS_RL_NKOrigin = "USLAX";
			masterShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			masterShipment.ConsigneeDeliveryAddress.OrganisationPK = consignee.PK;
			masterShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			masterShipment.ConsignorPickupAddress.OrganisationPK = consignor.PK;
			CusHAWB hawb = CusHAWB.CreateNew(masterBill, (ForwardingShipment)masterShipment);
			hawb.CS_HAWB = "HAWB1LOL";
			hawb.CS_GoodsDescription = "Cuckoo Squeakers";
			hawb.CS_FreightPrepaidCollect = "PO";
			hawb.CS_PiecesManifested = 6;
			Factory.Save();
			CusHAWBAIRCRMessageManager manager = new CusHAWBAIRCRMessageManager(hawb);
			manager.GenerateOriginalMessages(hawb);
			AssertEquals(1, hawb.Messages.Count);
			hawb.CS_GoodsDescription = "Need more cuckoo squeakers";
			using (Freight.Forwarding.GUI.ConsolForm form = new Freight.Forwarding.GUI.ConsolForm(consol))
			{
				form.Show();
				hawb.CS_GoodsDescription = "More cuckoo squeakers";
				form.FireSaveButton();
				var errorNotificationToUsers = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("There are messages waiting for responses", errorNotificationToUsers);
				AssertContains("The changes cannot be saved.", errorNotificationToUsers);
			}
		}

		public void TestDetachShipmentFromConsolWithUnderbondInProgressDoesNotThrowException()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			CommonShipment masterShipment = consol.Shipments.AddNew();
			CusMAWB masterBill = CusMAWB.CreateNew(consol);
			consol.JK_RL_NKLoadPort = "USLAX";
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consignee.PK));
			Transport transport = consol.Transports[0];
			transport.JW_ETA = new ZDateTime(2006, 03, 30);
			transport.JW_VoyageFlight = "QF123";
			consol.JK_MasterBillNum = "08142938273";
			masterShipment.JS_RL_NKOrigin = "USLAX";
			masterShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			masterShipment.ConsigneeDeliveryAddress.OrganisationPK = consignee.PK;
			masterShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			masterShipment.ConsignorPickupAddress.OrganisationPK = consignor.PK;
			CusHAWB hawb = CusHAWB.CreateNew(masterBill, (ForwardingShipment)masterShipment);
			hawb.CS_HAWB = "HAWB1LOL";
			hawb.CS_GoodsDescription = "Cuckoo Squeakers";
			hawb.CS_FreightPrepaidCollect = "PO";
			hawb.CS_PiecesManifested = 6;
			CusUnderbond underbond = hawb.Underbonds.AddNew();
			underbond.C4_ParentID = hawb.PK;
			underbond.C4_ParentTableCode = hawb.TablePrefix;
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			underbond.C4_ModeOfMovement = Core.Constants.TransportModes.Road;
			underbond.C4_IsMoveFromDischarge = true;
			underbond.C4_SendersMessageReference = "U00001062";
			Factory.Save();
			AssertEquals("NOT", underbond.underbondStatus.Code);
			CMRUBMREQMessage underbondMessage = Factory.New<CMRUBMREQMessage>();
			underbond.Messages.Add(underbondMessage);
			underbondMessage.EM_MessageType = CMRMessage.CMRMessageTypes.UBMREQ;
			underbondMessage.EM_ReceiveTransmit = "TRX";
			underbondMessage.EM_Status = "SNT";
			underbondMessage.EM_MessageText = Messaging.Business.EDIMessage.MessageNumberPlaceHolder;
			CMRUBMREQEMessage uBMREQE = Factory.New<CMRUBMREQEMessage>();
			uBMREQE.EM_MessageText = @"UNH+000008+CUSRES:D:99B:UN'
BGM+961:::UBMREQE+4C9H 5B0J GC06:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:UBMREQ'
RFF+AFM:9'
RFF+ABO:U00001062/CMT1::001'
DTM+310:20060323061704:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000008'".Replace("\r\n", "");
			uBMREQE.SetEM_LinkedObject();
			Factory.Save();
			underbond.underbondStatus.Code = "CLR";
			AssertEquals("CLR", underbond.underbondStatus.Code);
			using (Freight.Forwarding.GUI.Testing.ConsolShipmentModuleButtonGridTest.MockConsolForm form = new Freight.Forwarding.GUI.Testing.ConsolShipmentModuleButtonGridTest.MockConsolForm(consol))
			{
				form.Show();
				// Detach Shipment
				form.InnerGrid.Select(0);
				consol.Shipments.Remove(masterShipment);
				form.FireSaveButton();
			}
		}

		public void TestDetachShipmentFromConsolWithUnderbondOnMAWBLevelInProgressDoesNotThrowException()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			CommonShipment masterShipment = consol.Shipments.AddNew();
			CusMAWB masterBill = CusMAWB.CreateNew(consol);
			consol.JK_RL_NKLoadPort = "USLAX";
			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, consignee.PK));
			Transport transport = consol.Transports[0];
			transport.JW_ETA = new ZDateTime(2006, 03, 30);
			transport.JW_VoyageFlight = "QF123";
			consol.JK_MasterBillNum = "08142938273";
			masterShipment.JS_RL_NKOrigin = "USLAX";
			masterShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			masterShipment.ConsigneeDeliveryAddress.OrganisationPK = consignee.PK;
			masterShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			masterShipment.ConsignorPickupAddress.OrganisationPK = consignor.PK;
			CusHAWB hawb = CusHAWB.CreateNew(masterBill, (ForwardingShipment)masterShipment);
			hawb.CS_HAWB = "HAWB1LOL";
			hawb.CS_GoodsDescription = "Cuckoo Squeakers";
			hawb.CS_FreightPrepaidCollect = "PO";
			hawb.CS_PiecesManifested = 6;
			CusUnderbond underbond = masterBill.Underbonds.AddNew();
			underbond.C4_ParentID = masterBill.PK;
			underbond.C4_ParentTableCode = "CM";
			underbond.C4_OriginPremiseID = "9920A";
			underbond.C4_DestinationPremiseID = "9914N";
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			underbond.C4_ModeOfMovement = Core.Constants.TransportModes.Road;
			underbond.C4_IsMoveFromDischarge = true;
			underbond.C4_SendersMessageReference = "U00001062";
			Factory.Save();
			AssertEquals("NOT", underbond.underbondStatus.Code);
			CMRUBMREQMessage underbondMessage = Factory.New<CMRUBMREQMessage>();
			underbond.Messages.Add(underbondMessage);
			underbondMessage.EM_MessageType = CMRMessage.CMRMessageTypes.UBMREQ;
			underbondMessage.EM_ReceiveTransmit = "TRX";
			underbondMessage.EM_Status = "SNT";
			underbondMessage.EM_MessageText = Messaging.Business.EDIMessage.MessageNumberPlaceHolder;
			CMRUBMREQEMessage uBMREQE = Factory.New<CMRUBMREQEMessage>();
			uBMREQE.EM_MessageText = @"UNH+000008+CUSRES:D:99B:UN'
BGM+961:::UBMREQE+4C9H 5B0J GC06:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:UBMREQ'
RFF+AFM:9'
RFF+ABO:U00001062/CMT1::001'
DTM+310:20060323061704:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000008'".Replace("\r\n", "");
			uBMREQE.SetEM_LinkedObject();
			Factory.Save();
			underbond.underbondStatus.Code = "CLR";
			AssertEquals("CLR", underbond.underbondStatus.Code);
			using (Freight.Forwarding.GUI.Testing.ConsolShipmentModuleButtonGridTest.MockConsolForm form = new Freight.Forwarding.GUI.Testing.ConsolShipmentModuleButtonGridTest.MockConsolForm(consol))
			{
				form.Show();
				// Detach Shipment
				form.InnerGrid.Select(0);
				consol.Shipments.Remove(masterShipment);
				form.FireSaveButton();
			}
		}

		public void TestGetMasterBillFromConsol()
		{
			CusMAWB masterBill = CusMAWB.CreateNew(consol);
			using (AirCargoConsolPlugIn testPlugIn1 = GetAirCargoConsolPlugin(consol))
			{
				CusMAWB masterBill1 = testPlugIn1.BusinessEntity as CusMAWB;
				using (AirCargoConsolPlugIn testPlugIn2 = GetAirCargoConsolPlugin(consol))
				{
					CusMAWB masterBill2 = testPlugIn2.BusinessEntity as CusMAWB;
					Assert("Two MasterBill refers to the same object", masterBill1.PK == masterBill2.PK);
				}
			}
		}

		public void TestMAWBGetsSynchedFromConsol()
		{
			CusMAWB masterBill = CusMAWB.CreateNew(consol);
			consol.JK_RL_NKDischargePort = "AUFRE";
			masterBill.CM_RL_NKDischargePort = "AUBNE";
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				CusMAWB masterBill1 = testPlugIn.BusinessEntity as CusMAWB;
				Transport transport = consol.Transports[0];
				transport.JW_RL_NKDiscPort = "AUSYD";
				AssertEquals("AUSYD", masterBill.CM_RL_NKDischargePort);
				transport.JW_RL_NKDiscPort = "AUMEL";
				AssertEquals("AUMEL", masterBill.CM_RL_NKDischargePort);
				transport.JW_RL_NKLoadPort = "USLAX";
				AssertEquals("USLAX", masterBill.CM_RL_NKLoadPort);
				transport.JW_ETA = new ZDateTime(2005, 12, 12);
				AssertEquals(new ZDateTime(2005, 12, 12), masterBill.CM_ArrivalDate);
				consol.JK_MasterBillNum = "08198726378";
				AssertEquals("08198726378", masterBill.CM_MAWB);
			}
		}

		public void TestAirCargoConsolPlugInVisibility()
		{
			using (ZForm dummyForm = new ZForm(consol))
			{
				dummyForm.PlugIns.Add(ControllerIDs.Customs.AU.AirCargo);
				AirCargoConsolPlugIn testPlugIn = dummyForm.PlugIns.Instances[0] as AirCargoConsolPlugIn;
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("AirCargo Menu Visible", true, testPlugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("AirCargo Menu Visible", false, testPlugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("AirCargo Menu Visible", true, testPlugIn.Enabled);
				consol.JK_RL_NKDischargePort = "NZAKL"; //Changed to NZ one
				AssertEquals("AirCargo Menu Visible", false, testPlugIn.Enabled);
				consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("AirCargo Menu Visible", true, testPlugIn.Enabled);
			}
		}

		public void TestDontSynchroniseIfPlugInDisabled()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKDischargePort = "AUSYD";
				mAWB.CM_RL_NKDischargePort = "";
				AssertEquals("AirCargo Consol PlugIn Visible", true, testPlugIn.Enabled);
				testPlugIn.OnSaving();
				AssertNotNull("AirCargo job retrieved", testPlugIn.MasterBill);
				AssertEquals("Port of Discharge synchronised", "AUSYD", testPlugIn.MasterBill.CM_RL_NKDischargePort);
				AssertEquals("Port of load empty yet", "", testPlugIn.MasterBill.CM_RL_NKLoadPort);
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "NZAKL";
				AssertEquals("PlugIn invisible and disabled", false, testPlugIn.Enabled);
				testPlugIn.OnSaving();
				AssertEquals("Disabled -> Port of load not synchronised", "", testPlugIn.MasterBill.CM_RL_NKLoadPort);
			}
		}

		public void TestMessagingNotAllowedWhenAnAirShipmentIsOpenForEdit()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingShipment shipment = consol.Shipments.AddNew();
			CusMAWB mAWB = CusMAWB.CreateNew(consol);
			mAWB.CM_JK = consol.PK;
			mAWB.CM_FlightNo = "QF1";
			mAWB.CM_ArrivalDate = ZDateTime.Today;
			mAWB.CM_MAWB = "081-11111111";
			mAWB.CM_RL_NKDischargePort = "AUSYD";
			mAWB.CM_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKLoadPort = "USLAX";
			CusHAWB house = mAWB.ChildBills.AddNew();
			house.CS_Weight = 56.4m;
			house.CS_WeightUQ = "KG";
			house.CS_HAWB = "30000000000";
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.FillWithValidTestData();
			consignor.OH_FullName = "DONNELLY MIRRORS";
			consignor.MainAddress.OA_Address1 = "1079 FARRINGTON DR";
			consignor.MainAddress.OA_City = "CHICAGO";
			consignor.MainAddress.OA_State = "IL";
			consignor.MainAddress.OA_PostCode = "60646";
			consignor.OH_RL_NKClosestPort = "USLAX";
			house.CS_OH_Consignor = consignor.PK;
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_FullName = "RAINSFORD PTY LTD";
			consignee.MainAddress.OA_Address1 = "450 EUSTON RD";
			consignee.MainAddress.OA_City = "BROOKVALE";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_PostCode = "2100";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			house.CS_OH_Consignee = consignee.PK;
			house.CS_RL_NKDestination = "AUSYD";
			house.CS_RL_NKOrigin = "GBLHR";
			house.CS_FreightPrepaidCollect = "PO";
			house.CS_GoodsDescription = "10 AUTOMOTIVE MIRRORS CHROME COATED";
			house.CS_PiecesManifested = 10;
			house.CS_GoodsValue = 745m;
			house.CS_RX_NKGoodsCurrency = "USD";
			house.CS_JS = shipment.PK;
			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ConsolFormHelper form = new ConsolFormHelper(consol))
			{
				form.Show();
				MenuItem menuItem = form.Menu.MenuItems.FindByText("Air Cargo", false);
				AssertNotNull(menuItem);
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, menuItem, new object[] { EventArgs.Empty });
				menuItem = menuItem.MenuItems.FindByText("Send Message(s)", true);
				AssertNotNull(menuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals(SeaCargo.GUI.SeaCargoConsolMenu.ReasonMessagingIsSuppressedText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestManager()
		{
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				AssertEquals("ManagerType", typeof(CusMAWBMessageManager), testPlugIn.Manager.GetType());
			}
		}

		public void TestManagerGetsMAWB()
		{
			using (AirCargoConsolPlugIn testPlugIn = GetAirCargoConsolPlugin(consol))
			{
				AssertEquals("MAWB", null, testPlugIn.Manager.MAWB);
				CusMAWB masterBill = Factory.New<CusMAWB>();
				AssertEquals("MAWB", testPlugIn.MasterBill, testPlugIn.Manager.MAWB);
			}
		}

		public void TestHasUserControlWhenCMRMAWBExists()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			using (var testPlugIn = new AirCargoConsolPlugIn(consol))
			{
				AssertNotNull("HasUserControl", testPlugIn.UserControl);
			}
		}

		[TestDate(2010, 6, 1)]
		public void TestHasUserControlWhereNoMAWBExistsAndAfterCMR()
		{
			using (var testPlugIn = new AirCargoConsolPlugIn(consol))
			{
				AssertNotNull("HasUserControl", testPlugIn.UserControl);
			}
		}

		public void TestQueryUserToCreateAirCargoJob()
		{
			using (var form = new ZForm())
			using (var testPlugIn = new AirCargoConsolPlugInForTest(consol))
			{
				form.Controls.Add(testPlugIn.UserControl);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
				AssertEquals(AirCargoConsolPlugIn.YouHaveChosenNotToCreateAnAirCargo, testPlugIn.PlugInNotDisplayedMessage);
				AssertNull("Refused to create an air cargo.", testPlugIn.MasterBill);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
				AssertNotNull("Accepted to create an air cargo.", testPlugIn.MasterBill);
				Assert("Saved to database.", testPlugIn.MasterBill.IsInDatabase);
			}
		}

		public void TestQueryUserToCreateAirCargoJob_Legacy()
		{
			var mawb = Factory.New<Business.CusMAWB>();
			mawb.CM_ApplicationCode = "";
			mawb.CM_JK = consol.PK;

			using (var testForm = new ZForm())
			using (var testPlugIn = new AirCargoConsolPlugInForTest(consol))
			{
				testForm.Controls.Add(testPlugIn.UserControl);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
				AssertEquals("Air Cargo Data is Invalid.", testPlugIn.PlugInNotDisplayedMessage);
				AssertNull("Refused to create an air cargo.", testPlugIn.MasterBill);
			}
		}

		public void TestConcurrency()
		{
			using (var form = new ZForm())
			using (var testPlugin = new AirCargoConsolPlugInForTest(consol))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.Controls.Add(testPlugin.UserControl);
				testPlugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
				using (var form2 = new ZForm())
				using (var testPlugin2 = new AirCargoConsolPlugInForTest(new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK)))
				{
					form2.Controls.Add(testPlugin2.UserControl);
					testPlugin2.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
					AssertEquals(AirCargoConsolPlugIn.SomeoneIsEditingOneOfTheShipments, testPlugin2.PlugInNotDisplayedMessage);
					AssertEquals("Should only have created 1 CusMAWB", 1, Factory.Load<CusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, consol.PK)).Length);
				}
			}
		}

		public void TestAskUserToSaveBeforeCreateAirCargoJob()
		{
			using (var form = new ZForm())
			using (var testPlugIn = new AirCargoConsolPlugInForTest(consol))
			{
				form.Controls.Add(testPlugIn.UserControl);
				consol.HasChanges = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
				AssertEquals(AirCargoConsolPlugIn.YouHaveToSaveBeforeCreateAnAirCargo, testPlugIn.PlugInNotDisplayedMessage);
				AssertNull("Accepted to create an air cargo, but refused to save the consol.", testPlugIn.MasterBill);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
				AssertNotNull("Accepted to create an air cargo and save the consol.", testPlugIn.MasterBill);
				Assert("Saved to database.", testPlugIn.MasterBill.IsInDatabase);
			}
		}

		public void TestCCRProcessorCreatesAirCargoFirst()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			consol.Shipments.AddNew().JS_HouseBill = "HB001";
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = forwarder.PK;
			company.Branches.AddNew();
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var consolInFactory2 = factory2.Load<ForwardingConsol>(consol.PK);
			var notifications = new NotificationBuffer();
			var ccrProcessor = new Business.CCRProcessor(consolInFactory2, Core.Constants.CountryCodes.Australia);
			ccrProcessor.Process(notifications, new System.Threading.CancellationToken());
			using (var plugin = new AirCargoConsolPlugInForTest(consol))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
			}

			Assert("Workflow has MAWB", consolInFactory2.AUCusMAWB != null);
			Assert("Plugin has MAWB", consol.AUCusMAWB != null);
			AssertEquals("Plugin has the same MAWB as the workflow", consolInFactory2.AUCusMAWB.PK, consol.AUCusMAWB.PK);
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var consol = Factory.New<ForwardingConsol>();
			var mAWB = Factory.New<CusMAWB>();
			mAWB.CM_JK = consol.PK;
			mAWB.ChildBills.AddNew();
			return new AirCargoConsolPlugIn(consol);
		}

		protected virtual AirCargoConsolPlugIn GetAirCargoConsolPlugin(ForwardingConsol consol) => new AirCargoConsolPlugIn(consol);

		protected ForwardingConsol consol;
		protected CommonShipment masterShipment;
		protected CommonShipment coLoadShipment;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "AUSYD";
			coLoadShipment = consol.Shipments.AddNew();
			masterShipment = consol.Shipments.AddNew();
			coLoadShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			masterShipment.CoLoadShipments.Add(coLoadShipment);
			//Consol Collection in Shipment needs this to get loaded correctly(to create a pivot object)
			Factory.Save();
		}

		sealed class ConsolFormHelper : Freight.Forwarding.GUI.ConsolForm
		{
			public ConsolFormHelper(ForwardingConsol businessEntity) : base(businessEntity)
			{
			}

			public override bool IsAnyShipmentOpenForEdit
			{
				get
				{
					return true;
				}
			}
		}

		sealed class AirCargoConsolPlugInForTest : AirCargoConsolPlugIn
		{
			public AirCargoConsolPlugInForTest(ForwardingConsol hostBusinessEntity)
				: base(hostBusinessEntity)
			{
			}

			internal bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
