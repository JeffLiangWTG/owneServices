using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IStmALogProvider
	{
		Logs Logs { get; }
		BusinessObjectFactory LogsFactory { get; }
	}
}
