using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(NEXDOCNotificationModule))]
	sealed class NEXDOCNotificationModuleTest : ZModuleBasherTest
	{
		public void TestModule()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as NEXDOCNotificationModule)
			{
				Assert("Has actions", module.HasActions);
				AssertEquals("Licence Check Point", Env.Licence.Broker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.NEXDOCReferenceFile, module.SecurityCheckpoint);
				AssertEquals(false, module.AllowDelete & module.AllowEdit & module.AllowNew);
				AssertEquals("Not allow to view/edit/delete", false, module.AllowDelete & module.AllowEdit & module.AllowView);
			}
		}

		public void TestAcknowledgeMenuItem()
		{
			var notification1 = Factory.New<QuarantineNexDocNotification>();
			notification1.QN_AcknowledgeStatus = "ACC";
			notification1.QN_SystemCreateTimeUtc = ZDateTime.Now;
			var notification2 = Factory.New<QuarantineNexDocNotification>();
			notification2.QN_AcknowledgeStatus = "NOT";
			notification2.QN_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			using (var form = new ZForm())
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as NEXDOCNotificationModule)
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				((ModuleTextFilter)module.FilterBusinessObject.ActiveModuleFilters[0]).Property = "";
				module.PerformSearch_ForTest();
				var acknowledgeMenuItem = module.ActionsMenuItem.MenuItems.FindByText("Acknowledge Forward/Transfer Request");
				acknowledgeMenuItem.PerformClick();
				AssertEquals("Should have shown not-selected message.", "Please select a notification to acknowledge.", UnitTestUserNotification.Instance.LastMessage.Text);
				module.DisplayGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				acknowledgeMenuItem.PerformClick();
				AssertEquals("Should have shown multi-select message.", "Please select only one notification.", UnitTestUserNotification.Instance.LastMessage.Text);
				module.DisplayGrid.UnSelectAll();
				module.DisplayGrid.Select(0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				acknowledgeMenuItem.PerformClick();
				AssertEquals("Should have shown Acknowledge status incorrect message.", "Acknowledge status must be NOT - Not Actioned or ERR - Error.", UnitTestUserNotification.Instance.LastMessage.Text);
				module.DisplayGrid.UnSelectAll();
				module.DisplayGrid.Select(1);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				acknowledgeMenuItem.PerformClick();
				Assert("Should have shown NEXDOCAcknowledgeForm.", ZFormModaliser.LastFormShownDialogForTest is NEXDOCAcknowledgeForm);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.NexDocNotifications;

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}
