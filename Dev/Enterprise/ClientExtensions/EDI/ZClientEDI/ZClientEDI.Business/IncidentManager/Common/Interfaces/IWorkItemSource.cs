using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IWorkItemSource : IBusiness
	{
		void PopulateWorkItem(NewWorkItem workItem);
	}
}
