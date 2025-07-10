namespace Enterprise.Customs.BR.GUI
{
	partial class JobComInvoiceHeaderCopyOptionsForm
	{
		protected Enterprise.ZArchitecture.GUI.ZGroupBox OptionsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton OnlyLinesRequireLicenseRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton AllLinesRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ConfirmButton;
		internal Enterprise.ZArchitecture.GUI.ZButton Cancel2Button;
		private System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.OptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OnlyLinesRequireLicenseRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AllLinesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel2Button = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 128, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(333);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceHeaderCopyOptions);
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("b380b8d8-1ecf-4468-bacb-16289f2e21c0", "Invoice Items that require a license");
			this.OptionsGroupBox.Controls.Add(this.OnlyLinesRequireLicenseRadioButton);
			this.OptionsGroupBox.Controls.Add(this.AllLinesRadioButton);
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 83, true);
			this.OptionsGroupBox.TabIndex = 1;
			this.OptionsGroupBox.TabStop = false;
			// 
			// OnlyLinesRequireLicenseRadioButton
			// 
			this.OnlyLinesRequireLicenseRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.OnlyLinesRequireLicenseRadioButton, "OnlyLinesRequireLicense");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobComInvoiceHeaderCopyOptions)(null)).OnlyLinesRequireLicense)));
			this.OnlyLinesRequireLicenseRadioButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0c779d3d-b21d-4077-9939-ea8c0f15c410", "Only Invoice Lines that require license");
			this.OnlyLinesRequireLicenseRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 47, true);
			this.OnlyLinesRequireLicenseRadioButton.Name = "OnlyLinesRequireLicenseRadioButton";
			this.OnlyLinesRequireLicenseRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 24, true);
			this.OnlyLinesRequireLicenseRadioButton.TabIndex = 3;
			// 
			// AllLinesRadioButton
			// 
			this.AllLinesRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.AllLinesRadioButton, "AllLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobComInvoiceHeaderCopyOptions)(null)).AllLines)));
			this.AllLinesRadioButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("fd6c676e-08c4-4672-a864-f59ff18224e7", "All Lines from Invoice");
			this.AllLinesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 26, true);
			this.AllLinesRadioButton.Name = "AllLinesRadioButton";
			this.AllLinesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 15, true);
			this.AllLinesRadioButton.TabIndex = 1;
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0eda427e-8a5a-4567-a35c-8fb5a2e630ab", "Confirm");
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(478, 95, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.ConfirmButton.TabIndex = 2;
			this.ConfirmButton.ToolTipCaption = null;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// Cancel2Button
			// 
			this.Cancel2Button.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("88ca657a-d8db-4d0d-a17a-0e71b4b5df2c", "Cancel");
			this.Cancel2Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(574, 95, true);
			this.Cancel2Button.Name = "Cancel2Button";
			this.Cancel2Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.Cancel2Button.TabIndex = 3;
			this.Cancel2Button.ToolTipCaption = null;
			this.Cancel2Button.Click += new System.EventHandler(this.CancelButton2_Click);
			// 
			// JobComInvoiceHeaderCopyOptionsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("6d49964a-7061-4a9b-ab7f-a597b7d53908", "Type of Copy");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 151, true);
			this.Controls.Add(this.Cancel2Button);
			this.Controls.Add(this.ConfirmButton);
			this.Controls.Add(this.OptionsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.BR.Business";
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceHeaderCopyOptions);
			this.DataSourceTypeName = "Enterprise.Customs.BR.Business.JobComInvoiceHeaderCopyOptions";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "JobComInvoiceHeaderCopyOptionsForm";
			this.Controls.SetChildIndex(this.OptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.ConfirmButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.Cancel2Button, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsGroupBox.ResumeLayout(false);
			this.OptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
