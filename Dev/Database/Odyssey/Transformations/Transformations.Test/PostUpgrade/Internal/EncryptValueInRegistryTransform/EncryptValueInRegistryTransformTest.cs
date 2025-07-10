using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	public abstract class EncryptValueInRegistryTransformTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var regItems = new List<string>((TransformationToTest as EncryptValueInRegistryTransform).RegistryNamesToEncrypt);
			regItems.Add(nullRegItem);
			regItems.Add(missingRegItem);
			regItems.Add(unrelatedRegItem);

			foreach (var name in regItems)
			{
				using (var cmd = Db.Connection.Command($"select SD_BinaryValue from dbo.StmData where SD_Name = '{name}'")) // db commands are fine in the transformations
				{
					var dataObj = cmd.ExecuteScalar();

					if (name == nullRegItem)
					{
						AssertEquals(true, dataObj is DBNull);
					}
					else
					{
						var data = dataObj as byte[];

						if (name == missingRegItem)
						{
							AssertNull(data);
						}
						else
						{
							if (data == null || data.Length == 0)
							{
								Fail("The value should still exist");
							}

							string asString = Encoding.Unicode.GetString(data);

							if (name == unrelatedRegItem)
							{
								AssertEquals("The value should have not been encrypted", passwords[name], asString);
							}
							else
							{
								var decrypted = PasswordHashingTransformationHelper.DecryptWithTwoWayEncoder(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"), asString);
								AssertEquals(passwords[name], decrypted);
							}
						}
					}
				}
			}
		}

		readonly Dictionary<string, string> passwords = new Dictionary<string, string>();
		const string nullRegItem = "NullRegItem";
		const string missingRegItem = "MissingRegItem";
		const string unrelatedRegItem = "UnrelatedRegItem"; //not to be encrypted

		protected override void PrepareTestData()
		{
			var regItems = new List<string>((TransformationToTest as EncryptValueInRegistryTransform).RegistryNamesToEncrypt);
			regItems.Add(unrelatedRegItem);

			foreach (var name in regItems)
			{
				var pwd = GenerateString(10);
				passwords.Add(name, pwd);
				EncryptValueInRegistryTransformTestHelper.InsertPasswordStmDataRecord(TestConnection, name, pwd);
			}

			EncryptValueInRegistryTransformTestHelper.InsertPasswordStmDataRecord(TestConnection, nullRegItem, null);
			passwords.Add(nullRegItem, null);
		}
	}
}
