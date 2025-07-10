using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	abstract class ReferenceDbUpgraderVersionTest<T> : RefDbTransactionedTestCase<T> where T : ReferenceDbUpgraderForVersionTesting
	{
		public void TestUpgradeToAVersion()
		{
			refDbUpgrader.latestVersionOverride = 0;

			using (var createDbConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.CreateDbIfNotExists(createDbConnection, refDbUpgrader.DbName);
			}

			using (((ICurrentDbControl)testConnection).UseDatabase(refDbUpgrader.DbName))
			{
				refDbUpgrader.DoDataUpgrade_Exposed(testConnection, 0, 0);
				DoExtraAssertions();
				PrepareTestData(testConnection);
				CreateDummyConstraintNameForRename(testConnection);
			}

			SetRefDbTestVersionNumber(testConnection, LatestVersionNumber - 1);
			refDbUpgrader.latestVersionOverride = LatestVersionNumber;
			refDbUpgrader.CreateAndUpgradeIfRequired();

			using (((ICurrentDbControl)testConnection).UseDatabase(refDbUpgrader.DbName))
			{
				AssertEquals(LatestVersionNumber, refDbUpgrader.DbPreparationStrategy.GetVersionFromDatabase());
				AssertUpgradeResult();
				AssertDummyConstraintNameRenamed(testConnection);
			}
		}

		protected virtual void DoExtraAssertions()
		{
		}

		protected override T GetNewReferenceDbUpgrader()
		{
			return (T)Activator.CreateInstance(typeof(T), mockUpgradeContext.Object, testConnection, logger);
		}

		void SetRefDbTestVersionNumber(DbConnection conn, int version)
		{
			DataUtils.SaveDbExtendedProperty(conn, ReferenceDbUpgrader.VersionPropertyName, version.ToString(), refDbUpgrader.DbName);
		}

		protected virtual void PrepareTestData(DbConnection conn)
		{
		}

		protected abstract void AssertUpgradeResult();
		protected abstract int LatestVersionNumber { get; }

		void CreateDummyConstraintNameForRename(DbConnection conn)
		{
			conn.ExecuteNonQuery($@"IF OBJECT_ID('DummyTableForConstraintName') IS NOT NULL
BEGIN
	DROP TABLE DummyTableForConstraintName
END
CREATE TABLE DummyTableForConstraintName(
XXX_StringValue NVARCHAR(4000) NOT NULL CONSTRAINT DF_DummyTableForConstraintName_FOR_RENAME  DEFAULT ('')
)");
		}

		void AssertDummyConstraintNameRenamed(DbConnection conn)
		{
			Assert("DF_DummyTableForConstraintName_FOR_RENAME should be renamed", !DbObjectCreator.ObjectExists(testConnection, "DF_DummyTableForConstraintName_FOR_RENAME"));
			if (DbObjectCreator.TableExists(conn, "DummyTableForConstraintName"))
			{
				Assert("should be renamed to DF_DummyTableForConstraintName_XXX_StringValue", DbObjectCreator.ObjectExists(testConnection, "DF_DummyTableForConstraintName_XXX_StringValue"));
			}
		}
	}
}
