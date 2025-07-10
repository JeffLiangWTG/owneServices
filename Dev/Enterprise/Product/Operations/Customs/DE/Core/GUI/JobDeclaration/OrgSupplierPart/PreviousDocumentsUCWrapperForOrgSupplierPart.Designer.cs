namespace Enterprise.Customs.DE.GUI
{
	partial class PreviousDocumentsUCWrapperForOrgSupplierPart
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ExportPreviousDocumentsUserControl = new Enterprise.Customs.DE.GUI.ExportInvoiceLinePreviousDocumentsUserControl();
			this.ImportPreviousDocumentsUserControl = new Enterprise.Customs.EU.GUI.PlugIn.PreviousDocumentsUserControl();
			this.PrevDocsGroupBox.SuspendLayout();
			this.PrevDocsTypeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportPreviousDocumentsUserControl.SuspendLayout();
			this.ImportPreviousDocumentsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExportPreviousDocumentsUserControl
			// 
			this.ExportPreviousDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportPreviousDocumentsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)))));
			this.ExportPreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportPreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportPreviousDocumentsUserControl.Name = "ExportPreviousDocumentsUserControl";
			this.ExportPreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 238, true);
			this.ExportPreviousDocumentsUserControl.TabIndex = 7;
			// 
			// ImportPreviousDocumentsUserControl
			// 
			this.ImportPreviousDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportPreviousDocumentsUserControl, ".");
			this.ImportPreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportPreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportPreviousDocumentsUserControl.Name = "ImportPreviousDocumentsUserControl";
			this.ImportPreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 238, true);
			this.ImportPreviousDocumentsUserControl.TabIndex = 8;
			// 
			// PreviousDocumentsUCWrapperForOrgSupplierPart
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ExportPreviousDocumentsUserControl);
			this.Controls.Add(this.ImportPreviousDocumentsUserControl);
			this.Name = "PreviousDocumentsUCWrapperForOrgSupplierPart";
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.ImportPreviousDocumentsUserControl, 0);
			this.Controls.SetChildIndex(this.ExportPreviousDocumentsUserControl, 0);
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
			this.ExportPreviousDocumentsUserControl.ResumeLayout(true);
			this.ExportPreviousDocumentsUserControl.PerformLayout();
			this.ImportPreviousDocumentsUserControl.ResumeLayout(true);
			this.ImportPreviousDocumentsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ExportInvoiceLinePreviousDocumentsUserControl ExportPreviousDocumentsUserControl;
		internal EU.GUI.PlugIn.PreviousDocumentsUserControl ImportPreviousDocumentsUserControl;
	}
}
