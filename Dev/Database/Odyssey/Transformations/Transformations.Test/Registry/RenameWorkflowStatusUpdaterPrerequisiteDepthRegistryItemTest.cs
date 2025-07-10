using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RenameWorkflowStatusUpdaterPrerequisiteDepthRegistryItem))]
	class RenameWorkflowStatusUpdaterPrerequisiteDepthRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("WorkflowStatusUpdaterPrerequisiteDepth"));
			AssertEquals(1, Helper.GetStmDataRowCount("MaximumDepthOfAnalyzedWorkflowHierarchy"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameWorkflowStatusUpdaterPrerequisiteDepthRegistryItem();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("WorkflowStatusUpdaterPrerequisiteDepth", Guid.Empty, Guid.Empty, true);
		}
	}
}
