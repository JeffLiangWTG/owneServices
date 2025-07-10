
namespace Enterprise.Client.WCB.DaimlerChrysler.GUI
{
	partial class DCImportDataEditorForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DeclarationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.InvoiceHeaderGrid = new Enterprise.ZArchitecture.ZGrid();
			this.InvoiceLineGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DeclarationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceHeadersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationGrid)).BeginInit();
			this.DeclarationGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceHeaderGrid)).BeginInit();
			this.InvoiceHeaderGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLineGrid)).BeginInit();
			this.InvoiceLineGrid.SuspendLayout();
			this.DeclarationsGroupBox.SuspendLayout();
			this.InvoiceHeadersGroupBox.SuspendLayout();
			this.InvoiceLinesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 520, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 0, true);
			this.MainStatusBar.TabIndex = 6;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(316);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(317);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads);
			// 
			// DeclarationGrid
			// 
			this.DeclarationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeclarationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).JE_MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).JE_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).Lookups.SuppliersList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).JE_OH_Importer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).Lookups.ImportersList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).JE_VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).JE_VoyageFlightNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).JE_RL_NKFinalDestination)));
			this.DeclarationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Master Bill";
			zTextBoxColumnStyleInfo1.ColumnName = "JE_MasterBill";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.SuppliersList";
			zGuidFindBoxColumnStyleInfo1.Caption = "Supplier";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JE_OH_Supplier";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.BindToList = "Lookups.ImportersList";
			zGuidFindBoxColumnStyleInfo2.Caption = "Importer";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JE_OH_Importer";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Vessel";
			zTextBoxColumnStyleInfo2.ColumnName = "JE_VesselName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.Caption = "Voyage/Flight";
			zTextBoxColumnStyleInfo3.ColumnName = "JE_VoyageFlightNo";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.Caption = "Destination";
			zTextBoxColumnStyleInfo4.ColumnName = "JE_RL_NKFinalDestination";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeclarationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.DeclarationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.DeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DeclarationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DeclarationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DeclarationGrid.GridId = "54054a18-af43-4ee4-90e9-c7f477ccf6b2";
			this.DeclarationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeclarationGrid.LayoutKey = "DeclarationGrid";
			this.DeclarationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DeclarationGrid.Name = "DeclarationGrid";
			this.DeclarationGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DeclarationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 122, true);
			this.DeclarationGrid.TabIndex = 0;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.IsCaptionOverridden = true;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(560, 488, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 5;
			this.CancelButton.Text = "&Cancel";
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 15, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = "Please enter the missing values:";
			// 
			// InvoiceHeaderGrid
			// 
			this.InvoiceHeaderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceHeaderGrid, "FixedInvoices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).FixedInvoices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.WCB.InvHeadWithFixedInvLines)(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).FixedInvoices)).SyncRoot)).JZ_InvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.WCB.InvHeadWithFixedInvLines)(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).FixedInvoices)).SyncRoot)).JZ_Calc_FOBAmount)));
			this.InvoiceHeaderGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.Caption = "Invoice No";
			zTextBoxColumnStyleInfo5.ColumnName = "JZ_InvoiceNumber";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = " FOB Amount";
			zCalcEditColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("DCImportDataEditorForm|0ff20c00-f37b-4fc6-b2ce-b8d7f3eb66e2", "FOB", "FOB Amount", "Free On Board Value");
			zCalcEditColumnStyleInfo1.ColumnName = "JZ_Calc_FOBAmount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.InvoiceHeaderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InvoiceHeaderGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoiceHeaderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceHeaderGrid.GridId = "2bbb5794-2e3b-470a-bc4e-3a6f94f2eb6e";
			this.InvoiceHeaderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceHeaderGrid.LayoutKey = "InvoiceHeaderGrid";
			this.InvoiceHeaderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvoiceHeaderGrid.Name = "InvoiceHeaderGrid";
			this.InvoiceHeaderGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoiceHeaderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 125, true);
			this.InvoiceHeaderGrid.TabIndex = 0;
			// 
			// InvoiceLineGrid
			// 
			this.InvoiceLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InvoiceLineGrid, "Invoices.JobComInvoiceLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).Invoices)).SyncRoot)).JobComInvoiceLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).Invoices)).SyncRoot)).JobComInvoiceLines)).SyncRoot)).JI_PartNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).Invoices)).SyncRoot)).JobComInvoiceLines)).SyncRoot)).JI_PartAttrib1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).Invoices)).SyncRoot)).JobComInvoiceLines)).SyncRoot)).JI_IsPackToBondForLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads)(null)).Invoices)).SyncRoot)).JobComInvoiceLines)).SyncRoot)).JI_Calc_FOB)));
			this.InvoiceLineGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo6.Caption = "Part No";
			zTextBoxColumnStyleInfo6.ColumnName = "JI_PartNo";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.Caption = "Commission No";
			zTextBoxColumnStyleInfo7.ColumnName = "JI_PartAttrib1";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.Caption = "Is Pack To Bond";
			zCheckBoxColumnStyleInfo1.ColumnName = "JI_IsPackToBondForLine";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "FOB Amount";
			zCalcEditColumnStyleInfo2.ColumnName = "JI_Calc_FOB";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.InvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InvoiceLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.InvoiceLineGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoiceLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.InvoiceLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineGrid.GridId = "b83ad784-138f-4658-a790-3648d83cc399";
			this.InvoiceLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceLineGrid.LayoutKey = "InvoiceLineGrid";
			this.InvoiceLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvoiceLineGrid.Name = "InvoiceLineGrid";
			this.InvoiceLineGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 146, true);
			this.InvoiceLineGrid.TabIndex = 0;
			// 
			// DeclarationsGroupBox
			// 
			this.DeclarationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationsGroupBox.Controls.Add(this.DeclarationGrid);
			this.DeclarationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 25, true);
			this.DeclarationsGroupBox.Name = "DeclarationsGroupBox";
			this.DeclarationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 141, true);
			this.DeclarationsGroupBox.TabIndex = 1;
			this.DeclarationsGroupBox.TabStop = false;
			this.DeclarationsGroupBox.Text = "Declarations";
			// 
			// InvoiceHeadersGroupBox
			// 
			this.InvoiceHeadersGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceHeadersGroupBox.Controls.Add(this.InvoiceHeaderGrid);
			this.InvoiceHeadersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 172, true);
			this.InvoiceHeadersGroupBox.Name = "InvoiceHeadersGroupBox";
			this.InvoiceHeadersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 144, true);
			this.InvoiceHeadersGroupBox.TabIndex = 2;
			this.InvoiceHeadersGroupBox.TabStop = false;
			this.InvoiceHeadersGroupBox.Text = "Invoice Headers";
			// 
			// InvoiceLinesGroupBox
			// 
			this.InvoiceLinesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceLinesGroupBox.Controls.Add(this.InvoiceLineGrid);
			this.InvoiceLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 320, true);
			this.InvoiceLinesGroupBox.Name = "InvoiceLinesGroupBox";
			this.InvoiceLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 165, true);
			this.InvoiceLinesGroupBox.TabIndex = 3;
			this.InvoiceLinesGroupBox.TabStop = false;
			this.InvoiceLinesGroupBox.Text = "Invoice Lines";
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.IsCaptionOverridden = true;
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 488, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ImportButton.TabIndex = 4;
			this.ImportButton.Text = "&Import";
			this.ImportButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ImportButton.ToolTipCaption = null;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// DCImportDataEditorForm
			// 
			this.AcceptButton = this.ImportButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 520, true);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.InvoiceLinesGroupBox);
			this.Controls.Add(this.InvoiceHeadersGroupBox);
			this.Controls.Add(this.DeclarationsGroupBox);
			this.Controls.Add(this.zLabel1);
			this.DataSourceType = typeof(Enterprise.Client.WCB.JobDeclarationWithFixedInvHeads);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 513, true);
			this.Name = "DCImportDataEditorForm";
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DeclarationsGroupBox, 0);
			this.Controls.SetChildIndex(this.InvoiceHeadersGroupBox, 0);
			this.Controls.SetChildIndex(this.InvoiceLinesGroupBox, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationGrid)).EndInit();
			this.DeclarationGrid.ResumeLayout(false);
			this.DeclarationGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceHeaderGrid)).EndInit();
			this.InvoiceHeaderGrid.ResumeLayout(false);
			this.InvoiceHeaderGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceLineGrid)).EndInit();
			this.InvoiceLineGrid.ResumeLayout(false);
			this.InvoiceLineGrid.PerformLayout();
			this.DeclarationsGroupBox.ResumeLayout(false);
			this.DeclarationsGroupBox.PerformLayout();
			this.InvoiceHeadersGroupBox.ResumeLayout(false);
			this.InvoiceHeadersGroupBox.PerformLayout();
			this.InvoiceLinesGroupBox.ResumeLayout(false);
			this.InvoiceLinesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid DeclarationGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZGrid InvoiceHeaderGrid;
		private Enterprise.ZArchitecture.ZGrid InvoiceLineGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceHeadersGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceLinesGroupBox;
		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.GUI.ZButton ImportButton;
	}
}
