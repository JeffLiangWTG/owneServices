using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class QueueStatusFilter : ModuleTextFilter
	{
		public QueueStatusFilter()
			: base(ProcessHeader.ModuleFilterConstants.QueueStatus, EmptyQuery, () => new QueueStatusList())
		{
		}

		static ZQuery EmptyQuery(ZString value)
		{
			throw new NotSupportedException("GetQuery() is overriden so this empty query should never be used, and yet...");
		}

		protected override ZQuery GetQuery()
		{
			string query;

			switch (Property)
			{
				case QueueStatusList.Codes.ReadyToRelease:
					query = GetRTRQuery();
					break;
				case QueueStatusList.Codes.Blocked:
					query = GetBLKQuery();
					break;
				default:
					return new ZQuery();
			}

			return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, @"
				FH_PK IN
				(
					{0}
				)
				", query), null);
		}

		string GetRTRQuery()
		{
			return string.Format(CultureInfo.InvariantCulture, GetSqlBase(), "=");
		}

		string GetBLKQuery()
		{
			return string.Format(CultureInfo.InvariantCulture, GetSqlBase(), ">");
		}

		string GetSqlBase()
		{
			var result = string.Format(CultureInfo.InvariantCulture, NextWorkflowInEachQueueSql, ProcessHeaderSchema.Constants.PK, "{0}");

			if (!BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this))
			{
				result += (NoResString)@"
UNION ALL
" + string.Format(CultureInfo.InvariantCulture, NextWorkflowInEachQueueSql, ProcessHeaderSchema.Constants.FH_FH_ParentHeader, (NoResString)"{0}"); // SQL Expression
			}

			return result;
		}

		const string NextWorkflowInEachQueueSql = @"
SELECT	FH_PK
FROM	dbo.ProcessHeader
WHERE	{0} IN
        (
		    SELECT PK
		    FROM   dbo.GetWorkQueueTagLinks() queued
		    JOIN
		    (
		        SELECT	 Queue, MIN(Sequence) as MinSequence
		        FROM     dbo.GetWorkQueueTagLinks() groupedQueued
		        WHERE	 1 = 1
    		    GROUP BY Queue
		    ) minSequenceInEachQueue 
		    ON  queued.Queue = minSequenceInEachQueue.Queue
		    AND queued.Sequence {1} minSequenceInEachQueue.MinSequence
        )";
	}
}
