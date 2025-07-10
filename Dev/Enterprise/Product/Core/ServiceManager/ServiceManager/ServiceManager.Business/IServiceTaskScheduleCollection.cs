using CargoWise.EntityFramework;

namespace Enterprise.ServiceManager.Business
{
	public interface IServiceTaskScheduleCollection
	{
		BusinessObjectCollection<ServiceTaskSchedule> Tasks { get; }
	}
}
