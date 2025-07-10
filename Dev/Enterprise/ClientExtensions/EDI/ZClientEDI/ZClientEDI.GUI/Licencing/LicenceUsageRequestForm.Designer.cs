namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class LicenceUsageRequestForm
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
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.dateFromField.SuspendLayout();
            this.dateToField.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 154, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.LicenceUsageRequest);
            // 
            // dateFromField
            // 
            this.dateFromField.AllowDrop = true;
            this.dateFromField.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.dateFromField.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateFromField, "DateFrom");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Licencing.Business.LicenceUsageRequest)(null)).DateFrom)));
            this.dateFromField.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 55, true);
            this.dateFromField.Name = "dateFromField";
            this.dateFromField.TabIndex = 1;
            // 
            // zLabel1
            // 
            this.zLabel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 52, true);
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
            this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 85, true);
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Licencing.Business.LicenceUsageRequest)(null)).DateTo)));
            this.dateToField.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 87, true);
            this.dateToField.Name = "dateToField";
            this.dateToField.TabIndex = 4;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.CausesValidation = false;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.IsCaptionOverridden = true;
            this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 120, true);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
            this.cancelButton.TabIndex = 14;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.ToolTipCaption = null;
            // 
            // sendButton
            // 
            this.sendButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.sendButton.IsCaptionOverridden = true;
            this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 120, true);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
            this.sendButton.TabIndex = 13;
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
            this.notSupportedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 37, true);
            this.notSupportedLabel.TabIndex = 15;
            this.notSupportedLabel.Text = "Destination version not supported";
            this.notSupportedLabel.UseMnemonic = false;
            this.notSupportedLabel.Visible = false;
            // 
            // LicenceUsageRequestForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 178, true);
            this.Controls.Add(this.notSupportedLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.zLabel2);
            this.Controls.Add(this.dateToField);
            this.Controls.Add(this.zLabel1);
            this.Controls.Add(this.dateFromField);
            this.DataSourceType = typeof(Enterprise.Client.EDI.Licencing.Business.LicenceUsageRequest);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LicenceUsageRequestForm";
            this.Text = "Request Licence Usage";
            this.Controls.SetChildIndex(this.dateFromField, 0);
            this.Controls.SetChildIndex(this.zLabel1, 0);
            this.Controls.SetChildIndex(this.dateToField, 0);
            this.Controls.SetChildIndex(this.zLabel2, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.sendButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.notSupportedLabel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.dateFromField.ResumeLayout(true);
            this.dateFromField.PerformLayout();
            this.dateToField.ResumeLayout(true);
            this.dateToField.PerformLayout();
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
	}
}
