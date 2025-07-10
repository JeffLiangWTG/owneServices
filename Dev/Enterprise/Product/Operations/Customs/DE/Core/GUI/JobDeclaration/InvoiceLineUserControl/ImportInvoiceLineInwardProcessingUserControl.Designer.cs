using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportInvoiceLineInwardProcessingUserControl
	{
		void InitializeComponent()
		{
			Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo tariffColumnStyleInfo1 = new Enterprise.Customs.Universal.GUI.TariffColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExtraInfoForClassificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EconomicConditionsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IdentificationMeansTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProcesedProductsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProcessedProductGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DescriptionGroupBox.SuspendLayout();
			this.EconomicConditionsDropEdit.SuspendLayout();
			this.IdentificationMeansTypeDropEdit.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ProcesedProductsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProcessedProductGrid)).BeginInit();
			this.ProcessedProductGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("f798377c-c83b-4e91-9ffd-970d4e332846", "Description");
			this.DescriptionGroupBox.Controls.Add(this.ExtraInfoForClassificationTextBox);
			this.DescriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DescriptionGroupBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 0, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 74, true);
			this.DescriptionGroupBox.TabIndex = 1;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// ExtraInfoForClassificationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExtraInfoForClassificationTextBox, "FilteredInvoiceLines.JI_ExtraInfoForClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ExtraInfoForClassification)));
			this.ExtraInfoForClassificationTextBox.CaptionResourceString = null;
			this.ExtraInfoForClassificationTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExtraInfoForClassificationTextBox, false);
			this.ExtraInfoForClassificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ExtraInfoForClassificationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExtraInfoForClassificationTextBox.Multiline = true;
			this.ExtraInfoForClassificationTextBox.Name = "ExtraInfoForClassificationTextBox";
			this.ExtraInfoForClassificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 55, true);
			this.ExtraInfoForClassificationTextBox.TabIndex = 0;
			// 
			// EconomicConditionsDropEdit
			// 
			this.EconomicConditionsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EconomicConditionsDropEdit, "FilteredInvoiceLines.ZG_EconomicConditions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_EconomicConditions)));
			this.EconomicConditionsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 19, true);
			this.EconomicConditionsDropEdit.Name = "EconomicConditionsDropEdit";
			this.EconomicConditionsDropEdit.ShouldResizeByMaxLength = true;
			this.EconomicConditionsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.EconomicConditionsDropEdit.TabIndex = 0;
			// 
			// IdentificationMeansTypeDropEdit
			// 
			this.IdentificationMeansTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IdentificationMeansTypeDropEdit, "FilteredInvoiceLines.ZG_IdentificationMeansType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZG_IdentificationMeansType)));
			this.IdentificationMeansTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 45, true);
			this.IdentificationMeansTypeDropEdit.Name = "IdentificationMeansTypeDropEdit";
			this.IdentificationMeansTypeDropEdit.ShouldResizeByMaxLength = true;
			this.IdentificationMeansTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.IdentificationMeansTypeDropEdit.TabIndex = 1;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.Controls.Add(this.DescriptionGroupBox);
			this.DetailsPanel.Controls.Add(this.DetailsGroupBox);
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 74, true);
			this.DetailsPanel.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("310aebca-6881-4d10-a1f4-53f95102ebbb", "Inward Processing Details");
			this.DetailsGroupBox.Controls.Add(this.EconomicConditionsDropEdit);
			this.DetailsGroupBox.Controls.Add(this.IdentificationMeansTypeDropEdit);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DetailsGroupBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 74, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ProcesedProductsGroupBox
			//
			this.ProcesedProductsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("d53518fa-3c8d-4021-b58e-bf02d4a65563", "Processed Products");
			this.ProcesedProductsGroupBox.Controls.Add(this.ProcessedProductGrid);
			this.ProcesedProductsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcesedProductsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 74, true);
			this.ProcesedProductsGroupBox.Name = "ProcesedProductsGroupBox";
			this.ProcesedProductsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 211, true);
			this.ProcesedProductsGroupBox.TabIndex = 1;
			this.ProcesedProductsGroupBox.TabStop = false;
			// 
			// ProcessedProductGrid
			// 
			this.ProcessedProductGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProcessedProductGrid, "FilteredInvoiceLines.InwardProcessingProducts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InwardProcessingProducts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.InwardProcessingProduct)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InwardProcessingProducts)).SyncRoot)).FormattedTariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.InwardProcessingProduct)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InwardProcessingProducts)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.InwardProcessingProduct)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InwardProcessingProducts)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.InwardProcessingProduct)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).InwardProcessingProducts)).SyncRoot)).CSI_AdditionalDescription)));
			this.ProcessedProductGrid.CaptionVisible = false;
			tariffColumnStyleInfo1.ColumnName = "FormattedTariff";
			tariffColumnStyleInfo1.IsMandatory = true;
			tariffColumnStyleInfo1.SelectNomenclatureModes = null;
			tariffColumnStyleInfo1.TariffType = Customs.Business.UniversalReferenceConstants.CusTariffTypes.ExportTariff;
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zMultiLineTextBoxColumnInfo1.ColumnName = "CSI_AdditionalDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ProcessedProductGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.ProcessedProductGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProcessedProductGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProcessedProductGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ProcessedProductGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProcessedProductGrid.GridId = "fdcc0829-9cf4-443b-8eda-6d4713b82d3d";
			this.ProcessedProductGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProcessedProductGrid.LayoutKey = "InwardProcessingProductGrid";
			this.ProcessedProductGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ProcessedProductGrid.Name = "ProcessedProductGrid";
			this.ProcessedProductGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(903, 192, true);
			this.ProcessedProductGrid.TabIndex = 0;
			// 
			// ImportInvoiceLineInwardProcessingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProcesedProductsGroupBox);
			this.Controls.Add(this.DetailsPanel);
			this.Name = "ImportInvoiceLineInwardProcessingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 285, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionGroupBox.ResumeLayout(false);
			this.DescriptionGroupBox.PerformLayout();
			this.EconomicConditionsDropEdit.ResumeLayout(true);
			this.EconomicConditionsDropEdit.PerformLayout();
			this.IdentificationMeansTypeDropEdit.ResumeLayout(true);
			this.IdentificationMeansTypeDropEdit.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ProcesedProductsGroupBox.ResumeLayout(false);
			this.ProcesedProductsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProcessedProductGrid)).EndInit();
			this.ProcessedProductGrid.ResumeLayout(false);
			this.ProcessedProductGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox DescriptionGroupBox;
		private ZArchitecture.GUI.ZPanel DetailsPanel;
		private ZArchitecture.ZTextBox ExtraInfoForClassificationTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit EconomicConditionsDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ProcesedProductsGroupBox;
		private ZArchitecture.ZGrid ProcessedProductGrid;
		private Enterprise.ZArchitecture.GUI.ZDropEdit IdentificationMeansTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
	}
}
