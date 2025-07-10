using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HRMRemHistoryCompact))]
	class HRMRemHistoryCompactTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var staff0 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();
			var staff2 = Guid.NewGuid();
			var staff3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(@staff0, 'T00', 'Staff00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'T01', 'Staff01', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff2, 'T02', 'Staff02', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff3, 'T03', 'Staff03', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO hrm.GlbStaffClassification
	(GSL_PK, GSL_GS_Staff, GSL_EffectiveDate, GSL_Classification, GSL_SystemCreateTimeUtc, GSL_SystemLastEditTimeUtc, GSL_SystemCreateUser, GSL_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2020-01-11 01:00:00 +01:00', 'AC1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), @staff0, '2020-02-21 11:00:00 +07:00', 'AC2', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), @staff1, '2020-03-02 03:00:00 +11:00', 'AC3', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), @staff1, '2020-04-15 17:00:00 -03:00', 'AC4', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), @staff2, '2020-01-30 02:00:00 +00:00', 'AC5', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), @staff2, '2020-04-22 21:00:00 -05:00', 'AC6', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), @staff3, '2021-05-07 07:00:00 +01:00', 'AC7', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(NEWID(), @staff3, '2021-07-06 14:00:00 -04:00', 'AC8', GETUTCDATE(), GETUTCDATE(), 'E', 'E')

INSERT INTO [hrm].[GlbStaffRemuneration]
	(GSR_PK, GSR_GS_Staff, GSR_EffectiveDate, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemLastEditTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2020-01-11 01:00:00 -01:00', 'AU', 'AUD', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @staff0, '2021-11-01 18:00:00 +02:00', 'US', 'USD', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @staff1, '2020-02-12 03:00:00 -04:00', 'EN', 'EUR', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @staff1, '2021-10-13 16:00:00 +05:00', 'UK', 'GBP', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @staff2, '2020-03-14 05:00:00 -08:00', 'JP', 'JPY', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @staff2, '2021-09-15 14:00:00 +03:00', 'CN', 'RMB', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @staff3, '2020-04-16 07:00:00 +01:00', 'FR', 'EUR', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(NEWID(), @staff3, '2021-08-17 12:00:00 -01:00', 'GE', 'EUR', GETDATE(), GETDATE(), 'ZZN', 'ZZN');

INSERT INTO hrm.GlbStaffReview
	(GSV_PK, GSV_GS_Staff, GSV_EffectiveDate, GSV_GS_NKReviewer, GSV_Score, GSV_Comments, GSV_AutoVersion, GSV_SystemCreateUser, GSV_SystemLastEditUser, GSV_SystemCreateTimeUtc, GSV_SystemLastEditTimeUtc)
VALUES
	(NEWID(), @staff0, '2019-01-15 05:10:00 -01:00', 'RVA', 11, 'Comment 1001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff0, '2020-03-16 06:20:10 -02:00', 'RVB', 22, 'Comment 2001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff0, '2021-05-17 07:30:00 -03:00', 'RVC', 33, 'Comment 3001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff1, '2019-07-18 08:40:30 -04:00', 'RVD', 44, 'Comment 4001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff1, '2020-09-19 09:50:00 -05:00', 'RVE', 55, 'Comment 5001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff1, '2021-11-21 11:00:50 -06:00', 'RVF', 66, 'Comment 6001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff2, '2019-02-22 12:10:00 -07:00', 'RVG', 77, 'Comment 7001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff2, '2020-04-23 13:20:20 -08:00', 'RVH', 88, 'Comment 8001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff2, '2021-06-24 14:30:00 -09:00', 'RVI', 99, 'Comment 9001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff3, '2019-08-05 15:40:40 -10:00', 'RVJ', 12, 'Comment 1001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff3, '2020-10-06 16:50:00 -11:00', 'RVK', 23, 'Comment 2001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff3, '2021-12-07 17:00:10 +00:00', 'RVL', 34, 'Comment 3001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE());

INSERT INTO hrm.GlbStaffWorkingBasis
	(GSW_PK, GSW_GS_Staff, GSW_EffectiveDate, GSW_WorkingBasis, GSW_AutoVersion, GSW_SystemCreateUser, GSW_SystemLastEditUser, GSW_SystemCreateTimeUtc, GSW_SystemLastEditTimeUtc)
VALUES
	(NEWID(), @staff0, '2019-01-15 05:10:00 -01:00', 'WB1', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff0, '2021-11-26 09:30:00 -02:00', 'WB4', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff1, '2019-03-04 07:10:00 +01:00', 'WB3', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(NEWID(), @staff1, '2022-01-13 08:50:00 -04:00', 'WB2', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE());

			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);
				p.AddParameter("@staff3", SqlDbType.UniqueIdentifier, staff3);
			});

			var results = Execute();
			var resultRows = results.Select();

			DataTable baseview;
			using (var command = TestConnection.Command("SELECT GMR_PK, GMR_GS_Staff FROM hrm.HRMRemHistoryBase"))
			{
				baseview = DataUtils.GetDataTableFromCommand(command);
			}

			AssertEquals(2, results.Columns.Count);
			Assert(results.Columns.Contains("GMR_PK"));
			Assert(results.Columns.Contains("GMR_GS_Staff"));

			AssertEquals(baseview.Rows.Count, resultRows.Length);

			for (int i = 0; i < baseview.Rows.Count; i++)
			{
				var matchedRow = resultRows.FirstOrDefault(r => r["GMR_PK"].Equals(baseview.Rows[i]["GMR_PK"]));
				AssertNotNull(matchedRow);

				AssertEquals("GMR_GS_Staff", baseview.Rows[i]["GMR_GS_Staff"], matchedRow["GMR_GS_Staff"]);
			}
		}

		DataTable Execute()
		{
			using (var command = TestConnection.Command($"SELECT * FROM hrm.HRMRemHistoryCompact"))
			{
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
