using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class SupportingDocumentsSelectionDialog : ZChildForm
	{
		public SupportingDocumentsSelectionDialog()
		{
			InitializeComponent();
		}

		public SupportingDocumentsSelectionDialog(AsycudaManifestHeaderWrapper header)
			: base(header)
		{
			InitializeComponent();
			Text = ResString.GetMultilingualString("4B8C9597-BF8A-4837-AC25-8F5EFF33269C", "Send Supporting Documents");
		}

		AsycudaManifestHeaderWrapper AsycudaManifestHeaderWrapper => (AsycudaManifestHeaderWrapper)DataSource;

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
			DialogResult = DialogResult.Cancel;
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			try
			{
				ButtonPanel.Enabled = false;

				var hasErrors = false;

				if (AsycudaManifestHeaderWrapper.SupportingDocuments != null)
				{
					AsycudaManifestHeaderWrapper.ClearAllNotifications();
					AsycudaManifestHeaderWrapper.RunPreSaveValidation();
					hasErrors = AsycudaManifestHeaderWrapper.NotificationsIncludingChildren.HasErrors();
				}

				if (!hasErrors)
				{
					var notificationCollection = RunPreSendValidation();

					if (notificationCollection.Count == 0
						|| Globals.Message.Show(notificationCollection.NotificationsAsString(), Res.GetString("D6209FB3-EEAB-495E-BAFE-72F212811358", "Continue to Send"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						DialogResult = DialogResult.OK;
						Close();
					}
				}
				else
				{
					ShowErrorsDialog();
				}
			}
			finally
			{
				ButtonPanel.Enabled = true;
			}
		}

		MessageSendingNotificationCollection RunPreSendValidation()
		{
			var messageSendingNotificationCollection = new MessageSendingNotificationCollection();

			var asycudaManifestHeaderWrapperValidation = MessageSendingValidation.New(AsycudaManifestHeaderWrapper, null);
			messageSendingNotificationCollection.AddRange(asycudaManifestHeaderWrapperValidation.CheckBusinessObjectLevelValidation());

			foreach (var supportingDocumentWrapper in AsycudaManifestHeaderWrapper.SupportingDocuments.Cast<SupportingDocumentWrapper>().Where(w => w.IsSelected))
			{
				var messageSendingValidation = MessageSendingValidation.New(supportingDocumentWrapper.SupportingDocument, null);
				messageSendingNotificationCollection.AddRange(messageSendingValidation.CheckBusinessObjectLevelValidation());
			}

			return messageSendingNotificationCollection;
		}

		void DeselectAllButton_Click(object sender, EventArgs e)
		{
			UpdateIsSelected(DeselectAllButton, false);
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			UpdateIsSelected(SelectAllButton, true);
		}

		void UpdateIsSelected(Button button, bool isSelected)
		{
			try
			{
				ButtonPanel.Enabled = false;

				foreach (var item in AsycudaManifestHeaderWrapper.SupportingDocuments.Cast<SupportingDocumentWrapper>())
				{
					item.IsSelected = isSelected;
				}

				button.Select();
			}
			finally
			{
				ButtonPanel.Enabled = true;
			}
		}
	}
}
