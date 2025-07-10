using System;
using System.Data;
using CargoWise.Common;

namespace CargoWise.Data
{
	class DataReaderExecuteAsReaderLogin : IDataReader
	{
		public DataReaderExecuteAsReaderLogin(IDataReader dataReader, Action revertExecuteAsUserAction)
		{
			Argument.NotNull(dataReader, nameof(dataReader));
			Argument.NotNull(revertExecuteAsUserAction, nameof(revertExecuteAsUserAction));

			this.dataReader = dataReader;
			this.revertExecuteAsUserAction = revertExecuteAsUserAction;
		}

		public void Dispose()
		{
			dataReader.Dispose();
			revertExecuteAsUserAction();
		}

		public int Depth { get { return dataReader.Depth; } }
		public bool IsClosed { get { return dataReader.IsClosed; } }
		public int RecordsAffected { get { return dataReader.RecordsAffected; } }
		public void Close() { dataReader.Close(); }
		public DataTable GetSchemaTable() { return dataReader.GetSchemaTable(); }
		public bool NextResult() { return dataReader.NextResult(); }
		public bool Read() { return dataReader.Read(); }

		public object this[string name] { get { return dataReader[name]; } }
		public object this[int i] { get { return dataReader[i]; } }

		public int FieldCount { get { return dataReader.FieldCount; } }

		public bool GetBoolean(int i) { return dataReader.GetBoolean(i); }
		public byte GetByte(int i) { return dataReader.GetByte(i); }
		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { return dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length); }
		public char GetChar(int i) { return dataReader.GetChar(i); }
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { return dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length); }
		public IDataReader GetData(int i) { return dataReader.GetData(i); }
		public string GetDataTypeName(int i) { return dataReader.GetDataTypeName(i); }
		public DateTime GetDateTime(int i) { return dataReader.GetDateTime(i); }
		public decimal GetDecimal(int i) { return dataReader.GetDecimal(i); }
		public double GetDouble(int i) { return dataReader.GetDouble(i); }
		public Type GetFieldType(int i) { return dataReader.GetFieldType(i); }
		public float GetFloat(int i) { return dataReader.GetFloat(i); }
		public Guid GetGuid(int i) { return dataReader.GetGuid(i); }
		public short GetInt16(int i) { return dataReader.GetInt16(i); }
		public int GetInt32(int i) { return dataReader.GetInt32(i); }
		public long GetInt64(int i) { return dataReader.GetInt64(i); }
		public string GetName(int i) { return dataReader.GetName(i); }
		public int GetOrdinal(string name) { return dataReader.GetOrdinal(name); }
		public string GetString(int i) { return dataReader.GetString(i); }
		public object GetValue(int i) { return dataReader.GetValue(i); }
		public int GetValues(object[] values) { return dataReader.GetValues(values); }
		public bool IsDBNull(int i) { return dataReader.IsDBNull(i); }

		readonly IDataReader dataReader;
		readonly Action revertExecuteAsUserAction;
	}
}