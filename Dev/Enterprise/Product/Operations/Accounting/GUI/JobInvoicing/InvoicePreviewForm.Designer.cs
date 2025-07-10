using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class InvoicePreviewForm
	{


		#region Windows Form Designer generated code

		ZGrid InvoicesGrid;
		ZButton CloseButton;
		ZButton PreviewAndDeliverButton;
		ZButton PreviewOnlyButton;
		System.ComponentModel.Container components = null;

		new void InitializeComponent()
		{
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			this.InvoicesGrid = new ZGrid();
			this.CloseButton = new ZButton();
			this.PreviewAndDeliverButton = new ZButton();
			this.PreviewOnlyButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 248, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(InvoicesPreviewer);
			// 
			// InvoicesGrid
			// 
			this.InvoicesGrid.AllowNavigation = false;
			this.InvoicesGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoicesGrid, "PreviewInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_TransactionCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).TransactionCategoryDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_RX_NKTransactionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_OSTotal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_TransactionNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_ChequeOrReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_InvoiceDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((InvoicingBase)(((System.Collections.IList)(((InvoicesPreviewer)(null)).PreviewInvoices)).SyncRoot)).AH_DueDate)));
			this.InvoicesGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "AH_OH";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "AH_Desc";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "AH_TransactionCategory";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePreviewForm|33a65c2e-1cd0-4e76-a14a-c8cd84530734", "Invoice Type");
			zTextBoxColumnStyleInfo3.ColumnName = "TransactionCategoryDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePreviewForm|5b9c3d9b-0b96-4649-8493-8730f29036e1", "Ledger");
			zTextBoxColumnStyleInfo6.ColumnName = "AH_Ledger";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "AH_RX_NKTransactionCurrency";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AH_OSTotal";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "AH_TransactionNum";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c720417e-0b02-422b-9f4e-246ccdde7d31", "Sell Reference");
			zTextBoxColumnStyleInfo5.ColumnName = "AH_ChequeOrReference";
			zDateEditColumnStyleInfo1.ColumnName = "AH_InvoiceDate";
			zDateEditColumnStyleInfo2.ColumnName = "AH_DueDate";
			this.InvoicesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InvoicesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InvoicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InvoicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.InvoicesGrid.CopySelectedRowsAllowed = true;
			this.InvoicesGrid.GridId = "b24ce08c-1059-45f0-bf68-a608ca56fc7d";
			this.InvoicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoicesGrid.IsWholeRowSelectedOnClick = true;
			this.InvoicesGrid.LayoutKey = "zGrid1";
			this.InvoicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.InvoicesGrid.Name = "InvoicesGrid";
			this.InvoicesGrid.ReadOnly = true;
			this.InvoicesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 204, true);
			this.InvoicesGrid.TabIndex = 4;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePreviewForm|6d165247-dce4-4a93-8104-1cd7120f3564", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 219, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// PreviewAndDeliverButton
			// 
			this.PreviewAndDeliverButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviewAndDeliverButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePreviewForm|83dfe04e-edfb-42af-876f-b43d0d7543ec", "Preview And Deliver");
			this.PreviewAndDeliverButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 219, true);
			this.PreviewAndDeliverButton.Name = "PreviewAndDeliverButton";
			this.PreviewAndDeliverButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.PreviewAndDeliverButton.TabIndex = 5;
			this.PreviewAndDeliverButton.UseVisualStyleBackColor = true;
			this.PreviewAndDeliverButton.Click += new EventHandler(this.PreviewAndDeliverButton_Click);
			// 
			// PreviewOnlyButton
			// 
			this.PreviewOnlyButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviewOnlyButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePreviewForm|295d9aca-5355-42df-9916-70d2dbfa6c52", "Preview Only");
			this.PreviewOnlyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 219, true);
			this.PreviewOnlyButton.Name = "PreviewOnlyButton";
			this.PreviewOnlyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 23, true);
			this.PreviewOnlyButton.TabIndex = 6;
			this.PreviewOnlyButton.UseVisualStyleBackColor = true;
			this.PreviewOnlyButton.Click += new EventHandler(this.PreviewOnlyButton_Click);
			// 
			// InvoicePreviewForm
			// 
			this.AcceptButton = this.CloseButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("InvoicePreviewForm|c363fd65-c502-4962-a919-281eeabca1e7", "Preview All Transactions");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 274, true);
			this.Controls.Add(this.PreviewOnlyButton);
			this.Controls.Add(this.InvoicesGrid);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.PreviewAndDeliverButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(InvoicesPreviewer);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.JobInvoicing.Posting.InvoicesPreviewer";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "InvoicePreviewForm";
			this.Controls.SetChildIndex(this.PreviewAndDeliverButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.InvoicesGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PreviewOnlyButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoicesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}