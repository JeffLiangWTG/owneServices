using System.Text;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing
{
	public abstract class BoolRegistryItemToCodePairRegistryItemTransformationTest<T> : RegistryDataTransformationTestCase
			where T : BoolRegistryItemToCodePairRegistryItemTransformation
	{
		void PrepareTestData(bool value)
		{
			var transformation = GetNewTestTransformationInstance() as T;
			var binaryData = Encoding.Unicode.GetBytes(value ? bool.TrueString : bool.FalseString);
			Helper.InsertStmDataRow(transformation.RegistryItemName, "BOL", binaryData);
		}

		void AssertTransformationResults(bool value)
		{
			var transformation = GetNewTestTransformationInstance() as T;
			var expectedValue = value ? transformation.ReplaceTrueWith : transformation.ReplaceFalseWith;
			var actualValue = Encoding.Unicode.GetString(Helper.GetStmDataValue(transformation.RegistryItemName));
			AssertEquals("Updated registry item", expectedValue, actualValue);
		}

		public void TestTransform_FalseValue()
		{
			PrepareTestData(false);
			var transformation = GetNewTestTransformationInstance() as T;
			transformation.Run();
			AssertTransformationResults(false);
		}

		public void TestTransform_NoValue()
		{
			var transformation = GetNewTestTransformationInstance() as T;
			transformation.Run();
			AssertEquals("No value should exist if the registry does not exist", null, Helper.GetStmDataValue(transformation.RegistryItemName));
		}

		protected override void PrepareTestData()
		{
			PrepareTestData(true);
		}

		protected override void AssertTransformationResults()
		{
			AssertTransformationResults(true);
		}
	}
}
