namespace Enterprise.Customs.BE.GUI
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
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IAllCusEntryLineCollection<Enterprise.Customs.BE.Business.Declaration.CusEntryLine>);
            // 
            // EntryLineDutyAndTaxGroupBox
            // 
			this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("0DF39613-1C1E-463C-8680-FA959F56D6CC", "Duty And Tax");
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).ChargeTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_RateOverrideReasonCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_BaseValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfCalculation)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_Rate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfPayment)));
            this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.BE.GUI.Res.GetData("EC73ED11-9936-4600-86A1-4E96FCD36448", "Charge Type");
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("AB36BCAF-7CC5-40C7-81F8-9121772E2CE7", "Description");
            zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.BE.GUI.Res.GetData("EC73ED11-9936-4600-86A1-4E96FCD36448", "Charge Type");
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo2.ColumnName = "CF_RateOverrideReasonCode";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo3.ColumnName = "CF_MethodOfCalculation";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo4.ColumnName = "CF_MethodOfPayment";
            zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
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
            this.EntryLineDutyAndTaxGrid.GridId = "03B345DB-A07D-4813-828B-09A5D9FD164D";
            this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
            this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
            this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
            this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 299, true);
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
		internal ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
	}
}
