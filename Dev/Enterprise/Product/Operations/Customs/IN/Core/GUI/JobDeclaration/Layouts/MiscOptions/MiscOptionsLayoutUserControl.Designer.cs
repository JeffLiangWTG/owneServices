namespace Enterprise.Customs.IN.GUI;

partial class MiscOptionsLayoutUserControl
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
	private void InitializeComponent()
	{
			this.NonStandardExchangeRateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NonStandardExchangeRatesGridUserControl = new Enterprise.Customs.IN.GUI.NonStandardExchangeRatesGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NonStandardExchangeRateGroupBox.SuspendLayout();
			this.NonStandardExchangeRatesGridUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobDeclaration);
			// 
			// NonStandardExchangeRateGroupBox
			// 
			this.NonStandardExchangeRateGroupBox.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("46c54e1e-2edb-45e8-9351-70024ba6ac61", "Ex. Rate for Non-Standard Currency");
			this.NonStandardExchangeRateGroupBox.Controls.Add(this.NonStandardExchangeRatesGridUserControl);
			this.NonStandardExchangeRateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 15, true);
			this.NonStandardExchangeRateGroupBox.Name = "NonStandardExchangeRateGroupBox";
			this.NonStandardExchangeRateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 132, true);
			this.NonStandardExchangeRateGroupBox.TabIndex = 0;
			this.NonStandardExchangeRateGroupBox.TabStop = false;
			// 
			// NonStandardExchangeRatesGridUserControl
			// 
			this.NonStandardExchangeRatesGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NonStandardExchangeRatesGridUserControl, "NonStandardExchangeRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.IN.Business.NonStandardExchangeRateCollection)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).NonStandardExchangeRates)));
			this.NonStandardExchangeRatesGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NonStandardExchangeRatesGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.NonStandardExchangeRatesGridUserControl.Name = "NonStandardExchangeRatesGridUserControl";
			this.NonStandardExchangeRatesGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 113, true);
			this.NonStandardExchangeRatesGridUserControl.TabIndex = 0;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NonStandardExchangeRateGroupBox);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 162, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NonStandardExchangeRateGroupBox.ResumeLayout(false);
			this.NonStandardExchangeRateGroupBox.PerformLayout();
			this.NonStandardExchangeRatesGridUserControl.ResumeLayout(true);
			this.NonStandardExchangeRatesGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion
	private NonStandardExchangeRatesGridUserControl NonStandardExchangeRatesGridUserControl;
	internal ZArchitecture.GUI.ZGroupBox NonStandardExchangeRateGroupBox;
}

