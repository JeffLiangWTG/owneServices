namespace Enterprise.Customs.GB.GUI
{
	partial class EntryLineTaxAndConfirmedFeeUserControl
	{
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.EntryLineConfirmedDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntryLineCalculatedDutyAndTaxUserControl = new Enterprise.Customs.EU.GUI.EntryLineTaxAndFeeUserControl();
			this.EntryLineConfirmedDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineConfirmedDutyAndTaxGrid)).BeginInit();
			this.EntryLineConfirmedDutyAndTaxGrid.SuspendLayout();
			this.EntryLineCalculatedDutyAndTaxUserControl.SuspendLayout();
			this.EntryLineConfirmedDutyAndTaxGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.AllCusEntryLineCollection<Business.Declaration.CusEntryLine>);
			// 
			// EntryLineConfirmedDutyAndTaxGrid
			// 
			this.EntryLineConfirmedDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineConfirmedDutyAndTaxGrid, "ConfirmedFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).CF_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).CF_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).CF_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).CF_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).CF_MethodOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.GB.Business.Declaration.CusEntryLine)(null)).ConfirmedFees)).SyncRoot)).CF_IsLandedCostOnly)));
			this.EntryLineConfirmedDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("43503560-1562-470c-8619-62e5890467d5", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.GB.GUI.Res.GetData("bd973848-1bc3-4082-910d-fe1005e9b5a7", "Charge Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f51ba931-4f2a-44fb-b79b-ad608ecb310e", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.GB.GUI.Res.GetData("bd973848-1bc3-4082-910d-fe1005e9b5a7", "Charge Type");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("27c2ba50-50f0-40e3-954e-33fa72ce25d9", "Base Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("c34b139f-7a67-4fbf-bae6-29d08aecb22c", "Method of Calculation");
			zDropEditColumnStyleInfo2.ColumnName = "CF_MethodOfCalculation";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("fdb570b6-a1b5-498b-8909-410d34dc0179", "Tax Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("12fa0894-68b9-4ad6-821c-55020dbdadba", "Total Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0cd3cdaf-dcc7-492e-b5a0-6eb45c2616e1", "Method of Payment");
			zDropEditColumnStyleInfo3.ColumnName = "CF_MethodOfPayment";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("7915d439-105c-440a-aa9e-2665b5cdd479", "Is Landed Cost Only");
			zCheckBoxColumnStyleInfo1.ColumnName = "CF_IsLandedCostOnly";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.EntryLineConfirmedDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineConfirmedDutyAndTaxGrid.GridId = "cb5d6fde-d363-4be2-9ea0-92f3b3e3c124";
			this.EntryLineConfirmedDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineConfirmedDutyAndTaxGrid.LayoutKey = "EntryLineConfirmedDutyAndTaxGrid";
			this.EntryLineConfirmedDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLineConfirmedDutyAndTaxGrid.Name = "EntryLineConfirmedDutyAndTaxGrid";
			this.EntryLineConfirmedDutyAndTaxGrid.ReadOnly = true;
			this.EntryLineConfirmedDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 116, true);
			this.EntryLineConfirmedDutyAndTaxGrid.TabIndex = 1;
			// 
			// EntryLineCalculatedDutyAndTaxUserControl
			// 
			this.EntryLineCalculatedDutyAndTaxUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryLineCalculatedDutyAndTaxUserControl, ".");
			this.EntryLineCalculatedDutyAndTaxUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineCalculatedDutyAndTaxUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLineCalculatedDutyAndTaxUserControl.Name = "EntryLineCalculatedDutyAndTaxUserControl";
			this.EntryLineCalculatedDutyAndTaxUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 132, true);
			this.EntryLineCalculatedDutyAndTaxUserControl.TabIndex = 0;
			// 
			// EntryLineConfirmedDutyAndTaxGroupBox
			// 
			this.EntryLineConfirmedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("2EC70159-1CAC-4D87-82B3-B7C487591791", "Confirmed Duty And Tax");
			this.EntryLineConfirmedDutyAndTaxGroupBox.Controls.Add(this.EntryLineConfirmedDutyAndTaxGrid);
			this.EntryLineConfirmedDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineConfirmedDutyAndTaxGroupBox, true);
			this.EntryLineConfirmedDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.EntryLineConfirmedDutyAndTaxGroupBox.Name = "EntryLineConfirmedDutyAndTaxGroupBox";
			this.EntryLineConfirmedDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 135, true);
			this.EntryLineConfirmedDutyAndTaxGroupBox.TabIndex = 5;
			this.EntryLineConfirmedDutyAndTaxGroupBox.TabStop = false;
			// 
			// EntryLineTaxAndConfirmedFeeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryLineCalculatedDutyAndTaxUserControl);
			this.Controls.Add(this.EntryLineConfirmedDutyAndTaxGroupBox);
			this.Name = "EntryLineTaxAndConfirmedFeeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineConfirmedDutyAndTaxGrid)).EndInit();
			this.EntryLineConfirmedDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineConfirmedDutyAndTaxGrid.PerformLayout();
			this.EntryLineCalculatedDutyAndTaxUserControl.ResumeLayout(true);
			this.EntryLineCalculatedDutyAndTaxUserControl.PerformLayout();
			this.EntryLineConfirmedDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineConfirmedDutyAndTaxGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal EU.GUI.EntryLineTaxAndFeeUserControl EntryLineCalculatedDutyAndTaxUserControl;
		internal ZArchitecture.GUI.ZGroupBox EntryLineConfirmedDutyAndTaxGroupBox;
		internal ZArchitecture.ZGrid EntryLineConfirmedDutyAndTaxGrid;
	}
}
