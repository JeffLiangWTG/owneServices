using System;
using System.Data;
using System.IO;
using CargoWise.Common;

namespace CargoWise.Data
{
	public class SqlTextFieldReader : TextReader, ISqlFieldSource
	{
		public SqlTextFieldReader(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType, int headerSize = 8)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(pkColumnName, nameof(pkColumnName));
			Argument.NotNullOrEmpty(dataColumnName, nameof(dataColumnName));
			if (headerSize <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(headerSize));
			}

			#region IRowFieldInfo

			this.tableName = tableName;
			this.columnName = dataColumnName;
			this.rowPK = pkValue;
			this.pkColumnName = pkColumnName;

			#endregion

			helper = new SqlStreamReader<char>(connection, tableName, pkColumnName, pkValue, dataColumnName, dataColumnType, headerSize);
		}

		readonly SqlStreamHelper<char> helper;

		#region TextReader Override Methods

		public override void Close()
		{
			Dispose(true);
			base.Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				helper.Dispose();
			}

			base.Dispose(disposing);
		}

		public override int Peek()
		{
			int value = Read();
			Seek(-1, SeekOrigin.Current);
			return value;
		}

		public override int Read()
		{
			char[] buffer = new char[1];
			int length = Read(buffer, 0, 1);
			if (length <= 0)
			{
				return -1;
			}
			else
			{
				return buffer[0];
			}
		}

		public override int Read(char[] buffer, int index, int count)
		{
			return helper.Read(buffer, index, count); // Just Pass return value
		}

		#endregion

		#region StreamHelper methods

		public bool CanRead
		{
			get
			{
				return helper.CanRead;
			}
		}

		public bool CanSeek
		{
			get
			{
				return helper.CanSeek;
			}
		}

		public bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public long Length
		{
			get
			{
				return helper.Length;
			}
		}

		public long Position
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

		public long Seek(long offset, SeekOrigin origin)
		{
			return helper.Seek(offset, origin);
		}

		#endregion

		#region ISqlFieldSource

		readonly string tableName;
		readonly string columnName;
		readonly Guid rowPK;
		readonly string pkColumnName;

		string IRowFieldInfo.TableName
		{
			get { return tableName; }
		}

		string IRowFieldInfo.ColumnName
		{
			get { return columnName; }
		}

		string IRowFieldInfo.PKColumnName
		{
			get { return pkColumnName; }
		}

		Guid IRowFieldInfo.RowPK
		{
			get { return rowPK; }
		}

		#endregion
	}
}
