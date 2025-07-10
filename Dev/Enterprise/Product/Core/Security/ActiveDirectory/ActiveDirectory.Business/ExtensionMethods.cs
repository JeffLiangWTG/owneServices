using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using CargoWise.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public static class ExtensionMethods
	{
		public static int IndexOfFirst<T>(this IEnumerable<T> source, Predicate<T> predicate)
		{
			Argument.NotNull(source, "source");
			Argument.NotNull(predicate, "predicate");

			var index = 0;
			foreach (var item in source)
			{
				if (predicate(item))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public static int IndexOfFirst<T>(this IEnumerable<T> source, int startIndex, Predicate<T> predicate)
		{
			Argument.NotNull(source, "source");
			Argument.NotNull(predicate, "predicate");

			var array = source.ToArray();
			for (int i = startIndex; i < array.Length; i++)
			{
				if (predicate(array[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			Argument.NotNull(source, "source");
			Argument.NotNull(action, "action");

			foreach (var item in source)
			{
				action(item);
			}
		}

		public static object GetValue(this IDirectoryEntry directoryEntry, SchemaColumn schemaColumn, int index = 0)
		{
			var value = directoryEntry[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(schemaColumn), index];
			var stringValue = value as string;

			if (stringValue != null && stringValue.Length > schemaColumn.MaxLength)
			{
				return stringValue.Substring(0, schemaColumn.MaxLength);
			}

			return value;
		}

		public static void SetValue(this IDirectoryEntry directoryEntry, SchemaColumn schemaColumn, object value, int index = 0)
		{
			directoryEntry[AttributeMap.Current.GetActiveDirectoryAttributeFromSchema(schemaColumn), index] = value;
		}
	}
}
