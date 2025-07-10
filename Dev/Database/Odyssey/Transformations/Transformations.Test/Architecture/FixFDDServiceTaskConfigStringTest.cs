using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	class FixFDDServiceTaskConfigStringTest : TestCase
	{
		[TestedType(typeof(FixFDDServiceTaskConfigString))]
		class TestBrokenConfigStringShouldBeFixed : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new FixFDDServiceTaskConfigString();
			}

			protected override void PrepareTestData()
			{
				lastEditTime = new DateTime(2025, 6, 1).ToUniversalTime();

				using var noTrigger = DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmScheduleTask");
				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask

INSERT INTO dbo.StmServiceTask
(
	SST_PK,
	SST_ServiceTaskCode,
	SST_Active,
	SST_Configuration,
	SST_SystemCreateTimeUtc,
	SST_SystemCreateUser,
	SST_SystemLastEditTimeUtc,
	SST_SystemLastEditUser,
	SST_NextRunTime
)
SELECT
	NEWID(),
	'FDD',
	0,
	'<ScheduleConfig><NextRunTimeCalculatorDays Period=""1"" ScheduledRunTime=""00:00:00"" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString>&amp;lt;FtpJobConfigXml&amp;gt;&amp;lt;NotifyPrintUser&amp;gt;True&amp;lt;/NotifyPrintUser&amp;gt;&amp;lt;NotifyOnSuccess&amp;gt;True&amp;lt;/NotifyOnSuccess&amp;gt;&amp;lt;NotifyOnFailure&amp;gt;True&amp;lt;/NotifyOnFailure&amp;gt;&amp;lt;NotificationGroup_PK&amp;gt;00000000-0000-0000-0000-000000000000&amp;lt;/NotificationGroup_PK&amp;gt;&amp;lt;/FtpJobConfigXml&amp;gt;</ConfigString></ScheduleConfig>',
	@LastEditTime,
	'~BP',
	@LastEditTime,
	'~BP',
	GetUtcDate()
",
					cmd =>
					{
						cmd.AddParameter("LastEditTime", SqlDbType.SmallDateTime, lastEditTime);
					});
			}

			protected override void AssertTransformationResults()
			{
				// Arrange
				var sqlText = "SELECT SST_Configuration, SST_SystemLastEditTimeUtc FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'FDD'";
				var expectedConfigString = "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"00:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString>&lt;FtpJobConfigXml&gt;&lt;NotifyPrintUser&gt;True&lt;/NotifyPrintUser&gt;&lt;NotifyOnSuccess&gt;True&lt;/NotifyOnSuccess&gt;&lt;NotifyOnFailure&gt;True&lt;/NotifyOnFailure&gt;&lt;NotificationGroup_PK&gt;00000000-0000-0000-0000-000000000000&lt;/NotificationGroup_PK&gt;&lt;/FtpJobConfigXml&gt;</ConfigString></ScheduleConfig>";

				// Action
				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				// Assert
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(expectedConfigString, dataTable.Rows[0]["SST_Configuration"]);
				AssertGreaterThan((DateTime)dataTable.Rows[0]["SST_SystemLastEditTimeUtc"], lastEditTime);
			}

			DateTime lastEditTime;
		}

		[TestedType(typeof(FixFDDServiceTaskConfigString))]
		class TestConfigStringShouldNotBeUpdatedIfNotExist : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new FixFDDServiceTaskConfigString();
			}

			protected override void PrepareTestData()
			{
				lastEditTime = new DateTime(2025, 6, 1).ToUniversalTime();

				using var noTrigger = DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmScheduleTask");
				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask

INSERT INTO dbo.StmServiceTask
(
	SST_PK,
	SST_ServiceTaskCode,
	SST_Active,
	SST_Configuration,
	SST_SystemCreateTimeUtc,
	SST_SystemCreateUser,
	SST_SystemLastEditTimeUtc,
	SST_SystemLastEditUser,
	SST_NextRunTime
)
SELECT
	NEWID(),
	'FDD',
	0,
	'<ScheduleConfig><NextRunTimeCalculatorDays Period=""1"" ScheduledRunTime=""00:00:00"" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>',
	@LastEditTime,
	'~BP',
	@LastEditTime,
	'~BP',
	GetUtcDate()
",
					cmd =>
					{
						cmd.AddParameter("LastEditTime", SqlDbType.SmallDateTime, lastEditTime);
					});
			}

			protected override void AssertTransformationResults()
			{
				// Arrange
				var sqlText = "SELECT SST_Configuration, SST_SystemLastEditTimeUtc FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'FDD'";
				var expectedConfigString = "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"00:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";

				// Action
				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				// Assert
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(expectedConfigString, dataTable.Rows[0]["SST_Configuration"]);
				AssertEquals(lastEditTime, dataTable.Rows[0]["SST_SystemLastEditTimeUtc"]);
			}

			DateTime lastEditTime;
		}

		[TestedType(typeof(FixFDDServiceTaskConfigString))]
		class TestConfigStringShouldNotBeUpdatedIfValid : DataTransformationTestCase
		{
			protected override DataTransformation GetNewTestTransformationInstance()
			{
				return new FixFDDServiceTaskConfigString();
			}

			protected override void PrepareTestData()
			{
				lastEditTime = new DateTime(2025, 6, 1).ToUniversalTime();

				using var noTrigger = DataTransformationHelper.SuspendInsertAuditTriggerIfExists("StmScheduleTask");
				TestConnection.ExecuteNonQuery(@"
DELETE from dbo.StmServiceTask

INSERT INTO dbo.StmServiceTask
(
	SST_PK,
	SST_ServiceTaskCode,
	SST_Active,
	SST_Configuration,
	SST_SystemCreateTimeUtc,
	SST_SystemCreateUser,
	SST_SystemLastEditTimeUtc,
	SST_SystemLastEditUser,
	SST_NextRunTime
)
SELECT
	NEWID(),
	'FDD',
	0,
	'<ScheduleConfig><NextRunTimeCalculatorDays Period=""1"" ScheduledRunTime=""00:00:00"" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString>&lt;FtpJobConfigXml&gt;&lt;NotifyPrintUser&gt;True&lt;/NotifyPrintUser&gt;&lt;NotifyOnSuccess&gt;True&lt;/NotifyOnSuccess&gt;&lt;NotifyOnFailure&gt;True&lt;/NotifyOnFailure&gt;&lt;NotificationGroup_PK&gt;00000000-0000-0000-0000-000000000000&lt;/NotificationGroup_PK&gt;&lt;/FtpJobConfigXml&gt;</ConfigString></ScheduleConfig>',
	@LastEditTime,
	'~BP',
	@LastEditTime,
	'~BP',
	GetUtcDate()
",
					cmd =>
					{
						cmd.AddParameter("LastEditTime", SqlDbType.SmallDateTime, lastEditTime);
					});
			}

			protected override void AssertTransformationResults()
			{
				// Arrange
				var sqlText = "SELECT SST_Configuration, SST_SystemLastEditTimeUtc FROM dbo.StmServiceTask WHERE SST_ServiceTaskCode = 'FDD'";
				var expectedConfigString = "<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"00:00:00\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount><ConfigString>&lt;FtpJobConfigXml&gt;&lt;NotifyPrintUser&gt;True&lt;/NotifyPrintUser&gt;&lt;NotifyOnSuccess&gt;True&lt;/NotifyOnSuccess&gt;&lt;NotifyOnFailure&gt;True&lt;/NotifyOnFailure&gt;&lt;NotificationGroup_PK&gt;00000000-0000-0000-0000-000000000000&lt;/NotificationGroup_PK&gt;&lt;/FtpJobConfigXml&gt;</ConfigString></ScheduleConfig>";

				// Action
				var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);

				// Assert
				AssertEquals(1, dataTable.Rows.Count);
				AssertEquals(expectedConfigString, dataTable.Rows[0]["SST_Configuration"]);
				AssertEquals(lastEditTime, dataTable.Rows[0]["SST_SystemLastEditTimeUtc"]);
			}

			DateTime lastEditTime;
		}
	}
}
