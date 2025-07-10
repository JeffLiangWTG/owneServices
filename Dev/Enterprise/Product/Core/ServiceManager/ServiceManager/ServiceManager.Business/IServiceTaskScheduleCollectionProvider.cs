using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ServiceManager.Business
{
	public interface IServiceTaskScheduleCollectionProvider
	{
		IServiceTaskScheduleCollection Load(BusinessObjectFactory businessObjectFactory, ServiceTaskScheduleCollection.RemoteStatus remoteStatus = ServiceTaskScheduleCollection.RemoteStatus.WithStatus);
		IServiceTaskScheduleCollection LoadUpdatedFromTime(BusinessObjectFactory businessObjectFactory, ZDateTime fromTimeUtc);
	}
}
