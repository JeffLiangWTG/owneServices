using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class WorkflowExceptionGenerationProcessor : IProcessor
	{
		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				var today = ZDateTime.UtcToday;
				try
				{
					new Batcher(today, isExpired: false).Process(notifications, token);
					new Batcher(today, isExpired: true).Process(notifications, token);
					SystemDataRegistry.Instance.WorkflowExceptionGenerationHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, today.AddDays(-1).ToDateTime());
				}
				catch (Exception ex)
				{
					notifications.AddError(FormattableString.Invariant($"Error occurred during processing:\r\n{ex.Print()}"));
					throw;
				}
			}
		}

		public class Batcher : ManagedBatchProcessor<ProcessTask>
		{
			public Batcher(ZDateTime today, bool isExpired)
			{
				this.today = today;
				this.isExpired = isExpired;
			}
			readonly ZDateTime today;
			readonly bool isExpired;

			protected override ZQuery GetSingularQuery(ProcessTask row) => new ZQuery(ProcessTasksSchema.PK, row.PK);
			protected override void MarkRowAsBadCore(INotifications notifications, ProcessTask row, BusinessObjectFactory factory) => row.P9_MilestoneExceptionAdded = ZDateTime.UtcNow;
			protected override IList<ProcessTask> LoadBatchCore(BusinessObjectFactory factory, ZQuery query) => factory.Load<ProcessTask>(query);

			protected override void ProcessRowCore(INotifications notifications, CancellationToken token, ProcessTask milestone, BusinessObjectFactory factory, (int, int) position)
			{
				if (milestone.P9_ActualDate.IsEmpty)
				{
					milestone.CreateMilestoneException();
				}
			}

			protected override ZQuery GetQuery()
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
				if (SystemDataRegistry.Instance.WorkflowExceptionGenerationHWM.Value != DateTime.MinValue)
				{
					query.AddToFilter(ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.GreaterThanOrEqualTo, SystemDataRegistry.Instance.WorkflowExceptionGenerationHWM.Value.AddMinutes(-5));
				}

				if (isExpired)
				{
					ZQuery expiredQuery = new ZQuery(ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcNow);
					expiredQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, SQLComparisonOperator.Equal, ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired);

					query.AddToFilter(expiredQuery);
				}
				else
				{
					query.AddToFilter(ProcessTasksSchema.P9_ScheduledDateUtc, SQLComparisonOperator.LessThan, today);
				}

				query.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);
				query.AddToFilter(ProcessTasksSchema.P9_ActualDate, ZDateTime.Empty);
				query.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, SQLComparisonOperator.NotEqual, ZString.Empty);
				query.AddToFilter(ProcessTasksSchema.P9_MilestoneExceptionAdded, ZDateTime.Empty);

				return query;
			}
		}
	}
}
