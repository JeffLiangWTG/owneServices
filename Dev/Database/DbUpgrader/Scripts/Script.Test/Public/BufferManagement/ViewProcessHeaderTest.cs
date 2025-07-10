using System;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement.Reports;
using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement.Triggers;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Testing
{
	[TestedType(typeof(ViewProcessHeader))]
	sealed class ViewProcessHeaderTest : DbCreateScriptTest
	{
		public void TestIncidentsNotInViewProcessHeader()
		{
			var incidentPK = Guid.NewGuid();

			var sql = BMDbTestHelper.GetProcessHeaderInsertSql(Guid.NewGuid(), parentHeaderPK: null, parentId: incidentPK, parentTableCode: "IM", workflowType: "INC");
			TestConnection.ExecuteNonQuery(sql);

			AssertEquals("Incidents should be not be in ViewProcessHeader in non-EDI clients.", null, TestConnection.ExecuteScalar($"SELECT VFH_WorkflowType FROM dbo.ViewProcessHeader WHERE VFH_ParentId = '{incidentPK}'"));
		}

		public void TestContainsRowsInViewClientProcessHeader()
		{
			var incidentPK = Guid.NewGuid();

			var dropCommands = new[]
			{
				"drop trigger TG_UPD_ViewProcessHeader",
				"drop function Report_ContainmentBarrierOutcomes",
				"drop view ViewProcessHeader",
				"drop view ViewClientProcessHeader"
			};
			var createCommands = new[]
			{
				new ViewProcessHeader().Text,
				new Report_ContainmentBarrierOutcomes().Text,
				new TG_UPD_ViewProcessHeader().Text
			};

			foreach (var dropCommand in dropCommands)
			{
				TestConnection.ExecuteNonQuery(dropCommand);
			}

			TestConnection.ExecuteNonQuery($@"
CREATE VIEW ViewClientProcessHeader
WITH SCHEMABINDING
as

SELECT
	convert(uniqueidentifier, '{incidentPK}') as VFH_PK,
	convert(bit, 1) as VFH_IsActive,
	NEWID() as VFH_FC_CurrentComponent,
	NEWID() as VFH_FC_DedicatedBuffer,
	NEWID() as VFH_ParentTemplateId,
	convert(datetime, NULL) as VFH_ReleaseDateTime,
	NEWID() as VFH_GG_ReleaseGroup,
	convert(smallint, 1) as VFH_VoteUpDownAmount,
	NEWID() as VFH_P0_Template,
	convert(nvarchar(max), 'My Test Completion Statement') as VFH_CompletionStatement,
	convert(smalldatetime, NULL) as VFH_AgreedDeliveryDate,
	convert(smalldatetime, NULL) as VFH_DoNotStartBeforeDate,
	convert(smalldatetime, NULL) as VFH_JobDoNotStartBeforeDate,
	convert(char(3), 'CLS') as VFH_Status,
	convert(smalldatetime, NULL) as VFH_StaggeredReleaseDelayExpiry,
	convert(decimal(2,1), 1) as VFH_TimeDelayFactor,
	1 as VFH_TimeDelayMinutes,
	NEWID() as VFH_FH_ParentHeader,
	NEWID() as VFH_ParentId,
	convert(varchar(3), 'WKI') as VFH_ParentTableCode,
	convert(bit, 1) as VFH_AllowTaskAutoAssignment,
	convert(bit, 1) as VFH_IsStandby,
	convert(bit, 1) as VFH_IsCriticalHandover,
	convert(datetime, '2020-12-15') as VFH_SystemCreateTimeUtc,
	convert(varchar(3), 'MD3') as VFH_SystemCreateUser,
	convert(datetime, '2020-12-15') as VFH_SystemLastEditTimeUtc,
	convert(varchar(3), 'MD3') as VFH_SystemLastEditUser,
	convert(int, 1) as VFH_PlannedDurationInMinutes,
	convert(decimal(10,3), 50) as VFH_BufferPenetrationPercentWhenCompleted,
	convert(bit, 1) as VFH_IsReleasableUnitParent,
	convert(varchar(3), 'Wow') as VFH_DateAcceptability,
	convert(varchar(3), 'MAD') as VFH_WorkflowType,
	convert(nvarchar(100), 'woah') as VFH_JobCode,
	convert(varchar(3), '') as VFH_LastTransferType,
	convert(nvarchar(max), 'My Beautiful Test Description') as VFH_Description
");

			foreach (var createCommand in createCommands)
			{
				TestConnection.ExecuteNonQuery(createCommand);
			}

			AssertEquals("ViewProcessHeader should contain rows in ViewClientProcessHeader.", "My Test Completion Statement", TestConnection.ExecuteScalar($"SELECT VFH_CompletionStatement FROM dbo.ViewProcessHeader WHERE VFH_PK = '{incidentPK}'"));
		}
	}
}

