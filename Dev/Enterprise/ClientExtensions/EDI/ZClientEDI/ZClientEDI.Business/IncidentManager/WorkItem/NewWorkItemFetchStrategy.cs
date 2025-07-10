using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	internal class NewWorkItemFetchStrategy : ProcessManagement.Business.WorkItemFetchStrategy
	{
		public NewWorkItemFetchStrategy(NewWorkItem workitem)
				: base(workitem)
		{
			this.workitem = workitem;
		}

		readonly NewWorkItem workitem;

		protected override TableColumn GetColumn(TableColumn[] columns)
		{
			var result = columns.FirstOrDefault(
				c => c.ColumnName.Contains(nameof(NewWorkItem.RelatedClientCode))
				|| c.ColumnName.Contains(nameof(NewWorkItem.CriticalityBasedOnRelatedIncidents)));

			return result ?? base.GetColumn(columns);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var columnNames = columns.Select(x => x.ColumnName);
			AddProcessTasksFetchHint(columnNames);
		}

		protected void AddProcessTasksFetchHint(IEnumerable<string> columnNames)
		{
			if (columnNames.Any(x => x.StartsWith(nameof(NewWorkItem.AssignedToCode)) || x.StartsWith(nameof(NewWorkItem.OverallTaskStatusCode)) ||
			x.StartsWith(nameof(NewWorkItem.OverallTaskStatusDescription)) || x.StartsWith(nameof(NewWorkItem.CurrentOrNextTask)) ||
			x.StartsWith(nameof(NewWorkItem.CurrentTask)) || x.StartsWith(nameof(NewWorkItem.JobAgreedDeliveryDate))))
			{
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, workitem.PK);
				Factory.AddFetchHint(ProcessHeaderSchema.FH_ParentId, workitem.PK);
			}
		}
	}
}
