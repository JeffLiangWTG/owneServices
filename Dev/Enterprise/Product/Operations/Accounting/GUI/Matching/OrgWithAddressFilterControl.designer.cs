
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Accounting.GUI
{
	partial class OrgWithAddressFilterControl
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
			this.AddressEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Filters.OrgWithAddressFilter);
			// 
			// AddressEdit
			// 
			this.AddressEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressEdit, "Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Base.Filters.OrgWithAddressFilter)(null)).Address)));
			this.AddressEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6b031e2e-991b-4952-8129-85bc71628298", "Address");
			this.AddressEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 29, true);
			this.AddressEdit.Name = "AddressEdit";
			this.AddressEdit.PreBoundMaxLength = 8;
			this.AddressEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.AddressEdit.TabIndex = 1;
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "Organization");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Filters.OrgWithAddressFilter)(null)).Organization)));
			this.OrganisationFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fb14919c-c441-4e5b-86df-9b9b41186cfd", "Org.");
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 3, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.OrganisationFindBox.TabIndex = 0;
			// 
			// OrgWithAddressFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrganisationFindBox);
			this.Controls.Add(this.AddressEdit);
			this.Name = "OrgWithAddressFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidDropEdit AddressEdit;
		private ZOrganisationFindBox OrganisationFindBox;

	}
}

