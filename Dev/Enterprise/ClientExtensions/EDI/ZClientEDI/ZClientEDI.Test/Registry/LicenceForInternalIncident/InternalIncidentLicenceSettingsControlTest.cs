using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI
{
	[TestedType(typeof(InternalIncidentLicenceSettingsControl))]
	class InternalIncidentLicenceSettingsControlTest : Enterprise.Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new InternalIncidentLicenceSettings();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			InternalIncidentLicenceSettingsControl licenceSettingsControl = (InternalIncidentLicenceSettingsControl)control;
			return licenceSettingsControl.InternalEnterpriseCodeGrid.ReadOnly && licenceSettingsControl.EdiProdLicenceGuidFindBox.ReadOnly && licenceSettingsControl.UatAlpLicenceGuidFindBox.ReadOnly && licenceSettingsControl.UatDprLicenceGuidFindBox.ReadOnly && licenceSettingsControl.UatStdLicenceGuidFindBox.ReadOnly && licenceSettingsControl.UatGpcLicenceGuidFindBox.ReadOnly && licenceSettingsControl.UatGprLicenceGuidFindBox.ReadOnly;
		}
	}
}
