namespace Enterprise.Customs.CA.GUI
{
	partial class EntryLinesTabUserControl
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EntryLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CenterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DutiesAndTaxesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AmendmentDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).BeginInit();
			this.EntryLinesGrid.SuspendLayout();
			this.CenterPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DutiesAndTaxesGrid)).BeginInit();
			this.DutiesAndTaxesGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AmendmentDetailsGrid)).BeginInit();
			this.AmendmentDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.EntryLinesGrid);
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 214, true);
			this.TopPanel.TabIndex = 0;
			// 
			// EntryLinesGrid
			// 
			this.EntryLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryLinesGrid, "CustomsEntryHeaders.AllEntryLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).SequenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).LineSubmissionStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_CustomsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CustomsUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).DutyAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).TotalLinePriceInLocalCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).CL_DutyPercent)));
			this.EntryLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2a50cce7-22af-4053-aed8-3f456f16ae3d", "Seq No.");
			zTextBoxColumnStyleInfo1.ColumnName = "SequenceNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("792e4846-2795-43c0-8d91-51d0c8f78631", "Status");
			zTextBoxColumnStyleInfo2.ColumnName = "LineSubmissionStatusDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("a8b11d29-9e02-4e28-8a74-fc0b375909ca", "Value");
			zCalcEditColumnStyleInfo1.ColumnName = "CL_CustomsValue";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e4b602d3-7a5b-49a0-9c76-9b8ab18fe2bf", "Tariff");
			zTextBoxColumnStyleInfo3.ColumnName = "FormattedTariff";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("46a1a24c-1641-472a-97db-3408c96ae0ab", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "CL_Description";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("be680a7a-5f1b-49f8-82fa-32e6a557db9c", "Qty");
			zCalcEditColumnStyleInfo2.ColumnName = "CustomsQuantity";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("caeae8fa-3a52-4f02-b429-1a1370683f93", "Units");
			zTextBoxColumnStyleInfo5.ColumnName = "CustomsUnitQty";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("93a86e87-77bb-45ce-814d-acbb6eaf3a53", "Duty");
			zCalcEditColumnStyleInfo3.ColumnName = "DutyAmount";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("731775bf-84b1-4475-aeb3-06a6f3a06f82", "Total Price");
			zCalcEditColumnStyleInfo4.ColumnName = "TotalLinePriceInLocalCurrency";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b4900aad-d4b2-44a0-8b9d-cff78aceb72d", "Duty %");
			zCalcEditColumnStyleInfo5.ColumnName = "CL_DutyPercent";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.EntryLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.EntryLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.EntryLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesGrid.GridId = "a03d76d3-cedf-4df3-a9d4-691187f26f4e";
			this.EntryLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryLinesGrid.LayoutKey = "EntryLinesGrid";
			this.EntryLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryLinesGrid.Name = "EntryLinesGrid";
			this.EntryLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 214, true);
			this.EntryLinesGrid.TabIndex = 0;
			// 
			// CenterPanel
			// 
			this.CenterPanel.Controls.Add(this.DutiesAndTaxesGrid);
			this.CenterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 217, true);
			this.CenterPanel.Name = "CenterPanel";
			this.CenterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 168, true);
			this.CenterPanel.TabIndex = 1;
			// 
			// DutiesAndTaxesGrid
			// 
			this.DutiesAndTaxesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DutiesAndTaxesGrid, "CustomsEntryHeaders.AllEntryLines.ConfirmedFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)).SyncRoot)).CF_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)).SyncRoot)).CF_ChargeAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)).SyncRoot)).CF_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)).SyncRoot)).CF_MethodOfCalculation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)).SyncRoot)).CF_BaseValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusEntryLineFee)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).ConfirmedFees)).SyncRoot)).CF_Source)));
			this.DutiesAndTaxesGrid.CaptionText = "Duty & Tax";
			this.DutiesAndTaxesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8868A26E-4212-4A8B-92BA-D01C46B3A8D9", "Type");
			zTextBoxColumnStyleInfo6.ColumnName = "CF_ChargeType";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("E7EB8ECD-E10C-4248-BF9D-3BF8BB8D4954", "Amount");
			zCalcEditColumnStyleInfo6.ColumnName = "CF_ChargeAmount";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6AF791BE-6B65-40AB-8A02-75E96DEF852E", "Rate");
			zCalcEditColumnStyleInfo7.ColumnName = "CF_Rate";
			zCalcEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("DA96301C-9DDF-4B8E-AF92-6CF1BE0F1DDD", "Rate Type");
			zTextBoxColumnStyleInfo7.ColumnName = "CF_MethodOfCalculation";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B14DD1FC-7F29-4A2D-BE6B-0A2A6197A2F0", "Base Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "CF_BaseValue";
			zCalcEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo8.IsReadOnly = true;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c848d1bf-8d9c-4ee6-a14b-8bfc075a26a3", "Source");
			zTextBoxColumnStyleInfo8.ColumnName = "CF_Source";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.DutiesAndTaxesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DutiesAndTaxesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DutiesAndTaxesGrid.GridId = "47e89880-412a-4336-9d30-26f905f32f35";
			this.DutiesAndTaxesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DutiesAndTaxesGrid.LayoutKey = "DutiesAndTaxesGrid";
			this.DutiesAndTaxesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DutiesAndTaxesGrid.Name = "DutiesAndTaxesGrid";
			this.DutiesAndTaxesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 168, true);
			this.DutiesAndTaxesGrid.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.AmendmentDetailsGrid);
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 388, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 97, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// AmendmentDetailsGrid
			// 
			this.AmendmentDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AmendmentDetailsGrid, "CustomsEntryHeaders.AllEntryLines.AmendmentDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AmendmentDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AmendmentDetails)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AmendmentDetails)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AmendmentDetails)).SyncRoot)).ReasonCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AmendmentDetails)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AmendmentDetails)).SyncRoot)).AppealsProgramCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).AllEntryLines)).SyncRoot)).AmendmentDetails)).SyncRoot)).CSI_Description)));
			this.AmendmentDetailsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo9.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("F1D72BFE-D1DE-42E8-B75E-253694C6A93D", "Reason Code");
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("AD2E00A3-EF21-4EBB-8066-83CB082AD194", "Description");
			zTextBoxColumnStyleInfo10.ColumnName = "ReasonCodeDescription";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("F1D72BFE-D1DE-42E8-B75E-253694C6A93D", "Reason Code");
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "CSI_SubType";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("20D983E0-CCC5-47D8-9EDB-6A067AD231A9", "Appeals Program Code");
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("32BFF65B-F55D-4560-AC57-DE160F864FBE", "Description");
			zTextBoxColumnStyleInfo12.ColumnName = "AppealsProgramCodeDescription";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.GroupName = Enterprise.Customs.CA.GUI.Res.GetData("20D983E0-CCC5-47D8-9EDB-6A067AD231A9", "Appeals Program Code");
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "CSI_Description";
			zMultiLineTextBoxColumnInfo1.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo1.IsReadOnly = true;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 350;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.AmendmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.AmendmentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.AmendmentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.AmendmentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.AmendmentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.AmendmentDetailsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.AmendmentDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AmendmentDetailsGrid.GridId = "66961096-b592-4f4a-903c-30c3058b90fc";
			this.AmendmentDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AmendmentDetailsGrid.LayoutKey = "AmendmentDetailsGrid";
			this.AmendmentDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AmendmentDetailsGrid.Name = "AmendmentDetailsGrid";
			this.AmendmentDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 97, true);
			this.AmendmentDetailsGrid.TabIndex = 0;
			// 
			// EntryLinesTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.CenterPanel);
			this.Controls.Add(this.BottomPanel);
			this.Name = "EntryLinesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 487, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryLinesGrid)).EndInit();
			this.EntryLinesGrid.ResumeLayout(false);
			this.EntryLinesGrid.PerformLayout();
			this.CenterPanel.ResumeLayout(false);
			this.CenterPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DutiesAndTaxesGrid)).EndInit();
			this.DutiesAndTaxesGrid.ResumeLayout(false);
			this.DutiesAndTaxesGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AmendmentDetailsGrid)).EndInit();
			this.AmendmentDetailsGrid.ResumeLayout(false);
			this.AmendmentDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel CenterPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.ZArchitecture.ZGrid EntryLinesGrid;
		private Enterprise.ZArchitecture.ZGrid DutiesAndTaxesGrid;
		private Enterprise.ZArchitecture.ZGrid AmendmentDetailsGrid;
	}
}
