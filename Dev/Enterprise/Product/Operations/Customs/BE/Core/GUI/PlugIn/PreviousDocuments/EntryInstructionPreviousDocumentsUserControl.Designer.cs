namespace Enterprise.Customs.BE.GUI.PlugIn
{
	partial class EntryInstructionPreviousDocumentsUserControl
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
			this.CodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CodeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobDeclaration);
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.Visible = false;
			// 
			// PrevDocsReferenceTextBox
			// 
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
			// 
			// CodeCodeFindBox
			// 
			this.CodeCodeFindBox.AllowDrop = true;
			this.CodeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CodeCodeFindBox, "CustomsEntryInstructions.PreviousDocuments.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).PreviousDocuments)).SyncRoot)).CSI_Code)));
			this.CodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 19, true);
			this.CodeCodeFindBox.Name = "CodeCodeFindBox";
			this.CodeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CodeCodeFindBox.PreBoundMaxLength = 4;
			this.CodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
			this.CodeCodeFindBox.TabIndex = 0;
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("4A6D0C6E-16BF-421F-98EF-99501C172EFA", "Previous Document");
			this.PrevDocsGroupBox.Controls.Add(this.CodeCodeFindBox);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.CodeCodeFindBox, 0);
			// 
			// PreviousDocumentsUserControl
			// 
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
			this.CodeCodeFindBox.ResumeLayout(true);
			this.CodeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CodeCodeFindBox;
	}
}
