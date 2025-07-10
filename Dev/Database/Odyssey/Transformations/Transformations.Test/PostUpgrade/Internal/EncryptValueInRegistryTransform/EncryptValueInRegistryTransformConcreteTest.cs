using System;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	[TestedType(typeof(EncryptValueInRegistryTransform))]
	public class EncryptValueInRegistryTransformConcreteTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Db.Connection.Command($"select SD_BinaryValue from dbo.StmData where SD_Name = '{EncryptValueInRegistryTransformForTest.GoodRegItem}'")) // db commands are fine in the transformations
			{
				var dataObj = cmd.ExecuteScalar();
				var data = dataObj as byte[];
				string asString = Encoding.Unicode.GetString(data);
				var decrypted = PasswordHashingTransformationHelper.DecryptWithTwoWayEncoder(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"), asString);
				AssertEquals(goodRegItemValue, decrypted);
			}

			using (var cmd = Db.Connection.Command($"select SD_BinaryValue from dbo.StmData where SD_Name = '{EncryptValueInRegistryTransformForTest.NullRegItem}'")) // db commands are fine in the transformations
			{
				var dataObj = cmd.ExecuteScalar();
				AssertEquals(true, dataObj is DBNull);
			}
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new EncryptValueInRegistryTransformForTest();
		}

		readonly string goodRegItemValue = GenerateString(10);

		protected override void PrepareTestData()
		{
			EncryptValueInRegistryTransformTestHelper.InsertPasswordStmDataRecord(TestConnection, EncryptValueInRegistryTransformForTest.GoodRegItem, goodRegItemValue);
			EncryptValueInRegistryTransformTestHelper.InsertPasswordStmDataRecord(TestConnection, EncryptValueInRegistryTransformForTest.NullRegItem, null);
		}

		protected override string ReasonNotToBeMapped => "Dummy test transformation";
	}
}
