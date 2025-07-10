using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public interface IProcessRunnerPoolFactory
	{
		IProcessRunnerPool GetOrCreate();
	}
}
