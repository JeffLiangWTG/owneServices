using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCOLSAttachmentsSelectionForm : ZChildForm
	{
		public AUCOLSAttachmentsSelectionForm(CusStorageDocPivotMessageSendingActionParent messageSendingActionParent, bool mustSelectOneOrMoreAttachments) : base(messageSendingActionParent)
		{
			InitializeComponent();
			MustSelectOneOrMoreAttachments = mustSelectOneOrMoreAttachments;
			if (MustSelectOneOrMoreAttachments)
			{
				messageSendingActionParent.SelectedSendingObjectsChanged += SendingObjectParent_SelectedSendingObjectsChanged;
				EnableOrDisableSendBoundButton();
			}
		}

		public bool MustSelectOneOrMoreAttachments { get; }

		public CusStorageDocPivotMessageSendingActionParent MessageSendingObjectParent => BusinessEntity as CusStorageDocPivotMessageSendingActionParent;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MessageSendingObjectParent.SelectedSendingObjectsChanged -= SendingObjectParent_SelectedSendingObjectsChanged;
			}
			base.Dispose(disposing);
		}

		void SendingObjectParent_SelectedSendingObjectsChanged(object sender, System.EventArgs e)
		{
			EnableOrDisableSendBoundButton();
		}

		void EnableOrDisableSendBoundButton()
		{
			SendBoundButton.Enabled = MessageSendingObjectParent.SelectedSendingObjects.Any();
		}

		void SendBoundButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelBoundButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
