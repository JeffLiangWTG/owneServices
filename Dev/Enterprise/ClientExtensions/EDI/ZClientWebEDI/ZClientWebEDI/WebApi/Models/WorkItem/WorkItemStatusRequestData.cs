using System.Collections.Generic;

namespace Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.WorkItem
{
	public sealed class WorkItemStatusRequestData
	{
		public IEnumerable<string> WorkItemNumbers { get; set; }
	}
}
