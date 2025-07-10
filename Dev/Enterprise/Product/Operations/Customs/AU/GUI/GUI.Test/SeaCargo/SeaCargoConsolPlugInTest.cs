using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoConsolPlugInTest : SeaCargoPlugInTest
	{
		public void TestCanDelete()
		{
			consol = Factory.New<ForwardingConsol>();
			using (var testPlugin = new SeaCargoConsolPlugIn(consol))
			{
				Assert(testPlugin.CanDelete);
			}
		}

		public void TestBreakBulkAndBulkContainersVisibility()
		{
			AssertEquals("Pre-Conditon there are 2 containers in the collection", 2, oceanBill.Containers.Count);
			using (var testPlugIn = new SeaCargoConsolPlugIn(consol))
			{
				testPlugIn.OnUserControlShown();
				AssertEquals("There is 1 container in the collection as the BreakBulk has been filtered", 2, oceanBill.Containers.Count);
			}
		}

		public void TestSeaCargoConsolPlugInVisibility()
		{
			using (ZForm dummyForm = new ZForm(consol))
			{
				dummyForm.PlugIns.Add(ControllerIDs.Customs.AU.SeaCargo);
				var testPlugIn = dummyForm.PlugIns.Instances[0] as SeaCargoConsolPlugIn;
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("SeaCargo Tab Visible", true, testPlugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("SeaCargo Tab Visible", false, testPlugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("SeaCargo Tab Visible", true, testPlugIn.Enabled);
				consol.JK_RL_NKDischargePort = "NZAKL"; //Changed to NZ one
				AssertEquals("SeaCargo Tab Visible", false, testPlugIn.Enabled);
				consol.JK_RL_NKDischargePort = "AUSYD";
				AssertEquals("SeaCargo Tab Visible", true, testPlugIn.Enabled);
			}
		}

		public void TestInitialVisibility()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (var testPlugIn = new SeaCargoConsolPlugIn(consol))
			{
				testPlugIn.OnGUIShown();
				AssertEquals("Plug In Enabled", true, testPlugIn.Enabled);
			}
		}

		public void TestSaveOceanBillAfterCreated()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			transport.JW_VoyageFlight = "123";
			transport.JW_ETA = ZDateTime.Now;
			using (var consolForm = new Freight.Forwarding.GUI.ConsolForm(consol))
			{
				try
				{
					using (var plugIn = new SeaCargoConsolPlugInForTest(consol, consolForm))
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						consolForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.SeaCargo;
						plugIn.CreateSeaCargoJobIfRequired();
						Assert("Consol is NOT saved.", !consol.IsInDatabase);
						AssertNull("Ocean bill is NOT created and saved.", consol.AUCMRCusSCAOceanBill);
						AssertNull("plugIn.OceanBill", plugIn.OceanBill);
					}
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}

			using (var consolForm2 = new Freight.Forwarding.GUI.ConsolForm(consol))
			{
				try
				{
					using (var plugIn2 = new SeaCargoConsolPlugInForTest(consol, consolForm2))
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						consolForm2.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.SeaCargo;
						plugIn2.CreateSeaCargoJobIfRequired();
						Assert("Consol is saved.", consol.IsInDatabase);
						AssertNotNull("Ocean bill is created and saved. consol.AUCMRCusSCAOceanBill", consol.AUCMRCusSCAOceanBill);
						AssertNotNull("Ocean bill is created and saved. plugIn.OceanBill", plugIn2.OceanBill);
					}
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestSeaCargoConsolCannotObtainMutex()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			transport.JW_VoyageFlight = "123";
			transport.JW_ETA = ZDateTime.Now;
			AssertNotNull("Schedule on Consol not Created", consol.Schedule);
			using (var consolForm = new Freight.Forwarding.GUI.ConsolForm(consol))
			{
				try
				{
					SeaCargoMutex.Lock();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					consolForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.AU.SeaCargo;
					consolForm.Show();
					var seaCargoPlugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.AU.SeaCargo);
					AssertEquals("Unable to create sea cargo job.  Someone is already in the process of creating this job.", seaCargoPlugin.PlugInNotDisplayedMessage);
				}
				finally
				{
					SeaCargoMutex.Unlock();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestSeaCargoConsolMutexFailureIsRecoverable()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "SGSIN";
			consol.JK_RL_NKDischargePort = "AUSYD";
			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			transport.JW_VoyageFlight = "123";
			transport.JW_ETA = ZDateTime.Now;

			using (var consolForm = new Freight.Forwarding.GUI.ConsolForm(consol))
			{
				try
				{
					using (var testPlugIn = new SeaCargoConsolPlugInForTest(consol, consolForm))
					{
						CusSCAOceanBill oceanBill = null;
						testPlugIn.OnOceanBillCreated += (x, y) =>
						{
							// simulate a mutex failure by injecting an attached ocean bill.
							// BaseCusSCAOceanBill will discover this during OnSaving() and throw a DuplicatedOceanBillException.
							oceanBill = Factory.New<CusSCAOceanBill>();
							oceanBill.CB_ParentId = consol.PK;
							oceanBill.CB_ParentTableCode = "JK";
							Factory.Save();
						};

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						testPlugIn.CreateSeaCargoJobIfRequired();
						Assert("Consol is saved.", consol.IsInDatabase);
						AssertEquals("Mutex failure has recovered and the 'phantom' ocean bill is retained.", consol.AUCMRCusSCAOceanBill.PK, oceanBill.PK);
						AssertEquals("OceanBill", oceanBill.PK, testPlugIn.OceanBill.PK);
						AssertNullOrEmpty("PlugInNotDisplayedMessage", testPlugIn.PlugInNotDisplayedMessage);
						Assert("Mutex should be unlocked", !testPlugIn.GetMutex().IsLocked);
					}
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestCCRProcessorCannotObtainMutex()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Shipments.AddNew().JS_HouseBill = "HB001";
			consol.Shipments.AddNew().JS_HouseBill = "HB002";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "UAIEV";
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = forwarder.MainAddress.PK;
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_OH_OrgProxy = forwarder.PK;
			company.Branches.AddNew();
			Factory.Save();
			using (var plugIn = new SeaCargoConsolPlugInForTest(consol))
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

		public void TestCCRProcessorCreatesOceanBillFirst()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
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
			var ccrProcessor = new CCRProcessor(consolInFactory2, Core.Constants.CountryCodes.Australia);
			ccrProcessor.Process(notifications, new System.Threading.CancellationToken());
			using (var plugin = new SeaCargoConsolPlugInForTest(consol))
			{
				plugin.CreateSeaCargoJobIfRequired();
			}

			Assert("Workflow has Ocean bill", consolInFactory2.AUCMRCusSCAOceanBill != null);
			Assert("Plugin has Ocean bill", consol.AUCMRCusSCAOceanBill != null);
			AssertEquals("Plugin has the same Ocean bill as the workflow", consolInFactory2.AUCMRCusSCAOceanBill.PK, consol.AUCMRCusSCAOceanBill.PK);
		}

		[ExpectNoExceptions()]
		public void TestNoException()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.AddNew();
			using (var plugin = new SeaCargoConsolPlugIn(shipment.ArrivalConsol))
			{
				plugin.OnGUIShown();
				shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
				shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			}
		}

		public void TestMessagingNotAllowedWhenASeaShipmentIsOpenForEdit()
		{
			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();
				var mainMenuItem = form.Menu.MenuItems.FindByText("Sea Cargo", false);
				AssertNotNull(mainMenuItem);
				typeof(MenuItem).InvokeMember("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, mainMenuItem, new object[] { EventArgs.Empty });
				var menuItem = mainMenuItem.MenuItems.FindByText("Send Message(s)", true);
				AssertNotNull(menuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals(SeaCargoConsolMenu.ReasonMessagingIsSuppressedText, UnitTestUserNotification.Instance.LastMessage.Text);
				menuItem = mainMenuItem.MenuItems.FindByText("Refresh Sea Cargo Data", true);
				AssertNotNull(menuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("One or more shipments of this consol are open on other forms. Please close these shipment forms before Synchronizing.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals("One or more shipments of this consol are open on other forms. Please close these shipment forms before Synchronizing.", UnitTestUserNotification.Instance.LastMessage.Text);
				menuItem = mainMenuItem.MenuItems.FindByText("Send Underbond Requests", true);
				AssertNotNull(menuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menuItem.PerformClick();
				AssertEquals(SeaCargoConsolMenu.ReasonMessagingIsSuppressedText, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOceanBillSynchronizerWhenloadThePlugin()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "TESTHB001";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "TESTOceanBill001";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			using (var plugin = new SeaCargoConsolPlugIn(consol))
			{
				consol.Shipments.Add(shipment);
				AssertEquals("Create a new house bill for the shipment", 1, oceanBill.HouseBills.Count);
				var houseBill = oceanBill.HouseBills[0];
				AssertEquals(shipment.PK, houseBill.CA_JS);
			}

			var consol2 = Factory.New<ForwardingConsol>();
			var shipment2 = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "TESTHB001";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			using (var plugin2 = new SeaCargoConsolPlugIn(consol2))
			{
				consol2.Shipments.Add(shipment2);
				AssertNull("No ocean bill created for the consol", consol2.AUCMRCusSCAOceanBill);
			}
		}

		public void TestManagerHasOceanBillSetWhenItIsCreated()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (var testPlugIn = new SeaCargoConsolPlugIn(consol))
			{
				try
				{
					AssertNull("OceanBill", testPlugIn.Manager.OceanBill);
					AssertEquals("Should Create the OceanBill", true, testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					AssertNotNull("OceanBill", testPlugIn.Manager.OceanBill);
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestShowPreSaveDialogs_CMR()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (var testPlugIn = new SeaCargoConsolPlugInForTest(consol))
			{
				testPlugIn.ShowPreSaveDialogs();
				AssertEquals("ShowPreSaveDialogsLegacyCalled", true, testPlugIn.ShowPreSaveDialogsCMRCalled);
			}
		}

		public void TestLoadZPlugIn()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (var testPlugIn = new SeaCargoConsolPlugIn(consol))
			{
				try
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var seaCargoMenu = testPlugIn.TopLevelMenu;
					AssertNotNull(seaCargoMenu);
					AssertEquals("Should Create the OceanBill", true, testPlugIn.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
					AssertNotNull("We should always return a OceanBill Object", testPlugIn.BusinessEntity);
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		protected override SeaCargoPlugIn GetPlugIn(CommonShipment parent) => new SeaCargoConsolPlugIn((ForwardingConsol)parent.ArrivalConsol);

		CommonShipment shipment;
		ForwardingConsol consol;
		CusSCAOceanBill oceanBill;
		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			CreateTestData();
		}

		void CreateTestData()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = TestPortOfLoading;
			consol.JK_RL_NKDischargePort = TestPortOfDischarge;
			consol.JK_MasterBillNum = TestOceanBill;
			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "23";
			transport.JW_ETA = DateTime.Now.AddDays(5);
			transport.JW_ETD = DateTime.Now;
			shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = TestHouseBillNumber;
			shipment.JS_RL_NKOrigin = TestPortOfLoading;
			shipment.JS_RL_NKDestination = TestPortOfDischarge;
			shipment.JS_INCO = Core.Constants.IncoTerms.DefaultIncoFromPaymentType(CMRMethodsOfPayment.Codes.PrepaidOnly);
			shipment.JS_ReleaseType = Core.Constants.ShipmentReleaseTypes.SeaWaybill;
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TESTIGNEE";
			consignee.OH_IsConsignee = true;
			consignee.MainAddress.OA_Address1 = "TEST CONSIGNEE ADDRESS";
			shipment.ConsigneePK = consignee.PK;
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TESTIGNOR";
			consignor.OH_IsConsignor = true;
			consignor.MainAddress.OA_Address1 = "TEST CONSIGNOR ADDRESS";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			shipment.ConsigneeDeliveryAddress.OrganisationPK = consignee.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = consignor.PK;
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
			houseBill.CA_ConsignorAddress2 = "PRIVATE BAG 93 002";
			houseBill.CA_ConsignorSuburb = "NEW LYNN AUCKLAND";
			houseBill.CA_NotifyName = "TRITEC PTY LTD";
			houseBill.CA_NotifyAddress1 = "C O NEW CENTURY PACKING";
			houseBill.CA_NotifyAddress2 = "UNIT 5 370 NUDGEE ROAD";
			houseBill.CA_NotifySuburb = "HENDRA QLD AUSTRALIA";
			houseBill.CA_NotifyPostcode = "4011";
			houseBill.CA_HouseBill = "49104160312018";
			houseBill.CA_JS = shipment.PK;
			var container = oceanBill.Containers.AddNew();
			container.CN_ContainerNumber = "FSCU6400235";
			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.CN_RC_NKContainerType = "40GP";
			container.CN_ShipperOwnedContainer = false;
			container.CN_SealNumber = "89644";
			var houseContainerPivot = houseBill.Pivot.AddNew();
			houseContainerPivot.CV_PackageCount = 40;
			houseContainerPivot.CV_PackageType = "BX";
			houseContainerPivot.CV_MarksAndNumbers = "FSCU6400235";
			houseContainerPivot.CV_GoodsDescription = "STC 40 BAGS OF PLASTIC REGRIND";
			houseContainerPivot.CV_WeightUQ = Core.Constants.Weight.Kilograms;
			houseContainerPivot.CV_Weight = 23900.00m;
			houseContainerPivot.CV_Volume = 68.000m;
			houseContainerPivot.CV_CN = container.PK;
			var container2 = oceanBill.Containers.AddNew();
			container2.CN_ContainerMode = "BBK";
			container2.CN_ContainerNumber = CusSCAPivot.BreakBulk;
			var houseContainerPivot2 = houseBill.Pivot.AddNew();
			houseContainerPivot2.CV_CN = container2.PK;
			houseContainerPivot2.CV_PackageCount = 56;
			houseContainerPivot2.CV_PackageType = "BX";
			houseContainerPivot2.CV_MarksAndNumbers = "WOW THIS IS GREAT";
			houseContainerPivot2.CV_GoodsDescription = "WOMEN TIED UP";
			houseContainerPivot2.CV_WeightUQ = Core.Constants.Weight.Kilograms;
			houseContainerPivot2.CV_Weight = 23900.00m;
			houseContainerPivot2.CV_Volume = 68.000m;
			Factory.Save();
			shipment.Consols.Load();
		}

		public void TestCreateSeaCargoJobIfRequired_Exception()
		{
			const string expectedDisplayMessage = "An error occurred when creating the sea cargo job.\r\nPlease change to another tab, then click back to this tab to create a sea cargo job for this Consol.";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var consol = SeaCargoPluginTestHelper.GetConsolForTest(Factory);

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (factory.NameForDebugging == "SeaCargoTemporaryFactory")
				{
					throw SeaCargoPluginTestHelper.GetZSaveExceptionForTest(Factory, ((INeedRow)consol).Row);
				}
			});

			using (var testPlugIn = new SeaCargoConsolPlugInForTest(consol))
			{
				testPlugIn.CreateSeaCargoJobIfRequired();
				CombineAssertions("An Exception throwed", () =>
				{
					AssertNull("OceanBill", testPlugIn.OceanBill);
					AssertEquals("PlugInNotDisplayedMessage", expectedDisplayMessage, testPlugIn.PlugInNotDisplayedMessage);
					Assert("Mutex should be unlocked", !testPlugIn.GetMutex().IsLocked);
				});
			}
		}

		ZGlobalMutex mutex;
		ZGlobalMutex SeaCargoMutex => mutex ?? (mutex = Customs.Business.BaseCusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.Australia));

		const string TestOceanBill = "OCEANTEST123";
		const string TestPortOfLoading = "HKHKG";
		const string TestPortOfDischarge = "AUSYD";
		const string TestHouseBillNumber = "TESTHOUSE123";

		sealed class ConsolFormForTest : Freight.Forwarding.GUI.ConsolForm
		{
			public ConsolFormForTest(ForwardingConsol businessEntity) : base(businessEntity)
			{
			}

			public override bool IsAnyShipmentOpenForEdit => true;
		}

		sealed class SeaCargoConsolPlugInForTest : SeaCargoConsolPlugIn
		{
			public SeaCargoConsolPlugInForTest(ForwardingConsol consol) : base(consol)
			{
			}

			public SeaCargoConsolPlugInForTest(ForwardingConsol consol, ZForm form) : base(consol)
			{
				fForm = form;
			}

			public ZGlobalMutex GetMutex() => Mutex;

			public new void CreateSeaCargoJobIfRequired() => base.CreateSeaCargoJobIfRequired();

			public bool ShowPreSaveDialogsCMRCalled;

			protected override ContinueWithSave ShowPreSaveDialogsCMR()
			{
				ShowPreSaveDialogsCMRCalled = true;
				return base.ShowPreSaveDialogsCMR(); // WinForm.ContinueWithSave.Yes;
			}

			protected override BusinessObjectFactory LoadOrCreateOceanBillInTemporaryFactory()
			{
				var result = base.LoadOrCreateOceanBillInTemporaryFactory();
				OnOceanBillCreated?.Invoke(this, null);
				return result;
			}

			public event EventHandler OnOceanBillCreated;
		}
	}
}
