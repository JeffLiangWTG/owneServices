using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HRMRemHistoryBase))]
	class HRMRemHistoryBaseTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var staff0 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();

			var classification0 = Guid.NewGuid();
			var classification1 = Guid.NewGuid();
			var remuneration0 = Guid.NewGuid();
			var remuneration1 = Guid.NewGuid();
			var review0 = Guid.NewGuid();
			var review1 = Guid.NewGuid();
			var workingbasis0 = Guid.NewGuid();
			var workingbasis1 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
VALUES
	(@staff0, 'T00', 'TestStaff00', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'T01', 'TestStaff01', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO hrm.GlbStaffClassification
	(GSL_PK, GSL_GS_Staff, GSL_EffectiveDate, GSL_Classification, GSL_SystemCreateTimeUtc, GSL_SystemLastEditTimeUtc, GSL_SystemCreateUser, GSL_SystemLastEditUser)
VALUES
	(@classification0, @staff0, '2020-01-11 01:00:00 +01:00', 'AC1', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
	(@classification1, @staff1, '2020-04-15 17:00:00 -03:00', 'AC4', GETUTCDATE(), GETUTCDATE(), 'E', 'E');

INSERT INTO hrm.GlbStaffRemuneration
	(GSR_PK, GSR_GS_Staff, GSR_EffectiveDate, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemLastEditTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditUser)
VALUES
	(@remuneration0, @staff0, '2020-01-11 01:00:00 -01:00', 'AU', 'AUD', GETDATE(), GETDATE(), 'ZZN', 'ZZN'),
	(@remuneration1, @staff1, '2021-10-13 16:00:00 +05:00', 'UK', 'GBP', GETDATE(), GETDATE(), 'ZZN', 'ZZN');

INSERT INTO hrm.GlbStaffReview
	(GSV_PK, GSV_GS_Staff, GSV_EffectiveDate, GSV_GS_NKReviewer, GSV_Score, GSV_Comments, GSV_AutoVersion, GSV_SystemCreateUser, GSV_SystemLastEditUser, GSV_SystemCreateTimeUtc, GSV_SystemLastEditTimeUtc)
VALUES
	(@review0, @staff0, '2019-01-15 05:10:00 -01:00', 'RVA', 11, 'Comment 1001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(@review1, @staff1, '2019-07-18 08:40:30 -04:00', 'RVD', 44, 'Comment 4001', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE());

INSERT INTO hrm.GlbStaffWorkingBasis
	(GSW_PK, GSW_GS_Staff, GSW_EffectiveDate, GSW_WorkingBasis, GSW_AutoVersion, GSW_SystemCreateUser, GSW_SystemLastEditUser, GSW_SystemCreateTimeUtc, GSW_SystemLastEditTimeUtc)
VALUES
	(@workingbasis0, @staff0, '2019-01-15 05:10:00 -01:00', 'WB1', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE()),
	(@workingbasis1, @staff1, '2022-01-13 08:50:00 -04:00', 'WB2', 0, 'E', 'E', GETUTCDATE(), GETUTCDATE());
			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@classification0", SqlDbType.UniqueIdentifier, classification0);
				p.AddParameter("@classification1", SqlDbType.UniqueIdentifier, classification1);
				p.AddParameter("@remuneration0", SqlDbType.UniqueIdentifier, remuneration0);
				p.AddParameter("@remuneration1", SqlDbType.UniqueIdentifier, remuneration1);
				p.AddParameter("@review0", SqlDbType.UniqueIdentifier, review0);
				p.AddParameter("@review1", SqlDbType.UniqueIdentifier, review1);
				p.AddParameter("@workingbasis0", SqlDbType.UniqueIdentifier, workingbasis0);
				p.AddParameter("@workingbasis1", SqlDbType.UniqueIdentifier, workingbasis1);
			});

			var results = Execute();
			var dataRows = results.Select();

			var columns = new string[] { "GMR_PK", "GMR_EffectiveDate", "GMR_GS_Staff", "GMR_GSL", "GMR_Classification", "GMR_GSR", "GMR_RN_NKCountry", "GMR_RX_NKCurrency", "GMR_GSV", "GMR_GS_NKReviewer", "GMR_Score", "GMR_GSW", "GMR_WorkingBasis" };
			var expectedValues = new object[,]
			{
				{ $"{staff0}:2019-01-15", new DateTime(2019, 01, 15), staff0, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,  review0, "RVA", (byte)11, workingbasis0, "WB1" },
				{ $"{staff1}:2019-07-18", new DateTime(2019, 07, 18), staff1, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value,  review1, "RVD", (byte)44,DBNull.Value, DBNull.Value },
				{ $"{staff0}:2020-01-11", new DateTime(2020, 01, 11), staff0, classification0, "AC1", remuneration0, "AU", "AUD", review0, "RVA", (byte)11, workingbasis0, "WB1" },
				{ $"{staff1}:2020-04-15", new DateTime(2020, 04, 15), staff1, classification1, "AC4", DBNull.Value, DBNull.Value, DBNull.Value, review1, "RVD", (byte)44,DBNull.Value, DBNull.Value },
				{ $"{staff1}:2021-10-13", new DateTime(2021, 10, 13), staff1, classification1, "AC4", remuneration1, "UK", "GBP", review1, "RVD", (byte)44,DBNull.Value, DBNull.Value },
				{ $"{staff1}:2022-01-13", new DateTime(2022, 01, 13), staff1, classification1, "AC4", remuneration1, "UK", "GBP", review1, "RVD", (byte)44, workingbasis1, "WB2" },
			};

			AssertEquals(columns.Length, results.Columns.Count);
			foreach (var column in columns)
			{
				Assert(results.Columns.Contains(column));
			}

			AssertEquals(expectedValues.GetLength(0), dataRows.Length);
			for (int rowIndex = 0; rowIndex < expectedValues.GetLength(0); rowIndex++)
			{
				var matchedRow = dataRows.FirstOrDefault(r => r["GMR_PK"].ToString().Trim().Equals(expectedValues[rowIndex,0].ToString().ToUpper()));
				AssertNotNull(matchedRow);

				for (int colIndex = 1; colIndex < columns.Length; colIndex++)
				{
					AssertEquals(columns[colIndex], expectedValues[rowIndex, colIndex], matchedRow[columns[colIndex]]);
				}
			}
		}

		DataTable Execute()
		{
			using (var command = TestConnection.Command($"SELECT * FROM [hrm].[HRMRemHistoryBase]"))
			{
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
