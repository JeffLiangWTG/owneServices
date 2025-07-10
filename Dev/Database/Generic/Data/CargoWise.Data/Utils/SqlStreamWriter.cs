using System;
using System.Data;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Data
{
	internal class SqlStreamWriter<T> : SqlStreamHelper<T> where T : struct
	{
		public SqlStreamWriter(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType, int writeChunkSize, bool append)
			: base(connection, tableName, pkColumnName, pkValue, dataColumnName, dataColumnType)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(pkColumnName, nameof(pkColumnName));
			Argument.NotNullOrEmpty(dataColumnName, nameof(dataColumnName));
			if (0 > writeChunkSize)
			{
				throw new ArgumentException("Invalid argument.", nameof(writeChunkSize));
			}

			writeBuffer = new T[writeChunkSize];
			initCommand = CreateInitCommand(connection, tableName, dataColumnName, pkColumnName, SqlDbType.UniqueIdentifier, pkValue, dataColumnType, writeChunkSize);
			writeCommand = CreateWriteCommand(connection, tableName, dataColumnName, pkColumnName, SqlDbType.UniqueIdentifier, pkValue, dataColumnType, writeChunkSize);
			if (append)
			{
				Length = GetSavedOrdinalLength(connection, tableName, dataColumnName, pkColumnName, SqlDbType.UniqueIdentifier, pkValue, dataColumnType);
				writeBufferOffset = position = Length;
			}
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		readonly DbCommand initCommand;
		readonly DbCommand writeCommand;

		readonly T[] writeBuffer;
		long writeBufferOffset;
		long writeBufferPosition;

		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override int Read(T[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Write-only stream");
		}

		public override void Write(T[] buffer, int offset, int count)
		{
			ValidateArguments(buffer, offset, count);

			try
			{
				long bufferPosition = offset;
				do
				{
					long bufferWriteLength = Math.Min(count - (bufferPosition - offset), writeBuffer.Length - writeBufferPosition);

					if (bufferWriteLength < 0)
					{
						throw new InvalidOperationException("Write length cannot be negative");
					}

					if (bufferPosition + bufferWriteLength > buffer.GetLowerBound(0) + buffer.Length || bufferPosition < buffer.GetLowerBound(0))
					{
						throw new InvalidOperationException("Data doesn't fit");
					}

					Array.Copy(buffer, bufferPosition, writeBuffer, writeBufferPosition, bufferWriteLength);
					writeBufferPosition += bufferWriteLength;
					bufferPosition += bufferWriteLength;
					position += bufferWriteLength;
					Length = position;
					if (writeBuffer.Length - writeBufferPosition == 0)
					{
						SaveCache();
					}
				}
				while (bufferPosition < offset + count);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ExceptionMessage = ex.Message;
				//RollBackTransaction();
				throw;
			}
		}

		public override void Flush()
		{
			try
			{
				SaveCache();
			}
			catch (SqlException ex)
			{
				ExceptionMessage = ex.Message;
				//RollBackTransaction();
				DisposeOnError();

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
				ExceptionMessage = ex.Message;
				DisposeOnError();
				//RollBackTransaction();
				throw;
			}
		}

		void SaveBuffer(T[] buffer, long offset, long length)
		{
			var command = offset == 0 ? initCommand : writeCommand;
			command.SetParameterValue("@buffer", buffer); // parameter name
			int rowCount = (int)(command.ExecuteScalar() ?? 0);
			if (rowCount == 0)
			{
				throw new ArgumentException(string.Format(pkValueNotFoundError + "={0}", pkValue)); // SQL query
			}
		}

		void SaveCache()
		{
			if (writeBufferOffset == 0 || writeBufferPosition > 0)
			{
				T[] saveBuffer = writeBuffer;
				if (writeBuffer.Length - writeBufferPosition > 0)
				{
					saveBuffer = GetWriteBuffer(writeBuffer, 0, (int)writeBufferPosition);
				}
				SaveBuffer(saveBuffer, writeBufferOffset, saveBuffer.Length);
				writeBufferOffset = position;
				writeBufferPosition = 0;
			}
		}

		static T[] GetWriteBuffer(T[] buffer, int offset, int count)
		{
			Argument.NotNull(buffer, nameof(buffer)); // Suggested By ReviewBot 

			if (buffer.Length == count)
			{
				return buffer;
			}

			T[] data = new T[count];

			if (offset + count > buffer.GetLowerBound(0) + buffer.Length)
			{
				throw new InvalidOperationException("Data doesn't fit");
			}

			Array.Copy(buffer, offset, data, 0, count);
			return data;
		}

#if DEBUG
		internal virtual
#endif
		DbCommand CreateInitCommand(DbConnection connection, string table, string dataColumn, string keyColumn, SqlDbType keyType, object keyValue, SqlDbType dataType, int writeChunkSize)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(table, nameof(table));
			Argument.NotNullOrEmpty(dataColumn, nameof(dataColumn));
			Argument.NotNullOrEmpty(keyColumn, nameof(keyColumn));

			var sql = string.Format(
				@" update {0} set [{1}] = @buffer where [{2}] = @key select @@ROWCOUNT ", table, dataColumn, keyColumn); // sql query format string
			DbCommand command = connection.Command(sql);

			command.AddParameter("@key", keyType, keyValue); // parameter name
			command.AddParameter("@buffer", dataType, writeChunkSize, null); // parameter name

			return command;
		}

#if DEBUG
		internal virtual
#endif
		DbCommand CreateWriteCommand(DbConnection connection, string table, string dataColumn, string keyColumn, SqlDbType keyType, object keyValue, SqlDbType dataType, int writeChunkSize)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(table, nameof(table));
			Argument.NotNullOrEmpty(dataColumn, nameof(dataColumn));
			Argument.NotNullOrEmpty(keyColumn, nameof(keyColumn));

			var sql = string.Format(@"update {0} set [{1}].write(@buffer, null, null) where [{2}] = @key select @@ROWCOUNT ", table, dataColumn, keyColumn); // sql query format string
			var command = connection.Command(sql);
			command.AddParameter("@key", keyType, keyValue); // parameter name
			command.AddParameter("@buffer", dataType, writeChunkSize, null); // parameter name

			return command;
		}

		static long GetSavedOrdinalLength(DbConnection connection, string table, string dataColumn, string keyColumn, SqlDbType keyType, object keyValue, SqlDbType dataColumnType)
		{
			Argument.NotNull(connection, nameof(connection)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(table, nameof(table));
			Argument.NotNullOrEmpty(dataColumn, nameof(dataColumn));
			Argument.NotNullOrEmpty(keyColumn, nameof(keyColumn));

			var sql = string.Format(@"select cast(datalength([{0}]) as bigint) from {1} where [{2}] = @key", dataColumn, table, keyColumn); // SQL query
			using (var command = connection.Command(sql))
			{
				command.AddParameter("@key", keyType, keyValue); // parameter name
				object result = command.ExecuteScalar();
				if (result == DBNull.Value || result == null)
				{
					return 0;
				}
				else
				{
					return (long)result / DataLength(dataColumnType);
				}
			}
		}

		internal void DisposeOnError()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			isDisposed = true;
		}

		bool isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				base.Dispose(disposing);
			}

			initCommand?.Dispose();
			writeCommand?.Dispose();
		}
	}
}
