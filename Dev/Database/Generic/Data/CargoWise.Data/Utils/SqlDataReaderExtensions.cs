using System;
using System.Data;
using CargoWise.Common;

namespace CargoWise.Data.Utils
{
	public static class SqlDataReaderExtensions
	{
		public static T GetValue<T>(this IDataReader dataReader, string columnName)
		{
			Argument.NotNull(dataReader, nameof(dataReader));
			Argument.NotNull(columnName, nameof(columnName));

			var t = typeof(T);
			var v = dataReader[columnName];
			if (Nullable.GetUnderlyingType(t) != null || t == typeof(string))
			{
				return v == null || v.Equals(DBNull.Value)
					? default(T)
					: (T)dataReader[columnName];
			}

			if (v == null || v.Equals(DBNull.Value))
			{
				throw new NoNullAllowedException(columnName);
			}

			return (T)dataReader[columnName];
		}

		public static bool HasColumn(this IDataReader dataReader, string columnName)
		{
			Argument.NotNull(dataReader, nameof(dataReader));
			Argument.NotNull(columnName, nameof(columnName));

			try
			{
				return dataReader.GetOrdinal(columnName) >= 0;
			}
			catch (IndexOutOfRangeException)
			{
				return false;
			}
		}

		public static byte[] ReadAllBytes(this IDataReader dataReader, int columnIndex)
		{
			Argument.NotNull(dataReader, nameof(dataReader));

			if (dataReader.IsDBNull(columnIndex))
			{
				return null;
			}

			var dataLength = (int)dataReader.GetBytes(columnIndex, 0, null, 0, 0);
			var data = new byte[dataLength];
			dataReader.GetBytes(columnIndex, 0, data, 0, dataLength);

			return data;
		}
	}
}
