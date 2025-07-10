namespace Enterprise.Customs.CA.GUI
{
	partial class MessageInstructionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IsContinueWithAWaitingForResponseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsContinueWithValidationErrorsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsContinueWithAdditionalWarningsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BillingJobReadyForPostingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NotificatoinsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SecurityNotAllowedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AdditionalWarningsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTextBoxSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageTextBoxSplitContainer)).BeginInit();
			this.MessageTextBoxSplitContainer.Panel1.SuspendLayout();
			this.MessageTextBoxSplitContainer.Panel2.SuspendLayout();
			this.MessageTextBoxSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 345, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.MessageInstruction);
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b08c3e55-b57f-4c6b-ab44-2d9a0380843e", "&Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 311, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 6;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// zCancelButton
			// 
			this.zCancelButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("de51aa84-3293-49e3-8553-f2d69f25156a", "&Cancel");
			this.zCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(484, 311, true);
			this.zCancelButton.Name = "zCancelButton";
			this.zCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zCancelButton.TabIndex = 7;
			this.zCancelButton.UseVisualStyleBackColor = true;
			this.zCancelButton.Click += new System.EventHandler(this.zCancelButton_Click);
			// 
			// IsContinueWithAWaitingForResponseCheckBox
			// 
			this.IsContinueWithAWaitingForResponseCheckBox.AutoSize = true;
			this.IsContinueWithAWaitingForResponseCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c9014456-317a-4577-902b-63822db5f511", "Continue to send even though the job is waiting for a CBSA response, or has a scheduled message?");
			this.IsContinueWithAWaitingForResponseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContinueWithAWaitingForResponseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 255, true);
			this.IsContinueWithAWaitingForResponseCheckBox.Name = "IsContinueWithAWaitingForResponseCheckBox";
			this.IsContinueWithAWaitingForResponseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 16, true);
			this.IsContinueWithAWaitingForResponseCheckBox.TabIndex = 3;
			this.IsContinueWithAWaitingForResponseCheckBox.UseVisualStyleBackColor = true;
			this.IsContinueWithAWaitingForResponseCheckBox.CheckedChanged += new System.EventHandler(this.UserConfirmationCheckBox_CheckedChanged);
			// 
			// IsContinueWithValidationErrorsCheckBox
			// 
			this.IsContinueWithValidationErrorsCheckBox.AutoSize = true;
			this.IsContinueWithValidationErrorsCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a26f3abb-7a4a-412d-a576-79f4db3be2ca", "Continue to send even though the job contains validation errors?");
			this.IsContinueWithValidationErrorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContinueWithValidationErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 273, true);
			this.IsContinueWithValidationErrorsCheckBox.Name = "IsContinueWithValidationErrorsCheckBox";
			this.IsContinueWithValidationErrorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 16, true);
			this.IsContinueWithValidationErrorsCheckBox.TabIndex = 4;
			this.IsContinueWithValidationErrorsCheckBox.UseVisualStyleBackColor = true;
			this.IsContinueWithValidationErrorsCheckBox.CheckedChanged += new System.EventHandler(this.UserConfirmationCheckBox_CheckedChanged);
			// 
			// IsContinueWithAdditionalWarningsCheckBox
			// 
			this.IsContinueWithAdditionalWarningsCheckBox.AutoSize = true;
			this.IsContinueWithAdditionalWarningsCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("539d2f2d-6f99-482a-97aa-b34d28326ac8", "Continue to send even though the job has additional warnings?");
			this.IsContinueWithAdditionalWarningsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContinueWithAdditionalWarningsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 291, true);
			this.IsContinueWithAdditionalWarningsCheckBox.Name = "IsContinueWithAdditionalWarningsCheckBox";
			this.IsContinueWithAdditionalWarningsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 16, true);
			this.IsContinueWithAdditionalWarningsCheckBox.TabIndex = 5;
			this.IsContinueWithAdditionalWarningsCheckBox.UseVisualStyleBackColor = true;
			this.IsContinueWithAdditionalWarningsCheckBox.CheckedChanged += new System.EventHandler(this.UserConfirmationCheckBox_CheckedChanged);
			// 
			// BillingJobReadyForPostingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.BillingJobReadyForPostingCheckBox, "BaseJobDeclaration+CA_JobReadyForPost");
			this.BillingJobReadyForPostingCheckBox.AutoSize = true;
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.MessageInstruction)(null)).BaseJobDeclaration.CA_JobReadyForPost)));
			this.BillingJobReadyForPostingCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0E691072-B9FC-4B07-8CA1-6B37EF1BFEDA", "Billing Job Ready for Posting?");
			this.BillingJobReadyForPostingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BillingJobReadyForPostingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 309, true);
			this.BillingJobReadyForPostingCheckBox.Name = "BillingJobReadyForPostingCheckBox";
			this.BillingJobReadyForPostingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 16, true);
			this.BillingJobReadyForPostingCheckBox.TabIndex = 6;
			this.BillingJobReadyForPostingCheckBox.UseVisualStyleBackColor = true;
			this.BillingJobReadyForPostingCheckBox.CheckedChanged += new System.EventHandler(this.UserConfirmationCheckBox_CheckedChanged);
			// 
			// NotificatoinsTextBox
			// 
			this.NotificatoinsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NotificatoinsTextBox, "ValidationErrorsMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.MessageInstruction)(null)).ValidationErrorsMessage)));
			this.NotificatoinsTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6cb37163-0a41-4f8c-b7aa-2f2c9d9b6822", "Validation Errors");
			this.NotificatoinsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.NotificatoinsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.NotificatoinsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 17, true);
			this.NotificatoinsTextBox.Multiline = true;
			this.NotificatoinsTextBox.Name = "NotificatoinsTextBox";
			this.NotificatoinsTextBox.ReadOnly = true;
			this.NotificatoinsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.NotificatoinsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 106, true);
			this.NotificatoinsTextBox.TabIndex = 1;
			// 
			// SecurityNotAllowedLabel
			// 
			this.SecurityNotAllowedLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7b342155-deb5-4a0c-b1ca-788f04002094", "Your current security settings do not allow you to send with message errors");
			this.SecurityNotAllowedLabel.ForeColor = System.Drawing.Color.Red;
			this.SecurityNotAllowedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 271, true);
			this.SecurityNotAllowedLabel.Name = "SecurityNotAllowedLabel";
			this.SecurityNotAllowedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 35, true);
			this.SecurityNotAllowedLabel.TabIndex = 8;
			// 
			// AdditionalWarningsTextBox
			// 
			this.AdditionalWarningsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AdditionalWarningsTextBox, "AdditionalWarningsMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.MessageInstruction)(null)).AdditionalWarningsMessage)));
			this.AdditionalWarningsTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b33b46c3-0541-4877-834f-9f8f36c28997", "Additional Warnings");
			this.AdditionalWarningsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AdditionalWarningsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.AdditionalWarningsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 22, true);
			this.AdditionalWarningsTextBox.Multiline = true;
			this.AdditionalWarningsTextBox.Name = "AdditionalWarningsTextBox";
			this.AdditionalWarningsTextBox.ReadOnly = true;
			this.AdditionalWarningsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 98, true);
			this.AdditionalWarningsTextBox.TabIndex = 2;
			// 
			// MessageTextBoxSplitContainer
			// 
			this.MessageTextBoxSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 0, true);
			this.MessageTextBoxSplitContainer.Name = "MessageTextBoxSplitContainer";
			this.MessageTextBoxSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MessageTextBoxSplitContainer.Panel1
			// 
			this.MessageTextBoxSplitContainer.Panel1.Controls.Add(this.NotificatoinsTextBox);
			this.MessageTextBoxSplitContainer.Panel1MinSize = 100;
			// 
			// MessageTextBoxSplitContainer.Panel2
			// 
			this.MessageTextBoxSplitContainer.Panel2.Controls.Add(this.AdditionalWarningsTextBox);
			this.MessageTextBoxSplitContainer.Panel2MinSize = 100;
			this.MessageTextBoxSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 249, true);
			this.MessageTextBoxSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(125);
			this.MessageTextBoxSplitContainer.TabIndex = 0;
			// 
			// MessageInstructionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3c766149-938f-4b30-bd10-5ca52f656fb1", "Message Sending Confirmation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 369, true);
			this.Controls.Add(this.MessageTextBoxSplitContainer);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.zCancelButton);
			this.Controls.Add(this.IsContinueWithAWaitingForResponseCheckBox);
			this.Controls.Add(this.SecurityNotAllowedLabel);
			this.Controls.Add(this.IsContinueWithValidationErrorsCheckBox);
			this.Controls.Add(this.IsContinueWithAdditionalWarningsCheckBox);
			this.Controls.Add(this.BillingJobReadyForPostingCheckBox);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.MessageInstruction);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "MessageInstructionForm";
			this.Load += new System.EventHandler(this.MessageInstructionForm_Load);
			this.Controls.SetChildIndex(this.BillingJobReadyForPostingCheckBox, 0);
			this.Controls.SetChildIndex(this.IsContinueWithAdditionalWarningsCheckBox, 0);
			this.Controls.SetChildIndex(this.IsContinueWithValidationErrorsCheckBox, 0);
			this.Controls.SetChildIndex(this.SecurityNotAllowedLabel, 0);
			this.Controls.SetChildIndex(this.IsContinueWithAWaitingForResponseCheckBox, 0);
			this.Controls.SetChildIndex(this.zCancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MessageTextBoxSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTextBoxSplitContainer.Panel1.ResumeLayout(false);
			this.MessageTextBoxSplitContainer.Panel1.PerformLayout();
			this.MessageTextBoxSplitContainer.Panel2.ResumeLayout(false);
			this.MessageTextBoxSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageTextBoxSplitContainer)).EndInit();
			this.MessageTextBoxSplitContainer.ResumeLayout(false);
			this.MessageTextBoxSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		#endregion

		private ZArchitecture.ZTextBox NotificatoinsTextBox;
		private ZArchitecture.ZTextBox AdditionalWarningsTextBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsContinueWithAWaitingForResponseCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsContinueWithValidationErrorsCheckBox;
		private ZArchitecture.ZLabel SecurityNotAllowedLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsContinueWithAdditionalWarningsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox BillingJobReadyForPostingCheckBox;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZButton zCancelButton;
		private CargoWise.Windows.UI.KSplitContainer MessageTextBoxSplitContainer;
	}
}
