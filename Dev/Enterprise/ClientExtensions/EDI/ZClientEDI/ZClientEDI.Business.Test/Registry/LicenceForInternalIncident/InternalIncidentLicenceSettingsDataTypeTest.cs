using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(InternalIncidentLicenceSettingsDataType))]
	class InternalIncidentLicenceSettingsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InternalIncidentLicenceSettingsDataType>
	{
		protected override InternalIncidentLicenceSettingsDataType GetNewDataType()
		{
			return new InternalIncidentLicenceSettingsDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InternalIncidentLicenceSettingsRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();

			return new[] { CreateValidSampleOne(factory) };
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		void GetSampleOneDependancies(BusinessObjectFactory factory, out LicenceHeader licence1, out LicenceHeader licence2, out LicenceHeader licence3, out LicenceEnterprise enterprise1, out LicenceEnterprise enterprise2)
		{
			var l1Pk = new Guid("BC55B4FB-74A0-43a4-A4D0-7AEA04B94F8A");
			var l2Pk = new Guid("D3E820EA-4A1D-461c-8F4E-509175737EFF");
			var l3Pk = new Guid("5A924121-7AA3-4c5b-9F3E-A6788DD02C07");
			var e1Pk = new Guid("90586BDF-1B86-4f92-91D3-FE947B5852F2");
			var e2Pk = new Guid("E1963109-48F9-483d-AC4A-B6A45A70950A");

			try
			{
				enterprise1 = (LicenceEnterprise)factory.New(typeof(LicenceEnterprise), e1Pk);
				enterprise1.LE_EnterpriseCode = "EDI";
				enterprise1.LE_OH = factory.NewWithValidTestData<OrgHeader>().PK;

				var company1 = (LicenceCompany)factory.New(typeof(LicenceCompany), new Guid("9DE4E519-8EF2-4244-B1D9-302740CD4710"));
				company1.LC_CompanyCode = "CO1";
				company1.LC_LE = enterprise1.PK;
				company1.LC_OH = factory.NewWithValidTestData<OrgHeader>().PK;

				licence1 = (LicenceHeader)factory.New(typeof(LicenceHeader), l1Pk);
				licence1.LA_LC = company1.PK;
				licence1.LA_LD = factory.NewWithValidTestData<LicenceDatabase>().PK;

				licence2 = (LicenceHeader)factory.New(typeof(LicenceHeader), l2Pk);
				licence2.LA_LC = company1.PK;
				licence2.LA_LD = factory.NewWithValidTestData<LicenceDatabase>().PK;

				enterprise2 = (LicenceEnterprise)factory.New(typeof(LicenceEnterprise), e2Pk);
				enterprise2.LE_EnterpriseCode = "CAR";
				enterprise2.LE_OH = factory.NewWithValidTestData<OrgHeader>().PK;

				var company2 = (LicenceCompany)factory.New(typeof(LicenceCompany), new Guid("571BF58C-FD55-4ac2-9A7C-95B3084BFF19"));
				company2.LC_CompanyCode = "CO2";
				company2.LC_LE = enterprise2.PK;
				company2.LC_OH = factory.NewWithValidTestData<OrgHeader>().PK;

				licence3 = (LicenceHeader)factory.New(typeof(LicenceHeader), l3Pk);
				licence3.LA_LC = company2.PK;
				licence3.LA_LD = factory.NewWithValidTestData<LicenceDatabase>().PK;

				factory.Save();
			}
			catch (ZSaveException) //Must have already made them
			{
				licence1 = factory.Load<LicenceHeader>(l1Pk);
				licence2 = factory.Load<LicenceHeader>(l2Pk);
				licence3 = factory.Load<LicenceHeader>(l3Pk);
				enterprise1 = factory.Load<LicenceEnterprise>(e1Pk);
				enterprise2 = factory.Load<LicenceEnterprise>(e2Pk);
			}
		}

		ValidSampleAndBinaryValueInDB CreateValidSampleOne(BusinessObjectFactory factory)
		{
			LicenceHeader licence1, licence2, licence3;
			LicenceEnterprise enterprise1, enterprise2;

			GetSampleOneDependancies(factory, out licence1, out licence2, out licence3, out enterprise1, out enterprise2);
			var licenceSettings = new InternalIncidentLicenceSettings(factory)
			{
				EdiProd_LicencePK = licence1.PK,
				UAT_ALP_LicencePK = licence2.PK,
				UAT_DPR_LicencePK = licence3.PK,
				UAT_STD_LicencePK = licence3.PK,
				UAT_GPC_LicencePK = licence1.PK,
				UAT_GPR_LicencePK = licence1.PK
			};

			var internalEnterprise1 = licenceSettings.LicenceEnterpriseKeys.AddNew();
			internalEnterprise1.LE_PK = enterprise1.PK;

			var internalEnterprise2 = licenceSettings.LicenceEnterpriseKeys.AddNew();
			internalEnterprise2.LE_PK = enterprise2.PK;

			#region ByteArrayValue

			var binaryValue = new byte[]
			{
				255, 254, 60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116,
				0, 102, 0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 73, 0, 110, 0, 116, 0, 101, 0, 114, 0, 110, 0, 97, 0, 108, 0, 73, 0, 110, 0, 99, 0, 105, 0, 100, 0, 101, 0, 110, 0, 116, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 83, 0, 101,
				0, 116, 0, 116, 0, 105, 0, 110, 0, 103, 0, 115, 0, 62, 0, 60, 0, 69, 0, 100, 0, 105, 0, 80, 0, 114, 0, 111, 0, 100, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 98, 0, 99, 0, 53, 0, 53, 0, 98, 0, 52, 0, 102,
				0, 98, 0, 45, 0, 55, 0, 52, 0, 97, 0, 48, 0, 45, 0, 52, 0, 51, 0, 97, 0, 52, 0, 45, 0, 97, 0, 52, 0, 100, 0, 48, 0, 45, 0, 55, 0, 97, 0, 101, 0, 97, 0, 48, 0, 52, 0, 98, 0, 57, 0, 52, 0, 102, 0, 56, 0, 97, 0, 60, 0, 47, 0, 69, 0, 100,
				0, 105, 0, 80, 0, 114, 0, 111, 0, 100, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 60, 0, 85, 0, 65, 0, 84, 0, 95, 0, 65, 0, 76, 0, 80, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80,
				0, 75, 0, 62, 0, 100, 0, 51, 0, 101, 0, 56, 0, 50, 0, 48, 0, 101, 0, 97, 0, 45, 0, 52, 0, 97, 0, 49, 0, 100, 0, 45, 0, 52, 0, 54, 0, 49, 0, 99, 0, 45, 0, 56, 0, 102, 0, 52, 0, 101, 0, 45, 0, 53, 0, 48, 0, 57, 0, 49, 0, 55, 0, 53, 0, 55,
				0, 51, 0, 55, 0, 101, 0, 102, 0, 102, 0, 60, 0, 47, 0, 85, 0, 65, 0, 84, 0, 95, 0, 65, 0, 76, 0, 80, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 60, 0, 85, 0, 65, 0, 84, 0, 95, 0, 68, 0, 80, 0, 82,
				0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 53, 0, 97, 0, 57, 0, 50, 0, 52, 0, 49, 0, 50, 0, 49, 0, 45, 0, 55, 0, 97, 0, 97, 0, 51, 0, 45, 0, 52, 0, 99, 0, 53, 0, 98, 0, 45, 0, 57, 0, 102, 0, 51,
				0, 101, 0, 45, 0, 97, 0, 54, 0, 55, 0, 56, 0, 56, 0, 100, 0, 100, 0, 48, 0, 50, 0, 99, 0, 48, 0, 55, 0, 60, 0, 47, 0, 85, 0, 65, 0, 84, 0, 95, 0, 68, 0, 80, 0, 82, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75,
				0, 62, 0, 60, 0, 85, 0, 65, 0, 84, 0, 95, 0, 83, 0, 84, 0, 68, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 53, 0, 97, 0, 57, 0, 50, 0, 52, 0, 49, 0, 50, 0, 49, 0, 45, 0, 55, 0, 97, 0, 97, 0, 51,
				0, 45, 0, 52, 0, 99, 0, 53, 0, 98, 0, 45, 0, 57, 0, 102, 0, 51, 0, 101, 0, 45, 0, 97, 0, 54, 0, 55, 0, 56, 0, 56, 0, 100, 0, 100, 0, 48, 0, 50, 0, 99, 0, 48, 0, 55, 0, 60, 0, 47, 0, 85, 0, 65, 0, 84, 0, 95, 0, 83, 0, 84, 0, 68, 0, 95,
				0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 60, 0, 85, 0, 65, 0, 84, 0, 95, 0, 71, 0, 80, 0, 67, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 98, 0, 99, 0, 53, 0, 53,
				0, 98, 0, 52, 0, 102, 0, 98, 0, 45, 0, 55, 0, 52, 0, 97, 0, 48, 0, 45, 0, 52, 0, 51, 0, 97, 0, 52, 0, 45, 0, 97, 0, 52, 0, 100, 0, 48, 0, 45, 0, 55, 0, 97, 0, 101, 0, 97, 0, 48, 0, 52, 0, 98, 0, 57, 0, 52, 0, 102, 0, 56, 0, 97, 0, 60,
				0, 47, 0, 85, 0, 65, 0, 84, 0, 95, 0, 71, 0, 80, 0, 67, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 60, 0, 85, 0, 65, 0, 84, 0, 95, 0, 71, 0, 80, 0, 82, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110,
				0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 98, 0, 99, 0, 53, 0, 53, 0, 98, 0, 52, 0, 102, 0, 98, 0, 45, 0, 55, 0, 52, 0, 97, 0, 48, 0, 45, 0, 52, 0, 51, 0, 97, 0, 52, 0, 45, 0, 97, 0, 52, 0, 100, 0, 48, 0, 45, 0, 55, 0, 97, 0, 101, 0, 97,
				0, 48, 0, 52, 0, 98, 0, 57, 0, 52, 0, 102, 0, 56, 0, 97, 0, 60, 0, 47, 0, 85, 0, 65, 0, 84, 0, 95, 0, 71, 0, 80, 0, 82, 0, 95, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 80, 0, 75, 0, 62, 0, 60, 0, 76, 0, 105, 0, 99, 0, 101,
				0, 110, 0, 99, 0, 101, 0, 69, 0, 110, 0, 116, 0, 101, 0, 114, 0, 112, 0, 114, 0, 105, 0, 115, 0, 101, 0, 75, 0, 101, 0, 121, 0, 115, 0, 62, 0, 60, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 69, 0, 110, 0, 116, 0, 101, 0, 114, 0, 112, 0, 114,
				0, 105, 0, 115, 0, 101, 0, 75, 0, 101, 0, 121, 0, 62, 0, 60, 0, 76, 0, 69, 0, 95, 0, 80, 0, 75, 0, 62, 0, 57, 0, 48, 0, 53, 0, 56, 0, 54, 0, 98, 0, 100, 0, 102, 0, 45, 0, 49, 0, 98, 0, 56, 0, 54, 0, 45, 0, 52, 0, 102, 0, 57, 0, 50, 0, 45,
				0, 57, 0, 49, 0, 100, 0, 51, 0, 45, 0, 102, 0, 101, 0, 57, 0, 52, 0, 55, 0, 98, 0, 53, 0, 56, 0, 53, 0, 50, 0, 102, 0, 50, 0, 60, 0, 47, 0, 76, 0, 69, 0, 95, 0, 80, 0, 75, 0, 62, 0, 60, 0, 47, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99,
				0, 101, 0, 69, 0, 110, 0, 116, 0, 101, 0, 114, 0, 112, 0, 114, 0, 105, 0, 115, 0, 101, 0, 75, 0, 101, 0, 121, 0, 62, 0, 60, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 69, 0, 110, 0, 116, 0, 101, 0, 114, 0, 112, 0, 114, 0, 105, 0, 115, 0, 101,
				0, 75, 0, 101, 0, 121, 0, 62, 0, 60, 0, 76, 0, 69, 0, 95, 0, 80, 0, 75, 0, 62, 0, 101, 0, 49, 0, 57, 0, 54, 0, 51, 0, 49, 0, 48, 0, 57, 0, 45, 0, 52, 0, 56, 0, 102, 0, 57, 0, 45, 0, 52, 0, 56, 0, 51, 0, 100, 0, 45, 0, 97, 0, 99, 0, 52,
				0, 97, 0, 45, 0, 98, 0, 54, 0, 97, 0, 52, 0, 53, 0, 97, 0, 55, 0, 48, 0, 57, 0, 53, 0, 48, 0, 97, 0, 60, 0, 47, 0, 76, 0, 69, 0, 95, 0, 80, 0, 75, 0, 62, 0, 60, 0, 47, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 69, 0, 110,
				0, 116, 0, 101, 0, 114, 0, 112, 0, 114, 0, 105, 0, 115, 0, 101, 0, 75, 0, 101, 0, 121, 0, 62, 0, 60, 0, 47, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 69, 0, 110, 0, 116, 0, 101, 0, 114, 0, 112, 0, 114, 0, 105, 0, 115, 0, 101, 0, 75, 0, 101,
				0, 121, 0, 115, 0, 62, 0, 60, 0, 47, 0, 73, 0, 110, 0, 116, 0, 101, 0, 114, 0, 110, 0, 97, 0, 108, 0, 73, 0, 110, 0, 99, 0, 105, 0, 100, 0, 101, 0, 110, 0, 116, 0, 76, 0, 105, 0, 99, 0, 101, 0, 110, 0, 99, 0, 101, 0, 83, 0, 101, 0, 116, 0, 116, 0, 105,
				0, 110, 0, 103, 0, 115, 0, 62, 0
			};

			#endregion

			return new ValidSampleAndBinaryValueInDB(licenceSettings, binaryValue);
		}
	}
}
