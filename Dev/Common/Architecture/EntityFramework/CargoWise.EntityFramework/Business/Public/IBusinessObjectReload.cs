using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectReload
	{
		BusinessObject Reload(BusinessObjectFactory loadingFactory);
		SchemaColumn ReloadPerformanceIncreaseColumn { get; }
		object ReloadPerformanceIncreaseColumnValue { get; }
	}
}
