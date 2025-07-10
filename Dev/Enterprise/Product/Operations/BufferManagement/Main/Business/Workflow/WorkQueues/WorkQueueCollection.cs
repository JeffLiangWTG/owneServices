using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkQueueCollection : ActiveBusinessObjectCollection<WorkQueue>
	{
		public WorkQueueCollection(BusinessObjectFactory factory)
			: base(factory, TagProvider.GetWorkQueuesTagGroup(factory), new ZQuery(), TagMagnitudeSchema.TGM_TGD_Tag)
		{
		}
	}
}
