using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(UpdateEnableInwardProcessingFunctionForFRAndDERegistryDataTransformation))]
	public class UpdateEnableInwardProcessingFunctionForFRAndDERegistryDataTransformationTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateEnableInwardProcessingFunctionForFRAndDERegistryDataTransformation();
		}

		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				AssertEquals(4, Helper.GetStmDataRowCount("EnableInwardProcessing"));

				var registryValue1 = Helper.GetStmDataValue("EnableInwardProcessing", frenchCompany1PK);
				AssertEquals("FR1: Value is override ans equals 'True'", true, BitConverter.ToBoolean(registryValue1, 0));

				var registryValue2 = Helper.GetStmDataValue("EnableInwardProcessing", frenchCompany2PK);
				AssertEquals("FR2: Value is override ans equals 'True'", true, BitConverter.ToBoolean(registryValue2, 0));

				var registryValue3 = Helper.GetStmDataValue("EnableInwardProcessing", frenchCompany3PK);
				AssertEquals("FR3: Value is override ans equals 'True'", true, BitConverter.ToBoolean(registryValue3, 0));

				var registryValue4 = Helper.GetStmDataValue("EnableInwardProcessing", germanCompany1PK);
				AssertEquals("DE1: Value is override ans equals 'True'", true, BitConverter.ToBoolean(registryValue4, 0));

				AssertNull("Registry overridden for other company", Helper.GetStmDataValue("EnableInwardProcessing", otherCompany1PK));
				AssertNull("Registry overridden for other company", Helper.GetStmDataValue("EnableInwardProcessing", otherCompany2PK));
			});
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("EnableInwardProcessing"));
		}

		protected override void PrepareTestData()
		{
			var helper = new TransformationTestDataCreator();

			frenchCompany1PK = helper.CreateGlbCompany("DFR", "FR");
			frenchCompany2PK = helper.CreateGlbCompany("DGP", "GP");
			frenchCompany3PK = helper.CreateGlbCompany("DBL", "BL");
			germanCompany1PK = helper.CreateGlbCompany("DDE", "DE");
			otherCompany1PK = helper.CreateGlbCompany("DNL", "NL");
			otherCompany2PK = helper.CreateGlbCompany("DIT", "IT");
		}

		Guid frenchCompany1PK;
		Guid frenchCompany2PK;
		Guid frenchCompany3PK;
		Guid germanCompany1PK;
		Guid otherCompany1PK;
		Guid otherCompany2PK;
	}
}
