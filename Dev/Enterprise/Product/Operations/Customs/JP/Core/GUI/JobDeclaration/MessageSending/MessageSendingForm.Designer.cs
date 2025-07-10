namespace Enterprise.Customs.JP.GUI
{
	partial class MessageSendingForm
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
			this.ValidationErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValidationErrorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalWarningsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalWarningsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContinueToSendCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportPathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportPathButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValidationErrorsGroupBox.SuspendLayout();
			this.AdditionalWarningsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(591, 551, true);
			this.SendButton.TabIndex = 11;
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(685, 551, true);
			this.CancelButton2.TabIndex = 12;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 222, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 203, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 580, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.DeclarationMessageSendingObjectParent);
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ValidationErrorsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("ef32af14-1234-4b5e-a232-a94614e89e8a", "Validation Errors");
			this.ValidationErrorsGroupBox.Controls.Add(this.ValidationErrorsTextBox);
			this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 238, true);
			this.ValidationErrorsGroupBox.Name = "ValidationErrorsGroupBox";
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 129, true);
			this.ValidationErrorsGroupBox.TabIndex = 4;
			this.ValidationErrorsGroupBox.TabStop = false;
			// 
			// ValidationErrorsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValidationErrorsTextBox, "BizObjValidationMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.DeclarationMessageSendingObjectParent)(null)).BizObjValidationMessageErrors)));
			this.ValidationErrorsTextBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("26d565ea-7bfb-461a-8fa1-615807572ca9", "Validation Errors");
			this.ValidationErrorsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ValidationErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ValidationErrorsTextBox.Multiline = true;
			this.ValidationErrorsTextBox.Name = "ValidationErrorsTextBox";
			this.ValidationErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 110, true);
			this.ValidationErrorsTextBox.TabIndex = 0;
			// 
			// AdditionalWarningsGroupBox
			// 
			this.AdditionalWarningsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalWarningsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("e14001ad-2226-42a3-bbd3-13077797d656", "Additional Warnings");
			this.AdditionalWarningsGroupBox.Controls.Add(this.AdditionalWarningsTextBox);
			this.AdditionalWarningsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 373, true);
			this.AdditionalWarningsGroupBox.Name = "AdditionalWarningsGroupBox";
			this.AdditionalWarningsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 145, true);
			this.AdditionalWarningsGroupBox.TabIndex = 5;
			this.AdditionalWarningsGroupBox.TabStop = false;
			// 
			// AdditionalWarningsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalWarningsTextBox, "AdditionalWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.DeclarationMessageSendingObjectParent)(null)).AdditionalWarnings)));
			this.AdditionalWarningsTextBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("e0f9329c-71d1-43d7-b8bb-8b31c196cf26", "Additional Warnings");
			this.AdditionalWarningsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalWarningsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalWarningsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalWarningsTextBox.Multiline = true;
			this.AdditionalWarningsTextBox.Name = "AdditionalWarningsTextBox";
			this.AdditionalWarningsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 126, true);
			this.AdditionalWarningsTextBox.TabIndex = 0;
			// 
			// ContinueToSendCheckBox
			// 
			this.ContinueToSendCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ContinueToSendCheckBox, "AllowSendWithError");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Business.DeclarationMessageSendingObjectParent)(null)).AllowSendWithError)));
			this.ContinueToSendCheckBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("95df8f9e-a7da-4e45-b64f-130e69742e8e", "Continue to send/export even though the selected message(s) contains validation errors?");
			this.ContinueToSendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 551, true);
			this.ContinueToSendCheckBox.Name = "ContinueToSendCheckBox";
			this.ContinueToSendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 22, true);
			this.ContinueToSendCheckBox.TabIndex = 6;
			this.ContinueToSendCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExportPathTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportPathTextBox, "ExportPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.DeclarationMessageSendingObjectParent)(null)).ExportPath)));
			this.ExportPathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 525, true);
			this.ExportPathTextBox.Name = "ExportPathTextBox";
			this.ExportPathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 20, true);
			this.ExportPathTextBox.TabIndex = 8;
			this.ExportPathTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			// 
			// ExportPathButton
			//
			this.ExportPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportPathButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("EC686999-E3EF-447A-8343-65525327FFE0", "Choose");
			this.ExportPathButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(685, 524, true);
			this.ExportPathButton.Name = "ExportPathButton";
			this.ExportPathButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ExportPathButton.TabIndex = 9;
			this.ExportPathButton.ToolTipCaption = null;
			// 
			// ExportButton
			//
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportButton.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("e5970734-24cc-4a5e-83f4-dc6646ccd006", "Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(497, 551, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ExportButton.TabIndex = 10;
			this.ExportButton.ToolTipCaption = null;
			// 
			// MessageSendingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 603, true);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.ExportPathButton);
			this.Controls.Add(this.ExportPathTextBox);
			this.Controls.Add(this.ContinueToSendCheckBox);
			this.Controls.Add(this.AdditionalWarningsGroupBox);
			this.Controls.Add(this.ValidationErrorsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.JP.Business.DeclarationMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 640, true);
			this.Name = "MessageSendingForm";
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.ValidationErrorsGroupBox, 0);
			this.Controls.SetChildIndex(this.AdditionalWarningsGroupBox, 0);
			this.Controls.SetChildIndex(this.ContinueToSendCheckBox, 0);
			this.Controls.SetChildIndex(this.ExportPathTextBox, 0);
			this.Controls.SetChildIndex(this.ExportPathButton, 0);
			this.Controls.SetChildIndex(this.ExportButton, 0);
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ValidationErrorsGroupBox.ResumeLayout(false);
			this.ValidationErrorsGroupBox.PerformLayout();
			this.AdditionalWarningsGroupBox.ResumeLayout(false);
			this.AdditionalWarningsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGroupBox ValidationErrorsGroupBox;
		ZArchitecture.ZTextBox ValidationErrorsTextBox;
		ZArchitecture.GUI.ZGroupBox AdditionalWarningsGroupBox;
		ZArchitecture.ZTextBox AdditionalWarningsTextBox;
		ZArchitecture.GUI.ZCheckBox ContinueToSendCheckBox;
		ZArchitecture.ZTextBox ExportPathTextBox;
		ZArchitecture.GUI.ZButton ExportPathButton;
		ZArchitecture.GUI.ZButton ExportButton;
	}
}
