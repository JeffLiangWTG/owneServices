namespace Enterprise.Customs.IL.GUI
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.EntryLineConfirmedDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
            this.EntryLineCalculatedDutyAndTaxUserControl = new Enterprise.Customs.IL.GUI.EntryLineTaxAndFeeUserControl();
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
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IAllCusEntryLineCollection<Enterprise.Customs.IL.Business.CusEntryLine>);
            // 
            // EntryLineConfirmedDutyAndTaxGrid
            // 
            this.EntryLineConfirmedDutyAndTaxGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.EntryLineConfirmedDutyAndTaxGrid, "ConfirmedFeesReadOnly");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_ChargeType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).ChargeTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_RateOverrideReasonCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_BaseValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_MethodOfCalculation)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_Rate)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_ChargeAmount)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_MethodOfPayment)));
            this.EntryLineConfirmedDutyAndTaxGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("4A78ED17-1C6B-4A66-88F2-59BE2E5B00E4", "Type");
            zTextBoxColumnStyleInfo1.ColumnName = "CF_ChargeType";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.IL.GUI.Res.GetData("F3A1949D-B460-463E-8B54-86EE445608A3", "Charge Type");
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("8191BFBB-9CC8-4906-A8F7-1B5415302718", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "ChargeTypeDescription";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.IL.GUI.Res.GetData("F3A1949D-B460-463E-8B54-86EE445608A3", "Charge Type");
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("3D64ECAA-E30F-472E-9423-94696F713A0A", "Action");
            zTextBoxColumnStyleInfo3.ColumnName = "CF_RateOverrideReasonCode";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("5F0810E8-0295-485A-A870-C3CED58366D3", "Base Amount");
            zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("357BA2BF-7E6A-4D8B-9E5A-6BC2BFF368D8", "Method of Calculation");
            zTextBoxColumnStyleInfo4.ColumnName = "CF_MethodOfCalculation";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.IsVisible = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("5CB8AF98-7CD6-48A2-A450-CECECB66BB29", "Tax Rate");
            zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("2D7CE2CF-B06F-4296-86AA-549807DA35B8", "Total Amount");
            zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("71E27E95-5986-46A8-8EC6-71189C4070F3", "Method of Payment");
            zTextBoxColumnStyleInfo5.ColumnName = "CF_MethodOfPayment";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsVisible = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.EntryLineConfirmedDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EntryLineConfirmedDutyAndTaxGrid.GridId = "739D778E-605C-4E72-807A-82215A29F1CB";
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
            this.EntryLineConfirmedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("4EB30549-F059-4CFC-98AB-4B968CD202D8", "Confirmed Duties And Taxes");
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

		public IL.GUI.EntryLineTaxAndFeeUserControl EntryLineCalculatedDutyAndTaxUserControl;
		internal ZArchitecture.GUI.ZGroupBox EntryLineConfirmedDutyAndTaxGroupBox;
		public ZArchitecture.ZGrid EntryLineConfirmedDutyAndTaxGrid;
	}
}
