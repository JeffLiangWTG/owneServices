using System;
using System.Runtime.CompilerServices;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ScavengingImportServiceTask;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

#if DEBUG
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
#endif
[assembly: HostedService(
	"SIT",
	"Scavenging Import Service Task",
	"ESV",
	typeof(Enterprise.Client.EDI.ServiceTasks.ScavengingImportServiceTask),
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1minute"
	)]
namespace Enterprise.Client.EDI.ServiceTasks
{
	public class ScavengingImportServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var service = CreateServiceCore();
			try
			{
				var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					service.Process(token);
				}
			}
			catch (OperationCanceledException)
			{
				Notifier.Notify(new InfoNotification("Import canceled."));
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		internal virtual INotifications Notifier
		{
			get { return ServiceLogger.GetTaskNotificationSubscriber(); }
		}

		internal virtual IImportService CreateServiceCore()
		{
			var container = new ScavengingImportContainer(Notifier);
			return container.ResolveService();
		}
	}
}
