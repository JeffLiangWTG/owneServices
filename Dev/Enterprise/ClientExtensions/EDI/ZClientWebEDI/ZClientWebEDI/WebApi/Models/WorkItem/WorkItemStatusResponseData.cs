using System;
using System.Collections.Generic;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WorkItem
{
	public sealed class WorkItemStatusResponseData
	{
		public IEnumerable<WorkItemStatus> Statuses { get; set; }
	}

	public sealed class WorkItemStatus
	{
		public Guid WorkItemPk { get; set; }

		public string WorkItemNumber { get; set; }

		public string Status { get; set; }
	}
}
