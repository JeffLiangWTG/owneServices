using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class ScriptUpgradeTransactionalTest : TransactionedTestCase
	{
		public void TestUpdateScriptVersion()
		{
			var testVersion = new VersionLabel(4321, 321);
			DbRegistry.DatabaseMajorScriptVersion.SaveValue(testVersion.Major, TestConnection);
			DbRegistry.DatabaseMinorScriptVersion.SaveValue(testVersion.Minor, TestConnection);
			AssertScriptVersion(testVersion);

			new ScriptUpgraderForVersionTesting(TestConnection).UpdateScriptVersion_Exposed();
			AssertScriptVersion(ScriptVersion.Application);
		}

		void AssertScriptVersion(VersionLabel expectedVersion)
		{
			var majorVersionInDb = GetVersion("DatabaseMajorCoreScriptVersion");
			AssertEquals("ScriptVersion - Major", expectedVersion.Major, majorVersionInDb);

			var minorVersionInDb = GetVersion("DatabaseMinorCoreScriptVersion");
			AssertEquals("ScriptVersion - Minor", expectedVersion.Minor, minorVersionInDb);
		}

		int GetVersion(string name)
		{
			const string sql = "SELECT convert(nvarchar(4000), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = @Name";
			var command = TestConnection.Command(sql);
			command.AddParameter("@Name", SqlDbType.VarChar, name.Length, name);
			var versionStr = (string)command.ExecuteScalar();
			return Convert.ToInt32(versionStr, CultureInfo.InvariantCulture);
		}

		class ScriptUpgraderForVersionTesting : ScriptUpgrader
		{
			public ScriptUpgraderForVersionTesting(DbConnection connection)
			: base(new DummyUpgradeManager(), connection, connection, connection, new VersionLabel(0, 0))
			{
			}

			public void UpdateScriptVersion_Exposed()
			{
				UpdateScriptVersion();
			}
		}
	}
}
