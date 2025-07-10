using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.BusinessIntelligence;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.BusinessIntelligence
{
	[TestedType(typeof(DeleteBiUseCDCHotfixRegistry))]
	class DeleteBiUseCDCHotfixRegistryTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new DeleteBiUseCDCHotfixRegistry();

		protected override void PrepareTestData() => Helper.InsertStmDataRow("BiUseCDCHotfix", Guid.Empty, Guid.Empty);

		protected override void AssertTransformationResults() => AssertEquals(0, Helper.GetStmDataRowCount("BiUseCDCHotfix"));
	}
}
