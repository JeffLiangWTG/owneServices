using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.AUS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Products.Testing
{
	[TestedType(typeof(AUSOrgSupplierPartDataLoad))]
	public class AUSOrgSupplierPartDataLoadTest : DataLoadTestCase<AUSOrgSupplierPartDataLoad>
	{
		[ExpectNoExceptions]
		public void TestAUSOrgSupplierPartDataLoad()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			OrgHeader testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			OrgHeader testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			CreateTestRegistryData(testImporter, testSupplier);
			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile, "Part Number,Description,Import Classification");
				AUSOrgSupplierPartDataLoad testDataLoader = new AUSOrgSupplierPartDataLoad();
				testDataLoader.ImportProductData(tempFile.Filename, false, testImporter.PK, testSupplier.PK);
			}

			using (TempFile tempFile = TempFile.NewWithExtension("csv"))
			{
				PopulateTestFile(tempFile, "AAAAAAA,BBBBBBB,CCCCCCC");
				AUSOrgSupplierPartDataLoad testDataLoader = new AUSOrgSupplierPartDataLoad();
				testDataLoader.ImportProductData(tempFile.Filename, false, testImporter.PK, testSupplier.PK);
			}
		}

		protected override void ImportCSVTemplateHeading(AUSOrgSupplierPartDataLoad dataLoad, string dataLocation)
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			var testImporter = TestFactory.LoadTop1<OrgHeader>(new ZQuery());
			var testSupplier = TestFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			CreateTestRegistryData(testImporter, testSupplier);
			dataLoad.ImportProductData(dataLocation, false, testImporter.PK, testSupplier.PK);
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
		void PopulateTestFile(TempFile tempFile, ZString headerRow)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine(headerRow);
				sw.WriteLine("A1870GE,OWNER MANUAL IMPREZA  MY04,17C,SUSF");
				sw.WriteLine("B1870AE,SUPPLEMENT IMPREZA    MY04,17C,SUSF");
				sw.WriteLine("E2417AG010VW,FRONT UNDER SPOILER        21Z,209E,SUSF");
				sw.WriteLine("B1870AE,SUPPLEMENT IMPREZA    MY04,17C,SUSF"); // duplicate part
				sw.Flush();
			}
		}

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

		protected override AUSOrgSupplierPartDataLoad GetNewDataLoader()
		{
			return new AUSOrgSupplierPartDataLoad();
		}
		#endregion
	}
}
