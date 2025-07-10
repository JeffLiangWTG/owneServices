using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public interface IWorkItemCreator
	{
		IEnumerable<CreateWorkItemResult> CreateWorkItems(IEnumerable<WorkItemDTO> items);
		public WorkItemDTO CreateDefaultWorkItemDTO();
		ValueTask CopyEmails(ValueTask copyEmailTask);
	}
}
