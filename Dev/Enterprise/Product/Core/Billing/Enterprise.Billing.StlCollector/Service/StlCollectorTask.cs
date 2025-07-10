using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.RefDataRepo.Ent.Client;
using CargoWise.Types;
using Enterprise.Billing.StlCollector.Retriever;
using Enterprise.Billing.StlCollector.Service;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	StlCollectorTask.ServiceTaskCode,
	StlCollectorTask.ServiceTaskName,
	"SYS",
	typeof(StlCollectorTask),
	IsMandatory = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "1hour",
	IsScheduleReadOnly = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)]

[assembly: HostedServiceQueueProvider(
	StlCollectorTask.ServiceTaskCode,
	StlCollectorTask.ServiceTaskName,
	typeof(StlCollectorTaskQueue))]

namespace Enterprise.Billing.StlCollector.Service
{
	public class StlCollectorTaskQueue : IHostedServiceQueueProvider
	{
		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public QueueResult QueueResult
		{
			get
			{
				if (!StlCollectorTask.IsEnabled(out _))
				{
					return QueueResult.Zero;
				}
				var stlCollectors = ObjectFactory.Get<IScriptFactory>().CreateScripts(new BusinessObjectFactory() { RefreshEnabled = false }).ToDictionary(f => f.Code.ToUpper());
				var sqlText = @"
SELECT sd_name, cast(cast(SD_BinaryValue as nvarchar(max)) as datetime) FROM dbo.StmData
WHERE sd_name like 'WATERBILLDATE%' AND sd_name <> 'WATERBILLDATE' AND
cast(cast(sd_Binaryvalue as nvarchar(max)) as datetime) < @expectedWaterBillDate";
				using (var cmd = Db.Connection.Command(sqlText))
				{
					var queueSize = 0;
					var oldestItemAge = TimeSpan.Zero;
					cmd.AddParameter("@expectedWaterBillDate", SqlDbType.DateTime, StlCollectorTask.ExpectedWaterBillDate);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var waterMarkName = reader[0].ToString();
							var code = waterMarkName.Substring(13).ToUpper();
							if (stlCollectors.ContainsKey(code))
							{
								++queueSize;
								var itemAge = !reader.IsDBNull(1) ? ZDateTime.UtcNow - reader.GetDateTime(1) : TimeSpan.Zero;
								if (itemAge > oldestItemAge)
								{
									oldestItemAge = itemAge;
								}
							}
						}
					}

					return new QueueResult(queueSize, oldestItemAge);
				}
			}
		}
	}

	public class StlCollectorTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "STL";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System Service Task")]
		public const string ServiceTaskName = "STL Collector Service";

		readonly IStlRetrieverFactory retrieverFactory;

		public StlCollectorTask()
			: this(new StlRetrieverFactory())
		{
		}

		public StlCollectorTask(IStlRetrieverFactory stlRetrieverFactory)
		{
			retrieverFactory = stlRetrieverFactory;
		}

		public static DateTime ExpectedWaterBillDate
		{
			get
			{
				return new CollectionTimeProvider().MaximumSafeEndDateTimeExclusive.AddHours(-1);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "literal String is safe to use in this context")]
		public static bool IsEnabled(out string message)
		{
			message = string.Empty;
			var isDisabledThroughRefDb = new RefSysConfig.Loader(new BusinessObjectFactory()).GetBoolValue("STOPSTLUSS");
			if (isDisabledThroughRefDb)
			{
				message = "STL collection has been disabled through reference data.";
				return false;
			}

			if (!IsProductionOreHubTestingEnabled)
			{
				message = "STL Collection is Disabled, as this is not a production system. Please turn on System->Testing->eHub Testing registry explicitly and leave System->Remote Database Repository-> Service Uri registry at it's default value if you want to submit STL collection data to test eHub.";
				return false;
			}

			return true;
		}

		public static bool IsProductionOreHubTestingEnabled
		{
			get
			{
				var productRegistration = ObjectFactory.Get<IProductRegistration>();
				return !productRegistration.IsWiseTechGlobalInternalSystem() || (DataRegistry.Instance.EHubTesting && UsingDefaultReferenceDatabase);
			}
		}

		static bool UsingDefaultReferenceDatabase
		{
			get
			{
				return string.Compare(RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.Value, RemoteDatabaseRegistry.Instance.RemoteDatabaseServiceUri.DefaultValue, StringComparison.OrdinalIgnoreCase) == 0;
			}
		}

		public override void RunTask(CancellationToken token)
		{
			if (!IsEnabled(out var message))
			{
				ServiceLogger.Log(LogType.Information, message);
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "[STL Collection] - Start");
				var retriever = retrieverFactory.Create(ServiceLogger);
				retriever.CollectAndSend(token);
				ServiceLogger.Log(LogType.Information, "[STL Collection] - End");
			}
		}
	}
}
