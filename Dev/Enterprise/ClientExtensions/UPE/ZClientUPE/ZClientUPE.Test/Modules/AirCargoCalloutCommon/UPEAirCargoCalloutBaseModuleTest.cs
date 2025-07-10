using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal class UPEAirCargoCalloutBaseModuleTest : TestCaseWithFactory
	{
		public void TestLicenceAndSecurityCheckpoint()
		{
			AssertEquals(Env.Licence.AlwaysAllow, Module.LicenceCheckPoint);
			AssertEquals("Security MUST be ACAHouse otherwise it doesn't show up at the left nav pane", Env.Security.ACAHouse, Module.SecurityCheckpoint);
		}

		public void TestAllowNewAndDelete()
		{
			AssertEquals(true, Module.AllowNew);
			AssertEquals(false, Module.AllowDelete);
		}

		public void TestGridCollection()
		{
			AssertEquals(typeof(ModuleHAWBCollection), Module.GetNewGridCollection().GetType());
		}

		public void TestBulkStatusUpdatingToolbarButton()
		{
			ZToolBarButton bulkStatusUpdateButton = (ZToolBarButton)Module.ToolBarButtons.FindByText("Bulk Status Update");
			AssertNotNull("The button should exist", bulkStatusUpdateButton);
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB1 = mAWB.ChildBills.AddNew();
			CusHAWB hAWB2 = mAWB.ChildBills.AddNew();
			Factory.Save();
			Module.SelectedGridItems = new BusinessObject[] { hAWB1, hAWB2 };
			bulkStatusUpdateButton.PerformClick();
			AssertEquals("ShowDialog should have been called on the form", true, Module.LastCreatedBulkStatusUpdatingForm.ShowDialogWasCalled);
			AssertEquals("There should be 2 items selected for bulk update", 2, ((BulkStatusUpdating)Module.LastCreatedBulkStatusUpdatingForm.LastDataSourceForTest).ItemsToBulkUpdate.Length);
			Module.LastCreatedBulkStatusUpdatingForm.Dispose();
		}

		public void TestBulkStatusUpdatingToolbarButton_RestrictedToControllerUser()
		{
			bool wasController = GlbStaff.CurrentUser.GS_IsController;
			try
			{
				int numberOfButtons = 0;
				GlbStaff.CurrentUser.GS_IsController = false;
				using (TestUPEAirCargoCalloutBaseModule module = new TestUPEAirCargoCalloutBaseModule())
				{
					ToolBarButton bulkStatusUpdateButton = module.ToolBarButtons.FindByText("Bulk Status Update");
					AssertNull("The button should not exist for a non-controller user", bulkStatusUpdateButton);
					numberOfButtons = module.ToolBarButtons.Length;
				}

				GlbStaff.CurrentUser.GS_IsController = true;
				using (TestUPEAirCargoCalloutBaseModule module = new TestUPEAirCargoCalloutBaseModule())
				{
					ToolBarButton bulkStatusUpdateButton = module.ToolBarButtons.FindByText("Bulk Status Update");
					AssertNotNull("The button should exist for a controller", bulkStatusUpdateButton);
					AssertEquals("the number of buttons should existing + 1", numberOfButtons + 1, module.ToolBarButtons.Length);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = wasController;
			}
		}

		public void TestViewEntryPrintDocumentMenuItem()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(Customs.Business.BaseJobDeclaration)).PK;
			Module.SelectedGridItems = new BusinessObject[] { callout };
			MenuItem menuItem = Module.ContextMenu.FindByText("Actions").MenuItems.FindByText("View Entry Print");
			AssertNotNull("Should be able to find the View Entry Print menu item", menuItem);
			menuItem.PerformClick();
			AssertNotNull("Should have shown the 'deliver documents' form", Module.EntryPrintDocumentHelper.LastPrintTask);
		}

		public void TestViewEntryPrintDocumentMenuItem_WhenNoItemsSelected()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(Customs.Business.BaseJobDeclaration)).PK;
			Module.SelectedGridItems = System.Array.Empty<BusinessObject>();
			Module.ContextMenu.FindByText("Actions").MenuItems.FindByText("View Entry Print").PerformClick();
			AssertNull("Should NOT have shown the 'deliver documents' form", Module.EntryPrintDocumentHelper.LastPrintTask);
			AssertEquals("You must select a finance item with a formal declaration in the grid", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestViewEntryPrintDocumentMenuItem_WhenNoDecAttached()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_JE_CustomsFormalEntry = ZGuid.Empty;
			Module.SelectedGridItems = new BusinessObject[] { callout };
			Module.ContextMenu.FindByText("Actions").MenuItems.FindByText("View Entry Print").PerformClick();
			AssertNull("Should NOT have shown the 'deliver documents' form", Module.EntryPrintDocumentHelper.LastPrintTask);
			AssertEquals("The selected finance item doesn't have a declaration attached", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#region Implementation
		class TestUPEAirCargoCalloutBaseModule : UPEAirCargoModule
		{
			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return base.GetNewGridCollection();
			}

			public new MenuItem[] ContextMenu
			{
				get
				{
					return base.ContextMenu;
				}
			}

			public BusinessObject[] SelectedGridItems;
			public TestBulkStatusUpdatingForm LastCreatedBulkStatusUpdatingForm;
			protected override BulkStatusUpdatingForm NewBulkStatusUpdatingForm(BusinessObject[] selectedItems)
			{
				LastCreatedBulkStatusUpdatingForm = new TestBulkStatusUpdatingForm(FilterBusinessObject, SelectedGridItems);
				return LastCreatedBulkStatusUpdatingForm;
			}

			protected override void OnViewEntryPrint_Click(BusinessObject[] selectedGridItems)
			{
				base.OnViewEntryPrint_Click(this.SelectedGridItems);
			}

			public TestEntryPrintDocumentHelper EntryPrintDocumentHelper = new TestEntryPrintDocumentHelper();
			protected override EntryPrintDocumentHelper GetEntryPrintDocumentHelper()
			{
				return EntryPrintDocumentHelper;
			}
		}

		class TestEntryPrintDocumentHelper : EntryPrintDocumentHelper
		{
			public PrintTask LastPrintTask;
			protected override void RunPrintTask(PrintTask task)
			{
				LastPrintTask = task;
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Module = new TestUPEAirCargoCalloutBaseModule();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}

		TestUPEAirCargoCalloutBaseModule Module;
		#endregion
	}
}
