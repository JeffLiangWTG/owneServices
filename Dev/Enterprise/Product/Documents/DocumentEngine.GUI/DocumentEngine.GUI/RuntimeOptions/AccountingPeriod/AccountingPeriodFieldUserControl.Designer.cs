namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class AccountingPeriodFieldUserControl
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
			this.SingleRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RangeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.YearToPeriodRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.AllRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SinglePeriodEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulablePeriodEdit();
			this.YearToPeriodEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulablePeriodEdit();
			this.PeriodRangeEdit = new Enterprise.DocumentEngine.GUI.RuntimeOptions.SchedulablePeriodRangeEdit();
			this.FieldGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SinglePeriodEdit.SuspendLayout();
			this.YearToPeriodEdit.SuspendLayout();
			this.PeriodRangeEdit.SuspendLayout();
			this.FieldGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.AccountingPeriodField);
			// 
			// SingleRadioButton
			// 
			this.SingleRadioButton.AutoCheck = false;
			this.SingleRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SingleRadioButton, "UseSinglePeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.AccountingPeriodField)(null)).UseSinglePeriod)));
			this.SingleRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccountingPeriodFieldUserControl|33cebadf-bde4-411d-84e5-94e3386824ad", "Single Period");
			this.SingleRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SingleRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 18, true);
			this.SingleRadioButton.Name = "SingleRadioButton";
			this.SingleRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 17, true);
			this.SingleRadioButton.TabIndex = 0;
			// 
			// RangeRadioButton
			// 
			this.RangeRadioButton.AutoCheck = false;
			this.RangeRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RangeRadioButton, "UsePeriodRange");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.AccountingPeriodField)(null)).UsePeriodRange)));
			this.RangeRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccountingPeriodFieldUserControl|4445272f-60f6-4d81-b66a-e279c64140b9", "Period Range");
			this.RangeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RangeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 45, true);
			this.RangeRadioButton.Name = "RangeRadioButton";
			this.RangeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.RangeRadioButton.TabIndex = 2;
			// 
			// YearToPeriodRadioButton
			// 
			this.YearToPeriodRadioButton.AutoCheck = false;
			this.YearToPeriodRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.YearToPeriodRadioButton, "UseYearToPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.AccountingPeriodField)(null)).UseYearToPeriod)));
			this.YearToPeriodRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccountingPeriodFieldUserControl|26a17a82-eb31-4fc1-abcb-575ebda7db28", "Year to Period");
			this.YearToPeriodRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.YearToPeriodRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 98, true);
			this.YearToPeriodRadioButton.Name = "YearToPeriodRadioButton";
			this.YearToPeriodRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
			this.YearToPeriodRadioButton.TabIndex = 4;
			// 
			// AllRadioButton
			// 
			this.AllRadioButton.AutoCheck = false;
			this.AllRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AllRadioButton, "UseAllPeriods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.RuntimeOptions.AccountingPeriodField)(null)).UseAllPeriods)));
			this.AllRadioButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccountingPeriodFieldUserControl|e12011ec-698d-49eb-b5e4-d3089251c289", "All Periods");
			this.AllRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 120, true);
			this.AllRadioButton.Name = "AllRadioButton";
			this.AllRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 17, true);
			this.AllRadioButton.TabIndex = 6;
			// 
			// SinglePeriodEdit
			// 
			this.SinglePeriodEdit.AllowDrop = true;
			this.SinglePeriodEdit.BindTo = "SinglePeriod";
			this.SinglePeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 16, true);
			this.SinglePeriodEdit.Name = "SinglePeriodEdit";
			this.SinglePeriodEdit.ReadOnly = false;
			this.SinglePeriodEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
			this.SinglePeriodEdit.TabIndex = 1;
			// 
			// YearToPeriodEdit
			// 
			this.YearToPeriodEdit.AllowDrop = true;
			this.YearToPeriodEdit.BindTo = "YearToPeriod";
			this.YearToPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 96, true);
			this.YearToPeriodEdit.Name = "YearToPeriodEdit";
			this.YearToPeriodEdit.ReadOnly = false;
			this.YearToPeriodEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 23, true);
			this.YearToPeriodEdit.TabIndex = 5;
			// 
			// PeriodRangeEdit
			// 
			this.PeriodRangeEdit.AllowDrop = true;
			this.PeriodRangeEdit.BindToHigh = "ToPeriod";
			this.PeriodRangeEdit.BindToLow = "FromPeriod";
			this.PeriodRangeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 40, true);
			this.PeriodRangeEdit.Name = "PeriodRangeEdit";
			this.PeriodRangeEdit.ReadOnly = false;
			this.PeriodRangeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 52, true);
			this.PeriodRangeEdit.TabIndex = 3;
			// 
			// FieldGroupBox
			// 
			this.FieldGroupBox.Controls.Add(this.PeriodRangeEdit);
			this.FieldGroupBox.Controls.Add(this.YearToPeriodEdit);
			this.FieldGroupBox.Controls.Add(this.SinglePeriodEdit);
			this.FieldGroupBox.Controls.Add(this.AllRadioButton);
			this.FieldGroupBox.Controls.Add(this.YearToPeriodRadioButton);
			this.FieldGroupBox.Controls.Add(this.RangeRadioButton);
			this.FieldGroupBox.Controls.Add(this.SingleRadioButton);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FieldGroupBox, false);
			this.FieldGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.FieldGroupBox.Name = "FieldGroupBox";
			this.FieldGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 141, true);
			this.FieldGroupBox.TabIndex = 0;
			this.FieldGroupBox.TabStop = false;
			// 
			// AccountingPeriodFieldUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FieldGroupBox);
			this.Name = "AccountingPeriodFieldUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(356, 148, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SinglePeriodEdit.ResumeLayout(true);
			this.SinglePeriodEdit.PerformLayout();
			this.YearToPeriodEdit.ResumeLayout(true);
			this.YearToPeriodEdit.PerformLayout();
			this.PeriodRangeEdit.ResumeLayout(true);
			this.PeriodRangeEdit.PerformLayout();
			this.FieldGroupBox.ResumeLayout(false);
			this.FieldGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZRadioButton SingleRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton RangeRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton YearToPeriodRadioButton;
		internal Enterprise.ZArchitecture.GUI.ZRadioButton AllRadioButton;
		internal SchedulablePeriodEdit SinglePeriodEdit;
		internal SchedulablePeriodEdit YearToPeriodEdit;
		internal SchedulablePeriodRangeEdit PeriodRangeEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox FieldGroupBox;

	}
}
