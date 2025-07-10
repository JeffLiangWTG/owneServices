namespace Enterprise.Customs.ES.GUI;
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
		this.EntryLineCalculatedDutyAndTaxUserControl = new Enterprise.Customs.ES.GUI.ImportEntryLineTaxAndFeeUserControl();
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
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.IAllCusEntryLineCollection<Enterprise.Customs.ES.Business.Declaration.CusEntryLine>);
		// 
		// EntryLineConfirmedDutyAndTaxGrid
		// 
		this.EntryLineConfirmedDutyAndTaxGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.EntryLineConfirmedDutyAndTaxGrid, "ConfirmedFeesReadOnly");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_ChargeType)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).ChargeTypeDescription)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_RateOverrideReasonCode)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_BaseValue)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_MethodOfCalculation)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_Rate)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_ChargeAmount)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).CF_MethodOfPayment)));
		this.EntryLineConfirmedDutyAndTaxGrid.CaptionVisible = false;
		zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("7766E898-3F32-45E6-8B91-BB24F8FB6AB1", "Type");
		zTextBoxColumnStyleInfo1.ColumnName = "CF_ChargeType";
		zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.ES.GUI.Res.GetData("D1A547F7-BAC7-4B21-A814-9A66EEE618CA", "Charge Type");
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("2EFE8B50-08AF-4114-A842-F9F24CBDD83B", "Description");
		zTextBoxColumnStyleInfo2.ColumnName = "ChargeTypeDescription";
		zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.ES.GUI.Res.GetData("D1A547F7-BAC7-4B21-A814-9A66EEE618CA", "Charge Type");
		zTextBoxColumnStyleInfo2.IsReadOnly = true;
		zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("5C5AB5AA-3A3E-4B4B-854B-3F86938A41EF", "Action");
		zTextBoxColumnStyleInfo3.ColumnName = "CF_RateOverrideReasonCode";
		zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
		zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("285D7004-AA46-4D82-9C21-59601B942A7A", "Base Amount");
		zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
		zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("1C356ACE-B7B4-45A0-80C0-769EE62D67C9", "Method of Calculation");
		zTextBoxColumnStyleInfo4.ColumnName = "CF_MethodOfCalculation";
		zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
		zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("9C881C05-314D-4857-9F03-E205ADB37D58", "Tax Rate");
		zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
		zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
		zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("0C558E27-25FF-404F-A45B-5B0530B10D28", "Total Amount");
		zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
		zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
		zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("99217252-2152-401C-9270-4971C8FCF71A", "Method of Payment");
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
		this.EntryLineConfirmedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("BDF0CE73-28E7-4E25-904D-E4F227FC7BBE", "Customs Duty And Tax");
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

	public ES.GUI.ImportEntryLineTaxAndFeeUserControl EntryLineCalculatedDutyAndTaxUserControl;
	internal ZArchitecture.GUI.ZGroupBox EntryLineConfirmedDutyAndTaxGroupBox;
	public ZArchitecture.ZGrid EntryLineConfirmedDutyAndTaxGrid;
}
