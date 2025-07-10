using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ArchiveManager;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ArchiveManager
{
	[TestedType(typeof(DeleteOldParallelRegistryItems))]
	class DeleteOldParallelRegistryItemsTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("EnableParallelDocumentGeneration"));
			AssertEquals(0, Helper.GetStmDataRowCount("EnableParallelArchiveSetProcessing"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteOldParallelRegistryItems();

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("EnableParallelDocumentGeneration", Guid.Empty, Guid.Empty);
			Helper.InsertStmDataRow("EnableParallelArchiveSetProcessing", Guid.Empty, Guid.Empty);
		}
	}
}
