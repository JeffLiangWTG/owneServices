using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace CargoWise.Data
{
	sealed class SqlStreamReader<T> : SqlStreamHelper<T> where T : struct
	{
		public SqlStreamReader(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType, int headerSize)
			: base(connection, tableName, pkColumnName, pkValue, dataColumnName, dataColumnType)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(pkColumnName, nameof(pkColumnName));
			if (headerSize <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(headerSize));
			}

			Argument.NotNullOrEmpty(dataColumnName, nameof(dataColumnName));

			try
			{
				this.headerSize = headerSize;
				header = new Queue<T>(headerSize);
				readCommand = CreateReadCommand(connection, tableName, dataColumnName, pkColumnName, SqlDbType.UniqueIdentifier, pkValue, dataColumnType);
				Length = CreateReader() / DataLength(dataColumnType);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				base.Dispose();
				throw;
			}
		}

		readonly DbCommand readCommand;
		IDataReader reader;
		readonly Queue<T> header;
		readonly int headerSize;
		long headerOffset;
		const string DataLengthColumnName = "DataLength"; // not seen by user
		const string DataColumnName = "Data"; // not seen by user

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override void Flush()
		{
		}

		public override void Write(T[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Read-only stream");
		}

		public override int Read(T[] buffer, int offset, int count)
		{
			int initialCount = count;
			ValidateArguments(buffer, offset, count);

			try
			{
				int headerReadLength = 0;
				if (position < headerOffset + header.Count)
				{
					if (position < headerOffset)
					{
						throw new InvalidOperationException("Attempt to rewind reader beyond header size");
					}

					headerReadLength = (int)Math.Min(headerOffset + header.Count - position, count);

					if (headerReadLength < 0)
					{
						throw new InvalidOperationException("Header length cannot be negative");
					}

					if (offset + headerReadLength > buffer.GetLowerBound(0) + buffer.Length || offset < buffer.GetLowerBound(0))
					{
						throw new InvalidOperationException("Data doesn't fit");
					}

					var source = header.ToArray();

					if (position - headerOffset + headerReadLength > source.GetLowerBound(0) + source.Length)
					{
						throw new InvalidOperationException("Data doesn't fit");
					}

					Array.Copy(source, position - headerOffset, buffer, offset, headerReadLength);
					Position += headerReadLength;
					offset += headerReadLength;
					count -= headerReadLength;
					if (count == 0)
					{
						return headerReadLength;
					}
				}

				T[] data = Read(Position, count);

				int headerWriteLength;
				if (data.Length >= headerSize)
				{
					header.Clear();
					headerWriteLength = headerSize;
					headerOffset = Position + (data.Length - headerWriteLength);
				}
				else
				{
					headerWriteLength = data.Length;
					while (headerWriteLength + header.Count > headerSize)
					{
						header.Dequeue();
						headerOffset++;
					}
				}

				for (int i = 0; i < headerWriteLength; i++)
				{
					header.Enqueue(data[data.Length - headerWriteLength + i]);
				}

				if (offset + data.Length > buffer.GetLowerBound(0) + buffer.Length || offset < buffer.GetLowerBound(0))
				{
					throw new InvalidOperationException("Data doesn't fit");
				}

				Array.Copy(data, 0, buffer, offset, data.Length);
				position += data.Length;

				var result = data.Length + headerReadLength;

				if (result > initialCount)
				{
					throw new InvalidOperationException();
				}

				return result;
			}
			catch
			{
				//RollBackTransaction();
				throw;
			}
		}

		T[] Read(long offset, long length)
		{
			var buffer = new T[length];
			long read = 0;

			if (reader != null)
			{
				read = typeof(T) == typeof(char) ?
					reader.GetChars(reader.GetOrdinal(DataColumnName), (int)(@offset), (char[])(object)buffer, 0, buffer.Length) :
					reader.GetBytes(reader.GetOrdinal(DataColumnName), (int)(@offset), (byte[])(object)buffer, 0, buffer.Length);
			}

			if (read < buffer.Length)
			{
				Array.Resize(ref buffer, (int)read);
				CloseReader();
			}

			return buffer;
		}

		[SuppressMessage("Microsoft.Contracts", "Nonnull-129-0")] // Suggested By ReviewBot
		long CreateReader()
		{
			if (reader == null)
			{
				try
				{
					reader = readCommand.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow);
					if (reader.Read())
					{
						int columnOrdinal = reader.GetOrdinal(DataLengthColumnName);
						var value = reader.GetValue(columnOrdinal);

						if (value == DBNull.Value)
						{
							return 0;
						}

						return (long)value;
					}

					throw new SqlStreamReaderRowNotFoundException(string.Format(pkValueNotFoundError + " = '{0}'. Stream can't be open.", pkValue));
				}
				catch (SqlException ex)
				{
					CloseReaderAndRollBackTransaction();

					DbErrorMatch errorMatch = new DbErrorMatch(ex);
					if (errorMatch.ExceptionType == DbErrorType.InvalidObjectName || errorMatch.ExceptionType == DbErrorType.InvalidColumnName)
					{
						throw new ArgumentException(ex.Message);
					}
					else
					{
						throw;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					CloseReaderAndRollBackTransaction();
					throw;
				}
			}

			return 0;
		}

		void CloseReaderAndRollBackTransaction()
		{
			if (reader != null)
			{
				reader.Close();
			}
			//RollBackTransaction();
		}

		void CloseReader()
		{
			if (reader != null)
			{
				reader.Close();
				reader = null;
			}
		}

		static DbCommand CreateReadCommand(DbConnection connection, string table, string dataColumnInTable, string keyColumn, SqlDbType keyType, object keyValue, SqlDbType dataColumnType)
		{
			Argument.NotNullOrEmpty(table, nameof(table));
			Argument.NotNullOrEmpty(dataColumnInTable, nameof(dataColumnInTable));
			Argument.NotNullOrEmpty(keyColumn, nameof(keyColumn));
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 

			string dataColumnSelector;

			// Calling GetChars() on an Xml column is incredible slow for large datasets - a slowdown measured of over 3000x for 90MB of data.
			// With the same dataset, NVarChar (which has the same length as Xml - 2 bytes per char) ran in normal time.
			// Thus, for Xml columns, we convert to NVarChar for performance. See details in WI00079841
			if (dataColumnType == SqlDbType.Xml)
			{
				dataColumnType = SqlDbType.NVarChar;
				dataColumnSelector = string.Format("CONVERT(NVARCHAR(MAX), [{0}])", dataColumnInTable);  // building a SQL query - not a UI string
			}
			else
			{
				dataColumnSelector = string.Format("[{0}]", dataColumnInTable);
			}

			if (table[0] != '[')
			{
				table = '[' + table.Replace(".", "].[") + ']';
			}

			var sql =
				string.Format(@"SELECT CAST(DATALENGTH([{0}]) AS BIGINT) AS [{1}], {2} AS [{3}] FROM {4} WHERE [{5}] = @key",
					dataColumnInTable, DataLengthColumnName, dataColumnSelector, DataColumnName, table, keyColumn);
			var command = connection.Command(sql);
			command.AddParameter("@key", keyType, keyValue); // not seen by user
			return command;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				CloseReader();
				base.Dispose(disposing);
			}
			finally
			{
				if (readCommand != null)
				{
					readCommand.Dispose();
				}
			}
		}
	}
}
