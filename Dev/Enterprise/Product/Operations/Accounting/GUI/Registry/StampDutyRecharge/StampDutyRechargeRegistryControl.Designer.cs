using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class StampDutyRechargeRegistryControl
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
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.StampDutyRechargeOrganizationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StampDutyRechargeTransactionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.topPanel.SuspendLayout();
			this.StampDutyRechargeOrganizationTypeDropEdit.SuspendLayout();
			this.StampDutyRechargeTransactionTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.StampDutyRecharge);
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.StampDutyRechargeOrganizationTypeDropEdit);
			this.topPanel.Controls.Add(this.StampDutyRechargeTransactionTypeDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 117, true);
			this.topPanel.TabIndex = 3;
			// 
			// StampDutyRechargeOrganizationTypeDropEdit
			// 
			this.StampDutyRechargeOrganizationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StampDutyRechargeOrganizationTypeDropEdit, "StampDutyRechargeOrganizationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.StampDutyRecharge)(null)).StampDutyRechargeOrganizationType)));
			this.StampDutyRechargeOrganizationTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StampDutyRechargeRegistryControl|49175DF0-D9B9-4D6F-A6D0-D2CBBCC53FDE", "Recharge the following AR Organizations Stamp Duty");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.StampDutyRechargeOrganizationTypeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.StampDutyRechargeOrganizationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 28, true);
			this.StampDutyRechargeOrganizationTypeDropEdit.Name = "StampDutyRechargeOrganizationTypeDropEdit";
			this.StampDutyRechargeOrganizationTypeDropEdit.PreBoundMaxLength = 3;
			this.StampDutyRechargeOrganizationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 20, true);
			this.StampDutyRechargeOrganizationTypeDropEdit.TabIndex = 2;
			// 
			// StampDutyRechargeTransactionTypeDropEdit
			// 
			this.StampDutyRechargeTransactionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StampDutyRechargeTransactionTypeDropEdit, "StampDutyRechargeTransactionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Registry.Business.StampDutyRecharge)(null)).StampDutyRechargeTransactionType)));
			this.StampDutyRechargeTransactionTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("StampDutyRechargeRegistryControl|24759C45-2B56-4E16-9109-06D29DEECF63", "Recharge Stamp Duty on the following AR Transaction Types");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.StampDutyRechargeTransactionTypeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.StampDutyRechargeTransactionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 84, true);
			this.StampDutyRechargeTransactionTypeDropEdit.Name = "StampDutyRechargeTransactionTypeDropEdit";
			this.StampDutyRechargeTransactionTypeDropEdit.PreBoundMaxLength = 3;
			this.StampDutyRechargeTransactionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 20, true);
			this.StampDutyRechargeTransactionTypeDropEdit.TabIndex = 3;
			// 
			// StampDutyRechargeRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.topPanel);
			this.Name = "StampDutyRechargeRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 117, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.StampDutyRechargeOrganizationTypeDropEdit.ResumeLayout(true);
			this.StampDutyRechargeOrganizationTypeDropEdit.PerformLayout();
			this.StampDutyRechargeTransactionTypeDropEdit.ResumeLayout(true);
			this.StampDutyRechargeTransactionTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel topPanel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StampDutyRechargeOrganizationTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StampDutyRechargeTransactionTypeDropEdit;
	}
}
