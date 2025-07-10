using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.Integration;
using static CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class PartialEventHandler : IPartialEventHandler
	{
		public void Handle(IStmALog log, IStmALogParent master)
		{
			if (log == null || master == null || log.SL_IsCancelled || !log.IsPartialLog())
			{
				return;
			}

			var incomingPartialLog = log.ToPartialEventLog();

			var partialLogs = master
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.ToPartialEventLogs(log.SL_SE_NKEvent)
				.Where(partialLog => partialLog.Total == incomingPartialLog.Total && partialLog.Location == incomingPartialLog.Location)
				.OrderByDescending(partialLog => partialLog.Log.SL_EventTime)
				.ToArray();

			if (partialLogs.Length <= 0)
			{
				return;
			}

			if (IsDuplicate(incomingPartialLog, partialLogs))
			{
				incomingPartialLog.Log.Cancel();
				return;
			}

			var sumOfPartialPieces = partialLogs.Sum(l => l.Partial);

			if (incomingPartialLog.Total <= sumOfPartialPieces)
			{
				var latestPartialLog = partialLogs.First();

				var parameters = GetParametersForCompletionLog(partialLogs.Select(partialLog => partialLog.Log))
					.ToDictionary(parameter => parameter.Key, parameter => parameter.Value);

				parameters[Location] = latestPartialLog.Location;
				parameters[Total] = incomingPartialLog.Total.ToString(CultureInfo.InvariantCulture);

				var eventValue = new EventValue(
					eventType: Events.All[log.SL_SE_NKEvent],
					eventTime: latestPartialLog.Log.SL_EventTime.ToOffset(),
					parameters: parameters);

				master.Logs.AddNewWithoutDuplicateCheck(eventValue);
			}
		}

		bool IsDuplicate(PartialEventLog log, IEnumerable<PartialEventLog> partialLogs)
		{
			return partialLogs.Any(partialLog =>
				log.Log != partialLog.Log
				&& Equals(log.Log, partialLog.Log));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Key Value Pairs are good.")]
		protected abstract IEnumerable<KeyValuePair<string, string>> GetParametersForCompletionLog(IEnumerable<IStmALog> logsOrderedByEventTimeDesc);

		protected abstract bool Equals(IStmALog log1, IStmALog log2);
	}
}
