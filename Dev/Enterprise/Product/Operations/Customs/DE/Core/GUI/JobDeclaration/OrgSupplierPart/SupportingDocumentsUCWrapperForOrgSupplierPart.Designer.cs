using Enterprise.Customs.DE.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	partial class SupportingDocumentsUCWrapperForOrgSupplierPart
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ExportSupportingDocumentsUserControl = new Enterprise.Customs.DE.GUI.PlugIn.ExportInvoiceLineSupportingDocumentsUserControl();
			this.ImportSupportingDocumentsUserControl = new Enterprise.Customs.DE.GUI.ImportInvoiceLineSupportingDocumentsUserControl();
			this.SupportingDocumentsFieldsControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportSupportingDocumentsUserControl.SuspendLayout();
			this.ImportSupportingDocumentsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SupportingDocumentsSplitter
			// 
			this.SupportingDocumentsSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SupportingDocumentsSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// gridSplitter
			// 
			this.gridSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.gridSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// ExportSupportingDocumentsUserControl
			// 
			this.ExportSupportingDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportSupportingDocumentsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)))));
			this.ExportSupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportSupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportSupportingDocumentsUserControl.Name = "ExportSupportingDocumentsUserControl";
			this.ExportSupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 379, true);
			this.ExportSupportingDocumentsUserControl.TabIndex = 3;
			// 
			// ImportSupportingDocumentsUserControl
			// 
			this.ImportSupportingDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportSupportingDocumentsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)))));
			this.ImportSupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportSupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportSupportingDocumentsUserControl.Name = "ImportSupportingDocumentsUserControl";
			this.ImportSupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 379, true);
			this.ImportSupportingDocumentsUserControl.TabIndex = 4;
			// 
			// SupportingDocumentsUCWrapperForOrgSupplierPart
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ExportSupportingDocumentsUserControl);
			this.Controls.Add(this.ImportSupportingDocumentsUserControl);
			this.Name = "SupportingDocumentsUCWrapperForOrgSupplierPart";
			this.Controls.SetChildIndex(this.ImportSupportingDocumentsUserControl, 0);
			this.Controls.SetChildIndex(this.ExportSupportingDocumentsUserControl, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.gridSplitter, 0);
			this.Controls.SetChildIndex(this.SupportingDocumentsGrid, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.SupportingDocumentsFieldsControl.ResumeLayout(true);
			this.SupportingDocumentsFieldsControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportSupportingDocumentsUserControl.ResumeLayout(true);
			this.ExportSupportingDocumentsUserControl.PerformLayout();
			this.ImportSupportingDocumentsUserControl.ResumeLayout(true);
			this.ImportSupportingDocumentsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ExportInvoiceLineSupportingDocumentsUserControl ExportSupportingDocumentsUserControl;
		internal ImportInvoiceLineSupportingDocumentsUserControl ImportSupportingDocumentsUserControl;
	}
}
