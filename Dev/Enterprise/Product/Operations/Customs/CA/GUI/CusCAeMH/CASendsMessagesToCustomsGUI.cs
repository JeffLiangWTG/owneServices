using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public class CASendsMessagesToCustomsGUI : SendsMessagesToCustomsGUI, ICASendsMessagesToCustoms
	{
		public ActionPurpose FormActionPurpose { get; set; }

		protected override MessagesChooserDialog CreateMessageChooseDialog(MessageChooserNonPersistent chooser) => new CAMessagesChooserDialog(chooser);

		protected override Control CreateControlToBind(MessagesChooserDialog dialog)
		{
			var cAChooserDialog = dialog as CAMessagesChooserDialog;
			return cAChooserDialog != null ? cAChooserDialog.MessagesTreeView : base.CreateControlToBind(dialog);
		}

		protected override MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, ZString question) => new CAMessageChooserNonPersistent(managers, question, FormActionPurpose);

		protected override MessageChooserNonPersistent CreateMessageChooser(SingleMessageManager[] managers, ZString question, ZString action) => new CAMessageChooserNonPersistent(managers, question, action, FormActionPurpose);
	}
}
