using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business.Testing
{
	internal class ClientAUSOriginPreferenceMappingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationIsHookedUp()
		{
			ClientAUSOriginPreferenceMapping duplicateMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			AssertEquals(typeof(ClientAUSOriginPreferenceMappingValidation), duplicateMapping.Validation.GetType());
		}

		public void TestOriginValidation()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ZString testOrigin = "FJ";
			ClientAUSOriginPreferenceMapping testMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			testMapping.T7_OH_Importer = testImporter.PK;
			testMapping.T7_OH_Supplier = testSupplier.PK;
			testMapping.T7_RN_NKOrigin = testOrigin;
			Assert("Validation should pass", !testMapping.HasErrors);
			Factory.Save();
			ClientAUSOriginPreferenceMapping duplicateMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			duplicateMapping.T7_OH_Importer = testImporter.PK;
			duplicateMapping.T7_OH_Supplier = testSupplier.PK;
			duplicateMapping.T7_RN_NKOrigin = testOrigin;
			Assert("Validation error message expected", duplicateMapping.T7_RN_NKOriginInfo.HasError("Mapping already exists for this Importer, Supplier & Origin combination"));
			duplicateMapping.T7_RN_NKOrigin = "JP";
			Assert("Validation should pass again", !duplicateMapping.T7_RN_NKOriginInfo.HasErrors());
		}

		public void TestImporterValidation()
		{
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			ClientAUSOriginPreferenceMapping testMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			testMapping.T7_OH_Importer = Guid.Empty;
			Assert("Validation error message expected", testMapping.T7_OH_ImporterInfo.HasError("The Importer code entered is not valid"));
			testMapping.T7_OH_Importer = testImporter.PK;
			Assert("Validation should pass", !testMapping.HasErrors);
		}

		public void TestSupplierValidation()
		{
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSOriginPreferenceMapping testMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			testMapping.T7_OH_Supplier = Guid.Empty;
			Assert("Validation error message expected", testMapping.T7_OH_SupplierInfo.HasError("The Supplier code entered is not valid"));
			testMapping.T7_OH_Supplier = testSupplier.PK;
			Assert("Validation should pass", !testMapping.HasErrors);
		}
	}
}
