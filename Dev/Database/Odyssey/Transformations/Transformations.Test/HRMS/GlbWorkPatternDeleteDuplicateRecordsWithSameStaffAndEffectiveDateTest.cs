using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.HRMS;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.HRMS;

[TestedType(typeof(GlbWorkPatternDeleteDuplicateRecordsWithSameStaffAndEffectiveDate))]
public class GlbWorkPatternDeleteDuplicateRecordsWithSameStaffAndEffectiveDateTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() =>
		new GlbWorkPatternDeleteDuplicateRecordsWithSameStaffAndEffectiveDate();

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

			-- Disable the trigger and index to avoid constraints affecting test data insertion
			DISABLE TRIGGER TG_Maintain_GWP_EffectiveEndDateTrigger ON dbo.GlbWorkPattern;
			ALTER INDEX FK_UX__GWP_GS_Staff_GWP_AutoEffectiveEndDate ON [GlbWorkPattern] DISABLE;

			-- Insert test work patterns for the created staff members
			INSERT INTO [dbo].[GlbWorkPattern] 
				([GWP_PK], [GWP_GS_Staff], [GWP_Name], [GWP_Comment], [GWP_EffectiveDate], [GWP_StandardDuration], 
					[GWP_SystemCreateTimeUtc], [GWP_SystemCreateUser], [GWP_SystemLastEditTimeUtc], [GWP_SystemLastEditUser], [GWP_IsApproved]) 
			VALUES 
				-- Approved records
				(NEWID(), @GS_PK1, 'Standard 9-5', 'Regular 9 AM - 5 PM work shift', '2024-03-01', '08:00:00', '2024-03-01 08:10:00', 'USR', '2024-03-01 08:15:00', 'ADM', 1),
				(NEWID(), @GS_PK1, 'Standard 9-5', 'Regular 9 AM - 5 PM work shift', '2024-03-01', '08:00:00', '2024-03-01 08:10:00', 'USR', '2024-03-01 08:15:00', 'ADM', 1),
				(NEWID(), @GS_PK1, 'Night Shift', 'Work pattern for night shifts (10 PM - 6 AM)', '2024-03-02', '08:00:00', '2024-03-02 22:10:00', 'ADM', '2024-03-02 22:20:00', 'USR', 1),
				(NEWID(), @GS_PK2, 'Flexible Hours', 'Flexible work pattern (core hours 10 AM - 3 PM)', '2024-03-04', '07:00:00', '2024-03-04 10:10:00', 'USR', '2024-03-04 10:20:00', 'SYS', 1),

				-- Duplicate of the approved records but marked as not approved
				(NEWID(), @GS_PK1, 'Standard 9-5', 'Regular 9 AM - 5 PM work shift', '2024-03-01', '08:00:00', '2024-03-01 08:10:00', 'USR', '2024-03-01 08:15:00', 'ADM', 0),
				(NEWID(), @GS_PK1, 'Standard 9-5', 'Regular 9 AM - 5 PM work shift', '2024-03-01', '08:00:00', '2024-03-01 08:10:00', 'USR', '2024-03-01 08:15:00', 'ADM', 0),
				(NEWID(), @GS_PK1, 'Night Shift', 'Work pattern for night shifts (10 PM - 6 AM)', '2024-03-02', '08:00:00', '2024-03-02 22:10:00', 'ADM', '2024-03-02 22:20:00', 'USR', 0),
				(NEWID(), @GS_PK2, 'Flexible Hours', 'Flexible work pattern (core hours 10 AM - 3 PM)', '2024-03-04', '07:00:00', '2024-03-04 10:10:00', 'USR', '2024-03-04 10:20:00', 'SYS', 0);
		";

		_ = TestConnection.ExecuteNonQuery(sqlInsertData);
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("Should have no duplicate", 0, TestConnection.ExecuteScalar<int>("WITH DUP AS (SELECT [GWP_GS_Staff], [GWP_EffectiveDate], COUNT(*) AS C FROM [GlbWorkPattern] WHERE [GWP_IsApproved] = 1 GROUP BY [GWP_GS_Staff], [GWP_EffectiveDate] HAVING COUNT(*) > 1) SELECT COUNT(1) from DUP"));

		AssertGlbWorkPatternData();
	}

	void AssertGlbWorkPatternData()
	{
		var sql = @"
		SELECT 
			s.GS_Code,
			w.GWP_Name,
			w.GWP_Comment,
			w.GWP_EffectiveDate,
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
		AssertEquals(7, dataTable.Rows.Count);

		var expectedRecords = new List<(string staffCode, string name, string comment, string effectiveDate, string standardDuration,
										string createTime, string createUser, string lastEditTime, string lastEditUser, bool isApproved)>
			{
				// Approved records (duplicates removed)
				("Te1", "Night Shift", "Work pattern for night shifts (10 PM - 6 AM)", "2024-03-02", "1/01/1900 8:00:00 AM",
				 "2024-03-02 22:10:00", "ADM", "2024-03-02 22:20:00", "USR", true),

				("Te1", "Standard 9-5", "Regular 9 AM - 5 PM work shift", "2024-03-01", "1/01/1900 8:00:00 AM",
				 "2024-03-01 08:10:00", "USR", "2024-03-01 08:15:00", "ADM", true),

				("Te2", "Flexible Hours", "Flexible work pattern (core hours 10 AM - 3 PM)", "2024-03-04", "1/01/1900 7:00:00 AM",
				 "2024-03-04 10:10:00", "USR", "2024-03-04 10:20:00", "SYS", true),

				// Non-approved records (unchanged)
				("Te1", "Night Shift", "Work pattern for night shifts (10 PM - 6 AM)", "2024-03-02", "1/01/1900 8:00:00 AM",
				 "2024-03-02 22:10:00", "ADM", "2024-03-02 22:20:00", "USR", false),

				("Te1", "Standard 9-5", "Regular 9 AM - 5 PM work shift", "2024-03-01", "1/01/1900 8:00:00 AM",
				 "2024-03-01 08:10:00", "USR", "2024-03-01 08:15:00", "ADM", false),

				("Te1", "Standard 9-5", "Regular 9 AM - 5 PM work shift", "2024-03-01", "1/01/1900 8:00:00 AM",
				 "2024-03-01 08:10:00", "USR", "2024-03-01 08:15:00", "ADM", false),

				("Te2", "Flexible Hours", "Flexible work pattern (core hours 10 AM - 3 PM)", "2024-03-04", "1/01/1900 7:00:00 AM",
				 "2024-03-04 10:10:00", "USR", "2024-03-04 10:20:00", "SYS", false)
			};

		for (var i = 0; i < dataTable.Rows.Count; i++)
		{
			var row = dataTable.Rows[i];
			var expected = expectedRecords[i];

			var staffCode = row["GS_Code"].ToString();
			var name = row["GWP_Name"].ToString();
			var comment = row["GWP_Comment"].ToString();
			var effectiveDate = ((DateTimeOffset)row["GWP_EffectiveDate"]).DateTime.ToString("yyyy-MM-dd");
			var standardDuration = row["GWP_StandardDuration"].ToString();
			var createTime = Convert.ToDateTime(row["GWP_SystemCreateTimeUtc"]).ToString("yyyy-MM-dd HH:mm:ss");
			var createUser = row["GWP_SystemCreateUser"].ToString();
			var lastEditTime = Convert.ToDateTime(row["GWP_SystemLastEditTimeUtc"]).ToString("yyyy-MM-dd HH:mm:ss");
			var lastEditUser = row["GWP_SystemLastEditUser"].ToString();

			var context = $"{staffCode} - {name}";

			CombineAssertions(() =>
			{
				AssertEquals($"Mismatch in staff code for {context}", expected.staffCode, staffCode);
				AssertEquals($"Mismatch in name for {context}", expected.name, name);
				AssertEquals($"Mismatch in comment for {context}", expected.comment, comment);
				AssertEquals($"Mismatch in effective date for {context}", expected.effectiveDate, effectiveDate);
				AssertEquals($"Mismatch in standard duration for {context}", expected.standardDuration, standardDuration);
				AssertEquals($"Mismatch in create time for {context}", expected.createTime, createTime);
				AssertEquals($"Mismatch in create user for {context}", expected.createUser, createUser);
				AssertEquals($"Mismatch in last edit time for {context}", expected.lastEditTime, lastEditTime);
				AssertEquals($"Mismatch in last edit user for {context}", expected.lastEditUser, lastEditUser);
			});
		}
	}
}
