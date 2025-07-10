using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WTG.TestHelpers.SpecTesting
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class TableColumnNameAttribute : Attribute
	{
		public string Name { get; set; }
		public int LineNumber { get; set; }

		public TableColumnNameAttribute(string name = null, [CallerLineNumber] int lineNumber = 0)
		{
			Name = name;
			LineNumber = lineNumber;
		}
	}

	class TableColumn
	{
		public string InternalName { get; set; }
		public string DisplayName { get; set; }
		public int OrderIndex { get; set; }

		// Lazily evaluated/added during generation
		public int MaxWidth { get; set; }
		public string[] Data { get; set; } = Array.Empty<string>();
	}

	public static class SpecTableGen
	{
		public static string ConvertArrayToTable<T>(T[] array)
		{
			var columns = GetTableColumns<T>();
			foreach (var column in columns)
			{
				PopulateTableColumn(column, array);
			}

			var headerTitleLine = "";
			var separatorLine = "";
			var bodyLines = new List<string>();
			for (int i = 0; i < array.Length; i++)
			{
				bodyLines.Add("");
			}

			void AddColumnSeparator()
			{
				separatorLine += "|";
				headerTitleLine += "|";
				for (int i = 0; i < bodyLines.Count; i++)
				{
					bodyLines[i] += "|";
				}
			}

			void AddSpacer()
			{
				separatorLine += "-";
				headerTitleLine += " ";
				for (int i = 0; i < bodyLines.Count; i++)
				{
					bodyLines[i] += " ";
				}
			}

			AddColumnSeparator();
			foreach (var column in columns)
			{
				AddSpacer();

				headerTitleLine += column.DisplayName.PadRight(column.MaxWidth);
				separatorLine += "".PadRight(column.MaxWidth, '-');
				for (int i = 0; i < bodyLines.Count; i++)
				{
					bodyLines[i] += column.Data[i].PadRight(column.MaxWidth);
				}

				AddSpacer();
				AddColumnSeparator();
			}

			return headerTitleLine + "\r\n" + separatorLine + "\r\n" + string.Join("\r\n", bodyLines);
		}

		static TableColumn[] GetTableColumns<T>()
		{
			var result = new List<TableColumn>();

			var type = typeof(T);
			foreach (var property in type.GetProperties())
			{
				var attribute = property.GetCustomAttribute<TableColumnNameAttribute>();
				if (attribute == null)
				{
					continue;
				}

				var column = new TableColumn()
				{
					InternalName = property.Name,
					DisplayName = attribute.Name ?? property.Name,
					OrderIndex = attribute.LineNumber,
				};

				result.Add(column);
			}

			return result.OrderBy(col => col.OrderIndex).ToArray();
		}

		static void PopulateTableColumn<T>(TableColumn column, T[] data)
		{
			var strings = new List<string>();
			foreach (var item in data)
			{
				var value = item.GetType().GetProperty(column.InternalName).GetValue(item);
				var stringValue = value?.ToString() ?? "";
				strings.Add(stringValue);
			}

			column.Data = strings.ToArray();

			column.MaxWidth = column.Data.Length == 0 ? 0 : column.Data.Max(s => s.Length);
			column.MaxWidth = Math.Max(column.MaxWidth, column.DisplayName.Length);
		}
	}
}
