namespace Enterprise.Customs.DE.GUI
{
	partial class ExportInvoiceLinePreviousDocumentsUserControl
	{
		private void InitializeComponent()
		{
            this.CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
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
            this.CodeFindBox.SuspendLayout();
            this.UnitOfQuantityDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // PrevDocsGroupBox
            // 
            this.PrevDocsGroupBox.Controls.Add(this.CodeFindBox);
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
            this.PrevDocsGroupBox.Controls.SetChildIndex(this.CodeFindBox, 0);
            // 
            // PrevDocsReferenceTextBox
            // 
            this.PrevDocsReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            // 
            // PrevDocsTypeDropEdit
            // 
            this.PrevDocsTypeDropEdit.Visible = false;
            // 
            // PreviousDocumentsGrid
            // 
            this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 138, true);
            // 
            // TopPanel
            // 
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 138, true);
            // 
            // BottomPanel
            // 
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 175, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
            // 
            // CodeFindBox
            // 
            this.CodeFindBox.AllowDrop = true;
            this.CodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CodeFindBox, "FilteredInvoiceLines.PreviousDocuments.CSI_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
            this.CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 19, true);
            this.CodeFindBox.Name = "CodeFindBox";
            this.CodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CodeFindBox.ParentType = null;
            this.CodeFindBox.PreBoundMaxLength = 4;
            this.CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 15, true);
            this.CodeFindBox.TabIndex = 0;
            // 
            // ItemNumberCalcEdit
            // 
            this.ItemNumberCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_ItemNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ItemNumber)));
            this.ItemNumberCalcEdit.CaptionResourceString = null;
            this.ItemNumberCalcEdit.DecimalPlaces = 0;
            this.ItemNumberCalcEdit.Decimals = 0;
            this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 71, true);
            this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
            this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 15, true);
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_UnitOfQuantity)));
            this.UnitOfQuantityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.UnitOfQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 97, true);
            this.UnitOfQuantityDropEdit.Name = "UnitOfQuantityDropEdit";
            this.UnitOfQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 15, true);
            this.UnitOfQuantityDropEdit.TabIndex = 3;
			this.UnitOfQuantityDropEdit.TabStop = true;
			// 
			// QuantityCalcEdit
			// 
			this.QuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.QuantityCalcEdit, "FilteredInvoiceLines.PreviousDocuments.CSI_Quantity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Quantity)));
            this.QuantityCalcEdit.CaptionResourceString = null;
            this.QuantityCalcEdit.DecimalPlaces = 2;
            this.QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 123, true);
            this.QuantityCalcEdit.Name = "QuantityCalcEdit";
            this.QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
            this.QuantityCalcEdit.TabIndex = 4;
            this.QuantityCalcEdit.Text = "0.00";
            this.QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DescriptionTextBox
            // 
            this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FilteredInvoiceLines.PreviousDocuments.CSI_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Description)));
            this.DescriptionTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("99fa7ddb-e209-4ac1-b95e-34b64277e4e4", "Complement");
            this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 149, true);
            this.DescriptionTextBox.Name = "DescriptionTextBox";
            this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 15, true);
            this.DescriptionTextBox.TabIndex = 5;
            // 
            // ExportInvoiceLinePreviousDocumentsUserControl
            // 
            this.Name = "ExportInvoiceLinePreviousDocumentsUserControl";
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
            this.CodeFindBox.ResumeLayout(true);
            this.CodeFindBox.PerformLayout();
            this.UnitOfQuantityDropEdit.ResumeLayout(true);
            this.UnitOfQuantityDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZCodeFindBox CodeFindBox;
		ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit UnitOfQuantityDropEdit;
		ZArchitecture.ZCalcEdit QuantityCalcEdit;
		Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
