using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.GUI
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.EntryLineConfirmedDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntryLineDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EntryLineDutyAndTaxGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntryLineConfirmedDutyAndTaxGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineConfirmedDutyAndTaxGrid)).BeginInit();
			this.EntryLineConfirmedDutyAndTaxGrid.SuspendLayout();
			this.EntryLineDutyAndTaxGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).BeginInit();
			this.EntryLineDutyAndTaxGrid.SuspendLayout();
			this.EntryLineConfirmedDutyAndTaxGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.AllCusEntryLineCollection<Enterprise.Customs.EU.Business.Declaration.CusEntryLine>);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineConfirmedFeeWrapper)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).ConfirmedFeesReadOnly)).SyncRoot)).NationalFeeTypeCode)));
			this.EntryLineConfirmedDutyAndTaxGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("7D8C54D4-1622-420A-AD73-7FC5EAB0519B", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("B8800F7B-E130-4A8C-B9A2-116051DC0C59", "Charge Type");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E74934AA-1136-4773-B80B-CF6E269700BB", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("B8800F7B-E130-4A8C-B9A2-116051DC0C59", "Charge Type");
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(183);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("73590BEA-EEAC-4FEB-8634-B8212090C329", "Action");
			zTextBoxColumnStyleInfo3.ColumnName = "CF_RateOverrideReasonCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("6F405D12-709B-4D15-B7E6-16A9FB7F24EC", "Base Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("D5A296D9-8054-4180-879C-4152968A4517", "Method of Calculation");
			zTextBoxColumnStyleInfo4.ColumnName = "CF_MethodOfCalculation";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("71D95BAB-B959-4211-A968-4DC093D3323F", "Tax Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(126);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("2605869E-EA45-4525-919D-4C525C1E8C09", "Total Amount");
			zCalcEditColumnStyleInfo3.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(142);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1B50C1F5-6285-4B59-95C5-511B8BE2BA3C", "Method of Payment");
			zTextBoxColumnStyleInfo5.ColumnName = "CF_MethodOfPayment";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(118);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("A55A683D-8575-4A40-A73F-51FA2E041BE5", "National Type");
			zTextBoxColumnStyleInfo6.ColumnName = "NationalFeeTypeCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntryLineConfirmedDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.EntryLineConfirmedDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineConfirmedDutyAndTaxGrid.GridId = "739D778E-605C-4E72-807A-82215A29F1CB";
			this.EntryLineConfirmedDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineConfirmedDutyAndTaxGrid.LayoutKey = "EntryLineConfirmedDutyAndTaxGrid";
			this.EntryLineConfirmedDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLineConfirmedDutyAndTaxGrid.Name = "EntryLineConfirmedDutyAndTaxGrid";
			this.EntryLineConfirmedDutyAndTaxGrid.ReadOnly = true;
			this.EntryLineConfirmedDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 116, true);
			this.EntryLineConfirmedDutyAndTaxGrid.TabIndex = 1;
			// 
			// EntryLineDutyAndTaxGroupBox
			// 
			this.EntryLineDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("06DF9907-D8C8-412C-B6EB-EA883EC2DABF", "Calculated Duty And Tax");
			this.EntryLineDutyAndTaxGroupBox.Controls.Add(this.EntryLineDutyAndTaxGrid);
			this.EntryLineDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineDutyAndTaxGroupBox, true);
			this.EntryLineDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLineDutyAndTaxGroupBox.Name = "EntryLineDutyAndTaxGroupBox";
			this.EntryLineDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 132, true);
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusEntryLine)(null)).Fees)).SyncRoot)).NationalFeeTypeCode)));
			this.EntryLineDutyAndTaxGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("7D8C54D4-1622-420A-AD73-7FC5EAB0519B", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "CF_ChargeType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("321D28C7-371B-4933-9416-555BBA2266C9", "Charge Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E74934AA-1136-4773-B80B-CF6E269700BB", "Description");
			zTextBoxColumnStyleInfo7.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("321D28C7-371B-4933-9416-555BBA2266C9", "Charge Type");
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(183);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("73590BEA-EEAC-4FEB-8634-B8212090C329", "Action");
			zDropEditColumnStyleInfo2.ColumnName = "CF_RateOverrideReasonCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("6F405D12-709B-4D15-B7E6-16A9FB7F24EC", "Base Amount");
			zCalcEditColumnStyleInfo4.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("D5A296D9-8054-4180-879C-4152968A4517", "Method of Calculation");
			zDropEditColumnStyleInfo3.ColumnName = "CF_MethodOfCalculation";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("71D95BAB-B959-4211-A968-4DC093D3323F", "Tax Rate");
			zCalcEditColumnStyleInfo5.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(126);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("2605869E-EA45-4525-919D-4C525C1E8C09", "Total Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(142);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("1B50C1F5-6285-4B59-95C5-511B8BE2BA3C", "Method of Payment");
			zDropEditColumnStyleInfo4.ColumnName = "CF_MethodOfPayment";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(118);
			zDropEditColumnStyleInfo5.ColumnName = "NationalFeeTypeCode";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.EntryLineDutyAndTaxGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.EntryLineDutyAndTaxGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLineDutyAndTaxGrid.GridId = "65dfe8f0-5f77-4f2b-92c9-13c660e1fa51";
			this.EntryLineDutyAndTaxGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLineDutyAndTaxGrid.LayoutKey = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EntryLineDutyAndTaxGrid.Name = "EntryLineDutyAndTaxGrid";
			this.EntryLineDutyAndTaxGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 113, true);
			this.EntryLineDutyAndTaxGrid.TabIndex = 0;
			// 
			// EntryLineConfirmedDutyAndTaxGroupBox
			// 
			this.EntryLineConfirmedDutyAndTaxGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("20A168E4-AEAF-46C9-B592-47175F5EB51F", "Confirmed Duty And Tax");
			this.EntryLineConfirmedDutyAndTaxGroupBox.Controls.Add(this.EntryLineConfirmedDutyAndTaxGrid);
			this.EntryLineConfirmedDutyAndTaxGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.EntryLineConfirmedDutyAndTaxGroupBox, true);
			this.EntryLineConfirmedDutyAndTaxGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.EntryLineConfirmedDutyAndTaxGroupBox.Name = "EntryLineConfirmedDutyAndTaxGroupBox";
			this.EntryLineConfirmedDutyAndTaxGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 135, true);
			this.EntryLineConfirmedDutyAndTaxGroupBox.TabIndex = 5;
			this.EntryLineConfirmedDutyAndTaxGroupBox.TabStop = false;
			// 
			// EntryLineTaxAndConfirmedFeeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryLineDutyAndTaxGroupBox);
			this.Controls.Add(this.EntryLineConfirmedDutyAndTaxGroupBox);
			this.Name = "EntryLineTaxAndConfirmedFeeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineConfirmedDutyAndTaxGrid)).EndInit();
			this.EntryLineConfirmedDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineConfirmedDutyAndTaxGrid.PerformLayout();
			this.EntryLineDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineDutyAndTaxGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLineDutyAndTaxGrid)).EndInit();
			this.EntryLineDutyAndTaxGrid.ResumeLayout(false);
			this.EntryLineDutyAndTaxGrid.PerformLayout();
			this.EntryLineConfirmedDutyAndTaxGroupBox.ResumeLayout(false);
			this.EntryLineConfirmedDutyAndTaxGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox EntryLineDutyAndTaxGroupBox;
		internal ZArchitecture.ZGrid EntryLineDutyAndTaxGrid;
		internal ZArchitecture.GUI.ZGroupBox EntryLineConfirmedDutyAndTaxGroupBox;
		internal ZArchitecture.ZGrid EntryLineConfirmedDutyAndTaxGrid;
	}
}
