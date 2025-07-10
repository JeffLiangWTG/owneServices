using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public static class AddInfoParser
	{
		public const char Separator = '*';
		public const char SpecialCharRepresentingStar = '¤';
		public const char CodeValueSeparator = '=';
		public const char CodeInfoSeparator = '^';

		public static ZString EscapeValue(ZString value) => value.Replace(SpecialCharRepresentingStar, Separator);
		public static ZString CombineValues(ZString value1, ZString value2)
		{
			var result = value1 + CodeInfoSeparator + value2;
			return result.Trim(CodeInfoSeparator);
		}

		public static void AddValue(Dictionary<ZString, ZString> addInfo, ZString key, ZString value)
		{
			if (addInfo.TryGetValue(key, out var currentValue))
			{
				addInfo[key] = CombineValues(currentValue, value);
			}
			else
			{
				addInfo.Add(key, value);
			}
		}

		public static Dictionary<ZString, ZString> CreateDictionaryWithAddInfoString(ZString addInfoString)
		{
			var result = new Dictionary<ZString, ZString>();
			if (!addInfoString.IsEmpty)
			{
				var keyValuePairs = addInfoString.Split(Separator);
				foreach (var pair in keyValuePairs)
				{
					var firstCodeValueSeparator = pair.IndexOf(CodeValueSeparator);
					if (firstCodeValueSeparator > 0 && pair.Length > firstCodeValueSeparator)
					{
						var key = pair.Substring(0, firstCodeValueSeparator);
						ZString value;
						if (!result.TryGetValue(key, out value))
						{
							value = ZString.Empty;
							result.Add(key, value);
						}

						result[key] = CombineValues(value, EscapeValue(pair.Substring(firstCodeValueSeparator + 1)));
					}
				}
			}
			return result;
		}

		public static ZString Serialise(IEnumerable<KeyValuePair<ZString, ZString>> pairs, bool sortByKey = false)
		{
			var builder = new ZStringBuilder();
			var sortedPairs = sortByKey ? pairs.OrderBy(x => x.Key) : pairs;
			foreach (var pair in sortedPairs)
			{
				builder.Append(Serialise(pair.Key, pair.Value));
			}
			return builder.ToString().Trim(AddInfoParser.Separator);
		}

		public static ZString Serialise(ZString key, ZString value)
		{
			return Separator + key + CodeValueSeparator + value.Replace(Separator, SpecialCharRepresentingStar);
		}

		public static ZString ConcatAddInfoStrings(ZString addInfoString, ZString nAddInfoString)
		{
			return addInfoString + (addInfoString.IsEmpty || nAddInfoString.IsEmpty ? string.Empty : Separator.ToString()) + nAddInfoString;
		}

		public static IZType ConvertToZType(Type zType, object sourceValue)
		{
			if (sourceValue == null)
			{
				return null;
			}

			var stringValue = sourceValue.ToString();

			return zType switch
			{
				Type _ when zType == typeof(ZDate) => ((ZDateTime)Parse(ZDateTime.TryParseISO8601Date, stringValue, ZDateTime.Invalid)).Date,
				Type _ when zType == typeof(ZDateTime) => Parse(ZDateTime.TryParseISO8601Date, stringValue, ZDateTime.Invalid),
				Type _ when zType == typeof(ZDateTimeOffset) => Parse(ZDateTimeOffset.TryParse, stringValue, ZDateTimeOffset.Invalid),
				Type _ when zType == typeof(ZString) => new ZString(sourceValue),
				Type _ when zType == typeof(ZDecimal) => Parse<ZDecimal>(ZDecimal.TryParse, stringValue),
				Type _ when zType == typeof(ZByte) => Parse<ZByte>(ZByte.TryParse, stringValue),
				Type _ when zType == typeof(ZInt) => Parse<ZInt>(ZInt.TryParse, stringValue),
				Type _ when zType == typeof(ZLong) => Parse<ZLong>(ZLong.TryParse, stringValue),
				Type _ when zType == typeof(ZShort) => Parse<ZShort>(ZShort.TryParse, stringValue),
				Type _ when zType == typeof(ZGeography) => Parse<ZGeography>(ZGeography.TryParse, stringValue),
				Type _ when zType == typeof(ZGuid) => string.IsNullOrEmpty(stringValue)
					? ZGuid.Empty : Parse(ZGuid.TryParse, stringValue, ZGuid.Invalid),
				Type _ when zType == typeof(ZBool) => (sourceValue is string or ZString) && string.IsNullOrEmpty(stringValue)
					? ZBool.False : Parse(ZBool.TryParse, stringValue, ZBool.False),
				_ => ErrorUnsupportedType(zType)
			};
		}

		delegate bool TryParseFunction<T>(string input, out T result) where T : IZType;
		static IZType Parse<T>(TryParseFunction<T> tryParseMethod, string stringValue) where T : IZType
		{
			tryParseMethod(stringValue, out var result);
			return result;
		}

		static IZType Parse<T>(TryParseFunction<T> tryParseMethod, string stringValue, T defaultValue) where T : IZType
		{
			return tryParseMethod(stringValue, out var result) ? result : defaultValue;
		}

		static IZType ErrorUnsupportedType(Type zType)
		{
			ErrorReporter.ReportOnce("Unsupported Object Type", "Please add support for objects of type : " + zType.Name + " to AssignValueToPropertyInfoHash");
			return null;
		}

		public static void Deserialise(ZString addInfoString, IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping, bool clearExisting)
		{
			var addInfoNamesToClear = clearExisting ? addInfoNamesMapping.Keys.ToHashSet() : new HashSet<string>();
			foreach (var kvp in CreateDictionaryWithAddInfoString(addInfoString))
			{
				var key = kvp.Key;
				var value = kvp.Value;
				if (addInfoNamesMapping.TryGetValue(key, out var addInfoPropertyData))
				{
					addInfoPropertyData.Value = addInfoPropertyData.OriginalValue = AddInfoParser.ConvertToZType(addInfoPropertyData.Value.GetType(), value);
					addInfoNamesToClear.Remove(key);
				}
			}
			addInfoNamesToClear.ForEach(key =>
			{
				var addInfoPropertyData = addInfoNamesMapping[key];
				var defaultValue = addInfoPropertyData.Value.Default;
				if (defaultValue != addInfoPropertyData.Value)
				{
					addInfoPropertyData.Value = addInfoPropertyData.OriginalValue = defaultValue;
				}
			});
		}

		public static ZString Serialise(IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping)
			=> Serialise(addInfoNamesMapping, Enumerable.Empty<KeyValuePair<ZString, ZString>>());

		public static ZString Serialise(IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping, IEnumerable<KeyValuePair<ZString, ZPropertyInfo>> additionalPairs)
			=> Serialise(addInfoNamesMapping,
					additionalPairs?.Where(additionalPair => !additionalPair.Value.IsNAddInfoField())
							.Where(additionalPair => !additionalPair.Value.Value.IsDefault)
							.Select(additionalPair => new KeyValuePair<ZString, ZString>(additionalPair.Key, additionalPair.Value.Value.GetStringRepresentation()))
						?? Enumerable.Empty<KeyValuePair<ZString, ZString>>());

#if NETFRAMEWORK
		public static ZString Serialise(IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping, IEnumerable<KeyValuePair<ZString, ZString>> additionalPairs)
			=> Serialise(addInfoNamesMapping
					.Where(x => !x.Value.Value.IsDefault)
					.Select(x => new KeyValuePair<ZString, ZString>(x.Key, x.Value.Value.GetStringRepresentation()))
					.Union(additionalPairs)
					.DistinctBy(x => x.Key)
				, true)
				.Trim(Separator);
#else
		public static ZString Serialise(IDictionary<string, IAddInfoPropertyData> addInfoNamesMapping, IEnumerable<KeyValuePair<ZString, ZString>> additionalPairs)
			=> Serialise( IEnumerableExtensions.DistinctBy(addInfoNamesMapping
						.Where(x => !x.Value.Value.IsDefault)
						.Select(x => new KeyValuePair<ZString, ZString>(x.Key, x.Value.Value.GetStringRepresentation()))
						.Union(additionalPairs),
						x => x.Key)
					, true)
				.Trim(Separator);
#endif

		public static void HandleConcurrencyException(IEnumerable<IPropertyRecord> propertyRecords, Func<string, ZPropertyInfo> getPropertyInfo, IDictionary<string, IDictionary<string, IAddInfoPropertyData>> addInfoColumnMappings)
		{
			foreach (var propertyRecord in propertyRecords)
			{
				if (addInfoColumnMappings.TryGetValue(propertyRecord.ColumnName, out var addInfoNamesMapping))
				{
					foreach (var databaseData in CreateDictionaryWithAddInfoString(propertyRecord.DatabaseValue.ToString()))
					{
						if (addInfoNamesMapping.TryGetValue(databaseData.Key, out var addInfoData))
						{
							var databaseValue = ConvertToZType(addInfoData.Value.GetType(), databaseData.Value);
							var currentValue = addInfoData.Value;
							if (!databaseValue.Equals(currentValue) && getPropertyInfo(addInfoData.PropertyName) is ZPropertyInfo info)
							{
								info.AddWarningWithoutValidationCheck(PropertyRecord.GetWarning(propertyRecord.LastModified, currentValue.GetStringRepresentation(), databaseValue.GetStringRepresentation()));
							}
						}
					}
				}
			}
		}

		public static void AppendDecoratedDisplayName(StringBuilder stringBuilder, IPropertyRecord record, Func<string, ZPropertyInfo> getPropertyInfo, IDictionary<string, IDictionary<string, IAddInfoPropertyData>> addInfoColumnMappings)
		{
			if (addInfoColumnMappings.TryGetValue(record.ColumnName, out var mappings))
			{
				var databaseValues = CreateDictionaryWithAddInfoString(record.DatabaseValue.ToString());
				stringBuilder.Append(ConcurrencyResolver.Tab).AppendLine(record.DisplayName);
				mappings.Where(pair => !pair.Value.Value.Equals(ConvertToZType(pair.Value.Value.GetType(), databaseValues.GetValueSafe(pair.Key))))
					.Select(pair => pair.Value)
					.ForEach(data =>
					{
						var info = getPropertyInfo(data.PropertyName);
						if (info != null)
						{
							stringBuilder.Append(ConcurrencyResolver.Tab)
								.Append(ConcurrencyResolver.Tab)
								.AppendLine(info.HumanReadableName);
						}
					});
			}
			else
			{
				stringBuilder.Append(ConcurrencyResolver.Tab).AppendLine(record.DisplayName);
			}
		}

		public static void CopyAddInfoPropertyData(BusinessObject bizO, BusinessObjectCloneArgs args, params (IDictionary<string, IAddInfoPropertyData>, IDictionary<string, IAddInfoPropertyData>)[] mappings)
		{
			foreach (var (target, source) in mappings)
			{
				foreach (var entry in source)
				{
					if (args.PerformRowCopyWithoutTriggeringValidationAndSetter && target.ContainsKey(entry.Key))
					{
						target[entry.Key].Value = entry.Value.Value;
					}
					else if (bizO.ZPropertyInfoHash.ContainsKey(entry.Value.PropertyName))
					{
						bizO[entry.Value.PropertyName] = entry.Value.Value;
					}
				}
			}
		}
	}
}
