using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	class RemoveLoggingMethodsForHostedClientsTest : TestCase
	{
		[TestedType(typeof(RemoveLoggingMethodsForHostedClients))]
		class DeleteRowsForHostedTest : RegistryDataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				upgradeManagerMock = new Mock<IUpgradeManager>();
				upgradeManagerMock
					.SetupGet(manager => manager.IsHosted)
					.Returns(true);

				var transformation = new RemoveLoggingMethodsForHostedClients();
				transformation.Initialise(manager: upgradeManagerMock.Object);
				return transformation;
			}

			protected override void PrepareTestData()
			{
				Helper.InsertStmDataRow("LoggingMethods", Guid.Empty, Guid.Empty);
			}

			protected override void AssertTransformationResults()
			{
				AssertEquals(0, Helper.GetStmDataRowCount("LoggingMethods"));
			}

			Mock<IUpgradeManager> upgradeManagerMock;
		}

		[TestedType(typeof(RemoveLoggingMethodsForHostedClients))]
		class DeleteRowsForInternalSystemTest : RegistryDataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				upgradeManagerMock = new Mock<IUpgradeManager>();
				upgradeManagerMock
					.SetupGet(manager => manager.IsHosted)
					.Returns(false);
				upgradeManagerMock
					.SetupGet(manager => manager.IsInternalSystem)
					.Returns(true);

				var transformation = new RemoveLoggingMethodsForHostedClients();
				transformation.Initialise(manager: upgradeManagerMock.Object);
				return transformation;
			}

			protected override void PrepareTestData()
			{
				Helper.InsertStmDataRow("LoggingMethods", Guid.Empty, Guid.Empty);
			}

			protected override void AssertTransformationResults()
			{
				AssertEquals(0, Helper.GetStmDataRowCount("LoggingMethods"));
			}

			Mock<IUpgradeManager> upgradeManagerMock;
		}

		[TestedType(typeof(RemoveLoggingMethodsForHostedClients))]
		class ChangesNothingForNotHostedAndNotInternalTest : RegistryDataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				upgradeManagerMock = new Mock<IUpgradeManager>();
				upgradeManagerMock
					.SetupGet(manager => manager.IsHosted)
					.Returns(false);
				upgradeManagerMock
					.SetupGet(manager => manager.IsInternalSystem)
					.Returns(false);

				var transformation = new RemoveLoggingMethodsForHostedClients();
				transformation.Initialise(manager: upgradeManagerMock.Object);
				return transformation;
			}

			protected override void PrepareTestData()
			{
				TestConnection.ExecuteNonQuery(@"
DELETE FROM dbo.StmData
WHERE
	SD_Name = 'LoggingMethods'

INSERT INTO dbo.StmData
(SD_PK                                 , SD_Name         , SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
VALUES
('4895FA20-CC2F-42D5-86A7-D3412EABC001', 'LoggingMethods', 0x010203040506, GETUTCDATE()          , 'E'                , GETUTCDATE()            , 'E')
");
			}

			protected override void AssertTransformationResults()
			{
				var result = TestConnection.ExecuteScalar<string>("SELECT CONVERT(varchar(MAX), SD_BinaryValue, 2) FROM dbo.StmData WHERE SD_Name = 'LoggingMethods'");
				AssertEquals("010203040506", result);
				upgradeManagerMock
					.Verify(manager => manager.ShowInfoMessage(It.Is<string>(s => s.Contains("Client is not hosted, skipping."))), Times.Once);
				upgradeManagerMock
					.Invocations
					.Clear();
			}

			Mock<IUpgradeManager> upgradeManagerMock;
		}

		[TestedType(typeof(RemoveLoggingMethodsForHostedClients))]
		class ChangesNothingForNotHostedAndNullInternalTest : RegistryDataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				upgradeManagerMock = new Mock<IUpgradeManager>();
				upgradeManagerMock
					.SetupGet(manager => manager.IsHosted)
					.Returns(false);
				upgradeManagerMock
					.SetupGet(manager => manager.IsInternalSystem)
					.Returns((bool?)null);

				var transformation = new RemoveLoggingMethodsForHostedClients();
				transformation.Initialise(manager: upgradeManagerMock.Object);
				return transformation;
			}

			protected override void PrepareTestData()
			{
				TestConnection.ExecuteNonQuery(@"
DELETE FROM dbo.StmData
WHERE
	SD_Name = 'LoggingMethods'

INSERT INTO dbo.StmData
(SD_PK                                 , SD_Name         , SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
VALUES
('4895FA20-CC2F-42D5-86A7-D3412EABC001', 'LoggingMethods', 0x010203040506, GETUTCDATE()          , 'E'                , GETUTCDATE()            , 'E')
");
			}

			protected override void AssertTransformationResults()
			{
				var result = TestConnection.ExecuteScalar<string>("SELECT CONVERT(varchar(MAX), SD_BinaryValue, 2) FROM dbo.StmData WHERE SD_Name = 'LoggingMethods'");
				AssertEquals("010203040506", result);
				upgradeManagerMock
					.Verify(manager => manager.ShowInfoMessage(It.Is<string>(s => s.Contains("Client is not hosted, skipping."))), Times.Once);
				upgradeManagerMock
					.Invocations
					.Clear();
			}

			Mock<IUpgradeManager> upgradeManagerMock;
		}
	}
}
