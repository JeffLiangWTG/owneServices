using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Tasks.DbMaintenance;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(DbMaintenanceTasks.DocManagerDatabaseCreator, "DocManager Database Creator", "DDC", typeof(DocManagerDatabaseCreatorTask),
	IsMandatory = true,
	MinimumPeriod = "1day",
	MaximumPeriod = "1week",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "2hours",
	DefaultScheduleRandomStartOffset = "60minutes")
]

namespace Enterprise.ServiceManager.Tasks.DbMaintenance
{
	class DocManagerDatabaseCreatorTask : ServiceProviderImpl
	{
		public DocManagerDatabaseCreatorTask()
			: base()
		{
		}

		[HostedServiceRequirement]
		public static string CheckShouldRun() => HostedServiceRequirementAttribute.CheckValueIsEqualTo(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.DB);

		public override void RunTask(CancellationToken iDoNotNeedToReactToThisToken)
		{
			RunCore();
		}

		void RunCore()
		{
			var builder = new StringBuilder();
			try
			{
				var helper = ObjectFactory.Get<DocumentScanning.Integration.IDocManagerDBHelper>();
				var oldHighestNumber = helper.GetStorageDocDbNumbersIncludingMainDb().OrderByDescending(x => x).First();
				var result = helper.LastWritableDatabaseWithFreeSpace(0, true, builder);
				var log = builder.ToString();
				if (!string.IsNullOrEmpty(log))
				{
					ServiceLogger.Log(LogType.Error, log);
				}

				if (result > oldHighestNumber)
				{
					ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "eDocs database {0}_SD{1} created.", Db.DatabaseName, result.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0')));
				}
				else
				{
					ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Finished processing with no database created."));
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var log = builder.ToString();
				if (!string.IsNullOrEmpty(log))
				{
					ServiceLogger.Log(LogType.Error, log);
				}
				ServiceLogger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Error when attempting to create new database:\r\n{0}", e));
				ErrorReporter.ReportOnce("DDCFailure", log, e);
			}
		}
	}
}
