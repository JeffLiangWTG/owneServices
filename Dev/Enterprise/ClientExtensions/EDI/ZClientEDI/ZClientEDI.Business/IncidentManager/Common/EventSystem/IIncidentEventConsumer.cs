using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IIncidentEventConsumer : IWorkflowProvider
	{
		BusinessObjectFactory Factory { get; }
		bool IsMatchingEventTemplates { get; set; }
	}
}
