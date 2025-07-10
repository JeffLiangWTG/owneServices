using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(RemoveRollUpByTaxReportingCodeRegistryItem))]
	class RemoveRollUpByTaxReportingCodeRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
			=> new RemoveRollUpByTaxReportingCodeRegistryItem();

		protected override void PrepareTestData()
			=> Helper.InsertStmDataRow("RollUpByTaxReportingCode", Guid.Empty, Guid.Empty);

		protected override void AssertTransformationResults()
			=> AssertEquals(0, Helper.GetStmDataRowCount("RollUpByTaxReportingCode"));
	}
}
