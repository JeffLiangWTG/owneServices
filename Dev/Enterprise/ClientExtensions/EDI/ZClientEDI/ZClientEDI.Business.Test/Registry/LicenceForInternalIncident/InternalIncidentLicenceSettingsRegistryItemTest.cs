using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(InternalIncidentLicenceSettingsRegistryItem))]
	class InternalIncidentLicenceSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<InternalIncidentLicenceSettings>
	{
		public void TestConstructor()
		{
			InternalIncidentLicenceSettingsRegistryItem item = (InternalIncidentLicenceSettingsRegistryItem)GetNewRegistryItem();
			AssertEquals("InternalIncidentLicenceSettings", item.Name);
			AssertEquals("CustomerIncident", item.Category);
			AssertEquals("Internal Incident License Settings", item.Caption);
			AssertEquals("Please specify the licenses for reporting internal incidents on different releases. The enterprise code list restricts the license codes to be chosen for internal incidents.", item.Hint);
			AssertEquals(typeof(InternalIncidentLicenceSettingsDataType), item.DataType.GetType());
			AssertEquals(RegistryStorageFlags.System, item.Storage);
		}

		protected override StronglyTypedRegistryItem<InternalIncidentLicenceSettings, InternalIncidentLicenceSettings> GetNewRegistryItem()
		{
			return new InternalIncidentLicenceSettingsRegistryItem("CustomerIncident");
		}

		protected override InternalIncidentLicenceSettings ValidValue
		{
			get
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();

				LicenceEnterprise enterprise1 = factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise1.LE_EnterpriseCode = "EDI";

				LicenceCompany company1 = factory.NewWithValidTestData<LicenceCompany>();
				company1.LC_LE = enterprise1.PK;
				LicenceHeader licence1 = factory.NewWithValidTestData<LicenceHeader>();
				licence1.LA_LC = company1.PK;
				LicenceCompany company2 = factory.NewWithValidTestData<LicenceCompany>();
				company2.LC_LE = enterprise1.PK;
				LicenceHeader licence2 = factory.NewWithValidTestData<LicenceHeader>();
				licence2.LA_LC = company2.PK;

				LicenceEnterprise enterprise2 = factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise2.LE_EnterpriseCode = "CAR";

				LicenceCompany company3 = factory.NewWithValidTestData<LicenceCompany>();
				company3.LC_LE = enterprise2.PK;
				LicenceHeader licence3 = factory.NewWithValidTestData<LicenceHeader>();
				licence3.LA_LC = company3.PK;

				factory.Save();

				InternalIncidentLicenceSettings licenceSettings = new InternalIncidentLicenceSettings();
				LicenceEnterpriseKey internalEnprise1 = licenceSettings.LicenceEnterpriseKeys.AddNew();
				internalEnprise1.LE_PK = enterprise1.PK;
				LicenceEnterpriseKey internalEnprise2 = licenceSettings.LicenceEnterpriseKeys.AddNew();
				internalEnprise2.LE_PK = enterprise2.PK;
				licenceSettings.EdiProd_LicencePK = licence1.PK;
				licenceSettings.UAT_ALP_LicencePK = licence2.PK;
				licenceSettings.UAT_DPR_LicencePK = licence2.PK;
				licenceSettings.UAT_STD_LicencePK = licence3.PK;
				licenceSettings.UAT_GPC_LicencePK = licence3.PK;
				licenceSettings.UAT_GPR_LicencePK = licence1.PK;

				return licenceSettings;
			}
		}
	}
}
