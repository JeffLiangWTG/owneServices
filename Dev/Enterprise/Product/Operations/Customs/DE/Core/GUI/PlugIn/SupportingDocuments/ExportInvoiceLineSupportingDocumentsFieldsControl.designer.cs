
namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class ExportInvoiceLineSupportingDocumentsFieldsControl
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
            this.CSI_ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CSI_ValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CSI_DateOfExpiryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.CSI_DateOfIssueDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.CSI_ReferenceNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CSI_UnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CSI_UnitOfQuantity2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.CSI_ReferenceNumber2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_FullTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.CSI_AdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CSI_ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.CSI_RX_NKCurrencyCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CSI_DateOfExpiryDateEdit.SuspendLayout();
            this.CSI_DateOfIssueDateEdit.SuspendLayout();
            this.SupportingDocumentsGroupBox.SuspendLayout();
            this.CSI_ReferenceNumberCodeFindBox.SuspendLayout();
            this.CSI_UnitOfQuantityDropEdit.SuspendLayout();
            this.CSI_UnitOfQuantity2DropEdit.SuspendLayout();
            this.CSI_FullTypeCodeFindBox.SuspendLayout();
            this.CSI_RX_NKCurrencyCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
            // 
            // CSI_ReferenceNumberTextBox
            // 
            this.CSI_ReferenceNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CSI_ReferenceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
            this.CSI_ReferenceNumberTextBox.CaptionResourceString = null;
            this.CSI_ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 37, true);
            this.CSI_ReferenceNumberTextBox.Name = "CSI_ReferenceNumberTextBox";
            this.CSI_ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 15, true);
            this.CSI_ReferenceNumberTextBox.TabIndex = 1;
            // 
            // CSI_QuantityCalcEdit
            // 
            this.CSI_QuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_QuantityCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Quantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Quantity)));
            this.CSI_QuantityCalcEdit.CaptionResourceString = null;
            this.CSI_QuantityCalcEdit.DecimalPlaces = 4;
            this.CSI_QuantityCalcEdit.Decimals = 4;
            this.CSI_QuantityCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
            this.CSI_QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 81, true);
            this.CSI_QuantityCalcEdit.Name = "CSI_QuantityCalcEdit";
            this.CSI_QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.CSI_QuantityCalcEdit.TabIndex = 4;
            this.CSI_QuantityCalcEdit.Text = "0.0000";
            this.CSI_QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CSI_ValueCalcEdit
            // 
            this.CSI_ValueCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_ValueCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_Value");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Value)));
            this.CSI_ValueCalcEdit.CaptionResourceString = null;
            this.CSI_ValueCalcEdit.DecimalPlaces = 2;
            this.CSI_ValueCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
            this.CSI_ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 103, true);
            this.CSI_ValueCalcEdit.Name = "CSI_ValueCalcEdit";
            this.CSI_ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.CSI_ValueCalcEdit.TabIndex = 7;
            this.CSI_ValueCalcEdit.Text = "0.00";
            this.CSI_ValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CSI_DateOfExpiryDateEdit
            // 
            this.CSI_DateOfExpiryDateEdit.AllowDrop = true;
            this.CSI_DateOfExpiryDateEdit.AutoCompleteMonthThreshold = 1;
            this.CSI_DateOfExpiryDateEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_DateOfExpiryDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfExpiry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfExpiry)));
            this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 126, true);
            this.CSI_DateOfExpiryDateEdit.Name = "CSI_DateOfExpiryDateEdit";
            this.CSI_DateOfExpiryDateEdit.TabIndex = 10;
            // 
            // CSI_DateOfIssueDateEdit
            // 
            this.CSI_DateOfIssueDateEdit.AllowDrop = true;
            this.CSI_DateOfIssueDateEdit.AutoCompleteMonthThreshold = 1;
            this.CSI_DateOfIssueDateEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_DateOfIssueDateEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_DateOfIssue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_DateOfIssue)));
            this.CSI_DateOfIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 126, true);
            this.CSI_DateOfIssueDateEdit.Name = "CSI_DateOfIssueDateEdit";
            this.CSI_DateOfIssueDateEdit.TabIndex = 9;
            // 
            // SupportingDocumentsGroupBox
            // 
            this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("E69940D6-16AA-47F3-AC06-F9F841682503", "[44] Supporting Documents");
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberCodeFindBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantityDropEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_UnitOfQuantity2DropEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumber2TextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DescriptionTextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_FullTypeCodeFindBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ValueCalcEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfIssueDateEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ReferenceNumberTextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_QuantityCalcEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_DateOfExpiryDateEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_AdditionalDescriptionTextBox);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_ItemNumberCalcEdit);
            this.SupportingDocumentsGroupBox.Controls.Add(this.CSI_RX_NKCurrencyCodeFindBox);
            this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
            this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 200, true);
            this.SupportingDocumentsGroupBox.TabIndex = 11;
            this.SupportingDocumentsGroupBox.TabStop = false;
            // 
            // CSI_ReferenceNumberCodeFindBox
            // 
            this.CSI_ReferenceNumberCodeFindBox.AllowDrop = true;
            this.CSI_ReferenceNumberCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumberCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
            this.CSI_ReferenceNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 37, true);
            this.CSI_ReferenceNumberCodeFindBox.Name = "CSI_ReferenceNumberCodeFindBox";
            this.CSI_ReferenceNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_ReferenceNumberCodeFindBox.ParentType = null;
            this.CSI_ReferenceNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 15, true);
            this.CSI_ReferenceNumberCodeFindBox.TabIndex = 1;
            // 
            // CSI_UnitOfQuantityDropEdit
            // 
            this.CSI_UnitOfQuantityDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
            this.CSI_UnitOfQuantityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 81, true);
            this.CSI_UnitOfQuantityDropEdit.Name = "CSI_UnitOfQuantityDropEdit";
            this.CSI_UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 15, true);
            this.CSI_UnitOfQuantityDropEdit.TabIndex = 5;
            // 
            // CSI_UnitOfQuantity2DropEdit
            // 
            this.CSI_UnitOfQuantity2DropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CSI_UnitOfQuantity2DropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity2)));
            this.CSI_UnitOfQuantity2DropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_UnitOfQuantity2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(683, 81, true);
            this.CSI_UnitOfQuantity2DropEdit.Name = "CSI_UnitOfQuantity2DropEdit";
            this.CSI_UnitOfQuantity2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 15, true);
            this.CSI_UnitOfQuantity2DropEdit.TabIndex = 6;
            // 
            // CSI_ReferenceNumber2TextBox
            // 
            this.BindingSource.SetBindingMember(this.CSI_ReferenceNumber2TextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_ReferenceNumber2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber2)));
            this.CSI_ReferenceNumber2TextBox.CaptionResourceString = null;
            this.CSI_ReferenceNumber2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_ReferenceNumber2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(683, 59, true);
            this.CSI_ReferenceNumber2TextBox.Name = "CSI_ReferenceNumber2TextBox";
            this.CSI_ReferenceNumber2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 15, true);
            this.CSI_ReferenceNumber2TextBox.TabIndex = 3;
            // 
            // CSI_DescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.CSI_DescriptionTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_Description)));
            this.CSI_DescriptionTextBox.CaptionResourceString = null;
            this.CSI_DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 59, true);
            this.CSI_DescriptionTextBox.Name = "CSI_DescriptionTextBox";
            this.CSI_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 15, true);
            this.CSI_DescriptionTextBox.TabIndex = 2;
            // 
            // CSI_FullTypeCodeFindBox
            // 
            this.CSI_FullTypeCodeFindBox.AllowDrop = true;
            this.CSI_FullTypeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CSI_FullTypeCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_FullType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_FullType)));
            this.CSI_FullTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 15, true);
            this.CSI_FullTypeCodeFindBox.Name = "CSI_FullTypeCodeFindBox";
            this.CSI_FullTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_FullTypeCodeFindBox.ParentType = null;
            this.CSI_FullTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(675, 15, true);
            this.CSI_FullTypeCodeFindBox.TabIndex = 0;
            // 
            // CSI_AdditionalDescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.CSI_AdditionalDescriptionTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_AdditionalDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
            this.CSI_AdditionalDescriptionTextBox.CaptionResourceString = null;
            this.CSI_AdditionalDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.CSI_AdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 149, true);
            this.CSI_AdditionalDescriptionTextBox.Name = "CSI_AdditionalDescriptionTextBox";
            this.CSI_AdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 15, true);
            this.CSI_AdditionalDescriptionTextBox.TabIndex = 11;
            // 
            // CSI_ItemNumberCalcEdit
            // 
            this.CSI_ItemNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.CSI_ItemNumberCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_ItemNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
            this.CSI_ItemNumberCalcEdit.CaptionResourceString = null;
            this.CSI_ItemNumberCalcEdit.DecimalPlaces = 0;
            this.CSI_ItemNumberCalcEdit.Decimals = 0;
            this.CSI_ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 172, true);
            this.CSI_ItemNumberCalcEdit.Name = "CSI_ItemNumberCalcEdit";
            this.CSI_ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 15, true);
            this.CSI_ItemNumberCalcEdit.TabIndex = 12;
            this.CSI_ItemNumberCalcEdit.Text = "0";
            this.CSI_ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CSI_RX_NKCurrencyCodeFindBox
            // 
            this.CSI_RX_NKCurrencyCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CSI_RX_NKCurrencyCodeFindBox, "FilteredInvoiceLines.SupportingDocuments.CSI_RX_NKCurrency");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_RX_NKCurrency)));
            this.CSI_RX_NKCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 103, true);
            this.CSI_RX_NKCurrencyCodeFindBox.Name = "CSI_RX_NKCurrencyCodeFindBox";
            this.CSI_RX_NKCurrencyCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CSI_RX_NKCurrencyCodeFindBox.ParentType = null;
            this.CSI_RX_NKCurrencyCodeFindBox.PreBoundMaxLength = 4;
            this.CSI_RX_NKCurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 15, true);
            this.CSI_RX_NKCurrencyCodeFindBox.TabIndex = 8;
            // 
            // ExportInvoiceLineSupportingDocumentsFieldsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.SupportingDocumentsGroupBox);
            this.Name = "ExportInvoiceLineSupportingDocumentsFieldsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 200, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
            this.CSI_DateOfExpiryDateEdit.PerformLayout();
            this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
            this.CSI_DateOfIssueDateEdit.PerformLayout();
            this.SupportingDocumentsGroupBox.ResumeLayout(false);
            this.SupportingDocumentsGroupBox.PerformLayout();
            this.CSI_ReferenceNumberCodeFindBox.ResumeLayout(true);
            this.CSI_ReferenceNumberCodeFindBox.PerformLayout();
            this.CSI_UnitOfQuantityDropEdit.ResumeLayout(true);
            this.CSI_UnitOfQuantityDropEdit.PerformLayout();
            this.CSI_UnitOfQuantity2DropEdit.ResumeLayout(true);
            this.CSI_UnitOfQuantity2DropEdit.PerformLayout();
            this.CSI_FullTypeCodeFindBox.ResumeLayout(true);
            this.CSI_FullTypeCodeFindBox.PerformLayout();
            this.CSI_RX_NKCurrencyCodeFindBox.ResumeLayout(true);
            this.CSI_RX_NKCurrencyCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CSI_ReferenceNumberTextBox;
		private ZArchitecture.ZCalcEdit CSI_QuantityCalcEdit;
		private ZArchitecture.ZCalcEdit CSI_ValueCalcEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfExpiryDateEdit;
		private ZArchitecture.GUI.ZDateEdit CSI_DateOfIssueDateEdit;
		private ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CSI_FullTypeCodeFindBox;
		private ZArchitecture.ZTextBox CSI_DescriptionTextBox;
		private ZArchitecture.ZTextBox CSI_ReferenceNumber2TextBox;
		private ZArchitecture.GUI.ZDropEdit CSI_UnitOfQuantityDropEdit;
		private ZArchitecture.GUI.ZDropEdit CSI_UnitOfQuantity2DropEdit;
		private ZArchitecture.GUI.ZCodeFindBox CSI_ReferenceNumberCodeFindBox;
		private ZArchitecture.ZTextBox CSI_AdditionalDescriptionTextBox;
		private ZArchitecture.ZCalcEdit CSI_ItemNumberCalcEdit;
		public ZArchitecture.GUI.ZCodeFindBox CSI_RX_NKCurrencyCodeFindBox;
	}
}
