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
	[TestedType(typeof(UpdateCA_K84AccountingDateOnLVXDeclaration))]
	class UpdateCA_K84AccountingDateOnLVXDeclarationTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertAddInfo("Declaration1 should execute DataTransformation", LVXDeclaration1, "AssesmentOption=1*MergeBy=NON*K84AccountingDate=2024-04-22 00:00:00", "2024-04-22 00:00:00");
			AssertAddInfo("Declaration2 should execute DataTransformation", LVXDeclaration2, "K84AccountingDate=2024-04-22 00:00:00", "2024-04-22 00:00:00");
			AssertAddInfo("Declaration3 should not execute DataTransformation", LVXDeclaration3, "AssesmentOption=1*K84AccountingDate=2016-04-22 00:00:00*MergeBy=NON");
			AssertAddInfo("Declaration4 should not execute DataTransformation", LVXDeclaration4, "");
			AssertAddInfo("Declaration5 should not execute DataTransformation", LVXDeclaration5, "AssesmentOption=1*MergeBy=NON");
			AssertAddInfo("Declaration6 should execute DataTransformation", LVXDeclaration6, "K84AccountingDate=2011-01-11 00:00:00", "2011-01-11 00:00:00");
		}

		void AssertAddInfo(string message, Guid declarationPK, string expectedAddInfoData, string expectedDate = null)
		{
			var actuaAddInfolData = Db.Connection.ExecuteScalar($"SELECT JE_AddInfo FROM dbo.JobDeclaration WHERE JE_PK = '{declarationPK}'");
			AssertEquals(message, expectedAddInfoData, actuaAddInfolData);
			var actualDate = Db.Connection.ExecuteScalar($"SELECT XA_Data FROM dbo.GenAddOnColumn where XA_ParentTableCode = 'JE' AND XA_Name = 'CA_K84AccountingDate' AND XA_ParentID = '{declarationPK}'");
			if (string.IsNullOrEmpty(expectedDate))
			{
				AssertNull(message, actualDate);
			}
			else
			{
				AssertEquals(message, expectedDate, actualDate);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCA_K84AccountingDateOnLVXDeclaration();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var companyPK = testDataCreator.CreateGlbCompany("AAA", "CA");
			var branchPK = testDataCreator.CreateGlbBranch("DDD", companyPK);
			var org = testDataCreator.CreateOrg("TES");
			LVXDeclaration1 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVX", new DateTime(2024, 06, 15), 1, "", "", "AssesmentOption=1*K84AccountingDate=2016-04-22 00:00:00.000*MergeBy=NON");
			var lvxHeader1 = testDataCreator.CreateJobComInvoiceHeader(branchPK, LVXDeclaration1, 2, "", "CA");
			var lVSDeclaration1 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVS", new DateTime(2024, 06, 15), 3, "", "", "K84AccountingDate=2024-04-22 00:00:00");
			testDataCreator.CreateGenAddOnColumn( "CA_K84AccountingDate", "2024-04-22 00:00:00", "JE", lVSDeclaration1);
			testDataCreator.CreateGenPivot("ZE", "JZ", lvxHeader1, "JE", lVSDeclaration1);

			LVXDeclaration2 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVX", new DateTime(2024, 06, 15), 4, "", "", "");
			var lvxHeader2 = testDataCreator.CreateJobComInvoiceHeader(branchPK, LVXDeclaration2, 5, "", "CA");
			testDataCreator.CreateGenAddOnColumn("CA_K84AccountingDate", "2011-01-11 00:00:00", "JE", LVXDeclaration2);
			testDataCreator.CreateGenPivot("ZE", "JZ", lvxHeader2, "JE", lVSDeclaration1);

			LVXDeclaration3 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVX", new DateTime(2024, 06, 15), 7, "", "", "AssesmentOption=1*K84AccountingDate=2016-04-22 00:00:00*MergeBy=NON");
			var lvxHeader3 = testDataCreator.CreateJobComInvoiceHeader(branchPK, LVXDeclaration3, 8, "", "CA");
			var lVSDeclaration3 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVS", new DateTime(2024, 06, 15), 9, "", "", "");
			testDataCreator.CreateGenPivot("ZE", "JZ", lvxHeader3, "JE", lVSDeclaration3);

			LVXDeclaration4 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVX", new DateTime(2024, 06, 15), 10, "", "", "");
			var lvxHeader4 = testDataCreator.CreateJobComInvoiceHeader(branchPK, LVXDeclaration4, 11, "", "CA");
			testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVS", new DateTime(2024, 06, 15), 12, "", "", "AssesmentOption=1*K84AccountingDate=2016-04-22 00:00:00*MergeBy=NON");

			LVXDeclaration5 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVX", new DateTime(2024, 06, 15), 13, "", "", "AssesmentOption=1*MergeBy=NON");
			var lvxHeader5 = testDataCreator.CreateJobComInvoiceHeader(branchPK, LVXDeclaration5, 14, "", "CA");
			var declaration = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "", new DateTime(2024, 06, 15), 15, "", "", "K84AccountingDate=2024-04-22 00:00:00");
			testDataCreator.CreateGenPivot("ZE", "JZ", lvxHeader5, "JE", declaration);

			LVXDeclaration6 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVX", new DateTime(2024, 06, 15), 16, "", "", "");
			var lvxHeader6 = testDataCreator.CreateJobComInvoiceHeader(branchPK, LVXDeclaration6, 17, "", "CA");
			var lVSDeclaration6 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVS", new DateTime(2024, 06, 15), 18, "", "", "");
			testDataCreator.CreateGenAddOnColumn("CA_K84AccountingDate", "2011-01-11 00:00:00", "JE", lVSDeclaration6);
			testDataCreator.CreateGenPivot("ZE", "JZ", lvxHeader6, "JE", lVSDeclaration6);
		}
		Guid LVXDeclaration1;
		Guid LVXDeclaration2;
		Guid LVXDeclaration3;
		Guid LVXDeclaration4;
		Guid LVXDeclaration5;
		Guid LVXDeclaration6;

		public void TestLogging_RealBatchSizeCanNotCoverClusterKeyRange()
		{
			AssertLogging_Pagination(7, new[]
			{
				"updated declarations [3], insert GenAddOnColumn [2], updated GenAddOnColumn [1].",
				"\tCompleted: Online Update CA_K84AccountingDate On LVX Declaration",
			});
		}

		public void TestLogging_RealBatchSizeCoverClusterKeyRange()
		{
			AssertLogging_Pagination(5000, new[]
			{
				"updated declarations [3], insert GenAddOnColumn [2], updated GenAddOnColumn [1].",
				"\tCompleted: Online Update CA_K84AccountingDate On LVX Declaration",
			});
		}

		void AssertLogging_Pagination(int batchSize, string[] expectedLog)
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)(new UpdateCA_K84AccountingDateOnLVXDeclaration(batchSize));
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expectedLog, logger);

			var indexCountQuery = @"SELECT COUNT(Name)
FROM sys.indexes 
WHERE name='GenAddOnColumn_Index_For_Online_Update_K84AccountingDate' AND object_id = OBJECT_ID('dbo.GenAddOnColumn')";

			var indexCount = Db.Connection.ExecuteScalar(indexCountQuery);
			AssertEquals("Index should be deleted once transformation completes", 0, indexCount);
		}
	}
}
