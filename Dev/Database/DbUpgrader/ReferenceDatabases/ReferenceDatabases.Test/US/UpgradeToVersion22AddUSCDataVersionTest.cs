using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion22AddUSCDataVersionTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals("Db has USCDataVersion.UZ_Name", true, DbObjectCreator.ColumnExists(testConnection, "USCDataVersion", "UZ_Name"));
			AssertEquals("Db has USCDataVersion.UZ_Version", true, DbObjectCreator.ColumnExists(testConnection, "USCDataVersion", "UZ_Version"));
			AssertEquals("Db has USCDataVersion.UZ_Note", true, DbObjectCreator.ColumnExists(testConnection, "USCDataVersion", "UZ_Note"));
			AssertEquals("Db has USCDataVersion.UZ_UpdateTime", true, DbObjectCreator.ColumnExists(testConnection, "USCDataVersion", "UZ_UpdateTime"));
			AssertEquals("Db has USCDataVersion.IX_USCDataVersion_UZ_Name", true, DbObjectCreator.IndexExists(testConnection, "USCDataVersion", "IX_USCDataVersion_UZ_Name"));
		}

		protected override int LatestVersionNumber
		{
			get { return 22; }
		}
	}
}
