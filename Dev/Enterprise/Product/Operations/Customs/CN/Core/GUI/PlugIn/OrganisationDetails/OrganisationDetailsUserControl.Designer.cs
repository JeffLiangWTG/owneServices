namespace Enterprise.Customs.CN.GUI
{
	partial class OrganisationDetailsUserControl
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
			this.ZO_MessageSubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZO_IsAssuredInspectClearanceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ZO_IsConsolidatedDutyCollectionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ZO_IntelligentDeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ZO_MessageSubTypeDropEdit.SuspendLayout();
			this.ZO_IntelligentDeclarationTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CNOrgImpAddInfo);
			// 
			// ZO_MessageSubTypeDropEdit
			// 
			this.ZO_MessageSubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZO_MessageSubTypeDropEdit, "ZO_MessageSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CNOrgImpAddInfo)(null)).ZO_MessageSubType)));
			this.ZO_MessageSubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 8, true);
			this.ZO_MessageSubTypeDropEdit.Name = "ZO_MessageSubTypeDropEdit";
			this.ZO_MessageSubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ZO_MessageSubTypeDropEdit.TabIndex = 0;
			// 
			// ZO_IsAssuredInspectClearanceCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ZO_IsAssuredInspectClearanceCheckBox, "ZO_IsAssuredInspectClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.CN.Business.CNOrgImpAddInfo)(null)).ZO_IsAssuredInspectClearance)));
			this.ZO_IsAssuredInspectClearanceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 62, true);
			this.ZO_IsAssuredInspectClearanceCheckBox.Name = "ZO_IsAssuredInspectClearanceCheckBox";
			this.ZO_IsAssuredInspectClearanceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 24, true);
			this.ZO_IsAssuredInspectClearanceCheckBox.TabIndex = 2;
			// 
			// ZO_IsConsolidatedDutyCollectionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ZO_IsConsolidatedDutyCollectionCheckBox, "ZO_IsConsolidatedDutyCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.CN.Business.CNOrgImpAddInfo)(null)).ZO_IsConsolidatedDutyCollection)));
			this.ZO_IsConsolidatedDutyCollectionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 93, true);
			this.ZO_IsConsolidatedDutyCollectionCheckBox.Name = "ZO_IsConsolidatedDutyCollectionCheckBox";
			this.ZO_IsConsolidatedDutyCollectionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 24, true);
			this.ZO_IsConsolidatedDutyCollectionCheckBox.TabIndex = 3;
			// 
			// ZO_IntelligentDeclarationTypeDropEdit
			// 
			this.ZO_IntelligentDeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZO_IntelligentDeclarationTypeDropEdit, "ZO_IntelligentDeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.CNOrgImpAddInfo)(null)).ZO_IntelligentDeclarationType)));
			this.ZO_IntelligentDeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 35, true);
			this.ZO_IntelligentDeclarationTypeDropEdit.Name = "ZO_IntelligentDeclarationTypeDropEdit";
			this.ZO_IntelligentDeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ZO_IntelligentDeclarationTypeDropEdit.TabIndex = 1;
			// 
			// OrganisationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ZO_IntelligentDeclarationTypeDropEdit);
			this.Controls.Add(this.ZO_IsConsolidatedDutyCollectionCheckBox);
			this.Controls.Add(this.ZO_MessageSubTypeDropEdit);
			this.Controls.Add(this.ZO_IsAssuredInspectClearanceCheckBox);
			this.Name = "OrganisationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 148, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ZO_MessageSubTypeDropEdit.ResumeLayout(true);
			this.ZO_MessageSubTypeDropEdit.PerformLayout();
			this.ZO_IntelligentDeclarationTypeDropEdit.ResumeLayout(true);
			this.ZO_IntelligentDeclarationTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion


		private Enterprise.ZArchitecture.GUI.ZDropEdit ZO_MessageSubTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ZO_IsAssuredInspectClearanceCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ZO_IsConsolidatedDutyCollectionCheckBox;
		private ZArchitecture.GUI.ZDropEdit ZO_IntelligentDeclarationTypeDropEdit;
	}
}
