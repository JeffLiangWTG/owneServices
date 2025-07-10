using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Common;

namespace CargoWise.Data
{
	public class SqlTextFieldWriter : TextWriter
	{
		public SqlTextFieldWriter(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType, bool append = false) : base(CultureInfo.InvariantCulture)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(pkColumnName, nameof(pkColumnName));
			Argument.NotNullOrEmpty(dataColumnName, nameof(dataColumnName));

			const int writeChunkSize = 80400; //https://msdn.microsoft.com/en-us/library/ms177523.aspx
			helper = new SqlStreamWriter<char>(connection, tableName, pkColumnName, pkValue, dataColumnName, dataColumnType, writeChunkSize, append);
		}

		readonly SqlStreamHelper<char> helper;

		#region TextWriter Override Methods

		public override Encoding Encoding
		{
			get
			{
				return Encoding.UTF8;
			}
		}

		public override void Close()
		{
			this.Dispose(true);
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

		public override void Flush()
		{
			helper.Flush();
			base.Flush();
		}

		public override void Write(char value)
		{
			throw new NotSupportedException();
		}

		public override void Write(string value)
		{
			throw new NotSupportedException();
		}

		public override void Write(char[] buffer, int index, int count)
		{
			if (helper.ExceptionMessage != null)
			{
				throw new InvalidOperationException(string.Format("This writer has previously encountered an exception and shouldn't be getting called again. Message = {0}", helper.ExceptionMessage));
			}
			helper.Write(buffer, index, count);
		}

		#endregion

		#region StreamHelpert

		public bool CanRead
		{
			get
			{
				return false;
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
				return helper.CanWrite;
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
	}
}
