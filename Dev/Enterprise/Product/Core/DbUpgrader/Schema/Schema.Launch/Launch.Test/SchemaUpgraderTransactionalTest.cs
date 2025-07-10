using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class SchemaUpgraderTransactionalTest : TransactionedTestCase
	{
		public void TestUpdateVersion()
		{
			var testVersion = new VersionLabel(4321, 234);
			Env.Registry.DatabaseMajorSchemaVersion = testVersion.Major;
			Env.Registry.DatabaseMinorSchemaVersion = testVersion.Minor;
			AssertVersion(testVersion);

			new SchemaUpgraderForVersionTesting(TestConnection).UpdateSchemaVersion_Exposed();
			AssertVersion(SchemaVersion.Application);
		}

		void AssertVersion(VersionLabel expectedVersion)
		{
			string sqlText = "SELECT convert(nvarchar(4000), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = 'DATABASE_SCHEMA_VERSION'";
			int majorVersionInDb = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			AssertEquals("ScriptVersion - Major", expectedVersion.Major, majorVersionInDb);

			sqlText = "SELECT convert(nvarchar(4000), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = 'DATABASE_MINOR_SCHEMA_VERSION'";
			int minorVersionInDb = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			AssertEquals("ScriptVersion - Minor", expectedVersion.Minor, minorVersionInDb);
		}

		class SchemaUpgraderForVersionTesting : Schema.SchemaUpgrader
		{
			public SchemaUpgraderForVersionTesting(DbConnection connection)
			: base(new DummyUpgradeManager(), connection, connection, connection)
			{
			}

			public void UpdateSchemaVersion_Exposed()
			{
				UpdateSchemaVersion();
			}
		}
	}
}
