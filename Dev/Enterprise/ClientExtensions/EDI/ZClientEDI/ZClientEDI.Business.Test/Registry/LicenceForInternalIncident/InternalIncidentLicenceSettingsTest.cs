using System;
using CargoWise.ComponentModel;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(InternalIncidentLicenceSettings))]
	internal class InternalIncidentLicenceSettingsTest : RegistryBusinessObjectTemplateTestCase<InternalIncidentLicenceSettings>
	{
		#region Properties

		public void TestEdiProd_LicencePK()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();

			licenceSettings.EdiProd_LicencePK = licence.PK;
			AssertEquals(licence.PK, licenceSettings.EdiProd_LicencePK);
			AssertEquals(licence, licenceSettings.EdiProd_Licence);
		}

		public void TestUAT_ALP_LicencePK()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();

			licenceSettings.UAT_ALP_LicencePK = licence.PK;
			AssertEquals(licence.PK, licenceSettings.UAT_ALP_LicencePK);
			AssertEquals(licence, licenceSettings.UAT_ALP_Licence);
		}

		public void TestUAT_DPR_LicencePK()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();

			licenceSettings.UAT_DPR_LicencePK = licence.PK;
			AssertEquals(licence.PK, licenceSettings.UAT_DPR_LicencePK);
			AssertEquals(licence, licenceSettings.UAT_DPR_Licence);
		}

		public void TestUAT_STD_LicencePK()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();

			licenceSettings.UAT_STD_LicencePK = licence.PK;
			AssertEquals(licence.PK, licenceSettings.UAT_STD_LicencePK);
			AssertEquals(licence, licenceSettings.UAT_STD_Licence);
		}

		public void TestUAT_GPC_LicencePK()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();

			licenceSettings.UAT_GPC_LicencePK = licence.PK;
			AssertEquals(licence.PK, licenceSettings.UAT_GPC_LicencePK);
			AssertEquals(licence, licenceSettings.UAT_GPC_Licence);
		}

		public void TestUAT_GPR_LicencePK()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			LicenceHeader licence = Factory.NewWithValidTestData<LicenceHeader>();

			licenceSettings.UAT_GPR_LicencePK = licence.PK;
			AssertEquals(licence.PK, licenceSettings.UAT_GPR_LicencePK);
			AssertEquals(licence, licenceSettings.UAT_GPR_Licence);
		}

		public void TestInternalEnterprises()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			AssertNotNull(licenceSettings.LicenceEnterpriseKeys);
			AssertEquals(0, licenceSettings.LicenceEnterpriseKeys.Count);
			licenceSettings.LicenceEnterpriseKeys.AddNew();
			AssertEquals(1, licenceSettings.LicenceEnterpriseKeys.Count);
		}

		#endregion

		#region Lookups

		public void TestLookups()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();

			AssertNotNull(licenceSettings.Lookups);
			AssertEquals(typeof(InternalIncidentLicenceSettingsLookups), licenceSettings.Lookups.GetType());
		}

		#endregion

		#region Clone

		public void TestGetClone()
		{
			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();
			LicenceEnterpriseKey internalEnterprise = licenceSettings.LicenceEnterpriseKeys.AddNew();
			internalEnterprise.LE_PK = Factory.NewWithValidTestData<LicenceEnterprise>().PK;

			InternalIncidentLicenceSettings clone = (InternalIncidentLicenceSettings)licenceSettings.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals(1, clone.LicenceEnterpriseKeys.Count);
			AssertEquals(internalEnterprise.LE_PK, clone.LicenceEnterpriseKeys[0].LE_PK);
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			#region Test Data

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

			#endregion

			InternalIncidentLicenceSettings licenceSettings = NewPopulatedBusinessObject();

			licenceSettings.RunPreSaveValidation();
			Assert("Should has error: mandatory check", licenceSettings.EdiProd_LicencePKInfo.HasErrors());
			Assert("Should has error: mandatory check", licenceSettings.UAT_ALP_LicencePKInfo.HasErrors());
			Assert("Should has error: mandatory check", licenceSettings.UAT_DPR_LicencePKInfo.HasErrors());
			Assert("Should has error: mandatory check", licenceSettings.UAT_STD_LicencePKInfo.HasErrors());
			Assert("Should has error: mandatory check", licenceSettings.UAT_GPC_LicencePKInfo.HasErrors());
			Assert("Should has error: mandatory check", licenceSettings.UAT_GPR_LicencePKInfo.HasErrors());

			licenceSettings.EdiProd_LicencePK = licence1.PK;
			licenceSettings.UAT_ALP_LicencePK = licence1.PK;
			licenceSettings.UAT_DPR_LicencePK = licence2.PK;
			licenceSettings.UAT_STD_LicencePK = licence2.PK;
			licenceSettings.UAT_GPC_LicencePK = licence3.PK;
			licenceSettings.UAT_GPR_LicencePK = licence3.PK;

			licenceSettings.RunPreSaveValidation();
			Assert("Should has error: no valid enterpise code", licenceSettings.EdiProd_LicencePKInfo.HasErrors());
			Assert("Should has error: no valid enterpise code", licenceSettings.UAT_ALP_LicencePKInfo.HasErrors());
			Assert("Should has error: no valid enterpise code", licenceSettings.UAT_DPR_LicencePKInfo.HasErrors());
			Assert("Should has error: no valid enterpise code", licenceSettings.UAT_STD_LicencePKInfo.HasErrors());
			Assert("Should has error: no valid enterpise code", licenceSettings.UAT_GPC_LicencePKInfo.HasErrors());
			Assert("Should has error: no valid enterpise code", licenceSettings.UAT_GPR_LicencePKInfo.HasErrors());

			LicenceEnterpriseKey internalEnterpise1 = licenceSettings.LicenceEnterpriseKeys.AddNew();
			internalEnterpise1.LE_PK = enterprise1.PK;
			licenceSettings.RunPreSaveValidation();
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.EdiProd_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.UAT_ALP_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.UAT_DPR_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.UAT_STD_LicencePKInfo.HasErrors());
			Assert("Should has error: invalid enterpise code CAR", licenceSettings.UAT_GPC_LicencePKInfo.HasErrors());
			Assert("Should has error: invalid enterpise code CAR", licenceSettings.UAT_GPR_LicencePKInfo.HasErrors());

			LicenceEnterpriseKey internalEnterpise2 = licenceSettings.LicenceEnterpriseKeys.AddNew();
			internalEnterpise2.LE_PK = enterprise2.PK;
			licenceSettings.RunPreSaveValidation();
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.EdiProd_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.UAT_ALP_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.UAT_DPR_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is EDI", !licenceSettings.UAT_STD_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is CAR", !licenceSettings.UAT_GPC_LicencePKInfo.HasErrors());
			Assert("Should has no error: enterpise code is CAR", !licenceSettings.UAT_GPR_LicencePKInfo.HasErrors());
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override InternalIncidentLicenceSettings GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override InternalIncidentLicenceSettings GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		InternalIncidentLicenceSettings NewPopulatedBusinessObject()
		{
			InternalIncidentLicenceSettings result = new InternalIncidentLicenceSettings(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		#endregion
	}
}
