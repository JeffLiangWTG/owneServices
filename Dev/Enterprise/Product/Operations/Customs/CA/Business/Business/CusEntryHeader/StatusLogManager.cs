namespace Enterprise.Customs.CA.Business
{
	using CargoWise.Types;
	using Enterprise.Customs.Business.Extensions;
	using Enterprise.ZArchitecture.Business;

	static class StatusLogManager
	{
		internal static void AddCustomsCommencedEvent(Logs logs, string reference)
		{
			var mostRecentCommencedLog = logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsCommenced, reference);
			if (mostRecentCommencedLog == null)
			{
				logs.AddNew(Events.CustomsCommenced, reference);
			}
		}

		internal static void AddCustomsClearedEvent(Logs logs, string reference, ZDateTimeOffset time)
		{
			if (logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.CustomsCleared) == null)
			{
				logs.AddNew(AutoEvents.CustomsCleared, reference, time);
			}
		}

		internal static void CancelCustomsClearedEvent(Logs logs)
		{
			var log = logs.MostRecentLogByEventTimeExcludingEstimated(AutoEvents.CustomsCleared);
			if (log != null)
			{
				log.Cancel();
				logs.AddNew(AutoEvents.Cancelled, "Customs Cleared event canceled");
			}
		}

		public static StmALog AddALogIfNecessary(Logs logs, ZString status)
		{
			StmALog result = null;
			if (status != MessageStatusList.Codes.NotSent)
			{
				result = AddAStatusLog(logs, status);
			}
			return result;
		}

		public static StmALog AddAStatusLog(Logs logs, ZString status)
		{
			var logsForNominatedEvent = new LogsForNominatedEvent(logs, AutoEvents.CustomsEntryStatus, true);
			return logsForNominatedEvent.AddNew(status);
		}

		public static bool HasAClearLog(Logs logs, ZString messageType, IStatusList list)
		{
			var result = ZBool.False;
			ZString status = list.GetFirstClearStatusFor(MessageTypeList.GetMessagesTypesRightFor(messageType));
			if (!status.IsEmpty)
			{
				var logsForNominatedEvent = new LogsForNominatedEvent(logs, AutoEvents.CustomsEntryStatus, true);
				result = logsForNominatedEvent.DescriptionExists(status);
			}

			return result;
		}

		internal static void AddCustomsReadyToPayEvent(Logs logs, ZDateTimeOffset time)
		{
			if (logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay) == null)
			{
				logs.AddNew(Events.CustomsReadyToPay, time);
			}
		}

		internal static void CancelCustomsReadyToPayEvent(Logs logs)
		{
			var log = logs.MostRecentLogByEventTimeExcludingEstimated(Events.CustomsReadyToPay);
			if (log != null)
			{
				if (log.IsInDatabase)
				{
					log.Cancel();
				}
				else
				{
					log.Delete();
				}
			}
		}
	}
}
