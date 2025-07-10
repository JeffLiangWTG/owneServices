using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry;

[TestedType(typeof(RenameGlowRateSelectorTransformation))]
public class RenameGlowRateSelectorTransformationTest : RegistryDataTransformationTestCase
{
	protected override void AssertTransformationResults()
	{
		AssertEquals(0, Helper.GetStmDataRowCount("GlowRateSelector"));
		AssertEquals(1, Helper.GetStmDataRowCount("CargoWiseCarrierConnectForJobAutorating"));
	}

	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new RenameGlowRateSelectorTransformation();
	}

	protected override void PrepareTestData()
	{
		Helper.InsertStmDataRow("GlowRateSelector", Guid.Empty, Guid.Empty, true);
	}	
}
