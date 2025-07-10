using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class WebPrintNudgeUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.OptionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.EnableURLAddressRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.EnableIPAddressRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.hoursLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.WebPrintNudgeWrapper);
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.Controls.Add(this.EnableURLAddressRadioButton);
			this.OptionGroupBox.Controls.Add(this.EnableIPAddressRadioButton);
			this.OptionGroupBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionGroupBox.Name = "optionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 80, true);
			this.OptionGroupBox.TabIndex = 1;
			this.OptionGroupBox.TabStop = false;
			this.OptionGroupBox.Text = Enterprise.Registry.GUI.Res.GetString("RegistryForm|66191AF1-3E95-4701-B0CE-F71A7795625D", "Sending Nudge");
			// 
			// EnableURLAddressRadioButton
			// 
			this.EnableURLAddressRadioButton.AutoCheck = false;
			this.EnableURLAddressRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableURLAddressRadioButton, "EnableURLAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.EnableURLAddressRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableURLAddressRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 50, true);
			this.EnableURLAddressRadioButton.Name = "EnableURLAddressRadioButton";
			this.EnableURLAddressRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.EnableURLAddressRadioButton.TabIndex = 3;
			this.EnableURLAddressRadioButton.TabStop = true;
			this.EnableURLAddressRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|C82E6DEE-BFC7-4EC8-8CFB-AF8A947D4CD0", "Web Service URL Address");
			this.EnableURLAddressRadioButton.BackColor = System.Drawing.Color.Transparent;
			// 
			// EnableIPAddressRadioButton
			// 
			this.EnableIPAddressRadioButton.AutoCheck = false;
			this.EnableIPAddressRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.EnableIPAddressRadioButton, "EnableIPAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.GUI.WebPrintNudgeWrapper)(null)).EnableIPAddress)));
			this.EnableIPAddressRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableIPAddressRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 24, true);
			this.EnableIPAddressRadioButton.Name = "EnableIPAddressRadioButton";
			this.EnableIPAddressRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.EnableIPAddressRadioButton.TabIndex = 2;
			this.EnableIPAddressRadioButton.TabStop = true;
			this.EnableIPAddressRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|30656DA7-F1F0-4FB2-BAFE-5FE482A2C586", "IP address");
			this.EnableIPAddressRadioButton.BackColor = System.Drawing.Color.Transparent;
			//
			// SwtichBackToIPAddressIntervalInHoursCalcEdit
			//
			this.BindingSource.SetBindingMember(this.SwtichBackToIPAddressIntervalInHoursCalcEdit, "SwtichBackToIPAddressIntervalInHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|9177C185-5E2E-4816-A0D4-B971371E7DAB", "Switch back to IP address after:");
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.DecimalPlaces = 0;
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.IsCalculatorEnabled = false;
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 90, true);
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.Name = "EnableUrlAddressInHoursCalcEdit";
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.TabIndex = 4;
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.TabStop = false;
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SwtichBackToIPAddressIntervalInHoursCalcEdit.AllowNegative = false;
			// 
			// hoursLabel
			// 
			this.hoursLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("F8C71884-D09D-4095-B1A4-B4E5EB79818E", "hours");
			this.hoursLabel.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.hoursLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 83, true);
			this.hoursLabel.Name = "hoursLabel";
			this.hoursLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 33, true);
			// 
			// WebPrintNudgeDirectIPAddressUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OptionGroupBox);
			this.Controls.Add(this.SwtichBackToIPAddressIntervalInHoursCalcEdit);
			this.Controls.Add(this.hoursLabel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 170, true);
			this.Name = "WebPrintNudgeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private CargoWise.Windows.UI.KGroupBox OptionGroupBox;
		private ZArchitecture.GUI.ZRadioButton EnableIPAddressRadioButton;
		private ZArchitecture.GUI.ZRadioButton EnableURLAddressRadioButton;
		private ZArchitecture.ZCalcEdit SwtichBackToIPAddressIntervalInHoursCalcEdit;
		private ZArchitecture.ZLabel hoursLabel;
	}
}
