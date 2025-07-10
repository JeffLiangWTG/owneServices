
namespace Enterprise.Customs.KR.GUI
{
	partial class InvoiceLineCertificateOfOriginUserControl
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
            this.COOIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.COODeterminationRuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.COOLabelLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.FTATypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.COOIndicatorDropEdit.SuspendLayout();
            this.COODeterminationRuleDropEdit.SuspendLayout();
            this.COOLabelLocationDropEdit.SuspendLayout();
            this.FTATypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceLine);
            // 
            // COOIndicatorDropEdit
            // 
            this.COOIndicatorDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.COOIndicatorDropEdit, "CertificateOfOriginIssueStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).CertificateOfOriginIssueStatus)));
            this.COOIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 10, true);
            this.COOIndicatorDropEdit.Name = "COOIndicatorDropEdit";
            this.COOIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
            this.COOIndicatorDropEdit.TabIndex = 9;
            // 
            // COODeterminationRuleDropEdit
            // 
            this.COODeterminationRuleDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.COODeterminationRuleDropEdit, "CriteriaForDeterminingCountryOfOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).CriteriaForDeterminingCountryOfOrigin)));
            this.COODeterminationRuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 62, true);
            this.COODeterminationRuleDropEdit.Name = "COODeterminationRuleDropEdit";
            this.COODeterminationRuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
            this.COODeterminationRuleDropEdit.TabIndex = 10;
            // 
            // COOLabelLocationDropEdit
            // 
            this.COOLabelLocationDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.COOLabelLocationDropEdit, "JI_COOLabelLocation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_COOLabelLocation)));
            this.COOLabelLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 116, true);
            this.COOLabelLocationDropEdit.Name = "COOLabelLocationDropEdit";
            this.COOLabelLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
            this.COOLabelLocationDropEdit.TabIndex = 11;
            // 
            // FTATypeDropEdit
            // 
            this.FTATypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.FTATypeDropEdit, "JI_PrimaryPreference");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(null)).JI_PrimaryPreference)));
            this.FTATypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 158, true);
            this.FTATypeDropEdit.Name = "FTATypeDropEdit";
            this.FTATypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
            this.FTATypeDropEdit.TabIndex = 0;
            // 
            // InvoiceLineCertificateOfOriginUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.FTATypeDropEdit);
            this.Controls.Add(this.COOLabelLocationDropEdit);
            this.Controls.Add(this.COODeterminationRuleDropEdit);
            this.Controls.Add(this.COOIndicatorDropEdit);
            this.Name = "InvoiceLineCertificateOfOriginUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 190, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.COOIndicatorDropEdit.ResumeLayout(true);
            this.COOIndicatorDropEdit.PerformLayout();
            this.COODeterminationRuleDropEdit.ResumeLayout(true);
            this.COODeterminationRuleDropEdit.PerformLayout();
            this.COOLabelLocationDropEdit.ResumeLayout(true);
            this.COOLabelLocationDropEdit.PerformLayout();
            this.FTATypeDropEdit.ResumeLayout(true);
            this.FTATypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit COOIndicatorDropEdit;
		public ZArchitecture.GUI.ZDropEdit COODeterminationRuleDropEdit;
		public ZArchitecture.GUI.ZDropEdit COOLabelLocationDropEdit;
		public ZArchitecture.GUI.ZDropEdit FTATypeDropEdit;
	}
}
