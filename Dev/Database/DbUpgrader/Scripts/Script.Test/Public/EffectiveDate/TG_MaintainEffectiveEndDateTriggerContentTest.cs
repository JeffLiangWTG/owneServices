using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.EffectiveDate;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.EffectiveDate
{
	[TestedType(typeof(TG_Maintain_GSR_EffectiveEndDateTrigger))]
	class TG_MaintainEffectiveEndDateTriggerContentTest : DbCreateScriptTest
	{
		public void TestGeneratedProcedure()
		{
			var scriptFromCode = ScriptToTest.Text;

			var expectedDefinition = @"CREATE TRIGGER TG_Maintain_GSR_EffectiveEndDateTrigger
ON hrm.GlbStaffRemuneration
INSTEAD OF INSERT, UPDATE, DELETE
AS
BEGIN
	IF (@@ROWCOUNT = 0) RETURN
	SET NOCOUNT ON;

	WITH TargetRecords AS
	 (
		-- All rows where a StaffPK appears in either INSERTED or DELETED virtual tables
		SELECT
			GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser,
			GSR_AutoEffectiveEndDate	-- Move AutoEffectiveEndDateColumn to the end of the list to make it easier to match the column lists
		FROM hrm.GlbStaffRemuneration a
		WHERE EXISTS
		 (
			SELECT 1
			FROM INSERTED x
			WHERE a.GSR_GS_Staff = x.GSR_GS_Staff
			UNION ALL
			SELECT 1 
			FROM DELETED x
			WHERE a.GSR_GS_Staff = x.GSR_GS_Staff
		)
	),
		SourceRecords AS
	 (
		-- This contains the rows we'll be attempting to MERGE against the target records
		-- with an additional column for the adjusted effective end date
		SELECT
			GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser, 
			NextEffectiveStartDT=LEAD(a.GSR_EffectiveDate, 1) OVER (PARTITION BY a.GSR_GS_Staff ORDER BY a.GSR_EffectiveDate)
		FROM
		 (
			SELECT
				GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser
			FROM TargetRecords
			UNION ALL
			SELECT
				GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser
			FROM INSERTED
			EXCEPT 
			SELECT
				GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser
			FROM DELETED
		) a
	)
	MERGE TargetRecords t
	USING SourceRecords s
	-- Rows are considered 'matched' when they have the same primary key (GSR_PK) value
	ON s.GSR_PK = t.GSR_PK
	WHEN MATCHED AND (
			-- If we've matched, we must be updating an existing row.
			-- Since we've selected all rows for this user, we need to filter out rows which have no changes
			ISNULL(NULLIF(s.NextEffectiveStartDT, t.GSR_AutoEffectiveEndDate), NULLIF(t.GSR_AutoEffectiveEndDate, s.NextEffectiveStartDT)) is not NULL
			-- Any of the time-sensitive attributes
			OR t.GSR_AutoVersion <> s.GSR_AutoVersion
			OR t.GSR_EffectiveDate <> s.GSR_EffectiveDate
			OR t.GSR_FullTimeEquivalent <> s.GSR_FullTimeEquivalent
			OR t.GSR_GS_Staff <> s.GSR_GS_Staff
			OR t.GSR_LeaveLiabilityHourlyRate <> s.GSR_LeaveLiabilityHourlyRate
			OR t.GSR_RN_NKCountry <> s.GSR_RN_NKCountry
			OR t.GSR_RX_NKCurrency <> s.GSR_RX_NKCurrency
			OR t.GSR_SystemCreateTimeUtc <> s.GSR_SystemCreateTimeUtc
			OR t.GSR_SystemCreateUser <> s.GSR_SystemCreateUser
			OR t.GSR_SystemLastEditTimeUtc <> s.GSR_SystemLastEditTimeUtc
			OR t.GSR_SystemLastEditUser <> s.GSR_SystemLastEditUser
		)
	THEN UPDATE 
		SET GSR_AutoEffectiveEndDate = s.NextEffectiveStartDT,
			-- If anything has changed, we ensure to update the entire row
			GSR_AutoVersion = s.GSR_AutoVersion,
			GSR_EffectiveDate = s.GSR_EffectiveDate,
			GSR_FullTimeEquivalent = s.GSR_FullTimeEquivalent,
			GSR_GS_Staff = s.GSR_GS_Staff,
			GSR_LeaveLiabilityHourlyRate = s.GSR_LeaveLiabilityHourlyRate,
			GSR_RN_NKCountry = s.GSR_RN_NKCountry,
			GSR_RX_NKCurrency = s.GSR_RX_NKCurrency,
			GSR_SystemCreateTimeUtc = s.GSR_SystemCreateTimeUtc,
			GSR_SystemCreateUser = s.GSR_SystemCreateUser,
			GSR_SystemLastEditTimeUtc = s.GSR_SystemLastEditTimeUtc,
			GSR_SystemLastEditUser = s.GSR_SystemLastEditUser

	WHEN NOT MATCHED -- BY TARGET
	-- Insert the new row
	-- Using the calculated effective end date
	THEN INSERT
		 (GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser, GSR_AutoEffectiveEndDate)
	VALUES
		 (s.GSR_AutoVersion, s.GSR_EffectiveDate, s.GSR_FullTimeEquivalent, s.GSR_GS_Staff, s.GSR_LeaveLiabilityHourlyRate, s.GSR_PK, s.GSR_RN_NKCountry, s.GSR_RX_NKCurrency, s.GSR_SystemCreateTimeUtc, s.GSR_SystemCreateUser, s.GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser, s.NextEffectiveStartDT)

	-- If a row is not in our source and it is in the DELETED virtual table
	-- that means it needs to be deleted from the target
	WHEN NOT MATCHED BY SOURCE AND EXISTS
		 (
			SELECT 1
			FROM
			(
				SELECT
					GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser
				FROM DELETED
				EXCEPT 
				SELECT
					GSR_AutoVersion, GSR_EffectiveDate, GSR_FullTimeEquivalent, GSR_GS_Staff, GSR_LeaveLiabilityHourlyRate, GSR_PK, GSR_RN_NKCountry, GSR_RX_NKCurrency, GSR_SystemCreateTimeUtc, GSR_SystemCreateUser, GSR_SystemLastEditTimeUtc, GSR_SystemLastEditUser
				FROM INSERTED
			) x
			WHERE t.GSR_GS_Staff = x.GSR_GS_Staff AND t.GSR_EffectiveDate = x.GSR_EffectiveDate
		)
	THEN DELETE;
END
";

			Assert("Generated procedure should be correct.", expectedDefinition.Trim().Equals(scriptFromCode.Trim()));
		}

		//This test script is not in the database, override this test and simply pass it.
		public override void TestScriptIsTheSameAsInTheDatabase()
		{
			Assert(true);
		}
	}

	class GlbStaff : SQLDataObject<GlbStaff>
	{
		public GlbStaff(string code)
		{
			GS_Code = code;
			GS_LoginName = code + ".Login";
			GS_SystemCreateUser = "E";
			GS_SystemLastEditUser = "E";
			GS_SystemCreateTimeUtc = DateTime.Now;
			GS_SystemLastEditTimeUtc = DateTime.Now;
		}

		public string GS_Code { get; }
		public string GS_LoginName { get; }
		public string GS_SystemCreateUser { get; }
		public string GS_SystemLastEditUser { get; }

		public DateTime? GS_SystemCreateTimeUtc { get; }
		public DateTime? GS_SystemLastEditTimeUtc { get; }
	}

	class GlbHolidaySource : SQLDataObject<GlbHolidaySource>
	{
		public GlbHolidaySource(string code)
		{
			GHS_Code = code;
			GHS_Name = code + ".Name";
			GHS_SystemCreateUser = "E";
			GHS_SystemLastEditUser = "E";
			GHS_SystemCreateTimeUtc = DateTime.Now;
			GHS_SystemLastEditTimeUtc = DateTime.Now;
		}

		public string GHS_Code { get; }
		public string GHS_Name { get; }
		public string GHS_SystemCreateUser { get; }
		public string GHS_SystemLastEditUser { get; }

		public DateTime GHS_SystemCreateTimeUtc { get; }
		public DateTime GHS_SystemLastEditTimeUtc { get; }
	}

	class GlbDepartment : SQLDataObject<GlbDepartment>
	{
		public GlbDepartment(string code)
		{
			GE_Code = code;
		}

		public string GE_Code { get; }
	}

	class TG_Maintain_GSR_EffectiveEndDateTrigger : TG_MaintainEffectiveEndDateTriggerBase<GlbStaffRemunerationSchema>
	{
	}
}

