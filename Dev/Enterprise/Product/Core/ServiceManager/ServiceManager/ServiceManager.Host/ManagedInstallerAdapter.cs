using System.Configuration.Install;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ManagedInstallerAdapter : IManagedInstallerAdapter
	{
		public void Install(string[] args)
		{
			ManagedInstallerClass.InstallHelper(args);
		}
	}
}
