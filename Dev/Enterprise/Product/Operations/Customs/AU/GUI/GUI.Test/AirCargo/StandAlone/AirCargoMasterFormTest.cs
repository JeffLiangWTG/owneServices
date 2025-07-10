using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(AirCargoMasterForm))]
	sealed class AirCargoMasterFormTest : ZFormBasherTest
	{
		public void TestRegisterHouseBillsAsEditableChild()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			using (AirCargoMasterForm form = new AirCargoMasterForm(mAWB))
			{
				AssertEquals("MAWB has House Bills as editable child", true, mAWB.IsRegisteredEditableChildObject(mAWB.ChildBills));
			}
		}

		public void TestUnderbondPlugin()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			using (AirCargoMasterForm form = new AirCargoMasterForm(Factory.New<CusMAWB>()))
			{
				AssertEquals("Should be only one plugin", 1, form.AirCargoDeclarationUserControl.MasterTabControl.PlugIns.Instances.Length);
				AssertEquals("Should be only one plugin", 1, form.AirCargoDeclarationUserControl.HAWBTabControl.PlugIns.Instances.Length);
			}
		}

		public void TestOnCurrentHouseBillChanged()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew();
			using (AirCargoMasterForm form = new AirCargoMasterForm(mAWB))
			{
				form.OnCurrentHouseBillChanged(hAWB);
				AssertEquals(hAWB, form.AirCargoDeclarationUserControl.HAWB);
			}
		}

		public void TestCMRControl()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			using (AirCargoMasterForm form = new AirCargoMasterForm(Factory.New<CusMAWB>()))
			{
				AssertEquals(typeof(CMRACAStandAloneUserControl), form.AirCargoDeclarationUserControl.GetType());
			}
		}

		public void TestShowPreSaveDialogWhenHouseMessageIsSent()
		{
			CusMAWB mAWB = GetMAWBHouseMessageSent();
			CusHAWB hAWB = mAWB.ChildBills[0];
			Factory.Save();
			hAWB.CS_ShipmentType = "AAA"; //does not affect messaging, but affects HasChanges
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			try
			{
				using (AirCargoMasterForm masterForm = new AirCargoMasterForm(mAWB))
				{
					AssertEquals("OK to save", ContinueWithSave.Yes, ((IShowPreSaveDialog)masterForm).ShowPreSaveDialogs());
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestAdviseUsersToCreateAHouseBillBeforeSave()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (AirCargoMasterForm masterForm = new AirCargoMasterForm(mAWB))
				{
					AssertEquals("Not OK to save", ContinueWithSave.No, ((IShowPreSaveDialog)masterForm).ShowPreSaveDialogs());
					AssertEquals("Advised Users To Create A HouseBill", true, UnitTestUserNotification.Instance.LastMessage.Contains(OneHouseBillMandatoryBeforeSavingMessage));
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestAdviseUsersToCreateAHouseBillBeforeSaveCMR()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			mAWB.CM_ArrivalDate = new ZDateTime(2006, 12, 12);
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (AirCargoMasterForm masterForm = new AirCargoMasterForm(mAWB))
				{
					AssertEquals("Not OK to save", ContinueWithSave.No, ((IShowPreSaveDialog)masterForm).ShowPreSaveDialogs());
					AssertEquals("Advised Users To Create A HouseBill", true, UnitTestUserNotification.Instance.LastMessage.Contains(OneHouseBillMandatoryBeforeSavingMessage));
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestManager()
		{
			CusMAWB mAWB = GetMAWBHouseMessageSent();
			using (AirCargoMasterForm masterForm = new AirCargoMasterForm(mAWB))
			{
				AssertEquals("ManagerType", typeof(CusMAWBMessageManager), masterForm.Manager.GetType());
			}
		}

		public void TestCustomFieldControls()
		{
			var mawb = Factory.New<CusMAWB>();
			using (var testForm = new AirCargoMasterForm(mawb))
			{
				testForm.Show();
				var mainTabControl = testForm.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = testForm.FindSingle<ZTabPage>("MainTabPage");
				mainTabControl.SelectTab(mainTabPage);
				var airCargoDeclarationUserControl = mainTabPage.FindSingle<Control>("AirCargoDeclarationUserControl");
				var houseBillsPanel = airCargoDeclarationUserControl.FindSingle<Control>("HouseBillsPanel");
				var masterTabControl = houseBillsPanel.FindSingle<ZTemplateTabControl>("MasterTabControl");
				var customFieldsTabPage = houseBillsPanel.FindSingle<ZTabPage>("CustomFieldsTabPage");
				masterTabControl.SelectTab(customFieldsTabPage);
				var airCargoCustomFieldsControl = customFieldsTabPage.FindSingle<Control>("AirCargoCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoCustomFieldsControl loaded", airCargoCustomFieldsControl.Visible);
				var houseBillsTabPage = houseBillsPanel.FindSingle<ZTabPage>("houseBillsTabPage");
				masterTabControl.SelectTab(houseBillsTabPage);
				var hawbTabControl = houseBillsTabPage.FindSingle<ZTemplateTabControl>("HAWBTabControl");
				var houseCustomFieldsTabPage = houseBillsTabPage.FindSingle<ZTabPage>("HouseCustomFieldsTabPage");
				hawbTabControl.SelectTab(houseCustomFieldsTabPage);
				var airCargoHouseCustomFieldsControl = houseCustomFieldsTabPage.FindSingle<Control>("AirCargoHouseCustomFieldsControl");
				Assert("Custom Fields tab is in tabs bar and has AirCargoHouseCustomFieldsControl loaded", airCargoHouseCustomFieldsControl.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var cusMAWB = factory.New<CusMAWB>();
			var result = new AirCargoMasterForm(cusMAWB);
			result.ControllerID = ControllerIDs.Customs.AU.AirCargo;
			return result;
		}

		CusMAWB GetMAWBHouseMessageSent()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = helper.CreateTestHouseBill(mAWB);
			CMRAIRCRMessage message = Factory.New<CMRAIRCRMessage>();
			hAWB.Messages.Add(message);
			message.EM_MessageType = CMRMessage.CMRMessageTypes.AIRCR;
			message.EM_ReceiveTransmit = "TRX";
			message.EM_Status = "QUE";
			message.EM_MessageText = Messaging.Business.EDIMessage.MessageNumberPlaceHolder;
			Factory.Save();
			return mAWB;
		}

		const string OneHouseBillMandatoryBeforeSavingMessage = "You should create at least one house bill before you save this record.";
	}
}
