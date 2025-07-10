using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(RenameServiceTaskProcessingBatchSize))]
	public class RenameServiceTaskProcessingBatchSizeTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("ServiceTaskProcessingBatchSize"));
			AssertEquals(1, Helper.GetStmDataRowCount("ServiceTaskProcessingMaximumBatchSize"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameServiceTaskProcessingBatchSize();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("ServiceTaskProcessingBatchSize", Guid.Empty, Guid.Empty, true);
		}
	}
}
