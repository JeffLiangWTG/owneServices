using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.AUS.Business.Testing
{
	internal class ClientAUSProductImportRegistryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationIsHookedUp()
		{
			ClientAUSProductImportRegistry duplicateRegistry = Factory.New<ClientAUSProductImportRegistry>();
			AssertEquals(typeof(ClientAUSProductImportRegistryValidation), duplicateRegistry.Validation.GetType());
		}

		public void TestImporterValidation()
		{
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			ClientAUSProductImportRegistry testRegistry = Factory.New<ClientAUSProductImportRegistry>();
			testRegistry.T6_OH_Importer = Guid.Empty;
			Assert("Validation error message expected", testRegistry.T6_OH_ImporterInfo.HasError("The Importer code entered is not valid"));
			testRegistry.T6_OH_Importer = testImporter.PK;
			Assert("Validation should pass", !testRegistry.HasErrors);
		}

		public void TestSupplierValidation()
		{
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSProductImportRegistry testRegistry = Factory.New<ClientAUSProductImportRegistry>();
			testRegistry.T6_OH_Supplier = Guid.Empty;
			Assert("Validation error message expected", testRegistry.T6_OH_SupplierInfo.HasError("The Supplier code entered is not valid"));
			testRegistry.T6_OH_Supplier = testSupplier.PK;
			Assert("Validation should pass", !testRegistry.HasErrors);
		}

		public void TestDuplicateEntryValidation()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			OrgHeader testSupplier2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));
			ClientAUSProductImportRegistry testRegistry = Factory.New<ClientAUSProductImportRegistry>();
			testRegistry.T6_OH_Importer = testImporter.PK;
			testRegistry.T6_OH_Supplier = testSupplier.PK;
			Assert("Validation should pass", !testRegistry.HasErrors);
			Factory.Save();
			ClientAUSProductImportRegistry duplicateRegistry = Factory.New<ClientAUSProductImportRegistry>();
			duplicateRegistry.T6_OH_Importer = testImporter.PK;
			duplicateRegistry.T6_OH_Supplier = testSupplier.PK;
			Assert("Validation error message expected", duplicateRegistry.T6_OH_SupplierInfo.HasError("Product Registry already exists for this Importer & Supplier"));
			duplicateRegistry.T6_OH_Supplier = testSupplier2.PK;
			Assert("Validation should pass again", !duplicateRegistry.HasErrors);
		}

		public void TestAllValidation()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSProductImportRegistry testRegistry = Factory.New<ClientAUSProductImportRegistry>();
			testRegistry.T6_OH_Importer = testImporter.PK;
			testRegistry.T6_OH_Supplier = testSupplier.PK;
			testRegistry.T6_AllProductsFileName = "";
			testRegistry.T6_ClientInvoicingFileName = "";
			testRegistry.T6_ProductUpdateFileName = "";
			testRegistry.T6_BackUpFileName = "";
			testRegistry.T6_DirectoryImportedParts = "";
			testRegistry.T6_DirectoryRejectedParts = "";
			testRegistry.T6_DirectoryToStoreFiles = "";
			Assert("Validation should fail", testRegistry.HasErrors);
			Assert("AllProductsFileName Validation fail", testRegistry.T6_AllProductsFileNameInfo.HasErrors());
			testRegistry.T6_AllProductsFileName = "AllProducts.csv";
			Assert("AllProductsFileName Validation should now pass", !testRegistry.T6_AllProductsFileNameInfo.HasErrors());
			testRegistry.T6_ClientInvoicingFileName = "ClientInv.csv";
			Assert("ClientInvoicingFileName Validation should now pass", !testRegistry.T6_ClientInvoicingFileNameInfo.HasErrors());
			testRegistry.T6_ProductUpdateFileName = "ProductUpdate.csv";
			testRegistry.T6_BackUpFileName = "BackUp.csv";
			testRegistry.T6_DirectoryImportedParts = "C:/ImportedDirectory";
			testRegistry.T6_DirectoryRejectedParts = "C:/RejectedDirectory";
			testRegistry.T6_DirectoryToStoreFiles = "C:/StoreDirectory";
			Assert("All Validations should now pass", !testRegistry.HasErrors);
		}
	}
}
