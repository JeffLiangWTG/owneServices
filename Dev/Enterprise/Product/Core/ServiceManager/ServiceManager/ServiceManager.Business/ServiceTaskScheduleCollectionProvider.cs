using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Business
{
	public class ServiceTaskScheduleCollectionProvider : IServiceTaskScheduleCollectionProvider
	{
		public IServiceTaskScheduleCollection Load(BusinessObjectFactory businessObjectFactory, ServiceTaskScheduleCollection.RemoteStatus remoteStatus = ServiceTaskScheduleCollection.RemoteStatus.WithStatus)
		{
			var collection = new ServiceTaskScheduleCollection(businessObjectFactory, remoteStatus);
			collection.Load();
			return collection;
		}

		public IServiceTaskScheduleCollection LoadUpdatedFromTime(BusinessObjectFactory businessObjectFactory, ZDateTime fromTimeUtc)
		{
			var collection = new ServiceTaskScheduleCollection(businessObjectFactory, ServiceTaskScheduleCollection.RemoteStatus.WithoutStatus);
			collection.LoadWithMoreFiltering(new ZQuery(StmScheduleTaskSchema.S5_SystemLastEditTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, fromTimeUtc));
			return collection;
		}
	}
}
