using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class AddInfoCollectionExtensions
	{
		public static void AddAddInfo(this IAddInfoCollectionParent parent, ZString key, IZType value, bool addEmpty = false)
		{
			if (addEmpty || !value.IsEmpty)
			{
				if (parent.AddInfoCollection == null)
				{
					parent.SetAddInfoCollection(() => new List<AddInfo>());
				}
				if (parent.AddInfoCollection != null)
				{
					parent.AddInfoCollection.AddAddInfo(key, value, addEmpty);
				}
			}
		}

		public static void AddAddInfo(this List<AddInfo> addInfoCollection, ZString key, IZType value, bool addEmpty = false)
		{
			if (addEmpty || !value.IsEmpty)
			{
				addInfoCollection.Add(new AddInfo() { Key = key, Value = value.ToString() });
			}
		}

		public static ZString? GetZStringValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZString>(addInfos, key, logger);
		}

		public static ZDecimal? GetZDecimalValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZDecimal>(addInfos, key, logger);
		}

		public static ZBool? GetZBoolValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZBool>(addInfos, key, logger);
		}

		public static ZDate? GetZDateValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZDate>(addInfos, key, logger);
		}

		public static ZDateTime? GetZDateTimeValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZDateTime>(addInfos, key, logger);
		}

		public static ZDateTimeOffset? GetZDateTimeOffsetValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZDateTimeOffset>(addInfos, key, logger);
		}
		public static ZTime? GetZTimeValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZTime>(addInfos, key, logger);
		}

		public static ZGeography? GetZGeographyValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZGeography>(addInfos, key, logger);
		}

		public static ZInt? GetZIntValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZInt>(addInfos, key, logger);
		}

		public static ZLong? GetZLongValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZLong>(addInfos, key, logger);
		}

		public static ZShort? GetZShortValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZShort>(addInfos, key, logger);
		}

		public static ZGuid? GetZGuidValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZGuid>(addInfos, key, logger);
		}

		public static ZByte? GetZByteValue(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger = null)
		{
			return GetValue<ZByte>(addInfos, key, logger);
		}

		static TSource? GetValue<TSource>(this IEnumerable<AddInfo> addInfos, ZString key, IXmlImportLogger logger)
			where TSource : struct
		{
			ZString? value = null;
			if (addInfos != null)
			{
				var addInfo = addInfos.FirstOrDefault(x => x.Key.GetValueOrDefault() == key);
				if (addInfo != null)
				{
					value = addInfo.Value;
				}
			}
			if (value.HasValue)
			{
				try
				{
					return (TSource)ZDataType.ObjectToZType(typeof(TSource), value.Value);
				}
				catch (ZTypeValueException ex)
				{
					Log(key, value.Value, logger, ex);
					return null;
				}
				catch (FormatException ex)
				{
					Log(key, value.Value, logger, ex);
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		static void Log(ZString key, ZString value, IXmlImportLogger logger, Exception ex)
		{
			if (logger != null)
			{
				logger.Log(LogType.Warning, Res.GetString("2C393ACB-D822-4C3E-9F8D-CCAC87412A0B", "{3} Key ({0}) has a invalid value ({1}):\r\n{2}", key, value, ex.Message, "AddInfo"));
			}
		}
	}
}
