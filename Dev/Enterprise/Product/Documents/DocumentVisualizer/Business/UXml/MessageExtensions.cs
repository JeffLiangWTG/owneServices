using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.DocumentVisualizer.Business
{
	public static class MessageExtensions
	{
		#region CalculateSubmissionVersion

		public static int CalculateSubmissionVersion(this IStmALogParent logParent, string documentName)
		{
			if (logParent == null)
			{
				return 1;
			}

			return logParent
				.Logs
				.Find(log => log.SL_SE_NKEvent == Events.MessageSentCode
					&& log.MatchesDocumentName(documentName))
				.Count() + 1;
		}

		#endregion

		#region HasSentDocument

		public static bool HasSentDocument(this IStmALogParent logParent, string documentName)
		{
			if (logParent == null || string.IsNullOrEmpty(documentName))
			{
				return false;
			}

			return logParent.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.MessageSentCode && log.MatchesDocumentName(documentName));
		}

		#endregion

		#region CalculateDataVersion

		public static int CalculateDataVersion(this IVisualizerDocumentData data, string documentName, bool orderByLocalTime)
		{
			return CalculateDataVersion(data as IStmALogParent, documentName, orderByLocalTime);
		}

		public static int CalculateDataVersion(this IStmALogParent logParent, string documentName, bool orderByLocalTime)
		{
			if (logParent == null)
			{
				return 1;
			}

			var version = 1;

			var dialogs = logParent
				.GetDialogs(documentName, orderByLocalTime)
				.ToArray();

			foreach (var dialog in dialogs)
			{
				if ((dialog.IsWithdrawal() && dialog.HasBeenAccepted()) || dialog.HasBeenResetToOriginal())
				{
					version = 1;
				}
				else if (!dialog.HasBeenRejected() && !dialog.IsWithdrawal())
				{
					version++;
				}
			}

			return Math.Max(version, 1);
		}

		#endregion

		#region GetCurrentMessageStatus

		internal static string GetCurrentMessageStatus(this IStmALogParent logParent, IDocument document, IMessageInstructions messageInstructions)
		{
			_ = logParent ?? throw new ArgumentNullException(nameof(logParent));
			_ = document ?? throw new ArgumentNullException(nameof(document));
			_ = messageInstructions ?? throw new ArgumentNullException(nameof(messageInstructions));

			return GetCurrentMessageStatusFromMessageExtensions(logParent, document, messageInstructions)
				?? GetCurrentMessageStatusFromLogs(logParent, document.Name, messageInstructions);
		}

		public static string GetCurrentMessageStatusFromLogs(this IStmALogParent logParent, string documentName, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(logParent, nameof(logParent));
			Argument.NotNull(logParent.Logs, nameof(logParent.Logs));

			var messageEventLogs = logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => IsValidMessageEvent(documentName, log));

			var orderedMessageEventLogs =
				(
					messageInstructions.OrderLogsByLocalTime
						? messageEventLogs.OrderByDescending(log => log.SL_EventTime)
						: messageEventLogs.OrderByDescending(log => log.SL_PostedTimeUtc)
				)
				.ThenByDescending(log => log.SL_SE_NKEvent);

			var lastLog = orderedMessageEventLogs.FirstOrDefault();

			return GetTranslatedMessageStatusFromLog(lastLog, documentName, messageInstructions);
		}

		static string GetTranslatedMessageStatusFromLog(StmALog lastLog, string documentName, IMessageInstructions messageInstructions)
		{
			var translatedDocumentName = string.IsNullOrEmpty(messageInstructions.TranslatedDocumentName) ? documentName : messageInstructions.TranslatedDocumentName;

			var lastEventDisplayEventReference = lastLog?.DisplayEventReference.ToString();
			if (lastEventDisplayEventReference != null)
			{
				var readonlyFactory = lastLog.Factory.GetCachedReadOnlyFactory();
				var newLog = readonlyFactory.New<StmALog>();

				using (((IUpdateFieldsLockReachAround)newLog).LockForUpdatingKeyFields(shouldNotifyOnRelease: false))
				{
					newLog.SL_SE_NKEvent = lastLog.SL_SE_NKEvent;
					newLog.SL_IsEstimate = lastLog.SL_IsEstimate;
					newLog.SL_Table = lastLog.SL_Table;
					newLog.SL_Parent = lastLog.SL_Parent;
					newLog.SL_FireWorkflow = false;
					newLog.SL_Reference = RegenerateLogReference(lastLog, translatedDocumentName);
				}

				return newLog.DisplayEventReference.ToString();
			}

			return Res.GetString("fa8aa0ad-8874-4103-ba12-8d4a0885a62a", "No {0} Messages Have Been Sent.", translatedDocumentName);
		}

		static string RegenerateLogReference(StmALog lastLog, string translatedDocumentName)
		{
			var parameters = new Dictionary<string, string>();
			var parameterNameWhichStoresDocumentName = lastLog.GetParameterNameWhichStoresDocumentName();

			foreach (var element in lastLog.Parameters)
			{
				if (element.Key == parameterNameWhichStoresDocumentName)
				{
					parameters[element.Key] = translatedDocumentName;
				}
				else
				{
					parameters[element.Key] = element.Value;
				}
			}

			return StmALog.GenerateEventReferenceToFitInReferenceMaxLength(lastLog.ReferenceFreeText, parameters);
		}

		static string GetCurrentMessageStatusFromMessageExtensions(IStmALogParent logParent, IDocument document, IMessageInstructions messageInstructions)
		{
			if (logParent is IVisualizerDocumentData documentData)
			{
				var messageExtensions = documentData
					?.Parent
					?.GetSupporter()
					?.GetMessagingExtensions(document, messageInstructions);

				return messageExtensions?.GetMessageStatus();
			}

			return null;
		}

		internal static bool IsValidMessageEvent(string documentName, StmALog log)
		{
			return log != null
				&& MessageEventCodes.MessageStatusEventCodes.Contains(log.SL_SE_NKEvent.ToString())
				&& log.MatchesDocumentName(documentName);
		}

		#endregion

		#region GetDialogs

		public static IEnumerable<IDialog> GetDialogs(this IStmALogParent logParent, string documentName, bool orderByLocalTime, Func<StmALog, bool> isDialogInitiatingEvent = null)
		{
			var result = new List<IDialog>();

			if (logParent == null)
			{
				return result;
			}

			var documentLogs = new List<StmALog>();

			var logs = logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => !log.SL_IsCancelled);

			var orderedLogs = (orderByLocalTime
				? logs.OrderBy(log => log.SL_EventTime)
				: logs.OrderBy(log => log.SL_PostedTimeUtc))
				.ThenBy(log => log.SL_SE_NKEvent == Events.DataExportCode ? 1 : 0) // make sure DEX is after MSN etc
				.ToArray();

			StmALog prevApplicableLog = null;
			var allEventCodes = new HashSet<string>(MessageEventCodes.All);
			var messageSentEventCodes = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);

			foreach (var log in orderedLogs)
			{
				if (log.SL_SE_NKEvent == Events.DataExportCode
					&& prevApplicableLog != null
					&& messageSentEventCodes.Contains(prevApplicableLog.SL_SE_NKEvent))
				{
					documentLogs.Add(log);
					prevApplicableLog = null;
				}
				else if (allEventCodes.Contains(log.SL_SE_NKEvent)
					&& log.MatchesDocumentName(documentName))
				{
					documentLogs.Add(log);
					prevApplicableLog = log;
				}
			}

			var accumulator = new List<StmALog>();

			var dialogInitiatingEvents = new List<string>(MessageEventCodes.SentMessagesEventCodes)
			{
				Events.StatusUpdatedCode
			};

			isDialogInitiatingEvent ??= log => dialogInitiatingEvents.Contains(log.SL_SE_NKEvent.ToString());

			foreach (var log in documentLogs)
			{
				if (isDialogInitiatingEvent(log))
				{
					if (accumulator.Any())
					{
						result.Add(new Dialog(accumulator.ToArray(), orderByLocalTime));
						accumulator.Clear();
					}
				}

				accumulator.Add(log);
			}

			if (accumulator.Any())
			{
				result.Add(new Dialog(accumulator.ToArray(), orderByLocalTime));
			}

			return result;
		}

		#endregion

		#region MatchesDocumentName

		public static bool MatchesDocumentName(this StmALog log, string documentName)
		{
			if (log == null)
			{
				return false;
			}

			var parameterName = log.GetParameterNameWhichStoresDocumentName();

			return log.Parameters.TryGetValue(parameterName, out var messageType)
				&& string.Equals(messageType, documentName, StringComparison.OrdinalIgnoreCase);
		}

		public static string GetParameterNameWhichStoresDocumentName(this StmALog log)
		{
			if (log == null)
			{
				return string.Empty;
			}

			switch (log.SL_SE_NKEvent)
			{
				case Events.AuthorisedCode:
				case Events.AuthorisationRejectedCode:
					return EventReferenceParameters.Type;

				default:
					return EventReferenceParameters.MessageType;
			}
		}

		#endregion

		#region GetUXml

		public static XDocument GetUXml(this IDocument document, string ns, string dataContext)
		{
			Argument.NotNull(document, nameof(document));

			XDocument xml = null;

			if (document.Data.Value is IDataObject)
			{
				IXmlWriter writer = new DynamicDataUXmlWriter(document.Data, ns);

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					writer.WriteXML(null, stream);
					xml = XDocument.Load(stream);
				}
			}
			else
			{
				var dataObject = ObjectFactory.Get<IForwardingDocDataObjectUXmlWriter>().GetDataObject(DefaultDataObjectWriterStrategy.Instance, document)
					?? ObjectFactory.Get<IAgencyDocDataObjectUXmlWriter>().GetDataObject(DefaultDataObjectWriterStrategy.Instance, document);

				if (dataObject != null)
				{
					IXmlWriter writer = new DocDataObjectUXmlWriter(dataObject, ns);
					using (var stream = (SubStreamableStream)new MemoryStream())
					{
						writer.WriteXML(null, stream);
						xml = XDocument.Load(stream);
					}
				}
			}

			return xml;
		}

		#endregion
	}
}
