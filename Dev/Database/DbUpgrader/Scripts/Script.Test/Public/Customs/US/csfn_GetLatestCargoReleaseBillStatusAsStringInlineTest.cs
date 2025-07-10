using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetLatestCargoReleaseBillStatusAsStringInline))]
	class csfn_GetLatestCargoReleaseBillStatusAsStringInlineTest : DbCreateScriptTest
	{
		public void TestGetBillStatusWithDescription()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var declaration1PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1);
			var bill1PK = CreateCusDecHouseBill(declaration1PK, "MB00001", 1);
			CreateCusAddInfo(bill1PK, "Code=91*DispositionDate=2018-08-29 13:45:00.000*Order=1*Source=SO");
			AssertBillStatusEquals(1, "91", "91-NO BILL MATCH");

			var declaration2PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2);
			var bill2PK = CreateCusDecHouseBill(declaration2PK, "MB00002", 2);
			CreateCusAddInfo(bill2PK, "Code=91*DispositionDate=2018-08-29 13:45:00.000*Order=1*Source=SO");
			CreateCusAddInfo(bill2PK, "Code=91*DispositionDate=2018-08-29 14:32:00.000*Order=2*Source=SO");
			CreateCusAddInfo(bill2PK, "Code=54*DispositionDate=2018-08-29 14:32:00.000*Order=3*Source=SO");
			AssertBillStatusEquals(2, "54,91", "54-CBP MANIFEST HOLD REMOVED,91-NO BILL MATCH");

			var declaration3PK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3);
			var bill3PK = CreateCusDecHouseBill(declaration3PK, "MB00003", 3);
			CreateCusAddInfo(bill3PK, "Code=91*DispositionDate=2018-01-05 11:43:00.000*Order=1*Source=SO");
			CreateCusAddInfo(bill3PK, "Code=95*DispositionDate=2018-01-15 11:51:00.000*Order=2*Source=SO");
			CreateCusAddInfo(bill3PK, "Code=69*DispositionDate=2018-12-06 22:46:00.000*Order=3*Source=CQ");
			CreateCusAddInfo(bill3PK, "Code=80*DispositionDate=2019-01-15 11:51:00.000*Order=4*Source=SO");
			AssertBillStatusEquals(3, "", "");
		}

		static Guid CreateCusDecHouseBill(Guid declarationPK, string billNumber, int clusterKey)
		{
			var billPK = Guid.NewGuid();
			const string sql = @"
INSERT INTO dbo.CusDecHouseBill(CU_PK, CU_JE, CU_BillNum, CU_BillType, CU_ClusterKey)
VALUES (@billPK, @declarationPK, @billNum, @billType, @clusterKey)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@billPK", SqlDbType.UniqueIdentifier, billPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@billType", SqlDbType.VarChar, CusDecHouseBillSchema.CU_BillType.MaxLength, "MB");
				command.AddParameter("@billNum", SqlDbType.VarChar, CusDecHouseBillSchema.CU_BillNum.MaxLength, billNumber);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return billPK;
		}

		static void CreateCusAddInfo(Guid parentPK, string addinfoData)
		{
			const string sql = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID)
VALUES (@addinfoPK, 'UDP', @addinfoData, 'CU', @parentID)";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@addinfoPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@addinfoData", SqlDbType.VarChar, CusAddInfoSchema.B7_AddInfoData.MaxLength, addinfoData);
				command.ExecuteNonQuery();
			}
		}

		static void AssertBillStatusEquals(int declarationClusterKey, string expectedStatusCode, string expectedStatusDescription)
		{
			const string sql = @"select * from csfn_GetLatestCargoReleaseBillStatusAsStringInline(@declarationClusterKey)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationClusterKey", System.Data.SqlDbType.Int, declarationClusterKey);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(expectedStatusCode, reader["SEBillStatus"].ToString());
					AssertEquals(expectedStatusDescription, reader["SEBillStatusDescription"].ToString());
				}
			}
		}
	}
}
