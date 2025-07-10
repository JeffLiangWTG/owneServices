using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting;

[TestedType(typeof(ClearRootFileExportPathRegistryItemValues))]
class ClearRootFileExportPathRegistryItemValuesTest : RegistryDataTransformationTestCase
{
	const string RegistryName = "ComplianceReportConfigurationRootFileExportPath";
	const string AnotherRegistryName = "AnotherRegistryName";

	protected override void AssertTransformationResults()
	{
		AssertEquals(0, Helper.GetStmDataRowCount(RegistryName));
		AssertEquals(1, Helper.GetStmDataRowCount(AnotherRegistryName));
	}

	protected override DataTransformation GetNewTestTransformationInstance() => new ClearRootFileExportPathRegistryItemValues();

	protected override void PrepareTestData()
	{
		Helper.InsertStmDataRow(RegistryName, Guid.Empty, Guid.Empty);
		Helper.InsertStmDataRow(AnotherRegistryName, Guid.Empty, Guid.Empty);
	}
}
