using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.NudgingClient;
using ServiceManager.Integration.NudgingClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.Exceptions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;
using ServiceManager.Integration.ServiceHostClient.Exceptions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW;
using WTG.StaticAnalysis.Annotation;
using NudgeEventArgs = ServiceManager.Integration.Abstractions.NudgeEventArgs;
using NudgeFailedEventArgs = ServiceManager.Integration.Abstractions.NudgeFailedEventArgs;

namespace ServiceManager.Integration.CW
{
	[ThreadSafe]
	sealed class NudgingController : INudgingController
	{
		public NudgingController()
			: this(
				new CommunicationFailureNotification(),
				HostedServiceBusinessObjectBindingsProvider.Instance,
				new PredicateFactory(new NudgingSchemaResolver(new EnterpriseSchemaResolver())),
				() => new NudgeClient(ObjectFactory.Get<IServiceHostsCache>()))
		{
		}

		internal NudgingController(
			INudgingFailedUserNotification nudgingFailedUserNotification,
			IHostedServiceBusinessObjectBindingsProvider bindingsProvider,
			IPredicateFactory predicateFactory,
			Func<INudgeClient> nudgeClientFactory)
		{
			var localNudgingFailedUserNotification = nudgingFailedUserNotification ?? throw new ArgumentNullException(nameof(nudgingFailedUserNotification));
			var localBindingsProvider = bindingsProvider ?? throw new ArgumentNullException(nameof(bindingsProvider));
			var localPredicateFactory = predicateFactory ?? throw new ArgumentNullException(nameof(predicateFactory));
			this.nudgeClientFactory = nudgeClientFactory ?? throw new ArgumentNullException(nameof(nudgeClientFactory));

			localNudgingFailedUserNotification.Attach(this);
			bindingsList = new Lazy<IEnumerable<IServiceTaskBinding>>(() =>
			{
				return localBindingsProvider.BusinessObjectBindings
					.Select(binding => new { binding, predicates = localPredicateFactory.GeneratePredicates(binding.Predicates, binding.Table) })
					.Select(arg => new ServiceTaskBinding(arg.binding.ServiceTaskCode, arg.binding.Table, arg.predicates))
					.Cast<IServiceTaskBinding>()
					.ToList();
			});
			nudgeClient = InitNudgeClient();
		}

		Lazy<INudgeClient> InitNudgeClient()
		{
			return new Lazy<INudgeClient>(() =>
			{
				var client = nudgeClientFactory();
				client.Nudged += OnNudged;
				client.NudgeFailed += OnNudgeFailed;
				return client;
			});
		}

		void OnNudged(object? sender, NudgingClient.Abstractions.EventArgs.NudgeEventArgs e)
		{
			var successful = new List<ITaskNudged>();
			var unSuccessful = new List<ITaskNudged>();
			foreach (var taskNudged in e.Tasks)
			{
				if (taskNudged.TaskActionOutcome.IsSuccessful())
				{
					successful.Add(taskNudged);
				}
				else
				{
					unSuccessful.Add(taskNudged);
				}
			}

			if (successful.Count > 0)
			{
				ReportNudgeSucceeded(successful.Select(nudged => nudged.TaskCode));
			}

			if (unSuccessful.Count > 0)
			{
				var reportingGroups = unSuccessful
					.GroupBy(nudged => (nudged.TaskActionOutcome, nudged.RetriesRemained), nudged => nudged.TaskCode);

				foreach (var pair in reportingGroups)
				{
					ReportNudgeFailed(pair.AsEnumerable(), pair.Key.TaskActionOutcome.ToString(), pair.Key.RetriesRemained);
				}
			}
		}

		void OnNudgeFailed(object? sender, NudgingClient.Abstractions.EventArgs.NudgeFailedEventArgs e)
		{
			var reportingGroups = e.Tasks
				.GroupBy(nudged => nudged.RetriesRemained, nudged => nudged.TaskCode);

			foreach (var pair in reportingGroups)
			{
				ReportNudgeFailed(pair.AsEnumerable(), e.Exception, pair.Key);
			}
		}

		public event EventHandler<NudgeFailedEventArgs>? NudgeFailedEvent;
		public event EventHandler<NudgeEventArgs>? NudgeTrackingEvent;

		public void ScheduleTasks(IEnumerable<string> taskCodes, bool? echoes = null, TimeSpan? delay = null)
		{
			nudgeClient.Value?.ScheduleTasks(taskCodes ?? throw new ArgumentNullException(nameof(taskCodes)), echoes, delay);
		}

		public IEnumerable<IServiceTaskBinding> ServiceTaskBindings => bindingsList.Value;

		public void ReportNudgeFailed(IEnumerable<string> taskCodes, Exception ex, int retriesRemaining)
		{
			if (!IsIgnoredExceptionType(ex))
			{
				ErrorReporter.ReportOnce("Exception during nudging", ex);
			}

			using (Db.DisposableActionForDbConnection())
			{
				NudgeFailedEvent?.Invoke(this, new NudgeFailedEventArgs(taskCodes, ex, retriesRemaining));
			}

			bool IsIgnoredExceptionType(Exception e)
			{
				return
					e is ServiceHostCommunicationException
					|| e is UnsupportedTaskException
					|| e is TaskCanceledException
					|| e is NoAvailableHostsException;
			}
		}

		public void ReportNudgeFailed(IEnumerable<string> taskCodes, string description, int retriesRemaining)
		{
			using (Db.DisposableActionForDbConnection())
			{
				NudgeFailedEvent?.Invoke(this, new NudgeFailedEventArgs(taskCodes, description, retriesRemaining));
			}
		}

		public void ReportNudgeSucceeded(IEnumerable<string> taskCodes)
		{
			ReportNudgeEventArgs(new NudgeSucceededEventArgs(taskCodes));
		}

		public void ReportNudgeStarted(IEnumerable<string> taskCodes, StackTrace stackTrace)
		{
			ReportNudgeEventArgs(new NudgeStartedEventArgs(taskCodes, stackTrace));
		}

		public void ReportNudgeIgnored(IEnumerable<string> taskCodes, string description)
		{
			ReportNudgeEventArgs(new NudgeIgnoredEventArgs(taskCodes, description));
		}

		public void ReportNudgeDeferred(IEnumerable<string> taskCodes, string description)
		{
			ReportNudgeEventArgs(new NudgeDeferredEventArgs(taskCodes, description));
		}

		public void ReportNudgeAbandoned(IEnumerable<string> taskCodes, string description)
		{
			ReportNudgeEventArgs(new NudgeAbandonedEventArgs(taskCodes, description));
		}

		void ReportNudgeEventArgs(NudgeEventArgs eventArgs)
		{
			using (Db.DisposableActionForDbConnection())
			{
				NudgeTrackingEvent?.Invoke(this, eventArgs);
			}
		}

		readonly Lazy<IEnumerable<IServiceTaskBinding>> bindingsList;
		readonly Func<INudgeClient> nudgeClientFactory;
		readonly Lazy<INudgeClient> nudgeClient;
	}
}
