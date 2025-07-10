using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.AUS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Products.Testing
{
	public class ExportProductsTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestExportProducts()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ExportProducts exporter = new ExportProducts(testImporter.PK, testSupplier.PK, false, false, "EmailAddress");
			exporter.Export(null);
		}

		[ExpectNoExceptions]
		public void TestCreationOfExportFiles()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			CreateTestRegistryData(testImporter, testSupplier);
			CreateTestProductData(testImporter, testSupplier, "TestPart");
			ExportProducts exporter = new ExportProducts(testImporter.PK, testSupplier.PK, false, false, "EmailAddress");
			exporter.Export(new ProcessedEventHandler(DummyProgressBar));
		}

		public void TestSimulationExportData()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			CreateTestRegistryData(testImporter, testSupplier);
			//Establish some test product data (4 records) to simulate export data run
			CreateTestProductData(testImporter, testSupplier, "TestPart1");
			CreateTestProductData(testImporter, testSupplier, "TestPart2");
			CreateTestSimulatedImportedProductData(testImporter, testSupplier, OrgPartRelation.RelationshipTypes.Owner);
			TestFactory.Save(); // 3 parts will appear, 1 part should show as deleted...
			CreateTestProductData(testImporter, testSupplier, "TestPart3");
			ClientAUSProductInterfaceMediator testMediator = new ClientAUSProductInterfaceMediator(testImporter.PK, testSupplier.PK);
			ClientAUSProductInterfaceMediator.ProductInterfaceDetails[] productsInterfaceList = testMediator.GetAllProductKeysForThisImporterAndSupplier();
			AssertEquals("There should only be 1 interface record currently", 1, productsInterfaceList.Length);
			ExportProducts exporter = new ExportProducts(testImporter.PK, testSupplier.PK, true, true, "EmailAddress");
			exporter.Export(new ProcessedEventHandler(DummyProgressBar));
			productsInterfaceList = testMediator.GetAllProductKeysForThisImporterAndSupplier();
			AssertEquals("There should now only be 3 interface records for this Importer/Supplier", 3, productsInterfaceList.Length);
			foreach (ClientAUSProductInterfaceMediator.ProductInterfaceDetails interfaceKeys in productsInterfaceList)
			{
				CheckInterfaceRecordHasBeenClosed(interfaceKeys.InterfacePK);
			}
		}

		public void TestSimulationExportDataWithBoth()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			CreateTestRegistryData(testImporter, testSupplier);
			//Establish some test product data (4 records) to simulate export data run
			CreateTestProductData(testImporter, testSupplier, "TestPart1");
			CreateTestProductData(testImporter, testSupplier, "TestPart2");
			CreateTestSimulatedImportedProductData(testImporter, testSupplier, OrgPartRelation.RelationshipTypes.Both);
			TestFactory.Save(); // 3 parts will appear, 1 part should show as deleted...
			CreateTestProductData(testImporter, testSupplier, "TestPart3");
			ClientAUSProductInterfaceMediator testMediator = new ClientAUSProductInterfaceMediator(testImporter.PK, testSupplier.PK);
			ClientAUSProductInterfaceMediator.ProductInterfaceDetails[] productsInterfaceList = testMediator.GetAllProductKeysForThisImporterAndSupplier();
			AssertEquals("There should only be 1 interface record currently", 1, productsInterfaceList.Length);
			ExportProducts exporter = new ExportProducts(testImporter.PK, testSupplier.PK, true, true, "EmailAddress");
			exporter.Export(new ProcessedEventHandler(DummyProgressBar));
			productsInterfaceList = testMediator.GetAllProductKeysForThisImporterAndSupplier();
			AssertEquals("There should now only be 3 interface records for this Importer/Supplier", 3, productsInterfaceList.Length);
			foreach (ClientAUSProductInterfaceMediator.ProductInterfaceDetails interfaceKeys in productsInterfaceList)
			{
				CheckInterfaceRecordHasBeenClosed(interfaceKeys.InterfacePK);
			}
		}

		#region Implementation
		#region TestFactory
		public BusinessObjectFactory TestFactory
		{
			get
			{
				if (fTestFactory == null)
				{
					fTestFactory = new BusinessObjectFactory();
				}

				return fTestFactory;
			}
		}

		BusinessObjectFactory fTestFactory;
		#endregion
		void CreateTestRegistryData(OrgHeader importer, OrgHeader supplier)
		{
			ClientAUSProductImportRegistry testData = TestFactory.NewWithValidTestData<ClientAUSProductImportRegistry>();
			testData.T6_OH_Importer = importer.PK;
			testData.T6_OH_Supplier = supplier.PK;
			testData.T6_AllProductsFileName = "AllProducts";
			testData.T6_BackUpFileName = "BUp";
			testData.T6_ClientInvoicingFileName = "ClientInvoicing";
			testData.T6_DirectoryImportedParts = @"Directory\Imported";
			testData.T6_DirectoryRejectedParts = @"Directory\Rejected";
			testData.T6_DirectoryToStoreFiles = @"Directory\ToStore";
			testData.T6_ProductUpdateFileName = "ProductUpdate";
			testData.T6_Email = @"testemail@company.com.au";
			TestFactory.Save();
		}

		void CreateTestProductData(OrgHeader importer, OrgHeader supplier, string partNum)
		{
			OrgSupplierPart testPart = TestFactory.NewWithValidTestData<OrgSupplierPart>();
			testPart.OP_PartNum = partNum;
			OrgPartRelation importerRelation = TestFactory.NewWithValidTestData<OrgPartRelation>();
			importerRelation.OU_OP = testPart.PK;
			importerRelation.OU_OH = importer.PK;
			importerRelation.OU_Relationship = "OWN";
			OrgPartRelation supplierRelation = TestFactory.NewWithValidTestData<OrgPartRelation>();
			supplierRelation.OU_OP = testPart.PK;
			supplierRelation.OU_OH = supplier.PK;
			supplierRelation.OU_Relationship = "SUP";
		}

		void CreateTestSimulatedImportedProductData(OrgHeader importer, OrgHeader supplier, string ownerRelationship)
		{
			OrgSupplierPart testPart = TestFactory.NewWithValidTestData<OrgSupplierPart>();
			testPart.OP_PartNum = "TestPart";
			OrgPartRelation importerRelation = TestFactory.NewWithValidTestData<OrgPartRelation>();
			importerRelation.OU_OP = testPart.PK;
			importerRelation.OU_OH = importer.PK;
			importerRelation.OU_Relationship = ownerRelationship;
			OrgPartRelation supplierRelation = TestFactory.NewWithValidTestData<OrgPartRelation>();
			supplierRelation.OU_OP = testPart.PK;
			supplierRelation.OU_OH = supplier.PK;
			supplierRelation.OU_Relationship = "SUP";
			ClientAUSProductInterfaceMediator testMediator = new ClientAUSProductInterfaceMediator(importer.PK, supplier.PK);
			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData testData = new ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData();
			testData.ImporterPK = importer.PK;
			testData.SupplierPK = supplier.PK;
			testData.Indicator = 'I';
			testData.OriginalProductCode = "ProductCode";
			testData.OriginalProductDesc = "ProductDesc";
			testData.OriginalImportClassification = "TestClass";
			testData.ProductPK = testPart.PK;
			testData.Status = "Imported - Changed";
			testData.ImportDateTime = ZDateTime.UtcNow;
			testMediator.InsertRow(testData);
			AssertEquals("InterfaceRecord should exist for this part", true, testMediator.InterfaceRecordExistsForPart(testPart.PK));
		}

		void DummyProgressBar(object sender, ProcessedEventArgs arg)
		{
		}

		void CheckInterfaceRecordHasBeenClosed(ZGuid interfacePK)
		{
			string sqlText = @"SELECT T5_Indicator FROM ClientAUSProductInterface";
			DbCommand command = Db.Connection.Command(sqlText);
			char result = Convert.ToChar(command.ExecuteScalar());
			AssertEquals("Interface Record should be closed", 'C', result);
		}
		#endregion
	}
}
