using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CASendsMessagesToCustomsGUITest : TestCaseWithFactory
	{
		public void TestCreateMessageChooseDialog()
		{
			using (var dialog = SendsMessagesToCustomsGUI.CreateMessageChooseDialog(Chooser))
			{
				AssertNotNull(dialog as CAMessagesChooserDialog);
			}
		}

		public void TestCreateControlToBind()
		{
			using (var dialog = SendsMessagesToCustomsGUI.CreateMessageChooseDialog(Chooser))
			{
				var control = SendsMessagesToCustomsGUI.CreateControlToBind(dialog);
				AssertNotNull(control as ZArchitecture.GUI.ZTreeView);
			}
		}

		public void TestCreateMessageChooser()
		{
			var chooser = SendsMessagesToCustomsGUI.CreateMessageChooser(Chooser.Managers, Chooser.Question);
			AssertNotNull(chooser as CAMessageChooserNonPersistent);
			chooser = SendsMessagesToCustomsGUI.CreateMessageChooser(Chooser.Managers, Chooser.Question, Chooser.Action);
			AssertNotNull(chooser as CAMessageChooserNonPersistent);
		}

		CASendsMessagesToCustomsGUIForTest sendsMessagesToCustomsGUI;
		CASendsMessagesToCustomsGUIForTest SendsMessagesToCustomsGUI => sendsMessagesToCustomsGUI ?? (sendsMessagesToCustomsGUI = new CASendsMessagesToCustomsGUIForTest());

		MessageChooserNonPersistent chooser;
		MessageChooserNonPersistent Chooser
		{
			get
			{
				if (chooser == null)
				{
					var cusCAeMHMaster = Factory.New<CusCAeMHMaster>();
					var cusCAeMHHouse = cusCAeMHMaster.HouseBills.AddNew();
					cusCAeMHHouse.BW_HouseCCN = "ccn";
					var closeWrapper = new CusCAeMHMasterCloseWrapper(cusCAeMHMaster);
					var closeMessageManager = new ACIForwarderCloseMessageManager(closeWrapper, null);
					var houseBillMessageManager = new ACIHouseBillMessageManager(cusCAeMHHouse, null);
					chooser = new MessageChooserNonPersistent(new SingleMessageManager[] { closeMessageManager, houseBillMessageManager }, "question", "Action");
				}

				return chooser;
			}
		}

		sealed class CASendsMessagesToCustomsGUIForTest : CASendsMessagesToCustomsGUI
		{
			internal new MessagesChooserDialog CreateMessageChooseDialog(MessageChooserNonPersistent chooser) => base.CreateMessageChooseDialog(chooser);

			internal new Control CreateControlToBind(MessagesChooserDialog dialog) => base.CreateControlToBind(dialog);

			internal new MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, ZString question) => base.CreateMessageChooser(managers, question);

			internal new MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, ZString question, ZString action) => base.CreateMessageChooser(managers, question, action);
		}
	}
}
