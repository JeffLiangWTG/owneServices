using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.eNett_Integration;
using Enterprise.Accounting.GUI.eNett;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class ContainerStorageInvoiceUserControl
	{


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			this.HeaderGroupBox = new ZGroupBox();
			this.InvoiceLinesGroupBox = new ZGroupBox();
			this.TransactionLinesGrid = new ZGrid();
			this.InvoiceDetailsGroupBox = new ZGroupBox();
			this.ChargeCodeGuidFindBox = new ZGuidFindBox();
			this.APInvoiceNumberTextBox = new ZTextBox();
			this.RetrieveStorageFeesButton = new ZButton();
			this.StorageChargesCalcFindBox = new ZCalcFindBox();
			this.AH_DescTextbox = new ZTextBox();
			this.ApportionmentMethodDropEdit = new ZDropEdit();
			this.OrganisationDetailsGroupBox = new ZGroupBox();
			this.ContainerNumberTextBox = new ZTextBox();
			this.zLabel1 = new ZLabel();
			this.PickupDateDateEdit = new ZDateEdit();
			this.SelectOrgButton = new ZButton();
			this.CreditorGuidFindBox = new ZGuidFindBox();
			this.PortCodeTextBox = new ZTextBox();
			this.PaymentDetailsGroupBox = new ZGroupBox();
			this.zDropEdit1 = new ZDropEdit();
			this.BankAccountGuidFindBox = new ZGuidFindBox();
			this.ChequeOrReferenceTextBox = new ZTextBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderGroupBox.SuspendLayout();
			this.InvoiceLinesGroupBox.SuspendLayout();
			((ISupportInitialize)(this.TransactionLinesGrid)).BeginInit();
			this.TransactionLinesGrid.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ChargeCodeGuidFindBox.SuspendLayout();
			this.StorageChargesCalcFindBox.SuspendLayout();
			this.ApportionmentMethodDropEdit.SuspendLayout();
			this.OrganisationDetailsGroupBox.SuspendLayout();
			this.PickupDateDateEdit.SuspendLayout();
			this.CreditorGuidFindBox.SuspendLayout();
			this.PaymentDetailsGroupBox.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.BankAccountGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(StorageFeeInvoicePayment);
			// 
			// HeaderGroupBox
			// 
			this.HeaderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HeaderGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|15965f2c-d599-4090-9fdb-5f8ef8babbbc", "Invoice Summary");
			this.HeaderGroupBox.Controls.Add(this.InvoiceLinesGroupBox);
			this.HeaderGroupBox.Controls.Add(this.InvoiceDetailsGroupBox);
			this.HeaderGroupBox.Controls.Add(this.OrganisationDetailsGroupBox);
			this.HeaderGroupBox.Controls.Add(this.PaymentDetailsGroupBox);
			this.HeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderGroupBox.Name = "HeaderGroupBox";
			this.HeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 531, true);
			this.HeaderGroupBox.TabIndex = 0;
			this.HeaderGroupBox.TabStop = false;
			// 
			// InvoiceLinesGroupBox
			// 
			this.InvoiceLinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceLinesGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|fab772a6-1db2-4e19-b60c-ede51e3d47b7", "Job Allocation of Storage Charges");
			this.InvoiceLinesGroupBox.Controls.Add(this.TransactionLinesGrid);
			this.InvoiceLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 327, true);
			this.InvoiceLinesGroupBox.Name = "InvoiceLinesGroupBox";
			this.InvoiceLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 198, true);
			this.InvoiceLinesGroupBox.TabIndex = 3;
			this.InvoiceLinesGroupBox.TabStop = false;
			// 
			// TransactionLinesGrid
			// 
			this.TransactionLinesGrid.AllowNavigation = false;
			this.TransactionLinesGrid.AllowSorting = false;
			this.TransactionLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransactionLinesGrid, "Invoice.Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).GenericCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).ChargeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).JobCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).DepartmentCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_OSExTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).Lookups.TaxRates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_OSTaxAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).GSTInclusiveAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_IsFinalCharge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_ExchangeRate_Decimals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((Business.ARAP.Invoicing.InvoicingLineBase)(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.Lines)).SyncRoot)).AL_ExchangeRate)));
			this.TransactionLinesGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|5c93fcf1-cb5c-4367-9882-538fe55b844a", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GenericCharge";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AL_JH";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "AL_Desc";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AL_GB";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AL_GE";
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|c3ae0ffd-0d36-4d0a-a5ae-e75f1c133271", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "AL_OSExTaxAmount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "AL_AT";
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "Decimals";
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|7892c3c5-1f82-495e-a7cf-06f552f1f868", "Tax Amt");
			zCalcEditColumnStyleInfo2.ColumnName = "AL_OSTaxAmount";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|8fbf2ce2-b172-4f7b-8255-a75475fcf7d5", "Total");
			zCalcEditColumnStyleInfo3.ColumnName = "GSTInclusiveAmount";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "AL_IsFinalCharge";
			zCheckBoxColumnStyleInfo1.ToolTip = "Final";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = "AL_ExchangeRate_Decimals";
			zCalcEditColumnStyleInfo4.ColumnName = "AL_ExchangeRate";
			zCalcEditColumnStyleInfo4.Decimals = 4;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.TransactionLinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TransactionLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.TransactionLinesGrid.CopySelectedRowsAllowed = true;
			this.TransactionLinesGrid.GridId = "02af035f-64f3-4890-a0e6-fe3c58133fd4";
			this.TransactionLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionLinesGrid.LayoutKey = "TransactionLinesGrid";
			this.TransactionLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.TransactionLinesGrid.Name = "TransactionLinesGrid";
			this.TransactionLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(926, 173, true);
			this.TransactionLinesGrid.TabIndex = 0;
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|33659f1a-3249-4564-b4d4-5a50415a2425", "Wharf Storage Charges and Invoice Details");
			this.InvoiceDetailsGroupBox.Controls.Add(this.ChargeCodeGuidFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.APInvoiceNumberTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.RetrieveStorageFeesButton);
			this.InvoiceDetailsGroupBox.Controls.Add(this.StorageChargesCalcFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.AH_DescTextbox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.ApportionmentMethodDropEdit);
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 129, true);
			this.InvoiceDetailsGroupBox.Name = "InvoiceDetailsGroupBox";
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 107, true);
			this.InvoiceDetailsGroupBox.TabIndex = 1;
			this.InvoiceDetailsGroupBox.TabStop = false;
			// 
			// ChargeCodeGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.ChargeCodeGuidFindBox, "ChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((StorageFeeInvoicePayment)(null)).ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Lookups.ChargeCodes)));
			this.ChargeCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 47, true);
			this.ChargeCodeGuidFindBox.Name = "ChargeCodeGuidFindBox";
			this.ChargeCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.ChargeCodeGuidFindBox.TabIndex = 2;
			// 
			// APInvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.APInvoiceNumberTextBox, "Invoice.AH_TransactionNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((StorageFeeInvoicePayment)(null)).Invoice.AH_TransactionNum)));
			this.APInvoiceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.APInvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 21, true);
			this.APInvoiceNumberTextBox.Name = "APInvoiceNumberTextBox";
			this.APInvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.APInvoiceNumberTextBox.TabIndex = 1;
			// 
			// RetrieveStorageFeesButton
			// 
			this.RetrieveStorageFeesButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|06dfcbaa-f69b-431b-923d-f5a3a8237220", "Retrieve Storage Charges");
			this.RetrieveStorageFeesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 19, true);
			this.RetrieveStorageFeesButton.Name = "RetrieveStorageFeesButton";
			this.RetrieveStorageFeesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.RetrieveStorageFeesButton.TabIndex = 0;
			this.RetrieveStorageFeesButton.UseVisualStyleBackColor = true;
			this.RetrieveStorageFeesButton.Click += new EventHandler(this.RetrieveStorageFeesButton_Click);
			// 
			// StorageChargesCalcFindBox
			// 
			this.StorageChargesCalcFindBox.BindToAmount = "StorageCharges";
			this.StorageChargesCalcFindBox.BindToDecimalPlaces = "StorageChargesDecimals";
			this.StorageChargesCalcFindBox.BindToUnit = "LocalCurrency";
			this.StorageChargesCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.StorageChargesCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 47, true);
			this.StorageChargesCalcFindBox.Name = "StorageChargesCalcFindBox";
			this.StorageChargesCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 20, true);
			this.StorageChargesCalcFindBox.TabIndex = 3;
			// 
			// AH_DescTextbox
			// 
			this.AH_DescTextbox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.AH_DescTextbox, "Invoice.AH_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((StorageFeeInvoicePayment)(null)).Invoice.AH_Desc)));
			this.AH_DescTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AH_DescTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 73, true);
			this.AH_DescTextbox.Name = "AH_DescTextbox";
			this.AH_DescTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.AH_DescTextbox.TabIndex = 4;
			// 
			// ApportionmentMethodDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ApportionmentMethodDropEdit, "ApportionmentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((StorageFeeInvoicePayment)(null)).ApportionmentMethod)));
			this.ApportionmentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 73, true);
			this.ApportionmentMethodDropEdit.Name = "ApportionmentMethodDropEdit";
			this.ApportionmentMethodDropEdit.ShowDescriptionBox = false;
			this.ApportionmentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ApportionmentMethodDropEdit.TabIndex = 5;
			// 
			// OrganisationDetailsGroupBox
			// 
			this.OrganisationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.OrganisationDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|e64b8bf1-49ee-4822-8b4e-c59305aef944", "Terminal / Wharf Storage Organization");
			this.OrganisationDetailsGroupBox.Controls.Add(this.ContainerNumberTextBox);
			this.OrganisationDetailsGroupBox.Controls.Add(this.zLabel1);
			this.OrganisationDetailsGroupBox.Controls.Add(this.PickupDateDateEdit);
			this.OrganisationDetailsGroupBox.Controls.Add(this.SelectOrgButton);
			this.OrganisationDetailsGroupBox.Controls.Add(this.CreditorGuidFindBox);
			this.OrganisationDetailsGroupBox.Controls.Add(this.PortCodeTextBox);
			this.OrganisationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.OrganisationDetailsGroupBox.Name = "OrganisationDetailsGroupBox";
			this.OrganisationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 104, true);
			this.OrganisationDetailsGroupBox.TabIndex = 0;
			this.OrganisationDetailsGroupBox.TabStop = false;
			// 
			// ContainerNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContainerNumberTextBox, "ContainerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((StorageFeeInvoicePayment)(null)).ContainerNumber)));
			this.ContainerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 49, true);
			this.ContainerNumberTextBox.Name = "ContainerNumberTextBox";
			this.ContainerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.ContainerNumberTextBox.TabIndex = 4;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|22496e4c-d3bc-4279-a6c8-873b5b5d0bd0", "Terminal");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 21, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// PickupDateDateEdit
			// 
			this.PickupDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PickupDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PickupDateDateEdit, "PickupDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((StorageFeeInvoicePayment)(null)).PickupDate)));
			this.PickupDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PickupDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 75, true);
			this.PickupDateDateEdit.Name = "PickupDateDateEdit";
			this.PickupDateDateEdit.TabIndex = 5;
			// 
			// SelectOrgButton
			// 
			this.SelectOrgButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|1fdbfe57-ce0c-4b90-8c41-7e4a657360e3", "Select Organization");
			this.SelectOrgButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 20, true);
			this.SelectOrgButton.Name = "SelectOrgButton";
			this.SelectOrgButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 23, true);
			this.SelectOrgButton.TabIndex = 1;
			this.SelectOrgButton.UseVisualStyleBackColor = true;
			this.SelectOrgButton.Click += new EventHandler(this.SelectOrgButton_Click);
			// 
			// CreditorGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.CreditorGuidFindBox, "Invoice.AH_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((StorageFeeInvoicePayment)(null)).Invoice.AH_OH)));
			this.CreditorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 23, true);
			this.CreditorGuidFindBox.Name = "CreditorGuidFindBox";
			this.CreditorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.CreditorGuidFindBox.TabIndex = 3;
			// 
			// PortCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PortCodeTextBox, "PortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((StorageFeeInvoicePayment)(null)).PortCode)));
			this.PortCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 22, true);
			this.PortCodeTextBox.Name = "PortCodeTextBox";
			this.PortCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PortCodeTextBox.TabIndex = 2;
			// 
			// PaymentDetailsGroupBox
			// 
			this.PaymentDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PaymentDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|e2619caa-6f73-425c-8f87-92b02f1de889", "ComPay Payment Details");
			this.PaymentDetailsGroupBox.Controls.Add(this.zDropEdit1);
			this.PaymentDetailsGroupBox.Controls.Add(this.BankAccountGuidFindBox);
			this.PaymentDetailsGroupBox.Controls.Add(this.ChequeOrReferenceTextBox);
			this.PaymentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 242, true);
			this.PaymentDetailsGroupBox.Name = "PaymentDetailsGroupBox";
			this.PaymentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 79, true);
			this.PaymentDetailsGroupBox.TabIndex = 2;
			this.PaymentDetailsGroupBox.TabStop = false;
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "Invoice.ReceiptPaymentAH_ReceiptType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((StorageFeeInvoicePayment)(null)).Invoice.ReceiptPaymentAH_ReceiptType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.PaymentMethods)));
			this.zDropEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|8b9da42b-f64c-43a5-8f67-1ba8cd71e673", "Type");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 19, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.zDropEdit1.TabIndex = 0;
			// 
			// BankAccountGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BankAccountGuidFindBox, "Invoice.ReceiptPaymentAH_AB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((StorageFeeInvoicePayment)(null)).Invoice.ReceiptPaymentAH_AB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((StorageFeeInvoicePayment)(null)).Invoice.BankAccountLookup)));
			this.BankAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|652a5724-6eb1-4e2b-9ed3-9106b01a93a4", "Bank");
			this.BankAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 45, true);
			this.BankAccountGuidFindBox.Name = "BankAccountGuidFindBox";
			this.BankAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.BankAccountGuidFindBox.TabIndex = 2;
			// 
			// ChequeOrReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChequeOrReferenceTextBox, "Invoice.ReceiptPaymentAH_ChequeOrReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((StorageFeeInvoicePayment)(null)).Invoice.ReceiptPaymentAH_ChequeOrReference)));
			this.ChequeOrReferenceTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainerStorageInvoiceUserControl|550533d6-c96c-4dc9-a98a-ab94d501a400", "Payment Reference");
			this.ChequeOrReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(674, 19, true);
			this.ChequeOrReferenceTextBox.Name = "ChequeOrReferenceTextBox";
			this.ChequeOrReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ChequeOrReferenceTextBox.TabIndex = 1;
			// 
			// ContainerStorageInvoiceUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HeaderGroupBox);
			this.Name = "ContainerStorageInvoiceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(955, 534, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderGroupBox.ResumeLayout(false);
			this.HeaderGroupBox.PerformLayout();
			this.InvoiceLinesGroupBox.ResumeLayout(false);
			this.InvoiceLinesGroupBox.PerformLayout();
			((ISupportInitialize)(this.TransactionLinesGrid)).EndInit();
			this.TransactionLinesGrid.ResumeLayout(false);
			this.TransactionLinesGrid.PerformLayout();
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.PerformLayout();
			this.ChargeCodeGuidFindBox.ResumeLayout(true);
			this.ChargeCodeGuidFindBox.PerformLayout();
			this.StorageChargesCalcFindBox.ResumeLayout(true);
			this.StorageChargesCalcFindBox.PerformLayout();
			this.ApportionmentMethodDropEdit.ResumeLayout(true);
			this.ApportionmentMethodDropEdit.PerformLayout();
			this.OrganisationDetailsGroupBox.ResumeLayout(false);
			this.OrganisationDetailsGroupBox.PerformLayout();
			this.PickupDateDateEdit.ResumeLayout(true);
			this.PickupDateDateEdit.PerformLayout();
			this.CreditorGuidFindBox.ResumeLayout(true);
			this.CreditorGuidFindBox.PerformLayout();
			this.PaymentDetailsGroupBox.ResumeLayout(false);
			this.PaymentDetailsGroupBox.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.BankAccountGuidFindBox.ResumeLayout(true);
			this.BankAccountGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}