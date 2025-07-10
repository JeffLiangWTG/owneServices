using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoShipmentPlugInTest : SeaCargoPlugInTest
	{
		public class SeaCargoShipmentPlugInTestCase : BaseSeaCargoPlugInTest
		{
			protected const string TestLloydsNumber = "1234567";
			protected const string TestVesselName = "SCOTTSFLOATINGBROTHEL";
			protected const string TestOceanBill = "OCEANTEST123";
			protected const string TestPortOfLoading = "HKHKG";
			protected const string TestPortOfDischarge = "AUSYD";
			protected const string TestVoyageNumber = "23";
			protected const string TestPrincipalID = "C067764014";
			protected const string TestHouseBillNumber = "TESTHOUSE123";
			protected ForwardingShipment shipment;
			protected ForwardingConsol consol;

			public void TestOnSaveCompletedOrAborted()
			{
				var oceanBill = Factory.New<CusSCAOceanBill>();
				oceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				var house = oceanBill.HouseBills.AddNew();
				house.CA_JS = shipment.PK;
				var message = house.Messages.AddNew();
				message.EM_ReceiveTransmit = "TRX";
				using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
				{
					testPlugIn.OnSaveCompletedOrAborted(true);
					Assert(!message.IsDeleted);
					testPlugIn.OnSaveCompletedOrAborted(false);
					Assert(message.IsDeleted);
				}
			}

			public void TestSeaCargoOceanBillDefaults()
			{
				using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertEquals("Should Create the OceanBill", true, testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					AssertEquals("Oceanbill Number matches MasterBill Number", TestOceanBill, testPlugIn.HouseBill.OceanBill.CB_OceanBill);
					AssertEquals("Oceanbill Port of Loading matches Consol Port of Loading", TestPortOfLoading, testPlugIn.HouseBill.OceanBill.CB_RL_NKPortOfLoading);
					AssertEquals("Oceanbill Port of Discharge matches Consol Port of Discharge", TestPortOfDischarge, testPlugIn.HouseBill.OceanBill.CB_RL_NKPortOfDischarge);
					AssertEquals("Oceanbill LLoyds matches Consol Lloyds", TestVesselName, testPlugIn.HouseBill.OceanBill.CB_VesselName);
					AssertEquals("Oceanbill Voayge Number matches Consol Voyage Number", TestVoyageNumber, testPlugIn.HouseBill.OceanBill.CB_Voyage);
					AssertEquals("Oceanbill Principal ID matches Consol ShipLine Principal ID", TestPrincipalID, testPlugIn.HouseBill.OceanBill.CB_PrincipalID);
				}
			}

			public void TestManager()
			{
				using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal();
					AssertEquals("ManagerType", typeof(CusSCAHouseMessageManager), testPlugIn.Manager.GetType());
					AssertNotNull(testPlugIn.Manager.Factory);
				}
			}

			public void TestSeaPlugInNotVisibleIfUntilImportConsolExists()
			{
				using (var dummyForm = new ZForm(consol))
				{
					dummyForm.PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
					var testPlugIn = dummyForm.PlugIns.Instances[0] as SeaCargoConsolPlugIn;

					consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

					consol.JK_RL_NKDischargePort = "USLAX";
					AssertEquals("User Control Visibility", false, testPlugIn.Enabled);

					consol.JK_RL_NKDischargePort = TestPortOfDischarge;
					AssertEquals("User Control Visibility", true, testPlugIn.Enabled);
				}
			}

			public void TestPlugAlwaysReturnAnObject()
			{
				using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertEquals("Should Create the OceanBill", true, testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					AssertNotNull("We always need to return a business Object", testPlugIn.BusinessEntity);
				}
			}

			public void TestSeaCargoShipmentPlugInVisibility()
			{
				consol.Delete(); // delete this phantom consol
				using (var dummyForm = new ZForm(shipment))
				{
					dummyForm.PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
					var testPlugIn = dummyForm.PlugIns.Instances[0] as SeaCargoShipmentPlugIn;

					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					var aUConsol = shipment.Consols.AddNew();
					aUConsol.JK_RL_NKDischargePort = "AUSYD";
					AssertEquals("SeaCargo Tab Visible", true, testPlugIn.Enabled);

					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					AssertEquals("SeaCargo Tab Visible", false, testPlugIn.Enabled);

					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					AssertEquals("SeaCargo Tab Visible", true, testPlugIn.Enabled);

					aUConsol.JK_RL_NKDischargePort = "NZAKL";//Changed to NZ one
					AssertEquals("SeaCargo Tab Visible", false, testPlugIn.Enabled);

					aUConsol.JK_RL_NKDischargePort = "AUSYD";
					AssertEquals("SeaCargo Tab Visible", true, testPlugIn.Enabled);

					shipment.Consols.Remove(aUConsol);
					AssertEquals("SeaCargo Tab Visible", false, testPlugIn.Enabled);
				}
			}

			public void TestSettingTheHouseBillSetsTheManagersHouseBill()
			{
				var consol1 = Factory.New<ForwardingConsol>();
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";

				var transport = consol1.Transports[0];
				transport.JW_RL_NKDiscPort = "AUSYD";
				transport.JW_RL_NKLoadPort = "USLAX";

				var shipment1 = consol1.Shipments.AddNew();
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment1.JS_RL_NKDestination = "AUSYD";

				Factory.Save();
				shipment1.Consols.Load();

				using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment1))
				{
					AssertNull("HouseBill should be Null", testPlugIn.Manager.HouseBill);
					var oceanBill = Factory.New<CusSCAOceanBill>();
					testPlugIn.HouseBill = oceanBill.HouseBills.AddNew();
					AssertEquals("HouseBill", oceanBill.HouseBills[0], testPlugIn.Manager.HouseBill);
				}
			}

			public void TestSynchronisationWorksIfWeStartOffWithNoHouseBill()
			{
				var shipment = CreateShipmentToSynchronise();
				using (var plugin = new SeaCargoShipmentPlugIn(shipment))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertEquals("Should Create the OceanBill", true, plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					AssertNotNull("PreCondition", plugin.HouseBill);
					shipment.JS_HouseBill = "12345";
					AssertEquals("CA_HouseBill", "12345", plugin.HouseBill.CA_HouseBill);
				}
			}

			public void TestSynchronisationOfJustThisShipment()
			{
				var shipment = CreateShipmentToSynchronise();
				var consol = shipment.ArrivalConsol;

				var oceanBill = Factory.New<CusSCAOceanBill>();
				oceanBill.CB_ParentId = consol.PK;
				oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
				var otherShipment = consol.Shipments.AddNew();

				using (var plugin = new SeaCargoShipmentPlugIn(shipment))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertEquals("Should Create the OceanBill", true, plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					AssertNotNull("PreCondition", plugin.HouseBill);
					shipment.JS_HouseBill = "12345";
					AssertEquals("CA_HouseBill", "12345", plugin.HouseBill.CA_HouseBill);

					AssertNull("Synchronising one shipment does not affect other shipments", otherShipment.AUCusSCAHouse);
				}
			}

			public void TestStartSynchronisationWhenHaveOceanBill()
			{
				consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = TestPortOfLoading;
				consol.JK_RL_NKDischargePort = TestPortOfDischarge;
				consol.JK_MasterBillNum = TestOceanBill;
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

				var transport = consol.Transports[0];
				transport.JW_Vessel = TestVesselName;
				transport.JW_VoyageFlight = "23";

				var seaCargoSynchroniser = new CMRSeaCargoSynchroniser(consol);
				_ = seaCargoSynchroniser.OceanBill;
				Factory.Save();

				shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_HouseBill = TestHouseBillNumber;
				shipment.JS_RL_NKOrigin = TestPortOfLoading;
				shipment.JS_RL_NKDestination = TestPortOfDischarge;
				shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

				var consignee = Factory.New<OrgHeader>();
				consignee.OH_FullName = "TEST Consignee";
				consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
				consignee.OH_RL_NKClosestPort = TestPortOfDischarge;
				consignee.MiscServ.OM_IMDefaultINCOTerm = Core.Constants.IncoTerms.CostAndFreight;
				shipment.ConsigneePK = consignee.PK;

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "TEST Consignor";
				consignor.OH_RL_NKClosestPort = TestPortOfLoading;
				consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
				shipment.ConsignorPK = consignor.PK;

				using (var plugin = new SeaCargoShipmentPlugIn(shipment))
				{
					var oceanBill = plugin.OceanBill;
					AssertEquals("OceanBill has 1 house bill", 1, oceanBill.HouseBills.Count);
					var houseBill = oceanBill.HouseBills[0];
					AssertEquals("The house bill was synchronised from shipment", shipment.PK, houseBill.CA_JS);
				}

				var consol2 = Factory.New<ForwardingConsol>();
				consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol2.JK_RL_NKLoadPort = TestPortOfLoading;
				consol2.JK_RL_NKDischargePort = TestPortOfDischarge;
				consol2.JK_MasterBillNum = TestOceanBill;
				consol2.JK_AgentType = Core.Constants.AgentType.CoLoad;

				var transport2 = consol2.Transports[0];
				transport2.JW_Vessel = TestVesselName;
				transport2.JW_VoyageFlight = "23";

				Factory.Save();

				var shipment2 = consol2.Shipments.AddNew();
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment2.JS_HouseBill = TestHouseBillNumber;
				shipment2.JS_RL_NKOrigin = TestPortOfLoading;
				shipment2.JS_RL_NKDestination = TestPortOfDischarge;
				shipment2.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);
				shipment2.ConsigneePK = consignee.PK;
				shipment2.ConsignorPK = consignor.PK;

				using (var plugin = new SeaCargoShipmentPlugIn(shipment2))
				{
					var oceanBill = plugin.OceanBill;
					AssertNull("No sychronization when oceanBill was not created", oceanBill);
				}
			}

			#region Implementation

			protected override void SetUp()
			{
				base.SetUp();
				CreateTestVessel(TestVesselName);
				CreateConsolAndShipment();
			}

			protected void CreateConsolAndShipment()
			{
				consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = TestPortOfLoading;
				consol.JK_RL_NKDischargePort = TestPortOfDischarge;
				consol.JK_MasterBillNum = TestOceanBill;
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

				var transport = consol.Transports[0];
				transport.JW_Vessel = TestVesselName;
				transport.JW_VoyageFlight = "23";

				shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_HouseBill = TestHouseBillNumber;
				shipment.JS_RL_NKOrigin = TestPortOfLoading;
				shipment.JS_RL_NKDestination = TestPortOfDischarge;
				shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);

				var consignee = Factory.New<OrgHeader>();
				consignee.OH_FullName = "TEST Consignee";
				consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
				consignee.OH_RL_NKClosestPort = TestPortOfDischarge;
				consignee.MiscServ.OM_IMDefaultINCOTerm = Core.Constants.IncoTerms.CostAndFreight;
				shipment.ConsigneePK = consignee.PK;

				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "TEST Consignor";
				consignor.OH_RL_NKClosestPort = TestPortOfLoading;
				consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
				shipment.ConsignorPK = consignor.PK;
				Factory.Save();
			}

			protected void CreateTestVessel(string vesselName)
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

			#endregion
		}

		public void TestCCRProcessorCannotObtainMutex()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB001";
			consol.Shipments.AddNew().JS_HouseBill = "HB002";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = forwarder.PK;
			company.Branches.AddNew();
			Factory.Save();
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var plugIn = new SeaCargoShipmentPlugInForTest(shipment))
			{
				var mutex = plugIn.GetMutex();
				try
				{
					mutex.Lock();
					var notifications = new NotificationBuffer();
					new CCRProcessor(consol, Core.Constants.CountryCodes.Australia).Process(notifications, new System.Threading.CancellationToken());
					AssertEquals(string.Format("A Customs master bill record cannot be created or updated as someone else is trying to create or update a master bill for this consol {0}.", consol.HumanReadableName), notifications.AsString.Trim());
				}
				finally
				{
					mutex.Unlock();
				}
			}
		}

		public void TestWhenNoArrivalConsol()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var consol = CreateFCLConsol();
			consol.JK_RL_NKDischargePort = "NZAKL";
			var shipment = consol.Shipments.AddNew();
			using (var plugIn = GetPlugIn(shipment))
			{
				Assert("No exception should be thrown and no business entity created", !QueryUserShouldPlugInGUIAndBusinessEntityBeCreated(plugIn));
			}
		}

		public void TestCanDelete()
		{
			using (var testPlugin = new SeaCargoShipmentPlugIn(shipment))
			{
				Assert(testPlugin.CanDelete);
			}
		}

		public void TestLoadZPlugIn()
		{
			using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPlugIn.OnUserControlShown();
				Assert("No Error Message when no HouseBill on Shipment", !UnitTestUserNotification.Instance.LastMessage.WasError);
				shipment.JS_HouseBill = "TESTHOUSE123";
				testPlugIn.OnUserControlShown();
				AssertNotNull(testPlugIn.UserControl);
			}
		}

		public void TestPlugInVisibility()
		{
			using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
			{
				AssertEquals("Plug In Enabled", true, testPlugIn.Enabled);
				shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				AssertEquals("Plug In Enabled", true, testPlugIn.Enabled);
				shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
				AssertEquals("Plug In Enabled", true, testPlugIn.Enabled);
			}
		}

		[ExpectNoExceptions()]
		public void TestTranshipmentInSeaCargo()
		{
			using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
			{
				shipment.JS_RL_NKDestination = "NZAKL";
				testPlugIn.OnGUIShown();
			}
		}

		public void TestShowPreSaveDialogs_CMR()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			house.CA_JS = shipment.PK;
			using (var testPlugIn = new SeaCargoShipmentPlugInForTest(shipment))
			{
				testPlugIn.ShowPreSaveDialogs();
				AssertEquals("CMRPreSaveCalled", true, testPlugIn.ShowPreSaveDialogs_CMRCalled);
			}
		}

		public void TestNewShipmentSynchronisation()
		{
			var shipment = CreateShipmentToSynchronise();
			synchroniser = GetSeaCargoSynchroniser(shipment.ArrivalConsol);
			var house = synchroniser.GetHouseBill(shipment);
			TestSynchronisation(true, shipment, house);
		}

		public void TestSynchronisationWorksIfWeStartOffWithAHouseBill()
		{
			var shipment = CreateShipmentToSynchronise();
			var ocean = CreateOceanBill();
			ocean.CB_ParentId = shipment.ArrivalConsol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = CreateHouseBill(ocean, "54321");
			house.CA_JS = shipment.PK;
			using (var plugin = new SeaCargoShipmentPlugIn(shipment))
			{
				plugin.OnGUIShown();
				AssertNotNull("PreCondition", plugin.HouseBill);
				shipment.JS_HouseBill = "12345";
				AssertEquals("CA_HouseBill", "12345", plugin.HouseBill.CA_HouseBill);
			}
		}

		[ExpectNoExceptions]
		public void TestDeletingHouseOnConsolDoesNotCrashPlugin_NoNewHouseBill()
		{
			var shipment = CreateShipmentToSynchronise();
			var ocean = CreateOceanBill();
			ocean.CB_ParentId = shipment.ArrivalConsol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = CreateHouseBill(ocean, "54321");
			house.CA_JS = shipment.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);
			using (var form = new ZForm(shipmentInFactory2))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
				form.Show();
				house.Delete();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Should show cover label when user chooses no on the prompt to create a new when the existing one is deleted", true, form.FindSingle<ZTabPage>(ctrl => ctrl.Text == "Sea Cargo").FindAll<ZLabel>(ctrl => ctrl.Visible && ctrl.Text.Contains("You have chosen not to create a sea cargo job now")).Any());
					var houseBills = (factory2 as IBusinessObjectFactoryInternals).AllBusinessObjects.Where(bo => bo is CusSCAHouse);
					AssertEquals("No new BO created silently after deletion", false, houseBills.Any(bo => !bo.IsDeleted && bo.HasChanges));
					AssertEquals("No change pending", false, shipmentInFactory2.HasChanges);
					form.FireSaveButton();
					ocean.Reload();
					AssertEquals("No house bill", 0, ocean.HouseBills.Count);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestDeletingHouseOnConsolDoesNotCrashPlugin_NewHouseBill()
		{
			var shipment = CreateShipmentToSynchronise();
			var ocean = CreateOceanBill();
			ocean.CB_ParentId = shipment.ArrivalConsol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = CreateHouseBill(ocean, "54321");
			house.CA_JS = shipment.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);
			using (var form = new ZForm(shipmentInFactory2))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
				form.Show();
				house.Delete();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Factory.Save();
				CombineAssertions(() =>
				{
					AssertEquals("Should not show cover label when user chooses yes on the prompt to create a new when the existing one is deleted", false, form.FindSingle<ZTabPage>(ctrl => ctrl.Text == "Sea Cargo").FindAll<ZLabel>(ctrl => ctrl.Visible && ctrl.Text.Contains("You have chosen not to create a sea cargo job now")).Any());
					var pluginDataBound = form.FindSingle<SeaCargoDeclarationUserControl>().CurrentDataItem as BusinessObject;
					AssertNotEquals("New BO is different from deleted one", house.PK, pluginDataBound.PK);
					form.FindSingle<ZCheckBox>(ctrl => ctrl.Text == "Consolidation (FF Ind)").Checked = true;
					AssertEquals("Change to be saved", true, shipmentInFactory2.HasChanges);
					form.FireSaveButton();
					ocean.Reload();
					AssertEquals("One house bill", 1, ocean.HouseBills.Count);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestDeletingHouseOnConsolDoesNotCrashPlugin_TabSelectionChanges()
		{
			var shipment = CreateShipmentToSynchronise();
			var ocean = CreateOceanBill();
			ocean.CB_ParentId = shipment.ArrivalConsol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = CreateHouseBill(ocean, "54321");
			house.CA_JS = shipment.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);
			using (var form = new ZForm(shipmentInFactory2))
			{
				var tabControl = new ZTabControl();
				form.Controls.Add(tabControl);
				var dummyTab = new TabPage();
				dummyTab.Text = "Dummy";
				tabControl.Controls.Add(dummyTab);
				form.PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
				form.Show();
				tabControl.SelectedIndex = 0;
				house.Delete();
				Factory.Save();
				CombineAssertions(() =>
				{
					// change to plugin tab and do not create new
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					tabControl.SelectedIndex = 1;
					AssertEquals("Should show cover label when user chooses no on the prompt to create a new when the existing one is deleted", true, form.FindSingle<ZTabPage>(ctrl => ctrl.Text == "Sea Cargo").FindAll<ZLabel>(ctrl => ctrl.Visible && ctrl.Text.Contains("You have chosen not to create a sea cargo job now")).Any());
					// change to plugin tab and create new
					tabControl.SelectedIndex = 0;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					tabControl.SelectedIndex = 1;
					AssertEquals("Should not show cover label when user chooses no on the prompt to create a new when the existing one is deleted", false, form.FindSingle<ZTabPage>(ctrl => ctrl.Text == "Sea Cargo").FindAll<ZLabel>(ctrl => ctrl.Visible && ctrl.Text.Contains("You have chosen not to create a sea cargo job now")).Any());
					var pluginDataBound = form.FindSingle<SeaCargoDeclarationUserControl>().CurrentDataItem as BusinessObject;
					AssertNotEquals("New BO is different from deleted one", house.PK, pluginDataBound.PK);
					form.FindSingle<ZCheckBox>(ctrl => ctrl.Text == "Consolidation (FF Ind)").Checked = true;
					AssertEquals("New BO can be edited", true, shipmentInFactory2.HasChanges);
					form.FireSaveButton();
					ocean.Reload();
					AssertEquals("One house bill", 1, ocean.HouseBills.Count);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestDeletingHouseOnConsolDoesNotCrashPlugin_TabPageNotificationExposed()
		{
			var shipment = CreateShipmentToSynchronise();
			var ocean = CreateOceanBill();
			ocean.CB_ParentId = shipment.ArrivalConsol.PK;
			ocean.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var house = CreateHouseBill(ocean, "54321");
			house.CA_JS = shipment.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var shipmentInFactory2 = factory2.Load<ForwardingShipment>(shipment.PK);
			using (var form = new ZForm(shipmentInFactory2))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
				form.Show();
				house.Delete();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();
				CombineAssertions(() =>
				{
					// we only want to test the tab of House Bill. So other tabs are deleted to remove exceptions
					var pluginTab = form.FindSingle<ZTabPage>(tab => tab.Text == "Sea Cargo");
					(pluginTab as ZTabPagePlugIn).ResetBinding();
					shipmentInFactory2.JS_HBLAWBChargesDisplayInfo.AddWarningWithoutValidationCheck("Error to trigger exposer");
					TabPageNotificationsExposer.ExposeTabPageNotifications(form, shipmentInFactory2);
				});
			}
		}

		public void TestSavedShipmentSynchronisationNoMessages()
		{
			var shipment = CreateShipmentToSynchronise();
			synchroniser = GetSeaCargoSynchroniser(shipment.ArrivalConsol);
			var house = synchroniser.GetHouseBill(shipment);
			Factory.Save();
			TestSynchronisation(true, shipment, house);
		}

		public void TestShipmentSynchronisationAcknowledged()
		{
			var shipment = CreateShipmentToSynchronise();
			synchroniser = GetSeaCargoSynchroniser(shipment.ArrivalConsol);
			var house = synchroniser.GetHouseBill(shipment);
			EnsureValidSeaCargoShipment(shipment);
			house.AcceptCurrentAsAcknowledged();
			Factory.Save();
			TestSynchronisation(false, shipment, house);
		}

		public void TestIsShipmentCMRShipment()
		{
			AssertIsShipmentCMRShipment(true);
			AssertIsShipmentCMRShipment(false);
		}

		[ExpectNoExceptions()]
		public void TestIssue49389TopLevelBusinessObjectIsNull()
		{
			var mock = new Mock<ISeaCargoShipmentInfo> { CallBase = true };
			mock.Setup(m => m.IsVisible).Returns(true);
			using (var plugin = new SeaCargoShipmentPlugIn(mock.Object))
			{
				mock.Setup(m => m.HouseBill).Returns((CusSCAHouse)null);
				plugin.HouseBill = null;
				var oceanBill = Factory.New<CusSCAOceanBill>();
				var house = oceanBill.HouseBills.AddNew();
				var container = oceanBill.Containers.AddNew();
				var pivot = container.Pivots.AddNew();
				pivot.CV_CA = house.PK;
				mock.Setup(m => m.HouseBill).Returns(house);
				plugin.ShowPreSaveDialogs();
			}
		}

		public void TestSaveHouseBillAfterCreated()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery()).RV_Code;
			transport.JW_VoyageFlight = "123";
			transport.JW_ETA = ZDateTime.Now;
			Factory.Save();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB0001";
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			using (var shipmentForm = new Freight.Forwarding.GUI.ShipmentForm(shipment))
			{
				try
				{
					using (var plugIn = new SeaCargoShipmentPlugInForTest(shipment, shipmentForm))
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						shipmentForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.SeaCargo;
						plugIn.CreateSeaCargoJobIfRequiredExposed();
						Assert("Shipment is NOT saved.", !shipment.IsInDatabase);
						AssertNull("Ocean bill is NOT created and saved.", consol.AUCMRCusSCAOceanBill);
						AssertNull("House bill is NOT created and saved.", shipment.AUCusSCAHouse);
						AssertNull("tmpFactory saved successfully", plugIn.HouseBill);
					}
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			using (var shipmentForm2 = new Freight.Forwarding.GUI.ShipmentForm(shipment))
			{
				try
				{
					using (var plugIn2 = new SeaCargoShipmentPlugInForTest(shipment, shipmentForm2))
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						shipmentForm2.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.SeaCargo;
						plugIn2.CreateSeaCargoJobIfRequiredExposed();
						Assert("Shipment is saved.", shipment.IsInDatabase);
						AssertNotNull("Ocean bill is created and saved. consol.AUCMRCusSCAOceanBill", consol.AUCMRCusSCAOceanBill);
						AssertNotNull("House bill is created and saved. shipment.AUCusSCAHouse", shipment.AUCusSCAHouse);
						AssertNotNull("House bill is created and saved. plugIn2.HouseBill", plugIn2.HouseBill);
					}
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		ForwardingShipment shipment;
		ForwardingConsol consol;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "USLAX";
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = ContainerNumber2;
			shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKDestination = "AUSYD";
			PackLine packLine = shipment.OuterPackLines.AddNew();
			//Consol Collection in Shipment needs this to get loaded correctly(to create a pivot object)
			Factory.Save();
			shipment.Consols.Load();
		}

		protected override SeaCargoPlugIn GetPlugIn(CommonShipment parent) => new SeaCargoShipmentPlugIn((ForwardingShipment)parent);

		bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated(ZPlugIn plugIn)
		{
			MethodInfo method = typeof(ZPlugIn).GetMethod("QueryUserShouldPlugInGUIAndBusinessEntityBeCreated", BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null);
			return (bool)method.Invoke(plugIn, null);
		}

		void AssertIsShipmentCMRShipment(bool arrivalConsolExists)
		{
			using (var testPlugIn = new SeaCargoShipmentPlugIn(shipment))
			{
				if (!arrivalConsolExists)
				{
					shipment.Consols.RemoveAll();
				}

				var shipmentWrapper = new ShipmentWrapper(shipment);
				AssertEquals("TestPlugIn.IsShipmentCMRShipment", arrivalConsolExists, shipmentWrapper.IsShipmentCMRShipment);
			}
		}

		void TestSynchronisation(bool enabled, ForwardingShipment shipment, CusSCAHouse house)
		{
			AssertEquals("Synchroniser.GetHouseBill and House not same object", house, synchroniser.GetHouseBill(shipment));
			AssertSuccessfulSynchronisation(enabled, "House Bill", HouseBillNumber3, shipment.JS_HouseBillInfo, house.CA_HouseBillInfo);
			CommonContainer freightContainer = shipment.OuterPackLines[0].Containers[0];
			var sCAContainer = (CusSCAContainer)house.OceanBill.Containers.Find(new ZQuery(CusSCAContainerSchema.CN_ContainerNumber, freightContainer.JC_ContainerNum))[0];
			AssertSuccessfulSynchronisation(enabled, "Container Number", "OLCU1231231", freightContainer.JC_ContainerNumInfo, sCAContainer.CN_ContainerNumberInfo);
			AssertSuccessfulSynchronisation(enabled, "Ocean Bill", "BL3834983829", shipment.ArrivalConsol.JK_MasterBillNumInfo, house.OceanBill.CB_OceanBillInfo);
			AssertSuccessfulSynchronisation(enabled, "House Bill", "BOX", shipment.OuterPackLines[0].JL_F3_NKPackTypeInfo, house.Pivot[0].CV_PackageTypeInfo, "BX");
		}

		SeaCargoSynchroniser synchroniser;

		void AssertSuccessfulSynchronisation(bool enabled, string message, ZString expectedValue, ZPropertyInfo shipmentField, ZPropertyInfo houseField)
		{
			AssertSuccessfulSynchronisation(enabled, message, expectedValue, shipmentField, houseField, expectedValue);
		}

		void AssertSuccessfulSynchronisation(bool enabled, string message, ZString shipmentValue, ZPropertyInfo shipmentField, ZPropertyInfo houseField, ZString expectedHouseValue)
		{
			Assert("Pre Condition - " + message + " Not Currently That Value", (ZString)houseField.Value != expectedHouseValue);
			shipmentField.Value = shipmentValue;
			if (enabled)
			{
				AssertEquals("Synch On should happen " + message, expectedHouseValue, houseField.Value);
			}
			else
			{
				Assert("Synch Off should not happen " + message, expectedHouseValue != (ZString)houseField.Value);
			}
		}

		void EnsureValidSeaCargoShipment(ForwardingShipment shipment)
		{
			shipment.OuterPackLines[0].JL_ActualVolume = 1m;
			shipment.JS_GoodsDescription = "Test Description of Goods";
			var container = shipment.OuterPackLines[0].GetContainer(shipment.ArrivalConsol);
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			shipment.OuterPackLines[0].JL_ActualWeight = 1000m;
			shipment.JS_F3_NKPackType = "PKG";
			//Container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			shipment.ArrivalConsol.JK_PrepaidCollect = "PPD";
			shipment.JS_HouseBill = "78923478923";
			shipment.ConsigneePK = GetConsignee("SCA Consignee").PK;
			shipment.ConsignorPK = GetConsignor("SCA Consignor").PK;
			shipment.JS_MarksAndNumbers = "Nil MArks";
		}

		public void TestCreateSeaCargoJobIfRequired_DuplicateWaitObjectException()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var consol = SeaCargoPluginTestHelper.GetConsolForTest(Factory);
			var shipment = consol.Shipments.AddNew();
			Factory.Save();
			CusSCAHouse duplicatedHouseBill = null;

			using (var testPlugIn = new SeaCargoShipmentPlugInForTest(shipment))
			{
				testPlugIn.OnHouseBillCreated += (x, y) =>
				{
					duplicatedHouseBill = CreateHouseBillForShipment(Factory, shipment.PK, consol.PK);
				};

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.CreateSeaCargoJobIfRequiredExposed();
				CombineAssertions("A DuplicatedOceanBillException Throwed", () =>
				{
					AssertEquals("HouseBill", duplicatedHouseBill.PK, testPlugIn.HouseBill.PK);
					AssertNullOrEmpty("PlugInNotDisplayedMessage", testPlugIn.PlugInNotDisplayedMessage);
					Assert("Mutex should be unlocked", !testPlugIn.GetMutex().IsLocked);
				});
			}
		}

		public void TestCreateSeaCargoJobIfRequired_Exception()
		{
			const string expectedDisplayMessage = "An error occurred when creating the sea cargo job.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Shipment.";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var consol = SeaCargoPluginTestHelper.GetConsolForTest(Factory);
			var shipment = consol.Shipments.AddNew();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (factory.NameForDebugging == "SeaCargoTemporaryFactory")
				{
					throw SeaCargoPluginTestHelper.GetZSaveExceptionForTest(Factory, ((INeedRow)consol).Row);
				}
			});

			using (var testPlugIn = new SeaCargoShipmentPlugInForTest(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testPlugIn.CreateSeaCargoJobIfRequiredExposed();
				CombineAssertions("An Exception Throwed", () =>
				{
					AssertNull("HouseBill", testPlugIn.HouseBill);
					AssertEquals("PlugInNotDisplayedMessage", expectedDisplayMessage, testPlugIn.PlugInNotDisplayedMessage);
					Assert("Mutex should be unlocked", !testPlugIn.GetMutex().IsLocked);
				});
			}
		}

		static CusSCAHouse CreateHouseBillForShipment(BusinessObjectFactory factory, ZGuid shipmentPK, ZGuid consolPK)
		{
			var shipment = factory.Load<ForwardingShipment>(shipmentPK);
			var consolInNewFactory = factory.Load<ForwardingConsol>(consolPK);
			var synchroniserInNewFactory = new CMRSeaCargoSynchroniser(consolInNewFactory, synchroniseConsol: false);
			var duplicatedHouseBill = synchroniserInNewFactory.GetHouseBill(shipment);
			factory.Save();
			return duplicatedHouseBill;
		}

		sealed class SeaCargoShipmentPlugInForTest : SeaCargoShipmentPlugIn
		{
			public SeaCargoShipmentPlugInForTest(ForwardingShipment shipment) : base(shipment)
			{
			}

			public SeaCargoShipmentPlugInForTest(ForwardingShipment shipment, ZForm form) : base(shipment)
			{
				this.fForm = form;
			}

			protected override ContinueWithSave ShowPreSaveDialogs_CMR()
			{
				ShowPreSaveDialogs_CMRCalled = true;
				return ContinueWithSave.Yes;
			}

			public ZGlobalMutex GetMutex() => SeaCargoShipmentInfo.Mutex;

			public void CreateSeaCargoJobIfRequiredExposed()
			{
				CreateSeaCargoJobIfRequired();
			}

			public bool ShowPreSaveDialogs_CMRCalled;

			protected override BusinessObjectFactory LoadOrCreateHouseBillInTemporaryFactory()
			{
				var result = base.LoadOrCreateHouseBillInTemporaryFactory();
				OnHouseBillCreated?.Invoke(this, null);
				return result;
			}

			public event EventHandler OnHouseBillCreated;
		}
	}
}
