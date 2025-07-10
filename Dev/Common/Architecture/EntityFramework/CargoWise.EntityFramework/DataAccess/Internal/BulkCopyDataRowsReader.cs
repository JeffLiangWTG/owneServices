using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Schema;

namespace CargoWise.EntityFramework;

public class BulkCopyDataRowsReader : IDataReader
{
	readonly IEnumerator<DataRow> enumerator;
	readonly DataTable dataTable;
	bool isClosed;
	readonly SchemaColumnCollection schemas;

	public BulkCopyDataRowsReader(IList<DataRow> dataRows, ITableSchema schema)
	{
		Argument.NotNull(dataRows, nameof(dataRows));

		if (dataRows.Count <= 0)
		{
			throw new ArgumentException("DataRows cannot be empty.", nameof(dataRows));
		}

		dataTable = dataRows[0].Table;
		enumerator = dataRows.GetEnumerator();
		FieldCount = dataTable.Columns.Count;
		schemas = schema.All;
	}

	public void Dispose()
	{
		Close();
		GC.SuppressFinalize(this);
	}

	public string GetName(int i)
	{
		return dataTable.Columns[i].ColumnName;
	}

	public string GetDataTypeName(int i)
	{
		return IsNonBitBoolColumn(i) ? "System.String" : dataTable.Columns[i].DataType.Name;
	}

	public Type GetFieldType(int i)
	{
		return IsNonBitBoolColumn(i) ? typeof(string) : dataTable.Columns[i].DataType;
	}

	public object GetValue(int i)
	{
		var row = enumerator.Current;
		var value = row?[i];
		if (value != null && IsNonBitBoolColumn(i))
		{
			var boolValue = (bool)value;
			return boolValue ? "Y" : "N";
		}

		return value;
	}

	public int GetValues(object[] values)
	{
		var count = Math.Min(values.Length, FieldCount);

		for (var i = 0; i < count; i++)
		{
			values[i] = GetValue(i);
		}

		return count;
	}

	public int GetOrdinal(string name)
	{
		return dataTable.Columns[name].Ordinal;
	}

	public bool GetBoolean(int i)
	{
		return (bool)GetValue(i);
	}

	public byte GetByte(int i)
	{
		return (byte)GetValue(i);
	}

	public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
	{
		var value = GetValue(i);

		if (value is byte[] bytes)
		{
			Array.Copy(bytes, fieldOffset, buffer, bufferoffset, length);
			return length;
		}

		throw new InvalidCastException("The value is not a byte array.");
	}

	public char GetChar(int i)
	{
		return (char)GetValue(i);
	}

	public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
	{
		var value = GetValue(i);

		if (value is string str)
		{
			var charsToCopy = Math.Min(length, str.Length - (int)fieldoffset);
			Array.Copy(str.ToCharArray((int)fieldoffset, charsToCopy), 0, buffer, bufferoffset, charsToCopy);
			return charsToCopy;
		}

		throw new InvalidCastException("The value is not a string.");
	}

	public Guid GetGuid(int i)
	{
		return (Guid)GetValue(i);
	}

	public short GetInt16(int i)
	{
		return (short)GetValue(i);
	}

	public int GetInt32(int i)
	{
		return (int)GetValue(i);
	}

	public long GetInt64(int i)
	{
		return (long)GetValue(i);
	}

	public float GetFloat(int i)
	{
		return (float)GetValue(i);
	}

	public double GetDouble(int i)
	{
		return (double)GetValue(i);
	}

	public string GetString(int i)
	{
		return (string)GetValue(i);
	}

	public decimal GetDecimal(int i)
	{
		return (decimal)GetValue(i);
	}

	public DateTime GetDateTime(int i)
	{
		return (DateTime)GetValue(i);
	}

	public IDataReader GetData(int i)
	{
		throw new NotImplementedException();
	}

	public bool IsDBNull(int i)
	{
		var result = false;
		var row = enumerator.Current;

		if (row != null)
		{
			result = row.IsNull(i);
		}

		return result;
	}

	public int FieldCount { get; }

	public object this[int i] => GetValue(i);

	public object this[string name] => GetValue(GetOrdinal(name));

	public void Close()
	{
		isClosed = true;
		enumerator.Dispose();
	}

	public DataTable GetSchemaTable()
	{
		return dataTable;
	}

	public bool NextResult()
	{
		return false;
	}

	public bool Read()
	{
		if (isClosed)
		{
			throw new InvalidOperationException("DataReader is closed.");
		}

		return enumerator.MoveNext();
	}

	public int Depth => 0;
	public bool IsClosed => isClosed;
	public int RecordsAffected => 0;

	bool IsNonBitBoolColumn(int i)
	{
		var columnName = dataTable.Columns[i].ColumnName;
		return schemas[columnName] is SchemaBoolColumn { IsBitField: false };
	}
}
