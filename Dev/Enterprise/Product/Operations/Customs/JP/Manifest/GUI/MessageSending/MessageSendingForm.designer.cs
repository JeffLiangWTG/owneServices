namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class MessageSendingForm
	{
		new void InitializeComponent()
		{
			this.ValidationErrorsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ValidationErrorsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalWarningsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalWarningsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportPathTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExportPathButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContinueToSendCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HAWBGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ENDCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValidationErrorsGroupBox.SuspendLayout();
			this.AdditionalWarningsGroupBox.SuspendLayout();
			this.HAWBGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 568, true);
			this.SendButton.TabIndex = 10;
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 568, true);
			this.CancelButton2.TabIndex = 11;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Controls.Add(this.HAWBGroupBox);
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 248, true);
			this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.HAWBGroupBox, 0);
			this.messageSendingObjectsGroupBox.Controls.SetChildIndex(this.MessageSendingObjectsGrid, 0);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 55, true);
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 191, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 591, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent);
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ValidationErrorsGroupBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("E94F2162-81FF-40D4-9FD1-EB0DCCE0B0DA", "Validation Errors");
			this.ValidationErrorsGroupBox.Controls.Add(this.ValidationErrorsTextBox);
			this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 264, true);
			this.ValidationErrorsGroupBox.Name = "ValidationErrorsGroupBox";
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 131, true);
			this.ValidationErrorsGroupBox.TabIndex = 4;
			this.ValidationErrorsGroupBox.TabStop = false;
			// 
			// ValidationErrorsTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValidationErrorsTextBox, "BizObjValidationMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent)(null)).BizObjValidationMessageErrors)));
			this.ValidationErrorsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValidationErrorsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ValidationErrorsTextBox.Multiline = true;
			this.ValidationErrorsTextBox.Name = "ValidationErrorsTextBox";
			this.ValidationErrorsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 114, true);
			this.ValidationErrorsTextBox.TabIndex = 0;
			// 
			// AdditionalWarningsGroupBox
			// 
			this.AdditionalWarningsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalWarningsGroupBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("217C54F1-20B8-4E52-92CD-CBEA615D2236", "Additional Warnings");
			this.AdditionalWarningsGroupBox.Controls.Add(this.AdditionalWarningsTextBox);
			this.AdditionalWarningsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 399, true);
			this.AdditionalWarningsGroupBox.Name = "AdditionalWarningsGroupBox";
			this.AdditionalWarningsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 130, true);
			this.AdditionalWarningsGroupBox.TabIndex = 5;
			this.AdditionalWarningsGroupBox.TabStop = false;
			// 
			// AdditionalWarningsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalWarningsTextBox, "AdditionalWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent)(null)).AdditionalWarnings)));
			this.AdditionalWarningsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalWarningsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AdditionalWarningsTextBox.Multiline = true;
			this.AdditionalWarningsTextBox.Name = "AdditionalWarningsTextBox";
			this.AdditionalWarningsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(745, 113, true);
			this.AdditionalWarningsTextBox.TabIndex = 0;
			// 
			// ExportPathTextBox
			// 
			this.ExportPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExportPathTextBox, "ExportPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent)(null)).ExportPath)));
			this.ExportPathTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 535, true);
			this.ExportPathTextBox.Name = "ExportPathTextBox";
			this.ExportPathTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 17, true);
			this.ExportPathTextBox.TabIndex = 6;
			// 
			// ExportPathButton
			// 
			this.ExportPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportPathButton.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("5E63C627-8EE8-43DE-8128-3857305CE827", "Choose");
			this.ExportPathButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 535, true);
			this.ExportPathButton.Name = "ExportPathButton";
			this.ExportPathButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ExportPathButton.TabIndex = 7;
			this.ExportPathButton.ToolTipCaption = null;
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportButton.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("9C03C8D2-064B-4881-83C5-46651328FAA2", "Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 568, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ExportButton.TabIndex = 9;
			this.ExportButton.ToolTipCaption = null;
			// 
			// ContinueToSendCheckBox
			// 
			this.ContinueToSendCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.ContinueToSendCheckBox, "AllowSendWithError");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent)(null)).AllowSendWithError)));
			this.ContinueToSendCheckBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("FA90C769-FEE8-4496-BCB7-E856EA688EC0", "Continue to send even though the selected message(s) contains validation errors?");
			this.ContinueToSendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 566, true);
			this.ContinueToSendCheckBox.Name = "ContinueToSendCheckBox";
			this.ContinueToSendCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 22, true);
			this.ContinueToSendCheckBox.TabIndex = 8;
			this.ContinueToSendCheckBox.UseVisualStyleBackColor = true;
			// 
			// HAWBGroupBox
			// 
			this.HAWBGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.HAWBGroupBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("48C1F333-933C-4248-82F5-6E1E32A65018", "MAWB");
			this.HAWBGroupBox.Controls.Add(this.ENDCheckBox);
			this.HAWBGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HAWBGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.HAWBGroupBox.Name = "HAWBGroupBox";
			this.HAWBGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 40, true);
			this.HAWBGroupBox.TabIndex = 6;
			this.HAWBGroupBox.Visible = false;
			this.HAWBGroupBox.TabStop = false;
			// 
			// ENDCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ENDCheckBox, "EndSendMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent)(null)).EndSendMessage)));
			this.ENDCheckBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("7FF151F6-61EC-4F7F-81EA-281FFCA8D7F8", "END – All HAWBs for the MAWB have been registered or included in this message.");
			this.ENDCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 15, true);
			this.ENDCheckBox.Name = "ENDCheckBox";
			this.ENDCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 22, true);
			this.ENDCheckBox.TabIndex = 8;
			this.ENDCheckBox.UseVisualStyleBackColor = true;
			// 
			// MessageSendingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 614, true);
			this.Controls.Add(this.ContinueToSendCheckBox);
			this.Controls.Add(this.ExportPathButton);
			this.Controls.Add(this.ExportPathTextBox);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.AdditionalWarningsGroupBox);
			this.Controls.Add(this.ValidationErrorsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.ManifestMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 614, true);
			this.Name = "MessageSendingForm";
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.ValidationErrorsGroupBox, 0);
			this.Controls.SetChildIndex(this.AdditionalWarningsGroupBox, 0);
			this.Controls.SetChildIndex(this.ExportButton, 0);
			this.Controls.SetChildIndex(this.ExportPathTextBox, 0);
			this.Controls.SetChildIndex(this.ExportPathButton, 0);
			this.Controls.SetChildIndex(this.ContinueToSendCheckBox, 0);
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
			this.HAWBGroupBox.ResumeLayout(false);
			this.HAWBGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected ZArchitecture.GUI.ZGroupBox ValidationErrorsGroupBox;
		protected ZArchitecture.ZTextBox ValidationErrorsTextBox;
		protected ZArchitecture.GUI.ZGroupBox AdditionalWarningsGroupBox;
		protected ZArchitecture.ZTextBox AdditionalWarningsTextBox;
		protected ZArchitecture.ZTextBox ExportPathTextBox;
		protected ZArchitecture.GUI.ZButton ExportPathButton;
		protected ZArchitecture.GUI.ZButton ExportButton;
		protected ZArchitecture.GUI.ZCheckBox ContinueToSendCheckBox;
		protected ZArchitecture.GUI.ZGroupBox HAWBGroupBox;
		protected ZArchitecture.GUI.ZCheckBox ENDCheckBox;
	}
}
