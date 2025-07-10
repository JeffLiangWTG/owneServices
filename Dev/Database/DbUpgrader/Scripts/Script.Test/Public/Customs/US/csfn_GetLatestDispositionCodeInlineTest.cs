using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(csfn_GetLatestDispositionCodeInline))]
	class csfn_GetLatestDispositionCodeInlineTest : DbCreateScriptTest
	{
		public void Testcsfn_GetLatestDispositionCodeInline()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var headerPK = Guid.NewGuid();
			CreateCusInbondHeader(headerPK, "AMS112233", branchPK);

			var billPK1 = Guid.NewGuid();
			CreateCusInBondBill(billPK1, headerPK);
			CreateCusAddInfo(billPK1, "01", "1", new DateTime(2023, 12, 1));
			CreateCusAddInfo(billPK1, "02", "2", new DateTime(2023, 12, 2));
			GetAndAssertLatestDispositionCode(billPK1, "", "02");
			GetAndAssertLatestDispositionCode(billPK1, "01", "01");
			GetAndAssertLatestDispositionCode(billPK1, "01,02", "02");

			var billPK2 = Guid.NewGuid();
			CreateCusInBondBill(billPK2, headerPK);
			CreateCusAddInfo(billPK2, "01", "1", new DateTime(2023, 12, 1));
			CreateCusAddInfo(billPK2, "02", "2", new DateTime(2023, 12, 2));
			CreateCusAddInfo(billPK2, "03", "3", new DateTime(2023, 12, 3));
			GetAndAssertLatestDispositionCode(billPK2, "", "03");
			GetAndAssertLatestDispositionCode(billPK2, "02", "02");
			GetAndAssertLatestDispositionCode(billPK2, "01", "01");
			GetAndAssertLatestDispositionCode(billPK2, "01,02", "02");
			GetAndAssertLatestDispositionCode(billPK2, "02,03", "03");

			var billPK3 = Guid.NewGuid();
			CreateCusInBondBill(billPK3, headerPK);
			CreateCusAddInfo(billPK3, "01", "1", new DateTime(2023, 12, 1));
			CreateCusAddInfo(billPK3, "09", "9", new DateTime(2023, 12, 2));
			CreateCusAddInfo(billPK3, "10", "10", new DateTime(2023, 12, 3));
			CreateCusAddInfo(billPK3, "11", "11", new DateTime(2023, 12, 4));
			GetAndAssertLatestDispositionCode(billPK3, "", "11");

			var billPK4 = Guid.NewGuid();
			CreateCusInBondBill(billPK4, headerPK);
			CreateCusAddInfo(billPK4, "01", "1", new DateTime(2023, 12, 1));
			CreateCusAddInfo(billPK4, "02", "2", new DateTime(2023, 12, 2));
			CreateCusAddInfo(billPK4, "03", "3", new DateTime(2023, 12, 1));
			GetAndAssertLatestDispositionCode(billPK4, "", "02");
			GetAndAssertLatestDispositionCode(billPK4, "01,03", "03");

			var billPK5 = Guid.NewGuid();
			CreateCusInBondBill(billPK5, headerPK);
			CreateCusAddInfo(billPK5, "01", "1", new DateTime(2023, 12, 1));
			CreateCusAddInfo(billPK5, "02", "1", new DateTime(2023, 12, 2));
			GetAndAssertLatestDispositionCode(billPK5, "", "02");
		}

		void CreateCusInbondHeader(Guid headerPK, string jobNumber, Guid branchPK)
		{
			var sql = @"
INSERT INTO CusInbondHeader
(BH_PK, BH_JobReference, BH_GB, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_ApplicationCode)
VALUES
(@BH_PK, @BH_JobReference, @BH_GB, GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'AMS')
";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@BH_PK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@BH_JobReference", SqlDbType.VarChar, CusInBondHeaderSchema.BH_JobReference.MaxLength, jobNumber);
				command.AddParameter("@BH_GB", SqlDbType.UniqueIdentifier, branchPK);
				command.ExecuteNonQuery();
			}
		}

		void CreateCusInBondBill(Guid billPK, Guid inBondHeaderPK)
		{
			var sql = @"
INSERT INTO CusInBondBill (B0_PK, B0_BH, B0_ShipmentType, B0_SystemCreateTimeUtc, B0_SystemCreateUser, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser)
VALUES (@cusInBondBillPK, @inBondHeaderPK, 'IMP', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusInBondBillPK", SqlDbType.UniqueIdentifier, billPK);
				command.AddParameter("@inBondHeaderPK", SqlDbType.UniqueIdentifier, inBondHeaderPK);
				command.ExecuteNonQuery();
			}
		}

		void CreateCusAddInfo(Guid parentPK, string dispositionCode, string order, DateTime dateTime)
		{
			const string sql = @"
INSERT INTO CusAddInfo(B7_PK, B7_Type, B7_AddInfoData, B7_ParentTableCode, B7_ParentID)
VALUES (@addinfoPK, 'UDP', @addinfoData, 'B0', @parentID)
INSERT INTO GenAddOnColumn (XA_PK, XA_ParentID, XA_ParentTableCode, XA_Type, XA_Name, XA_Data)
VALUES(NEWID(), @addinfoPK, 'B7', 'STR', 'US_Code', @data)";

			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@addinfoPK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@addinfoData", SqlDbType.VarChar, $"Code={dispositionCode}*DispositionDate={dateTime.ToString("yyyy-MM-dd HH:mm:ss tt")}*Order={order}");
				command.AddParameter("@data", SqlDbType.VarChar, dispositionCode);
				command.ExecuteNonQuery();
			}
		}

		void GetAndAssertLatestDispositionCode(Guid parentPK, string codesNeedToBeFiltered, string expectedValue)
		{
			var sql = @"
SELECT DispositionCode FROM csfn_GetLatestDispositionCodeInline(@parentPK, @codesNeedToBeFiltered)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@codesNeedToBeFiltered", SqlDbType.VarChar, codesNeedToBeFiltered);
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(expectedValue, reader["DispositionCode"].ToString());
					}
					AssertEquals(1, count);
				}
			}
		}
	}
}
