using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentFieldChangedEventLogger
	{
		public SupportIncidentFieldChangedEventLogger(SupportIncident incident)
		{
			Argument.NotNull(incident, nameof(incident));
			this.incident = incident;
			CurrentEventLogGroupKey = ZGuid.NewZGuid();
		}

		readonly SupportIncident incident;

		BusinessObjectFactory Factory => incident?.Factory;

		IncidentLogs Logs => incident?.Logs;

		const string logGroupTag = "FieldChangedLogGroup";

		public ZGuid CurrentEventLogGroupKey { get; private set; }

		readonly string logStatusChangeReferenceTemplate = "{0} - {1} to {2}";
		protected readonly Regex FieldChangeEventParser = new Regex(@"^(?<FieldDescription>.*?) - (?<From>.*?) to( ?)(?<To>.*)$");

		public bool LogFieldChange(Event logEvent, ZPropertyInfo propInfo)
		{
			if (propInfo.HasChanges || (!incident.IsInDatabase && !propInfo.Value.IsEmpty))
			{
				var originalValue = (!incident.IsInDatabase) ? "" : propInfo.OriginalValue.ToString();
				LogFieldChange(logEvent, GetFieldDescription(propInfo), originalValue, (ZString)propInfo.Value);
				return true;
			}

			return false;
		}

		void LogFieldChange(Event logEvent, ZString fieldDescription, ZString from, ZString to)
		{
			var referenceFreeText = BuildReferenceFreeText(fieldDescription, from, to);
			var result = Logs.AddNew(logEvent, referenceFreeText);
		}

		public string BuildReferenceFreeText(string description, string originalValue, string newValue)
		{
			return string.Format(CultureInfo.InvariantCulture, logStatusChangeReferenceTemplate, description, originalValue, newValue);
		}

		public bool TryParseFieldChangedEvent(StmALog eventLog, out FieldChangedEventReference result)
		{
			result = null;
			if (eventLog != null)
			{
				if (FieldChangeEventParser.IsMatch(eventLog.ReferenceFreeText))
				{
					var matcher = FieldChangeEventParser.Match(eventLog.ReferenceFreeText);
					result = new FieldChangedEventReference()
					{
						FieldDescription = matcher.Groups["FieldDescription"].Value,
						From = matcher.Groups["From"].Value,
						To = matcher.Groups["To"].Value,
					};

					return true;
				}
			}

			return false;
		}

		public StmALog[] GetFieldChangedLogGroupByKey(ZGuid key)
		{
			var query = new ZQuery(StmALogSchema.SL_Parent, incident.PK);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"{CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ContentID}={key.ToString().ToUpper()}");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"{CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Group}={logGroupTag}");
			var result = new List<FieldChangedEventReference>();
			var queryResult = Factory.Load<StmALog>(query);

			return queryResult.ToArray();
		}

		public StmALog MostRecentLogStatusChange(string description, string eventCode = AutoEvents.StatusChangeCode)
		{
			var query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Parent, incident.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, description);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
			return Factory.LoadTop1<StmALog>(query);
		}

		string GetFieldDescription(ZPropertyInfo propertyInfo) => propertyInfo.GetAttribute<LoggingValueChangesAttribute>()?.fieldDescription;

		public LoadingPreviousFieldValueResult TryGetPreviousFieldValue<T>(ZPropertyInfo propertyInfo, out T result, string eventCode = AutoEvents.StatusChangeCode) where T : IZType
		{
			result = default(T);
			if (!incident.IsInDatabase)
			{
				return LoadingPreviousFieldValueResult.ParentCouldNotBeLoadedFromDatabase;
			}
			else if (incident.IsDeleted || incident.IsDeleting)
			{
				return LoadingPreviousFieldValueResult.ParentHasBeenDeleted;
			}

			var fieldDescription = GetFieldDescription(propertyInfo);
			if (string.IsNullOrEmpty(fieldDescription))
			{
				return LoadingPreviousFieldValueResult.PropertyDoesNotHaveDescription;
			}

			if (propertyInfo.HasChanges)
			{
				result = (T)propertyInfo.OriginalValue;
				return LoadingPreviousFieldValueResult.Success;
			}
			else
			{
				var changedLog = MostRecentLogStatusChange(fieldDescription, eventCode);
				if (changedLog == null)
				{
					return LoadingPreviousFieldValueResult.NoLog;
				}

				if (TryParseFieldChangedEvent(changedLog, out var parsedLog))
				{
					var convertedFromValue = (IZType)propertyInfo.PropertyDescriptor.Converter.ConvertFromString(parsedLog.From);
					var convertedToValue = (IZType)propertyInfo.PropertyDescriptor.Converter.ConvertFromString(parsedLog.To);

					result = (T)convertedFromValue;
					if (propertyInfo.Value.Equals(convertedToValue))
					{
						return LoadingPreviousFieldValueResult.Success;
					}
					else
					{
						var factory = new BusinessObjectFactory() { RefreshEnabled = false };
						var incidentFromDatabase = factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, incident.PK));
						if (incidentFromDatabase != null)
						{
							var propertyInfoFromDatabase = incidentFromDatabase.FindPropertyInfo(propertyInfo.Name);
							if (!propertyInfoFromDatabase.Value.Equals(convertedToValue))
							{
								return LoadingPreviousFieldValueResult.DatabaseValueDoesNotMatchLog;
							}
							else
							{
								return LoadingPreviousFieldValueResult.ParentDataOutDated;
							}
						}
						else
						{
							return LoadingPreviousFieldValueResult.ParentCouldNotBeLoadedFromDatabase;
						}
					}
				}
				else
				{
					ErrorReporter.ReportOnce("A field changed log could not be parsed when getting previous value", $"Incident:{incident.Number}; EventReference:{changedLog.SL_Reference}");
					return LoadingPreviousFieldValueResult.CannotParseLog;
				}
			}
		}
	}

	public enum LoadingPreviousFieldValueResult
	{
		Success,
		ParentCouldNotBeLoadedFromDatabase,
		ParentHasBeenDeleted,
		ParentDataOutDated,
		DatabaseValueDoesNotMatchLog,
		CannotParseLog,
		PropertyDoesNotHaveDescription,
		NoLog,
	}

	public class FieldChangedEventReference
	{
		public string FieldDescription { get; set; }

		public string From { get; set; }

		public string To { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class LoggingValueChangesAttribute : Attribute
	{
		public LoggingValueChangesAttribute(string fieldDescription)
		{
			this.fieldDescription = fieldDescription;
		}

		public readonly string fieldDescription;
	}
}
