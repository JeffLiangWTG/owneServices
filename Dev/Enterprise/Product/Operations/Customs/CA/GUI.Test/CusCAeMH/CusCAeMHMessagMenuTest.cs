using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CusCAeMHMessagMenuTest : TestCaseWithFactory
	{
		public void TestRefreshAllMenuItem()
		{
			var consol = Factory.New<ForwardingConsol>();
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_ParentID = consol.PK;

			var manager = new ACIHouseBillMultiMessageManagerTest(master);
			using (var menu = new CusCAeMHMessageMenuHelper(manager))
			{
				menu.OnPopupExposed(null);
				var refreshAllMenuItem = menu.MenuItems.FindByText("Refresh eManifest Data");
				AssertNotNull(refreshAllMenuItem);
			}
		}

		public void TestForcedMessages()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "8036X666";
			var house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house.Items.AddNew();
			house.BW_HouseCCN = "8036X555";
			house.BW_Weight = 1;

			var manager = new ACIHouseBillMultiMessageManagerTest(master);
			using (var form = new CusCAeMHMasterForm(master))
			using (var menu = new CusCAeMHMessageMenuHelper(manager))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopupExposed(EventArgs.Empty);
				var menuItem = menu.MenuItems.FindByText("Forced Messages").MenuItems.FindByText("Send Original Messages");
				menuItem.PerformClick();
				AssertEquals("A message created on house", 1, house.Messages.Count);
				Assert("Is of correct sub-type", house.Messages[0].EM_MessageText.Contains("BGM+714+8036X555+9'"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem = menu.MenuItems.FindByText("Forced Messages").MenuItems.FindByText("Send Change Messages");
				menuItem.PerformClick();
				AssertEquals("A message created on house", 2, house.Messages.Count);
				Assert("Is of correct sub-type", house.Messages[1].EM_MessageText.Contains("BGM+714+8036X555+4'"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItem = menu.MenuItems.FindByText("Forced Messages").MenuItems.FindByText("Send Post-arrival Change Messages");
				menuItem.PerformClick();
				AssertEquals("A message created on house", 3, house.Messages.Count);
				Assert("Is of correct sub-type", house.Messages[2].EM_MessageText.Contains("BGM+714+8036X555+52'"));
			}
		}

		public void TestCloseMessageMenuItems()
		{
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDXX");
			var master = Factory.New<CusCAeMHMaster>();
			master.BP_PrimaryCCN = "8036X666";
			var house = master.HouseBills.AddNew();
			house.BW_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			house.Items.AddNew();
			house.BW_HouseCCN = "8036X555";
			house.BW_Weight = 1;

			var manager = new ACIHouseBillMultiMessageManagerTest(master);
			using (var form = new ZForm(master))
			using (var menu = new CusCAeMHMessageMenuHelper(manager))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopupExposed(EventArgs.Empty);
				var menuItem = menu.MenuItems.FindByText("Send Close Message(s)");
				menuItem.PerformClick();
				AssertEquals("A message created on master", 1, master.Messages.Count);
				Assert("Is of correct sub-type", master.Messages[0].EM_MessageText.Contains("BGM+87+8036X666+9'"));

				menuItem.PerformClick();
				AssertEquals("A message created on master", 1, master.Messages.Count);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("There are messages waiting for responses, please do not send it again."));

				AssertNotNull(menu.MenuItems.FindByText("Forced Messages").MenuItems.FindByText("Send Original Close Messages"));
				AssertNotNull(menu.MenuItems.FindByText("Forced Messages").MenuItems.FindByText("Send Change Close Messages"));
				AssertNotNull(menu.MenuItems.FindByText("Forced Messages").MenuItems.FindByText("Send Post Arrival Change Close Messages"));

				master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.OnPopupExposed(EventArgs.Empty);
				menuItem = menu.MenuItems.FindByText("Forced Messages").MenuItems.FindByText("Withdraw Close Messages");
				menuItem.PerformClick();
				AssertEquals("A message created on master", 2, master.Messages.Count);
				Assert("Is of correct sub-type", master.Messages[1].EM_MessageText.Contains("BGM+87+8036X666+1'"));
			}
		}

		sealed class CusCAeMHMessageMenuHelper : CusCAeMHMessageMenu
		{
			public CusCAeMHMessageMenuHelper(ACIHouseBillMultiMessageManager messageManager)
				: base(messageManager)
			{
			}

			internal ACIHouseBillMultiMessageManager ManagerExposed => Manager;

			internal void OnPopupExposed(EventArgs e) => OnPopup(e);
		}
	}
}
