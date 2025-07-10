using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class InvoiceLineRFPDetailsUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InvoiceLineRFPDetailsUserControl));
			this.ProductDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductConditionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductConditionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProductPartDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EUTariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.QuarantineEUTariffFindBox();
			this.FinalConsumerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DrawbackIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TreatmentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NatureOfCommodityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_ClientLineItemIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_RelatedExportPermitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.JI_RelatedExportPermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_RelatedExportPermitAuthorityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_AddtionalDeclarationCommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_ProductDescriptionQualityQualifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_CategoryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QL_ProductDescriptionLocationQualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_CutCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QL_PreservationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_PackTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_SupplimentaryCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.QL_SupplimentaryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QL_ProductTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProductDetailsGroupBox.SuspendLayout();
			this.ProductConditionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductConditionGrid)).BeginInit();
			this.ProductConditionGrid.SuspendLayout();
			this.ProductPartDropEdit.SuspendLayout();
			this.EUTariffFindBox.SuspendLayout();
			this.TreatmentCodeDropEdit.SuspendLayout();
			this.NatureOfCommodityDropEdit.SuspendLayout();
			this.JI_RelatedExportPermitDateDateEdit.SuspendLayout();
			this.JI_RelatedExportPermitAuthorityDropEdit.SuspendLayout();
			this.QL_CategoryCodeFindBox.SuspendLayout();
			this.QL_ProductDescriptionLocationQualifierDropEdit.SuspendLayout();
			this.QL_CutCodeCodeFindBox.SuspendLayout();
			this.QL_PreservationTypeDropEdit.SuspendLayout();
			this.QL_PackTypeDropEdit.SuspendLayout();
			this.QL_SupplimentaryCodeCodeFindBox.SuspendLayout();
			this.QL_SupplimentaryCodeDropEdit.SuspendLayout();
			this.QL_ProductTypeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			// 
			// ProductDetailsGroupBox
			// 
			this.ProductDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProductDetailsGroupBox.Controls.Add(this.ProductConditionGroupBox);
			this.ProductDetailsGroupBox.Controls.Add(this.ProductPartDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.EUTariffFindBox);
			this.ProductDetailsGroupBox.Controls.Add(this.FinalConsumerCheckBox);
			this.ProductDetailsGroupBox.Controls.Add(this.DrawbackIndicatorCheckBox);
			this.ProductDetailsGroupBox.Controls.Add(this.TreatmentCodeDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.NatureOfCommodityDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_ClientLineItemIDTextBox);
			this.ProductDetailsGroupBox.Controls.Add(this.JI_RelatedExportPermitDateDateEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.JI_RelatedExportPermitNumberTextBox);
			this.ProductDetailsGroupBox.Controls.Add(this.JI_RelatedExportPermitAuthorityDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_AddtionalDeclarationCommentsTextBox);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_ProductDescriptionQualityQualifierTextBox);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_CategoryCodeFindBox);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_ProductDescriptionLocationQualifierDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_CutCodeCodeFindBox);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_PreservationTypeDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_PackTypeDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_SupplimentaryCodeCodeFindBox);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_SupplimentaryCodeDropEdit);
			this.ProductDetailsGroupBox.Controls.Add(this.QL_ProductTypeCodeFindBox);
			this.ProductDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ProductDetailsGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 255, true);
			this.ProductDetailsGroupBox.Name = "ProductDetailsGroupBox";
			this.ProductDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 255, true);
			this.ProductDetailsGroupBox.TabIndex = 0;
			this.ProductDetailsGroupBox.TabStop = false;
			this.ProductDetailsGroupBox.Text = "Product Details";
			// 
			// ProductConditionGroupBox
			// 
			this.ProductConditionGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("9DB38ACB-CE20-465F-BBE9-9FC0551BD831", "Product Conditions");
			this.ProductConditionGroupBox.Controls.Add(this.ProductConditionGrid);
			this.ProductConditionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 19, true);
			this.ProductConditionGroupBox.Name = "ProductConditionGroupBox";
			this.ProductConditionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 178, true);
			this.ProductConditionGroupBox.TabIndex = 28;
			this.ProductConditionGroupBox.TabStop = false;
			// 
			// ProductConditionGrid
			// 
			this.ProductConditionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductConditionGrid, "QuarantineExDocLine.ProductConditions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.ProductConditions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ProductCondition)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.ProductConditions)).SyncRoot)).CY_Code)));
			this.ProductConditionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ProductConditionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProductConditionGrid.GridId = "592e3f78-96d6-452a-b8b2-0ed30324e48b";
			this.ProductConditionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductConditionGrid.LayoutKey = "ProductConditionGrid";
			this.ProductConditionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ProductConditionGrid.Name = "ProductConditionGrid";
			this.ProductConditionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 160, true);
			this.ProductConditionGrid.TabIndex = 26;
			// 
			// ProductPartDropEdit
			// 
			this.ProductPartDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductPartDropEdit, "QuarantineExDocLine.QL_ProductPart");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ProductPart)));
			this.ProductPartDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 210, true);
			this.ProductPartDropEdit.Name = "ProductPartDropEdit";
			this.ProductPartDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.ProductPartDropEdit.TabIndex = 27;
			// 
			// EUTariffFindBox
			// 
			this.EUTariffFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EUTariffFindBox, "QuarantineExDocLine.QL_FormattedCombinedNomenclature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_FormattedCombinedNomenclature)));
			this.EUTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 233, true);
			this.EUTariffFindBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.EUTariffFindBox.Name = "EUTariffFindBox";
			this.EUTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 17, true);
			this.EUTariffFindBox.PreBoundMaxLength = 15;
			this.EUTariffFindBox.ShowDescriptionBox = false;
			this.EUTariffFindBox.TabIndex = 17;
			// 
			// FinalConsumerCheckBox
			// 
			this.FinalConsumerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FinalConsumerCheckBox, "QuarantineExDocLine.QL_FinalConsumer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_FinalConsumer)));
			this.FinalConsumerCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FinalConsumerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FinalConsumerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 214, true);
			this.FinalConsumerCheckBox.Name = "FinalConsumerCheckBox";
			this.FinalConsumerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.FinalConsumerCheckBox.TabIndex = 16;
			this.FinalConsumerCheckBox.UseVisualStyleBackColor = true;
			// 
			// DrawbackIndicatorCheckBox
			// 
			this.DrawbackIndicatorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.DrawbackIndicatorCheckBox, "JI_Drawback");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).JI_Drawback)));
			this.DrawbackIndicatorCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("dd11ee1c-1b27-414d-9f22-bdc34e6151e8", "Drawback Ind.", "Drawback Indicator", "");
			this.DrawbackIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DrawbackIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DrawbackIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(779, 149, true);
			this.DrawbackIndicatorCheckBox.Name = "DrawbackIndicatorCheckBox";
			this.DrawbackIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 16, true);
			this.DrawbackIndicatorCheckBox.TabIndex = 25;
			this.DrawbackIndicatorCheckBox.Text = "Drawback Ind.";
			this.DrawbackIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// TreatmentCodeDropEdit
			// 
			this.TreatmentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TreatmentCodeDropEdit, "QuarantineExDocLine.QL_TreatmentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_TreatmentType)));
			this.TreatmentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 191, true);
			this.TreatmentCodeDropEdit.Name = "TreatmentCodeDropEdit";
			this.TreatmentCodeDropEdit.ShouldResizeByMaxLength = true;
			this.TreatmentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.TreatmentCodeDropEdit.TabIndex = 15;
			// 
			// NatureOfCommodityDropEdit
			// 
			this.NatureOfCommodityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NatureOfCommodityDropEdit, "QuarantineExDocLine.QL_NatureOfCommodity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_NatureOfCommodity)));
			this.NatureOfCommodityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 189, true);
			this.NatureOfCommodityDropEdit.Name = "NatureOfCommodityDropEdit";
			this.NatureOfCommodityDropEdit.ShouldResizeByMaxLength = true;
			this.NatureOfCommodityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.NatureOfCommodityDropEdit.TabIndex = 9;
			// 
			// QL_ClientLineItemIDTextBox
			// 
			this.QL_ClientLineItemIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_ClientLineItemIDTextBox, "QuarantineExDocLine.QL_ClientLineItemID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ClientLineItemID)));
			this.QL_ClientLineItemIDTextBox.CaptionResourceString = null;
			this.QL_ClientLineItemIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 169, true);
			this.QL_ClientLineItemIDTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.QL_ClientLineItemIDTextBox.Name = "QL_ClientLineItemIDTextBox";
			this.QL_ClientLineItemIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 17, true);
			this.QL_ClientLineItemIDTextBox.TabIndex = 14;
			// 
			// JI_RelatedExportPermitDateDateEdit
			// 
			this.JI_RelatedExportPermitDateDateEdit.AllowDrop = true;
			this.JI_RelatedExportPermitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.JI_RelatedExportPermitDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JI_RelatedExportPermitDateDateEdit, "JI_RelatedExportPermitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).JI_RelatedExportPermitDate)));
			this.JI_RelatedExportPermitDateDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|6e6c122e-15c2-45e9-b0aa-4beaf6e157ce", "Related Export Permit Date");
			this.JI_RelatedExportPermitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 147, true);
			this.JI_RelatedExportPermitDateDateEdit.Name = "JI_RelatedExportPermitDateDateEdit";
			this.JI_RelatedExportPermitDateDateEdit.TabIndex = 13;
			// 
			// JI_RelatedExportPermitNumberTextBox
			// 
			this.JI_RelatedExportPermitNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JI_RelatedExportPermitNumberTextBox, "JI_RelatedExportPermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).JI_RelatedExportPermitNumber)));
			this.JI_RelatedExportPermitNumberTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|8c6d7a67-76f9-474e-8b4b-a727b3aba4b0", "Related Export Permit Number");
			this.JI_RelatedExportPermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 103, true);
			this.JI_RelatedExportPermitNumberTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.JI_RelatedExportPermitNumberTextBox.Name = "JI_RelatedExportPermitNumberTextBox";
			this.JI_RelatedExportPermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 17, true);
			this.JI_RelatedExportPermitNumberTextBox.TabIndex = 11;
			// 
			// JI_RelatedExportPermitAuthorityDropEdit
			// 
			this.JI_RelatedExportPermitAuthorityDropEdit.AllowDrop = true;
			this.JI_RelatedExportPermitAuthorityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JI_RelatedExportPermitAuthorityDropEdit, "JI_RelatedExportPermitAuthority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).JI_RelatedExportPermitAuthority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).Lookups.EXDOCPermitAuthorityList)));
			this.JI_RelatedExportPermitAuthorityDropEdit.BindToList = "Lookups+EXDOCPermitAuthorityList";
			this.JI_RelatedExportPermitAuthorityDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|099a1dac-11e7-4620-82c4-dadbc03a67ed", "Related Export Permit Authority");
			this.JI_RelatedExportPermitAuthorityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 125, true);
			this.JI_RelatedExportPermitAuthorityDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.JI_RelatedExportPermitAuthorityDropEdit.Name = "JI_RelatedExportPermitAuthorityDropEdit";
			this.JI_RelatedExportPermitAuthorityDropEdit.ShouldResizeByMaxLength = true;
			this.JI_RelatedExportPermitAuthorityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 17, true);
			this.JI_RelatedExportPermitAuthorityDropEdit.TabIndex = 12;
			// 
			// QL_AddtionalDeclarationCommentsTextBox
			// 
			this.QL_AddtionalDeclarationCommentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_AddtionalDeclarationCommentsTextBox, "QuarantineExDocLine.QL_AddtionalDeclarationComments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_AddtionalDeclarationComments)));
			this.QL_AddtionalDeclarationCommentsTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.QL_AddtionalDeclarationCommentsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.QL_AddtionalDeclarationCommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 31, true);
			this.QL_AddtionalDeclarationCommentsTextBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 78, true);
			this.QL_AddtionalDeclarationCommentsTextBox.Multiline = true;
			this.QL_AddtionalDeclarationCommentsTextBox.Name = "QL_AddtionalDeclarationCommentsTextBox";
			this.QL_AddtionalDeclarationCommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 69, true);
			this.QL_AddtionalDeclarationCommentsTextBox.TabIndex = 10;
			// 
			// QL_ProductDescriptionQualityQualifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.QL_ProductDescriptionQualityQualifierTextBox, "QuarantineExDocLine.QL_ProductDescriptionQualityQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ProductDescriptionQualityQualifier)));
			this.QL_ProductDescriptionQualityQualifierTextBox.CaptionResourceString = null;
			this.QL_ProductDescriptionQualityQualifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 168, true);
			this.QL_ProductDescriptionQualityQualifierTextBox.Name = "QL_ProductDescriptionQualityQualifierTextBox";
			this.QL_ProductDescriptionQualityQualifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_ProductDescriptionQualityQualifierTextBox.TabIndex = 8;
			// 
			// QL_CategoryCodeFindBox
			// 
			this.QL_CategoryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_CategoryCodeFindBox, "QuarantineExDocLine.QL_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_Category)));
			this.QL_CategoryCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("4d7237b7-5427-428d-939a-4e0959ff4bda", "Category Code");
			this.QL_CategoryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 40, true);
			this.QL_CategoryCodeFindBox.Name = "QL_CategoryCodeFindBox";
			this.QL_CategoryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.QL_CategoryCodeFindBox.ParentType = null;
			this.QL_CategoryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_CategoryCodeFindBox.TabIndex = 2;
			// 
			// QL_ProductDescriptionLocationQualifierDropEdit
			// 
			this.QL_ProductDescriptionLocationQualifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_ProductDescriptionLocationQualifierDropEdit, "QuarantineExDocLine.QL_ProductDescriptionLocationQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ProductDescriptionLocationQualifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.LocationQualifier)));
			this.QL_ProductDescriptionLocationQualifierDropEdit.BindToList = "QuarantineExDocLine.Lookups+LocationQualifier";
			this.QL_ProductDescriptionLocationQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 147, true);
			this.QL_ProductDescriptionLocationQualifierDropEdit.Name = "QL_ProductDescriptionLocationQualifierDropEdit";
			this.QL_ProductDescriptionLocationQualifierDropEdit.ShouldResizeByMaxLength = true;
			this.QL_ProductDescriptionLocationQualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_ProductDescriptionLocationQualifierDropEdit.TabIndex = 7;
			// 
			// QL_CutCodeCodeFindBox
			// 
			this.QL_CutCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_CutCodeCodeFindBox, "QuarantineExDocLine.QL_CutCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_CutCode)));
			this.QL_CutCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 125, true);
			this.QL_CutCodeCodeFindBox.Name = "QL_CutCodeCodeFindBox";
			this.QL_CutCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.QL_CutCodeCodeFindBox.ParentType = null;
			this.QL_CutCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_CutCodeCodeFindBox.TabIndex = 6;
			// 
			// QL_PreservationTypeDropEdit
			// 
			this.QL_PreservationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_PreservationTypeDropEdit, "QuarantineExDocLine.QL_PreservationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_PreservationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.Preservation)));
			this.QL_PreservationTypeDropEdit.BindToList = "QuarantineExDocLine.Lookups+Preservation";
			this.QL_PreservationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 104, true);
			this.QL_PreservationTypeDropEdit.Name = "QL_PreservationTypeDropEdit";
			this.QL_PreservationTypeDropEdit.ShouldResizeByMaxLength = true;
			this.QL_PreservationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_PreservationTypeDropEdit.TabIndex = 5;
			// 
			// QL_PackTypeDropEdit
			// 
			this.QL_PackTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_PackTypeDropEdit, "QuarantineExDocLine.QL_PackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.PackType)));
			this.QL_PackTypeDropEdit.BindToList = "QuarantineExDocLine.Lookups+PackType";
			this.QL_PackTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 83, true);
			this.QL_PackTypeDropEdit.Name = "QL_PackTypeDropEdit";
			this.QL_PackTypeDropEdit.ShouldResizeByMaxLength = true;
			this.QL_PackTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_PackTypeDropEdit.TabIndex = 4;
			// 
			// QL_SupplimentaryCodeCodeFindBox
			// 
			this.QL_SupplimentaryCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_SupplimentaryCodeCodeFindBox, "QuarantineExDocLine.QL_SupplimentaryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_SupplimentaryCode)));
			this.QL_SupplimentaryCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 61, true);
			this.QL_SupplimentaryCodeCodeFindBox.Name = "QL_SupplimentaryCodeCodeFindBox";
			this.QL_SupplimentaryCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.QL_SupplimentaryCodeCodeFindBox.ParentType = null;
			this.QL_SupplimentaryCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_SupplimentaryCodeCodeFindBox.TabIndex = 3;
			// 
			// QL_SupplimentaryCodeDropEdit
			// 
			this.QL_SupplimentaryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_SupplimentaryCodeDropEdit, "QuarantineExDocLine.QL_SupplimentaryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_SupplimentaryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.SupplementaryCodes)));
			this.QL_SupplimentaryCodeDropEdit.BindToList = "QuarantineExDocLine.Lookups+SupplementaryCodes";
			this.QL_SupplimentaryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 61, true);
			this.QL_SupplimentaryCodeDropEdit.Name = "QL_SupplimentaryCodeDropEdit";
			this.QL_SupplimentaryCodeDropEdit.ShouldResizeByMaxLength = true;
			this.QL_SupplimentaryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_SupplimentaryCodeDropEdit.TabIndex = 3;
			// 
			// QL_ProductTypeCodeFindBox
			// 
			this.QL_ProductTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_ProductTypeCodeFindBox, "QuarantineExDocLine.QL_ProductType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_ProductType)));
			this.QL_ProductTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 19, true);
			this.QL_ProductTypeCodeFindBox.Name = "QL_ProductTypeCodeFindBox";
			this.QL_ProductTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.QL_ProductTypeCodeFindBox.ParentType = null;
			this.QL_ProductTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.QL_ProductTypeCodeFindBox.TabIndex = 1;
			// 
			// InvoiceLineRFPDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ProductDetailsGroupBox);
			this.Name = "InvoiceLineRFPDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 260, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProductDetailsGroupBox.ResumeLayout(false);
			this.ProductDetailsGroupBox.PerformLayout();
			this.ProductConditionGroupBox.ResumeLayout(false);
			this.ProductConditionGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductConditionGrid)).EndInit();
			this.ProductConditionGrid.ResumeLayout(false);
			this.ProductConditionGrid.PerformLayout();
			this.ProductPartDropEdit.ResumeLayout(true);
			this.ProductPartDropEdit.PerformLayout();
			this.EUTariffFindBox.ResumeLayout(true);
			this.EUTariffFindBox.PerformLayout();
			this.TreatmentCodeDropEdit.ResumeLayout(true);
			this.TreatmentCodeDropEdit.PerformLayout();
			this.NatureOfCommodityDropEdit.ResumeLayout(true);
			this.NatureOfCommodityDropEdit.PerformLayout();
			this.JI_RelatedExportPermitDateDateEdit.ResumeLayout(true);
			this.JI_RelatedExportPermitDateDateEdit.PerformLayout();
			this.JI_RelatedExportPermitAuthorityDropEdit.ResumeLayout(true);
			this.JI_RelatedExportPermitAuthorityDropEdit.PerformLayout();
			this.QL_CategoryCodeFindBox.ResumeLayout(true);
			this.QL_CategoryCodeFindBox.PerformLayout();
			this.QL_ProductDescriptionLocationQualifierDropEdit.ResumeLayout(true);
			this.QL_ProductDescriptionLocationQualifierDropEdit.PerformLayout();
			this.QL_CutCodeCodeFindBox.ResumeLayout(true);
			this.QL_CutCodeCodeFindBox.PerformLayout();
			this.QL_PreservationTypeDropEdit.ResumeLayout(true);
			this.QL_PreservationTypeDropEdit.PerformLayout();
			this.QL_PackTypeDropEdit.ResumeLayout(true);
			this.QL_PackTypeDropEdit.PerformLayout();
			this.QL_SupplimentaryCodeCodeFindBox.ResumeLayout(true);
			this.QL_SupplimentaryCodeCodeFindBox.PerformLayout();
			this.QL_SupplimentaryCodeDropEdit.ResumeLayout(true);
			this.QL_SupplimentaryCodeDropEdit.PerformLayout();
			this.QL_ProductTypeCodeFindBox.ResumeLayout(true);
			this.QL_ProductTypeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZGroupBox ProductDetailsGroupBox;
		internal ZGroupBox ProductConditionGroupBox;
		internal ZGrid ProductConditionGrid;
		internal ZDropEdit ProductPartDropEdit;
		QuarantineEUTariffFindBox EUTariffFindBox;
		ZCheckBox FinalConsumerCheckBox;
		ZCheckBox DrawbackIndicatorCheckBox;
		ZDropEdit TreatmentCodeDropEdit;
		ZDropEdit NatureOfCommodityDropEdit;
		ZTextBox QL_ClientLineItemIDTextBox;
		ZDateEdit JI_RelatedExportPermitDateDateEdit;
		ZTextBox JI_RelatedExportPermitNumberTextBox;
		ZDropEdit JI_RelatedExportPermitAuthorityDropEdit;
		ZTextBox QL_AddtionalDeclarationCommentsTextBox;
		ZTextBox QL_ProductDescriptionQualityQualifierTextBox;
		ZCodeFindBox QL_CategoryCodeFindBox;
		ZDropEdit QL_ProductDescriptionLocationQualifierDropEdit;
		ZCodeFindBox QL_CutCodeCodeFindBox;
		ZDropEdit QL_PreservationTypeDropEdit;
		ZDropEdit QL_PackTypeDropEdit;
		ZCodeFindBox QL_SupplimentaryCodeCodeFindBox;
		ZDropEdit QL_SupplimentaryCodeDropEdit;
		ZCodeFindBox QL_ProductTypeCodeFindBox;
	}
}
