namespace Enterprise.Customs.EU.GUI
{
	partial class EntryLineTaxAndFeeUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.EntryLineDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryLineDutyAndTaxGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).BeginInit();
			this.EntryLineDutyAndTaxGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IAllCusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>);
			// 
			// EntryLineDutyAndTaxGroupBox
			// 
			this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A35180F8-25B4-479B-BA97-78FACA5619BF", "Duty And Tax");
			this.EntryLineDutyAndTaxGroupBox.Controls.Add(this.EntryLineDutyAndTaxGrid);
			this.EntryLineDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineDutyAndTaxGroupBox, true);
			this.EntryLineDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLineDutyAndTaxGroupBox.Name = "EntryLineDutyAndTaxGroupBox";
			this.EntryLineDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 314, true);
			this.EntryLineDutyAndTaxGroupBox.TabIndex = 5;
			this.EntryLineDutyAndTaxGroupBox.TabStop = false;
			// 
			// EntryLineDutyAndTaxGrid
			// 
			this.EntryLineDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineDutyAndTaxGrid, "Fees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_RateOverrideReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfPayment)));
			this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("a284a7a0-6f1b-4980-ac4d-b0ac861ea603", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("7116D434-32A6-495D-8CC2-31609E454702", "Charge Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("8CCFCB25-7982-4042-990D-6D44FB1ECFDD", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("7116D434-32A6-495D-8CC2-31609E454702", "Charge Type");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7385b7dd-4310-46df-bcae-5d180ff524df", "Action");
			zDropEditColumnStyleInfo2.ColumnName = "CF_RateOverrideReasonCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("2d476748-ac32-47fe-a7a6-4058cc6f3d36", "Base Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("379b92e2-ea7c-44d1-8206-b04293e3fe70", "Method of Calculation");
			zDropEditColumnStyleInfo3.ColumnName = "CF_MethodOfCalculation";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("b06eca36-32aa-46ae-b5b5-4b777924f8fa", "Tax Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("59fd0ed1-8ac7-4f2c-8cd8-cfdee8984a68", "Total Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4e4a6b38-2a05-4e05-acd3-ad54458e1409", "Method of Payment");
			zDropEditColumnStyleInfo4.ColumnName = "CF_MethodOfPayment";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EntryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineDutyAndTaxGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
			this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 295, true);
			this.EntryLineDutyAndTaxGrid.TabIndex = 0;
			// 
			// EntryLineTaxAndFeeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryLineDutyAndTaxGroupBox);
			this.Name = "EntryLineTaxAndFeeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(923, 314, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryLineDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineDutyAndTaxGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).EndInit();
			this.EntryLineDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineDutyAndTaxGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox EntryLineDutyAndTaxGroupBox;
		public ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
	}
}
