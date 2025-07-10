using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	[DebuggerDisplay("{TransmissionCode} -> {ResponseCode}")]
	sealed class Dialog : IDialog
	{
		public Dialog(StmALog[] logs, bool orderByLocalTime)
		{
			this.logs = new Lazy<StmALog[]>(() => GetSanitizedLogs(logs, orderByLocalTime).ToArray());

			this.transmittedMessageText = new Lazy<string>(GetTransmittedMessageText);
			this.lazyTransmissionCode = new Lazy<string>(GetTransmissionCode);
			this.lazyResponseCode = new Lazy<string>(GetResponseCode);
		}

		readonly Lazy<StmALog[]> logs;

		public StmALog[] Logs => logs.Value;

		IEnumerable<StmALog> GetSanitizedLogs(StmALog[] sourceLogs, bool orderByLocalTime)
		{
			List<StmALog> result = new List<StmALog>();

			if (sourceLogs == null)
			{
				return result;
			}

			var messageTypes = new List<string>(MessageEventCodes.All);
			var orderedLogs = orderByLocalTime ? sourceLogs.OrderBy(log => log.SL_EventTime) : sourceLogs.OrderBy(log => log.SL_PostedTimeUtc);

			foreach (var log in orderedLogs)
			{
				var eventCode = log.SL_SE_NKEvent;

				if (messageTypes.Contains(eventCode))
				{
					result.Add(log);
					log.Factory.AddFetchHint(GenPivotSchema.XX_Relation1ID, log.PK);
					messageTypes.Remove(eventCode);
				}
			}

			return result;
		}

		#region TransmittedMessageText

		public string TransmittedMessageText => transmittedMessageText.Value;
		readonly Lazy<string> transmittedMessageText;

		string GetTransmittedMessageText()
		{
			return logs
				.Value
				.Where(log => log.SL_SE_NKEvent == Events.DataExportCode)
				.Select(log => log.RelatedEDIMessage)
				.Where(msg => msg != null && msg.Message != null)
				.Select(msg => msg.Message.EM_MessageText)
				.FirstOrDefault();
		}

		#endregion

		public string TransmissionCode => lazyTransmissionCode.Value;

		readonly Lazy<string> lazyTransmissionCode;

		string GetTransmissionCode()
		{
			var applicableEventCodes = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);
			applicableEventCodes.Add(Events.StatusUpdatedCode);

			var sentLog = logs
				.Value
				.FirstOrDefault(log => applicableEventCodes.Contains(log.SL_SE_NKEvent.ToString()));

			return sentLog != null
				? sentLog.SL_SE_NKEvent.ToString()
				: string.Empty;
		}

		#region ResponseCode

		public string ResponseCode => lazyResponseCode.Value;

		readonly Lazy<string> lazyResponseCode;

		string GetResponseCode()
		{
			var receivedLog = logs
				.Value
				.LastOrDefault(log => MessageEventCodes.MessageStatusConfirmationEventCodes.Contains(log.SL_SE_NKEvent.ToString()));

			return receivedLog != null
				? receivedLog.SL_SE_NKEvent.ToString()
				: string.Empty;
		}

		#endregion
	}
}
