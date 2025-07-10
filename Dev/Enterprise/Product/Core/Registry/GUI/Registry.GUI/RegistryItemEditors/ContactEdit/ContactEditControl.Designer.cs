using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class ContactEditControl : ZUserControl
	{
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ContactFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox OrganisationFindBox;
		Enterprise.ZArchitecture.ZLabel SelectOrgLabel;
		Enterprise.ZArchitecture.ZLabel ChooseContactLabel;

		void InitializeComponent()
		{
			this.ContactFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OrganisationFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SelectOrgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ChooseContactLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.ZContactBusinessObject);
			// 
			// ContactFindBox
			// 
			this.BindingSource.SetBindingMember(this.ContactFindBox, "SelectedContactPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.GUI.ZContactBusinessObject)(null)).SelectedContactPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.ZContactBusinessObject)(null)).Contacts)));
			this.ContactFindBox.BindToList = "Contacts";
			this.ContactFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			this.ContactFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.ContactFindBox.Name = "ContactFindBox";
			this.ContactFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.ContactFindBox.TabIndex = 1;
			// 
			// OrganisationFindBox
			// 
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "SelectedOrganisationPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.GUI.ZContactBusinessObject)(null)).SelectedOrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.GUI.ZContactBusinessObject)(null)).SelectedOrganisationList)));
			this.OrganisationFindBox.BindToList = "SelectedOrganisationList";
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.OrganisationFindBox.TabIndex = 2;
			// 
			// SelectOrgLabel
			// 
			this.SelectOrgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SelectOrgLabel.Name = "SelectOrgLabel";
			this.SelectOrgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 23, true);
			this.SelectOrgLabel.TabIndex = 3;
			this.SelectOrgLabel.Text = "Select an Organisation from which to choose a contact:";
			// 
			// ChooseContactLabel
			// 
			this.ChooseContactLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.ChooseContactLabel.Name = "ChooseContactLabel";
			this.ChooseContactLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 23, true);
			this.ChooseContactLabel.TabIndex = 3;
			this.ChooseContactLabel.Text = "Choose a contact:";
			// 
			// ContactEditControl
			// 
			this.Controls.Add(this.SelectOrgLabel);
			this.Controls.Add(this.OrganisationFindBox);
			this.Controls.Add(this.ContactFindBox);
			this.Controls.Add(this.ChooseContactLabel);
			this.Name = "ContactEditControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 96, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#region Dispose

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
