using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface IIncidentDetailsSource
	{
		BusinessObjectFactory Factory { get; }
		ZString Product { get; }
		ZString Criticality { get; }
		ModuleListType ModuleType { get; }
		ZString ProductArea { get; }
		ZString Module { get; }
		ZString SourceModuleId { get; }
	}
}
