using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.ClearDataScripts
{
	public class ClearActiveDirectoryRelatedData : IClearDataScript
	{
		public void BuildScript(ClearDataScriptBuilder builder)
		{
			var serviceUser = User.ServiceUserCode;
			// RESET LOCAL PASSWORD FOR AD-LINKED USERS
			builder.UpdateRecords().From("GlbStaff")
				.Set("GS_PasswordHash", "NULL")
				.Set("GS_PasswordHashIterations", "0")
				.Set("GS_PasswordSalt", "NULL")
				.Set("GS_SystemLastEditUser", $"'{serviceUser}'")
				.Set("GS_SystemLastEditTimeUtc", "GetUtcDate()")
				.Where("GS_ActiveDirectoryObjectGuid IS NOT NULL AND GS_PasswordHash IS NOT NULL");

			//CLEAR ACTIVE DIRECTORY OBJECT GUID AND DOMAIN DATA
			builder.UpdateRecords().From("GlbStaff")
				.Set("GS_ActiveDirectoryObjectGUID", "NULL")
				.Set("GS_DomainName", "''")
				.Set("GS_SystemLastEditUser", $"'{serviceUser}'")
				.Set("GS_SystemLastEditTimeUtc", "GetUtcDate()");

			builder.UpdateRecords().From("GlbGroup")
				.Set("GG_ActiveDirectoryObjectGUID", "NULL")
				.Set("GG_DomainName", "''")
				.Set("GG_SystemLastEditUser", $"'{serviceUser}'")
				.Set("GG_SystemLastEditTimeUtc", "GetUtcDate()");
		}
	}
}
