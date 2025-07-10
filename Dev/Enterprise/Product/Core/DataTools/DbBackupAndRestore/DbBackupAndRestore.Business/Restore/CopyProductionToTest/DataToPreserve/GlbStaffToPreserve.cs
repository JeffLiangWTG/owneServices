using System.Globalization;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	class GlbStaffToPreserve : PreserveTestValueScripts
	{
		#region PreserveTestValueScripts Members

		protected override string TargetTableName
		{
			get { return "GlbStaff"; }
		}

		public override string GetCopyTempDbDataToTestDbScript(string auxDbName, string targetDbName)
		{
			// Clear local password for all the staff that are AD-linked
			// Preserve local password from the Test system if the test's login name match to the Prod's login name

			return string.Format(CultureInfo.InvariantCulture, PreservePasswordScript, auxDbName, targetDbName);
		}

		const string PreservePasswordScript = @"
-- PRESERVE LOCAL PASSWORD FOR EXISTING USERS
UPDATE [{1}]..GlbStaff SET
GS_PasswordHash = PreviousTestGlbStaff.GS_PasswordHash,
GS_PasswordSalt = PreviousTestGlbStaff.GS_PasswordSalt,
GS_PasswordHashIterations = PreviousTestGlbStaff.GS_PasswordHashIterations,
GS_SystemCreateTimeUtc = COALESCE(PreviousTestGlbStaff.GS_SystemCreateTimeUtc, GetUtcDate()),
GS_SystemCreateUser = COALESCE(PreviousTestGlbStaff.GS_SystemCreateUser, '~BP'), 
GS_SystemLastEditTimeUtc = GetUtcDate(),
GS_SystemLastEditUser = '~BP'
FROM [{0}]..TempGlbStaff PreviousTestGlbStaff
JOIN [{1}]..GlbStaff ProductionGlbStaff ON PreviousTestGlbStaff.GS_LoginName = ProductionGlbStaff.GS_LoginName AND PreviousTestGlbStaff.GS_PK = ProductionGlbStaff.GS_PK
";

		public override string GetClearDataToBeOverwrittenByTestDataScript(string auxDbName, string targetDbName)
		{
			return string.Empty;
		}

		#endregion
	}
}
