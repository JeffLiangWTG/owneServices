namespace Enterprise.Customs.BE.GUI.PlugIn
{
	partial class InvoiceLinePreviousDocumentsUserControl
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

		private void InitializeComponent()
		{
			this.FullTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitOfQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FullTypeCodeFindBox.SuspendLayout();
			this.UnitOfQuantityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Controls.Add(this.FullTypeCodeFindBox);
			this.PrevDocsGroupBox.Controls.Add(this.ItemNumberCalcEdit);
			this.PrevDocsGroupBox.Controls.Add(this.UnitOfQuantityDropEdit);
			this.PrevDocsGroupBox.Controls.Add(this.QuantityCalcEdit);
			this.PrevDocsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.PrevDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 175, true);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.QuantityCalcEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.UnitOfQuantityDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.ItemNumberCalcEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.FullTypeCodeFindBox, 0);
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.Visible = false;
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 95, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 95, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 95, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 218, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobDeclaration);
			// 
			// FullTypeCodeFindBox
			// 
			this.FullTypeCodeFindBox.AllowDrop = true;
			this.FullTypeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FullTypeCodeFindBox, "FilteredInvoiceLines.PreviousDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			this.FullTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 19, true);
			this.FullTypeCodeFindBox.Name = "FullTypeCodeFindBox";
			this.FullTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FullTypeCodeFindBox.ParentType = null;
			this.FullTypeCodeFindBox.PreBoundMaxLength = 4;
			this.FullTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.FullTypeCodeFindBox.TabIndex = 0;
			// 
			// ItemNumberCalcEdit
			// 
			this.ItemNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ItemNumber)));
			this.ItemNumberCalcEdit.CaptionResourceString = null;
			this.ItemNumberCalcEdit.DecimalPlaces = 0;
			this.ItemNumberCalcEdit.Decimals = 0;
			this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 71, true);
			this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
			this.ItemNumberCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.ItemNumberCalcEdit.TabIndex = 2;
			this.ItemNumberCalcEdit.Text = "0";
			this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitOfQuantityDropEdit
			// 
			this.UnitOfQuantityDropEdit.AllowDrop = true;
			this.UnitOfQuantityDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnitOfQuantityDropEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_UnitOfQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
			this.UnitOfQuantityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 97, true);
			this.UnitOfQuantityDropEdit.Name = "UnitOfQuantityDropEdit";
			this.UnitOfQuantityDropEdit.ShouldResizeByMaxLength = true;
			this.UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.UnitOfQuantityDropEdit.TabIndex = 3;
			// 
			// QuantityCalcEdit
			// 
			this.QuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.QuantityCalcEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Quantity)));
			this.QuantityCalcEdit.CaptionResourceString = null;
			this.QuantityCalcEdit.DecimalPlaces = 5;
			this.QuantityCalcEdit.Decimals = 5;
			this.QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 123, true);
			this.QuantityCalcEdit.Name = "QuantityCalcEdit";
			this.QuantityCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.QuantityCalcEdit.TabIndex = 4;
			this.QuantityCalcEdit.Text = "0.00000";
			this.QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FilteredInvoiceLines.PreviousDocuments.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Description)));
			this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("41EEA00C-08BE-4DF2-BDBE-69743643E562", "Complement");
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 149, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 20, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// InvoiceLinePreviousDocumentsUserControl
			// 
			this.Name = "InvoiceLinePreviousDocumentsUserControl";
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FullTypeCodeFindBox.ResumeLayout(true);
			this.FullTypeCodeFindBox.PerformLayout();
			this.UnitOfQuantityDropEdit.ResumeLayout(true);
			this.UnitOfQuantityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZCodeFindBox FullTypeCodeFindBox;
		ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit UnitOfQuantityDropEdit;
		protected ZArchitecture.ZCalcEdit QuantityCalcEdit;
		protected Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
