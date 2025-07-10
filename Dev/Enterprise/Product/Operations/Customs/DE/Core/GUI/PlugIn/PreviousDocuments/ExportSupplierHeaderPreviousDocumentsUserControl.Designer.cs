namespace Enterprise.Customs.DE.GUI
{
	partial class ExportSupplierHeaderPreviousDocumentsUserControl
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
            this.CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PrevDocsGroupBox.SuspendLayout();
            this.PrevDocsTypeDropEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
            this.PreviousDocumentsGrid.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PrevDocsGroupBox
            // 
            this.PrevDocsGroupBox.Controls.Add(this.CodeFindBox);
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
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
            // 
            // CodeFindBox
            // 
            this.CodeFindBox.AllowDrop = true;
            this.CodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CodeFindBox, "Invoices.PreviousDocuments.CSI_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
            this.CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 19, true);
            this.CodeFindBox.Name = "CodeFindBox";
            this.CodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.CodeFindBox.ParentType = null;
            this.CodeFindBox.PreBoundMaxLength = 4;
            this.CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 15, true);
            this.CodeFindBox.TabIndex = 0;
            // 
            // ExportSupplierHeaderPreviousDocumentsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "ExportSupplierHeaderPreviousDocumentsUserControl";
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
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZCodeFindBox CodeFindBox;
	}
}
