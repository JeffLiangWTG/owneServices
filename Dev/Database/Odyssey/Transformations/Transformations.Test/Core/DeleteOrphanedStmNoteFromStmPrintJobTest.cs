using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(DeleteOrphanedStmNoteFromStmPrintJob))]
	class DeleteOrphanedStmNoteFromStmPrintJobTest : DeleteOrphanedStmNoteRecordsTest
	{
		protected override string TableName => "StmPrintJob";

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteOrphanedStmNoteFromStmPrintJob();
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);

			for (var i = 0; i < 10; i++)
			{
				helper.Insert("StmNote", new
				{
					ST_PK = Guid.NewGuid(),
					ST_Table = "StmPrintJob",
					ST_ParentID = Guid.NewGuid(),
					ST_NoteText = $"Testing Orphaned StmNote - Cover Note {i}",
				});

				var printJobPK = Guid.NewGuid();

				helper.Insert("StmNote", new
				{
					ST_PK = Guid.NewGuid(),
					ST_Table = "StmPrintJob",
					ST_ParentID = printJobPK,
					ST_NoteText = $"Testing Non-orphaned StmNote - Cover Note {i}",
				});

				helper.Insert("StmPrintJob", new
				{
					SP_PK = printJobPK
				});
			}
		}
	}
}
