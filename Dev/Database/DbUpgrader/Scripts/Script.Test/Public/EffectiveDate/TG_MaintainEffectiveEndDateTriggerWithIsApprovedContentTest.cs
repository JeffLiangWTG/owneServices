using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.EffectiveDate;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.EffectiveDate
{
	[TestedType(typeof(TG_Maintain_GET_EffectiveEndDateTrigger))]
	class TG_MaintainEffectiveEndDateTriggerWithIsApprovedContentTest : DbCreateScriptTest
	{
		public void TestGeneratedProcedure()
		{
			var scriptFromCode = ScriptToTest.Text;

			var expectedDefinition = @"CREATE TRIGGER TG_Maintain_GET_EffectiveEndDateTrigger
ON dbo.GlbEmploymentTeam
INSTEAD OF INSERT, UPDATE, DELETE
AS
BEGIN
	IF (@@ROWCOUNT = 0) RETURN
	SET NOCOUNT ON;

	WITH TargetRecords AS
	 (
		-- All rows where a StaffPK appears in either INSERTED or DELETED virtual tables
		SELECT
			GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser,
			GET_AutoEffectiveEndDate	-- Move AutoEffectiveEndDateColumn to the end of the list to make it easier to match the column lists
		FROM dbo.GlbEmploymentTeam a
		WHERE EXISTS
		 (
			SELECT 1
			FROM INSERTED x
			WHERE a.GET_GS_Staff = x.GET_GS_Staff
			UNION ALL
			SELECT 1 
			FROM DELETED x
			WHERE a.GET_GS_Staff = x.GET_GS_Staff
		)
	),
		SourceRecords AS
	 (
		-- This contains the rows we'll be attempting to MERGE against the target records
		-- with an additional column for the adjusted effective end date
		SELECT
			GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser, 
			NextEffectiveStartDT=
				CASE
					WHEN a.GET_IsApproved = 1
						THEN LEAD(a.GET_EffectiveDate, 1) OVER (PARTITION BY a.GET_GS_Staff, a.GET_IsApproved ORDER BY a.GET_EffectiveDate)
					ELSE NULL
				END
		FROM
		 (
			SELECT
				GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser
			FROM TargetRecords
			UNION ALL
			SELECT
				GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser
			FROM INSERTED
			EXCEPT 
			SELECT
				GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser
			FROM DELETED
		) a
	)
	MERGE TargetRecords t
	USING SourceRecords s
	-- Rows are considered 'matched' when they have the same primary key (GET_PK) value
	ON s.GET_PK = t.GET_PK
	WHEN MATCHED AND (
			-- If we've matched, we must be updating an existing row.
			-- Since we've selected all rows for this user, we need to filter out rows which have no changes
			ISNULL(NULLIF(s.NextEffectiveStartDT, t.GET_AutoEffectiveEndDate), NULLIF(t.GET_AutoEffectiveEndDate, s.NextEffectiveStartDT)) is not NULL
			-- Any of the time-sensitive attributes
			OR t.GET_AutoVersion <> s.GET_AutoVersion
			OR t.GET_EffectiveDate <> s.GET_EffectiveDate
			OR t.GET_GCR_ChangeRequest <> s.GET_GCR_ChangeRequest OR (t.GET_GCR_ChangeRequest IS NULL AND s.GET_GCR_ChangeRequest IS NOT NULL) OR (t.GET_GCR_ChangeRequest IS NOT NULL AND s.GET_GCR_ChangeRequest IS NULL)
			OR t.GET_GS_Staff <> s.GET_GS_Staff
			OR t.GET_GST_NKTeamCode <> s.GET_GST_NKTeamCode
			OR t.GET_IsApproved <> s.GET_IsApproved
			OR t.GET_SystemCreateTimeUtc <> s.GET_SystemCreateTimeUtc
			OR t.GET_SystemCreateUser <> s.GET_SystemCreateUser
			OR t.GET_SystemLastEditTimeUtc <> s.GET_SystemLastEditTimeUtc
			OR t.GET_SystemLastEditUser <> s.GET_SystemLastEditUser
		)
	THEN UPDATE 
		SET GET_AutoEffectiveEndDate = s.NextEffectiveStartDT,
			-- If anything has changed, we ensure to update the entire row
			GET_AutoVersion = s.GET_AutoVersion,
			GET_EffectiveDate = s.GET_EffectiveDate,
			GET_GCR_ChangeRequest = s.GET_GCR_ChangeRequest,
			GET_GS_Staff = s.GET_GS_Staff,
			GET_GST_NKTeamCode = s.GET_GST_NKTeamCode,
			GET_IsApproved = s.GET_IsApproved,
			GET_SystemCreateTimeUtc = s.GET_SystemCreateTimeUtc,
			GET_SystemCreateUser = s.GET_SystemCreateUser,
			GET_SystemLastEditTimeUtc = s.GET_SystemLastEditTimeUtc,
			GET_SystemLastEditUser = s.GET_SystemLastEditUser

	WHEN NOT MATCHED -- BY TARGET
	-- Insert the new row
	-- Using the calculated effective end date
	THEN INSERT
		 (GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser, GET_AutoEffectiveEndDate)
	VALUES
		 (s.GET_AutoVersion, s.GET_EffectiveDate, s.GET_GCR_ChangeRequest, s.GET_GS_Staff, s.GET_GST_NKTeamCode, s.GET_IsApproved, s.GET_PK, s.GET_SystemCreateTimeUtc, s.GET_SystemCreateUser, s.GET_SystemLastEditTimeUtc, GET_SystemLastEditUser, s.NextEffectiveStartDT)

	-- If a row is not in our source and it is in the DELETED virtual table
	-- that means it needs to be deleted from the target
	WHEN NOT MATCHED BY SOURCE AND EXISTS
		 (
			SELECT 1
			FROM
			(
				SELECT
					GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser
				FROM DELETED
				EXCEPT 
				SELECT
					GET_AutoVersion, GET_EffectiveDate, GET_GCR_ChangeRequest, GET_GS_Staff, GET_GST_NKTeamCode, GET_IsApproved, GET_PK, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser
				FROM INSERTED
			) x
			WHERE t.GET_GS_Staff = x.GET_GS_Staff AND t.GET_EffectiveDate = x.GET_EffectiveDate
		)
	THEN DELETE;
END
";

			AssertEquals("Generated procedure should be correct.", expectedDefinition.Trim(), scriptFromCode.Trim());
		}

		//This test script is not in the database, override this test and simply pass it.
		public override void TestScriptIsTheSameAsInTheDatabase()
		{
			Assert(true);
		}
	}

	class TG_Maintain_GET_EffectiveEndDateTrigger : TG_MaintainEffectiveEndDateTriggerBase<GlbEmploymentTeamSchema>
	{
	}
}
