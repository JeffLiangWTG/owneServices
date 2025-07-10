using System;
using System.Data;
using System.IO;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Data
{
	public class SqlBinaryFieldStream : Stream
	{
		public static SqlBinaryFieldStream OpenReader(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType, int headerSize = 8)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(pkColumnName, nameof(pkColumnName));
			Argument.NotNullOrEmpty(dataColumnName, nameof(dataColumnName));
			if (headerSize <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(headerSize));
			}

			return new SqlBinaryFieldStream(new SqlStreamReader<byte>(connection, tableName, pkColumnName, pkValue, dataColumnName, dataColumnType, headerSize));
		}

		public static SqlBinaryFieldStream OpenWriter(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType, bool append = false)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(pkColumnName, nameof(pkColumnName));
			Argument.NotNullOrEmpty(dataColumnName, nameof(dataColumnName));

			const int writeChunkSize = 80400; // https://msdn.microsoft.com/en-us/library/ms177523.aspx
			return new SqlBinaryFieldStream(new SqlStreamWriter<byte>(connection, tableName, pkColumnName, pkValue, dataColumnName, dataColumnType, writeChunkSize, append));
		}

		SqlBinaryFieldStream(SqlStreamHelper<byte> helper)
		{
			Argument.NotNull(helper, nameof(helper));

			this.helper = helper;
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		readonly SqlStreamHelper<byte> helper;

		#region IStream Methods

		public override bool CanRead
		{
			get
			{
				return helper.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return helper.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return helper.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return helper.Length;
			}
		}

		public override long Position
		{
			get
			{
				return helper.Position;
			}
			set
			{
				helper.Position = value;
			}
		}

		public override void Flush()
		{
			helper.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (helper.CanSeek)
			{
				return helper.Seek(offset, origin);
			}

			throw new NotSupportedException("Seeking is not supported for writing.");
		}

		public override void SetLength(long value)
		{
			helper.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return helper.Read(buffer, offset, count);  // Just Pass return value
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			helper.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
				helper.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
