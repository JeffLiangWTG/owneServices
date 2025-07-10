using ExternalMailManager = MailManager.Module;

namespace Enterprise.MailManager.GUI
{
	public partial class MailItemCopySenderForm
	{
		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.MailAddressToSendCopyToGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MailAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SendCopyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessagesToSendLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NoMessagesToSendLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MailAddressToSendCopyToGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 92, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 23, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MailManager.Business.MailItemCopySender);
			//
			// MailAddressToSendCopyToGroupBox
			//
			this.MailAddressToSendCopyToGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.MailAddressToSendCopyToGroupBox.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemCopySenderForm|3bf986a9-85df-4000-b519-3c43d9a34bf1", "Mail Address To Send A Copy To");
			this.MailAddressToSendCopyToGroupBox.Controls.Add(this.MailAddressTextBox);
			this.MailAddressToSendCopyToGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.MailAddressToSendCopyToGroupBox.Name = "MailAddressToSendCopyToGroupBox";
			this.MailAddressToSendCopyToGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 45, true);
			this.MailAddressToSendCopyToGroupBox.TabIndex = 0;
			this.MailAddressToSendCopyToGroupBox.TabStop = false;
			//
			// MailAddressTextBox
			//
			this.MailAddressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MailAddressTextBox, "MailAddressToSendCopyTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItemCopySender)(null)).MailAddressToSendCopyTo)));
			this.MailAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MailAddressTextBox, false);
			this.MailAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.MailAddressTextBox.Name = "MailAddressTextBox";
			this.MailAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.MailAddressTextBox.TabIndex = 0;
			//
			// SendCopyButton
			//
			this.SendCopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendCopyButton.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemCopySenderForm|93607fe3-2505-4e60-bd66-36cf6509369b", "&Send Copy");
			this.SendCopyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SendCopyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 59, true);
			this.SendCopyButton.Name = "SendCopyButton";
			this.SendCopyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.SendCopyButton.TabIndex = 1;
			//
			// MessagesToSendLabel
			//
			this.MessagesToSendLabel.CaptionResourceString = ExternalMailManager.Res.GetData("MailItemCopySenderForm|8d71a241-7bfb-4712-b1d9-7be527099ebd", "Messages To Send");
			this.MessagesToSendLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 59, true);
			this.MessagesToSendLabel.Name = "MessagesToSendLabel";
			this.MessagesToSendLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.MessagesToSendLabel.TabIndex = 5;
			//
			// NoMessagesToSendLabel1
			//
			this.BindingSource.SetBindingMember(this.NoMessagesToSendLabel1, "MessagesToSend");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItemCopySender)(null)).MessagesToSend)));
			this.NoMessagesToSendLabel1.IsFontBold = true;
			this.NoMessagesToSendLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 59, true);
			this.NoMessagesToSendLabel1.Name = "NoMessagesToSendLabel1";
			this.NoMessagesToSendLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 22, true);
			this.NoMessagesToSendLabel1.TabIndex = 6;
			//
			// MailItemCopySenderForm
			//

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 115, true);
			this.Controls.Add(this.NoMessagesToSendLabel1);
			this.Controls.Add(this.MessagesToSendLabel);
			this.Controls.Add(this.SendCopyButton);
			this.Controls.Add(this.MailAddressToSendCopyToGroupBox);
			this.DataSourceAssemblyName = "MailManager";
			this.DataSourceType = typeof(Enterprise.MailManager.Business.MailItemCopySender);
			this.DataSourceTypeName = "Enterprise.MailManager.Business.MailItemCopySender";
			this.Name = "MailItemCopySenderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MailAddressToSendCopyToGroupBox, 0);
			this.Controls.SetChildIndex(this.SendCopyButton, 0);
			this.Controls.SetChildIndex(this.MessagesToSendLabel, 0);
			this.Controls.SetChildIndex(this.NoMessagesToSendLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MailAddressToSendCopyToGroupBox.ResumeLayout(false);
			this.MailAddressToSendCopyToGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox MailAddressToSendCopyToGroupBox;
		Enterprise.ZArchitecture.ZTextBox MailAddressTextBox;
		Enterprise.ZArchitecture.GUI.ZButton SendCopyButton;
		Enterprise.ZArchitecture.ZLabel MessagesToSendLabel;
		Enterprise.ZArchitecture.ZLabel NoMessagesToSendLabel1;
	}
}
