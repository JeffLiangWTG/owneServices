namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class NctsTaxOrFeeUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TaxOrFeesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TaxOrFeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxOrFeesGrid)).BeginInit();
			this.TaxOrFeesGrid.SuspendLayout();
			this.TaxOrFeesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// TaxOrFeesGrid
			// 
			this.TaxOrFeesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TaxOrFeesGrid, "Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_RateOverrideReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.IT.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_MethodOfPayment)));
			this.TaxOrFeesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "BFE_ChargeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "BFE_RateOverrideReasonCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "BFE_BaseValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.ColumnName = "BFE_MethodOfCalculation";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "BFE_Rate";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "BFE_ChargeAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.ColumnName = "BFE_MethodOfPayment";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.TaxOrFeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TaxOrFeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TaxOrFeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TaxOrFeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TaxOrFeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TaxOrFeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TaxOrFeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TaxOrFeesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxOrFeesGrid.GridId = "1f240903-483e-486d-8e5c-f7cdc19975b7";
			this.TaxOrFeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TaxOrFeesGrid.LayoutKey = "TaxOrFeesGrid";
			this.TaxOrFeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TaxOrFeesGrid.Name = "TaxOrFeesGrid";
			this.TaxOrFeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(875, 371, true);
			this.TaxOrFeesGrid.TabIndex = 0;
			// 
			// TaxOrFeesGroupBox
			// 
			this.TaxOrFeesGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("9b4cc8b8-8b26-42a8-a594-59eda726042f", "[47] Tax or Fee");
			this.TaxOrFeesGroupBox.Controls.Add(this.TaxOrFeesGrid);
			this.TaxOrFeesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TaxOrFeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TaxOrFeesGroupBox.Name = "TaxOrFeesGroupBox";
			this.TaxOrFeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 390, true);
			this.TaxOrFeesGroupBox.TabIndex = 1;
			this.TaxOrFeesGroupBox.TabStop = false;
			// 
			// NctsTaxOrFeeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TaxOrFeesGroupBox);
			this.Name = "NctsTaxOrFeeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 390, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TaxOrFeesGrid)).EndInit();
			this.TaxOrFeesGrid.ResumeLayout(false);
			this.TaxOrFeesGrid.PerformLayout();
			this.TaxOrFeesGroupBox.ResumeLayout(false);
			this.TaxOrFeesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid TaxOrFeesGrid;
		ZArchitecture.GUI.ZGroupBox TaxOrFeesGroupBox;
	}
}
