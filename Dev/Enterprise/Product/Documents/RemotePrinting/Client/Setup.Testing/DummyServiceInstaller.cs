using System.ComponentModel;
using System.ServiceProcess;
using WTG.StaticAnalysis.Annotation;

#if DEBUG
namespace Setup.Testing
{
	[CodeAlive("For testing only")]
	[RunInstaller(true)]
	public partial class DummyServiceInstaller : System.Configuration.Install.Installer
	{
		public DummyServiceInstaller()
		{
			ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
			ServiceInstaller serviceInstaller = new ServiceInstaller();

			//# Service Account Information
			serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
			serviceProcessInstaller.Username = null;
			serviceProcessInstaller.Password = null;

			//# Service Information
			serviceInstaller.DisplayName = "Dummy Remote Printing Test Service";
			serviceInstaller.Description = "Dummy Remote Printing Test Service for testing";
			serviceInstaller.StartType = ServiceStartMode.Automatic;

			//# This must be identical to the WindowsService.ServiceBase name
			//# set in the constructor of WindowsService.cs
			serviceInstaller.ServiceName = "Dummy Remote Printing Test Service";

			Installers.Add(serviceProcessInstaller);
			Installers.Add(serviceInstaller);
		}
	}
}
#endif