using System;
using System.Collections.Generic;
using System.Threading;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly:
	HostedService(
		Enterprise.AuditDataServices.DataScience.DataScienceSubscriberServiceTask.Code,
		Enterprise.AuditDataServices.DataScience.DataScienceSubscriberServiceTask.Description,
		"BI",
		typeof(Enterprise.AuditDataServices.DataScience.DataScienceSubscriberServiceTask),
		CanRunInAnyBranch = true,
		IsMandatory = true,
		AllowsMultipleInstances = false,
		MinimumPeriod = "1minute",
		MaximumPeriod = "30minutes",
		DefaultScheduleRunEvery = "5minutes",
		ActiveByDefault = true
	)
]

namespace Enterprise.AuditDataServices.DataScience
{
	public class DataScienceSubscriberServiceTask : ClientSpecificAuditSubscriberTask
	{
		public const string Code = "D5S";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task name")]
		public const string Description = "Data Science subscriber service task";

		public override string ServiceTaskCode => Code;
		public override string ServiceTaskDescription => Description;
		public override string SubscriberCode => SubscriberLoader.DataScienceSubscriberCode;
		public override string AssemblyName => SubscriberLoader.ZClientEdiBusinessAssemblyName;
		public override string SubscriberNamespace => SubscriberLoader.DataScienceNamespace;

		// TODO: remove me once the investigation part of WI00837801 is concluded
		static class SubscriberLogFlag
		{
			[WTG.StaticAnalysis.Annotation.ThreadSafe]
			public static int value = 1;
		}

		protected override IEnumerable<IAuditSubscriber> GetSubscribers()
		{
			try
			{
				var subscribers = base.GetSubscribers();

				// TODO: remove the following logs once the investigation part of WI00837801 is concluded
				if (Interlocked.CompareExchange(ref SubscriberLogFlag.value, 0, 1) == 1 && ServiceLogger is ILogger logger)
				{
					foreach (var subscriber in subscribers)
					{
						if (subscriber is IActualDataChangesAuditSubscriber tableSubscriber)
						{
							logger.Debug($"Using audit subscriber [{subscriber.Code}: {subscriber.GetType().FullName}] for table [{tableSubscriber.Table.SqlSchemaName}].[{tableSubscriber.Table.TableName}]");
						}
						else
						{
							logger.Debug($"Using audit subscriber [{subscriber.Code}: {subscriber.GetType().FullName}]");
						}
					}
				}

				return subscribers;
			}
			catch (Exception error)
			{
				ServiceLogger?.Error($"{GetType().FullName}.GetSubscribers() failed with exception [{error.GetType().FullName}]({error.Message})", error);
				throw;
			}
		}

		public override void RunTask(CancellationToken token)
		{
			try
			{
				ServiceLogger?.Debug($"RunTask() started: {ServiceCode}");  // TODO: remove me once the investigation part of WI00837801 is concluded
				base.RunTask(token);
				ServiceLogger?.Debug($"RunTask() finished: {ServiceCode}"); // TODO: remove me once the investigation part of WI00837801 is concluded
			}
			catch (Exception error)
			{
				ServiceLogger?.Error($"{GetType().FullName}.RunTask() failed with exception [{error.GetType().FullName}]({error.Message})", error);
				throw;
			}
		}
	}
}
