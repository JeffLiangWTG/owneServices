using System;
using System.Data;
using System.Diagnostics;

namespace CargoWise.Data
{
	public class DataReaderTimeoutWrapper : IDataReader
	{
		public DataReaderTimeoutWrapper(IDataReader internalReader, int timeoutSeconds, Stopwatch sw = null)
		{
			this.internalReader = internalReader;
			this.timeoutSeconds = timeoutSeconds;
			this.sw = sw ?? Stopwatch.StartNew();
		}

		readonly IDataReader internalReader;
		readonly int timeoutSeconds;
		readonly Stopwatch sw;
		readonly StringInterner stringInterner = new StringInterner();

		void CheckTimeout()
		{
			if (sw.Elapsed.TotalSeconds > timeoutSeconds)
			{
				throw new TimeoutException("Fill action has exceeded timeout.");
			}
		}

		public bool Read()
		{
			CheckTimeout();
			return internalReader.Read();
		}

		public bool NextResult()
		{
			CheckTimeout();
			return internalReader.NextResult();
		}

		public void Dispose()
		{
			sw.Stop();
			internalReader.Dispose();
		}

		public int Depth => internalReader.Depth;

		public bool IsClosed => internalReader.IsClosed;

		public int RecordsAffected => internalReader.RecordsAffected;

		public int FieldCount => internalReader.FieldCount;

		public object this[string name] => internalReader[name];

		public object this[int i] => internalReader[i];

		public void Close() => internalReader.Close();

		public DataTable GetSchemaTable() => internalReader.GetSchemaTable();

		public string GetName(int i) => internalReader.GetName(i);

		public string GetDataTypeName(int i) => internalReader.GetDataTypeName(i);

		public Type GetFieldType(int i) => internalReader.GetFieldType(i);

		public object GetValue(int i) => internalReader.GetValue(i);

		public int GetValues(object[] values)
		{
			var results = internalReader.GetValues(values);
			stringInterner.InternStringCells(internalReader, values);
			return results;
		}

		public int GetOrdinal(string name) => internalReader.GetOrdinal(name);

		public bool GetBoolean(int i) => internalReader.GetBoolean(i);

		public byte GetByte(int i) => internalReader.GetByte(i);

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => internalReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);

		public char GetChar(int i) => internalReader.GetChar(i);

		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => internalReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);

		public Guid GetGuid(int i) => internalReader.GetGuid(i);

		public short GetInt16(int i) => internalReader.GetInt16(i);

		public int GetInt32(int i) => internalReader.GetInt32(i);

		public long GetInt64(int i) => internalReader.GetInt64(i);

		public float GetFloat(int i) => internalReader.GetFloat(i);

		public double GetDouble(int i) => internalReader.GetDouble(i);

		public string GetString(int i) => internalReader.GetString(i);

		public decimal GetDecimal(int i) => internalReader.GetDecimal(i);

		public DateTime GetDateTime(int i) => internalReader.GetDateTime(i);

		public IDataReader GetData(int i) => internalReader.GetData(i);

		public bool IsDBNull(int i) => internalReader.IsDBNull(i);
	}
}
