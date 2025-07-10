using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class SupplyChainActorUserControl
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
			this.RoleDropEdit = new ZDropEdit();
			this.OwnerOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.ReferenceNumberTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference);
			//
			// RoleDropEdit
			// 
			this.RoleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RoleDropEdit, "CFR_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference)(null)).CFR_Code)));
			this.RoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 59, true);
			this.RoleDropEdit.Name = "RoleDropEdit";
			this.RoleDropEdit.PreBoundMaxLength = 2;
			this.RoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.RoleDropEdit.TabIndex = 0;
			// 
			// ReferenceNumberTextBox
			// 
			this.ReferenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CFR_Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference)(null)).CFR_Reference)));
			this.ReferenceNumberTextBox.CaptionResourceString = null;
			this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 96, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 1;
			// 
			// OwnerOrganisationFindBox
			// 
			this.OwnerOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerOrganisationFindBox, "OwnerOrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusSupplyChainActorReference)(null)).OwnerOrgPK)));
			this.OwnerOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 23, true);
			this.OwnerOrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OwnerOrganisationFindBox.Name = "OwnerOrganisationFindBox";
			this.OwnerOrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OwnerOrganisationFindBox.ParentType = null;
			this.OwnerOrganisationFindBox.ShowDescriptionBox = false;
			this.OwnerOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.OwnerOrganisationFindBox.TabIndex = 2;
			// 
			// SupplyChainActorUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RoleDropEdit);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.OwnerOrganisationFindBox);

			this.Name = "SupplyChainActorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZDropEdit RoleDropEdit;
		internal Enterprise.MasterFiles.GUI.ZOrganisationFindBox OwnerOrganisationFindBox;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
	}
}
