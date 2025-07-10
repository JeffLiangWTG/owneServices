using System;
using System.Globalization;
using System.Text;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(RenameAndUpdateGracePeriodForDriverSecurity))]
	public class RenameAndUpdateGracePeriodForDriverSecurityTest : RegistryDataTransformationTestCase
	{
		readonly string OldRegistryName = "GracePeriodForDriverSecurity";
		readonly string NewRegistryName = "DriverSecurityCertificationCheckingActivated";
		readonly DateTime CurrDateTime = new DateTime(2025, 1, 1, 0, 0, 0);

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new RenameAndUpdateGracePeriodForDriverSecurity();
		}

		void PrepareTestData(string value)
		{
			var dateTimeAsString = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture).ToSqlFormat();
			Helper.InsertStmDataRow(OldRegistryName, "DT", Encoding.Unicode.GetBytes(dateTimeAsString));
		}

		void AssertTransformationResults(string expectedValue)
		{
			var actualValue = Encoding.Unicode.GetString(Helper.GetStmDataValue(NewRegistryName));
			var oldRegistryData = Helper.GetStmDataValue(OldRegistryName);

			AssertEquals(expectedValue, actualValue);
			AssertEquals(null, oldRegistryData);
		}

		public void TestTransform_UpdateToTrue()
		{
			var pastDateTime = CurrDateTime.AddHours(-1).ToSqlFormat();
			PrepareTestData(pastDateTime);
			var transformation = GetNewTestTransformationInstance();
			transformation.Run();
			AssertTransformationResults("True");
		}

		public void TestTransform_UpdateToFalse()
		{
			var futureDateTime = CurrDateTime.AddHours(1).ToSqlFormat();
			PrepareTestData(futureDateTime);
			var transformation = GetNewTestTransformationInstance();
			transformation.Run();
			AssertTransformationResults("False");
		}
	}
}
