using System.Collections;
using System.Data;
using System.Data.Common;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

public class DeserializingDataReader : DbDataReader
{
	readonly CustomAsyncEnumerator<SqlProxyReaderResponseItem> records;
	IDisposable? disposable;

	SqlReaderResponseHeader? currentHeader;
	SqlReaderResponseRow? currentRow;
	bool justSwitchedToNewHeader;
	bool hasRecognizedCurrentRow;
	bool disposed;
	readonly object readerLock = new();

	DeserializingDataReader(CustomAsyncEnumerator<SqlProxyReaderResponseItem> records, IDisposable disposable)
	{
		// TODO: Uncomment this once we're on .net 8 and can use IAsyncEnumerable
		// this.records = records.GetAsyncEnumerator();

		this.records = records;
		this.disposable = disposable;
	}

	public static async Task<DeserializingDataReader> Create(CustomAsyncEnumerator<SqlProxyReaderResponseItem> records, IDisposable disposable)
	{
		var reader = new DeserializingDataReader(records, disposable);
		var initialized = await reader.InitializeNewTableAsync().ConfigureAwait(false);
		if (!initialized)
		{
			throw new InvalidOperationException($"No {nameof(SqlReaderResponseHeader)} item found in the response.");
		}

		return reader;
	}

	async Task<bool> InitializeNewTableAsync()
	{
		currentHeader = null;
		currentRow = null;

		while (true)
		{
			if (records.Current is SqlReaderResponseHeader header)
			{
				currentHeader = header;
				justSwitchedToNewHeader = true;
				hasRecognizedCurrentRow = false;
				return true;
			}

			var hasNext = await records.MoveNextAsync().ConfigureAwait(false);
			if (!hasNext)
			{
				return false;
			}
		}
	}

	async Task<bool> InitializeNewRowAsync()
	{
		currentRow = null;

		while (true)
		{
			// Return the current row only if it hasn't been returned already
			if (records.Current is SqlReaderResponseRow row && !hasRecognizedCurrentRow)
			{
				currentRow = row;
				hasRecognizedCurrentRow = true;
				return true;
			}

			// If the current record is a new header, then there's no row to return.
			// Unless we just switched to a new header, in which case this header is just
			// for the current table.
			if (records.Current is SqlReaderResponseHeader && !justSwitchedToNewHeader)
			{
				return false;
			}

			justSwitchedToNewHeader = false;
			hasRecognizedCurrentRow = false;
			var hasNext = await records.MoveNextAsync().ConfigureAwait(false);
			if (!hasNext)
			{
				return false;
			}
		}
	}

	object DeserializeValueAt(int i)
	{
		{
			if (currentHeader == null || currentRow == null)
			{
				throw new InvalidOperationException("No current header or row.");
			}

			var sqlValue = currentRow.Values[i];

#pragma warning disable CS8603 // Possible null reference return.
			return sqlValue.ToSqlValue(currentHeader.Columns[i].Type);
#pragma warning restore CS8603 // Possible null reference return.
		}
	}

	SqlReaderResponseRow CurrentRowOrThrow() =>
		currentRow ?? throw new InvalidOperationException("No current row.");
	SqlReaderResponseHeader CurrentHeaderOrThrow() =>
		currentHeader ?? throw new InvalidOperationException("No current header.");

	#region Interface Implementation

#pragma warning disable CS8603 // Possible null reference return.
	public override object this[int i] => SqlValueConverter.SqlValueToCsValue(DeserializeValueAt(i));

	public override object this[string name] => SqlValueConverter.SqlValueToCsValue(DeserializeValueAt(GetOrdinal(name))!);
#pragma warning restore CS8603 // Possible null reference return.

	public override int Depth => CurrentRowOrThrow().Depth;
	public override int RecordsAffected => CurrentHeaderOrThrow().RecordsAffected;
	public override int FieldCount => CurrentHeaderOrThrow().Columns.Length;
	public override bool HasRows => CurrentHeaderOrThrow().HasRows;

	public override bool IsClosed
	{
		get
		{
			lock (readerLock)
			{
				return disposable == null;
			}
		}
	}

	public override void Close()
	{
		if (!disposed)
		{
			disposed = true;

			lock (readerLock)
			{
				disposable?.Dispose();
				disposable = null;
				records.Dispose();
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		Close();

		base.Dispose(disposing);
	}

	public override bool GetBoolean(int i) => (bool)this[i]!;
	public override byte GetByte(int i) => (byte)this[i]!;
	public override char GetChar(int i) => (char)this[i]!;
	public override DateTime GetDateTime(int i) => (DateTime)this[i]!;
	public override decimal GetDecimal(int i) => (decimal)this[i]!;
	public override double GetDouble(int i) => (double)(decimal)this[i]!;
	public override float GetFloat(int i) => (float)(decimal)this[i]!;
	public override Guid GetGuid(int i) => (Guid)this[i]!;
	public override short GetInt16(int i) => (short)this[i]!;
	public override int GetInt32(int i) => (int)this[i]!;
	public override long GetInt64(int i) => (long)this[i]!;
	public override string GetString(int i) => (string)this[i]!;
	public override object GetValue(int i) => this[i];
	public override bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

	public override long GetBytes(int i, long dataIndex, byte[]? buffer, int bufferoffset, int length)
	{
		var value = this[i] ?? throw new ArgumentNullException(nameof(i));
		var srcBytes = (byte[])value;
		var maxLen = srcBytes?.Length ?? 0;
		if (srcBytes == null || buffer == null)
		{
			return maxLen;
		}

		if (dataIndex + length > maxLen)
		{
			maxLen -= (int)dataIndex;
		}
		else
		{
			maxLen = length;
		}

		if (maxLen < 0)
		{
			throw new IndexOutOfRangeException($"The requested starting index: {dataIndex} and length: {length} is out of range of the source data at column: {i}.");
		}

		Array.Copy(srcBytes, dataIndex, buffer, bufferoffset, maxLen);

		return maxLen;
	}

	public override long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
	{
		_ = buffer ?? throw new ArgumentNullException(nameof(buffer));

		if (length <= 0)
		{
			throw new ArgumentException("Invalid argument.", nameof(length));
		}

		var currentValue = this[i];
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
			throw new ArgumentNullException(nameof(i), "Value does not exist");
		}
	}

	public override string GetDataTypeName(int i) => CurrentHeaderOrThrow().Columns[i].Type;
	public override string GetName(int i) => CurrentHeaderOrThrow().Columns[i].Name;
	public override Type GetFieldType(int i) => TypeMappingHelper.SqlDbTypeToCsType(CurrentHeaderOrThrow().Columns[i].Type);
	public override int GetOrdinal(string name) => CurrentHeaderOrThrow().Columns.ToList().FindIndex(x => x.Name == name);

	public override DataTable? GetSchemaTable()
	{
		var schemaTable = new DataTable();
		var columns = CurrentHeaderOrThrow().Columns;
		for (var i = 0; i < columns.Length; i++)
		{
			var column = columns[i];
			var columnName = string.IsNullOrEmpty(column.Name) ? $"Column{i}" : column.Name;
			var dataType = TypeMappingHelper.SqlDbTypeToCsType(column.Type);
			var dataColumn = new DataColumn(columnName, dataType);

			schemaTable.Columns.Add(dataColumn);
			dataColumn.SetOrdinal(i);
		}

		return schemaTable;
	}

	public override int GetValues(object?[] values)
	{
		for (var i = 0; i < values.Length; i++)
		{
			values[i] = DeserializeValueAt(i);
		}
		return CurrentRowOrThrow().Values.Length;
	}

	public override bool NextResult()
	{
		if (IsClosed)
		{
			return false;
		}

		try
		{
			currentRow = null;
			currentHeader = null;

			if (records.Current is SqlReaderResponseHeader)
			{
				var hasNext = AsyncHelper.InvokeAsync(records.MoveNextAsync(), CancellationToken.None);
				if (!hasNext)
				{
					return false;
				}
			}

			return AsyncHelper.InvokeAsync(InitializeNewTableAsync(), CancellationToken.None);
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	public override bool Read()
	{
		currentRow = null;
		return AsyncHelper.InvokeAsync(InitializeNewRowAsync(), CancellationToken.None);
	}

	public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
	{
		currentRow = null;
		return await InitializeNewRowAsync().ConfigureAwait(false);
	}

	public override async Task<bool> NextResultAsync(CancellationToken cancellationToken)
	{
		currentRow = null;
		currentHeader = null;
		return await InitializeNewTableAsync().ConfigureAwait(false);
	}

	public override IEnumerator GetEnumerator()
	{
		return new DbEnumerator((IDataReader)this, false);
	}
	#endregion
}
