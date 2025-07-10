using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Business.Testing
{
	[TestedType(typeof(ClientAUSProductImportRegistry))]
	public class ClientAUSProductImportRegistryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestImporterSupplierRegistration()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSProductImportRegistry testRegistration = Factory.LoadTop1<ClientAUSProductImportRegistry>(new ZQuery());
			AssertEquals("Importer/Supplier should not be registered", true, (testRegistration == null));
			ClientAUSProductImportRegistry testRegistryData = Factory.NewWithValidTestData<ClientAUSProductImportRegistry>();
			testRegistryData.T6_OH_Importer = testImporter.PK;
			testRegistryData.T6_OH_Supplier = testSupplier.PK;
			testRegistryData.T6_Email = "test@test.com";
			Factory.Save();
			testRegistration = Factory.LoadTop1<ClientAUSProductImportRegistry>(new ZQuery());
			AssertEquals("Importer/Supplier should not be registered", true, (testRegistration != null));
			AssertEquals("test@test.com", testRegistration.UpdateEmailAddress);
		}

		public void TestTableProperties()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSProductImportRegistry testRegistration = Factory.LoadTop1<ClientAUSProductImportRegistry>(new ZQuery());
			AssertEquals("Importer/Supplier should not be registered", true, (testRegistration == null));
			ClientAUSProductImportRegistry testData = Factory.NewWithValidTestData<ClientAUSProductImportRegistry>();
			testData.T6_OH_Importer = testImporter.PK;
			testData.T6_OH_Supplier = testSupplier.PK;
			testData.T6_AllProductsFileName = "AllProducts";
			testData.T6_BackUpFileName = "BUp";
			testData.T6_ClientInvoicingFileName = "ClientInvoicing";
			testData.T6_DirectoryImportedParts = @"Directory\Imported";
			testData.T6_DirectoryRejectedParts = @"Directory\Rejected";
			testData.T6_DirectoryToStoreFiles = @"Directory\ToStore";
			testData.T6_ProductUpdateFileName = "ProductUpdate";
			testData.T6_Email = @"testemail@company.com.au";
			Factory.Save();
			testRegistration = Factory.LoadTop1<ClientAUSProductImportRegistry>(new ZQuery());
			AssertEquals("Importer/Supplier should not be registered", true, (testRegistration != null));
			AssertEquals("AllProducts", testRegistration.AllProductsFileName);
			AssertEquals("BUp", testRegistration.BackUpFileName);
			AssertEquals("ClientInvoicing", testRegistration.ClientInvoicingFileName);
			AssertEquals(@"Directory\ToStore", testRegistration.DirectoryToStoreExportFiles);
			AssertEquals("ProductUpdate", testRegistration.ProductUpdateFileName);
			AssertEquals(@"testemail@company.com.au", testRegistration.UpdateEmailAddress);
			AssertEquals(@"Directory\Imported", testRegistration.DirectoryForImportedPartsLog);
			AssertEquals(@"Directory\Rejected", testRegistration.DirectoryForRejectedPartsLog);
		}
	}
}
