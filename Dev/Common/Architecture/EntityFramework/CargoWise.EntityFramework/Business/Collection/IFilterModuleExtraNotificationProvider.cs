using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Implement this on a collection used in filter module to add extra notifications on search results
	/// </summary>
	public interface IFilterModuleExtraNotificationProvider
	{
		INotification GetExtraNotification(BusinessObject businessObject);
	}
}
