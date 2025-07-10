namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class NctsPreviousDocumentsUserControl
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
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PreviousDocumentsPanel.SuspendLayout();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsClassDropEdit.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PreviousDocumentsSplitter
			// 
			this.PreviousDocumentsSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.PreviousDocumentsSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PreviousDocumentsSplitter.TabIndex = 1;
			// 
			// PrevDocsGroupBox
			// 
			this.PrevDocsGroupBox.Controls.Add(this.LineNoCalcEdit);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsTypeDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsClassDropEdit, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.PrevDocsReferenceTextBox, 0);
			this.PrevDocsGroupBox.Controls.SetChildIndex(this.LineNoCalcEdit, 0);
			// 
			// PrevDocsReferenceTextBox
			// 
			this.PrevDocsReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PrevDocsReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.PrevDocsReferenceTextBox.TabIndex = 2;
			// 
			// PrevDocsClassDropEdit
			// 
			this.PrevDocsClassDropEdit.TabIndex = 1;
			// 
			// PrevDocsTypeDropEdit
			// 
			this.PrevDocsTypeDropEdit.TabIndex = 0;
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.TabIndex = 0;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "PreviousDocuments.CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).PreviousDocuments)).SyncRoot)).CSI_LineNo)));
			this.LineNoCalcEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("1A791C99-623E-4B12-A78B-CD089E7F5B7F", "Line No");
			this.LineNoCalcEdit.DecimalPlaces = 0;
			this.LineNoCalcEdit.Decimals = 0;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 62, true);
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.LineNoCalcEdit.TabIndex = 4;
			this.LineNoCalcEdit.Text = "0";
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NctsPreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "NctsPreviousDocumentsUserControl";
			this.PreviousDocumentsPanel.ResumeLayout(false);
			this.PreviousDocumentsPanel.PerformLayout();
			this.PrevDocsGroupBox.ResumeLayout(false);
			this.PrevDocsGroupBox.PerformLayout();
			this.PrevDocsClassDropEdit.ResumeLayout(true);
			this.PrevDocsClassDropEdit.PerformLayout();
			this.PrevDocsTypeDropEdit.ResumeLayout(true);
			this.PrevDocsTypeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit LineNoCalcEdit;
	}
}
