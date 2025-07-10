using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(GetDraftInvoicePostingStatus))]
	class GetDraftInvoicePostingStatusTest : DbCreateScriptTest
	{
		public void TestPostingStatus_PST_INV()
		{
			AssertPostingStatus(() => DbHelper.InsertTransactionHeader("AP", "INV", "INV123", 330m, PostDate, BranchPK, DepartmentPK), "PST");
		}

		public void TestPostingStatus_PST_CRD()
		{
			AssertPostingStatus(() => DbHelper.InsertTransactionHeader("AP", "CRD", "CRD123", 330m, PostDate, BranchPK, DepartmentPK), "PST");
		}

		public void TestPostingStatus_CAN()
		{
			AssertPostingStatus(() => DbHelper.InsertTransactionHeader("AP", "INV", "INV123", 330m, PostDate, BranchPK, DepartmentPK, isCancelled: true), "CAN");
		}

		public void TestPostingStatus_PIC_INI()
		{
			AssertPostingStatus(() => DbHelper.InsertTransactionHeader("IN", "INI", "INV123", 330m, PostDate, BranchPK, DepartmentPK), "PIC");
		}

		public void TestPostingStatus_PIC_INC()
		{
			AssertPostingStatus(() => DbHelper.InsertTransactionHeader("IN", "INC", "INC123", 330m, PostDate, BranchPK, DepartmentPK), "PIC");
		}

		public void TestPostingStatus_PAI_INV()
		{
			AssertPostingStatus(() => DbHelper.InsertTransactionHeader("AP", "INV", "INV123", 330m, PostDate, BranchPK, DepartmentPK, fullyPaidDate: PostDate), "PAI");
		}

		public void TestPostingStatus_PAI_CRD()
		{
			AssertPostingStatus(() => DbHelper.InsertTransactionHeader("AP", "CRD", "CRD123", 330m, PostDate, BranchPK, DepartmentPK, fullyPaidDate: PostDate), "PAI");
		}

		public void TestPostingStatus_PCM_INV()
		{
			AssertPostingStatus(() =>
			{
				var org = DbHelper.InsertOrgHeader("ZZC", "Org1");
				var orgContact = DbHelper.InsertOrgContact("OC1", "", false, org);
				var headerPK = DbHelper.InsertTransactionHeader("AP", "INV", "INV123", 100m, PostDate, BranchPK, DepartmentPK, outstandingAmount: 100m);
				DbHelper.InsertQueryClaim("Test1", "OPN", 10m, headerPK, BranchPK, orgContact, org);

				return headerPK;
			}, "PCM");
		}

		public void TestPostingStatus_PCM_CRD()
		{
			AssertPostingStatus(() =>
			{
				var org = DbHelper.InsertOrgHeader("ZZC", "Org1");
				var orgContact = DbHelper.InsertOrgContact("OC1", "", false, org);
				var headerPK = DbHelper.InsertTransactionHeader("AP", "CRD", "CRD123", 100m, PostDate, BranchPK, DepartmentPK, outstandingAmount: 100m);
				DbHelper.InsertQueryClaim("Test1", "OPN", 10m, headerPK, BranchPK, orgContact, org);

				return headerPK;
			}, "PCM");
		}

		public void TestPostingStatus_PUA_INI()
		{
			AssertPostingStatus(() =>
			{
				var org = DbHelper.InsertOrgHeader("ZZC", "Org1");
				var orgContact = DbHelper.InsertOrgContact("OC1", "", false, org);
				var headerPK = DbHelper.InsertTransactionHeader("IN", "INI", "INV123", 330m, PostDate, BranchPK, DepartmentPK);

				DbHelper.Insert(GenApprovalRequestSchema.Constants.TableName, new
				{
					XP_PK = Guid.NewGuid(),
					XP_GB_RequestingBranch = BranchPK,
					XP_ParentID = headerPK
				});

				return headerPK;
			}, "PUA");
		}

		public void TestPostingStatus_PUA_INC()
		{
			AssertPostingStatus(() =>
			{
				var org = DbHelper.InsertOrgHeader("ZZC", "Org1");
				var orgContact = DbHelper.InsertOrgContact("OC1", "", false, org);
				var headerPK = DbHelper.InsertTransactionHeader("IN", "INC", "INC123", 330m, PostDate, BranchPK, DepartmentPK);

				DbHelper.Insert(GenApprovalRequestSchema.Constants.TableName, new
				{
					XP_PK = Guid.NewGuid(),
					XP_GB_RequestingBranch = BranchPK,
					XP_ParentID = headerPK
				});

				return headerPK;
			}, "PUA");
		}

		void AssertPostingStatus(Func<Guid> createTransactionHeader, string expectedPostingStatus)
		{
			var headerPK = createTransactionHeader();
			var draftInvoiceHeaderPK = DbHelper.InsertAccDraftInvoiceHeader(CompanyPK, BranchPK, DepartmentPK, headerPK);

			var result = RunScript("NULL");
			AssertEquals("Expecting 1 row", 1, result.Rows.Count);

			var row = result.Rows[0];
			AssertEquals(draftInvoiceHeaderPK, row["AIHP_PK"]);
			AssertEquals(expectedPostingStatus, row["AIHP_PostingStatus"]);
		}

		public void TestLoadingTwoItems()
		{
			var headerPK = DbHelper.InsertTransactionHeader("AP", "INV", "INV123", 330, PostDate, BranchPK, DepartmentPK);
			var draftInvoiceHeaderPK = DbHelper.InsertAccDraftInvoiceHeader(CompanyPK, BranchPK, DepartmentPK, headerPK);
			var headerPK2 = DbHelper.InsertTransactionHeader("AP", "INV", "INV222", 330, PostDate, BranchPK, DepartmentPK);
			DbHelper.InsertAccDraftInvoiceHeader(CompanyPK, BranchPK, DepartmentPK, headerPK2);

			var result = RunScript("NULL");
			AssertEquals(2, result.Rows.Count);

			result = RunScript($"'{draftInvoiceHeaderPK}'");
			AssertEquals(1, result.Rows.Count);

			var row = result.Rows[0];
			AssertEquals(draftInvoiceHeaderPK, row["AIHP_PK"]);
			AssertEquals("PST", row["AIHP_PostingStatus"]);
		}

		protected override void SetUp()
		{
			CompanyPK = DbHelper.InsertCompany("SSC", "Company", "AUD", "AU", true, true);
			BranchPK = DbHelper.InsertBranch("AZB", CompanyPK);
			DepartmentPK = DbHelper.InsertDepartment("AZD");
			PostDate = DbHelper.ToDate("2020-07-03");
		}

		DataTable RunScript(string pk)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.GetDraftInvoicePostingStatus({pk})");
		}

		Guid CompanyPK;
		Guid BranchPK;
		Guid DepartmentPK;
		DateTime PostDate;
	}
}

