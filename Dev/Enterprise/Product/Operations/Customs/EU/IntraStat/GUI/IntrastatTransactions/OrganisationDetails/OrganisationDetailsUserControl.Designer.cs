using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	partial class OrganisationDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.SupplierOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.ConsigneeOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupplierOrganisationControl.SuspendLayout();
			this.ConsigneeOrganisationControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader);
			// 
			// SupplierOrganisationControl
			// 
			this.SupplierOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplierOrganisationControl, "CIH_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_OH_Supplier)));
			this.SupplierOrganisationControl.BindToOrganisations = "Lookups.Suppliers";
			this.SupplierOrganisationControl.Captions = new string[0];
			this.SupplierOrganisationControl.IsCaptionOverridden = false;
			this.SupplierOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.SupplierOrganisationControl.Name = "SupplierOrganisationControl";
			this.SupplierOrganisationControl.OrgAddressFormatter = null;
			this.SupplierOrganisationControl.PopupCaption = "";
			this.SupplierOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.SupplierOrganisationControl.TabIndex = 0;
			// 
			// ConsigneeOrganisationControl
			// 
			this.ConsigneeOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeOrganisationControl, "CIH_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_OH_Consignee)));
			this.ConsigneeOrganisationControl.BindToOrganisations = "Lookups.Consignees";
			this.ConsigneeOrganisationControl.Captions = new string[0];
			this.ConsigneeOrganisationControl.IsCaptionOverridden = false;
			this.ConsigneeOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 255, true);
			this.ConsigneeOrganisationControl.Name = "ConsigneeOrganisationControl";
			this.ConsigneeOrganisationControl.OrgAddressFormatter = null;
			this.ConsigneeOrganisationControl.PopupCaption = "";
			this.ConsigneeOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ConsigneeOrganisationControl.TabIndex = 1;
			// 
			// OrganisationDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupplierOrganisationControl);
			this.Controls.Add(this.ConsigneeOrganisationControl);
			this.Name = "OrganisationDetailsUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 503, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplierOrganisationControl.ResumeLayout(true);
			this.SupplierOrganisationControl.PerformLayout();
			this.ConsigneeOrganisationControl.ResumeLayout(true);
			this.ConsigneeOrganisationControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.MasterFiles.GUI.ZOrganisationControl SupplierOrganisationControl;
		internal Enterprise.MasterFiles.GUI.ZOrganisationControl ConsigneeOrganisationControl;

	}
}
