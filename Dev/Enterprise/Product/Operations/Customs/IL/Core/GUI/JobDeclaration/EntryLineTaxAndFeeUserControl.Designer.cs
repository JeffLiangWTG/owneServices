namespace Enterprise.Customs.IL.GUI
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
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IAllCusEntryLineCollection<Enterprise.Customs.IL.Business.CusEntryLine>);
            // 
            // EntryLineDutyAndTaxGroupBox
            // 
            this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("D9B10E8E-9A6B-41BB-A158-1646F1E3B38B", "Duty And Tax");
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).ChargeTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).CF_RateOverrideReasonCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).CF_BaseValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfCalculation)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).CF_Rate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).CF_ChargeAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).Fees)).SyncRoot)).CF_MethodOfPayment)));
            this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("476979BB-6643-4206-9F6C-DD0C1BE2AB19", "Type");
            zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.IL.GUI.Res.GetData("49E81C45-DF36-4F9C-AC42-F3A0D954578F", "Charge Type");
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("E0C1859C-70C6-49B2-BEC2-31BB9F897E97", "Description");
            zTextBoxColumnStyleInfo1.ColumnName = "ChargeTypeDescription";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.IL.GUI.Res.GetData("49E81C45-DF36-4F9C-AC42-F3A0D954578F", "Charge Type");
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("EF254617-ACBB-4DC4-A07C-C51359C252E8", "Action");
            zDropEditColumnStyleInfo2.ColumnName = "CF_RateOverrideReasonCode";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("C6E36003-23DB-4F7D-89ED-EF33BE7E56C8", "Base Amount");
            zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("94D83F5F-C5F6-4D4C-A14E-EA69E645EB1B", "Method of Calculation");
            zDropEditColumnStyleInfo3.ColumnName = "CF_MethodOfCalculation";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.IsVisible = false;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("49B903BC-564B-4F91-8723-ABCA02B6DD7C", "Tax Rate");
            zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("1FA555C6-4693-4FEE-AAA8-856962E722D0", "Total Amount");
            zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("544BF3E9-6910-4F3F-8268-9F75131CEBB9", "Method of Payment");
            zDropEditColumnStyleInfo4.ColumnName = "CF_MethodOfPayment";
            zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo4.IsVisible = false;
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
