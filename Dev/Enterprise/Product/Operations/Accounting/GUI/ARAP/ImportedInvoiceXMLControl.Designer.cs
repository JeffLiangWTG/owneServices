namespace Enterprise.Accounting.GUI
{
	partial class ImportedInvoiceXMLControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.xmlLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.xmlLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.xmlHeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.placeOfSupplyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlDepartmentNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlBranchNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlCreditorFullNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlLocalVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.xmlLocalExVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.xmlOSGSTVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.xmlOSExGSTVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.xmlNumberOfDocsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.xmlTransactionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.xmlDueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.xmlContactTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlAddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlDepartmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlBranchTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlLocalCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlOSCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlCreditorTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlTransactionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xmlPostDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.xmlDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.xmlLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.xmlLinesGrid)).BeginInit();
			this.xmlLinesGrid.SuspendLayout();
			this.xmlHeaderPanel.SuspendLayout();
			this.xmlTransactionDateEdit.SuspendLayout();
			this.xmlDueDateEdit.SuspendLayout();
			this.xmlPostDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper);
			// 
			// xmlLinesGroupBox
			// 
			this.xmlLinesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b7e6c41b-80fb-4610-9816-d1ecda309828", "Lines");
			this.xmlLinesGroupBox.Controls.Add(this.xmlLinesGrid);
			this.xmlLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xmlLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 282, true);
			this.xmlLinesGroupBox.Name = "xmlLinesGroupBox";
			this.xmlLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 92, true);
			this.xmlLinesGroupBox.TabIndex = 3;
			this.xmlLinesGroupBox.TabStop = false;
			// 
			// xmlLinesGrid
			// 
			this.xmlLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.xmlLinesGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).ChargeCodeSource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).GLAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).Job)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).OSCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).OSAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).VATTaxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).OSGSTVATAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).OSTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).LocalCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).LocalGSTVATAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).LocalTotalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).WithholdingTaxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).OSWHTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).LocalWHTAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).IsFinalCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).Consol)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).TaxMessageID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).RecoverableGSTVATPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).JobConsolXMLData)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).PlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionLineWrapper)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Lines)).SyncRoot)).SubAccounts)));
			this.xmlLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeSource";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "GLAccount";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "Job";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "Branch";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "Department";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "OSCurrency";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OSAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "VATTaxID";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OSGSTVATAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "OSTotalAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "LocalCurrency";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "LocalGSTVATAmount";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "LocalTotalAmount";
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "WithholdingTaxID";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "OSWHTAmount";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "LocalWHTAmount";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsFinalCharge";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "Consol";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.ColumnName = "TaxMessageID";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "RecoverableGSTVATPercentage";
			zCalcEditColumnStyleInfo10.IsVisible = false;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JobConsolXMLData";
			zMultiLineTextBoxColumnInfo1.IsSortable = false;
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.ColumnName = "PlaceOfSupply";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "SubAccounts";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.xmlLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.xmlLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.xmlLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.xmlLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.xmlLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.xmlLinesGrid.GridId = "30124155-f7aa-4f93-8c7c-b7117cb48202";
			this.xmlLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.xmlLinesGrid.LayoutKey = "xmlLinesGrid";
			this.xmlLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.xmlLinesGrid.Name = "xmlLinesGrid";
			this.xmlLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 76, true);
			this.xmlLinesGrid.TabIndex = 0;
			// 
			// xmlHeaderPanel
			// 
			this.xmlHeaderPanel.Controls.Add(this.placeOfSupplyTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlDepartmentNameTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlBranchNameTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlCreditorFullNameTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlLocalVATAmountCalcEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlLocalExVATAmountCalcEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlOSGSTVATAmountCalcEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlOSExGSTVATAmountCalcEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlNumberOfDocsCalcEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlTransactionDateEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlDueDateEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlContactTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlAddressTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlDepartmentTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlBranchTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlLocalCurrencyTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlOSCurrencyTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlCreditorTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlTransactionNumberTextBox);
			this.xmlHeaderPanel.Controls.Add(this.xmlPostDateEdit);
			this.xmlHeaderPanel.Controls.Add(this.xmlDescriptionTextBox);
			this.xmlHeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.xmlHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.xmlHeaderPanel.Name = "xmlHeaderPanel";
			this.xmlHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 282, true);
			this.xmlHeaderPanel.TabIndex = 2;
			// 
			// xmlDepartmentNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlDepartmentNameTextBox, "DepartmentName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).DepartmentName)));
			this.xmlDepartmentNameTextBox.CaptionResourceString = null;
			this.xmlDepartmentNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.xmlDepartmentNameTextBox, false);
			this.xmlDepartmentNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 210, true);
			this.xmlDepartmentNameTextBox.Name = "xmlDepartmentNameTextBox";
			this.xmlDepartmentNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.xmlDepartmentNameTextBox.TabIndex = 17;
			// 
			// xmlBranchNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlBranchNameTextBox, "BranchName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).BranchName)));
			this.xmlBranchNameTextBox.CaptionResourceString = null;
			this.xmlBranchNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.xmlBranchNameTextBox, false);
			this.xmlBranchNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 187, true);
			this.xmlBranchNameTextBox.Name = "xmlBranchNameTextBox";
			this.xmlBranchNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.xmlBranchNameTextBox.TabIndex = 15;
			// 
			// xmlCreditorFullNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlCreditorFullNameTextBox, "CreditorFullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).CreditorFullName)));
			this.xmlCreditorFullNameTextBox.CaptionResourceString = null;
			this.xmlCreditorFullNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.xmlCreditorFullNameTextBox, false);
			this.xmlCreditorFullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 49, true);
			this.xmlCreditorFullNameTextBox.Name = "xmlCreditorFullNameTextBox";
			this.xmlCreditorFullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 17, true);
			this.xmlCreditorFullNameTextBox.TabIndex = 5;
			// 
			// xmlLocalVATAmountCalcEdit
			// 
			this.xmlLocalVATAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.xmlLocalVATAmountCalcEdit, "LocalVATAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).LocalVATAmount)));
			this.xmlLocalVATAmountCalcEdit.CaptionResourceString = null;
			this.xmlLocalVATAmountCalcEdit.DecimalPlaces = 2;
			this.xmlLocalVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 141, true);
			this.xmlLocalVATAmountCalcEdit.Name = "xmlLocalVATAmountCalcEdit";
			this.xmlLocalVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.xmlLocalVATAmountCalcEdit.TabIndex = 12;
			this.xmlLocalVATAmountCalcEdit.Text = "0";
			this.xmlLocalVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// xmlLocalExVATAmountCalcEdit
			// 
			this.xmlLocalExVATAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.xmlLocalExVATAmountCalcEdit, "LocalExVATAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).LocalExVATAmount)));
			this.xmlLocalExVATAmountCalcEdit.CaptionResourceString = null;
			this.xmlLocalExVATAmountCalcEdit.DecimalPlaces = 2;
			this.xmlLocalExVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 118, true);
			this.xmlLocalExVATAmountCalcEdit.Name = "xmlLocalExVATAmountCalcEdit";
			this.xmlLocalExVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.xmlLocalExVATAmountCalcEdit.TabIndex = 10;
			this.xmlLocalExVATAmountCalcEdit.Text = "0";
			this.xmlLocalExVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// xmlOSGSTVATAmountCalcEdit
			// 
			this.xmlOSGSTVATAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.xmlOSGSTVATAmountCalcEdit, "OSGSTVATAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).OSGSTVATAmount)));
			this.xmlOSGSTVATAmountCalcEdit.CaptionResourceString = null;
			this.xmlOSGSTVATAmountCalcEdit.DecimalPlaces = 2;
			this.xmlOSGSTVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 141, true);
			this.xmlOSGSTVATAmountCalcEdit.Name = "xmlOSGSTVATAmountCalcEdit";
			this.xmlOSGSTVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.xmlOSGSTVATAmountCalcEdit.TabIndex = 11;
			this.xmlOSGSTVATAmountCalcEdit.Text = "0";
			this.xmlOSGSTVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// xmlOSExGSTVATAmountCalcEdit
			// 
			this.xmlOSExGSTVATAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.xmlOSExGSTVATAmountCalcEdit, "OSExGSTVATAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).OSExGSTVATAmount)));
			this.xmlOSExGSTVATAmountCalcEdit.CaptionResourceString = null;
			this.xmlOSExGSTVATAmountCalcEdit.DecimalPlaces = 2;
			this.xmlOSExGSTVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 118, true);
			this.xmlOSExGSTVATAmountCalcEdit.Name = "xmlOSExGSTVATAmountCalcEdit";
			this.xmlOSExGSTVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.xmlOSExGSTVATAmountCalcEdit.TabIndex = 9;
			this.xmlOSExGSTVATAmountCalcEdit.Text = "0";
			this.xmlOSExGSTVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// xmlNumberOfDocsCalcEdit
			// 
			this.xmlNumberOfDocsCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.xmlNumberOfDocsCalcEdit, "NumberOfSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).NumberOfSupportingDocuments)));
			this.xmlNumberOfDocsCalcEdit.CaptionResourceString = null;
			this.xmlNumberOfDocsCalcEdit.DecimalPlaces = 2;
			this.xmlNumberOfDocsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 26, true);
			this.xmlNumberOfDocsCalcEdit.Name = "xmlNumberOfDocsCalcEdit";
			this.xmlNumberOfDocsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 17, true);
			this.xmlNumberOfDocsCalcEdit.TabIndex = 3;
			this.xmlNumberOfDocsCalcEdit.Text = "0";
			this.xmlNumberOfDocsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// xmlTransactionDateEdit
			// 
			this.xmlTransactionDateEdit.AllowDrop = true;
			this.xmlTransactionDateEdit.AutoCompleteMonthThreshold = 1;
			this.xmlTransactionDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.xmlTransactionDateEdit, "TransactionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).TransactionDate)));
			this.xmlTransactionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 3, true);
			this.xmlTransactionDateEdit.Name = "xmlTransactionDateEdit";
			this.xmlTransactionDateEdit.TabIndex = 0;
			// 
			// xmlDueDateEdit
			// 
			this.xmlDueDateEdit.AllowDrop = true;
			this.xmlDueDateEdit.AutoCompleteMonthThreshold = 1;
			this.xmlDueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.xmlDueDateEdit, "DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).DueDate)));
			this.xmlDueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 72, true);
			this.xmlDueDateEdit.Name = "xmlDueDateEdit";
			this.xmlDueDateEdit.TabIndex = 6;
			// 
			// xmlContactTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlContactTextBox, "Contact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Contact)));
			this.xmlContactTextBox.CaptionResourceString = null;
			this.xmlContactTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.xmlContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 256, true);
			this.xmlContactTextBox.Name = "xmlContactTextBox";
			this.xmlContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.xmlContactTextBox.TabIndex = 19;
			// 
			// xmlAddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlAddressTextBox, "Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Address)));
			this.xmlAddressTextBox.CaptionResourceString = null;
			this.xmlAddressTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.xmlAddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 233, true);
			this.xmlAddressTextBox.Name = "xmlAddressTextBox";
			this.xmlAddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 17, true);
			this.xmlAddressTextBox.TabIndex = 18;
			// 
			// xmlDepartmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlDepartmentTextBox, "Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Department)));
			this.xmlDepartmentTextBox.CaptionResourceString = null;
			this.xmlDepartmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 210, true);
			this.xmlDepartmentTextBox.Name = "xmlDepartmentTextBox";
			this.xmlDepartmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.xmlDepartmentTextBox.TabIndex = 16;
			// 
			// xmlBranchTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlBranchTextBox, "Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Branch)));
			this.xmlBranchTextBox.CaptionResourceString = null;
			this.xmlBranchTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 187, true);
			this.xmlBranchTextBox.Name = "xmlBranchTextBox";
			this.xmlBranchTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.xmlBranchTextBox.TabIndex = 14;
			// 
			// xmlLocalCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlLocalCurrencyTextBox, "LocalCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).LocalCurrency)));
			this.xmlLocalCurrencyTextBox.CaptionResourceString = null;
			this.xmlLocalCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 95, true);
			this.xmlLocalCurrencyTextBox.Name = "xmlLocalCurrencyTextBox";
			this.xmlLocalCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.xmlLocalCurrencyTextBox.TabIndex = 8;
			// 
			// xmlOSCurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlOSCurrencyTextBox, "OSCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).OSCurrency)));
			this.xmlOSCurrencyTextBox.CaptionResourceString = null;
			this.xmlOSCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 95, true);
			this.xmlOSCurrencyTextBox.Name = "xmlOSCurrencyTextBox";
			this.xmlOSCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.xmlOSCurrencyTextBox.TabIndex = 7;
			// 
			// xmlCreditorTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlCreditorTextBox, "CreditorSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).CreditorSource)));
			this.xmlCreditorTextBox.CaptionResourceString = null;
			this.xmlCreditorTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 49, true);
			this.xmlCreditorTextBox.Name = "xmlCreditorTextBox";
			this.xmlCreditorTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.xmlCreditorTextBox.TabIndex = 4;
			// 
			// xmlTransactionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.xmlTransactionNumberTextBox, "TransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).TransactionNumber)));
			this.xmlTransactionNumberTextBox.CaptionResourceString = null;
			this.xmlTransactionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 3, true);
			this.xmlTransactionNumberTextBox.Name = "xmlTransactionNumberTextBox";
			this.xmlTransactionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			this.xmlTransactionNumberTextBox.TabIndex = 1;
			// 
			// xmlPostDateEdit
			// 
			this.xmlPostDateEdit.AllowDrop = true;
			this.xmlPostDateEdit.AutoCompleteMonthThreshold = 1;
			this.xmlPostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.xmlPostDateEdit, "PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).PostDate)));
			this.xmlPostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 26, true);
			this.xmlPostDateEdit.Name = "xmlPostDateEdit";
			this.xmlPostDateEdit.TabIndex = 2;
			// 
			// xmlDescriptionTextBox
			// 
			this.xmlDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.xmlDescriptionTextBox, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).Description)));
			this.xmlDescriptionTextBox.CaptionResourceString = null;
			this.xmlDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.xmlDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 164, true);
			this.xmlDescriptionTextBox.Name = "xmlDescriptionTextBox";
			this.xmlDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 17, true);
			this.xmlDescriptionTextBox.TabIndex = 13;
			// 
			// placeOfSupplyTextBox
			// 
			this.BindingSource.SetBindingMember(this.placeOfSupplyTextBox, "PlaceOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper)(null)).PlaceOfSupply)));
			this.placeOfSupplyTextBox.CaptionResourceString = null;
			this.placeOfSupplyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 69, true);
			this.placeOfSupplyTextBox.Name = "placeOfSupplyTextBox";
			this.placeOfSupplyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.placeOfSupplyTextBox.TabIndex = 20;
			// 
			// ImportedInvoiceXMLControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.xmlLinesGroupBox);
			this.Controls.Add(this.xmlHeaderPanel);
			this.Name = "ImportedInvoiceXMLControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 374, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.xmlLinesGroupBox.ResumeLayout(false);
			this.xmlLinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.xmlLinesGrid)).EndInit();
			this.xmlLinesGrid.ResumeLayout(false);
			this.xmlLinesGrid.PerformLayout();
			this.xmlHeaderPanel.ResumeLayout(false);
			this.xmlHeaderPanel.PerformLayout();
			this.xmlTransactionDateEdit.ResumeLayout(true);
			this.xmlTransactionDateEdit.PerformLayout();
			this.xmlDueDateEdit.ResumeLayout(true);
			this.xmlDueDateEdit.PerformLayout();
			this.xmlPostDateEdit.ResumeLayout(true);
			this.xmlPostDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox xmlLinesGroupBox;
		private ZArchitecture.ZGrid xmlLinesGrid;
		private ZArchitecture.GUI.ZPanel xmlHeaderPanel;
		private ZArchitecture.ZCalcEdit xmlLocalVATAmountCalcEdit;
		private ZArchitecture.ZCalcEdit xmlLocalExVATAmountCalcEdit;
		private ZArchitecture.ZCalcEdit xmlOSGSTVATAmountCalcEdit;
		private ZArchitecture.ZCalcEdit xmlOSExGSTVATAmountCalcEdit;
		private ZArchitecture.ZCalcEdit xmlNumberOfDocsCalcEdit;
		private ZArchitecture.ZTextBox xmlDepartmentNameTextBox;
		private ZArchitecture.ZTextBox xmlBranchNameTextBox;
		private ZArchitecture.ZTextBox xmlCreditorFullNameTextBox;
		private ZArchitecture.GUI.ZDateEdit xmlTransactionDateEdit;
		private ZArchitecture.GUI.ZDateEdit xmlDueDateEdit;
		private ZArchitecture.ZTextBox xmlContactTextBox;
		private ZArchitecture.ZTextBox xmlAddressTextBox;
		private ZArchitecture.ZTextBox xmlDepartmentTextBox;
		private ZArchitecture.ZTextBox xmlBranchTextBox;
		private ZArchitecture.ZTextBox xmlLocalCurrencyTextBox;
		private ZArchitecture.ZTextBox xmlOSCurrencyTextBox;
		private ZArchitecture.ZTextBox xmlCreditorTextBox;
		private ZArchitecture.ZTextBox xmlTransactionNumberTextBox;
		private ZArchitecture.GUI.ZDateEdit xmlPostDateEdit;
		private ZArchitecture.ZTextBox xmlDescriptionTextBox;
		private ZArchitecture.ZTextBox placeOfSupplyTextBox;
	}
}
