using Enterprise.Customs.FR.Business;

using Enterprise.Customs.FR.Business.MasterFiles;

namespace Enterprise.Customs.FR.GUI
{
	partial class OrganisationConsigneePlugInUserControl
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
            this.FRPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.VATProcedureDateLimitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.VATDeferralTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DeltaG1SubProcedureTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.FRPanel.SuspendLayout();
            this.VATProcedureDateLimitDateEdit.SuspendLayout();
            this.VATDeferralTypeDropEdit.SuspendLayout();
            this.DeltaG1SubProcedureTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(FROrgImpAddInfo);
            // 
            // FRPanel
            // 
            this.FRPanel.Controls.Add(this.VATProcedureDateLimitDateEdit);
            this.FRPanel.Controls.Add(this.VATDeferralTypeDropEdit);
            this.FRPanel.Controls.Add(this.DeltaG1SubProcedureTypeDropEdit);
            this.FRPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FRPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FRPanel.Name = "FRPanel";
            this.FRPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.FRPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 160, true);
            this.FRPanel.TabIndex = 3;
            this.FRPanel.Text = "FR";
            // 
            // VATProcedureDateLimitDateEdit
            // 
            this.VATProcedureDateLimitDateEdit.AllowDrop = true;
            this.VATProcedureDateLimitDateEdit.AutoCompleteMonthThreshold = 1;
            this.VATProcedureDateLimitDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.VATProcedureDateLimitDateEdit, "ZO_VATProcedureDateLimit");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((FROrgImpAddInfo)(null)).ZO_VATProcedureDateLimit)));
            this.VATProcedureDateLimitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 19, true);
            this.VATProcedureDateLimitDateEdit.Name = "VATProcedureDateLimitDateEdit";
            this.VATProcedureDateLimitDateEdit.TabIndex = 2;
            // 
            // VATDeferralTypeDropEdit
            // 
            this.VATDeferralTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VATDeferralTypeDropEdit, "ZO_VATDeferType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((FROrgImpAddInfo)(null)).ZO_VATDeferType)));
            this.VATDeferralTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 19, true);
            this.VATDeferralTypeDropEdit.Name = "VATDeferralTypeDropEdit";
            this.VATDeferralTypeDropEdit.PreBoundMaxLength = 1;
            this.VATDeferralTypeDropEdit.ShouldResizeByMaxLength = true;
            this.VATDeferralTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
            this.VATDeferralTypeDropEdit.TabIndex = 1;
            // 
            // DeltaG1SubProcedureTypeDropEdit
            // 
            this.DeltaG1SubProcedureTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeltaG1SubProcedureTypeDropEdit, "ZO_DeltaG1SubProcedure");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((FROrgImpAddInfo)(null)).ZO_DeltaG1SubProcedure)));
            this.DeltaG1SubProcedureTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 45, true);
            this.DeltaG1SubProcedureTypeDropEdit.Name = "DeltaG1SubProcedureTypeDropEdit";
            this.DeltaG1SubProcedureTypeDropEdit.PreBoundMaxLength = 1;
            this.DeltaG1SubProcedureTypeDropEdit.ShouldResizeByMaxLength = true;
            this.DeltaG1SubProcedureTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
            this.DeltaG1SubProcedureTypeDropEdit.TabIndex = 3;
            // 
            // OrganisationConsigneePlugInUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.FRPanel);
            this.Name = "OrganisationConsigneePlugInUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 160, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.FRPanel.ResumeLayout(false);
            this.FRPanel.PerformLayout();
            this.VATProcedureDateLimitDateEdit.ResumeLayout(true);
            this.VATProcedureDateLimitDateEdit.PerformLayout();
            this.VATDeferralTypeDropEdit.ResumeLayout(true);
            this.VATDeferralTypeDropEdit.PerformLayout();
            this.DeltaG1SubProcedureTypeDropEdit.ResumeLayout(true);
            this.DeltaG1SubProcedureTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel FRPanel;
		private ZArchitecture.GUI.ZDropEdit VATDeferralTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit DeltaG1SubProcedureTypeDropEdit;
		private ZArchitecture.GUI.ZDateEdit VATProcedureDateLimitDateEdit;
	}
}
