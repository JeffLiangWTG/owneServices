using System.Collections.Generic;
using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Core
{
	[TestedType(typeof(DeleteOrphanedStmNoteRecords))]
	abstract class DeleteOrphanedStmNoteRecordsTest : DataTransformationTestCase
	{
		protected abstract string TableName { get; }

		protected override void AssertTransformationResults()
		{
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM StmNote WHERE ST_NoteText LIKE 'Testing Orphaned StmNote - Cover Note%'"));
			AssertEquals(10, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM StmNote WHERE ST_NoteText LIKE 'Testing Non-orphaned StmNote - Cover Note%'"));
		}

		public void TestLogging()
		{
			PrepareTestData();
			var logger = new List<string>();
			var transformation = (IOnlineTransformation)GetNewTestTransformationInstance();
			transformation.Run(s => logger.Add(s), CancellationToken.None);

			var expectedLog = "Finished deleting 10 orphaned StmNote records from " + TableName + " in total.";
			AssertCollectionContains(expectedLog, logger);
		}
	}
}
