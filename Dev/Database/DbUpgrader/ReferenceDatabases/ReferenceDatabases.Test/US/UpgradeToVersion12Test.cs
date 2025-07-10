using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion12Test : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals("Db has USCImportEstablishment", true, DbObjectCreator.TableExists(testConnection, "USCImportEstablishment"));
			AssertEquals("Db has USCImportEstablishmentAlternateName", true, DbObjectCreator.TableExists(testConnection, "USCImportEstablishmentAlternateName"));
		}

		protected override int LatestVersionNumber
		{
			get { return 12; }
		}
	}
}
