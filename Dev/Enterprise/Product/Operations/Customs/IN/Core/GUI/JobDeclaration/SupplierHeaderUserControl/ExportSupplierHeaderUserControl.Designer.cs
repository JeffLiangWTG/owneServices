using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IN.GUI;

partial class ExportSupplierHeaderUserControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyle1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.InvoiceHeaderSWControlsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoiceHeaderSWControlsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.InvoicePartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BuyerDocAddress = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DetailsGroupBox.SuspendLayout();
			this.InvoiceDetailUserControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.InvoiceTabControl.SuspendLayout();
			this.ComInvoiceDetailsTabPage.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ChargesTabControl.SuspendLayout();
			this.InvoiceChargesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.InvoiceChargesGrid.SuspendLayout();
			this.ApportionedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).BeginInit();
			this.ApportionedChargesGrid.SuspendLayout();
			this.BaseGroupChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).BeginInit();
			this.BaseGroupChargesGrid.SuspendLayout();
			this.JZ_FOBAmountBoundCurrencyControl.SuspendLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.SuspendLayout();
			this.JZ_CIFAmountBoundCurrencyControl.SuspendLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).BeginInit();
			this.JobComInvoiceHeadersBoundGrid.SuspendLayout();
			this.InvCustomFieldsDisplayControl.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.InvoiceHeaderSWControlsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).BeginInit();
			this.Splitter.Panel1.SuspendLayout();
			this.Splitter.Panel2.SuspendLayout();
			this.Splitter.SuspendLayout();
			this.InvDetailLeftPanel.SuspendLayout();
			this.InvDetailRightPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).BeginInit();
			this.ChargesGroupsSplitterContainer.Panel1.SuspendLayout();
			this.ChargesGroupsSplitterContainer.Panel2.SuspendLayout();
			this.ChargesGroupsSplitterContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoicePartiesTabPage.SuspendLayout();
			this.BuyerDocAddress.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 243, true);
			// 
			// InvoiceDetailUserControl
			// 
			this.InvoiceDetailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 224, true);
			// 
			// InvoiceTabControl
			// 
			this.InvoiceTabControl.Controls.Add(this.InvoicePartiesTabPage);
			this.InvoiceTabControl.Controls.Add(this.InvoiceHeaderSWControlsTabPage);
			this.InvoiceTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.InvoicePartiesTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.InvoiceHeaderSWControlsTabPage, 0);
			this.InvoiceTabControl.Controls.SetChildIndex(this.ComInvoiceDetailsTabPage, 0);
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 127, true);
			// 
			// ChargesTabControl
			// 
			this.ChargesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 108, true);
			// 
			// InvoiceChargesTabPage
			// 
			this.InvoiceChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 81, true);
			// 
			// InvoiceChargesGrid
			// 
			this.InvoiceChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 81, true);
			// 
			// BaseGroupChargesGroupBox
			// 
			this.BaseGroupChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 110, true);
			// 
			// BaseGroupChargesGrid
			// 
			this.BaseGroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 91, true);
			// 
			// JobComInvoiceHeadersBoundGrid
			//
			zTextBoxColumnStyleInfo1.ColumnName = "JZ_ExporterContractNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JZ_PaymentDays";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.MaxValue = 999;
			zCalcEditColumnStyleInfo1.ShowEmptyStringForEmptyValue = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "JZ_PaymentMethod";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "BuyingPartyOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.IN.GUI.Res.GetData("53622263-63f7-4849-b1f8-b8ce06a9199e", "Buyer");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyle1.ColumnName = "BuyingPartyAddressPK";
			zGuidDropEditColumnStyle1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyle1.GroupName = Enterprise.Customs.IN.GUI.Res.GetData("53622263-63f7-4849-b1f8-b8ce06a9199e", "Buyer");
			zGuidDropEditColumnStyle1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(zGuidDropEditColumnStyle1);
			//
			// 
			// 
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.AllowNavigation = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CaptionVisible = false;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutUU5vFvxYnT4ITmuJkyIQgg==";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.LayoutKey = "JobComInvoiceHeadersBoundGrid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Name = "Grid";
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(988, 109, true);
			this.JobComInvoiceHeadersBoundGrid.InnerGrid.TabIndex = 0;
			// 
			// Splitter
			// 
			// 
			// InvDetailLeftPanel
			// 
			this.InvDetailLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 243, true);
			// 
			// InvDetailRightPanel
			// 
			this.InvDetailRightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 0, true);
			this.InvDetailRightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 243, true);
			// 
			// ChargesGroupsSplitterContainer
			// 
			this.ChargesGroupsSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 243, true);
			this.ChargesGroupsSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(127);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.JobDeclaration);
			//
			// InvoiceHeaderSWControlsTabPage
			// 
			this.InvoiceHeaderSWControlsTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("c35aebc0-3ec8-4fd1-9865-3bbd855b03e8", "SW Controls");
			this.InvoiceHeaderSWControlsTabPage.Controls.Add(this.InvoiceHeaderSWControlsUserControl);
			this.InvoiceHeaderSWControlsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoiceHeaderSWControlsTabPage.Name = "InvoiceHeaderSWControlsTabPage";
			this.InvoiceHeaderSWControlsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.InvoiceHeaderSWControlsTabPage.TabIndex = 1;
			this.InvoiceHeaderSWControlsTabPage.UseVisualStyleBackColor = true;
			// 
			// SWControlsUserControl
			// 
			this.InvoiceHeaderSWControlsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceHeaderSWControlsUserControl, "Invoices.SWControls");
			this.InvoiceHeaderSWControlsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceHeaderSWControlsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceHeaderSWControlsUserControl.Name = "InvoiceHeaderSWControlsUserControl";
			this.InvoiceHeaderSWControlsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.InvoiceHeaderSWControlsUserControl.TabIndex = 0;
			// 
			// InvoicePartiesTabPage
			// 
			this.InvoicePartiesTabPage.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("cdbc6f89-12b4-49f0-84c2-7cc1e6affaf0", "Invoice Parties");
			this.InvoicePartiesTabPage.Controls.Add(this.BuyerDocAddress);
			this.InvoicePartiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoicePartiesTabPage.Name = "InvoicePartiesTabPage";
			this.InvoicePartiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InvoicePartiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 243, true);
			this.InvoicePartiesTabPage.TabIndex = 100;
			this.InvoicePartiesTabPage.UseVisualStyleBackColor = true;
			// 
			// BuyerDocAddress
			// 
			this.BuyerDocAddress.AddressValidationProcessCmdKey = null;
			this.BuyerDocAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerDocAddress, "Invoices.BuyerDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.IN.Business.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.IN.Business.JobDeclaration)(null)).Invoices)).SyncRoot)).BuyerDocAddress)));
			this.BuyerDocAddress.BindToOrganisations = "Invoices.Lookups.Buyers";
			this.BuyerDocAddress.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("fa4d8eba-8b36-470f-a1b3-34c0c7247e2c", "Buyer");
			this.BuyerDocAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.BuyerDocAddress.Name = "BuyerDocAddress";
			this.BuyerDocAddress.ReadOnly = false;
			this.BuyerDocAddress.SingleLineNoGroupBoxPanelWidth = 296;
			this.BuyerDocAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.BuyerDocAddress.TabIndex = 0;
			this.BuyerDocAddress.ValidationJustForced = false;
			// 
			// ExportSupplierHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ExportSupplierHeaderUserControl";
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.InvoiceDetailUserControl.ResumeLayout(true);
			this.InvoiceDetailUserControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.InvoiceTabControl.ResumeLayout(false);
			this.InvoiceTabControl.PerformLayout();
			this.ComInvoiceDetailsTabPage.ResumeLayout(false);
			this.ComInvoiceDetailsTabPage.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ChargesTabControl.ResumeLayout(false);
			this.ChargesTabControl.PerformLayout();
			this.InvoiceChargesTabPage.ResumeLayout(false);
			this.InvoiceChargesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.InvoiceChargesGrid.ResumeLayout(false);
			this.InvoiceChargesGrid.PerformLayout();
			this.ApportionedTabPage.ResumeLayout(false);
			this.ApportionedTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ApportionedChargesGrid)).EndInit();
			this.ApportionedChargesGrid.ResumeLayout(false);
			this.ApportionedChargesGrid.PerformLayout();
			this.BaseGroupChargesGroupBox.ResumeLayout(false);
			this.BaseGroupChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BaseGroupChargesGrid)).EndInit();
			this.BaseGroupChargesGrid.ResumeLayout(false);
			this.BaseGroupChargesGrid.PerformLayout();
			this.JZ_FOBAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_FOBAmountBoundCurrencyControl.PerformLayout();
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.ResumeLayout(true);
			this.JZ_Calc_TNIBoundInvoiceCurrencyControl.PerformLayout();
			this.JZ_CIFAmountBoundCurrencyControl.ResumeLayout(true);
			this.JZ_CIFAmountBoundCurrencyControl.PerformLayout();
			this.LineTotalBoundConvertToLocalCurrencyControl.ResumeLayout(true);
			this.LineTotalBoundConvertToLocalCurrencyControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobComInvoiceHeadersBoundGrid.InnerGrid)).EndInit();
			this.JobComInvoiceHeadersBoundGrid.ResumeLayout(true);
			this.JobComInvoiceHeadersBoundGrid.PerformLayout();
			this.InvCustomFieldsDisplayControl.ResumeLayout(true);
			this.InvCustomFieldsDisplayControl.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.InvoiceHeaderSWControlsTabPage.ResumeLayout(false);
			this.InvoiceHeaderSWControlsTabPage.PerformLayout();
			this.Splitter.Panel1.ResumeLayout(false);
			this.Splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.Splitter)).EndInit();
			this.Splitter.ResumeLayout(false);
			this.Splitter.PerformLayout();
			this.InvDetailLeftPanel.ResumeLayout(false);
			this.InvDetailLeftPanel.PerformLayout();
			this.InvDetailRightPanel.ResumeLayout(false);
			this.InvDetailRightPanel.PerformLayout();
			this.ChargesGroupsSplitterContainer.Panel1.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ChargesGroupsSplitterContainer)).EndInit();
			this.ChargesGroupsSplitterContainer.ResumeLayout(false);
			this.ChargesGroupsSplitterContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoicePartiesTabPage.ResumeLayout(false);
			this.InvoicePartiesTabPage.PerformLayout();
			this.BuyerDocAddress.ResumeLayout(true);
			this.BuyerDocAddress.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZTabPage InvoicePartiesTabPage;
	internal Enterprise.MasterFiles.GUI.ZDocAddressControl BuyerDocAddress;
	internal Enterprise.ZArchitecture.GUI.ZTabPage InvoiceHeaderSWControlsTabPage;
	internal ZArchitecture.GUI.ZDynamicControlCreationUserControl InvoiceHeaderSWControlsUserControl;
}
