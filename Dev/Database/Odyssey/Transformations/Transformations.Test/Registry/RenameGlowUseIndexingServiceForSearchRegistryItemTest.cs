using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RenameGlowUseIndexingServiceForSearchRegistryItem))]
	class RenameGlowUseIndexingServiceForSearchRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("GlowUseIndexingServiceForSearch"));
			AssertEquals(1, Helper.GetStmDataRowCount("GlowUseIndexingServiceForGlobalSearch"));
		}

		protected override void PrepareTestData()
		{
			Helper.DeleteStmDataRow("GlowUseIndexingServiceForSearch");
			Helper.InsertStmDataRow("GlowUseIndexingServiceForSearch", Guid.Empty, Guid.Empty, true);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameGlowUseIndexingServiceForSearchRegistryItem();
		}
	}
}
