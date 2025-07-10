using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing;

[TestedType(typeof(BackFillMissingDetachedAttachedLogForGlbStaffAndGlbGroup))]
public class BackFillMissingDetachedAttachedLogForGlbStaffAndGlbGroupTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new BackFillMissingDetachedAttachedLogForGlbStaffAndGlbGroup();
	}

	protected override void PrepareTestData()
	{
		var helper = new TransformationTestDataCreator();
		var groupPK =	helper.CreateGlbGroup("EDITST", "STF", "EDI TEST");
		var staffPK = helper.CreateStaff("TEST", "TST");
		CreateGlbGroupLink(groupPK, staffPK, DateTime.UtcNow);
	}

	public void TestNoAttachedLogRecordWithGK_SystemCreateTimeUtc()
	{
		var result = PrepareTestDataForTestNoAttachedLogRecordWithGK_SystemCreateTimeUtc();
		GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		AssertTestNoAttachedLogRecord(result.groupPK, result.staffPK, result.createTime);
	}

	public void TestNoAttachedLogRecordWithDetachedLogPostTimeUtc()
	{
		var result = PrepareTestNoAttachedLogRecordWithDetachedLogPostTimeUtc();
		GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		AssertTestNoAttachedLogRecord(result.groupPK, result.staffPK, result.createTime.AddMinutes(1));
	}

	public void TestNoAttachedLogRecordWithNullDetachedLogAndNullGK_SystemCreateTimeUtc()
	{
		var result = PrepareTestNoAttachedLogRecordWithNullDetachedLogAndNullGK_SystemCreateTimeUtc();
		GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		AssertTestNoAttachedLogRecord(result.groupPK, result.staffPK, result.createTime);
	}

	public void TestLatestLogIsDetachedRecordWithGK_SystemCreateTimeUtc()
	{
		var result = PrepareTestLatestLogIsDetachedRecordWithGK_SystemCreateTimeUtc();
		GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		AssertTestNoAttachedLogRecord(result.groupPK, result.staffPK, result.createTime);
	}

	public void TestLatestLogIsDetachedRecordWithDetachedLogPostTimeUtc()
	{
		var result = PrepareTestLatestLogIsDetachedRecordWithDetachedLogPostTimeUtc();
		GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		AssertTestNoAttachedLogRecord(result.groupPK, result.staffPK, result.createTime.AddMinutes(1));
	}

	public void TestProcessOnlyHaveDetachedLogs()
	{
		var result = PrepareTestProcessOnlyHaveDetachedLogs();
		GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		AssertLogRecord("Attached - (EDITST) EDI TEST (Calculated)", result.staffPK, result.createTime);
	}

	public void TestAllDataShouldBeProcessed()
	{
		var helper = new TransformationTestDataCreator();

		for (int i = 0; i < 20; i++)
		{
			var groupPK =	helper.CreateGlbGroup($"EDITS{i}", "STF", "EDI TEST");
			var staffPK = helper.CreateStaff($"TEST{i}", $"~{i}");
			CreateGlbGroupLink(groupPK, staffPK, DateTime.UtcNow);
		}
		GetNewTestTransformationInstance().Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
		AssertEquals(0, Db.Connection.ExecuteScalar<int>(TestNoAttachedLogRecordSql));
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals(0, Db.Connection.ExecuteScalar<int>(TestNoAttachedLogRecordSql));
		AssertEquals(0, Db.Connection.ExecuteScalar<int>(TestLatestLogIsDetachedRecordSql));
		AssertEquals(0, Db.Connection.ExecuteScalar<int>(TestOnlyHaveDetachedLogRecordSql));
	}

	(Guid groupPK, Guid staffPK, DateTime createTime) PrepareTestDataForTestNoAttachedLogRecordWithGK_SystemCreateTimeUtc()
	{
		var glbGroupAndStaff = CreateGlbGroupAndStaff();

		var dateTime = new DateTime(2020, 1, 1);
		CreateGlbGroupLink(glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, dateTime);
		return (glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, dateTime);
	}

	(Guid groupPK, Guid staffPK, DateTime createTime) PrepareTestNoAttachedLogRecordWithDetachedLogPostTimeUtc()
	{
		var glbGroupAndStaff = CreateGlbGroupAndStaff();
		var helper = new TransformationTestDataCreator();
		CreateGlbGroupLink(glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, DateTime.MinValue);

		var dateTime = new DateTime(2014, 11, 21, 11, 30, 0);
		helper.CreateStmALog("GlbStaff", glbGroupAndStaff.staffPK, "E", "EDT", "Detached - (EDITST) EDI TEST", dateTime, dateTime);
		helper.CreateStmALog("GlbGroup", glbGroupAndStaff.groupPK, "E", "EDT", "Detached - (TST)", dateTime, dateTime);

		return (glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, dateTime);
	}

	(Guid groupPK, Guid staffPK, DateTime createTime) PrepareTestNoAttachedLogRecordWithNullDetachedLogAndNullGK_SystemCreateTimeUtc()
	{
		var dateTime = new DateTime(2024, 1, 1);
		var glbGroupAndStaff = CreateGlbGroupAndStaff(dateTime, dateTime.AddDays(-1));
		CreateGlbGroupLink(glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, DateTime.MinValue);
		return (glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, dateTime);
	}

	(Guid groupPK, Guid staffPK, DateTime createTime) PrepareTestLatestLogIsDetachedRecordWithGK_SystemCreateTimeUtc()
	{
		var glbGroupAndStaff = CreateGlbGroupAndStaff();
		var createTime = new DateTime(2020, 1, 1);
		CreateGlbGroupLink(glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, createTime);
		var dateTime = new DateTime(2014, 11, 21, 11, 30, 0);
		var helper = new TransformationTestDataCreator();
		helper.CreateStmALog("GlbStaff", glbGroupAndStaff.staffPK, "E", "EDT", "Detached - (EDITST) EDI TEST", dateTime, dateTime);
		helper.CreateStmALog("GlbGroup", glbGroupAndStaff.groupPK, "E", "EDT", "Detached - (TST)", dateTime, dateTime);
		helper.CreateStmALog("GlbStaff", glbGroupAndStaff.staffPK, "E", "EDT", "Attached - (EDITST) EDI TEST", dateTime.AddDays(-1), dateTime.AddDays(-1));
		helper.CreateStmALog("GlbGroup", glbGroupAndStaff.groupPK, "E", "EDT", "Attached - (TST)", dateTime.AddDays(-1), dateTime.AddDays(-1));

		return (glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, createTime);
	}

	(Guid groupPK, Guid staffPK, DateTime createTime) PrepareTestLatestLogIsDetachedRecordWithDetachedLogPostTimeUtc()
	{
		var glbGroupAndStaff = CreateGlbGroupAndStaff();
		CreateGlbGroupLink(glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, DateTime.MinValue);
		var dateTime = new DateTime(2014, 11, 21, 11, 30, 0);
		var helper = new TransformationTestDataCreator();
		helper.CreateStmALog("GlbStaff", glbGroupAndStaff.staffPK, "E", "EDT", "Detached - (EDITST) EDI TEST", dateTime, dateTime);
		helper.CreateStmALog("GlbGroup", glbGroupAndStaff.groupPK, "E", "EDT", "Detached - (TST)", dateTime, dateTime);
		helper.CreateStmALog("GlbStaff", glbGroupAndStaff.staffPK, "E", "EDT", "Attached - (EDITST) EDI TEST", dateTime.AddDays(-1), dateTime.AddDays(-1));
		helper.CreateStmALog("GlbGroup", glbGroupAndStaff.groupPK, "E", "EDT", "Attached - (TST)", dateTime.AddDays(-1), dateTime.AddDays(-1));

		return (glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, dateTime);
	}

	(Guid groupPk, Guid staffPK, DateTime createTime) PrepareTestProcessOnlyHaveDetachedLogs()
	{
		var helper = new TransformationTestDataCreator();
		var glbGroupAndStaff = CreateGlbGroupAndStaff();
		var dateTime = new DateTime(2020, 1, 1);
		helper.UpdateTableColumn("GlbStaff", "GS_SystemCreateTimeUtc", dateTime, SqlDbType.DateTime, "GS_PK", glbGroupAndStaff.staffPK);
		helper.UpdateTableColumn("GlbGroup", "GG_SystemCreateTimeUtc", dateTime.AddDays(-1), SqlDbType.DateTime, "GG_PK", glbGroupAndStaff.groupPK);

		helper.CreateStmALog("GlbStaff", glbGroupAndStaff.staffPK, "E", "EDT", "Detached - (EDITST) EDI TEST", DateTime.UtcNow, DateTime.UtcNow);
		helper.CreateStmALog("GlbGroup", glbGroupAndStaff.groupPK, "E", "EDT", "Detached - (TST)", DateTime.UtcNow, DateTime.UtcNow);

		return (glbGroupAndStaff.groupPK, glbGroupAndStaff.staffPK, dateTime);
	}

	void AssertTestNoAttachedLogRecord(Guid groupPK, Guid staffPK, DateTime createTime)
	{
		AssertLogRecord("Attached - (TST) (Calculated)", groupPK, createTime);
		AssertLogRecord("Attached - (EDITST) EDI TEST (Calculated)", staffPK, createTime);
	}

	(Guid groupPK, Guid staffPK) CreateGlbGroupAndStaff()
	{
		var helper = new TransformationTestDataCreator();
		var groupPK =	helper.CreateGlbGroup("EDITST", "STF", "EDI TEST");
		var staffPK = helper.CreateStaff("TEST", "TST");
		return (groupPK, staffPK);
	}

	(Guid groupPK, Guid staffPK) CreateGlbGroupAndStaff(DateTime createTimeForStaff, DateTime createTimeForGroup)
	{
		var helper = new TransformationTestDataCreator();
		var groupPK =	helper.CreateGlbGroup("EDITST", "STF", "EDI TEST");
		var staffPK = helper.CreateStaff("TEST", "TST");
		helper.UpdateTableColumn("GlbGroup", "GG_SystemCreateTimeUtc", createTimeForGroup, SqlDbType.DateTime, "GG_PK", groupPK);
		helper.UpdateTableColumn("GlbStaff", "GS_SystemCreateTimeUtc", createTimeForStaff, SqlDbType.DateTime, "GS_PK", staffPK);
		return (groupPK, staffPK);
	}
	void AssertLogRecord(string expectedReference, Guid pk, DateTime expectedPostTimeUtc)
	{
		string sql = "SELECT SL_Reference, SL_PostedTimeUtc FROM dbo.StmALog WHERE SL_Parent = @PK AND SL_Reference LIKE '%Calculated%'";
		string reference = "";
		DateTime postTimeUtc = DateTime.MinValue;

		using var cmd = Db.Connection.Command(sql);
		{
			cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);

			using var reader = cmd.ExecuteReader();
			if (reader.Read())
			{
				reference = (string)reader["SL_Reference"];
				postTimeUtc = (DateTime)reader["SL_PostedTimeUtc"];
			}
		}
		AssertEquals(expectedReference, reference);
		AssertEquals(expectedPostTimeUtc, postTimeUtc);
	}

	void CreateGlbGroupLink(Guid groupPK, Guid staffPK, DateTime createTimeUtc)
	{
		var pk = Guid.NewGuid();
		using (DataTransformationHelper.SuspendInsertAuditTriggerIfExists("GlbGroupLink"))
		using (var command = Db.Connection.Command(CreateGlbGroupLinkSql))
		{
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@groupPk", SqlDbType.UniqueIdentifier, groupPK);
			command.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK);
			command.AddParameter("@CreateUser", SqlDbType.VarChar, "");
			command.AddParameter("@CreateTimeUtc", SqlDbType.DateTime, createTimeUtc == DateTime.MinValue ? DBNull.Value : createTimeUtc);
			command.ExecuteNonQuery();
		}
	}

	const string CreateGlbGroupLinkSql = @"INSERT INTO dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS, GK_SystemCreateTimeUtc, GK_SystemCreateUser, GK_SystemLastEditTimeUtc, GK_SystemLastEditUser)
VALUES (@pk, @groupPk, @staffPk, @CreateTimeUtc, @CreateUser, GetUtcDate(), '~BP')";

	const string TestNoAttachedLogRecordSql = @"SELECT COUNT(1) 
FROM dbo.GlbGroupLink link
JOIN
    dbo.GlbStaff staff ON link.GK_GS = staff.GS_PK
    AND staff.GS_IsSystemAccount = 0
JOIN
    dbo.GlbGroup glbGroup ON link.GK_GG = glbGroup.GG_PK
    AND glbGroup.GG_Code != 'ALL'
LEFT JOIN
    dbo.StmALog log ON link.GK_GG = log.SL_Parent
    AND SL_SE_NKEvent = 'EDT'
    AND log.SL_Reference LIKE CONCAT('Attached - (', staff.GS_Code, ')%')
WHERE
    log.SL_Parent IS NULL";

	const string TestLatestLogIsDetachedRecordSql = @"SELECT COUNT(1) 
FROM (
    SELECT 
        GK_GS,
        GK_GG,
        SL_Reference,
        SL_PostedTimeUtc,
        GK_SystemCreateTimeUtc,
        ROW_NUMBER() OVER (PARTITION BY link.GK_GS, link.GK_GG ORDER BY log.SL_PostedTimeUtc DESC) AS RN
    FROM dbo.GlbGroupLink link
   	JOIN dbo.GlbStaff staff ON staff.GS_PK = link.GK_GS
        AND staff.GS_IsSystemAccount = 0
    JOIN  dbo.GlbGroup glbGroup ON link.GK_GG = glbGroup.GG_PK
        AND glbGroup.GG_Code != 'ALL'
    JOIN dbo.StmALog log ON link.GK_GG = log.SL_Parent 
        AND log.SL_SE_NKEvent = 'EDT' 
        AND (log.SL_Reference LIKE CONCAT('Attached - (', staff.GS_Code, ')%') OR log.SL_Reference LIKE CONCAT('Detached - (', staff.GS_Code, ')%'))
) AS Temp
WHERE RN = 1
AND SL_Reference LIKE ('Detached%')";

	const string TestOnlyHaveDetachedLogRecordSql = @"SELECT COUNT(1)
FROM dbo.StmALog log1
JOIN
    dbo.GlbStaff staff ON log1.SL_Parent = staff.GS_PK
    AND staff.GS_IsSystemAccount = 0
JOIN
    dbo.GlbGroup glbGroup ON log1.SL_Parent = glbGroup.GG_PK
    AND glbGroup.GG_Code != 'ALL'
LEFT JOIN dbo.StmALog log2 ON log1.SL_Parent = log2.SL_Parent
 	AND log2.SL_Reference LIKE CONCAT('Attached - ', SUBSTRING(log1.SL_Reference, CHARINDEX('(', log1.SL_Reference), CHARINDEX(')', log1.SL_Reference) + 1 - CHARINDEX('(', log1.SL_Reference)), '%')
WHERE log1.SL_SE_NKEvent = 'EDT'
  AND log1.SL_Table in ('GlbStaff', 'GlbGroup')
  AND log1.SL_Reference LIKE 'Detached%'
  AND log2.SL_Reference IS NULL";
}
