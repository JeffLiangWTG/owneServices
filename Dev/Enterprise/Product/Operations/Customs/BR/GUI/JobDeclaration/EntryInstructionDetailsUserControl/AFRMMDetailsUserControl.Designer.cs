using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class AFRMMDetailsUserControl
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
			this.AFRMMGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AFRMMRateOverrideZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UtilizationFeeOverrideZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsAFRMMRateOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SystemUtilizationFeeOverrideCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AFRMMMethodOfCalculationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AFRMMGroupBox.SuspendLayout();
			this.AFRMMMethodOfCalculationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusEntryInstruction);
			// 
			// AFRMMGroupBox
			//
			this.AFRMMGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("DDFBB69C-5717-4276-9D94-55360CCF66DF", "AFRMM");
			this.AFRMMGroupBox.Controls.Add(this.AFRMMRateOverrideZCalcEdit);
			this.AFRMMGroupBox.Controls.Add(this.UtilizationFeeOverrideZCalcEdit);
			this.AFRMMGroupBox.Controls.Add(this.IsAFRMMRateOverriddenCheckBox);
			this.AFRMMGroupBox.Controls.Add(this.SystemUtilizationFeeOverrideCurrencyTextBox);
			this.AFRMMGroupBox.Controls.Add(this.AFRMMMethodOfCalculationDropEdit);
			this.AFRMMGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AFRMMGroupBox.Name = "AFRMMGroupBox";
			this.AFRMMGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 48, true);
			this.AFRMMGroupBox.TabIndex = 6;
			this.AFRMMGroupBox.TabStop = false;
			// 
			// AFRMMRateOverrideZCalcEdit
			//
			this.BindingSource.SetBindingMember(this.AFRMMRateOverrideZCalcEdit, "CEI_AFRMMRateOverride");
			this.AFRMMRateOverrideZCalcEdit.DecimalPlaces = 2;
			this.AFRMMRateOverrideZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 19, true);
			this.AFRMMRateOverrideZCalcEdit.Name = "AFRMMRateOverrideZCalcEdit";
			this.AFRMMRateOverrideZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.AFRMMRateOverrideZCalcEdit.TabIndex = 1;
			this.AFRMMRateOverrideZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AFRMMRateOverrideZCalcEdit.TrackDisposedAccess = true;
			// 
			// SystemUtilizationFeeOverrideZCalcEdit
			//
			this.BindingSource.SetBindingMember(this.UtilizationFeeOverrideZCalcEdit, "CEI_UtilizationFeeOverride");
			this.UtilizationFeeOverrideZCalcEdit.DecimalPlaces = 2;
			this.UtilizationFeeOverrideZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 19, true);
			this.UtilizationFeeOverrideZCalcEdit.Name = "UtilizationFeeOverrideZCalcEdit";
			this.UtilizationFeeOverrideZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.UtilizationFeeOverrideZCalcEdit.TabIndex = 3;
			this.UtilizationFeeOverrideZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.UtilizationFeeOverrideZCalcEdit.TrackDisposedAccess = true;
			// 
			// IsAFRMMRateOverriddenCheckBox
			//
			this.BindingSource.SetBindingMember(this.IsAFRMMRateOverriddenCheckBox, "IsAFRMMRateOverridden");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IsAFRMMRateOverriddenCheckBox, false);
			this.IsAFRMMRateOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, -2, true);
			this.IsAFRMMRateOverriddenCheckBox.Name = "IsAFRMMRateOverriddenCheckBox";
			this.IsAFRMMRateOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 19, true);
			this.IsAFRMMRateOverriddenCheckBox.TabIndex = 2;
			this.IsAFRMMRateOverriddenCheckBox.UseVisualStyleBackColor = true;
			// 
			// SystemUtilizationFeeOverrideCurrencyTextBox
			//
			this.BindingSource.SetBindingMember(this.SystemUtilizationFeeOverrideCurrencyTextBox, "CEI_SystemUtilizationFeeOverrideCurrency");
			this.SystemUtilizationFeeOverrideCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 19, true);
			this.SystemUtilizationFeeOverrideCurrencyTextBox.Name = "SystemUtilizationFeeOverrideCurrencyTextBox";
			this.SystemUtilizationFeeOverrideCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.SystemUtilizationFeeOverrideCurrencyTextBox.TabIndex = 4;
			// 
			// AFRMMMethodOfCalculationDropEdit
			//
			this.BindingSource.SetBindingMember(this.AFRMMMethodOfCalculationDropEdit, "CEI_AFRMMMethodOfCalculation");
			this.AFRMMMethodOfCalculationDropEdit.AllowDrop = true;
			this.AFRMMMethodOfCalculationDropEdit.BindToList = "Lookups.AFRMMMethodOfCalculationList";
			this.AFRMMMethodOfCalculationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 19, true);
			this.AFRMMMethodOfCalculationDropEdit.Name = "AFRMMMethodOfCalculationDropEdit";
			this.AFRMMMethodOfCalculationDropEdit.PreBoundMaxLength = 4;
			this.AFRMMMethodOfCalculationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.AFRMMMethodOfCalculationDropEdit.TabIndex = 0;
			// 
			// AFRMMDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AFRMMGroupBox);
			this.Name = "AFRMMDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AFRMMGroupBox.ResumeLayout(false);
			this.AFRMMGroupBox.PerformLayout();
			this.AFRMMMethodOfCalculationDropEdit.ResumeLayout(true);
			this.AFRMMMethodOfCalculationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZGroupBox AFRMMGroupBox;
		internal ZDropEdit AFRMMMethodOfCalculationDropEdit;
		internal ZCheckBox IsAFRMMRateOverriddenCheckBox;
		internal ZArchitecture.ZCalcEdit AFRMMRateOverrideZCalcEdit;
		internal ZArchitecture.ZTextBox SystemUtilizationFeeOverrideCurrencyTextBox;
		internal ZArchitecture.ZCalcEdit UtilizationFeeOverrideZCalcEdit;
	}
}
