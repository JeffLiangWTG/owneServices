using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ArchiveManager.Engine.Test.TestDoubles;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Engine.Test
{
	public class ArchiveTableHelperTest : TransactionedTestCase
	{
		public void TestNameOfArchiveRelationshipTableWithoutCustoms()
		{
			TestArchiveRelationshipTableNameHelper(false);
		}

		public void TestNameOfArchiveRelationshipTableWithCustoms()
		{
			TestArchiveRelationshipTableNameHelper(true);
		}

		public void TestArchiveRelationshipTableNameHelper(bool includeCustoms)
		{
			var systemDescriptor = new DummySimpleArchiveSystemDescriptor();
			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, isVerboseLog: false, shouldIncludeDeclarations: includeCustoms);
			var logger = new TestArchiveLogger();
			var schedule = new TestArchiveSchedule();
			var stageDescriptor = systemDescriptor.GetArchiveStageDescriptors(config).FirstOrDefault();
			var archiveStage = new ArchiveStage(stageDescriptor, systemDescriptor);
			var tableName = includeCustoms ? "dbo.ArchiveRelationshipEDIDATSimpleStageWithDeclarations" : "dbo.ArchiveRelationshipEDIDATSimpleStageWithoutDeclarations";
			archiveStage.BeginRun(config, schedule, logger);

			AssertNoExceptionThrown("The archive relationships table should exist and have the correct name",
				() => { _ = Db.Connection.ExecuteNonQuery($"SELECT COUNT(*) FROM {tableName}"); });
		}
	}
}
