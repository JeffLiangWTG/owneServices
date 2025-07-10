using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using CargoWise.Common;
using CargoWise.Data.SqlProxy.Interface;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("SQL Over Http Connection")]
	class HttpDataReader : DbDataReader
	{
		public HttpDataReader(SqlProxyDataReader reader)
		{
			_ = Argument.NotNull(reader, nameof(reader));
			_ = Argument.NotNull(reader.ResultSets, nameof(reader.ResultSets));

			this.reader = reader;
			foreach (var resultSet in this.reader.ResultSets)
			{
				foreach (var row in resultSet.Rows)
				{
					for (var i = 0; i < row.Length; i++)
					{
						if (row[i] == null)
						{
							row[i] = DBNull.Value;
						}
					}
				}
			}
		}

		ResultSet[] ResultSets
		{
			get
			{
				return [.. reader.ResultSets];
			}
		}

		public object[] CurrentRow
		{
			get
			{
				if (currentRow < 0 || currentRow >= CurrentResultSet.Rows.Count)
				{
					throw new IndexOutOfRangeException();
				}

				if (CurrentResultSet.Rows[currentRow] == null)
				{
					throw new NullReferenceException();
				}

				return CurrentResultSet.Rows[currentRow];
			}
		}

		public ResultSet CurrentResultSet
		{
			get
			{
				if (currentResultSet < ResultSets.GetLowerBound(0) || currentResultSet >= ResultSets.Length)
				{
					throw new IndexOutOfRangeException();
				}

				var result = ResultSets[currentResultSet] ?? throw new NullReferenceException();
				return result;
			}
		}

		public override bool HasRows
		{
			get
			{
				return
					ResultSets[currentResultSet] != null
					&& ResultSets[currentResultSet].Rows != null
					&& ResultSets[currentResultSet].Rows.Count > 0;
			}
		}

		#region IDataReader Members

		public override void Close()
		{
		}

		public override int Depth
		{
			get { throw new NotSupportedException(); }
		}

		public override DataTable GetSchemaTable()
		{
			var result = new DataTable();
			_ = result.Columns.Add("ColumnName", typeof(string));
			_ = result.Columns.Add("DataType", typeof(Type));

			foreach (var field in CurrentResultSet.FieldNames)
			{
				_ = result.Rows.Add(field.Name, TypeMappingHelper.GetTypeFromSqlType(field.Type));
			}

			return result;
		}

		public override bool IsClosed => false;

		public override bool NextResult()
		{
			CurrentResultSet.Rows = null; // Optimisation - drop memory usage as soon as possible
			currentResultSet++;
			currentRow = StartingRow;
			return currentResultSet < ResultSets.Length;
		}

		public override bool Read()
		{
			currentRow++;
			var result = currentRow < CurrentResultSet.Rows.Count;
			return result;
		}

		public override int RecordsAffected => 0;

		#endregion

		#region IDataRecord Members

		public override int FieldCount => CurrentResultSet.FieldNames.Length;

		public override bool GetBoolean(int i)
		{
			return GetValue<bool>(i);
		}

		public override byte GetByte(int i)
		{
			return GetValue<byte>(i);
		}

		public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferoffset, int length)
		{
			var srcBytes = Convert.FromBase64String((string)CurrentRow[i]);
			var maxLen = srcBytes?.Length ?? 0;
			if (srcBytes != null && buffer != null)
			{
				if (dataIndex < maxLen)
				{
					if ((dataIndex + length) > maxLen)
					{
						maxLen -= (int)dataIndex;
					}
					else
					{
						maxLen = length;
					}
					if (maxLen < 0)
					{
						// the value of length shall be negative
						throw new ArgumentOutOfRangeException(nameof(length));
					}
					Array.Copy(srcBytes, dataIndex, buffer, bufferoffset, maxLen);
					return maxLen;
				}
			}

			return maxLen;
		}

		public override char GetChar(int i)
		{
			return GetValue<char>(i);
		}

		public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			Argument.NotNull(buffer, nameof(buffer));
			if (length <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(length));
			}

			var currentValue = CurrentRow[i];
			if (currentValue != null)
			{
				var sourceChars = ((string)currentValue).ToArray();
				var sourceLength = (long)sourceChars.Length;
				var actualSizeRead = sourceLength;
				if (fieldoffset >= sourceLength)
				{
					actualSizeRead = 0;
				}
				else
				{
					if (fieldoffset + length <= sourceLength)
					{
						actualSizeRead = length;
					}
					else
					{
						actualSizeRead -= fieldoffset;
					}
					Array.Copy(sourceChars, fieldoffset, buffer, bufferoffset, actualSizeRead);
				}
				return actualSizeRead;
			}
			else
			{
				throw new ArgumentNullException(nameof(i), $"Value does not exist in {nameof(CurrentRow)}");
			}
		}

		public override string GetDataTypeName(int i)
		{
			throw new NotSupportedException();
		}

		public override DateTime GetDateTime(int i)
		{
			return GetValue<DateTime>(i);
		}

		public override decimal GetDecimal(int i)
		{
			return GetValue<decimal>(i);
		}

		public override double GetDouble(int i)
		{
			return GetValue<double>(i);
		}

		public override Type GetFieldType(int i)
		{
			var fieldNames = CurrentResultSet.FieldNames[i];
			if (fieldNames != null)
			{
				return TypeMappingHelper.GetTypeFromSqlType(fieldNames.Type);
			}
			else
			{
				throw new ArgumentNullException(nameof(i), $"Value does not exist in {nameof(CurrentResultSet)}.{nameof(CurrentResultSet.FieldNames)}");
			}
		}

		public override float GetFloat(int i)
		{
			return GetValue<float>(i);
		}

		public override Guid GetGuid(int i)
		{
			return GetValue<Guid>(i);
		}

		public override short GetInt16(int i)
		{
			return GetValue<short>(i);
		}

		public override int GetInt32(int i)
		{
			return GetValue<int>(i);
		}

		public override long GetInt64(int i)
		{
			return GetValue<long>(i);
		}

		public override string GetName(int i)
		{
			try
			{
				var currentFieldName = CurrentResultSet.FieldNames[i]
					?? throw new ArgumentNullException(nameof(i), $"Could not find specified column in {CurrentResultSet}.{nameof(CurrentResultSet.FieldNames)}");
				return currentFieldName.Name;
			}
			catch
			{
				throw new IndexOutOfRangeException("Could not find specified column in results");
			}
		}

		public override int GetOrdinal(string name)
		{
			for (var i = 0; i < CurrentResultSet.FieldNames.Length; i++)
			{
				if (GetName(i) == name)
				{
					return i;
				}
			}
			throw new ArgumentOutOfRangeException(nameof(name));
		}

		public override string GetString(int i)
		{
			return GetValue<string>(i);
		}

		public T GetValue<T>(int i)
		{
			return (T)GetValue(i);
		}

		public override object GetValue(int i)
		{
			if (i >= CurrentRow.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(i));
			}

			var fieldType = GetFieldType(i);
			var value = CurrentRow[i];
			if (value == DBNull.Value)
			{
				if (fieldType == typeof(Guid))
				{
					return value;
				}

				if (fieldType.IsValueType)
				{
					return Activator.CreateInstance(fieldType);
				}

				return null;
			}

			if (typeof(IConvertible).IsAssignableFrom(fieldType))
			{
				return Convert.ChangeType(CurrentRow[i], GetFieldType(i));
			}
			else
			{
				return fieldType switch
				{
					Type t when t == typeof(Guid) => ToGuid(value),
					Type t when t == typeof(DateTimeOffset) => ToDateTimeOffset(value),
					Type t when t == typeof(TimeSpan) => ToDateTime(value),
					Type t when t == typeof(BigInteger) => ToInt64(value),
					Type t when t == typeof(byte[]) => Convert.FromBase64String(value.ToString()),
					_ => value,
				};
			}

			Guid ToGuid(object value)
			{
				if (value is Guid guid)
				{
					return guid;
				}

				return Guid.Parse(value.ToString());
			}

			DateTime ToDateTime(object value)
			{
				if (value is DateTime dateTime)
				{
					return dateTime;
				}

#pragma warning disable CW1122 // Do Not Use DateTime Parse Method
				return DateTime.Parse(value.ToString());
#pragma warning restore CW1122 // Do Not Use DateTime Parse Method
			}

			DateTimeOffset ToDateTimeOffset(object value)
			{
				if (value is DateTimeOffset dateTimeOffset)
				{
					return dateTimeOffset;
				}

				return DateTimeOffset.Parse(value.ToString());
			}

			long ToInt64(object value)
			{
				if (value is long longValue)
				{
					return longValue;
				}

				return long.Parse(value.ToString());
			}
		}

		public override int GetValues(object[] values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			Array.Copy(CurrentRow, values, CurrentRow.Length);

			return CurrentRow.Length;
		}

		public override bool IsDBNull(int i)
		{
			if (i >= CurrentRow.Length || i < CurrentRow.GetLowerBound(0))
			{
				throw new ArgumentOutOfRangeException(nameof(i));
			}
			var currentValue = CurrentRow[i];

			return currentValue == DBNull.Value;
		}

		public override object this[string name]
		{
			get
			{
				var i = GetOrdinal(name);
				return GetValue(i);
			}
		}

		public override object this[int i]
		{
			get
			{
				return GetValue(i);
			}
		}

		public override IEnumerator GetEnumerator()
		{
			return new DbEnumerator((IDataReader)this, IsCommandBehavior(CommandBehavior.CloseConnection));
		}

		bool IsCommandBehavior(CommandBehavior condition)
		{
			return condition == (condition & CommandBehavior);
		}
		#endregion

		const int StartingRow = -1;
		readonly SqlProxyDataReader reader;
		const CommandBehavior CommandBehavior = System.Data.CommandBehavior.Default;

		int currentResultSet;
		int currentRow = StartingRow;
	}
}
