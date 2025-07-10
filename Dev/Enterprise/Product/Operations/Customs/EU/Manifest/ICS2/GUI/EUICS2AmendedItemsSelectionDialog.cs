using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public partial class EUICS2AmendedItemsSelectionDialog : ZChildForm
	{
		public EUICS2AmendedItemsSelectionDialog()
		{
			InitializeComponent();
		}

		public EUICS2AmendedItemsSelectionDialog(ICS2AmendedItemsHeader header, string caption)
			: base(header)
		{
			this.header = Argument.NotNull(header, nameof(header));

			InitializeComponent();
			Text = ResString.GetMultilingualString("B5684081-1D8E-482A-9134-B1327DB53077", "Send {0}", caption.Replace("&", string.Empty));
		}

		readonly ICS2AmendedItemsHeader header;

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

				if (notificationCollection.Count <= 0
					|| Globals.Message.Show(notificationCollection.NotificationsAsString(), Res.GetString("FBD3AC1B-961D-4472-A5F0-F85E6331F3E2", "Continue to Send"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
			finally
			{
				ButtonPanel.Enabled = true;
			}
		}

		MessageSendingNotificationCollection RunPreSendValidation()
		{
			var messageSendingValidation = MessageSendingValidation.New(header, null);
			return messageSendingValidation.CheckBusinessObjectLevelValidation();
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

				foreach (ICS2AmendedItem item in header.AmendedItems)
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
