using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RenameEnableICS2FunctionsRegistryItem))]
	class RenameEnableICS2FunctionsRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("EnableICS2FunctionsRegistry"));
			AssertEquals(1, Helper.GetStmDataRowCount("EnableICS2Functions"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameEnableICS2FunctionsRegistryItem();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("EnableICS2FunctionsRegistry", Guid.Empty, Guid.Empty, true);
		}
	}
}
