using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.AUS.Products;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Testing
{
	public class ClientAUSProductInterfaceMediatorTest : TransactionedTestCase
	{
		public void TestInserRow()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			AssertEquals("No records should exist in interface table yet", 0, CountInterfaceRecords());
			InsertTestInterfaceData(testImporter.PK, testSupplier.PK);
			AssertEquals("Interface record should have been created", 1, CountInterfaceRecords());
		}

		public void TestCreateExportFile()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSProductInterfaceMediator testMediator = new ClientAUSProductInterfaceMediator(testImporter.PK, testSupplier.PK);
			AssertEquals("No export data should be created for this Importer/Supplier", false, testMediator.CreateExportFile());
			InsertTestInterfaceData(testImporter.PK, testSupplier.PK);
			AssertEquals("Export data is expected to be created for this Importer/Supplier", true, testMediator.CreateExportFile());
		}

		public void TestUpdateChangeDeleteRow()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			AssertEquals("No records should exist in interface table yet", 0, CountInterfaceRecords());
			InsertTestInterfaceData(testImporter.PK, testSupplier.PK);
			AssertEquals("Interface record should have been created", 1, CountInterfaceRecords());
			ClientAUSProductInterfaceMediator testMediator = new ClientAUSProductInterfaceMediator(testImporter.PK, testSupplier.PK);
			AssertEquals(false, testMediator.InterfaceRecordExists(Guid.NewGuid()));
			Guid interfacePK = GetInterfacePK();
			AssertEquals(true, testMediator.InterfaceRecordExists(interfacePK));
			AssertEquals("Interface details should not show as changed - manually added part", false, testMediator.CheckChangedDetails(interfacePK));
			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData testData = new ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData();
			testData.NewProductCode = "NewProductCode";
			testData.NewProductDesc = "NewProductDesc";
			testData.NewImportClassification = "NewTestClass";
			testMediator.UpdateRowWithCurrentDetails(testData, interfacePK);
			testMediator.UpdateRowStatus(interfacePK, 'I', "Imported - Changed");
			AssertEquals("Interface details should now show as changed", true, testMediator.CheckChangedDetails(interfacePK));
			testMediator.UpdateProductAsClosed(interfacePK, ZDateTime.UtcNow);
			testMediator.DeletePartFromProductInterface(interfacePK);
			AssertEquals("Interface record should no longer exist", false, testMediator.InterfaceRecordExists(interfacePK));
		}

		public void TestGetAllProductsForThisImporterAndSupplier()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			ClientAUSProductInterfaceMediator testMediator = new ClientAUSProductInterfaceMediator(testImporter.PK, testSupplier.PK);
			ClientAUSProductInterfaceMediator.ProductInterfaceDetails[] productsInterfaceList = testMediator.GetAllProductKeysForThisImporterAndSupplier();
			AssertEquals("Should be no interface data keys for this Importer/Supplier", 0, productsInterfaceList.Length);
			InsertTestInterfaceData(testImporter.PK, testSupplier.PK);
			productsInterfaceList = testMediator.GetAllProductKeysForThisImporterAndSupplier();
			AssertEquals("Should return 1 interface data keys for this Importer/Supplier", 1, productsInterfaceList.Length);
			Guid interfacePK = GetInterfacePK();
			foreach (ClientAUSProductInterfaceMediator.ProductInterfaceDetails interfaceKeys in productsInterfaceList)
			{
				AssertEquals(interfacePK, interfaceKeys.InterfacePK.ToGuid());
			}

			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData testData = new ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData();
			testData.NewProductCode = "New_Product_Code";
			testData.NewProductDesc = "New_Product_Desc";
			testData.NewImportClassification = "New_TestClass";
			testMediator.UpdateRowWithCurrentDetails(testData, interfacePK);
			ClientAUSProductInterfaceMediator.ProductInterfaceData[] productsList = testMediator.GetAllProductsForThisImporterAndSupplier(true);
			AssertEquals("Should also be 1 interface product data for this Importer/Supplier", 1, productsInterfaceList.Length);
			foreach (ClientAUSProductInterfaceMediator.ProductInterfaceData productData in productsList)
			{
				AssertEquals("New_Product_Code", productData.ProductCode);
				AssertEquals("New_Product_Desc", productData.ProductDesc);
				AssertEquals("New_TestClass", productData.ImportClassification);
				AssertEquals("Manually Added", productData.Status);
			}
		}

		#region Implementation
		void InsertTestInterfaceData(ZGuid testImporterPK, ZGuid testSupplierPK)
		{
			OrgSupplierPart testPart = TestFactory.NewWithValidTestData<OrgSupplierPart>();
			ClientAUSProductInterfaceMediator testMediator = new ClientAUSProductInterfaceMediator(testImporterPK, testSupplierPK);
			ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData testData = new ClientAUSProductInterfaceMediator.ClientAUSProductInterfaceData();
			testData.ImporterPK = testImporterPK;
			testData.SupplierPK = testSupplierPK;
			testData.Indicator = 'M';
			testData.OriginalProductCode = "ProductCode";
			testData.OriginalProductDesc = "ProductDesc";
			testData.OriginalImportClassification = "TestClass";
			testData.ProductPK = testPart.PK;
			testData.Status = "Manually Added";
			testData.ImportDateTime = ZDateTime.UtcNow;
			testMediator.InsertRow(testData);
			AssertEquals("InterfaceRecord should exist for this part", true, testMediator.InterfaceRecordExistsForPart(testPart.PK));
		}

		int CountInterfaceRecords()
		{
			string sqlText = @"SELECT count(*) FROM ClientAUSProductInterface";
			DbCommand command = Db.Connection.Command(sqlText);
			return ZArchitecture.Core.Utilities.ConvertToInt32(command.ExecuteScalar());
		}

		Guid GetInterfacePK()
		{
			string sqlText = @"SELECT TOP 1 T5_PK FROM ClientAUSProductInterface";
			DbCommand command = Db.Connection.Command(sqlText);
			return (Guid)(command.ExecuteScalar());
		}

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
		#endregion
	}
}
