using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CAMessagesChooserDialog))]
	sealed class CAMessagesChooserDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestSelectAll()
		{
			dialog.SelectAllInternal();
			AssertEquals(2, caChooser.SelectedManagers.Length);
		}

		public void TestDeselectAll()
		{
			dialog.SelectAllInternal();
			dialog.DeSelectAllInternal();
			AssertEquals(0, caChooser.SelectedManagers.Length);
		}

		protected override Form GetFormToBashCore() => new CAMessagesChooserDialog(caChooser);

		ACIForwarderCloseMessageManager closeMessageManager;
		ACIHouseBillMessageManager houseBillMessageManager;
		CAMessagesChooserDialog dialog;
		CAMessageChooserNonPersistent caChooser;

		protected override void SetUp()
		{
			base.SetUp();
			var cusCAeMHMaster = Factory.New<CusCAeMHMaster>();
			var cusCAeMHHouse = cusCAeMHMaster.HouseBills.AddNew();
			cusCAeMHHouse.BW_HouseCCN = "ccn";
			cusCAeMHHouse.BW_CustomsStatus = "val";
			var closeWrapper = new CusCAeMHMasterCloseWrapper(cusCAeMHMaster);
			closeMessageManager = new ACIForwarderCloseMessageManager(closeWrapper, null);
			houseBillMessageManager = new ACIHouseBillMessageManager(cusCAeMHHouse, null);
			caChooser = new CAMessageChooserNonPersistent(new SingleMessageManager[] { closeMessageManager, houseBillMessageManager }, "question", "Action", ActionPurpose.Origin);
			dialog = new CAMessagesChooserDialog(caChooser);
			var e = new QueryNewTreeNodeEventArgs(caChooser);
			dialog.MessagesTreeView_QueryNewTreeNode(this, e);
			dialog.MessagesTreeView.Nodes.Add(e.NewTreeNode);
		}

		protected override void TearDown()
		{
			base.TearDown();
			dialog.Dispose();
		}
	}
}
