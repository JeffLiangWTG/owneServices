using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.eServices;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.eServices
{
	[TestedType(typeof(UpdateRegistryForConnectionToXTServer))]
	class UpdateRegistryForConnectionToXTServerTest : RegistryDataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateRegistryForConnectionToXTServer();
		}

		protected override void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery(@"
DELETE FROM dbo.StmData
WHERE
	SD_Name = 'ConnectionToXTServer'

INSERT INTO dbo.StmData
(SD_PK                                 , SD_Name               , SD_BinaryValue, SD_PreserveTestValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
VALUES
('4299C103-96B4-40EF-9F3F-90EB058A2D10', 'ConnectionToXTServer', 0x540053005400, 0                   , GETUTCDATE()          , 'E'                , GETUTCDATE()            , 'E')
");
		}

		protected override void AssertTransformationResults()
		{
			var result = TestConnection.ExecuteScalar<bool>("SELECT SD_PreserveTestValue FROM dbo.StmData WHERE SD_Name = 'ConnectionToXTServer'");
			AssertEquals(true, result);
		}
	}
}
