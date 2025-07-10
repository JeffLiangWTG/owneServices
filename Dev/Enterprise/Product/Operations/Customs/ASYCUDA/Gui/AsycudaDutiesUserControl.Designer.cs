namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaDutiesUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.subgroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.DutiesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.subgroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DutiesGrid)).BeginInit();
            this.DutiesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaBill);
			// 
			// subgroup
			//
			this.subgroup.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("3D5E9593-D0A5-4D42-B4C3-A2142553C1B6", "subgroup");
			this.subgroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.subgroup.Controls.Add(this.DutiesGrid);
            this.subgroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.subgroup.Name = "subgroup";
            this.subgroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 373, true);
            this.subgroup.TabIndex = 2;
            this.subgroup.TabStop = false;
            // 
            // DutiesGrid
            // 
            this.DutiesGrid.AllowNavigation = false;
            this.DutiesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DutiesGrid, "AsycudaTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_MethodOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_RateOverrideReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTax)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).AsycudaTaxes)).SyncRoot)).AET_RX_NKCurrency)));
			this.DutiesGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "AET_ChargeType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "AET_BaseValue";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "AET_Rate";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "AET_ChargeAmount";
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ColumnName = "AET_MethodOfPayment";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo1.ColumnName = "AET_MethodOfCalculation";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.ColumnName = "AET_RateOverrideReasonCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AET_RX_NKCurrency";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.DutiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.DutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.DutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.DutiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.DutiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DutiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DutiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.DutiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.DutiesGrid.GridId = "dc8cdc3e-5c49-41e0-bd14-be354f34f741";
            this.DutiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.DutiesGrid.LayoutKey = "DutiesGrid";
            this.DutiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DutiesGrid.Name = "DutiesGrid";
            this.DutiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 370, true);
            this.DutiesGrid.TabIndex = 0;
			// 
			// AsycudaDutiesUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.subgroup);
            this.Name = "AsycudaDutiesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(943, 373, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.subgroup.ResumeLayout(false);
            this.subgroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DutiesGrid)).EndInit();
            this.DutiesGrid.ResumeLayout(false);
            this.DutiesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox subgroup;
		private ZArchitecture.ZGrid DutiesGrid;
	}
}
