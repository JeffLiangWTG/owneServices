using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class ShipmentHeldLetterForm : ZChildForm
	{
		public ShipmentHeldLetterForm()
		{
		}

		public ShipmentHeldLetterForm(ShipmentHeldLetterBusinessObject businessEntity, ShipmentHeldLetterRecipient recipient)
			: base(businessEntity)
		{
			this.Recipient = recipient;
			businessEntity.RecipientForValidation = recipient;
			businessEntity.ClearAllNotifications();

			UPEPrintBatch latestPrintBatch = new UPEPrintBatch.Loader(businessEntity.Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			QueueForBatchPrintBoundCheckBox.Text += " (Batch #" + latestPrintBatch.T7_BatchNumber + ")";
			SetupLayout();
		}

		public readonly ShipmentHeldLetterRecipient Recipient;

		public override string FormHeading
		{
			get { return "Customer Notification"; }
		}

		public new ShipmentHeldLetterBusinessObject BusinessEntity
		{
			get { return (ShipmentHeldLetterBusinessObject)base.BusinessEntity; }
		}

		#region SetupLayout

		void SetupLayout()
		{
			SetHeadingLabelText();
			if (Recipient == ShipmentHeldLetterRecipient.Unknown)
			{
				HideDeliveryMethodGroupBox();
			}
		}

		void SetHeadingLabelText()
		{
			switch (Recipient)
			{
				case ShipmentHeldLetterRecipient.Consignee: HeadingLabel.Text = "Customer Notification for Consignee"; break;
				case ShipmentHeldLetterRecipient.Consignor: HeadingLabel.Text = "Customer Notification for Consignor"; break;
				default: HeadingLabel.Text = "Please confirm Customer Notification details"; break;
			}
		}

		void HideDeliveryMethodGroupBox()
		{
			ControlDpiScalingHelper.SetHeight(this, Height - DeliveryMethodGroupBox.Height, false);
			DeliveryMethodGroupBox.Dispose();
		}

		#endregion

		#region Commit or Revert User Changes

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BusinessEntity.SaveToNote();

			if (Recipient == ShipmentHeldLetterRecipient.Unknown)
			{
				BusinessEntity.SetDefaultsForAutoDelivery();
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			if (DialogResult == DialogResult.Cancel)
			{
				BusinessEntity.LoadFromNote();
			}
			else
			{
				BusinessEntity.SaveToNote();
			}
		}

		#endregion

		#region Implementation

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool result = false;
			if (keyData == (Keys.Control | Keys.Enter))
			{
				OnOKButton_Click(this, EventArgs.Empty);
				result = true;
			}
			else
			{
				result = base.ProcessCmdKey(ref msg, keyData);
			}
			return result;
		}

		void OnOKButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
