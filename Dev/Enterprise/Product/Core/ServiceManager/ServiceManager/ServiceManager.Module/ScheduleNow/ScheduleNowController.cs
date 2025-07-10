using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;

namespace Enterprise.ServiceManager.Module.ScheduleNow
{
	class ScheduleNowController : IScheduleNowController
	{
		public void ScheduleNow(IEnumerable<string> taskCodes, Action<string> callbackAction, bool forceRestart)
		{
			_ = taskCodes ?? throw new ArgumentNullException(nameof(taskCodes));
			_ = callbackAction ?? throw new ArgumentNullException(nameof(callbackAction));

			var serviceHostsCache = ObjectFactory.Get<IServiceHostsCache>();

			if (!serviceHostsCache.ConfiguredServiceHosts.Any())
			{
				callbackAction(GetErrorMessage(Res.GetString("B84A83E9-3EF1-4845-AF06-DD60F6C7C934", "There are no Hosts available.")));
				return;
			}

			var syncContext = SynchronizationContext.Current;

			ThreadPool.QueueUserWorkItem(
				state =>
				{
					using (Res.TemporarilySwitchLanguage((string)state))
					{
						var aggregateResult = serviceHostsCache.CallAllConfiguredHostsUntilFirstSuccess(
							client => client.InvokeActionOnTasks("schedule", taskCodes, "echoes=0", $"force={forceRestart}", $"user={GlbStaff.CurrentUser.GS_Code}"));
						var successfulResult = aggregateResult.Results.SingleOrDefault(result => result.IsSuccessful);
						Action<string> localCallbackAction = callbackAction;
						if (syncContext != null)
						{
							localCallbackAction = (message => syncContext.Post(_ => callbackAction(message), null));
						}
						if (successfulResult != null)
						{
							localCallbackAction(GetSuccessfulMessage(successfulResult.HostName, successfulResult.Result));
						}
						else
						{
							var errorMessages = string.Join(System.Environment.NewLine, aggregateResult.Results.Select(result => result.Exception.Message));
							localCallbackAction(GetErrorMessage(errorMessages));
						}
					}
				},
				Res.CurrentLanguage);
		}

		static string GetSuccessfulMessage(ServiceHostName hostName, TasksActionResultDTO tasksActionResult)
		{
			string hostString = Res.GetString("7D9B3667-3227-4F2E-A636-90DBC459571E", "Host:");
			var orderedResults = tasksActionResult.Results
				.Select(pair => $"{pair.Key}: {pair.Value}")
				.OrderBy(s => s);
			return $"{hostString} {hostName}{System.Environment.NewLine}{string.Join(System.Environment.NewLine, orderedResults)}";
		}

		static string GetErrorMessage(string errorMessage)
		{
			return Res.GetString("{544F50AC-07D2-417F-84B3-2A3350F3C657}", "Error during scheduling: {0}", errorMessage);
		}
	}
}
