using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Host
{
	class CommandRequestHandler : RequestHandler
	{
		public CommandRequestHandler(ITaskScheduler taskScheduler, IJsonConverter jsonConverter, string hostName)
		{
			this.taskScheduler = taskScheduler;
			this.jsonConverter = jsonConverter;
			Uri = ServiceManagerHelper.GetCommandUri(hostName);
		}

		public override Uri Uri { get; }

		protected override string HandleCore(IHttpRequestInfo request)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return GetCommandResult(request);
			}
		}

		string GetCommandResult(IHttpRequestInfo request)
		{
			var action = request.QueryString["action"];
			var tasks = RequestInfoHelper.QueryCsvString(request, "tasks");

			if (tasks.Length == 0 && !string.IsNullOrEmpty(action))
			{
				return Invariant($"{RequestProcessor.ErrorPrefix} You need to provide at least one task for command: {action}");
			}

			if (string.Equals(action, "schedule", StringComparison.OrdinalIgnoreCase))
			{
				var result = taskScheduler.ScheduleTasks(
					tasks.Select(code => new TaskCodeDTO(code)).ToList(),
					RequestInfoHelper.QueryUInt(request, "echoes", 1) > 0,
					RequestInfoHelper.QueryTimeSpanFromSeconds(request, "delayInSeconds"),
					RequestInfoHelper.QueryString(request, "user"));
				return jsonConverter.Serialize(result);
			}

			if (string.Equals(action, "requestConfigReload", StringComparison.OrdinalIgnoreCase))
			{
				var result = taskScheduler.RequestReloadOfTaskConfiguration(tasks.Select(code => new TaskCodeDTO(code)).ToList());
				return jsonConverter.Serialize(result);
			}

			if (string.Equals(action, "setNextRuntime", StringComparison.OrdinalIgnoreCase)
				&& ZDateTime.TryParseExact(request.QueryString["nextRuntime"], out var nextRunTime, ServiceManagerConstants.JsonDateTimeFormat))
			{
				var utcDateTime = new ZDateTime(nextRunTime, DateTimeKind.Utc);
				var result = taskScheduler.SetNextRuntime(tasks.Select(code => new TaskCodeDTO(code)).ToList(), utcDateTime.ToNullableDateTimeOffset(), RequestInfoHelper.QueryBool(request, "reverting"));
				return jsonConverter.Serialize(result);
			}

			return Invariant($"{RequestProcessor.ErrorPrefix} Unknown command: {request.Uri.Query}");
		}

		readonly ITaskScheduler taskScheduler;
		readonly IJsonConverter jsonConverter;
	}
}
