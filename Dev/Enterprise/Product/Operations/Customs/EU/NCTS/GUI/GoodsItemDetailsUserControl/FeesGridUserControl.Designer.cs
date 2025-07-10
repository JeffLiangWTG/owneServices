namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class FeesGridUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.FeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FeesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FeesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).BeginInit();
			this.FeesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// FeesGroupBox
			// 
			this.FeesGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("CF5A58B8-A6C0-4311-A5F2-F36431FE2D79", "Calculated Duty and Tax");
			this.FeesGroupBox.Controls.Add(this.FeesGrid);
			this.FeesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FeesGroupBox, false);
			this.FeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FeesGroupBox.Name = "FeesGroupBox";
			this.FeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 135, true);
			this.FeesGroupBox.TabIndex = 0;
			this.FeesGroupBox.TabStop = false;
			// 
			// FeesGrid
			// 
			this.FeesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FeesGrid, "Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsCargoDescFee)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).Fees)).SyncRoot)).BFE_ChargeAmount)));
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("c79fa4b2-69cd-4aae-b0b9-498058dbff77", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "BFE_ChargeType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("98018eb0-1717-496b-b963-221eaf660380", "Method");
			zTextBoxColumnStyleInfo1.ColumnName = "BFE_MethodOfCalculation";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("f4bd17c0-1f90-41fa-b22d-53e95f402288", "Base");
			zCalcEditColumnStyleInfo1.ColumnName = "BFE_BaseValue";
			zCalcEditColumnStyleInfo1.Decimals = 2;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7819547c-37bd-483e-aa9f-cac00d30e565", "Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "BFE_Rate";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("662e5270-287d-46ab-a5ba-973f2a07b4d2", "Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "BFE_ChargeAmount";
			zCalcEditColumnStyleInfo3.Decimals = 2;
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			this.FeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FeesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FeesGrid.GridId = "156CDD73-DADE-4D2A-B336-712842B7889A";
			this.FeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FeesGrid.LayoutKey = "FeesGrid";
			this.FeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FeesGrid.Name = "FeesGrid";
			this.FeesGrid.ReadOnly = true;
			this.FeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 116, true);
			this.FeesGrid.TabIndex = 1;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FeesGroupBox);
			this.Name = "FeesGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FeesGroupBox.ResumeLayout(false);
			this.FeesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.FeesGrid)).EndInit();
			this.FeesGrid.ResumeLayout(false);
			this.FeesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid FeesGrid;
		internal ZArchitecture.GUI.ZGroupBox FeesGroupBox;
	}
}
