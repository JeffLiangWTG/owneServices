using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Client.SWL.ServiceTasks;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"ZS1", 
	"Shipnet Data Interface", 
	"CSP",
	typeof(ShipnetServiceTask),
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "6hours"
	)]

namespace Enterprise.Client.SWL.ServiceTasks
{
	public class ShipnetServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(Integration.LogType.Information, "================== Task started ==================");

			var validCompanies = GlbCompany.GetActiveCompanies().Where(company => company.HasActiveBranch && SWLDataRegistry.Instance.IsShipnetEnable.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty)).ToArray();
			if (validCompanies.Length > 0)
			{
				foreach (var company in validCompanies)
				{
					youMustReactToThisToken.ThrowIfCancellationRequested();
					using (company.FirstActiveBranch.SetAsTemporaryContext())
					{
						ProcessShipnetDataExport();
					}
				}
			}
			else
			{
				ServiceLogger.Log(Integration.LogType.Warning, "Can not run Shipnet Data Exporter. Shipnet is not enabled in the registry.");
			}

			PurgeBackupFiles();

			ServiceLogger.Log(Integration.LogType.Information, "================== Task ended ==================");
		}
		ShipnetServiceTaskProcessor Processor;

		protected virtual void ProcessShipnetDataExport()
		{
			Processor = new ShipnetServiceTaskProcessor(ServiceLogger);
			Processor.Process();
		}

		#region IInteruptibleServiceTask Members

		public void Stop()
		{
			if (Processor != null)
			{
				Processor.CanContinue = false;
			}
		}

		#endregion

		void PurgeBackupFiles()
		{
			try
			{
				var backupDirectory = SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.Value;
				if (Directory.Exists(backupDirectory))
				{
					var timeToPurge = ZDateTime.Now.AddDays(-SWLDataRegistry.Instance.ShipnetPurgeBackupItem.Value);
					foreach (string file in Directory.GetFiles(backupDirectory, "*", SearchOption.AllDirectories))
					{
						FileInfo fileInfo = new FileInfo(file);
						if (fileInfo.CreationTime < timeToPurge)
						{
							fileInfo.Delete();
						}
					}
				}
			}
			catch (IOException e)
			{
				ServiceLogger.Log(Integration.LogType.Error, string.Format(CultureInfo.CurrentCulture, "Error encountered when purging files: {0}", e.Message));
			}
			catch (UnauthorizedAccessException e)
			{
				ServiceLogger.Log(Integration.LogType.Error, string.Format(CultureInfo.CurrentCulture, "Error encountered when purging files: {0}", e.Message));
			}
		}
	}
}
