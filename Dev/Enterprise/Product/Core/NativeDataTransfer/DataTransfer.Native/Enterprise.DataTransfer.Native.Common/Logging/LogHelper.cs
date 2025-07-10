using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common.Behaviours;
using Enterprise.DataTransfer.Native.DB.Sql;

namespace Enterprise.DataTransfer.Native.Common.Logging
{
	public static class LogHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public static string GetMappingLogMessage(IEntity entity, string propertyName, string foreignValue, string localValue)
		{
			if (entity.Definition == null
				|| entity.Definition.MainAssociation == null
				|| entity.Definition.MainAssociation.ForeignKeys.Any(x => x == null)
				|| entity.Definition.MainAssociation.ForeignKeys[0].Table == null)
			{
				return string.Empty;
			}

			string logMessage = entity.Definition.MainAssociation.ForeignKeys[0].Table.Name + ": ";
			logMessage += string.Format(CultureInfo.CurrentCulture, "Code mapped from foreign code {0} to local code {1}", foreignValue, localValue);

			return logMessage;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public static string GetExpirationInfoLogMessage(BehaviourContext behaviourContext, IEntity entity, string propertyToUpdate, DateTime originalDate, DateTime newDate)
		{
			var criteriaList = CriteriaHelper.GetCriteriaFromProperties(entity, behaviourContext.Converter,
				entity.Definition.DateRangeStartField, entity.Definition.DateRangeEndField)
				.Where(x => !IsDefaultValue(x.Value));

			var logMessageBuilder = new ZStringBuilder();
			logMessageBuilder.Append(System.Environment.NewLine);
			logMessageBuilder.Append(string.Format(CultureInfo.CurrentCulture, "Existing {0} was expired by incoming {0}", entity.EntityName));
			logMessageBuilder.Append(GetLogMessageFromCriteriaList(entity, criteriaList));
			logMessageBuilder.Append(string.Format(CultureInfo.CurrentCulture, "{0} changed from value {1} to value {2}", propertyToUpdate,
				originalDate == DateTime.MinValue ? "Empty date" : originalDate.ToString(CultureInfo.CurrentCulture), newDate));
			logMessageBuilder.Append(System.Environment.NewLine);

			return logMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
		public static string GetDateRangeConflictLogMessage(BehaviourContext behaviourContext, IEntity entity, ZDateTime originalFrom, ZDateTime originalTo,
			ZDateTime newFrom, ZDateTime newTo)
		{
			const string rowDateRangeIsWithinEntityDateRange = "Incoming {0} date range conflicts with existing {0}.\r\nExisting {0} date range: {1} - {2}.\r\nIncoming {0} date range {3} - {4}";
			const string emptyDateMessage = "Empty date";

			var formattedMessage = string.Format(CultureInfo.CurrentCulture, rowDateRangeIsWithinEntityDateRange,
						entity.EntityName,
						originalFrom,
						originalTo.IsValid ? originalTo.ToString() : emptyDateMessage,
						newFrom,
						newTo.IsValid ? newTo.ToString() : emptyDateMessage);

			var criteriaList = CriteriaHelper.GetCriteriaFromProperties(entity, behaviourContext.Converter,
				entity.Definition.DateRangeStartField, entity.Definition.DateRangeEndField)
				.Where(x => !IsDefaultValue(x.Value));

			return formattedMessage + System.Environment.NewLine + GetLogMessageFromCriteriaList(entity, criteriaList);
		}

		static string GetLogMessageFromCriteriaList(IEntity entity, IEnumerable<Criteria> criterias)
		{
			var result = new ZStringBuilder();

			foreach (var criteria in criterias)
			{
				var property = entity.Properties.FirstOrDefault(x => x.Definition.ColumnDef.Name == criteria.ColumnName);

				if (property != null)
				{
					result.Append(property.Name + '[' + criteria.Value + ']');
				}
				else
				{
					var association = entity.Definition.AssociationCollection.FirstOrDefault(x => x.ForeignKeys.Any(y => y.Name == criteria.ColumnName));

					if (association != null)
					{
						result.Append(association.To.EntityName + '[' + criteria.Value + ']');
					}
				}
			}

			return result.ToStringWithNewLineBetweenAppends();
		}

		static bool IsDefaultValue(object value)
		{
			if (value is int)
			{
				return (int)value == default(int);
			}

			if (value is bool)
			{
				return (bool)value == default(bool);
			}

			if (value is string)
			{
				return string.IsNullOrEmpty((string)value);
			}

			return value == DBNull.Value;
		}
	}
}
