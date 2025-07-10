using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HRMRemHistory))]
	class HRMRemHistoryTest : DbCreateScriptTest
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

INSERT INTO dbo.GlbStaffManager
	(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
VALUES
	(newid(), 'PPL', @staff0, @staff1, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'HRM', @staff0, @staff2, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff0, @staff3, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E');

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
			using (var command = TestConnection.Command("SELECT * FROM hrm.HRMRemHistoryBase"))
			{
				baseview = DataUtils.GetDataTableFromCommand(command);
			}

			var hrmColumns = new[] { "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3" };

			AssertEquals(baseview.Columns.Count + hrmColumns.Length, results.Columns.Count);
			foreach (var column in baseview.Columns)
			{
				Assert(results.Columns.Contains((column as DataColumn).ColumnName));
			}
			foreach (var colName in hrmColumns)
			{
				Assert(results.Columns.Contains(colName));
			}

			var flags = new[] { false, true };
			var securityColumnValues =
				from isSelf in flags
				from isManaged1 in flags
				from isManaged2 in flags
				from isManaged3 in flags
				select new object[] { isSelf, isManaged1, isManaged2, isManaged3 };

			foreach (var baseviewRow in baseview.Select())
			{
				var matchedRows = resultRows.Where(r => r["GMR_PK"].Equals(baseviewRow["GMR_PK"]));
				AssertEquals(16, matchedRows.Count());

				matchedRows.ForEach(row =>
				{
					foreach (var col in baseview.Columns)
					{
						var colName = (col as DataColumn).ColumnName;
						AssertEquals(colName, baseviewRow[colName], row[colName]);
					}
				});

				foreach (var securityValues in securityColumnValues)
				{
					var matchedRow = matchedRows.
						FirstOrDefault(r => r["IsSelf"].Equals(securityValues[0]) &&
											r["IsManaged1"].Equals(securityValues[1]) &&
											r["IsManaged2"].Equals(securityValues[2]) &&
											r["IsManaged3"].Equals(securityValues[3]));

					AssertNotNull(matchedRow);
				}
			}
		}

		public void TestSecurityColumnTypesNotNull()
		{
			var securityColumns = HRSecurityColumns.GetColumnNames();

			DataTable result;
			using (var command = TestConnection.Command($@"
SELECT 
    c.name AS column_name,
    type_name(user_type_id) AS data_type,
    c.is_nullable
FROM 
    sys.columns c
    JOIN sys.views v ON v.object_id = c.object_id
WHERE 
    schema_name(v.schema_id) = 'hrm'
    AND object_name(c.object_id) = 'HRMRemHistory'
	AND c.name IN ('{string.Join("', '", securityColumns)}')
"))
			{
				result = DataUtils.GetDataTableFromCommand(command);
			}
			var valueArrays = result.Rows.Cast<DataRow>().Select(r => r.ItemArray).ToArray();

			AssertContainsExactElementsInAnyOrder(securityColumns, valueArrays.Select(item => item[0]).ToArray());
			Assert("All column types are BIT", valueArrays.All(item => ((string)item[1]) == "bit"));
			Assert("All columns are NOT NULL", valueArrays.All(item => !((bool)item[2])));
		}

		DataTable Execute()
		{
			using (var command = TestConnection.Command($"SELECT * FROM hrm.HRMRemHistory"))
			{
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
