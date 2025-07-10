
namespace Enterprise.Customs.CA.GUI
{
	partial class CAExportInvoiceLineUserControl
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
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.Customs.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CA_ConveyanceIdentificationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_StateOrRegionOfOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PermitsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PermitsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TariffFindBox = new Enterprise.Customs.GUI.TariffFindBox();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.LineDetailsTabPage.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PermitsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 257, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 577, true);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_StateOrRegionOfOriginDropEdit);
			this.InvoiceDetailsGroupBox.TabIndex = 1;
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_StateOrRegionOfOriginDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.CaptionResourceString =Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|1eabd67f-c2e5-4efa-908d-87c65c34b567", "Ctry/Rgn. Of Origin", "Country/Region in which the goods have been produced or manufactured, according to criteria laid down for the purposes of application of the Customs tariff, of quantitative restrictions, or of any other measure related to trade.");
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 40, true);
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 16, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 15;
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Controls.Add(this.TariffFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.PermitsGroupBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.CA_ConveyanceIdentificationNumberTextBox);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CA_ConveyanceIdentificationNumberTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.PermitsGroupBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.TariffFindBox, 0);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 577, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			zDropEditColumnStyleInfo1.ColumnName = "JI_StateOrRegionOfOrigin";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.CaptionResourceString =Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|4A17FEB1-F40A-471A-872F-5780875D8A8C", "Province", "Province of Origin/Shipment", "Province of Origin/Shipment","The Region in which the goods have been produced or manufactured. If the goods were originally imported into Canada and are being exported in the same condition, provide the province the goods were shipped from.");
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|cabb0756-1e01-4904-ac64-3d7eed9cdb1d", "Conveyance ID");
			zTextBoxColumnStyleInfo2.ColumnName = "CA_ConveyanceIdentificationNumber";
			tariffColumnStyleInfo1.BindToTariffPropertyInfo = "JI_FormattedTariffCodeTariffInfo";
			tariffColumnStyleInfo1.CaptionResourceString =Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|1a86eeff-3e24-4d98-ac00-e893f0a6dd07", "HS Code", "Must be a 10-digit Canadian National Customs Tariff code.");
			tariffColumnStyleInfo1.ColumnName = "JI_FormattedTariff";
			tariffColumnStyleInfo1.TariffCode = null;
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 577, true);
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.CaptionResourceString =Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|3a0b2ab7-e43b-4bef-acf5-e0e68c9da67e", "Customs Qty", "Measurement of the goods as required by Customs to be expressed for tariff, statistical or fiscal purposes. Must be specified if required by the Tariff (HS) code. The units must ba a valid Release Unit of Measure code.");
			this.CustomsQuantityCalcDropEdit.TabIndex = 1;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 65, true);
			this.VolumeCalcDropEdit.TabIndex = 13;
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 64, true);
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 21, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// CA_ConveyanceIdentificationNumberTextBox
			// 
			this.CA_ConveyanceIdentificationNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CA_ConveyanceIdentificationNumberTextBox, "FilteredInvoiceLines.CA_ConveyanceIdentificationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CA_ConveyanceIdentificationNumber)));
			this.CA_ConveyanceIdentificationNumberTextBox.CaptionResourceString =Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|eea4636b-f836-45be-b377-aa7fa5d8090e", "Conveyance IDs", "Conveyance IDs (VINs)", "Vehicle Identification Number(s) (VINs) are required for certain HS codes. Vehicle Identification Number is further expanded to include Hull Identification Numbers (HIN), or applicable Serial Numbers. Vehicles for export include all conveyances being exported, such as vehicles, motorcycles, all-terrain vehicles, boats, etc.");
			this.CA_ConveyanceIdentificationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			this.CA_ConveyanceIdentificationNumberTextBox.Name = "CA_ConveyanceIdentificationNumberTextBox";
			this.CA_ConveyanceIdentificationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.CA_ConveyanceIdentificationNumberTextBox.TabIndex = 2;
			// 
			// JI_StateOrRegionOfOriginDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JI_StateOrRegionOfOriginDropEdit, "FilteredInvoiceLines.JI_StateOrRegionOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_StateOrRegionOfOrigin)));
			this.JI_StateOrRegionOfOriginDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|39fa48f2-5574-4936-80c6-b5453bb06032", "P/Origin", "Province Of Origin/Shipment", "Province of Origin/Shipment", "The Region in which the goods have been produced or manufactured.  If the goods were originally imported into Canada and are being exported in the same condition, provide the province the goods were shipped from.");
			this.JI_StateOrRegionOfOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(559, 40, true);
			this.JI_StateOrRegionOfOriginDropEdit.Name = "JI_StateOrRegionOfOriginDropEdit";
			this.JI_StateOrRegionOfOriginDropEdit.PreBoundMaxLength = 2;
			this.JI_StateOrRegionOfOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.JI_StateOrRegionOfOriginDropEdit.TabIndex = 7;
			// 
			// PermitsGroupBox
			// 
			this.PermitsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)));
			this.PermitsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|8920fc02-71b9-4f04-b040-718058ced2b2", "Permits");
			this.PermitsGroupBox.Controls.Add(this.PermitsGrid);
			this.PermitsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 54, true);
			this.PermitsGroupBox.Name = "PermitsGroupBox";
			this.PermitsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 130, true);
			this.PermitsGroupBox.TabIndex = 3;
			this.PermitsGroupBox.TabStop = false;
			// 
			// PermitsGrid
			// 
			this.PermitsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitsGrid, "FilteredInvoiceLines.Permits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Permits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.InvoiceLineExportPermit)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Permits)).SyncRoot)).CY_Data)));
			this.PermitsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|504da8f8-dee7-4444-85e0-62974d925306", "Permit Number", "Must be provided for controlled or regulated goods that require export permits.");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.PermitsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PermitsGrid.GridId = "38db2164-99e7-461e-9385-fab1ca2f63aa";
			this.PermitsGrid.CopySelectedRowsAllowed = true;
			this.PermitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitsGrid.LayoutKey = "zGrid1";
			this.PermitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PermitsGrid.Name = "PermitsGrid";
			this.PermitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 111, true);
			this.PermitsGrid.TabIndex = 0;
			//
			// Splitter
			//
			this.Splitter.MinSize = 330;
			// 
			// TariffFindBox
			// 
			this.BindingSource.SetBindingMember(this.TariffFindBox, "FilteredInvoiceLines.JI_FormattedTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Common.TariffPropertyInfo)(((Enterprise.Customs.CA.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_FormattedTariffCodeTariffInfo)));
			this.TariffFindBox.BindToTariffPropertyInfo = "FilteredInvoiceLines.JI_FormattedTariffCodeTariffInfo";
			this.TariffFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|afa131b5-e6be-473a-940a-e93ddeecf339", "HS Code", "Must be a 10-digit Canadian National Customs Tariff code.");
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 14, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 20, true);
			this.TariffFindBox.TabIndex = 0;
			//
			// JI_LinePriceBoundCurrencyControl
			//
			this.JI_LinePriceBoundCurrencyControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("CAExportInvoiceLineUserControl|ec93d0c4-38cc-41f8-a09c-6eab740175b8", "Price", "Line price for line item.");
			// 
			// CAExportInvoiceLineUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CAExportInvoiceLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 577, true);
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.LineDetailTabControl.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.LineSummaryPanel.ResumeLayout(false);
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.ClassificationPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PermitsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PermitsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit JI_StateOrRegionOfOriginDropEdit;
		private Enterprise.ZArchitecture.ZTextBox CA_ConveyanceIdentificationNumberTextBox;
		private Enterprise.ZArchitecture.ZGrid PermitsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PermitsGroupBox;
		private Enterprise.Customs.GUI.TariffFindBox TariffFindBox;
	}
}
