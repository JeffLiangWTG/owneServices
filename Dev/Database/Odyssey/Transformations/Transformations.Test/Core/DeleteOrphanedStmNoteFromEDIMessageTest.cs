using System;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Test.Core;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core.Testing
{
	[TestedType(typeof(DeleteOrphanedStmNoteFromEDIMessage))]
	class DeleteOrphanedStmNoteFromEDIMessageTest : DeleteOrphanedStmNoteRecordsTest
	{
		protected override string TableName => "EDIMessage";

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteOrphanedStmNoteFromEDIMessage();
		}

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);

			var glbDepartmentPK = Guid.NewGuid();

			helper.Insert("GlbDepartment", new
			{
				GE_PK = glbDepartmentPK
			});

			var glbCompanyPK = Guid.NewGuid();

			helper.Insert("GlbCompany", new
			{
				GC_PK = glbCompanyPK,
				GC_Code = "CCC",
				GC_Name = "AU company"
			});

			var glbBranchPK = Guid.NewGuid();

			helper.Insert("GlbBranch", new
			{
				GB_PK = glbBranchPK,
				GB_GC = glbCompanyPK
			});

			for (var i = 0; i < 10; i++)
			{
				helper.Insert("StmNote", new
				{
					ST_PK = Guid.NewGuid(),
					ST_Table = "EDIMessage",
					ST_ParentID = Guid.NewGuid(),
					ST_NoteText = $"Testing Orphaned StmNote - Cover Note {i}",
				});

				var messagePK = Guid.NewGuid();

				helper.Insert("StmNote", new
				{
					ST_PK = Guid.NewGuid(),
					ST_Table = "EDIMessage",
					ST_ParentID = messagePK,
					ST_NoteText = $"Testing Non-orphaned StmNote - Cover Note {i}",
				});

				helper.Insert("EDIMessage", new
				{
					EM_PK = messagePK,
					EM_GB = glbBranchPK,
					EM_GE = glbDepartmentPK
				});
			}
		}
	}
}
