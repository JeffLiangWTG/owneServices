using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion42_AddACEADCVDTablesTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			Assert(DbObjectCreator.TableExists(testConnection, "USCACCase"));
			Assert(DbObjectCreator.TableExists(testConnection, "USCACCaseRate"));
			Assert(DbObjectCreator.TableExists(testConnection, "USCACCaseEvent"));
			Assert(DbObjectCreator.TableExists(testConnection, "USCACCaseBondCash"));
			Assert(DbObjectCreator.TableExists(testConnection, "USCACCaseTariff"));
			Assert(DbObjectCreator.TableExists(testConnection, "USCACCaseLiqSuspension"));
		}

		protected override int LatestVersionNumber
		{
			get { return 42; }
		}
	}
}
