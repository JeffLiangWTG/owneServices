using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Services.OperationalActions.GUI;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Module.Testing
{
	[TestedType(typeof(EUH7Module))]
	sealed class EUH7ModuleTest : ZModuleBasherTest
	{
		public void TestCopyActionMenuIsAdded()
		{
			using (var module = new EUH7Module())
			{
				var menuItem = module.FormActionMenu.FindByText("Copy");
				AssertNotNull("Copy action menu is added", menuItem);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.EUH7;

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (ASYCUDA.Business.AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.AMA_JobReference = ZString.Empty;
			return result;
		}

		protected override BusinessObject CreateValidBOForTestSpecifyWorkflowType(Type elementType)
		{
			return Factory.New<AsycudaManifestHeader>();
		}

		public void TestPlugins_ContainsOperationalActions()
		{
			using var euH7Module = GetModule() as ZFilterGridModule;
			AssertNotNull(euH7Module);
			var operationalActionPlugin = euH7Module?.Plugins.GetPlugin(ControllerIDs.OperationalActions);
			AssertNotNull(operationalActionPlugin);
		}

		public void TestModule_ImplementsIOperationalActionSupportable()
		{
			using var euH7Module = GetModule();
			var operationalActionSupportable = euH7Module as IOperationalActionSupportable;
			AssertNotNull(operationalActionSupportable);
			var supporter = operationalActionSupportable?.OperationalActionSupporter;
			AssertNotNull(supporter);
			Assert(supporter is EUH7OperationalActionSupporter);
		}

		[RequiresSTA]
		public void TestBulkSendToCustoms_RunOnSelectedRecords()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "IE123";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "IE2345";
			Factory.Save();

			using (var module = GetModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7FilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var actionMenu = module.FormActionMenu.FindByText("&Actions");
				actionMenu.ShowPopupMenu();
				var operationalMenu = actionMenu.MenuItems.FindByText("Operational Actions");
				operationalMenu.ShowPopupMenu();
				var sentToCustomsMenu = operationalMenu.MenuItems.FindByText("Send To Customs");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					var operationalForm = shownForm as OperationalActionRunnerForm;
					AssertNotNull("Should open operational act form", operationalForm);
					AssertEquals("Run \'Send To Customs\' on 1 job", operationalForm.FormHeading);
				});

				sentToCustomsMenu.PerformClick();
			}
		}

		[RequiresSTA]
		public void TestBulkSendToCustoms_RunOnAllMatchingRecords()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "IE123";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "IE2345";
			Factory.Save();

			using (var module = GetModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7FilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var actionMenu = module.FormActionMenu.FindByText("&Actions");
				actionMenu.ShowPopupMenu();
				var operationalMenu = actionMenu.MenuItems.FindByText("Operational Actions");
				operationalMenu.ShowPopupMenu();
				var sentToCustomsMenu = operationalMenu.MenuItems.FindByText("Send To Customs");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					var operationalForm = shownForm as OperationalActionRunnerForm;
					AssertNotNull("Should open operational act form", operationalForm);
					var button = operationalForm.Controls.Find("topPanel", true)[0].Controls.Find("RunOnAllMatchingRecordsRadioButton", true)[0] as ZRadioButton;
					button.PerformClick();
					AssertEquals("Run \'Send To Customs\' on 2 jobs", operationalForm.FormHeading);
				});

				sentToCustomsMenu.PerformClick();
			}
		}

		[RequiresSTA]
		public void TestBulkSendToCustoms_InformationMessagePopsUpWhenClickingOK()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_JobReference = "IE123";
			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header2.AMA_JobReference = "IE2345";
			Factory.Save();

			using (var module = GetModule())
			using (var form = new ZForm())
			{
				var filterControl = (EUH7FilterStripControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				filterControl.Find();

				var grid = (ZDisplayGrid)module.DisplayGrid;
				grid.Select(0);

				var actionMenu = module.FormActionMenu.FindByText("&Actions");
				actionMenu.ShowPopupMenu();
				var operationalMenu = actionMenu.MenuItems.FindByText("Operational Actions");
				operationalMenu.ShowPopupMenu();
				var sentToCustomsMenu = operationalMenu.MenuItems.FindByText("Send To Customs");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
				{
					var operationalForm = shownForm as OperationalActionRunnerForm;
					AssertNotNull("Should open operational act form", operationalForm);
					operationalForm.PerformClickOk();
				});

				sentToCustomsMenu.PerformClick();

				CombineAssertions(() =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Send To Customs", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Are you sure you want to run this action on 1 job?", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}
	}
}
