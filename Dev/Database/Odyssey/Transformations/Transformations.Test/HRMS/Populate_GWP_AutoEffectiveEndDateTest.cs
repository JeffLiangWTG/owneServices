using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.HRMS;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.HRMS
{
	[TestedType(typeof(Populate_GWP_AutoEffectiveEndDate))]
	public class Populate_GWP_AutoEffectiveEndDateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() =>
			new Populate_GWP_AutoEffectiveEndDate();

		protected override void PrepareTestData()
		{
			var sqlInsertData = @"
				-- Generate unique identifiers for two test staff records
				DECLARE @GS_PK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @GS_PK2 UNIQUEIDENTIFIER = NEWID();

				-- Insert test staff data into the GlbStaff table
				INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser])
				VALUES
					(@GS_PK1, 'Te1', 'StaffTest1', GETDATE(), 'TU1', GETDATE(), 'TU2'),
					(@GS_PK2, 'Te2', 'StaffTest2', GETDATE(), 'TU1', GETDATE(), 'TU2');

				-- Disable trigger and index to prevent constraints from interfering with test data insertion
				DISABLE TRIGGER TG_Maintain_GWP_EffectiveEndDateTrigger ON dbo.GlbWorkPattern;
				ALTER INDEX FK_UX__GWP_GS_Staff_GWP_AutoEffectiveEndDate ON [GlbWorkPattern] DISABLE;

				-- Insert test work patterns for the created staff members
				INSERT INTO [dbo].[GlbWorkPattern] 
					([GWP_PK], [GWP_GS_Staff], [GWP_Name], [GWP_Comment], [GWP_EffectiveDate], [GWP_StandardDuration], 
					 [GWP_SystemCreateTimeUtc], [GWP_SystemCreateUser], [GWP_SystemLastEditTimeUtc], [GWP_SystemLastEditUser], [GWP_IsApproved]) 
				VALUES 
					-- Approved work patterns (GWP_IsApproved = 1)
					(NEWID(), @GS_PK1, 'Standard 9-5 1', 'Regular 9 AM - 5 PM work shift', '2024-03-01', '08:00:00', '2024-03-01 08:10:00', 'USR', '2024-03-01 08:15:00', 'ADM', 1),
					(NEWID(), @GS_PK1, 'Night Shift 1', 'Work pattern for night shifts (10 PM - 6 AM)', '2024-03-02', '08:00:00', '2024-03-02 22:10:00', 'ADM', '2024-03-02 22:20:00', 'USR', 1),
					(NEWID(), @GS_PK1, 'Night Shift 2', 'Work pattern for night shifts (10 PM - 6 AM)', '2024-03-03', '08:00:00', '2024-03-02 22:10:00', 'ADM', '2024-03-02 22:20:00', 'USR', 1),
					(NEWID(), @GS_PK2, 'Weekend Shift 1', 'Saturday and Sunday work schedule', '2024-03-04', '06:00:00', '2024-03-03 09:05:00', 'SYS', '2024-03-03 09:10:00', 'ADM', 1),
					(NEWID(), @GS_PK2, 'Flexible Hours 1', 'Flexible work pattern (core hours 10 AM - 3 PM)', '2024-03-05', '07:00:00', '2024-03-04 10:10:00', 'USR', '2024-03-04 10:20:00', 'SYS', 1),
					(NEWID(), @GS_PK2, 'Weekend Shift 2', 'Saturday and Sunday work schedule', '2024-03-06', '06:00:00', '2024-03-03 09:05:00', 'SYS', '2024-03-03 09:10:00', 'ADM', 1),

					-- Duplicate records but marked as not approved (GWP_IsApproved = 0)
					(NEWID(), @GS_PK1, 'Standard 9-5 2', 'Regular 9 AM - 5 PM work shift', '2024-03-01', '08:00:00', '2024-03-01 08:10:00', 'USR', '2024-03-01 08:15:00', 'ADM', 0),
					(NEWID(), @GS_PK1, 'Night Shift 3', 'Work pattern for night shifts (10 PM - 6 AM)', '2024-03-02', '08:00:00', '2024-03-02 22:10:00', 'ADM', '2024-03-02 22:20:00', 'USR', 0),
					(NEWID(), @GS_PK1, 'Night Shift 4', 'Work pattern for night shifts (10 PM - 6 AM)', '2024-03-03', '08:00:00', '2024-03-02 22:10:00', 'ADM', '2024-03-02 22:20:00', 'USR', 0),
					(NEWID(), @GS_PK2, 'Weekend Shift 3', 'Saturday and Sunday work schedule', '2024-03-04', '06:00:00', '2024-03-03 09:05:00', 'SYS', '2024-03-03 09:10:00', 'ADM', 0),
					(NEWID(), @GS_PK2, 'Flexible Hours 2', 'Flexible work pattern (core hours 10 AM - 3 PM)', '2024-03-05', '07:00:00', '2024-03-04 10:10:00', 'USR', '2024-03-04 10:20:00', 'SYS', 0),
					(NEWID(), @GS_PK2, 'Weekend Shift 4', 'Saturday and Sunday work schedule', '2024-03-06', '06:00:00', '2024-03-03 09:05:00', 'SYS', '2024-03-03 09:10:00', 'ADM', 0);
		";
			_ = TestConnection.ExecuteNonQuery(sqlInsertData);

			beforeUpdateTime = GetSQLServerUTCDATE();
		}

		DateTime GetSQLServerUTCDATE() => (DateTime)TestConnection.ExecuteScalar("SELECT CAST(GETUTCDATE() AS SMALLDATETIME) AS CurrentUTCTimeSmalldatetime");

		DateTime beforeUpdateTime;

		protected override void AssertTransformationResults()
		{
			var sql = @"
		SELECT 
			s.GS_Code,
			w.GWP_Name,
			w.GWP_Comment,
			w.GWP_EffectiveDate,
			w.GWP_AutoEffectiveEndDate,
			w.GWP_StandardDuration,
			w.GWP_SystemCreateTimeUtc,
			w.GWP_SystemCreateUser,
			w.GWP_SystemLastEditTimeUtc,
			w.GWP_SystemLastEditUser,
			w.GWP_IsApproved
		FROM dbo.GlbWorkPattern w
		INNER JOIN dbo.GlbStaff s ON w.GWP_GS_Staff = s.GS_PK
		ORDER BY w.GWP_IsApproved DESC, s.GS_Code, w.GWP_EffectiveDate DESC
;";

			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(12, dataTable.Rows.Count);

			var utcNow = GetSQLServerUTCDATE();

			var expectedRecords = new List<(string staffCode, string name, string comment, string effectiveDate, string autoeffectiveEndDate, string standardDuration,
											string createTime, string createUser, string lastEditTime, string lastEditUser, bool isApproved)>
			{
				// Approved records for staff Te1
				("Te1", "Night Shift 2", "Work pattern for night shifts (10 PM - 6 AM)", "2024-03-03", null, "1/01/1900 8:00:00 AM",
					"2024-03-02 22:10:00", "ADM", "2024-03-02 22:20:00", "USR", true),

				("Te1", "Night Shift 1", "Work pattern for night shifts (10 PM - 6 AM)", "2024-03-02", "2024-03-03", "1/01/1900 8:00:00 AM",
					"2024-03-02 22:10:00", "ADM", "2024-03-02 22:20:00", "~BP", true),

				("Te1", "Standard 9-5 1", "Regular 9 AM - 5 PM work shift", "2024-03-01", "2024-03-02", "1/01/1900 8:00:00 AM",
					"2024-03-01 08:10:00", "USR", "2024-03-01 08:15:00", "~BP", true),

				// Approved records for staff Te2
				("Te2", "Weekend Shift 2", "Saturday and Sunday work schedule", "2024-03-06", null, "1/01/1900 6:00:00 AM",
					"2024-03-03 09:05:00", "SYS", "2024-03-03 09:10:00", "ADM", true),

				("Te2", "Flexible Hours 1", "Flexible work pattern (core hours 10 AM - 3 PM)", "2024-03-05", "2024-03-06", "1/01/1900 7:00:00 AM",
					"2024-03-04 10:10:00", "USR", "2024-03-04 10:20:00", "~BP", true),

				("Te2", "Weekend Shift 1", "Saturday and Sunday work schedule", "2024-03-04", "2024-03-05", "1/01/1900 6:00:00 AM",
					"2024-03-03 09:05:00", "SYS", "2024-03-03 09:10:00", "~BP", true),

				// Non-approved records for staff Te1
				("Te1", "Night Shift 4", "Work pattern for night shifts (10 PM - 6 AM)", "2024-03-03", null, "1/01/1900 8:00:00 AM",
					"2024-03-02 22:10:00", "ADM", "2024-03-02 22:20:00", "USR", false),

				("Te1", "Night Shift 3", "Work pattern for night shifts (10 PM - 6 AM)", "2024-03-02", null, "1/01/1900 8:00:00 AM",
					"2024-03-02 22:10:00", "ADM", "2024-03-02 22:20:00", "USR", false),

				("Te1", "Standard 9-5 2", "Regular 9 AM - 5 PM work shift", "2024-03-01", null, "1/01/1900 8:00:00 AM",
					"2024-03-01 08:10:00", "USR", "2024-03-01 08:15:00", "ADM", false),

				// Non-approved records for staff Te2
				("Te2", "Weekend Shift 4", "Saturday and Sunday work schedule", "2024-03-06", null, "1/01/1900 6:00:00 AM",
					"2024-03-03 09:05:00", "SYS", "2024-03-03 09:10:00", "ADM", false),

				("Te2", "Flexible Hours 2", "Flexible work pattern (core hours 10 AM - 3 PM)", "2024-03-05", null, "1/01/1900 7:00:00 AM",
					"2024-03-04 10:10:00", "USR", "2024-03-04 10:20:00", "SYS", false),

				("Te2", "Weekend Shift 3", "Saturday and Sunday work schedule", "2024-03-04", null, "1/01/1900 6:00:00 AM",
					"2024-03-03 09:05:00", "SYS", "2024-03-03 09:10:00", "ADM", false),
			};

			for (var i = 0; i < dataTable.Rows.Count; i++)
			{
				var row = dataTable.Rows[i];
				var expected = expectedRecords[i];

				var staffCode = row["GS_Code"].ToString();
				var name = row["GWP_Name"].ToString();
				var comment = row["GWP_Comment"].ToString();
				var effectiveDate = ((DateTimeOffset)row["GWP_EffectiveDate"]).DateTime.ToString("yyyy-MM-dd");
				var autoEffectiveEndDate = row["GWP_AutoEffectiveEndDate"] == DBNull.Value ? null : ((DateTimeOffset)row["GWP_AutoEffectiveEndDate"]).DateTime.ToString("yyyy-MM-dd");
				var standardDuration = row["GWP_StandardDuration"].ToString();
				var createTime = Convert.ToDateTime(row["GWP_SystemCreateTimeUtc"]).ToString("yyyy-MM-dd HH:mm:ss");
				var createUser = row["GWP_SystemCreateUser"].ToString();
				var lastEditTime = Convert.ToDateTime(row["GWP_SystemLastEditTimeUtc"]);
				var lastEditUser = row["GWP_SystemLastEditUser"].ToString();
				var isApproved = (bool)row["GWP_IsApproved"];

				var context = $"{staffCode} - {name}";

				CombineAssertions(() =>
				{
					AssertEquals($"Mismatch in staff code for {context}", expected.staffCode, staffCode);
					AssertEquals($"Mismatch in name for {context}", expected.name, name);
					AssertEquals($"Mismatch in comment for {context}", expected.comment, comment);
					AssertEquals($"Mismatch in effective date for {context}", expected.effectiveDate, effectiveDate);
					AssertEquals($"Mismatch in auto effective end date for {context}", expected.autoeffectiveEndDate, autoEffectiveEndDate);
					AssertEquals($"Mismatch in standard duration for {context}", expected.standardDuration, standardDuration);
					AssertEquals($"Mismatch in create time for {context}", expected.createTime, createTime);
					AssertEquals($"Mismatch in create user for {context}", expected.createUser, createUser);
					AssertEquals($"Mismatch in last edit user for {context}", expected.lastEditUser, lastEditUser);
					AssertEquals($"Mismatch in isApproved for {context}", expected.isApproved, isApproved);

					if (isApproved && autoEffectiveEndDate is not null)
					{
						Assert($"Mismatch in last edit time for {context}",
							lastEditTime <= utcNow && lastEditTime >= beforeUpdateTime);
					}
					else
					{
						AssertEquals($"Mismatch in last edit time for {context}",
							expected.lastEditTime, lastEditTime.ToString("yyyy-MM-dd HH:mm:ss"));
					}
				});
			}
		}

		protected override void TearDown()
		{
			_ = TestConnection.ExecuteNonQuery(@"
ALTER INDEX FK_UX__GWP_GS_Staff_GWP_AutoEffectiveEndDate ON [GlbWorkPattern] REBUILD;
ENABLE TRIGGER TG_Maintain_GWP_EffectiveEndDateTrigger ON dbo.GlbWorkPattern;");

			base.TearDown();
		}
	}
}
