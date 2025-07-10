using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[UseSnapshotProtection]
	sealed class AddUniqueIdentifierTVPTest : TestCase
	{
		public void TestRun()
		{
			using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
			{
				var sqlText = UpgraderUtils.GetSchemaBoundObjectsToDropSql(Db.SqlDbOwnerSchema, TVPHelper.TVP_uniqueidentifier.Replace(Db.SqlDbOwnerSchema + ".", ""), null, isType: true);
				new BatchRunner().RunCommandsGeneratedByQuery(Db.Connection, sqlText);
				Db.Connection.ExecuteNonQuery($"DROP TYPE {TVPHelper.TVP_uniqueidentifier}");

				var manager = new UpgradeManagerForTestWithOutputBuffer();
				var onlineUpgrader = new AddUniqueIdentifierTVP(manager);
				onlineUpgrader.Run();
				AssertContainsExactElementsInAnyOrder(new[] { $"Creating Table-Valued Parameter {TVPHelper.TVP_uniqueidentifier} if it does not exist." }, manager.OutputTextCollection);
				AssertTVPExists();

				onlineUpgrader.Run();
				AssertTVPExists();
			}
		}

		void AssertTVPExists()
		{
			AssertEquals("UniqueIdentifier TVP should exist after running the Transform.", 1,
					Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM sys.types WHERE user_type_id = TYPE_ID('{TVPHelper.TVP_uniqueidentifier}') AND is_user_defined = 1 AND is_table_type = 1"));
		}
	}
}
