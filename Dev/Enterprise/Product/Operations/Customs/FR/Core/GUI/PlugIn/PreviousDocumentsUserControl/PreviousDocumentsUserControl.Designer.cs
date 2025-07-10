namespace Enterprise.Customs.FR.GUI.PlugIn
{
	partial class PreviousDocumentsUserControl
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
			this.PrevDocsReferenceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrevDocsReferenceCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Controls.Add(this.PrevDocsReferenceCodeFindBox);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceCodeFindBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceTextBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.JobDeclaration);
			// 
			// PrevDocsReferenceCodeFindBox
			// 
			this.PrevDocsReferenceCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PrevDocsReferenceCodeFindBox, "FilteredInvoiceLines.PreviousDocuments.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.PrevDocsReferenceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 45, true);
			this.PrevDocsReferenceCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PrevDocsReferenceCodeFindBox.Name = "PrevDocsReferenceCodeFindBox";
			this.PrevDocsReferenceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PrevDocsReferenceCodeFindBox.ParentType = null;
			this.PrevDocsReferenceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 19, true);
			this.PrevDocsReferenceCodeFindBox.TabIndex = 2;
			// 
			// PreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "PreviousDocumentsUserControl";
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
			this.PrevDocsReferenceCodeFindBox.ResumeLayout(true);
			this.PrevDocsReferenceCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox PrevDocsReferenceCodeFindBox;
	}
}
