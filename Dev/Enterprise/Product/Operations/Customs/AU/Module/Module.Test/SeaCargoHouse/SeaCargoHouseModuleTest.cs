using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoHouseModule))]
	sealed class SeaCargoHouseModuleTest : ZModuleBasherTest
	{
		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on Sea Cargo House module", true);
		}

		public void TestModuleProperties()
		{
			using (var module = new SeaCargoHouseModule())
			{
				AssertEquals("module.ID", ModuleIDs.Customs.AU.HouseSeaCargo, module.ID);
				AssertEquals("module.SupportsWorkflow", true, module.SupportsWorkflow);
				AssertEquals("module.WorkflowType", WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode, module.WorkflowType);
			}
		}

		public void TestGetNewController()
		{
			using (var testModule = new SeaCargoHouseModule())
			{
				var houseBill1 = Factory.NewWithValidTestData<CusSCAHouse>();
				AssertEquals("StandAlone housebill form", typeof(SeaCargoHouseController), testModule.GetNewControllerInternal(houseBill1).GetType());

				var consol = Factory.New<ForwardingConsol>();
				var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
				var houseBill2 = Factory.NewWithValidTestData<CusSCAHouse>();
				oceanBill.CB_ParentId = consol.PK;
				oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
				houseBill2.CA_CB = oceanBill.PK;
				AssertEquals("Open Consol form", typeof(SeaCargo.AUCustomsSeaCargoController), testModule.GetNewControllerInternal(houseBill2).GetType());

				var shipment = consol.Shipments.AddNew();
				var houseBill3 = Factory.NewWithValidTestData<CusSCAHouse>();
				houseBill3.CA_JS = shipment.PK;
				AssertEquals("Open Shipment form", typeof(SeaCargoHouseShipmentController), testModule.GetNewControllerInternal(houseBill3).GetType());
			}
		}

		public void TestViewAndEditMenuItems()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.SeaCargoHouse, Constants.CountryCodes.Australia, ZDateTime.Now, true))
			{
				var bill1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
				bill1.CB_GB = GlbBranch.CurrentBranch.PK;
				var bill2 = Factory.NewWithValidTestData<CusSCAOceanBill>();
				bill2.CB_GB = GlbBranch.CurrentBranch.PK;
				for (int i = 0; i < 6; i++)
				{
					bill1.HouseBills.AddNew();
					bill2.HouseBills.AddNew();
				}

				Factory.Save();
				using (var moduleForm = new ZForm())
				using (var module = (SeaCargoHouseModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.AU.HouseSeaCargo))
				{
					var filterControl = module.EmbeddedControl;
					moduleForm.Controls.Add(filterControl);
					moduleForm.Show();
					var viewHouse = module.ViewMenuItem.MenuItems.FindByText("View Sea Cargo House");
					AssertNotNull(viewHouse);
					var viewReport = module.ViewMenuItem.MenuItems.FindByText("View Sea Cargo Report");
					AssertNotNull(viewReport);
					var editHouse = module.EditMenuItem.MenuItems.FindByText("Edit Sea Cargo House");
					AssertNotNull(editHouse);
					var editReport = module.EditMenuItem.MenuItems.FindByText("Edit Sea Cargo Report");
					AssertNotNull(editReport);
					((IFilterGridModuleInternalsForTesting)module).PerformSearch();
					Application.DoEvents();
					var viewCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.AU.HouseSeaCargo);
					AssertEquals(12, viewCollection.Count);
					module.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					viewHouse.PerformClick();
					AssertEquals("You have selected a large number of records, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, OpenedFormCache.GetInstance().Count);
					viewReport.PerformClick();
					var pk = ((CusSCAHouse)module.DisplayGrid.GetFirstSelectedRow()).OceanBill.PK;
					var controllerIdString = ControllerIDs.Customs.AU.SeaCargoStandAloneController.ToString();
					using (var form = (IZForm)OpenedFormCache.GetInstance().GetForm(pk.ToGuid(), controllerIdString))
					{
						AssertEquals("Should open the first one", 1, OpenedFormCache.GetInstance().Count);
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					editHouse.PerformClick();
					AssertEquals("You have selected a large number of records, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, OpenedFormCache.GetInstance().Count);
					editReport.PerformClick();
					using (var form = (IZForm)OpenedFormCache.GetInstance().GetForm(pk.ToGuid(), controllerIdString))
					{
						AssertEquals("Should open the first one", 1, OpenedFormCache.GetInstance().Count);
					}
				}
			}
		}

		public void TestActionMenuItems()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.SeaCargoHouse, Constants.CountryCodes.Australia, ZDateTime.Now, true))
			{
				var bill1 = Factory.NewWithValidTestData<CusSCAOceanBill>();
				bill1.CB_GB = GlbBranch.CurrentBranch.PK;
				bill1.CB_OceanBill = "Ocean Bill 1";
				var bill2 = Factory.NewWithValidTestData<CusSCAOceanBill>();
				bill2.CB_GB = GlbBranch.CurrentBranch.PK;
				bill2.CB_OceanBill = "Ocean Bill 2";
				bill1.HouseBills.AddNew();
				bill2.HouseBills.AddNew();
				Factory.Save();
				using (var moduleForm = new ZForm())
				using (var module = (SeaCargoHouseModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.AU.HouseSeaCargo))
				{
					var filterControl = module.EmbeddedControl;
					moduleForm.Controls.Add(filterControl);
					moduleForm.Show();
					var separator = module.ActionsMenuItem.MenuItems.FindByText("-");
					AssertNotNull(separator);
					var sendHouse = module.ActionsMenuItem.MenuItems.FindByText("Send Sea Cargo House Messages");
					AssertNotNull(sendHouse);
					var sendReport = module.ActionsMenuItem.MenuItems.FindByText("Send Sea Cargo Report Messages");
					AssertNotNull(sendReport);
					var sendUnderbond = module.ActionsMenuItem.MenuItems.FindByText("Send Underbond Request Messages");
					AssertNotNull(sendUnderbond);
					((IFilterGridModuleInternalsForTesting)module).PerformSearch();
					Application.DoEvents();
					var viewCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleIDs.Customs.AU.HouseSeaCargo);
					AssertEquals(2, viewCollection.Count);
					module.DisplayGrid.SelectAllElements();
					ZFormModaliser.ShowDialogsInTest = true;
					sendHouse.PerformClick();
					var dialog = ZFormModaliser.LastIBusinessShownOnDialogForTest;
					AssertNotNull(dialog);
					AssertEquals("Which messages do you want sent?", ((MessageChooserNonPersistent)dialog).Question);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendReport.PerformClick();
					AssertEquals("Please select a single Sea Cargo Report (Job) and try again", UnitTestUserNotification.Instance.LastMessage.Text);
					module.DisplayGrid.UnSelectAll();
					module.DisplayGrid.Select(0);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendReport.PerformClick();
					AssertEquals("This will send a Sea Cargo message for every House Bill on Ocean Bill 1 that does not have validation errors against it, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					module.DisplayGrid.SelectAllElements();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendUnderbond.PerformClick();
					AssertEquals("Please select a single Sea Cargo Report (Job) and try again", UnitTestUserNotification.Instance.LastMessage.Text);
					module.DisplayGrid.UnSelectAll();
					module.DisplayGrid.Select(1);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					sendUnderbond.PerformClick();
					AssertEquals("This will send an Underbond Request message for every House Bill on Ocean Bill 2 that does not have validation errors against it, do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.HouseSeaCargo;
	}
}
