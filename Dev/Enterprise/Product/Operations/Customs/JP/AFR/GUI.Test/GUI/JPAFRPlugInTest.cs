using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	class JPAFRPlugInTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		[ExpectNoExceptions]
		public void TestDeleteConsol()
		{
			using (var plugIn = new JPAFRPlugIn(consol))
			{
				consol.Delete();
				Factory.Save();
			}
		}

		public void TestAFRMenuItemOnConsolStopWhenValidationFails()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.Japan);
			using (var form = new ZForm(consol))
			{
				var tabControl = new ZTabControl();
				form.Controls.Add(tabControl);
				tabControl.TabPages.Add(new TabPage());
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.Transports[0].JW_IsLinked = false;
				consol.Transports[0].JW_Vessel = "AVSDP";
				consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-1);
				consol.Transports[0].JW_ETA = ZDateTime.Today;
				form.PlugIns.Add(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
				form.Show();
				var plugIn = (JPAFRPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
				plugIn.Enabled = true;
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems.FindByText("Register Manifest", true);
				AssertNull(plugIn.Header);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NoHeaderNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				tabControl.SelectedIndex = 1;
				var header = plugIn.Header;
				AssertNotNull(header);
				var bill = header.Bills.AddNew();
				bill.JPB_BillNumber = "MD321";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, consol.IsInDatabase);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, consol.IsInDatabase);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();
				AssertNotEquals("One Message Sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, consol.IsInDatabase);
			}
		}

		public void TestAFRMenuItemOnConsolForceSendingDispiteValidationFailure()
		{
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SDDD", Core.Constants.CountryCodes.Japan);
			using (var form = new ZForm(consol))
			{
				var tabControl = new ZTabControl();
				tabControl.TabPages.Add(new TabPage());

				form.Controls.Add(tabControl);
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.Transports[0].JW_IsLinked = false;
				consol.Transports[0].JW_Vessel = "AVSDP";
				consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-1);
				consol.Transports[0].JW_ETA = ZDateTime.Today;
				form.PlugIns.Add(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
				form.Show();
				var plugIn = (JPAFRPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
				plugIn.Enabled = true;
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems.FindByText("Register Manifest", true);
				AssertNull(plugIn.Header);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.NoHeaderNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				tabControl.SelectedIndex = 1;
				var header = plugIn.Header;
				AssertNotNull(header);
				var bill = header.Bills.AddNew();
				bill.JPB_BillNumber = "MD321";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, consol.IsInDatabase);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendMenuItem.PerformClick();
				AssertEquals(AFRMainMenuItem.Constants.Message.DataNotSavedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, consol.IsInDatabase);

				header.JPH_MasterBillNumber = "SPQAVICTM002001";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenuItem.PerformClick();
				AssertEquals("Sent one Manifest Registration Message containing details of 1 Bill.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, consol.IsInDatabase);
			}
		}

		public void TestEnabledAndDisable()
		{
			using (var plugIn = new JPAFRPlugIn(consol))
			{
				consol.JK_RL_NKDischargePort = "JPTKI";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "AUSYD";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKDischargePort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKFirstForeignPort = "JPTKI";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKFirstForeignPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKLastForeignPort = "JPTKI";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKLastForeignPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.JK_RL_NKPortOfFirstArrival = "JPTKI";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_RL_NKPortOfFirstArrival = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "JPTKI";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "";
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "JPTZU";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
				AssertEquals("PlugIn is enabled", false, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "AUSYD";
				consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "JPTKI";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				var testLeg = consol.Transports.AddNew();
				testLeg.JW_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				testLeg.JW_RL_NKLoadPort = "JPTKI";
				testLeg.JW_RL_NKDiscPort = "JPTKY";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("PlugIn is enabled", false, plugIn.Enabled);
				consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
				consol.Transports.MostInterestingTransport.Delete();
				AssertEquals("PlugIn is enabled", false, plugIn.Enabled);
				consol.Transports.AddNew();
				consol.JK_RL_NKDischargePort = "JPTKI";
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
			}
		}

		#region Implementation

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			return new JPAFRPlugIn(consol);
		}

		ForwardingConsol consol;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
		}

		#endregion
	}
}
