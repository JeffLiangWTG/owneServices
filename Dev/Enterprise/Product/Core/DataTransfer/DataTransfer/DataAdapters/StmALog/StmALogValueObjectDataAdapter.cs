using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class StmALogValueObjectDataAdapter : ValueObjectDataAdapter<StmALog, Xsd.Event>
	{
		#region Construction

		protected StmALogValueObjectDataAdapter(BusinessObject logParent, string errorContext, EventsWithSourceType triggerEvent)
		{
			this.LogParent = logParent;
			this.ErrorContext = errorContext;
			this.TriggerEvent = triggerEvent;
		}

		public readonly string ErrorContext;
		protected readonly EventsWithSourceType TriggerEvent;
		public BusinessObject LogParent { get; protected set; }

		public static StmALogValueObjectDataAdapter New(BusinessObject logParent, string errorContext)
		{
			return New(logParent, errorContext, EventsWithSourceType.Empty);
		}

		public static StmALogValueObjectDataAdapter New(BusinessObject logParent, string errorContext, EventsWithSourceType triggeredByEvents)
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden(logParent, errorContext, triggeredByEvents) : new StmALogValueObjectDataAdapter(logParent, errorContext, triggeredByEvents);
		}

		protected delegate StmALogValueObjectDataAdapter NewDelegate(BusinessObject logParent, string errorContext, EventsWithSourceType triggeredByEvents);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootCollectionElementName { get { return "Events"; } }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public override string RootElementName { get { return "Event"; } }
		public override XmlSchema Schema { get { return XmlSchemaDefinitions.Instance.SingleEventSchema; } }
		public override XmlSchema CollectionSchema { get { return XmlSchemaDefinitions.Instance.EventsSchema; } }

		#endregion

		#region Find

		protected override StmALog FindBusinessObject(Xsd.Event eventValue, IValueObjectImportContext context)
		{
			bool isEstimated = eventValue.IsEstimatedDateSpecified && (eventValue.IsEstimatedDate == Xsd.TrueFalse.@true);

			return LogParent.GetLogs().GetAllLogs().Cast<StmALog>().FirstOrDefault(
				e => e.SL_EventTime == eventValue.DateTime
				&& e.SL_SE_NKEvent == eventValue.Code
				&& e.SL_IsEstimate == isEstimated);
		}

		#endregion

		#region Import

		bool ShouldImportEventLog(string eventCode, IValueObjectImportContext context)
		{
			var shouldImport = true;
			if (string.IsNullOrEmpty(eventCode))
			{
				context.Notify(new ErrorNotification(ErrorType.UnknownCode, Res.GetString("7B7D95E7-E8E9-4984-B34B-71582B8DF7EF", "Event code is empty")));
				shouldImport = false;
			}
			else
			{
				var eventToImport = Events.All[eventCode];
				shouldImport = !Events.ChangeLogs.Contains(eventToImport)
				&& !Events.SystemReservedEvents.Contains(eventToImport);
			}

			return shouldImport;
		}

		public void FromXmlCollectionValueObject(Xsd.Events logsValue, IValueObjectImportContext context)
		{
			if (logsValue != null && logsValue.Event.IsSpecified)
			{
				foreach (Xsd.Event eventValue in logsValue.Event)
				{
					Event eventType = GetEventTypeByCode(eventValue.Code, context);
					if (eventType != null)
					{
						eventValue.Code = eventType.Code;
					}

					if (ShouldImportEventLog(eventValue.Code, context))
					{
						var logToUpdate = FindBusinessObject(eventValue, context) ?? LogParent.GetLogs().AddNew();

						ImportFromValueObject(logToUpdate, eventValue, context);
					}
				}
			}
		}

		protected override void ImportFromValueObjectCore(StmALog log, Xsd.Event value, IValueObjectImportContext context)
		{
			var fieldsLock = (IUpdateFieldsLock)log;
			var pcaDebugString = "";
			using (fieldsLock.LockForUpdatingKeyFields())
			{
				if (value.Code == Events.PickupCartageAdvised.Code)
				{
					pcaDebugString = GetPCADebugString(log, value, context);
				}

				var eventType = Events.All[value.Code];
				if (eventType == null)
				{
					context.Notify(new ErrorNotification(ErrorType.UnknownCode, Res.GetString("09c9328c-5cad-4fe9-95da-019bdd152e01", "Event code '{0}'", value.Code)));
				}
				else
				{
					context.SetPropertyInfoValue(log.SL_SE_NKEventInfo, value.Code, true);
				}

				if (value.DateTime.IsValid)
				{
					log.SL_EventTime = value.DateTime;
				}

				if (value.IsEstimatedDateSpecified)
				{
					log.SL_IsEstimate = value.IsEstimatedDate == Xsd.TrueFalse.@true;
				}
				context.SetPropertyInfoValue(log.SL_ReferenceInfo, value.Information, true);
				context.SetPropertyInfoValue(log.SL_TableInfo, LogParent.TableName, true);
			}

			if (value.Code == Events.PickupCartageAdvised.Code && log.IsDeleted)
			{
				ErrorReporter.ReportOnce("LogDeletedBeforeProcess_PCA", $"Information for Forwarding team:\n{pcaDebugString}\n{GetPCADebugString(log, value, context)}");
			}
		}

		#region PCA debug

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debugging string")]
		string GetPCADebugString(StmALog log, Xsd.Event value, IValueObjectImportContext context)
		{
			var debugStr = $@"
Event
Code: {value.Code}
DateTime: {value.DateTime}
Information: {value.Information}
IsEstimatedDate: {value.IsEstimatedDate}

Current Log:
";
			debugStr += GetPCADebugLogString(log);
			debugStr += "\n\nLogParent Logs:\n";
			var existingPcaLogs = LogParent.GetLogs().GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == Events.PickupCartageAdvised.Code).Select(l => GetPCADebugLogString(l));
			debugStr += string.Join("\n\n", existingPcaLogs);
			debugStr += "\n-------------------";
			return debugStr;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debugging string")]
		string GetPCADebugLogString(StmALog log)
		{
			if (log.IsDeleted)
			{
				return "Deleted log";
			}
			return $@"Log
IsInDatabase: {log.IsInDatabase}
SL_EventTime: {log.SL_EventTime}
SL_IsEstimate: {log.SL_IsEstimate}
SL_Parent: {log.SL_Parent}
SL_Reference: {log.SL_Reference}
SL_SE_NKEvent: {log.SL_SE_NKEvent}
SL_Table: {log.SL_Table}";
		}

		#endregion

		protected Event GetEventTypeByCode(ZString code, IValueObjectImportContext context)
		{
			Event eventType = Events.All[code];
			if (eventType == null)
			{
				code = context.ConvertRawStringToZTypeValue<ZString>(code, ForeignKeyType.EventCodeNK);
				if (!code.IsEmpty)
				{
					eventType = Events.All[code];
				}
			}

			return eventType;
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// the user doesn't want to hear about the 100's of events created
		}

		#endregion

		#region Export

		public Xsd.Events ToXmlCollectionValueObject(IValueObjectExportContext context)
		{
			Xsd.Events result = null;
			if (LogParent.GetLogs().GetAllLogs().Count > 0)
			{
				result = new Xsd.Events();
				result.Event = new Xsd.EventCollection();
				var sortedLogs = GetLogsSortedByDate(LogParent);

				result.InitialDataExported = GetFirstDataExportEventDateTime(sortedLogs);

				string triggerReference = TriggerEvent.TriggerAction != null && TriggerEvent.TriggerAction.Parent != null &&
					TriggerEvent.TriggerAction.Parent.TriggerCondition == EventReferenceConditionList.Codes.EventReference
					? TriggerEvent.TriggerAction.Parent.TriggerConditionValue.ToUpper() : null;

				var parent = TriggerEvent.TriggerParent;
				var parentPK = parent != null ? parent.PK : ZGuid.Empty;

				Dictionary<string, TriggeringLog> logsToSetTriggeredBy = new Dictionary<string, TriggeringLog>();
				foreach (var log in sortedLogs)
				{
					if (!ShouldExportEventLog(log, context))
					{
						continue;
					}

					var @event = ExportToValueObject(log, context);

					if (ShouldSetTriggeredBy(log, sortedLogs, triggerReference, parentPK))
					{
						if (!logsToSetTriggeredBy.ContainsKey(@event.Code))
						{
							logsToSetTriggeredBy.Add(@event.Code, new TriggeringLog(@event, log, triggerReference, parentPK));
						}
						else
						{
							if (CompareLogs(logsToSetTriggeredBy[@event.Code].Log, log) == -1)
							{
								logsToSetTriggeredBy[@event.Code] = new TriggeringLog(@event, log, triggerReference, parentPK);
							}
						}
					}

					result.Event.Add(@event);
				}

				foreach (var entry in logsToSetTriggeredBy)
				{
					SetTriggeredBy(entry.Value.Event);
				}

				if (logsToSetTriggeredBy.Count == 0)
				{
					//Try harder to set TriggeredBy on an event.
					//We threw out logs that have been cancelled. Try those and see if we find something.
					foreach (var log in sortedLogs)
					{
						if (ShouldSetTriggeredBy(log, sortedLogs, triggerReference, parentPK))
						{
							var @event = ExportToValueObject(log, context);
							SetTriggeredBy(@event);
							result.Event.Add(@event);
							break;
							//Or should we instead use the event for log IsLatestLogForThisEvent ?? So we don't add a cancelled event(log) to the results?
						}
					}
				}
			}
			return result;
		}

		class TriggeringLog
		{
			public TriggeringLog(Xsd.Event ev, StmALog log, ZString triggerReference, ZGuid parentPK)
			{
				Event = ev;
				Log = log;
				TriggerReference = triggerReference;
				ParentPK = parentPK;
			}

			public Xsd.Event Event { get; private set; }
			public StmALog Log { get; private set; }
			public ZString TriggerReference { get; private set; }
			public ZGuid ParentPK { get; private set; }
		}

		ZDateTime GetFirstDataExportEventDateTime(StmALog[] sortedLogs)
		{
			foreach (var log in sortedLogs)
			{
				if (log.SL_SE_NKEvent == Events.DataExport.Code)
				{
					return log.SL_EventTime;
				}
			}

			return ZDateTime.Empty;
		}

		bool IsLatestLogForThisEvent(StmALog[] sortedLogs, StmALog log)
		{
			var result = false;
			if (sortedLogs == null || log == null || sortedLogs.Length == 0)
			{
				return false;
			}

			for (var i = sortedLogs.Length - 1; i >= 0; i--)
			{
				if (sortedLogs[i].SL_SE_NKEvent != log.SL_SE_NKEvent ||
					sortedLogs[i].SL_Reference != log.SL_Reference ||
					sortedLogs[i].SL_IsEstimate ||
					sortedLogs[i].SL_IsCancelled)
				{
					continue;
				}

				if (log.PK == sortedLogs[i].PK)
				{
					result = true;
				}

				break;
			}

			return result;
		}

		void SetTriggeredBy(Xsd.Event @event)
		{
			@event.TriggeredBy = true;
			@event.TriggeredBySpecified = true;
		}

		bool ShouldSetTriggeredBy(StmALog log, StmALog[] sortedLogs, string triggerReference, ZGuid parentPK)
		{
			if (log == null || TriggerEvent == null)
			{
				return false;
			}

			if (!string.IsNullOrEmpty(triggerReference) && log.SL_Reference.ToUpper() != triggerReference)
			{
				return false;
			}

			if (!Array.Exists(TriggerEvent.GetEvents(), e => e != null && e.Code == log.SL_SE_NKEvent))
			{
				return false;
			}

			if (TriggerEvent.HasLog(log.PK))
			{
				return true;
			}

			if (!parentPK.IsEmpty && parentPK != log.SL_Parent)
			{
				return false;
			}

			if (log.SL_IsEstimate)
			{
				return false;
			}

			if (!IsLatestLogForThisEvent(sortedLogs, log))
			{
				return false;
			}

			return true;
		}

		#region SuspendExportingLogs

		public static IDisposable SuspendExportingLogs()
		{
			return new DisposableAction(() => isExportingLogsSuspended = true, () => isExportingLogsSuspended = false);
		}

		[ThreadStatic]
		static bool isExportingLogsSuspended;

		#endregion

		bool ShouldExportEventLog(StmALog log, IValueObjectExportContext context)
		{
			var result =
				!isExportingLogsSuspended &&
				(IncludeEditEventsInExport || log.SL_SE_NKEvent != Events.EditedARecord.Code) &&
				log.SL_SE_NKEvent != Events.DeletedARecordInTheSystem.Code &&
				(log.SL_SE_NKEvent != Events.DataImport.Code || ShouldExportDataImportEvent(log)) &&
				(!context.SimplifiedXML || log.SL_SE_NKEvent != Events.DataExport.Code) &&
				!log.SL_IsCancelled;

			return result;
		}

		protected virtual bool ShouldExportDataImportEvent(StmALog log)
		{
			return false;
		}

#if DEBUG
		internal
#endif
 protected virtual bool IncludeEditEventsInExport
		{
			get { return false; }
		}

		protected override void ExportToValueObjectCore(StmALog log, Xsd.Event result, IValueObjectExportContext context)
		{
			result.Code = log.SL_SE_NKEvent;
			result.CodeDescription = (log.Event == null) ? null : log.Event.SE_DescMultilingual.GetUnresolvedString();

			if (log.SL_EventTime.IsValid)
			{
				result.DateTime = log.SL_EventTime;
			}

			if (log.SL_IsEstimate.IsValid)
			{
				result.IsEstimatedDate = log.SL_IsEstimate ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
				result.IsEstimatedDateSpecified = true;
			}

			if (log.SL_PostedTimeUtc.IsValid)
			{
				result.PostedDateTime = log.SL_PostedTimeUtc;
			}

			if (!log.SL_Reference.IsEmpty)
			{
				result.Information = log.SL_Reference;
			}

			result.Source = log.SL_Table;
			result.User = log.SL_GS_NKUser;

			if (!log.SL_UserNameAndInitials.IsEmpty)
			{
				result.UserName = log.SL_UserNameAndInitials;
			}

			if (log.User != null && !log.User.GS_EmailAddress.IsEmpty)
			{
				result.UserEmailAddress = log.User.GS_EmailAddress;
			}
		}

		StmALog[] GetLogsSortedByDate(BusinessObject parent)
		{
			var logList = parent.GetLogs().GetAllLogs().ToList();
			var relatedBizos = ((IStmALogParent)parent).BusinessObjectsWithRelatedEvents?.WhereNotNull();
			if (relatedBizos != null)
			{
				foreach (var bizo in relatedBizos)
				{
					logList.AddRange(bizo.GetLogs().GetAllLogs());
				}
			}

			var logs = logList.Cast<StmALog>().ToArray();
			Array.Sort(logs, CompareLogs);
			return logs;
		}

		int CompareLogs(StmALog lhs, StmALog rhs)
		{
			var result = lhs.SL_PostedTimeUtc.CompareTo(rhs.SL_PostedTimeUtc);
			if (result == 0)
			{
				result = lhs.SL_EventTime.CompareTo(rhs.SL_EventTime);
			}

			if (result == 0)
			{
				result = lhs.PK.CompareTo(rhs.PK);
			}

			return result;
		}

		#endregion
	}
}
