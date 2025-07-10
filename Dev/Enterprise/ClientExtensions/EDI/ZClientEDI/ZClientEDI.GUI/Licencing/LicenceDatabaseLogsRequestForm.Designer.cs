namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class LicenceDatabaseLogsRequestForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
            this.dateFromField = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.dateToField = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.sendButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.notSupportedLabel = new Enterprise.ZArchitecture.ZLabel();
            this.serviceTaskCodeField = new Enterprise.ZArchitecture.ZTextBox();
            this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
            this.IncidentNumberFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.dateFromField.SuspendLayout();
            this.dateToField.SuspendLayout();
            this.IncidentNumberFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 225, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.LicenceDatabaseLogsRequest);
            // 
            // dateFromField
            // 
            this.dateFromField.AllowDrop = true;
            this.dateFromField.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.dateFromField.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateFromField, "DateFrom");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Licencing.Business.LicenceDatabaseLogsRequest)(null)).DateFrom)));
            this.dateFromField.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 53, true);
            this.dateFromField.Name = "dateFromField";
            this.dateFromField.TabIndex = 1;
            // 
            // zLabel1
            // 
            this.zLabel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 50, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
            this.zLabel1.TabIndex = 2;
            this.zLabel1.Text = "Date From";
            this.zLabel1.UseMnemonic = false;
            // 
            // zLabel2
            // 
            this.zLabel2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 83, true);
            this.zLabel2.Name = "zLabel2";
            this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
            this.zLabel2.TabIndex = 3;
            this.zLabel2.Text = "Date To";
            this.zLabel2.UseMnemonic = false;
            // 
            // dateToField
            // 
            this.dateToField.AllowDrop = true;
            this.dateToField.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.dateToField.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateToField, "DateTo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Licencing.Business.LicenceDatabaseLogsRequest)(null)).DateTo)));
            this.dateToField.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 85, true);
            this.dateToField.Name = "dateToField";
            this.dateToField.TabIndex = 2;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.IsCaptionOverridden = true;
            this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 191, true);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.ToolTipCaption = null;
            // 
            // sendButton
            // 
            this.sendButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.sendButton.IsCaptionOverridden = true;
            this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 191, true);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
            this.sendButton.TabIndex = 5;
            this.sendButton.Text = "Send";
            this.sendButton.ToolTipCaption = null;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // notSupportedLabel
            // 
            this.notSupportedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.notSupportedLabel.ForeColor = System.Drawing.Color.Red;
            this.notSupportedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
            this.notSupportedLabel.Name = "notSupportedLabel";
            this.notSupportedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 34, true);
            this.notSupportedLabel.TabIndex = 15;
            this.notSupportedLabel.Text = "Destination version not supported";
            this.notSupportedLabel.UseMnemonic = false;
            this.notSupportedLabel.Visible = false;
            // 
            // serviceTaskCodeField
            // 
            this.BindingSource.SetBindingMember(this.serviceTaskCodeField, "ServiceTaskCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.LicenceDatabaseLogsRequest)(null)).ServiceTaskCode)));
            this.serviceTaskCodeField.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 121, true);
            this.serviceTaskCodeField.Name = "serviceTaskCodeField";
            this.serviceTaskCodeField.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
            this.serviceTaskCodeField.TabIndex = 3;
            // 
            // zLabel3
            // 
            this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 118, true);
            this.zLabel3.Name = "zLabel3";
            this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 35, true);
            this.zLabel3.TabIndex = 17;
            this.zLabel3.Text = "Service Task Code(s) (comma separated)";
            this.zLabel3.UseMnemonic = false;
            // 
            // IncidentNumberFindBox
            // 
            this.IncidentNumberFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.IncidentNumberFindBox, "IncidentNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Licencing.Business.LicenceDatabaseLogsRequest)(null)).IncidentNumber)));
            this.IncidentNumberFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 156, true);
            this.IncidentNumberFindBox.Name = "IncidentNumberFindBox";
            this.IncidentNumberFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.IncidentNumberFindBox.ParentType = null;
            this.IncidentNumberFindBox.ShowDescriptionBox = false;
            this.IncidentNumberFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
            this.IncidentNumberFindBox.TabIndex = 4;
            // 
            // zLabel4
            // 
            this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 153, true);
            this.zLabel4.Name = "zLabel4";
            this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
            this.zLabel4.TabIndex = 19;
            this.zLabel4.Text = "Incident";
            this.zLabel4.UseMnemonic = false;
            // 
            // LicenceDatabaseLogsRequestForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 249, true);
            this.Controls.Add(this.zLabel4);
            this.Controls.Add(this.IncidentNumberFindBox);
            this.Controls.Add(this.zLabel3);
            this.Controls.Add(this.serviceTaskCodeField);
            this.Controls.Add(this.notSupportedLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.zLabel2);
            this.Controls.Add(this.dateToField);
            this.Controls.Add(this.zLabel1);
            this.Controls.Add(this.dateFromField);
            this.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.LicenceDatabaseLogsRequest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LicenceDatabaseLogsRequestForm";
            this.Text = "Request Service Task Logs";
            this.Controls.SetChildIndex(this.dateFromField, 0);
            this.Controls.SetChildIndex(this.zLabel1, 0);
            this.Controls.SetChildIndex(this.dateToField, 0);
            this.Controls.SetChildIndex(this.zLabel2, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.sendButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.notSupportedLabel, 0);
            this.Controls.SetChildIndex(this.serviceTaskCodeField, 0);
            this.Controls.SetChildIndex(this.zLabel3, 0);
            this.Controls.SetChildIndex(this.IncidentNumberFindBox, 0);
            this.Controls.SetChildIndex(this.zLabel4, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.dateFromField.ResumeLayout(true);
            this.dateFromField.PerformLayout();
            this.dateToField.ResumeLayout(true);
            this.dateToField.PerformLayout();
            this.IncidentNumberFindBox.ResumeLayout(true);
            this.IncidentNumberFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZDateEdit dateFromField;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit dateToField;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		protected Enterprise.ZArchitecture.GUI.ZButton sendButton;
		protected Enterprise.ZArchitecture.ZLabel notSupportedLabel;
		protected ZArchitecture.ZTextBox serviceTaskCodeField;
		private ZArchitecture.ZLabel zLabel3;
		protected ZArchitecture.GUI.ZCodeFindBox IncidentNumberFindBox;
		private ZArchitecture.ZLabel zLabel4;
	}
}
