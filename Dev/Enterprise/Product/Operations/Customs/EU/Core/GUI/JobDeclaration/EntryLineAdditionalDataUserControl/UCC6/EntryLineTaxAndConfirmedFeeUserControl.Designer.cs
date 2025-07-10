namespace Enterprise.Customs.EU.GUI
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IAllCusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>);
			// 
			// EntryLineConfirmedDutyAndTaxGrid
			// 
			this.EntryLineConfirmedDutyAndTaxGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLineConfirmedDutyAndTaxGrid, "ConfirmedFeesReadOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_RateOverrideReasonCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_MethodOfPayment)));
			this.EntryLineConfirmedDutyAndTaxGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A5A4934D-B40A-4DDD-BDD2-5046DAF7D509", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("9926DB68-0A1F-4F40-9662-CF658D3D8E8B", "Charge Type");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("6DAD86CA-7B6D-4224-8A21-C9CB175332C4", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.EU.GUI.Res.GetData("9926DB68-0A1F-4F40-9662-CF658D3D8E8B", "Charge Type");
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("D957A2C1-847B-4394-B355-F237C41D340A", "Action");
			zTextBoxColumnStyleInfo3.ColumnName = "CF_RateOverrideReasonCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9412539B-E489-469A-A5EE-00FE42D7EAF8", "Base Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("D15E364D-FE85-4F08-890E-989B8A38F349", "Method of Calculation");
			zTextBoxColumnStyleInfo4.ColumnName = "CF_MethodOfCalculation";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("87D4D626-92D1-4DFB-AB57-C7FDFAB5A3E6", "Tax Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("75A49D55-6E3A-41E7-ADD5-7D74DDE2EE97", "Total Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A2406CE3-7823-4C23-8565-73DEBB93F26E", "Method of Payment");
			zTextBoxColumnStyleInfo5.ColumnName = "CF_MethodOfPayment";
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
			this.EntryLineConfirmedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("BB1F3C1A-0F53-4308-B903-4E5804C64C77", "Confirmed Duties And Taxes");
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

		public EU.GUI.EntryLineTaxAndFeeUserControl EntryLineCalculatedDutyAndTaxUserControl;
		internal ZArchitecture.GUI.ZGroupBox EntryLineConfirmedDutyAndTaxGroupBox;
		public ZArchitecture.ZGrid EntryLineConfirmedDutyAndTaxGrid;
	}
}
