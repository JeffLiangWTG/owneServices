namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class REXDeclarationsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.REX_QH_ImportedProductFlagCaption = new Enterprise.ZArchitecture.ZLabel();
			this.REX_QH_ImportedProductFlagDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QuarantineSupportingInfosGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.textBoxDescription = new Enterprise.ZArchitecture.ZTextBox();
			this.QuarantineSupportingInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExporterDeclarationForREXTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QH_LegallyImportedFlagCaption = new Enterprise.ZArchitecture.ZLabel();
			this.QH_LegallyImportedFlagDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption = new Enterprise.ZArchitecture.ZLabel();
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.REX_QH_ImportedProductFlagDropEdit.SuspendLayout();
			this.QuarantineSupportingInfosGroupBox.SuspendLayout();
			this.DescriptionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuarantineSupportingInfosGrid)).BeginInit();
			this.QuarantineSupportingInfosGrid.SuspendLayout();
			this.QH_LegallyImportedFlagDropEdit.SuspendLayout();
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// REX_QH_ImportedProductFlagCaption
			// 
			this.REX_QH_ImportedProductFlagCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.REX_QH_ImportedProductFlagCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(855, 249, true);
			this.REX_QH_ImportedProductFlagCaption.Name = "REX_QH_ImportedProductFlagCaption";
			this.REX_QH_ImportedProductFlagCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 33, true);
			this.REX_QH_ImportedProductFlagCaption.TabIndex = 41;
			this.REX_QH_ImportedProductFlagCaption.Text = "Do any of the products listed in this RFP contain imported dairy ingredients, oth" +
    "er than from New Zealand?";
			// 
			// REX_QH_ImportedProductFlagDropEdit
			// 
			this.REX_QH_ImportedProductFlagDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REX_QH_ImportedProductFlagDropEdit, "QuarantineExDocHeader+QH_ImportedProductFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ImportedProductFlag)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.DeclarationOfCompliance)));
			this.REX_QH_ImportedProductFlagDropEdit.BindToList = "QuarantineExDocHeader+Lookups+DeclarationOfCompliance";
			this.REX_QH_ImportedProductFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(797, 255, true);
			this.REX_QH_ImportedProductFlagDropEdit.Name = "REX_QH_ImportedProductFlagDropEdit";
			this.REX_QH_ImportedProductFlagDropEdit.PreBoundMaxLength = 3;
			this.REX_QH_ImportedProductFlagDropEdit.ShouldResizeByMaxLength = true;
			this.REX_QH_ImportedProductFlagDropEdit.ShowDescriptionBox = false;
			this.REX_QH_ImportedProductFlagDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.REX_QH_ImportedProductFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.REX_QH_ImportedProductFlagDropEdit.TabIndex = 40;
			// 
			// QuarantineSupportingInfosGroupBox
			// 
			this.QuarantineSupportingInfosGroupBox.Controls.Add(this.DescriptionGroupBox);
			this.QuarantineSupportingInfosGroupBox.Controls.Add(this.QuarantineSupportingInfosGrid);
			this.QuarantineSupportingInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.QuarantineSupportingInfosGroupBox.Name = "QuarantineSupportingInfosGroupBox";
			this.QuarantineSupportingInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 268, true);
			this.QuarantineSupportingInfosGroupBox.TabIndex = 34;
			this.QuarantineSupportingInfosGroupBox.TabStop = false;
			this.QuarantineSupportingInfosGroupBox.Text = "Declaration Code";
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.Controls.Add(this.textBoxDescription);
			this.DescriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 141, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 125, true);
			this.DescriptionGroupBox.TabIndex = 3;
			this.DescriptionGroupBox.TabStop = false;
			this.DescriptionGroupBox.Text = "Description";
			// 
			// textBoxDescription
			// 
			this.BindingSource.SetBindingMember(this.textBoxDescription, "QuarantineExDocHeader+SupportingInfos.Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.SupportingInfos)).SyncRoot)).Description)));
			this.textBoxDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBoxDescription.CaptionResourceString = null;
			this.textBoxDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.textBoxDescription.Multiline = true;
			this.textBoxDescription.Name = "textBoxDescription";
			this.textBoxDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBoxDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 109, true);
			this.textBoxDescription.TabIndex = 7;
			// 
			// QuarantineSupportingInfosGrid
			// 
			this.QuarantineSupportingInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.QuarantineSupportingInfosGrid, "QuarantineExDocHeader+SupportingInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.SupportingInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.SupportingInfos)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.QuarantineSupportingInfo)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.SupportingInfos)).SyncRoot)).Description)));
			this.QuarantineSupportingInfosGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnComparer = null;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zCodeFindBoxColumnStyleInfo1.MaxLengthOverride = 6;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnComparer = null;
			zMultiLineTextBoxColumnInfo1.ColumnName = "Description";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.QuarantineSupportingInfosGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.QuarantineSupportingInfosGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.QuarantineSupportingInfosGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.QuarantineSupportingInfosGrid.GridId = "4E9B1F0C-F0E4-479B-9CE2-06E07DCDDBE0";
			this.QuarantineSupportingInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.QuarantineSupportingInfosGrid.LayoutKey = "RecommendationLettersGrid";
			this.QuarantineSupportingInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.QuarantineSupportingInfosGrid.Name = "QuarantineSupportingInfosGrid";
			this.QuarantineSupportingInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 126, true);
			this.QuarantineSupportingInfosGrid.TabIndex = 0;
			// 
			// ExporterDeclarationForREXTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExporterDeclarationForREXTextBox, "QuarantineExDocHeader+QH_ExporterDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ExporterDeclaration)));
			this.ExporterDeclarationForREXTextBox.CaptionResourceString = null;
			this.ExporterDeclarationForREXTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(797, 23, true);
			this.ExporterDeclarationForREXTextBox.Multiline = true;
			this.ExporterDeclarationForREXTextBox.Name = "ExporterDeclarationForREXTextBox";
			this.ExporterDeclarationForREXTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 144, true);
			this.ExporterDeclarationForREXTextBox.TabIndex = 35;
			// 
			// QH_LegallyImportedFlagCaption
			// 
			this.QH_LegallyImportedFlagCaption.AutoSize = true;
			this.QH_LegallyImportedFlagCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.QH_LegallyImportedFlagCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(855, 225, true);
			this.QH_LegallyImportedFlagCaption.Name = "QH_LegallyImportedFlagCaption";
			this.QH_LegallyImportedFlagCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 13, true);
			this.QH_LegallyImportedFlagCaption.TabIndex = 39;
			this.QH_LegallyImportedFlagCaption.Text = "Legally Imported Flag";
			// 
			// QH_LegallyImportedFlagDropEdit
			// 
			this.QH_LegallyImportedFlagDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_LegallyImportedFlagDropEdit, "QuarantineExDocHeader+QH_LegallyImportedFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_LegallyImportedFlag)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.DeclarationOfCompliance)));
			this.QH_LegallyImportedFlagDropEdit.BindToList = "QuarantineExDocHeader+Lookups+DeclarationOfCompliance";
			this.QH_LegallyImportedFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(797, 223, true);
			this.QH_LegallyImportedFlagDropEdit.Name = "QH_LegallyImportedFlagDropEdit";
			this.QH_LegallyImportedFlagDropEdit.PreBoundMaxLength = 3;
			this.QH_LegallyImportedFlagDropEdit.ShouldResizeByMaxLength = true;
			this.QH_LegallyImportedFlagDropEdit.ShowDescriptionBox = false;
			this.QH_LegallyImportedFlagDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_LegallyImportedFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.QH_LegallyImportedFlagDropEdit.TabIndex = 37;
			// 
			// QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption
			// 
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption.AutoSize = true;
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(855, 193, true);
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption.Name = "QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption";
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 13, true);
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption.TabIndex = 38;
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption.Text = "Manufactured Treated Packaged Labeled In Australia Flag";
			// 
			// QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit
			// 
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit, "QuarantineExDocHeader+QH_ManufacturedTreatedPackagedLabelledInAustralia");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.DeclarationOfCompliance)));
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.BindToList = "QuarantineExDocHeader+Lookups+DeclarationOfCompliance";
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(797, 187, true);
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.Name = "QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit";
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.PreBoundMaxLength = 3;
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.ShouldResizeByMaxLength = true;
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.ShowDescriptionBox = false;
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.TabIndex = 36;
			// 
			// REXDeclarationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.REX_QH_ImportedProductFlagCaption);
			this.Controls.Add(this.REX_QH_ImportedProductFlagDropEdit);
			this.Controls.Add(this.QuarantineSupportingInfosGroupBox);
			this.Controls.Add(this.ExporterDeclarationForREXTextBox);
			this.Controls.Add(this.QH_LegallyImportedFlagCaption);
			this.Controls.Add(this.QH_LegallyImportedFlagDropEdit);
			this.Controls.Add(this.QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption);
			this.Controls.Add(this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit);
			this.Name = "REXDeclarationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1149, 285, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.REX_QH_ImportedProductFlagDropEdit.ResumeLayout(true);
			this.REX_QH_ImportedProductFlagDropEdit.PerformLayout();
			this.QuarantineSupportingInfosGroupBox.ResumeLayout(false);
			this.QuarantineSupportingInfosGroupBox.PerformLayout();
			this.DescriptionGroupBox.ResumeLayout(false);
			this.DescriptionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.QuarantineSupportingInfosGrid)).EndInit();
			this.QuarantineSupportingInfosGrid.ResumeLayout(false);
			this.QuarantineSupportingInfosGrid.PerformLayout();
			this.QH_LegallyImportedFlagDropEdit.ResumeLayout(true);
			this.QH_LegallyImportedFlagDropEdit.PerformLayout();
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.ResumeLayout(true);
			this.QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel REX_QH_ImportedProductFlagCaption;
		private ZArchitecture.GUI.ZDropEdit REX_QH_ImportedProductFlagDropEdit;
		private ZArchitecture.GUI.ZGroupBox QuarantineSupportingInfosGroupBox;
		private ZArchitecture.GUI.ZGroupBox DescriptionGroupBox;
		private ZArchitecture.ZTextBox textBoxDescription;
		private ZArchitecture.ZGrid QuarantineSupportingInfosGrid;
		private ZArchitecture.ZTextBox ExporterDeclarationForREXTextBox;
		private ZArchitecture.ZLabel QH_LegallyImportedFlagCaption;
		private ZArchitecture.GUI.ZDropEdit QH_LegallyImportedFlagDropEdit;
		private ZArchitecture.ZLabel QH_ManufacturedTreatedPackagedLabelledInAustraliaCaption;
		private ZArchitecture.GUI.ZDropEdit QH_ManufacturedTreatedPackagedLabelledInAustraliaDropEdit;
	}
}
