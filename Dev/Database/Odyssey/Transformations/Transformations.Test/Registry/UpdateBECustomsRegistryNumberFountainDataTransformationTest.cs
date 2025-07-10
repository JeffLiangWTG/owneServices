using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry;

[TestedType(typeof(UpdateBECustomsRegistryNumberFountainDataTransformation))]
sealed class UpdateBECustomsRegistryNumberFountainDataTransformationTest : RegistryDataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new UpdateBECustomsRegistryNumberFountainDataTransformation();

	protected override void AssertPreConditions()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Data has no owner", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmData WHERE SD_Name = 'BECustomsRegistries' AND SD_Owner IS NULL"));
			AssertEquals("AA no date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d-00000000-0000-0000-0000-00000000000a-AA'"));
			AssertEquals("BB no date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d-00000000-0000-0000-0000-00000000000b-BB'"));
			AssertEquals("CC no date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d-00000000-0000-0000-0000-00000000000c-CC'"));
			AssertEquals("DD no date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d--DD'"));
		});
	}

	protected override void AssertTransformationResults()
	{
		CombineAssertions(() =>
		{
			AssertEquals("AA with date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d-00000000-0000-0000-0000-00000000000a-AA-01 Apr 2025'"));
			AssertEquals("BB with date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d-00000000-0000-0000-0000-00000000000b-BB-01 May 2025'"));
			AssertEquals("CC no date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d-00000000-0000-0000-0000-00000000000c-CC-'"));
			AssertEquals("DD with date", 1, TestConnection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmNums WHERE SN_Name = 'BECustomsRegistryNumber-00000000-0000-0000-0000-00000000000d--DD-15 May 2025'"));
		});
	}

	protected override void PrepareTestData()
	{
		var customsRegistrySettingsForBinaryValue = UpdateBECustomsRegistryNumberFountainDataTransformationTestHelper.GetEmbeddedResource(XmlCustomsRegistrySettings);
		const string ownerPK_d = "00000000-0000-0000-0000-00000000000d";

		const string sql = @$"
			INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc)
			VALUES (newid(), 'BECustomsRegistries', NULL, 'BIN', NULL, '~BP', GETUTCDATE(), '~BP', GETUTCDATE())

			INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc)
			VALUES (newid(), 'BECustomsRegistries', @ownerPK, 'BIN', @binaryParameter_invalid, '~BP', GETUTCDATE(), '~BP', GETUTCDATE())

			INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue, SD_SystemLastEditUser, SD_SystemLastEditTimeUtc, SD_SystemCreateUser, SD_SystemCreateTimeUtc)
			VALUES (newid(), 'BECustomsRegistries', @ownerPK_d, 'BIN', @binaryParameter_valid, '~BP', GETUTCDATE(), '~BP', GETUTCDATE())

			INSERT INTO dbo.StmNums (SN_Name, SN_Value)
			VALUES ('BECustomsRegistryNumber-{ownerPK_d}-00000000-0000-0000-0000-00000000000a-AA', 99)

			INSERT INTO dbo.StmNums (SN_Name, SN_Value)
			VALUES ('BECustomsRegistryNumber-{ownerPK_d}-00000000-0000-0000-0000-00000000000b-BB', 1)

			INSERT INTO dbo.StmNums (SN_Name, SN_Value)
			VALUES ('BECustomsRegistryNumber-{ownerPK_d}-00000000-0000-0000-0000-00000000000c-CC', 1)

			INSERT INTO dbo.StmNums (SN_Name, SN_Value)
			VALUES ('BECustomsRegistryNumber-{ownerPK_d}--DD', 1)
		";

		using (DbCommand command = Db.Connection.Command(sql))
		{
			command.AddParameter("@binaryParameter_invalid", SqlDbType.Binary, Encoding.Unicode.GetBytes(""));
			command.AddParameter("@binaryParameter_valid", SqlDbType.Binary, Encoding.Unicode.GetBytes(customsRegistrySettingsForBinaryValue));
			command.AddParameter("@ownerPK", SqlDbType.UniqueIdentifier, new Guid("00000000-0000-0000-0000-000000000000"));
			command.AddParameter("@ownerPK_d", SqlDbType.UniqueIdentifier, new Guid(ownerPK_d));
			command.ExecuteNonQuery();
		}
	}

	const string XmlCustomsRegistrySettings = "CustomsRegistrySettings.xml";

	static class UpdateBECustomsRegistryNumberFountainDataTransformationTestHelper
	{
		public static string GetEmbeddedResource(string name)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
			{
				if (stream == null)
				{
					throw new FileNotFoundException(
						$"Expected test data is not available. Please check that assembly manifest resource exists: {name}.");
				}

				using (var reader = new StreamReader(stream))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
