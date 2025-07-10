using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(CloseJobInBulk))]
	class CloseJobInBulkTest : DbCreateScriptTest
	{
		public void TestCloseJobInBulkTest_ExcludeJobShouldBeClosedByDsbBatch_EnableBulkDisbursementJobsClosure()
		{
			PrepareJobShouldBeClosedByDsbBatch(out Guid job1, out Guid job2, out Guid job3);

			AssertEquals(1, RunCloseJobInBulk(new List<Guid> { job1, job2, job3 }, DateTime.Now, "USE", true));

			Assert(!Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job1}' AND JH_Status = 'CLS'"));
			Assert(!Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job2}' AND JH_Status = 'CLS'"));
			Assert(Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job3}' AND JH_Status = 'CLS'"));
		}

		public void TestCloseJobInBulkTest_ExcludeJobShouldBeClosedByDsbBatch_NotEnableBulkDisbursementJobsClosure()
		{
			PrepareJobShouldBeClosedByDsbBatch(out Guid job1, out Guid job2, out Guid job3);

			AssertEquals(3, RunCloseJobInBulk(new List<Guid> { job1, job2, job3 }, DateTime.Now, "USE", false));

			Assert(Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job1}' AND JH_Status = 'CLS'"));
			Assert(Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job2}' AND JH_Status = 'CLS'"));
			Assert(Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job3}' AND JH_Status = 'CLS'"));
		}

		void PrepareJobShouldBeClosedByDsbBatch(out Guid job1, out Guid job2, out Guid job3)
		{
			var shipment1 = helper.InsertShipment("S001", new DateTime(2012, 01, 01));
			var shipment2 = helper.InsertShipment("S002", new DateTime(2012, 01, 01));
			var shipment3 = helper.InsertShipment("S003", new DateTime(2012, 01, 01));
			var chargeCode = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var today = DateTime.Today;

			Db.Connection.ExecuteNonQuery($@"UPDATE dbo.AccChargeCode
SET AC_AG_DisbursementSurplusAccount = '{helper.GLAccountPK1}',
AC_AG_DisbursementShortfallAccount = '{helper.GLAccountPK1}',
AC_ChargeType = 'DSB',
AC_SystemLastEditTimeUtc = GETUTCDATE(),
AC_SystemLastEditUser = 'TST'
WHERE AC_PK = '{chargeCode}'");

			job1 = helper.InsertJob("S001", TestDbHelper.DefaultCompanyPK, branch1, department1, "JS", shipment1, "WRK", new DateTime(2012, 07, 25));
			var line1 = Guid.NewGuid();
			var charge1 = Guid.NewGuid();
			InsertTransactionLine(line1, job1, "CST", org1, -100M, branch1, department1, company1, today, chargeCode);
			InsertJobCharge(charge1, job1, branch1, company1, department1, chargeCode, org1, 100M, org1, 100M, "DSB", line1, Guid.Empty);

			job2 = helper.InsertJob("S002", TestDbHelper.DefaultCompanyPK, branch1, department1, "JS", shipment2, "WRK", new DateTime(2012, 07, 25));
			var line2 = Guid.NewGuid();
			var charge2 = Guid.NewGuid();
			InsertTransactionLine(line2, job2, "REV", org1, 100M, branch1, department1, company1, today, chargeCode);
			InsertJobCharge(charge2, job2, branch1, company1, department1, chargeCode, org1, 100M, org1, 100M, "DSB", Guid.Empty, line2);

			job3 = helper.InsertJob("S003", TestDbHelper.DefaultCompanyPK, branch1, department1, "JS", shipment3, "WRK", new DateTime(2012, 07, 25));
			var line3 = Guid.NewGuid();
			var charge3 = Guid.NewGuid();
			InsertTransactionLine(line3, job3, "REV", org1, 100M, branch1, department1, company1, today, chargeCode);
			InsertJobCharge(charge3, job3, branch1, company1, department1, chargeCode, org1, 100M, org1, 100M, "TST", Guid.Empty, line3);

			Assert("Precondition", Db.Connection.Exists($@"FROM ShouldJobBeClosedByDsbBatch('{job1}')"));
			Assert("Precondition", Db.Connection.Exists($@"FROM ShouldJobBeClosedByDsbBatch('{job2}')"));
			Assert("Precondition", !Db.Connection.Exists($@"FROM ShouldJobBeClosedByDsbBatch('{job3}')"));

			Assert("Precondition", !Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job1}' AND JH_Status = 'CLS'"));
			Assert("Precondition", !Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job2}' AND JH_Status = 'CLS'"));
			Assert("Precondition", !Db.Connection.Exists($@"FROM dbo.JobHeader WHERE JH_PK = '{job3}' AND JH_Status = 'CLS'"));
		}

		protected void InsertJobCharge(Guid jobChargePK, Guid jobPK, Guid branchPK, Guid companyPK, Guid departmentPK, Guid chargeCodePK,
			Guid jR_OH_SellAccount, decimal jR_LocalSellAmt, Guid jR_OH_CostAccount, decimal jR_LocalCostAmt, string chargeType,
			Guid jR_AL_APLine, Guid jR_AL_ARLine)
		{
			helper.Insert(JobChargeSchema.Constants.TableName, new
			{
				JR_PK = jobChargePK,
				JR_JH = jobPK,
				JR_GB = branchPK,
				JR_GC = companyPK,
				JR_GE = departmentPK,
				JR_AC = chargeCodePK,
				JR_OH_SellAccount = jR_OH_SellAccount,
				JR_LocalSellAmt = jR_LocalSellAmt,
				JR_OH_CostAccount = jR_OH_CostAccount,
				JR_LocalCostAmt = jR_LocalCostAmt,
				JR_ChargeType = chargeType,
				JR_AL_APLine = jR_AL_APLine != Guid.Empty ? jR_AL_APLine : (object)DBNull.Value,
				JR_AL_ARLine = jR_AL_ARLine != Guid.Empty ? jR_AL_ARLine : (object)DBNull.Value,
			});
		}

		protected void InsertTransactionLine(Guid pK, Guid job, string lineType, Guid org, decimal lineAmount, Guid branch, Guid department, Guid company, DateTime postdate, Guid chargeCode)
		{
			helper.Insert(AccTransactionLinesSchema.Constants.TableName, new
			{
				AL_PK = pK,
				AL_JH = job,
				AL_LineType = lineType,
				AL_OH = org,
				AL_LineAmount = lineAmount,
				AL_GB = branch,
				AL_GE = department,
				AL_PostDate = postdate,
				AL_GC = company,
				AL_AC = chargeCode,
				AL_AG = helper.GLAccountPK1
			});
		}

		protected int RunCloseJobInBulk(List<Guid> jobPKs, DateTime jobCloseDate, string userCode, bool enableBulkDisbursementJobsClosure)
		{
			int result = 0;

			using (var command = Db.Connection.Command("EXEC CloseJobInBulk @JobPKs, @JobCloseDate, @CurrentUserCode, @EnableBulkDisbursementJobsClosure, @JobClosed OUTPUT"))
			{
				command.AddTableValuedParameter("@JobPKs", JobHeaderSchema.PK, jobPKs);
				command.AddParameter("@JobCloseDate", SqlDbType.DateTime, jobCloseDate);
				command.AddParameter("@CurrentUserCode", SqlDbType.VarChar, userCode);
				command.AddParameter("@EnableBulkDisbursementJobsClosure", SqlDbType.Bit, enableBulkDisbursementJobsClosure);
				command.AddOutputParameter("@JobClosed", SqlDbType.Int, 0, 0, 0, 0);
				command.ExecuteNonQuery();
				result = (int)command.GetParameterValue("@JobClosed");
			}

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new TestDbHelper(TestConnection);
			org1 = helper.InsertOrgHeader("ZZC", "Org1");
			org2 = helper.InsertOrgHeader("ZZD", "Org2");
			orgContact1 = helper.InsertOrgContact("OC1", "", false, org1);
			orgContact2 = helper.InsertOrgContact("OC2", "", false, org2);
			company1 = TestDbHelper.DefaultCompanyPK;
			branch1 = helper.InsertBranch("ZZB", company1);
			department1 = helper.InsertDepartment("ZZD");
		}

		protected TestDbHelper helper;
		protected Guid org1, org2, company1, branch1, department1, orgContact1, orgContact2;
	}
}

