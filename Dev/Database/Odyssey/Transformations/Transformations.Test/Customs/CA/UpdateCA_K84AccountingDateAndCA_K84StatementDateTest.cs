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
	[TestedType(typeof(UpdateCA_K84AccountingDateAndCA_K84StatementDate))]
	class UpdateCA_K84AccountingDateAndCA_K84StatementDateTest : DataTransformationTestCase
	{
		public void TestLogging_RealBatchSizeCanNotCoverClusterKeyRange()
		{
			AssertLogging_Pagination(4, new string[]
			{
				"updated declarations [4].",
				"Deleted GenAddonColumn(CA_K84AccountingDate) [2].",
				"\tCompleted: Online Update CA_K84AccountingDate And CA_K84StatementDate On IMP/LVS Declaration",
			});
		}

		public void TestLogging_RealBatchSizeCoverClusterKeyRange()
		{
			AssertLogging_Pagination(5000, new[]
			{
				"updated declarations [4].",
				"Deleted GenAddonColumn(CA_K84AccountingDate) [2].",
				"\tCompleted: Online Update CA_K84AccountingDate And CA_K84StatementDate On IMP/LVS Declaration",
			});
		}

		void AssertLogging_Pagination(int batchSize, string[] expectedLog)
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)(new UpdateCA_K84AccountingDateAndCA_K84StatementDate(batchSize));
			transformation.Run(s => logger.Add(s), CancellationToken.None);
			AssertContainsExactElementsInExactOrder(expectedLog, logger);
		}

		protected override void AssertTransformationResults()
		{
			AssertAddInfo("Declaration1 should execute DataTransformation", Declaration1, "AssesmentOption=1*MergeBy=NON*K84AccountingDate=2024-06-18 00:00:00.000*K84StatementDate=2024-06-18 00:00:00.000", "Jun 18 2024 12:00AM");
			AssertAddInfo("Declaration2 should execute DataTransformation", Declaration2, "AssesmentOption=1*K84AccountingDate=2016-04-22 00:00:00.000*K84StatementDate=2024-05-12 00:00:00.000");
			AssertAddInfo("Declaration3 should execute DataTransformation", Declaration3, "AssesmentOption=1*K84StatementDate=2016-04-22 00:00:00.000*K84AccountingDate=2024-05-25 00:00:00.000", "May 25 2024 12:00AM");
			AssertAddInfo("Declaration4 should not execute DataTransformation", Declaration4, "AssesmentOption=1*MergeBy=NON*K84AccountingDate=2016-04-22 00:00:00.000*K84StatementDate=2016-04-22 00:00:00.000");
			AssertAddInfo("Declaration5 should not execute DataTransformation", Declaration5, "AssesmentOption=1*MergeBy=NON");
			AssertAddInfo("Declaration6 should execute DataTransformation", Declaration6, "AssesmentOption=1*MergeBy=NON*K84AccountingDate=2024-06-18 00:00:00.000*K84StatementDate=2024-06-18 00:00:00.000", "Jun 18 2024 12:00AM");
			AssertAddInfo("Declaration7 should not execute DataTransformation", Declaration7, "AssesmentOption=1*MergeBy=NON");
		}

		void AssertAddInfo(string message, Guid declarationPK, string expectedAddInfoData, string expectedK84AccountingDate = null)
		{
			var actuaAddInfolData = Db.Connection.ExecuteScalar($"SELECT JE_AddInfo FROM dbo.JobDeclaration WHERE JE_PK = '{declarationPK}'");
			AssertEquals(message, expectedAddInfoData, actuaAddInfolData);
			var k84AccountingDate = Db.Connection.ExecuteScalar($"SELECT XA_Data FROM dbo.GenAddOnColumn where XA_ParentTableCode = 'JE' AND XA_Name = 'CA_K84AccountingDate' AND XA_ParentID = '{declarationPK}'");
			AssertNull(k84AccountingDate);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateCA_K84AccountingDateAndCA_K84StatementDate();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var companyPK = testDataCreator.CreateGlbCompany("AAA", "CA");
			var branchPK = testDataCreator.CreateGlbBranch("DDD", companyPK);
			var org = testDataCreator.CreateOrg("TES");

			Declaration1 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "IMP", new DateTime(2024, 06, 15), 1, "", "", "AssesmentOption=1*MergeBy=NON");
			var entry1 = testDataCreator.CreateCusEntryHeader("00000000000001", string.Empty, Declaration1, "CAD", new DateTime(2024, 06, 15), new DateTime(2024, 06, 18), 1, "CA");
			testDataCreator.CreateCusEntryLineFee(0m, "TOT", testDataCreator.CreateCusEntryLine(entry1, "CA", 1), 1, "CUS");
			testDataCreator.CreateGenAddOnColumn("CA_K84AccountingDate", "2016-04-22 00:00:00.000", "JE", Declaration1);

			Declaration2 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVS", new DateTime(2024, 06, 15), 2, "", "", "AssesmentOption=1*K84AccountingDate=2016-04-22 00:00:00.000");
			var entry2 = testDataCreator.CreateCusEntryHeader("00000000000002", string.Empty, Declaration2, "CAD", new DateTime(2024, 06, 15), new DateTime(2024, 05, 12), 2, "CA");
			testDataCreator.CreateCusEntryLineFee(0m, "TOT", testDataCreator.CreateCusEntryLine(entry2, "CA", 2), 2, "CUS");
			testDataCreator.CreateGenAddOnColumn("CA_K84AccountingDate", "2016-04-22 00:00:00.000", "JE", Declaration2);

			Declaration3 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "IMP", new DateTime(2024, 06, 15), 3, "", "", "AssesmentOption=1*K84StatementDate=2016-04-22 00:00:00.000");
			var entry3 = testDataCreator.CreateCusEntryHeader("00000000000003", string.Empty, Declaration3, "CAD", new DateTime(2024, 06, 15), new DateTime(2024, 05, 25), 3, "CA");
			testDataCreator.CreateCusEntryLineFee(0m, "TOT", testDataCreator.CreateCusEntryLine(entry3, "CA", 3), 3, "CUS");

			Declaration4 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVS", new DateTime(2024, 06, 15), 4, "", "", "AssesmentOption=1*MergeBy=NON*K84AccountingDate=2016-04-22 00:00:00.000*K84StatementDate=2016-04-22 00:00:00.000");
			var entry4 = testDataCreator.CreateCusEntryHeader("00000000000004", string.Empty, Declaration4, "CAD", new DateTime(2024, 06, 15), new DateTime(2024, 06, 18), 4, "CA");
			testDataCreator.CreateCusEntryLineFee(0m, "TOT", testDataCreator.CreateCusEntryLine(entry4, "CA", 4), 4, "CUS");

			Declaration5 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "IMP", new DateTime(2024, 06, 15), 5, "", "", "AssesmentOption=1*MergeBy=NON");
			var entry5 = testDataCreator.CreateCusEntryHeader("00000000000005", string.Empty, Declaration5, "CAD", new DateTime(2024, 06, 15), new DateTime(2024, 06, 18), 5, "CA");
			testDataCreator.CreateCusEntryLineFee(0m, "TOT", testDataCreator.CreateCusEntryLine(entry5, "CA", 5), 5, "CUS");
			testDataCreator.CreateCusEntryLineFee(10m, "TOT", testDataCreator.CreateCusEntryLine(entry5, "CA", 5), 5, "CUS");

			Declaration6 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "LVS", new DateTime(2024, 06, 15), 6, "", "", "AssesmentOption=1*MergeBy=NON");
			var entry6 = testDataCreator.CreateCusEntryHeader("00000000000006", string.Empty, Declaration6, "CAD", new DateTime(2024, 06, 15), new DateTime(2024, 06, 18), 6, "CA");
			testDataCreator.CreateCusEntryLineFee(0m, "TOT", testDataCreator.CreateCusEntryLine(entry6, "CA", 6), 6, "CW1");

			Declaration7 = testDataCreator.CreateJobDeclaration("CA", branchPK, companyPK, org, "IMP", new DateTime(2024, 06, 15), 7, "", "", "AssesmentOption=1*MergeBy=NON");
			var entry7 = testDataCreator.CreateCusEntryHeader("00000000000007", string.Empty, Declaration7, "CAD", new DateTime(2024, 06, 15), new DateTime(2024, 06, 18), 7, "CA");
		}

		Guid Declaration1;
		Guid Declaration2;
		Guid Declaration3;
		Guid Declaration4;
		Guid Declaration5;
		Guid Declaration6;
		Guid Declaration7;
	}
}
