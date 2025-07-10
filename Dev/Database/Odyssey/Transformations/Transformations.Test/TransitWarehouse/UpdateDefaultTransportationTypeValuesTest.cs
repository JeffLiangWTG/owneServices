using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(UpdateDefaultTransportationTypeValues))]
	public class UpdateDefaultTransportationTypeValuesTests : RegistryDataTransformationTestCase
	{
		const string RegistryName = "DefaultTransportationType";
		Guid Owner1 = Guid.Parse("4b52a8cb-2e7d-4569-9f87-e866bcabdfee");
		Guid Owner2 = Guid.Parse("fc115b7f-536a-4c66-8672-f65721b76482");
		Guid Owner3 = Guid.Parse("3455a170-5623-4b2b-afb9-bc85d5ea13f5");
		Guid Owner4 = Guid.Parse("bb2f64f0-3c16-4f6c-a626-83cc1d878987");
		Guid Owner5 = Guid.Parse("41D5F94C-91C9-4DD8-A036-03B2245C9AC4");

		const string CON = "CON";
		const string CNT = "CNT";
		const string VIC = "VIC";
		const string VEH = "VEH";
		const string ULD = "ULD";
		const string NON = "NON";

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateDefaultTransportationTypeValues();
		}

		void PrepareTestData(string value, Guid? ownerGuid = null)
		{
			var binaryData = Encoding.Unicode.GetBytes(value);
			if (ownerGuid.HasValue)
			{
				Helper.InsertStmDataRow(RegistryName, ownerGuid.Value, Guid.Empty, "STR", binaryData);
			}
			else
			{
				Helper.InsertStmDataRow(RegistryName, "STR", binaryData);
			}
		}

		void AssertTransformationResults(string expectedValue, Guid? ownerGuid = null)
		{
			var actualValue = ownerGuid.HasValue
				? Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName, ownerGuid.Value))
				: Encoding.Unicode.GetString(Helper.GetStmDataValue(RegistryName));

			AssertEquals($"Updated registry item for owner {ownerGuid?.ToString() ?? "default"}", expectedValue, actualValue);
		}

		public void TestTransform_ConToCnt()
		{
			PrepareTestData(CON);
			var transformation = GetNewTestTransformationInstance();
			transformation.Run();
			AssertTransformationResults(CNT);
		}

		public void TestTransform_VicToVeh()
		{
			PrepareTestData(VIC);
			var transformation = GetNewTestTransformationInstance();
			transformation.Run();
			AssertTransformationResults(VEH);
		}

		public void TestTransform_NoChange()
		{
			PrepareTestData(ULD);
			var transformation = GetNewTestTransformationInstance();
			transformation.Run();
			AssertTransformationResults(ULD);
		}

		public void TestTransform_NoValue()
		{
			var transformation = GetNewTestTransformationInstance();
			transformation.Run();
			AssertEquals("No value should exist if the registry does not exist", null, Helper.GetStmDataValue(RegistryName));
		}

		public void TestTransform_MultipleValuesWithDifferentOwners()
		{
			var testData = new (string InitialValue, Guid Owner, string ExpectedValue)[]
			{
				(VIC, Owner1, VEH),
				(CON, Owner2, CNT),
				(ULD, Owner3, ULD),
				(NON, Owner4, NON),
				(VIC, Owner5, VEH)
			};

			foreach (var (initialValue, owner, _) in testData)
			{
				PrepareTestData(initialValue, owner);
			}

			var transformation = GetNewTestTransformationInstance();
			transformation.Run();

			foreach (var (_, owner, expectedValue) in testData)
			{
				AssertTransformationResults(expectedValue, owner);
			}
		}
	}
}
