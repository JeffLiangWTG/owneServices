using Enterprise.Client.EDI.Modules;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class InternalIncidentLicenceSettingsControl : RegistryZUserControl
	{
		public InternalIncidentLicenceSettingsControl()
		{
			InitializeComponent();
			SetControlModuleIDs();
		}

		void SetControlModuleIDs()
		{
			((ZGuidFindBoxColumnStyleInfo)this.InternalEnterpriseCodeGrid.ColumnStyles[0]).ModuleID = ClientModuleRegistration.LicenceEnterprise;
			this.EdiProdLicenceGuidFindBox.ModuleID = ClientModuleRegistration.LicenceHeader;
			this.UatAlpLicenceGuidFindBox.ModuleID = ClientModuleRegistration.LicenceHeader;
			this.UatDprLicenceGuidFindBox.ModuleID = ClientModuleRegistration.LicenceHeader;
			this.UatStdLicenceGuidFindBox.ModuleID = ClientModuleRegistration.LicenceHeader;
			this.UatGpcLicenceGuidFindBox.ModuleID = ClientModuleRegistration.LicenceHeader;
			this.UatGprLicenceGuidFindBox.ModuleID = ClientModuleRegistration.LicenceHeader;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			this.InternalEnterpriseCodeGrid.ReadOnly = readOnly;
			this.EdiProdLicenceGuidFindBox.ReadOnly = readOnly;
			this.UatAlpLicenceGuidFindBox.ReadOnly = readOnly;
			this.UatDprLicenceGuidFindBox.ReadOnly = readOnly;
			this.UatStdLicenceGuidFindBox.ReadOnly = readOnly;
			this.UatGpcLicenceGuidFindBox.ReadOnly = readOnly;
			this.UatGprLicenceGuidFindBox.ReadOnly = readOnly;
		}
	}
}
