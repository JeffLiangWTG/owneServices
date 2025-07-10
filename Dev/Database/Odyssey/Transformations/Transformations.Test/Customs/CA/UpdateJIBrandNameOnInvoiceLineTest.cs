using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.CA;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.CA
{
	[TestedType(typeof(UpdateJIBrandNameOnInvoiceLine))]
	 class UpdateJIBrandNameOnInvoiceLineTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertAddInfo("Declaration1 should execute DataTransformation", invoiceLine1, "", "brandName");
			AssertAddInfo("Declaration2 should execute DataTransformation", invoiceLine2, "", "");
			AssertAddInfo("Declaration3 should execute DataTransformation", invoiceLine3, "CalculationMethod=N*ConveyanceIdentificationNumber=32434CFDSFSD", "brandName");
			AssertAddInfo("Declaration4 should execute DataTransformation", invoiceLine4, "CalculationMethod=N*ConveyanceIdentificationNumber=32434CFDSFSD", "brandName");
			AssertAddInfo("Declaration5 should execute DataTransformation", invoiceLine5, "ConveyanceIdentificationNumber=32434CFDSFSD", "brandName");

			AssertAddInfo("Declaration1 should execute DataTransformation", invoiceLine6, "", "brandName");
			AssertAddInfo("Declaration2 should execute DataTransformation", invoiceLine7, "", "");
			AssertAddInfo("Declaration3 should execute DataTransformation", invoiceLine8, "CalculationMethod=N*ConveyanceIdentificationNumber=32434CFDSFSD", "brandName");
			AssertAddInfo("Declaration4 should execute DataTransformation", invoiceLine9, "CalculationMethod=N*ConveyanceIdentificationNumber=32434CFDSFSD", "brandName");
			AssertAddInfo("Declaration5 should execute DataTransformation", invoiceLine10, "ConveyanceIdentificationNumber=32434CFDSFSD", "brandName");
		}

		void AssertAddInfo(string message, Guid invoiceLinePK, string expectedAddInfoData, string expectedBrandNameData)
		{
			var actuaAddInfolData = Db.Connection.ExecuteScalar($"SELECT JI_AddInfo FROM dbo.JobComInvoiceLine WHERE JI_PK = '{invoiceLinePK}'");
			var actuaBrandNamelData = Db.Connection.ExecuteScalar($"SELECT JI_BrandName FROM dbo.JobComInvoiceLine WHERE JI_PK = '{invoiceLinePK}'");

			AssertEquals(message + ". AddInfo is wrong", expectedAddInfoData, actuaAddInfolData);
			AssertEquals(message + ". BrandName is wrong", expectedBrandNameData, actuaBrandNamelData);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateJIBrandNameOnInvoiceLine();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var companyPK = testDataCreator.CreateGlbCompany("AAA", "CA");
			var branchPK = testDataCreator.CreateGlbBranch("DDD", companyPK);

			var invoiceHeader1 = testDataCreator.CreateJobComInvoiceHeader(branchPK, null, 1, dataModel: "CA");
			invoiceLine1 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, dataModel: "CA", addInfo: "BrandName=brandName");
			invoiceLine2 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, dataModel: "CA");
			invoiceLine3 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, dataModel: "CA", addInfo: "CalculationMethod=N*ConveyanceIdentificationNumber=32434CFDSFSD*BrandName=brandName");
			invoiceLine4 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, dataModel: "CA", addInfo: "CalculationMethod=N*BrandName=brandName*ConveyanceIdentificationNumber=32434CFDSFSD");
			invoiceLine5 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader1, 1, dataModel: "CA", addInfo: "BrandName=brandName*ConveyanceIdentificationNumber=32434CFDSFSD");

			var invoiceHeader2 = testDataCreator.CreateJobComInvoiceHeader(branchPK, null, 3, dataModel: "CA");

			invoiceLine6 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 3, dataModel: "CA", addInfo: "BrandName=brandName");
			invoiceLine7 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 3, dataModel: "CA");
			invoiceLine8 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 3, dataModel: "CA", addInfo: "CalculationMethod=N*ConveyanceIdentificationNumber=32434CFDSFSD*BrandName=brandName");
			invoiceLine9 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 3, dataModel: "CA", addInfo: "CalculationMethod=N*BrandName=brandName*ConveyanceIdentificationNumber=32434CFDSFSD");
			invoiceLine10 = testDataCreator.CreateJobComInvoiceLine(invoiceHeader2, 3, dataModel: "CA", addInfo: "BrandName=brandName*ConveyanceIdentificationNumber=32434CFDSFSD");
		}
		Guid invoiceLine1;
		Guid invoiceLine2;
		Guid invoiceLine3;
		Guid invoiceLine4;
		Guid invoiceLine5;
		Guid invoiceLine6;
		Guid invoiceLine7;
		Guid invoiceLine8;
		Guid invoiceLine9;
		Guid invoiceLine10;

		public void TestLogging_RealBatchSizeCanNotCoverClusterKeyRange()
		{
			AssertLogging_Pagination(2, new[]
			{
				"Batch start, cluster key end with [3].",
				"Updated lines [4].",
				"Batch start, cluster key end with [1].",
				"Updated lines [4].",
				"updated invoice line [8].",
				"\tCompleted: Update JI_BrandName On CA InvoiceLine"
			});
		}

		public void TestLogging_RealBatchSizeCoverClusterKeyRange()
		{
			AssertLogging_Pagination(5000, new[]
			{
				"Batch start, cluster key end with [3].",
				"Updated lines [8].",
				"updated invoice line [8].",
				"\tCompleted: Update JI_BrandName On CA InvoiceLine"
			});
		}

		void AssertLogging_Pagination(int batchSize, string[] expectedLog)
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)(new UpdateJIBrandNameOnInvoiceLine(batchSize));
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expectedLog, logger);

			var indexCountQuery = @"SELECT COUNT(Name)
FROM sys.indexes 
WHERE name='JobComInvoiceLine_Index_For_Online_Update_JI_BrandName' AND object_id = OBJECT_ID('dbo.JobComInvoiceLine')";

			var indexCount = Db.Connection.ExecuteScalar(indexCountQuery);
			AssertEquals("Index should be deleted once transformation completes", 0, indexCount);
		}
	}
}
