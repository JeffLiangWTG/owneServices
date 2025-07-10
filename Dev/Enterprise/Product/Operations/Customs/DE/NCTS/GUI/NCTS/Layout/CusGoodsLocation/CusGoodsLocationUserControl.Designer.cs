namespace Enterprise.Customs.DE.NCTS.GUI
{
	public partial class CusGoodsLocationUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.AdditionalIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorizationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganisationFindBox.SuspendLayout();
			this.AdditionalIdentifierDropEdit.SuspendLayout();
			this.AuthorizationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.CusGoodsLocation);
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "AddressIdentificationHolderPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.NCTS.Business.CusGoodsLocation)(null)).AddressIdentificationHolderPK)));
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 21, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OrganisationFindBox.ParentType = null;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 15, true);
			this.OrganisationFindBox.TabIndex = 8;
			// 
			// AdditionalIdentifierDropEdit
			// 
			this.AdditionalIdentifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalIdentifierDropEdit, "CGL_AdditionalIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.NCTS.Business.CusGoodsLocation)(null)).CGL_AdditionalIdentifier)));
			this.AdditionalIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 51, true);
			this.AdditionalIdentifierDropEdit.Name = "AdditionalIdentifierDropEdit";
			this.AdditionalIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 15, true);
			this.AdditionalIdentifierDropEdit.TabIndex = 14;
			// 
			// AuthorizationCodeFindBox
			// 
			this.AuthorizationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationCodeFindBox, "AddressAuthorisationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.NCTS.Business.CusGoodsLocation)(null)).AddressAuthorisationNumber)));
			this.AuthorizationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 82, true);
			this.AuthorizationCodeFindBox.Name = "AuthorizationCodeFindBox";
			this.AuthorizationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AuthorizationCodeFindBox.ParentType = null;
			this.AuthorizationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 15, true);
			this.AuthorizationCodeFindBox.TabIndex = 15;
			// 
			// CusGoodsLocationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AuthorizationCodeFindBox);
			this.Controls.Add(this.AdditionalIdentifierDropEdit);
			this.Controls.Add(this.OrganisationFindBox);
			this.Name = "CusGoodsLocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.AdditionalIdentifierDropEdit.ResumeLayout(true);
			this.AdditionalIdentifierDropEdit.PerformLayout();
			this.AuthorizationCodeFindBox.ResumeLayout(true);
			this.AuthorizationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal Enterprise.MasterFiles.GUI.ZOrganisationFindBox OrganisationFindBox;
		internal ZArchitecture.GUI.ZDropEdit AdditionalIdentifierDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox AuthorizationCodeFindBox;
	}
}
