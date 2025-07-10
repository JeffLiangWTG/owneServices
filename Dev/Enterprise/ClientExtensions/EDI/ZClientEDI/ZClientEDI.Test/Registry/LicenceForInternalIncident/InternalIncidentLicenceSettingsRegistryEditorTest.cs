using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(InternalIncidentLicenceSettingsRegistryEditor))]
	public class InternalIncidentLicenceSettingsRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new InternalIncidentLicenceSettingsRegistryItem("");
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new InternalIncidentLicenceSettingsRegistryEditor(new InternalIncidentLicenceSettingsDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(InternalIncidentLicenceSettingsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			InternalIncidentLicenceSettings[] licences = new InternalIncidentLicenceSettings[1];
			licences[0] = new InternalIncidentLicenceSettings(Factory);
			LicenceEnterprise enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "EDI";
			LicenceCompany company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_LE = enterprise1.PK;
			LicenceHeader licence1 = Factory.NewWithValidTestData<LicenceHeader>();
			licence1.LA_LC = company1.PK;
			LicenceCompany company2 = Factory.NewWithValidTestData<LicenceCompany>();
			company2.LC_LE = enterprise1.PK;
			LicenceHeader licence2 = Factory.NewWithValidTestData<LicenceHeader>();
			licence2.LA_LC = company2.PK;
			LicenceEnterprise enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "CAR";
			LicenceCompany company3 = Factory.NewWithValidTestData<LicenceCompany>();
			company3.LC_LE = enterprise2.PK;
			LicenceHeader licence3 = Factory.NewWithValidTestData<LicenceHeader>();
			licence3.LA_LC = company3.PK;
			Factory.Save();
			licences[0].EdiProd_LicencePK = licence1.PK;
			licences[0].UAT_ALP_LicencePK = licence2.PK;
			licences[0].UAT_DPR_LicencePK = licence2.PK;
			licences[0].UAT_STD_LicencePK = licence3.PK;
			licences[0].UAT_GPC_LicencePK = licence3.PK;
			licences[0].UAT_GPR_LicencePK = licence1.PK;
			LicenceEnterpriseKey internalEnterprise1 = licences[0].LicenceEnterpriseKeys.AddNew();
			internalEnterprise1.LE_PK = enterprise1.PK;
			LicenceEnterpriseKey internalEnterprise2 = licences[0].LicenceEnterpriseKeys.AddNew();
			internalEnterprise2.LE_PK = enterprise2.PK;
			return licences;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.TopLeft;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((InternalIncidentLicenceSettingsControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
