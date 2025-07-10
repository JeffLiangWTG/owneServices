namespace Enterprise.Customs.FR.GUI
{
	partial class ImportOrganizationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BuyerOrganisationGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SellerAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			this.DefermentPartyDocAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BuyerOrganisationGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 68, true);
			this.SellerAddressControl.TabIndex = 3;
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 90, true);
			this.ManufacturerAddressControl.TabIndex = 4;
			// 
			// DefermentPartyDocAddressControl
			// 
			this.DefermentPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 112, true);
			this.DefermentPartyDocAddressControl.TabIndex = 5;
			// 
			// BuyerOrganisationGuidFindBox
			// 
			this.BuyerOrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyerOrganisationGuidFindBox, "JE_OH_Buyer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_OH_Buyer)));
			this.BuyerOrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 46, true);
			this.BuyerOrganisationGuidFindBox.Name = "BuyerOrganisationGuidFindBox";
			this.BuyerOrganisationGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BuyerOrganisationGuidFindBox.ParentType = null;
			this.BuyerOrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 20, true);
			this.BuyerOrganisationGuidFindBox.TabIndex = 2;
			// 
			// ImportOrganizationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BuyerOrganisationGuidFindBox);
			this.Name = "ImportOrganizationUserControl";
			this.Controls.SetChildIndex(this.BuyerOrganisationGuidFindBox, 0);
			this.Controls.SetChildIndex(this.DefermentPartyDocAddressControl, 0);
			this.Controls.SetChildIndex(this.ManufacturerAddressControl, 0);
			this.Controls.SetChildIndex(this.SellerAddressControl, 0);
			this.Controls.SetChildIndex(this.RepresentativeAddressControl, 0);
			this.Controls.SetChildIndex(this.DeclarantOfficeAddressControl, 0);
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			this.DefermentPartyDocAddressControl.ResumeLayout(true);
			this.DefermentPartyDocAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BuyerOrganisationGuidFindBox.ResumeLayout(true);
			this.BuyerOrganisationGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZGuidFindBox BuyerOrganisationGuidFindBox;
	}
}
