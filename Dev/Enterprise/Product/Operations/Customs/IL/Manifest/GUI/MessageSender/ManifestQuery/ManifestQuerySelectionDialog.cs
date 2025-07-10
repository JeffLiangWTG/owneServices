using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class ManifestQuerySelectionDialog : ZChildForm
	{
		public ManifestQuerySelectionDialog()
		{
			InitializeComponent();
		}

		public ManifestQuerySelectionDialog(AsycudaManifestQueryMessageSendingObjectParent businessEntity) : base(businessEntity)
		{
			InitializeComponent();
			Text = ResString.GetMultilingualString("A16CACA9-F8D0-44E9-891C-02C16CA36BCB", "Send Manifest Query");
		}

		AsycudaManifestQueryMessageSendingObjectParent AsycudaManifestHeaderWrapper => (AsycudaManifestQueryMessageSendingObjectParent)DataSource;

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
				var notificationCollection = RunPreSendValidation();

				if (notificationCollection.Count > 0)
				{
					Globals.Message.ShowWarning(ResString.GetMultilingualString("199F3EE3-DF94-442F-8F54-7599D5B58727", "Manifest number and Parent Deal number (IL2) must have a value before sending"));
					DialogResult = DialogResult.Cancel;
				}
				else
				{
					DialogResult = DialogResult.OK;
				}
				Close();
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

			foreach (var manifestQueryMessageSendingObject in AsycudaManifestHeaderWrapper.SelectedSendingObjects.Cast<AsycudaManifestQueryMessageSendingObject>().Where(w => w.ShouldSend))
			{
				var messageSendingValidation = MessageSendingValidation.New(manifestQueryMessageSendingObject, null);
				messageSendingNotificationCollection.AddRange(messageSendingValidation.CheckBusinessObjectLevelValidation());
			}

			return messageSendingNotificationCollection;
		}
	}
}
