using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Common;

namespace CargoWise.Data
{
	internal abstract class SqlStreamHelper<T> : Disposable where T : struct
	{
		static SqlStreamHelper()
		{
			if ((typeof(T) != typeof(byte)) && (typeof(T) != typeof(char)))
			{
				throw new NotSupportedException("StreamHelper only supports byte and char types"); // Only used in exception messages
			}
		}

		public SqlStreamHelper(DbConnection connection, string tableName, string pkColumnName, Guid pkValue, string dataColumnName, SqlDbType dataColumnType)
		{
			Argument.NotNull(connection, nameof(connection));
			this.connection = connection;
			this.dataColumnName = dataColumnName;
			this.pkValue = pkValue;
		}

		internal string ExceptionMessage;

		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		protected DbConnection connection;
		protected Guid pkValue;
		protected string dataColumnName;

		protected const string pkValueNotFoundError = "There is no record with pkValue"; // exception message

		#region IStream Methods

		public abstract bool CanRead { get; }
		public abstract bool CanSeek { get; }
		public abstract bool CanWrite { get; }
		public abstract int Read(T[] buffer, int offset, int count);
		public abstract void Write(T[] buffer, int offset, int count);
		public abstract void Flush();

		/// <summary>
		///  Length of the stream in units of T.
		/// </summary>
		public long Length
		{
			get;
			protected set;
		}

		public long Position
		{
			get
			{
				return position;
			}
			set
			{
				if (CanSeek)
				{
					Seek(value, SeekOrigin.Begin);
				}
				else
				{
					throw new NotSupportedException();
				}
			}
		}
		protected long position;

		public long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek)
			{
				throw new NotSupportedException();
			}

			switch (origin)
			{
				case SeekOrigin.Begin:
					{
						if ((offset < 0) || (offset > Length))
						{
							throw new ArgumentException("Invalid seek origin.");
						}

						position = offset;
						break;
					}
				case SeekOrigin.End:
					{
						if ((offset > 0) || (offset < -Length))
						{
							throw new ArgumentException("Invalid seek origin.");
						}

						position = this.Length + offset; //You add a negative offset to the end of the stream.
						break;
					}
				case SeekOrigin.Current:
					{
						if ((position + offset > Length) || ((position + offset) < 0))
						{
							throw new ArgumentException("Invalid seek origin.");
						}

						position = position + offset;
						break;
					}
				default:
					{
						throw new ArgumentOutOfRangeException(nameof(origin), origin, "Invalid Origin");
					}
			}
			return position;
		}

		public void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Data length of one element of the given type.
		/// Used to convert the result of the SQL datalength function to an ordinal length.
		/// </summary>
		/// <param name="dataColumnType"></param>
		/// <returns></returns>
		protected static int DataLength(SqlDbType dataColumnType)
		{
			switch (dataColumnType)
			{
				case SqlDbType.Xml:
				case SqlDbType.NVarChar:
					return 2;

				case SqlDbType.VarBinary:
				case SqlDbType.VarChar:
					return 1;

				default:
					return 1;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();
			}
		}

		~SqlStreamHelper()
		{
			Dispose(false);
		}

		protected static void ValidateArguments(T[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if (buffer.Length - offset < count)
			{
				throw new ArgumentException("Offset and length were out of bounds for the array");
			}
		}

		#endregion
	}
}
