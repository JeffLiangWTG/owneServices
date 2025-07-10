using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RenameSRRMaximumRetryCountRegistryItem))]
	class RenameSRRMaximumRetryCountRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("SRRRetriesInLast24Hours"));
			AssertEquals(1, Helper.GetStmDataRowCount("SRRMaximumRetryCount"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameSRRMaximumRetryCountRegistryItem();
		}

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("SRRRetriesInLast24Hours", Guid.Empty, Guid.Empty, true);
		}
	}
}
