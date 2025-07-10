using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.LogWalker
{
	public class NewsPublisher
	{
		protected internal NewsPublisher()
		{
		}

		protected internal int CreateLogQueueItemsForEverythingExceptWorkflowTriggerEvents(DbConnection dbConnection, LogSubscriber[] subscribers)
		{
			if (subscribers.Length == 0)
			{
				throw new ArgumentException("List of subscribers cannot be empty", nameof(subscribers));
			}

			if (subscribers.Any(x => x.EventTypes.Any(y => y == Events.WorkflowTriggerEventCode) && !x.Name.Equals("WorkflowEventTrigger", StringComparison.OrdinalIgnoreCase)))
			{
				throw new NotSupportedException("WTE must be performed only for WorkflowEventTrigger");
			}

			var publishers = ObjectFactory.Get<IEventPublisherWorkflowDescriptorList>()
				.Cast<CodeDescriptionPair>()
				.Select(p => WorkflowDescriptors.Instance.TryGetValueSafe(p.Code))
				.Cast<IEventPublisher>()
				.ToArray();
			var subscriptionQuery = GetSubscriptionQuery(publishers);

			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(dbConnection, subscribers);

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @StmALogQueueTempTable TABLE
(
	SLQ_ParentTableName VARCHAR(35),
	SLQ_ParentID UNIQUEIDENTIFIER,
	SLQ_IsEstimate BIT,
	SLQ_IsCancelled BIT,
	SLQ_Reference VARCHAR(1024),
	SLQ_PostedTimeUtc DATETIME,
	SLQ_GS_NKUser VARCHAR(3),
	SLQ_FireWorkflow BIT,
	SLQ_ALogReference UNIQUEIDENTIFIER,
	SLQ_EventTime DATETIME,
	SLQ_EventTimeUtc DATETIME,
	SLQ_SE_NKEvent VARCHAR(3),
	SLQ_GB_NKBranch VARCHAR(3),
	SLQ_GE_NKDepartment VARCHAR(3)
);

DELETE TOP (100000) FROM dbo.StmALogQueue WITH (READPAST, READCOMMITTEDLOCK)
OUTPUT DELETED.SLQ_ParentTableName,
	DELETED.SLQ_ParentID,
	DELETED.SLQ_IsEstimate,
	DELETED.SLQ_IsCancelled,
	DELETED.SLQ_Reference,
	DELETED.SLQ_PostedTimeUtc,
	DELETED.SLQ_GS_NKUser,
	DELETED.SLQ_FireWorkflow,
	DELETED.SLQ_ALogReference,
	DELETED.SLQ_EventTime,
	DELETED.SLQ_EventTimeUtc,
	DELETED.SLQ_SE_NKEvent,
	DELETED.SLQ_GB_NKBranch,
	DELETED.SLQ_GE_NKDepartment
INTO @StmALogQueueTempTable;

DECLARE @ProcessedCount INT = @@RowCount;

INSERT INTO dbo.StmJobQueue
(
	SJ_PK, SJ_FilterName, SJ_SE_NKEvent, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_GS_NKUser,
	SJ_GB_NKBranch, SJ_GE_NKDepartment, SJ_IsCancelled, SJ_IsEstimate, SJ_Reference, SJ_ParentTableCode,
	SJ_ParentID, SJ_ALogReference, SJ_Status, SJ_IsDelayFired
)
SELECT
	SJ_PK				=	NEWID(), *
from (
SELECT DISTINCT
	SJ_FilterName		=	mappingTable.NPE_SubscriberName,
	SJ_SE_NKEvent		=	SLQ_SE_NKEvent,
	SJ_PostedTimeUtc	=	SLQ_PostedTimeUtc,
	SJ_EventTime		=	SLQ_EventTime,
	SJ_EventTimeUtc		=	ISNULL(SLQ_EventTimeUtc, SLQ_EventTime),
	SJ_GS_NKUser		=	SLQ_GS_NKUser,
	SJ_GB_NKBranch		=	SLQ_GB_NKBranch,
	SJ_GE_NKDepartment	=	SLQ_GE_NKDepartment,
	SJ_IsCancelled		=	SLQ_IsCancelled,
	SJ_IsEstimate		=	SLQ_IsEstimate,
	SJ_Reference		=	SLQ_Reference,
	SJ_ParentTableCode	=	mappingTable.NPE_TablePrefix,
	SJ_ParentID			=	SLQ_ParentID,
	SJ_ALogReference	=	SLQ_ALogReference,
	SJ_Status			=	'QUE',
	SJ_IsDelayFired		=	0
FROM @StmALogQueueTempTable
JOIN dbo.StmNewsPublisherEventMapping AS mappingTable
	ON 'TML' = mappingTable.NPE_EventCode
	AND SLQ_ParentTableName = mappingTable.NPE_TableName
WHERE
	SLQ_FireWorkflow = 1) IQ;

INSERT INTO dbo.StmJobQueue
(
	SJ_PK, SJ_FilterName, SJ_SE_NKEvent, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_GS_NKUser,
	SJ_GB_NKBranch, SJ_GE_NKDepartment, SJ_IsCancelled, SJ_IsEstimate, SJ_Reference, SJ_ParentTableCode,
	SJ_ParentID, SJ_ALogReference, SJ_Status, SJ_IsDelayFired
)
SELECT
	SJ_PK				=	NEWID(), *
from (
SELECT DISTINCT
	SJ_FilterName		=	mappingTable.NPE_SubscriberName,
	SJ_SE_NKEvent		=	SLQ_SE_NKEvent,	
	SJ_PostedTimeUtc	=	SLQ_PostedTimeUtc,
	SJ_EventTime		=	SLQ_EventTime,
	SJ_EventTimeUtc		=	ISNULL(SLQ_EventTimeUtc, SLQ_EventTime),
	SJ_GS_NKUser		=	SLQ_GS_NKUser,
	SJ_GB_NKBranch		=	SLQ_GB_NKBranch,
	SJ_GE_NKDepartment	=	SLQ_GE_NKDepartment,
	SJ_IsCancelled		=	SLQ_IsCancelled,
	SJ_IsEstimate		=	SLQ_IsEstimate,
	SJ_Reference		=	SLQ_Reference,
	SJ_ParentTableCode	=	mappingTable.NPE_TablePrefix,
	SJ_ParentID			=	SLQ_ParentID,
	SJ_ALogReference	=	SLQ_ALogReference,
	SJ_Status			=	'QUE',
	SJ_IsDelayFired		=	StmEvent.SE_IsDelayFired
FROM @StmALogQueueTempTable
JOIN dbo.StmNewsPublisherEventMapping AS mappingTable
	ON SLQ_SE_NKEvent = mappingTable.NPE_EventCode
	AND SLQ_ParentTableName = mappingTable.NPE_TableName
JOIN dbo.StmEvent ON SLQ_SE_NKEvent = SE_Code
WHERE
	SLQ_ParentID != 0x) IQ;

{0}

SELECT @ProcessedCount;
", subscriptionQuery);

			return dbConnection.ExecuteScalar<int>(sqlText);
		}

		static string GetSubscriptionQuery(IEnumerable<IEventPublisher> publishers)
		{
			if (publishers.Any())
			{
				return string.Format(CultureInfo.InvariantCulture,
					@"
INSERT INTO dbo.StmJobQueue
(
	SJ_PK, SJ_FilterName, SJ_SE_NKEvent, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_GS_NKUser,
	SJ_GB_NKBranch, SJ_GE_NKDepartment, SJ_IsCancelled, SJ_IsEstimate, SJ_Reference, SJ_ParentTableCode,
	SJ_ParentID, SJ_ALogReference, SJ_Status, SJ_IsDelayFired, SJ_TargetID
)
SELECT
	SJ_PK				=	NEWID(), *
from
(
	SELECT DISTINCT
		SJ_FilterName		=	'WorkflowEventPublish',
		SJ_SE_NKEvent		=	SLQ_SE_NKEvent,
		SJ_PostedTimeUtc	=	SLQ_PostedTimeUtc,
		SJ_EventTime		=	SLQ_EventTime,
		SJ_EventTimeUtc		=	ISNULL(SLQ_EventTimeUtc, SLQ_EventTime),
		SJ_GS_NKUser		=	SLQ_GS_NKUser,
		SJ_GB_NKBranch		=	SLQ_GB_NKBranch,
		SJ_GE_NKDepartment	=	SLQ_GE_NKDepartment,
		SJ_IsCancelled		=	SLQ_IsCancelled,
		SJ_IsEstimate		=	SLQ_IsEstimate,
		SJ_Reference		=	SLQ_Reference,
		SJ_ParentTableCode	=	mappingTable.NPE_TablePrefix,
		SJ_ParentID			=	SLQ_ParentID,
		SJ_ALogReference	=	SLQ_ALogReference,
		SJ_Status			=	'QUE',
		SJ_IsDelayFired		=	SE_IsDelayFired,
		SJ_TargetID			=	SubscriptionPK
	FROM
		@StmALogQueueTempTable
	JOIN 
		dbo.StmEvent ON SLQ_SE_NKEvent = SE_Code
	JOIN 
		dbo.StmNewsPublisherEventMapping AS mappingTable ON mappingTable.NPE_TableName = SLQ_ParentTableName AND mappingTable.NPE_SubscriberName = 'IEventPublisher' AND mappingTable.NPE_EventCode = '000'
	JOIN 
	(
		{0}
	) Subscriptions(SubscriptionPK, TargetID) on TargetID = SLQ_ParentID
) DistinctInnerQuery;", string.Join(" union all ", publishers.Select(p => p.GetSubscriptionsQuery())));
			}
			else
			{
				return string.Empty;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected internal int CreateLogQueueItemsForWorkflowTriggerEvents(DbConnection dbConnection)
		{
			using (var command = dbConnection.Command("CreateLogQueueItemsForWorkflowTriggerEvents"))
			{
				command.CommandType = CommandType.StoredProcedure;
				return (int)(command.ExecuteScalar());
			}
		}
	}
}
