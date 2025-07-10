using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class ManifestPluginMenuTest : TestCaseWithFactory
	{
		public void TestName()
		{
			AssertEquals("Customs Manifest", menu.Text);
			AssertEquals(2, menu.MenuItems.Count);
		}

		public void TestExportMenuItem()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var wrapper = new CustomsJobVoyageWrapper(voyage);
			var manifestMenu = new ManifestPluginMenu(wrapper);
			AssertEquals("Create Export Manifests", manifestMenu.MenuItems[0].Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			manifestMenu.MenuItems[0].PerformClick();
			AssertEquals("Should be Message", "Information The Sailing Schedule should be saved before creating Export Manifest", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			var vessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory).First();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";
			voyage.JV_VoyageType = ZString.Empty;
			var voyOrigin = voyage.Origins.AddNew();
			voyOrigin.JA_RL_NKPortOfLoading = "HKHKG";
			var voyDestination = voyage.Destinations.AddNew();
			voyDestination.JB_RL_NKPortOfDischarge = "AUSYD";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			manifestMenu.MenuItems[0].PerformClick();
			AssertEquals("Should be Message", "Information The Sailing Schedule should be saved before creating Export Manifest", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			manifestMenu.MenuItems[0].PerformClick();
			AssertEquals("Should be Message", "Information The Voyage Type should be set up before creating Export Manifest", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			voyage.JV_VoyageType = Core.Constants.VoyageType.MainVoyage;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			manifestMenu.MenuItems[0].PerformClick();
			AssertEquals("Should be NO Messages", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			AssertEquals(typeof(ExportManifestImportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestImportMenuItem()
		{
			menu.MenuItems.FindByText("Create Import Manifests").PerformClick();
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("There are no Bills of Lading relating to this voyage that are not already in a customs import manifest.", UnitTestUserNotification.Instance.LastMessage.Text);

			var voyageWrapper = new CustomsJobVoyageWrapper(Factory.NewWithValidTestData<JobVoyage>());
			var manifestMenu = new ManifestPluginMenu(voyageWrapper);
			manifestMenu.MenuItems.FindByText("Create Import Manifests").PerformClick();
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Sailing schedule should be created before!", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDontLoadStuffInTheSailingFactory_ExportManifest()
		{
			ZGuid voyagePK;
			{
				var createFactory = new BusinessObjectFactory();
				var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();
				var voyage = createFactory.New<JobVoyage>();
				voyage.JV_VoyageType = Core.Constants.VoyageType.MainVoyage;
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "001";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
				voyage.GenerateSailings();
				var sailing = voyage.Sailings[0];
				var bill = createFactory.New<BillOfLading>();
				bill.JS_JX = sailing.PK;
				var container1 = bill.RealContainers.AddNew();
				container1.JC_ContainerNum = "TEST4100013";
				container1.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				var booking = createFactory.New<AgencyBooking>();
				booking.JS_JX = sailing.PK;
				var container2 = booking.BookedContainers.AddNew();
				container2.JC_ContainerCount = 2;
				container2.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				createFactory.Save();
				voyagePK = voyage.PK;
			}

			{
				var sailingFactory = new BusinessObjectFactory();
				var voyage = sailingFactory.Load<JobVoyage>(voyagePK);
				voyage.MarkAsNeedingValidationIncludingChildren();
				voyage.RunPreSaveValidation();
				sailingFactory.ResetDatabaseLoadCount();
				var wrapper = new CustomsJobVoyageWrapper(voyage);
				using (var menu = new ManifestPluginMenu(wrapper))
				{
					var item = MenuAssertion.AssertHasMenu(menu, "Create Export Manifests");
					item.PerformClick();
				}

				AssertMaxDbHits("Should not have loaded anything new in the sailing factory", 0, sailingFactory);
			}
		}

		public void TestDontLoadStuffInTheSailingFactory_ImportManifest()
		{
			ZGuid voyagePK;
			{
				var createFactory = new BusinessObjectFactory();
				var vessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First();
				var voyage = createFactory.New<JobVoyage>();
				voyage.JV_RV_NKVessel = vessel.RV_FK;
				voyage.JV_VoyageFlight = "001";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
				voyage.GenerateSailings();
				var sailing = voyage.Sailings[0];
				var bill = createFactory.New<BillOfLading>();
				bill.JS_JX = sailing.PK;
				var container1 = bill.RealContainers.AddNew();
				container1.JC_ContainerNum = "TEST4100013";
				container1.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				var booking = createFactory.New<AgencyBooking>();
				booking.JS_JX = sailing.PK;
				var container2 = booking.BookedContainers.AddNew();
				container2.JC_ContainerCount = 2;
				container2.JC_RC = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				createFactory.Save();
				voyagePK = voyage.PK;
			}

			{
				var sailingFactory = new BusinessObjectFactory();
				var voyage = sailingFactory.Load<JobVoyage>(voyagePK);
				voyage.MarkAsNeedingValidationIncludingChildren();
				voyage.RunPreSaveValidation();
				sailingFactory.ResetDatabaseLoadCount();
				var wrapper = new CustomsJobVoyageWrapper(voyage);
				using (var menu = new ManifestPluginMenu(wrapper))
				{
					var item = MenuAssertion.AssertHasMenu(menu, "Create Import Manifests");
					item.PerformClick();
					AssertEquals("0 bills were added to the import manifest.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				AssertMaxDbHits("Should not have loaded anything new in the sailing factory", 0, sailingFactory);
			}
		}

		public void TestSavedBillsCount()
		{
			var vessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory).First();
			var voyage = Factory.New<JobVoyage>();
			var voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_E_DEP = new ZDateTime(2006, 11, 1);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			var bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_HouseBill = "Bill1";
			bill.JS_GoodsDescription = "foo";
			bill.JS_MarksAndNumbersShort = "Marks and Numbers";
			bill.JS_RL_NKOrigin = "SGSIN";
			bill.JS_RL_NKDestination = "AUSYD";
			bill.JS_OuterPacks = 22;
			bill.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			bill.JS_F3_NKPackType = Core.Constants.PkgUnit.Pail;
			Factory.Save();
			using (var menu = new ManifestPluginMenu(voyageWrapper))
			{
				menu.MenuItems[1].PerformClick();
				AssertEquals("0 bills were added to the import manifest.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
			{
				var manifestForm = (ImportManifestImportForm)obj;
				manifestForm.FindSingle<ZDropEdit>("ArrivalPortsDropEdit").Text = "ALL";
				manifestForm.FindSingle<ZButton>("SelectButton").PerformClick();
				manifestForm.FindSingle<ZButton>("SaveButton").PerformClick();
			});

			using (var menu = new ManifestPluginMenu(voyageWrapper))
			{
				menu.MenuItems[1].PerformClick();
				AssertEquals("1 bill was added to the import manifest.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var vessel = RefVessel.LookupVesselByName("ADMIRALENGRACHT", Factory).First();
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageType = Core.Constants.VoyageType.MainVoyage;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";
			var voyOrigin = voyage.Origins.AddNew();
			voyOrigin.JA_RL_NKPortOfLoading = "HKHKG";
			var voyDestination = voyage.Destinations.AddNew();
			voyDestination.JB_RL_NKPortOfDischarge = "AUSYD";
			voyageWrapper = new CustomsJobVoyageWrapper(voyage);
			menu = new ManifestPluginMenu(voyageWrapper);
		}

		protected override void TearDown()
		{
			menu.Dispose();
			base.TearDown();
		}

		CustomsJobVoyageWrapper voyageWrapper;
		MenuItem menu;
	}
}
