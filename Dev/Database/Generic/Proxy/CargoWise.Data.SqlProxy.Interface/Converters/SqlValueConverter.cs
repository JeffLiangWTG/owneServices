using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.Xml;
using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Converters
{
	public static class SqlValueConverter
	{
		public static string SerializeDataTable(DataTable? sourceTable)
		{
			var container = new SerializedDataTable();
			if (sourceTable is null)
			{
				return JsonConvert.SerializeObject(container);
			}

			container.TableName = sourceTable.TableName;
			container.PrimaryKey = sourceTable.PrimaryKey.Length == 0 ? null : sourceTable.PrimaryKey[0].ColumnName;
			container.Columns.AddRange(sourceTable.Columns.Cast<DataColumn>().Select(column => new SerializedDataColumn(column)));
			foreach (DataRow row in sourceTable.Rows)
			{
				var rowData = new List<string?>();
				for(var i = 0; i < sourceTable.Columns.Count; i++)
				{
					rowData.Add(ToJson(row[i], TypeMappingHelper.StringToSqlDbType(sourceTable.Columns[i].DataType.Name)));
				}

				container.Rows.Add(rowData);
			}

			return JsonConvert.SerializeObject(container);
		}

		public static DataTable DeserializeDataTable(string json)
		{
			var container = JsonConvert.DeserializeObject<SerializedDataTable>(json);
			var deserializeDataTable = new DataTable();
			if (container is null)
			{
				return deserializeDataTable;
			}

			deserializeDataTable.Locale = CultureInfo.InvariantCulture;
			deserializeDataTable.TableName = container.TableName;
			deserializeDataTable.Columns.AddRange(container.Columns.Select(x => x.ToDataColumn()).ToArray());

			foreach (var rowData in container.Rows)
			{
				var row = deserializeDataTable.NewRow();
				for (var i = 0; i < rowData.Count; i++)
				{
					var value = rowData[i];
					if (value != null)
					{
						row[i] = SqlValueToCsValue(FromJson(value, container.Columns[i].GetDataType().Name));
					}
				}
				deserializeDataTable.Rows.Add(row);
			}

			if (!string.IsNullOrEmpty(container.PrimaryKey) && deserializeDataTable.Columns[container.PrimaryKey] is { } dataColumn)
			{
				deserializeDataTable.PrimaryKey = [dataColumn];
			}

			return deserializeDataTable;
		}

		public static string? ToJson(object? rawValue, string dataTypeName)
		{
			return ToJson(rawValue, TypeMappingHelper.StringToSqlDbType(dataTypeName));
		}

		public static string? CsValueToJson(object? rawValue, DbType dataType)
		{
			return ToJson(CsValueToSqlValue(rawValue, dataType), TypeMappingHelper.DbTypeToSqlDbType(dataType));
		}

		public static string? ToJson(object? rawValue, SqlDbType dataType)
		{
			switch (rawValue)
			{
				// 1) Handle null / INullable
				case null:
				case DBNull:
				case INullable { IsNull: true }:
					// Timestamp is a complex edge case as the returned values differ between System.Data and Microsoft.Data
					//
					// We force the behavior to be identical to System.Data. Which means when the value is `null`, we return
					// an empty SqlBinary array.
					return dataType == SqlDbType.Timestamp ? Convert.ToBase64String([]) : null;
			}

			var lazyJsonSerializationException =
				new Lazy<JsonSerializationException>(() =>
					new JsonSerializationException($"Invalid value: {rawValue}, raw value type: {rawValue?.GetType().FullName}, for data type: {dataType}"));

			switch (dataType)
			{
				case SqlDbType.Binary:
				case SqlDbType.VarBinary:
				case SqlDbType.Timestamp:
				case SqlDbType.Image:
				{
					return rawValue switch
					{
						SqlBinary sqlBinary => Convert.ToBase64String(sqlBinary.Value),
						byte[] byteArray => Convert.ToBase64String(byteArray),
						string base64String => Convert.ToBase64String(Convert.FromBase64String(base64String)),
						_ => throw lazyJsonSerializationException.Value
					};
				}

				case SqlDbType.Bit:
				{
					return rawValue switch
					{
						SqlBoolean sqlBit => XmlConvert.ToString(sqlBit.Value),
						byte byteValue => XmlConvert.ToString(byteValue != 0),
						sbyte sbyteValue => XmlConvert.ToString(sbyteValue != 0),
						short shortValue => XmlConvert.ToString(shortValue != 0),
						int intValue => XmlConvert.ToString(intValue != 0),
						long longValue => XmlConvert.ToString(longValue != 0),
						bool bit => XmlConvert.ToString(bit),
						string strValue => XmlConvert.ToString(bool.TryParse(strValue, out var result) ? result : int.Parse(strValue) != 0),
						_ => throw lazyJsonSerializationException.Value
					};
				}

				case SqlDbType.TinyInt:
				case SqlDbType.SmallInt:
				case SqlDbType.Int:
				case SqlDbType.BigInt:
				{
					return rawValue switch
					{
						SqlByte sqlByte => XmlConvert.ToString(sqlByte.Value),
						SqlInt16 sqlSmallInt => XmlConvert.ToString(sqlSmallInt.Value),
						SqlInt32 sqlInt => XmlConvert.ToString(sqlInt.Value),
						SqlInt64 sqlBigInt => XmlConvert.ToString(sqlBigInt.Value),
						byte byteValue => XmlConvert.ToString(byteValue),
						sbyte sbyteValue => XmlConvert.ToString(sbyteValue),
						short shortValue => XmlConvert.ToString(shortValue),
						int intValue => XmlConvert.ToString(intValue),
						long longValue => XmlConvert.ToString(longValue),
						float or double or decimal => XmlConvert.ToString(Convert.ToInt64(rawValue)),
						_ => throw lazyJsonSerializationException.Value
					};
				}

				case SqlDbType.Decimal:
				case SqlDbType.Money:
				case SqlDbType.SmallMoney:
				{
					return rawValue switch
					{
						SqlDecimal sqlDecimal => sqlDecimal.ToString(),
						SqlMoney sqlMoney => new SqlDecimal(sqlMoney.Value).ToString(),
						decimal dec => new SqlDecimal(dec).ToString(),
						int intValue => new SqlDecimal(intValue).ToString(),
						long longValue => new SqlDecimal(longValue).ToString(),
						double doubleValue => new SqlDecimal((decimal)doubleValue).ToString(),
						float floatValue => new SqlDecimal((decimal)floatValue).ToString(),
						string strValue => decimal.TryParse(strValue, out var dec) ? new SqlDecimal(dec).ToString() : throw lazyJsonSerializationException.Value,
						_ => throw lazyJsonSerializationException.Value
					};
				}

				case SqlDbType.Float:
				case SqlDbType.Real:
				{
					return rawValue switch
					{
						SqlDouble sqlDouble => XmlConvert.ToString(sqlDouble.Value),
						SqlSingle sqlSingle => XmlConvert.ToString(sqlSingle.Value),
						double d => XmlConvert.ToString(d),
						float f => XmlConvert.ToString(f),
						byte byteValue => XmlConvert.ToString(byteValue),
						sbyte sbyteValue => XmlConvert.ToString(sbyteValue),
						short shortValue => XmlConvert.ToString(shortValue),
						int intValue => XmlConvert.ToString(intValue),
						long longValue => XmlConvert.ToString(longValue),
						string strValue => double.TryParse(strValue, out var result) ? XmlConvert.ToString(result) : throw lazyJsonSerializationException.Value,
						_ => throw lazyJsonSerializationException.Value
					};
				}

				case SqlDbType.UniqueIdentifier:
				{
					return rawValue switch
					{
						SqlGuid sqlGuid => sqlGuid.Value.ToString(),
						Guid guid => guid.ToString(),
						string str => str,
						byte[] byteArray => new Guid(byteArray).ToString(),
						_ => throw lazyJsonSerializationException.Value
					};
				}

				//
				// Text-likes (Char, NChar, VarChar, NVarChar, Text, NText, Xml)
				//
				case SqlDbType.Char:
				case SqlDbType.NChar:
				case SqlDbType.VarChar:
				case SqlDbType.NVarChar:
				case SqlDbType.Text:
				case SqlDbType.NText:
				{
					return rawValue switch
					{
						SqlString sqlString => sqlString.Value,
						byte byteValue => XmlConvert.ToString(byteValue),
						char c => c.ToString(),
						string str => str,
						_ => throw lazyJsonSerializationException.Value
					};
				}

				case SqlDbType.Xml:
				{
					return rawValue switch
					{
						// Expecting SqlXml
						SqlXml sqlXml => sqlXml.Value,
						string xmlString => xmlString,
						_ => throw lazyJsonSerializationException.Value
					};
				}

				//
				// DateTime-likes
				//
				case SqlDbType.DateTime:
				case SqlDbType.SmallDateTime:
				case SqlDbType.DateTime2:
				case SqlDbType.Date:
				{
					return rawValue switch
					{
						SqlDateTime sqlDateTime => sqlDateTime.Value.ToString("o", CultureInfo.InvariantCulture),
						DateTime dt => dt.ToString("o", CultureInfo.InvariantCulture),
						DateTimeOffset dto => dto.ToString("o", CultureInfo.InvariantCulture),
						_ => throw lazyJsonSerializationException.Value
					};
				}

				case SqlDbType.Time:
				{
					return rawValue is TimeSpan ts
						? XmlConvert.ToString(ts)
						: throw lazyJsonSerializationException.Value;
				}

				case SqlDbType.DateTimeOffset:
				{
					return rawValue is DateTimeOffset dto
						? XmlConvert.ToString(dto)
						: throw lazyJsonSerializationException.Value;
				}

				case SqlDbType.Variant:
				case SqlDbType.Udt:
				case SqlDbType.Structured:
				{
					return JsonHelper.ToJson(rawValue);
				}

				default:
				{
					throw new NotSupportedException($"Unsupported SqlDbType: {dataType}");
				}
			}
		}

		public static object FromJson(string? serializedJsonString, string dataTypeName)
		{
			var sqlDbType = TypeMappingHelper.StringToSqlDbType(dataTypeName);
			if (sqlDbType is SqlDbType.Udt or SqlDbType.Structured)
			{
#pragma warning disable CS8603 // Possible null reference return.
				var udtType = Type.GetType(dataTypeName);
				return udtType != null
					? JsonHelper.FromJson(serializedJsonString, udtType!)
					: serializedJsonString;
#pragma warning restore CS8603 // Possible null reference return.
			}

			return FromJson(serializedJsonString, sqlDbType);
		}

		public static object FromJson(string? serializedJsonString, SqlDbType dataType)
		{
			switch (dataType)
			{
				case SqlDbType.BigInt:
					{
						if (serializedJsonString == null)
						{
							return SqlInt64.Null;
						}

						long value = XmlConvert.ToInt64(serializedJsonString);
						return new SqlInt64(value);
					}

				case SqlDbType.Binary:
				case SqlDbType.VarBinary:
					{
						if (serializedJsonString == null)
						{
							return SqlBinary.Null;
						}

						var bytes = Convert.FromBase64String(serializedJsonString);
						return new SqlBinary(bytes);
					}

				case SqlDbType.Timestamp:
					{
						if (serializedJsonString == null)
						{
							return SqlBinary.Null;
						}

						var bytes = Convert.FromBase64String(serializedJsonString);
						return new SqlBinary(bytes);
					}

				case SqlDbType.Image:
					{
						if (serializedJsonString == null)
						{
							return SqlBinary.Null;
						}

						var bytes = Convert.FromBase64String(serializedJsonString);
						return new SqlBinary(bytes);
					}

				case SqlDbType.Bit:
					{
						if (serializedJsonString == null)
						{
							return SqlBoolean.Null;
						}
						bool parsedBool = XmlConvert.ToBoolean(serializedJsonString);
						return new SqlBoolean(parsedBool);
					}

				case SqlDbType.TinyInt:
					{
						if (serializedJsonString == null)
						{
							return SqlByte.Null;
						}

						byte parsed = XmlConvert.ToByte(serializedJsonString);
						return new SqlByte(parsed);
					}

				case SqlDbType.SmallInt:
					{
						if (serializedJsonString == null)
						{
							return SqlInt16.Null;
						}

						short parsed = XmlConvert.ToInt16(serializedJsonString);
						return new SqlInt16(parsed);
					}

				case SqlDbType.Int:
					{
						if (serializedJsonString == null)
						{
							return SqlInt32.Null;
						}

						int parsed = XmlConvert.ToInt32(serializedJsonString);
						return new SqlInt32(parsed);
					}

				case SqlDbType.Decimal:
					{
						if (serializedJsonString == null)
						{
							return SqlDecimal.Null;
						}

						return SqlDecimal.Parse(serializedJsonString);
					}

				case SqlDbType.Money:
				case SqlDbType.SmallMoney:
					{
						if (serializedJsonString == null)
						{
							return SqlMoney.Null;
						}

						decimal dec = XmlConvert.ToDecimal(serializedJsonString);
						return new SqlMoney(dec);
					}

				case SqlDbType.Float:
					{
						if (serializedJsonString == null)
						{
							return SqlDouble.Null;
						}

						double d = XmlConvert.ToDouble(serializedJsonString);
						return new SqlDouble(d);
					}

				case SqlDbType.Real:
					{
						if (serializedJsonString == null)
						{
							return SqlSingle.Null;
						}

						float f = XmlConvert.ToSingle(serializedJsonString);
						return new SqlSingle(f);
					}

				case SqlDbType.UniqueIdentifier:
					{
						if (serializedJsonString == null)
						{
							return SqlGuid.Null;
						}

						Guid g = Guid.Parse(serializedJsonString);
						return new SqlGuid(g);
					}

				//
				// String-like
				//
				case SqlDbType.Char:
				case SqlDbType.NChar:
				case SqlDbType.VarChar:
				case SqlDbType.NVarChar:
				case SqlDbType.Text:
				case SqlDbType.NText:
					{
						if (serializedJsonString == null)
						{
							return SqlString.Null;
						}

						// Use the string directly
						return new SqlString(serializedJsonString);
					}

				case SqlDbType.Xml:
					{
						if (serializedJsonString == null)
						{
							return SqlXml.Null;
						}

						return StringToSqlXml(serializedJsonString);
					}

				//
				// DateTime-likes
				//
				case SqlDbType.DateTime:
				case SqlDbType.SmallDateTime:
					{
						if (serializedJsonString == null)
						{
							return SqlDateTime.Null;
						}
						DateTime dt = DateTime.ParseExact(serializedJsonString, "o", CultureInfo.InvariantCulture);
						return new SqlDateTime(dt);
					}

				case SqlDbType.Time:
					{
						if (serializedJsonString == null)
						{
							return DBNull.Value;
						}
						return XmlConvert.ToTimeSpan(serializedJsonString);
					}

				case SqlDbType.Date:
					{
						if (serializedJsonString == null)
						{
							return DBNull.Value;
						}
						return DateTime.ParseExact(serializedJsonString, "o", CultureInfo.InvariantCulture);
					}

				case SqlDbType.DateTime2:
				{
					if (serializedJsonString == null)
					{
						return DBNull.Value;
					}
					return DateTimeOffset.ParseExact(serializedJsonString, "o", CultureInfo.InvariantCulture).DateTime;
				}

				case SqlDbType.DateTimeOffset:
					{
						if (serializedJsonString == null)
						{
							return DBNull.Value;
						}

						return XmlConvert.ToDateTimeOffset(serializedJsonString);
					}

				case SqlDbType.Variant:
				{
					if (serializedJsonString == null)
					{
						return DBNull.Value;
					}

					return serializedJsonString;
				}

				case SqlDbType.Udt:
				case SqlDbType.Structured:
				default:
				{
					return serializedJsonString == null ? DBNull.Value : serializedJsonString;
				}
			}
		}

		public static object? SqlValueToCsValue(object? value)
		{
			return value switch
			{
				DBNull _ => DBNull.Value,
				SqlString s => s.IsNull ? DBNull.Value : s.Value,
				SqlBoolean b => b.IsNull ? DBNull.Value : b.Value,
				SqlByte by => by.IsNull ? DBNull.Value : by.Value,
				SqlInt16 i16 => i16.IsNull ? DBNull.Value : i16.Value,
				SqlInt32 i32 => i32.IsNull ? DBNull.Value : i32.Value,
				SqlInt64 i64 => i64.IsNull ? DBNull.Value : i64.Value,
				SqlSingle sf => sf.IsNull ? DBNull.Value : sf.Value,
				SqlDouble d => d.IsNull ? DBNull.Value : d.Value,
				SqlDecimal dec => dec.IsNull ? DBNull.Value : dec.Value,
				SqlMoney m => m.IsNull ? DBNull.Value : m.Value,
				SqlDateTime dt => dt.IsNull ? DBNull.Value : dt.Value,
				SqlGuid g => g.IsNull ? DBNull.Value : g.Value,
				SqlBinary bin => bin.IsNull ? DBNull.Value : bin.Value,
				SqlXml xml => xml.IsNull ? DBNull.Value : xml.Value,
				_ => value
			};
		}

		public static object CsValueToSqlValue(object? csValue, DbType? type)
		{
			// Normalize to null for simpler conditions
			if (csValue is DBNull)
			{
				csValue = null;
			}

#pragma warning disable CS8603 // Possible null reference return.
			return type switch
			{
				DbType.AnsiString => ToSqlString(),
				DbType.AnsiStringFixedLength => ToSqlString(),
				DbType.Binary => ToSqlBinary(),
				DbType.Boolean => ToSqlBool(),
				DbType.Byte => ToSqlByte(),
				DbType.Currency => ToSqlMoney(),
				DbType.Date => ToSqlDateTime(),
				DbType.DateTime => ToSqlDateTime(),
				DbType.DateTime2 => ToSqlDateTimeOffset(),
				DbType.DateTimeOffset => ToSqlDateTimeOffset(),
				DbType.Decimal => ToSqlDecimal(),
				DbType.Double => ToSqlDouble(),
				DbType.Guid => ToSqlGuid(),
				DbType.Int16 => ToSqlInt16(),
				DbType.Int32 => ToSqlInt32(),
				DbType.Int64 => ToSqlInt64(),
				DbType.Object => csValue,
				DbType.SByte => ToSqlByte(),
				DbType.Single => ToSqlSingle(),
				DbType.String => ToSqlString(),
				DbType.StringFixedLength => ToSqlString(),
				DbType.Time => ToSqlTimeSpan(),
				DbType.UInt16 => ToSqlInt16(),
				DbType.UInt32 => ToSqlInt32(),
				DbType.UInt64 => ToSqlInt64(),
				DbType.Xml => ToSqlXml(),
				null => null,
				_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} or DbType {type}"),
			};
#pragma warning restore CS8603 // Possible null reference return.

			object ToSqlTimeSpan()
			{
				return csValue switch
				{
					null => DBNull.Value,
					TimeSpan value => value,
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlDateTimeOffset()
			{
				return csValue switch
				{
					null => DBNull.Value,
					DateTimeOffset value => value,
					DateTime dtValue => DateTimeToDateTimeOffset(dtValue),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			DateTimeOffset DateTimeToDateTimeOffset(DateTime dtValue)
			{
				switch (dtValue.Kind)
				{
					case DateTimeKind.Unspecified:
					case DateTimeKind.Utc:
						return new DateTimeOffset(dtValue, TimeSpan.Zero);

					case DateTimeKind.Local:
						var offset = TimeZoneInfo.Local.GetUtcOffset(dtValue);
						return new DateTimeOffset(dtValue, offset);

					default:
						throw new ArgumentException("Invalid DateTime.Kind value.");
				}
			}

			object ToSqlDateTime()
			{
				return csValue switch
				{
					null => SqlDateTime.Null,
					DateTime dtValue => new SqlDateTime(dtValue),
					SqlDateTime value => value,
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlMoney()
			{
				return csValue switch
				{
					null => SqlMoney.Null,
					SqlMoney value => value,
					decimal decValue => new SqlMoney(decValue),
					double doubleValue => new SqlMoney(doubleValue),
					int intValue => new SqlMoney(intValue),
					long longValue => new SqlMoney(longValue),
					float floatValue => new SqlMoney((decimal)floatValue),
					string strValue => decimal.TryParse(strValue, out var dec) ? new SqlMoney(dec) : throw new ArgumentException($"Unsupported string value {strValue} for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlByte()
			{
				return csValue switch
				{
					null => SqlByte.Null,
					SqlByte value => value,
					byte byteValue => new SqlByte(byteValue),
					sbyte sbyteValue => sbyteValue >= 0 ? new SqlByte((byte)sbyteValue) : throw new ArgumentException($"Value {sbyteValue} is out of range for DbType {type}"),
					short shortValue => shortValue is >= 0 and <= byte.MaxValue ? new SqlByte((byte)shortValue) : throw new ArgumentException($"Value {shortValue} is out of range for DbType {type}"),
					int intValue => intValue is >= 0 and <= byte.MaxValue ? new SqlByte((byte)intValue) : throw new ArgumentException($"Value {intValue} is out of range for DbType {type}"),
					long longValue => longValue is >= 0 and <= byte.MaxValue ? new SqlByte((byte)longValue) : throw new ArgumentException($"Value {longValue} is out of range for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlBool()
			{
				return csValue switch
				{
					null => SqlBoolean.Null,
					SqlBoolean value => value,
					bool boolValue => new SqlBoolean(boolValue),
					string str => new SqlBoolean(bool.TryParse(str, out var result) ? result : throw new ArgumentException($"Unsupported string value {str} for DbType {type}")),
					int intValue => new SqlBoolean(intValue != 0),
					byte byteValue => new SqlBoolean(byteValue != 0),
					sbyte sbyteValue => new SqlBoolean(sbyteValue != 0),
					short shortValue => new SqlBoolean(shortValue != 0),
					long longValue => new SqlBoolean(longValue != 0),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlBinary()
			{
				return csValue switch
				{
					null => SqlBinary.Null,
					SqlBinary value => value,
					byte[] byteArray => new SqlBinary(byteArray),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlString()
			{
				return csValue switch
				{
					null => SqlString.Null,
					SqlString value => value,
					string str => new SqlString(str),
					_ => new SqlString(csValue.ToString())
				};
			}

			object ToSqlDecimal()
			{
				return csValue switch
				{
					null => SqlDecimal.Null,
					SqlDecimal value => value,
					decimal decValue => new SqlDecimal(decValue),
					double doubleValue => new SqlDecimal((decimal)doubleValue),
					int intValue => new SqlDecimal(intValue),
					long longValue => new SqlDecimal(longValue),
					float floatValue => new SqlDecimal((decimal)floatValue),
					string strValue => decimal.TryParse(strValue, out var dec) ? new SqlDecimal(dec) : throw new ArgumentException($"Unsupported string value {strValue} for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlDouble()
			{
				return csValue switch
				{
					null => SqlDouble.Null,
					SqlDouble value => value,
					double doubleValue => new SqlDouble(doubleValue),
					decimal decValue => new SqlDouble((double)decValue),
					int intValue => new SqlDouble(intValue),
					long longValue => new SqlDouble(longValue),
					float floatValue => new SqlDouble(floatValue),
					string strValue => double.TryParse(strValue, out var dbl) ? new SqlDouble(dbl) : throw new ArgumentException($"Unsupported string value {strValue} for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlGuid()
			{
				return csValue switch
				{
					null => SqlGuid.Null,
					SqlGuid value => value,
					Guid guidValue => new SqlGuid(guidValue),
					string strValue => Guid.TryParse(strValue, out var guid) ? new SqlGuid(guid) : throw new ArgumentException($"Unsupported string value {strValue} for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlInt16()
			{
				return csValue switch
				{
					null => SqlInt16.Null,
					SqlInt16 value => value,
					short shortValue => new SqlInt16(shortValue),
					byte byteValue => new SqlInt16(byteValue),
					sbyte sbyteValue => new SqlInt16(sbyteValue),
					int intValue => intValue is >= short.MinValue and <= short.MaxValue ? new SqlInt16((short)intValue) : throw new ArgumentException($"Value {intValue} is out of range for DbType {type}"),
					long longValue => longValue is >= short.MinValue and <= short.MaxValue ? new SqlInt16((short)longValue) : throw new ArgumentException($"Value {longValue} is out of range for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlInt32()
			{
				return csValue switch
				{
					null => SqlInt32.Null,
					SqlInt32 value => value,
					int intValue => new SqlInt32(intValue),
					short shortValue => new SqlInt32(shortValue),
					byte byteValue => new SqlInt32(byteValue),
					sbyte sbyteValue => new SqlInt32(sbyteValue),
					long longValue => longValue is >= int.MinValue and <= int.MaxValue ? new SqlInt32((int)longValue) : throw new ArgumentException($"Value {longValue} is out of range for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlInt64()
			{
				return csValue switch
				{
					null => SqlInt64.Null,
					SqlInt64 value => value,
					long longValue => new SqlInt64(longValue),
					int intValue => new SqlInt64(intValue),
					short shortValue => new SqlInt64(shortValue),
					byte byteValue => new SqlInt64(byteValue),
					sbyte sbyteValue => new SqlInt64(sbyteValue),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlSingle()
			{
				return csValue switch
				{
					null => SqlSingle.Null,
					SqlSingle value => value,
					float floatValue => new SqlSingle(floatValue),
					double doubleValue => new SqlSingle((float)doubleValue),
					decimal decValue => new SqlSingle((float)decValue),
					int intValue => new SqlSingle(intValue),
					long longValue => new SqlSingle(longValue),
					string strValue => float.TryParse(strValue, out var flt) ? new SqlSingle(flt) : throw new ArgumentException($"Unsupported string value {strValue} for DbType {type}"),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}

			object ToSqlXml()
			{
				return csValue switch
				{
					null => SqlXml.Null,
					string strValue => StringToSqlXml(strValue),
					_ => throw new ArgumentException($"Unsupported value type {csValue?.GetType().Name} for DbType {type}")
				};
			}
		}

		public static SqlDbType SqlTypeToSqlDbType(object sqlValue)
		{
			return sqlValue switch
			{
				SqlInt32 => SqlDbType.Int,
				SqlBoolean => SqlDbType.Bit,
				SqlString => SqlDbType.NVarChar,
				SqlDateTime => SqlDbType.DateTime,
				SqlDecimal => SqlDbType.Decimal,
				SqlDouble => SqlDbType.Float,
				SqlGuid => SqlDbType.UniqueIdentifier,
				SqlBinary => SqlDbType.VarBinary,
				SqlMoney => SqlDbType.Money,
				SqlXml => SqlDbType.Xml,
				_ => throw new NotSupportedException($"Unsupported SqlType: {sqlValue.GetType().Name}")
			};
		}

		static SqlXml StringToSqlXml(string xmlString)
		{
			if (string.IsNullOrEmpty(xmlString))
			{
				return SqlXml.Null;
			}

			var settings = new XmlReaderSettings
			{
				ConformanceLevel = ConformanceLevel.Fragment // Allow multiple root elements or fragments as they are supported by SQL Server
			};

			using var stringReader = new StringReader(xmlString);
			using var xmlReader = XmlReader.Create(stringReader, settings);

			return new SqlXml(xmlReader);
		}
	}
}
