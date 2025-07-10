using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry;

[TestedType(typeof(RenameShipamaxRegistries))]
public class RenameShipamaxRegistriesTest : RegistryDataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance()
	{
		return new RenameShipamaxRegistries();
	}

	protected override void PrepareTestData()
	{
		Helper.InsertStmDataRow("EnableShipamaxIntegration", Guid.Empty, Guid.Empty, true);
		Helper.InsertStmDataRow("ShipamaxIntegrationUrl", Guid.Empty, Guid.Empty, true);
		Helper.InsertStmDataRow("ShipamaxClientId", Guid.Empty, Guid.Empty, true);
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals(0, Helper.GetStmDataRowCount("EnableShipamaxIntegration"));
		AssertEquals(0, Helper.GetStmDataRowCount("ShipamaxIntegrationUrl"));
		AssertEquals(0, Helper.GetStmDataRowCount("ShipamaxClientId"));

		AssertEquals(1, Helper.GetStmDataRowCount("EnableDocumentParsing"));
		AssertEquals(1, Helper.GetStmDataRowCount("DocumentParserUrl"));
		AssertEquals(1, Helper.GetStmDataRowCount("DocumentParserClientId"));
	}
}
