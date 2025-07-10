using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Scheduler.Business
{
	public class StmScheduleTaskCollection : BusinessObjectCollection<StmScheduleTask>
	{
		public StmScheduleTaskCollection(BusinessObjectFactory factory, ZString parentTableCode)
			: base(factory)
		{
			this.parentTableCode = parentTableCode;
		}

		public void LoadTasksToRun(ZDateTime nextScheduleDate)
		{
			var query = GetScheduleTaskQuery(nextScheduleDate);
			Load(query);
		}

		ZQuery GetScheduleTaskQuery(ZDateTime nextScheduleDate)
		{
			var query = CreateAdditionalFilter();
			query.AddToFilter(StmScheduleTaskSchema.S5_IsActive, true);
			query.AddToFilter(StmScheduleTaskSchema.S5_NextScheduledPrintRunTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, nextScheduleDate);
			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public QueueResult GetPendingTasksQueue(ZDateTime nextScheduleDate)
		{
			var taskFilter = GetScheduleTaskQuery(nextScheduleDate);
			var count = GetEstimatedLoadCount(taskFilter);
			var age = 0;
			using (var cmd = Db.Connection.Command(ageSqlSelect))
			{
				cmd.AddParameter("@ParentTableCode", SqlDbType.VarChar, parentTableCode.ToString());
				cmd.AddParameter("@NextScheduleDate", SqlDbType.DateTime, nextScheduleDate);
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						age = reader.GetInt32(0);
					}
				}
			}
			var ageInSeconds = TimeSpan.FromSeconds(age);
			return new QueueResult(count, ageInSeconds);
		}

		const string ageSqlSelect = "SELECT ISNULL(MAX(DATEDIFF(second, S5_NextScheduledPrintRunTimeUtc, GETUTCDATE())), 0) FROM dbo.StmScheduleTask WHERE S5_ParentTableCode = @ParentTableCode and S5_IsActive = 1 and S5_NextScheduledPrintRunTimeUtc <= @NextScheduleDate";

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();
			result.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, parentTableCode);
			return result;
		}

		readonly ZString parentTableCode;
	}
}
