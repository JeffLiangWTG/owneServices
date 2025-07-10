using System;
using System.Data;
using System.Globalization;

namespace Enterprise.AlwaysOn.Setup
{
	public static class SqlDataReaderExtensions
	{
		public static string Read(this IDataReader reader, int index)
		{
			return reader.Read<string>(index);
		}

		public static T Read<T>(this IDataReader reader, int index, T defaultValue = default)
		{
			var value = reader[index];
			if (value == null || value == DBNull.Value)
			{
				return defaultValue;
			}

			return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
		}

		public static string Read(this IDataReader reader, string name)
		{
			return reader.Read<string>(name);
		}

		public static T Read<T>(this IDataReader reader, string name, T defaultValue = default)
		{
			var value = reader[name];
			if (value == null || value == DBNull.Value)
			{
				return defaultValue;
			}

			return (T)Convert.ChangeType(value.ToString(), typeof(T), CultureInfo.InvariantCulture);
		}
	}
}
