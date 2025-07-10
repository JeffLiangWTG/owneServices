using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(PurgeOrphanRecordsFromTables))]
	class PurgeOrphanRecordsFromTablesEmptyTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PurgeOrphanRecordsFromTables();

		protected override void AssertTransformationResults()
		{
			Assert(true);
		}
	}
}
