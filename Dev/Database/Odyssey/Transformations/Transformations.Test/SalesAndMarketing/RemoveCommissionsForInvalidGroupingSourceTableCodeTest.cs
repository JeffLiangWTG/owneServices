using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(RemoveCommissionsForInvalidGroupingSourceTableCode))]
	public class RemoveCommissionsForInvalidGroupingSourceTableCodeTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Remove Commissions For Invalid Grouping Source Table Code_1] ON [dbo].[AccCommissionHeader] ([CH0_GroupingSourceTableCode]) INCLUDE ([CH0_PK]) WHERE ([CH0_GroupingSourceTableCode]<>'JH' AND [CH0_GroupingSourceTableCode]<>'AH') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override DataTransformation GetNewTestTransformationInstance()
	=> new RemoveCommissionsForInvalidGroupingSourceTableCode();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists("AccCommissionHeader", "Constraint_CH0_GroupingSourceTableCode_NoCheck");

			var companyPK = (Guid)Db.Connection.Command("SELECT TOP(1) GC_PK FROM dbo.GlbCompany").ExecuteScalar();
			var branchPK = (Guid)Db.Connection.Command($"SELECT TOP(1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{companyPK}'").ExecuteScalar();
			var departmentPK = (Guid)Db.Connection.Command("SELECT TOP(1) GE_PK FROM dbo.GlbDepartment").ExecuteScalar();

			var helper = new TransformationTestDataCreator();

			var transactionHeader1PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "001");
			var transactionHeader2PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "002");
			var transactionHeader3PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "003");
			var transactionHeader4PK = helper.CreateAccTransactionHeader("AP", "INV", branchPK, companyPK, departmentPK, "004");

			var shipment1PK = helper.CreateJobShipment("JS001", "CTN", "1");
			var job1PK = Guid.NewGuid();
			helper.CreateJobHeader(job1PK, shipment1PK, "JS", "SHP00001", companyPK, branchPK, departmentPK);

			var shipment2PK = helper.CreateJobShipment("JS002", "CTN", "2");
			var job2PK = Guid.NewGuid();
			helper.CreateJobHeader(job2PK, shipment2PK, "JS", "SHP00002", companyPK, branchPK, departmentPK);

			var orgPk = helper.CreateOrgHeader("TEST01", "TEST ORGANISATION");
			var oppPk = helper.CreateOrgOpportunity("O10000001", orgPk, companyPK);
			var agreementPk = helper.CreateOrgCommissionAgreement(oppPk, "#1", orgPk);
			var chargePk = helper.CreateAccChargeCode("TESCHG", "OVR", companyPK);

			header1 = helper.CreateAccCommissionHeader(companyPK, transactionHeader1PK, transactionHeader1PK, "AH", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");
			header2 = helper.CreateAccCommissionHeader(companyPK, transactionHeader2PK, job1PK, "JH", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");
			header3 = helper.CreateAccCommissionHeader(companyPK, transactionHeader3PK, job2PK, "JH", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");
			header4 = helper.CreateAccCommissionHeader(companyPK, transactionHeader4PK, Guid.Empty, "BAD", agreementPk, orgPk, "SHP", "", "", DateTime.Now, "SEA", "AU", "NZ");

			lineGroup1 = helper.CreateAccCommissionLineGroup(header1, chargePk, 200M, "AUD", 200M, "AUD", new DateTime(2024, 1, 1));
			lineGroup2 = helper.CreateAccCommissionLineGroup(header4, chargePk, 300M, "AUD", 300M, "AUD", new DateTime(2024, 1, 1));

			line1 = helper.CreateAccCommissionLine(lineGroup1, "CLG", "DUM", "AUD", 200M, "PCT", "AUD", 200M, 1, 1, 200M, 10M, 5M, true);
			line2 = helper.CreateAccCommissionLine(lineGroup2, "CLG", "DUM", "AUD", 300M, "PCT", "AUD", 300M, 1, 1, 300M, 15M, 5M, true);
			line3 = helper.CreateAccCommissionLine(header3, "CH0", "DUM", "AUD", 1000M, "PCT", "AUD", 1000M, 1, 1, 1000M, 50M, 5M, true);
			line4 = helper.CreateAccCommissionLine(header4, "CH0", "DUM", "AUD", 2000M, "PCT", "AUD", 2000M, 1, 1, 2000M, 100M, 5M, true);

			approvalRequest = helper.CreateAccCommissionApprovalRequest("BATCH01", "AAA", "BBB", false, false);
			approvalRequestItem1 = helper.CreateAccCommissionApprovalRequestItem(approvalRequest, line1, false);
			approvalRequestItem2 = helper.CreateAccCommissionApprovalRequestItem(approvalRequest, line2, false);
			approvalRequestItem3 = helper.CreateAccCommissionApprovalRequestItem(approvalRequest, line3, false);
			approvalRequestItem4 = helper.CreateAccCommissionApprovalRequestItem(approvalRequest, line4, false);
		}

		protected override void AssertTransformationResults()
		{
			AssertPrimaryKeys("AccCommissionApprovalRequestItem", "CRI_PK", [approvalRequestItem1, approvalRequestItem2, approvalRequestItem3, approvalRequestItem4], [approvalRequestItem1, approvalRequestItem3]);
			AssertPrimaryKeys("AccCommissionLine", "CL0_PK", [line1, line2, line3, line4], [line1, line3]);
			AssertPrimaryKeys("AccCommissionLineGroup", "CLG_PK", [lineGroup1, lineGroup2], [lineGroup1]);
			AssertPrimaryKeys("AccCommissionHeader", "CH0_PK", [header1, header2, header3, header4], [header1, header2, header3]);
		}

		protected void AssertPrimaryKeys(string tableName, string primaryKeyName, Guid[] whereClausePks, Guid[] expectedPks)
		{
			var sql = $"SELECT {primaryKeyName} FROM {tableName} WHERE {primaryKeyName} IN ('{string.Join("','", whereClausePks)}')";
			var pksFound = new List<Guid>();
			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					var pkFound = reader.GetGuid(0);
					pksFound.Add(pkFound);
				}
			}

			AssertContainsExactElementsInAnyOrder($"Found in {tableName}", expectedPks, pksFound);
		}

		Guid header1;
		Guid header2;
		Guid header3;
		Guid header4;
		Guid lineGroup1;
		Guid lineGroup2;
		Guid line1;
		Guid line2;
		Guid line3;
		Guid line4;
		Guid approvalRequest;
		Guid approvalRequestItem1;
		Guid approvalRequestItem2;
		Guid approvalRequestItem3;
		Guid approvalRequestItem4;
	}
}
