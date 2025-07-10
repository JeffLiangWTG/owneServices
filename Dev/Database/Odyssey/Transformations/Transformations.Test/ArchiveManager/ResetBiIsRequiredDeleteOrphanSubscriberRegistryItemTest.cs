using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(ResetBiIsRequiredDeleteOrphanSubscriberRegistryItem))]
	class ResetBiIsRequiredDeleteOrphanSubscriberRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
			=> new ResetBiIsRequiredDeleteOrphanSubscriberRegistryItem();

		protected override void PrepareTestData()
			=> Helper.InsertStmDataRow("BiIsRequiredDeleteOrphanSubscriber", Guid.Empty, Guid.Empty);

		protected override void AssertTransformationResults()
			=> AssertEquals(0, Helper.GetStmDataRowCount("BiIsRequiredDeleteOrphanSubscriber"));
	}
}
