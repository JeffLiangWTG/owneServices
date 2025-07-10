using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(RecreateIndexesForAuditDb))]
	internal class RecreateIndexesForAuditDBDoesNotThrowErrorWhenPkNameColumnDoesNotExistTest : AuditDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			Assert("IX_AccAlternateChart_StartLsn index should be created", TestConnection.Exists("from sys.indexes where name = 'IX_AccAlternateChart_StartLsn' and object_id in (OBJECT_ID('dbo.AccAlternateChart'))"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new RecreateIndexesForAuditDb();

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestGetListofTableToRecreateIndexDoesNotFailWhenPkNameDoesNotExist()
		{
			PrepareTestData();
			var recreateIndexes = new RecreateIndexesForAuditDb();
			recreateIndexes.EnsureTableConfigurationHasPkNameColumn(TestConnection);
			var indexesToRecreate = recreateIndexes.GetListofTableToRecreateIndex(TestConnection);
			CombineAssertions(() =>
			{
				AssertEquals("dbo", indexesToRecreate.ElementAt(0).SchemaName);
				AssertEquals("AccAlternateChart", indexesToRecreate.ElementAt(0).TableName);
				AssertEquals("AAC_PK", indexesToRecreate.ElementAt(0).PrimaryKeyName);
			});
		}

		DbConnection testConnection;
		protected override DbConnection TestConnection
		{
			get
			{
				return testConnection ?? (testConnection = Db.NewAdminConnection(Db.AuditDatabaseName));
			}
		}

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery(@"
ALTER TABLE [biadmin].[TableConfiguration]
DROP COLUMN PkName

DROP INDEX IX_AccAlternateChart_StartLsn ON [dbo].[AccAlternateChart];");

			var indexExists = TestConnection.Exists("FROM sys.indexes WHERE name = 'IX_AccAlternateChart_StartLsn'");
			Assert(!indexExists);
		}
	}
}
