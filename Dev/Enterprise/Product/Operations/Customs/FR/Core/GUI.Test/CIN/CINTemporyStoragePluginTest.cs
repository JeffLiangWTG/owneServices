using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.GUI.CIN.Testing
{
	class CINTemporaryStoragePluginTest : ZPlugInGenericTest
	{
		protected override ZPlugIn GetPlugInToTest()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUXXX";
			consol.JK_RL_NKDischargePort = "FRXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			var header = CusTempStorageJobHeader.New(Factory);
			header.SJH_OH_Customer = orgHeader.PK;
			header.SetRelatedBusinessObject(consol, Core.Constants.GenPivotTypes.CusStorageHeaderConsol);
			Factory.Save();
			var plugin = new CINTemporaryStoragePlugin(consol);
			return plugin;
		}

		public void TestCreateNewTemporaryStorage()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUXXX";
			consol.JK_RL_NKDischargePort = "FRXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (var pluginFR = form.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController))
				{
					AssertNull(pluginFR.BusinessEntity);
					AssertEquals("", ((IPlugInInternals)pluginFR).CoveringLabel.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					pluginFR.SelectTabPage();
					AssertNull(pluginFR.BusinessEntity);
					AssertEquals("You have chosen not to create a CIN Temporary Storage now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a CIN Temporary Storage for this Job.", ((IPlugInInternals)pluginFR).CoveringLabel.Text);
				}
			}

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				using (var pluginFR = form.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					pluginFR.SelectTabPage();
					AssertNotNull(pluginFR.BusinessEntity);
					var header = pluginFR.BusinessEntity as CusTempStorageJobHeader;
					AssertNotNull(header.RelatedBusinessObject);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestExposeTabPageNotificationsForCINTemporaryStoragePlugin()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "FRXXX";
			consol.JK_TransportMode = "AIR";

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				using (var pluginFR = form.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController))
				{
					TabPageNotificationsExposer.ExposeTabPageNotifications(pluginFR.TabPage, consol);
				}
			}
		}

		public void TestVisibility()
		{
			#region Plugin Italy for air consol is not visible

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "FRXXX";
			consol.JK_RL_NKDischargePort = "ITXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			AssertCINTemporyStoragePlugin(consol, false);

			#endregion

			#region Plugin France for air export consol is visible

			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUXXX";
			consol.JK_RL_NKDischargePort = "FRXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			AssertCINTemporyStoragePlugin(consol, true);

			#endregion

			#region Plugin France for sea  export consol is not visible

			consol.JK_TransportMode = "SEA";
			AssertCINTemporyStoragePlugin(consol, false);

			consol.JK_TransportMode = "AIR";
			AssertCINTemporyStoragePlugin(consol, true);

			#endregion

			#region Plugin France for air not export consol is not visible

			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AEXXX";
			consol.JK_RL_NKDischargePort = "AUXXX";
			AssertCINTemporyStoragePlugin(consol, false);

			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AEXXX";
			consol.JK_RL_NKDischargePort = "FRXXX";
			AssertCINTemporyStoragePlugin(consol, true);

			#endregion
		}

		public void TestSaveWithLocked()
		{
			var header = GetTemporaryStorage();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUXXX";
			consol.JK_RL_NKDischargePort = "FRXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";
			var mutex = new ZGlobalMutex(MutexIDs.CusTemporaryStorageMutex, consol.PK.ToString());

			try
			{
				mutex.Lock();
				AssertEquals("Precondition: the test needs to be holding the mutex", true, mutex.HasLock);
				AssertEquals("Precondition: the header should not yet be in the database", false, header.IsInDatabase);

				using (var form = new ConsolForm(consol))
				{
					form.Show();
					Application.DoEvents();
					UserIdleWorker.Flush();

					form.PlugIns.SelectPlugInTabPage(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController);
					var plug = form.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.CINTemporaryStorageConsolController) as CINTemporaryStoragePlugin;
					//Mutex.IsLocked
					AssertEquals("Should not be holding the mutex before clicking save", false, plug.Mutex.HasLock);
					form.FireSaveButton();
					AssertEquals("Should not be holding the mutex after clicking save", false, plug.Mutex.HasLock);
				}

				AssertEquals("Should not have been saved", false, header.IsInDatabase);
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		public void TestMessageMutexLocked()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUXXX";
			consol.JK_RL_NKDischargePort = "FRXXX";
			consol.JK_MasterBillNum = "MB0001";
			consol.JK_TransportMode = "AIR";

			using (var form = new ConsolForm(consol))
			using (var plugIn1 = new CINTemporaryStoragePluginForTest(consol))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();
				plugIn1.QueryAddTemporaryStorage();
				AssertEquals(false, plugIn1.PlugInNotDisplayedMessage.Equals("Someone else is already in the process of creating a CIN Temporary Storage for this Consol.\r\nYou should be able to access the CIN Temporary Storage when the person has saved the record. Please try later."));
				plugIn1.Mutex.Lock();
				form.FireSaveButton();
				plugIn1.QueryAddTemporaryStorage();
				AssertEquals(true, plugIn1.PlugInNotDisplayedMessage.Equals("Someone else is already in the process of creating a CIN Temporary Storage for this Consol.\r\nYou should be able to access the CIN Temporary Storage when the person has saved the record. Please try later."));
				if (plugIn1.Mutex.HasLock)
				{
					plugIn1.Mutex.Unlock();
				}
			}
		}

		public void TestCreateLineOfTemporaryStorageDec()
		{
			SetupSourceConsolAndShipments();

			using (var form = new ConsolForm(sourceConsol))
			using (var plugIn1 = new CINTemporaryStoragePluginForTest(sourceConsol))
			{
				form.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();
				plugIn1.CreateTemporaryStorage();
				var cusTempStorage = plugIn1.InternalHeader;
				var cusTempStorageDec = cusTempStorage.CusTempStorageDec;
				var lines = cusTempStorageDec.CusTempStorageLines;
				AssertEquals(2, lines.Count);
				var line = lines[0];
				AssertEquals("AWB", line.TSL_OwnerReferenceType);
				AssertEquals("BOL123", line.TSL_OwnerReferenceNumber);
				line = lines[1];
				AssertEquals("HWB", line.TSL_OwnerReferenceType);
				AssertEquals("111", line.TSL_OwnerReferenceNumber);
			}
		}

		#region Methods
		void AssertCINTemporyStoragePlugin(ForwardingConsol consol, bool expected)
		{
			using (var pluginFR = new CINTemporaryStoragePlugin(consol))
			{
				AssertEquals(expected, pluginFR.Enabled);
				var menu = pluginFR.TopLevelMenu;
				AssertEquals(expected, menu.Visible);
				AssertEquals("&CIN", menu.Text);
			}
		}
		protected CusTempStorageJobHeader GetTemporaryStorage()
		{
			var cusTempStorageJobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			return cusTempStorageJobHeader;
		}
		#endregion

		#region Consol Source Control

		void SetupSourceConsolAndShipments()
		{
			sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_UniqueConsignRef = "C4321";
			sourceConsol.JK_RL_NKDischargePort = "FR123";
			sourceConsol.JK_RL_NKLoadPort = "GBFXT";
			sourceConsol.JK_TransportMode = "AIR";
			sourceConsol.JK_ConsolMode = "FCL";
			sourceConsol.JK_MasterBillNum = "BOL123";
			sourceConsol.Transports[0].JW_VoyageFlight = "FR123";
			sourceConsol.Transports[0].JW_Vessel = "ADMIRALENGRACHT";
			sourceConsol.Transports[0].JW_ETA = ZDateTime.BrettsBirthday;
			sourceConsol.Transports[0].JW_ETD = ZDateTime.BrettsBirthday.AddDays(-1);
			sourceConsol.Transports[0].JW_VoyageFlightForBinding = "FRFE";
			sourceConsol.Transports[0].JW_RL_NKLoadPortForBinding = "GBFXT";
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			localClient = org.Addresses.AddNew();
			localClient.OA_Address1 = "A";
			refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "20DC";
			containerA = sourceConsol.Containers.AddNew();
			containerA.JC_ContainerNum = "AAAA1234567";
			containerA.JC_RC = refContainer.PK;
			containerA.JC_SealNum = "S1";
			containerA.JC_AdditionalSealNum = "S2";
			containerB = sourceConsol.Containers.AddNew();
			containerB.JC_ContainerNum = "BBBB1234567";
			containerC = sourceConsol.Containers.AddNew();
			containerC.JC_ContainerNum = "CCCC1234567";

			consignee = org.Addresses.AddNew();
			consignor = org.Addresses.AddNew();
			consignee.OA_Address1 = "B";
			consignor.OA_Address1 = "C";

			principal = org.Addresses.AddNew();
			principal.OA_Address1 = "P";
			sourceConsol.JK_OA_SendingForwarderAddress = principal.PK;

			AddShipment("S0001", "BOOKS", "GBDTE", "VUAUY", consignor, consignee, "T1", "111");
		}

		void AddShipment(ZString shipmentNo, ZString description, ZString origin, ZString destination, OrgAddress shipmentConsignor, OrgAddress shipmentConsignee, ZString ctStatus, ZString houseBill)
		{
			shipment = sourceConsol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = shipmentConsignee.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipmentConsignor.PK;
			shipment.CreateJobHeaderWithMutex();
			shipment.JobHeader.JH_OA_LocalChargesAddr = localClient.PK;
			shipment.JS_UniqueConsignRef = shipmentNo;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_OuterPacks = 26;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_ActualWeight = 35m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_GoodsDescription = description;
			shipment.JS_MarksAndNumbers = "MARKS";
			shipment.JS_ActualVolume = 38m;
			shipment.JS_UnitOfVolume = "M3";
			shipment.JS_GoodsValue = 1m;
			shipment.JS_RX_NKGoodsValueCurr = "HKD";
			shipment.JS_InsuranceValue = 2m;
			shipment.JS_RX_NKInsuranceCurrency = "NZD";
			shipment.JS_CommunityTransitStatus = ctStatus;
			shipment.JS_HouseBill = houseBill;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var pivot = shipment.OuterPackLines.AddNew();
			pivot.JL_JC = containerA.PK;
			pivot.JL_PackageCount = 69;
			pivot.JL_F3_NKPackType = "BOX";
			pivot.JL_MarksAndNumbers = "Marks and numbers";
		}

		ForwardingConsol sourceConsol;
		OrgAddress principal;
		OrgAddress consignee;
		OrgAddress consignor;
		OrgAddress localClient;
		RefContainer refContainer;
		ForwardingContainer containerA;
		ForwardingContainer containerB;
		ForwardingContainer containerC;
		ForwardingShipment shipment;
		#endregion

		class CINTemporaryStoragePluginForTest : CINTemporaryStoragePlugin
		{
			public CINTemporaryStoragePluginForTest(ForwardingConsol hostEntity)
			: base(hostEntity)
			{
			}

			public new void QueryAddTemporaryStorage() => base.QueryAddTemporaryStorage();
			public new IBusiness GetBusinessEntityForPlugIn() => base.GetBusinessEntityForPlugIn();
			public new CusTempStorageJobHeader InternalHeader => base.InternalHeader;
		}
	}
}
