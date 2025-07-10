using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Modules
{
	public interface IGlowReportingModuleDataQueryProvider
	{
		ZQuery BuildQuery();
	}
}
