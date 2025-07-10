using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(DeleteArchiveRelationshipRows))]
	public class DeleteArchiveRelationshipRowsTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteArchiveRelationshipRows();

		protected override void AssertTransformationResults()
		{
			Assert(!TestConnection.Exists("FROM dbo.StmData WHERE SD_Name LIKE '%LastVersionRelationshipsTableWasCreatedForStage%'"));
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("LastVersionRelationshipsTableWasCreatedForStage_TestSuffix_Test Stage Name", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("LastVersionRelationshipsTableWasCreatedForStage_TestSuffix_TestStageName", Guid.Empty, Guid.Empty);
		}
	}
}
