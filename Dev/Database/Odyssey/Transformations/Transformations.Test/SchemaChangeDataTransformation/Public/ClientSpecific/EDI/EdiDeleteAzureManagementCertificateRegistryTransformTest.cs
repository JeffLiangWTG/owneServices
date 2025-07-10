using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DbUpgrader.Testing
{
	[TestedType(typeof(EdiDeleteAzureManagementCertificateRegistryTransform))]
	class EdiDeleteAzureManagementCertificateRegistryTransformTest : DataTransformationTestCase
	{
		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue, SD_IsCancelled, SD_PreserveTestValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
VALUES (NEWID(), 'AzureApplicationManagementCertificate', 'BIN', 0, 0x1, 0, 1, '2023-06-01 00:00:00', 'E', '2023-06-01 00:00:00', '~BP')
 , (NEWID(), 'AzureApplicationManagementCertificatePassword', 'STR', 0, 0x2, 0, 1, '2022-06-01 00:00:00', 'E', '2022-06-01 00:00:00', '~BP')
 , (NEWID(), 'ShouldSurvive', 'BIN', 1, 0x3, 0, 1, '2022-04-25 00:00:00', 'E', '2022-04-25 00:00:00', '~BP')
";
			using var command = Db.Connection.Command(sql);
			command.ExecuteNonQuery();
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.StmData WHERE SD_Name = 'AzureApplicationManagementCertificate'"));
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.StmData WHERE SD_Name = 'AzureApplicationManagementCertificatePassword'"));
			AssertEquals(1, Db.Connection.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.StmData WHERE SD_Name = 'ShouldSurvive'"));
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new EdiDeleteAzureManagementCertificateRegistryTransform();
		}
	}
}
