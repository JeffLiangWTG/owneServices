namespace Enterprise.Customs.BE.GUI.PlugIn
{
	partial class InvoiceLineSupportingDocumentsFieldsControl
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
			this.SupDocAdditionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupDocLineNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SupDocUnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.CSI_RX_NKCurrencyCodeFindBox.SuspendLayout();
			this.SupDocTypeCodeFindBox.SuspendLayout();
			this.CSI_StatusDropEdit.SuspendLayout();
			this.CSI_DateOfExpiryDateEdit.SuspendLayout();
			this.SupDocReferenceCodeFindBox.SuspendLayout();
			this.CSI_DateOfIssueDateEdit.SuspendLayout();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupDocUnitOfQuantityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// CSI_RX_NKCurrencyCodeFindBox
			// 
			this.CSI_RX_NKCurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 87, true);
			this.CSI_RX_NKCurrencyCodeFindBox.TabIndex = 5;
			// 
			// SupDocQuantityCalcEdit
			// 
			this.SupDocQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 64, true);
			this.SupDocQuantityCalcEdit.TabIndex = 2;
			// 
			// CSI_StatusDropEdit
			// 
			this.CSI_StatusDropEdit.Visible = false;
			// 
			// CSI_Quantity2CalcEdit
			// 
			this.CSI_Quantity2CalcEdit.Visible = false;
			// 
			// CSI_UnitOfQuantity2TextBox
			// 
			this.CSI_UnitOfQuantity2TextBox.Visible = false;
			// 
			// CSI_ValueCalcEdit
			// 
			this.CSI_ValueCalcEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("0ADDAD34-6D42-44A8-BAAB-3D4081144D98", "Amount");
			this.CSI_ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 87, true);
			this.CSI_ValueCalcEdit.TabIndex = 4;
			// 
			// CSI_DateOfExpiryDateEdit
			// 
			this.CSI_DateOfExpiryDateEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("2F10A8EE-03F8-4A97-9BA7-E041ADB15973", "Validity Date");
			this.CSI_DateOfExpiryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 109, true);
			this.CSI_DateOfExpiryDateEdit.TabIndex = 6;
			// 
			// CSI_UnitOfQuantityTextBox
			// 
			this.CSI_UnitOfQuantityTextBox.Visible = false;
			// 
			// CSI_DateOfIssueDateEdit
			// 
			this.CSI_DateOfIssueDateEdit.Visible = false;
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocUnitOfQuantityDropEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocLineNumberCalcEdit);
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupDocAdditionalDescriptionTextBox);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocAdditionalDescriptionTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocLineNumberCalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocUnitOfQuantityDropEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_StatusDropEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocReferenceCodeFindBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_DateOfExpiryDateEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocQuantityCalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_Quantity2CalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_UnitOfQuantityTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_RX_NKCurrencyCodeFindBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocReferenceTextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_UnitOfQuantity2TextBox, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_DateOfIssueDateEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.CSI_ValueCalcEdit, 0);
			this.SupportingDocumentsGroupBox.Controls.SetChildIndex(this.SupDocTypeCodeFindBox, 0);
			// 
			// SupDocAdditionalDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupDocAdditionalDescriptionTextBox, "FilteredInvoiceLines.SupportingDocuments.CSI_AdditionalDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_AdditionalDescription)));
			this.SupDocAdditionalDescriptionTextBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("7E60D3DE-5E00-4095-83D4-465F2380E619", "Issuing Authority");
			this.SupDocAdditionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 132, true);
			this.SupDocAdditionalDescriptionTextBox.Name = "SupDocAdditionalDescriptionTextBox";
			this.SupDocAdditionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.SupDocAdditionalDescriptionTextBox.TabIndex = 7;
			// 
			// SupDocLineNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SupDocLineNumberCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_ItemNumber)));
			this.SupDocLineNumberCalcEdit.DecimalPlaces = 2;
			this.SupDocLineNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 132, true);
			this.SupDocLineNumberCalcEdit.Name = "SupDocLineNumberCalcEdit";
			this.SupDocLineNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 20, true);
			this.SupDocLineNumberCalcEdit.TabIndex = 8;
			this.SupDocLineNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SupDocUnitOfQuantityDropEdit
			// 
			this.SupDocUnitOfQuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupDocUnitOfQuantityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.SupDocUnitOfQuantityDropEdit.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("586393B2-B7BF-45A2-A791-43AF3C86C99E", "Unit of Measure");
			this.SupDocUnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 64, true);
			this.SupDocUnitOfQuantityDropEdit.Name = "SupDocUnitOfQuantityDropEdit";
			this.SupDocUnitOfQuantityDropEdit.ShouldResizeByMaxLength = true;
			this.SupDocUnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.SupDocUnitOfQuantityDropEdit.TabIndex = 3;
			// 
			// InvoiceLineSupportingDocumentsFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "InvoiceLineSupportingDocumentsFieldsControl";
			this.CSI_RX_NKCurrencyCodeFindBox.ResumeLayout(true);
			this.CSI_RX_NKCurrencyCodeFindBox.PerformLayout();
			this.SupDocTypeCodeFindBox.ResumeLayout(true);
			this.SupDocTypeCodeFindBox.PerformLayout();
			this.CSI_StatusDropEdit.ResumeLayout(true);
			this.CSI_StatusDropEdit.PerformLayout();
			this.CSI_DateOfExpiryDateEdit.ResumeLayout(true);
			this.CSI_DateOfExpiryDateEdit.PerformLayout();
			this.SupDocReferenceCodeFindBox.ResumeLayout(true);
			this.SupDocReferenceCodeFindBox.PerformLayout();
			this.CSI_DateOfIssueDateEdit.ResumeLayout(true);
			this.CSI_DateOfIssueDateEdit.PerformLayout();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupDocUnitOfQuantityDropEdit.ResumeLayout(true);
			this.SupDocUnitOfQuantityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox SupDocAdditionalDescriptionTextBox;
		private ZArchitecture.ZCalcEdit SupDocLineNumberCalcEdit;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth SupDocUnitOfQuantityDropEdit;
	}
}
