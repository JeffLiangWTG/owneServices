using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	class ChangeLoggingMethodToElasticSearchLoggingInSANDEnvironmentsTest : TestCase
	{
		const string ExpectedValue = @"<SystemDefinableCodeDescriptionBoolWithExtraBoolCollection DefaultCode=""ELK""><SystemDefinableCodeDescriptionBoolWithExtraBool><CodeMaxLength>3</CodeMaxLength><Code>FSL</Code><Description>File System Logging</Description><Bool>N</Bool><Bool2>Y</Bool2><SystemDefined>True</SystemDefined></SystemDefinableCodeDescriptionBoolWithExtraBool><SystemDefinableCodeDescriptionBoolWithExtraBool><CodeMaxLength>3</CodeMaxLength><Code>ELK</Code><Description>Elastic Search Logging</Description><Bool>Y</Bool><Bool2>N</Bool2><SystemDefined>True</SystemDefined></SystemDefinableCodeDescriptionBoolWithExtraBool><SystemDefinableCodeDescriptionBoolWithExtraBool><CodeMaxLength>3</CodeMaxLength><Code>KAF</Code><Description>Kafka Logging</Description><Bool>N</Bool><Bool2>N</Bool2><SystemDefined>True</SystemDefined></SystemDefinableCodeDescriptionBoolWithExtraBool><SystemDefinableCodeDescriptionBoolWithExtraBool><CodeMaxLength>3</CodeMaxLength><Code>SYS</Code><Description>Syslog Logging</Description><Bool>N</Bool><Bool2>N</Bool2><SystemDefined>True</SystemDefined></SystemDefinableCodeDescriptionBoolWithExtraBool><SystemDefinableCodeDescriptionBoolWithExtraBool><CodeMaxLength>3</CodeMaxLength><Code>CFL</Code><Description>Combined File System Logging</Description><Bool>N</Bool><Bool2>Y</Bool2><SystemDefined>True</SystemDefined></SystemDefinableCodeDescriptionBoolWithExtraBool></SystemDefinableCodeDescriptionBoolWithExtraBoolCollection>";

		[TestedType(typeof(ChangeLoggingMethodToElasticSearchLoggingInSANDEnvironments))]
		class TestLoggingMethodUpdatesInSANDEnvironment : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				var upgradeManagerMock = new Mock<IUpgradeManager>();
				upgradeManagerMock
					.SetupGet(manager => manager.IsUATSystem)
					.Returns(true);

				var transformation = new ChangeLoggingMethodToElasticSearchLoggingInSANDEnvironments();
				transformation.Initialise(manager: upgradeManagerMock.Object);
				return transformation;
			}

			protected override void PrepareTestData()
			{
				RegistryDataTransformation.DeleteRegistryItemRows("LoggingMethods");
			}

			protected override void AssertTransformationResults()
			{
				var result = TestConnection.ExecuteScalar<string>("SELECT CONVERT(xml, SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = 'LoggingMethods'");
				AssertEquals(ExpectedValue, result);
			}
		}

		[TestedType(typeof(ChangeLoggingMethodToElasticSearchLoggingInSANDEnvironments))]
		class TestValueIsOverwrittenInSANDEnvironment : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				var upgradeManagerMock = new Mock<IUpgradeManager>();
				upgradeManagerMock
					.SetupGet(manager => manager.IsUATSystem)
					.Returns(true);

				var transformation = new ChangeLoggingMethodToElasticSearchLoggingInSANDEnvironments();
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
				var result = TestConnection.ExecuteScalar<string>("SELECT CONVERT(xml, SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = 'LoggingMethods'");
				AssertEquals(ExpectedValue, result);
			}
		}

		[TestedType(typeof(ChangeLoggingMethodToElasticSearchLoggingInSANDEnvironments))]
		class TestLoggingMethodDoesNotChangeInProductionEnvironment : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				upgradeManagerMock = new Mock<IUpgradeManager>();
				upgradeManagerMock
					.SetupGet(manager => manager.IsUATSystem)
					.Returns(false);

				var transformation = new ChangeLoggingMethodToElasticSearchLoggingInSANDEnvironments();
				transformation.Initialise(manager: upgradeManagerMock.Object);
				return transformation;
			}

			protected override void PrepareTestData()
			{
				RegistryDataTransformation.DeleteRegistryItemRows("LoggingMethods");
			}

			protected override void AssertTransformationResults()
			{
				var loggingMethodRegistryBinaryValue = new RegistryTransformationHelper().GetStmDataValue("LoggingMethods");
				upgradeManagerMock
					.Verify(manager => manager.ShowInfoMessage(It.Is<string>(s => s.Contains("Not a UAT environment, skipping."))), Times.Once);
				upgradeManagerMock
					.Invocations
					.Clear();
				AssertNull(loggingMethodRegistryBinaryValue);
			}

			Mock<IUpgradeManager> upgradeManagerMock;
		}
	}
}
