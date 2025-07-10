using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(JobDeclarationOrderNumber))]
	class JobDeclarationOrderNumberTest : DbCreateScriptTest
	{
		public void TestJePkWhenOwnerRefIsInJobDeclaration()
		{
			var job1Pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, ownerReference: "OWNREF1");
			TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, ownerReference: "OWNREF2");

			var dtResult = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT JE_PK FROM dbo.JobDeclarationOrderNumber WHERE JE_OwnerRef = 'OWNREF1'");

			AssertEquals("Expected found records", 1, dtResult.Rows.Count);
			AssertEquals("Expected JobDeclaration PK", job1Pk, dtResult.Rows[0]["JE_PK"]);
		}

		public void TestJePkWhenOnwerRefIsInJobOrderItemLinkedToJobDeclaration()
		{
			var job1Pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1);
			var jobDocAndCartagePk = TestDataCreator.CreateJobDocsAndCartage(job1Pk, "JE");
			TestDataCreator.CreateJobOrderItem(jobDocAndCartagePk, "OWNREF1");

			var dtResult = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT JE_PK FROM dbo.JobDeclarationOrderNumber WHERE JE_OwnerRef = 'OWNREF1'");

			AssertEquals("Expected found records", 1, dtResult.Rows.Count);
			AssertEquals("Expected JobDeclaration PK", job1Pk, dtResult.Rows[0]["JE_PK"]);
		}

		public void TestJePkWhenOnwerRefIsInJobOrderItemLinkedToShipment()
		{
			var ship1Pk = TestDataCreator.CreateShipment("SHIP1");
			var job1Pk = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, shipmentPK: ship1Pk);
			var jobDocAndCartagePk = TestDataCreator.CreateJobDocsAndCartage(ship1Pk, "JS");
			TestDataCreator.CreateJobOrderItem(jobDocAndCartagePk, "OWNREF1");

			var dtResult = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT JE_PK FROM dbo.JobDeclarationOrderNumber WHERE JE_OwnerRef = 'OWNREF1'");

			AssertEquals("Expected found records", 1, dtResult.Rows.Count);
			AssertEquals("Expected JobDeclaration PK", job1Pk, dtResult.Rows[0]["JE_PK"]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "AUCHI");
			TestDataCreator.CreateDepartment("TST");
		}
		Guid branchPK;
		Guid companyPK;
	}
}

