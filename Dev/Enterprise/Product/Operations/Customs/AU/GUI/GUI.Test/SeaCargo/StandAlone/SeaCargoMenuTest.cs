using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoMenuTest : TestCaseWithFactory
	{
		public void TestSeaCargoMenu()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoMenuForTest(manager))
			{
				menu.ShowPopupMenu();
				CombineAssertions(() =>
				{
					AssertEquals("MenuItems[0].Text", "Send &Underbond Requests", menu.MenuItems[0].Text);
					AssertEquals("MenuItems[1].Text", "&Send Message(s)", menu.MenuItems[1].Text);
					AssertEquals("MenuItems[2].Text", "&Amend Message(s)", menu.MenuItems[2].Text);
					AssertEquals("MenuItems[3].Text", "&Withdraw Message(s)", menu.MenuItems[3].Text);
					AssertEquals("MenuItems[4].Text", "&Reset to Original", menu.MenuItems[4].Text);
					AssertEquals("MenuItems[5].Text", "Messaging Problems? Click for HELP.", menu.MenuItems[5].Text);
				});
				AssertEquals("MenuItems.Count", 6, menu.MenuItems.Count);
			}
		}

		public void TestSendUnderbondRequests_NoUnderbonds()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoMenuForTest(manager))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.ShowPopupMenu();
				var sendUnderbondRequestsMenuItem = menu.MenuItems.FindByText("Send Underbond Requests");
				sendUnderbondRequestsMenuItem.PerformClick();
				AssertEquals("No new messages generated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendUnderbondRequests_InvalidUnderbond()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var container = oceanBill.Containers.AddNew();
			var underbond = ((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoMenuForTest(manager))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.ShowPopupMenu();
				var sendUnderbondRequestsMenuItem = menu.MenuItems.FindByText("Send Underbond Requests");
				sendUnderbondRequestsMenuItem.PerformClick();
				AssertStartsWith("LastMessage", "You may not continue due to one or more critical problems:", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendUnderbondRequests_ValidUnderbond()
		{
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "67094168242");
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "OB1";
			oceanBill.CB_PrincipalID = "67094168242";
			oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
			oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
			oceanBill.CB_LloydsIMO = "8811924";
			oceanBill.CB_Voyage = "AA123";
			var container = oceanBill.Containers.AddNew();
			var underbond = ((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();
			underbond.C4_OriginPremiseID = "9532M";
			underbond.C4_DestinationPremiseID = "DP41B";
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoMenuForTest(manager))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.ShowPopupMenu();
				var sendUnderbondRequestsMenuItem = menu.MenuItems.FindByText("Send Underbond Requests");
				sendUnderbondRequestsMenuItem.PerformClick();
				AssertEquals("1 original message has been generated.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		sealed class SeaCargoMenuForTest : SeaCargoMenu
		{
			public SeaCargoMenuForTest(SeaCargoMessageManager messageManager) : base(messageManager)
			{
			}
		}
	}
}
